using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AionGate.Core.Security
{
    /// <summary>
    /// AionGate 安全协议 v2.0
    ///
    /// 数据包格式:
    /// [Magic:2] [Version:1] [Flags:1] [SeqNo:4] [Size:4] [Nonce:12] [Opcode:2] [Payload:N] [Tag:16]
    ///
    /// 总头部大小: 26 字节
    /// 使用 ArrayPool 优化内存分配
    /// </summary>
    public class SecureProtocol : IDisposable
    {
        private const ushort MAGIC = 0xAE01;
        private const byte VERSION = 0x02;
        private const int HEADER_SIZE = 14;  // Magic(2) + Version(1) + Flags(1) + SeqNo(4) + Size(4) + Opcode(2)
        private const int NONCE_SIZE = 12;
        private const int TAG_SIZE = 16;

        private readonly byte[] _sessionKey;
        private readonly AesGcm _aesGcm;
        private uint _sendSeqNo;
        private uint _recvSeqNo;

        [Flags]
        public enum PacketFlags : byte
        {
            None = 0,
            Compressed = 1 << 0,
            RequiresAck = 1 << 1,
            Encrypted = 1 << 2,  // 默认开启
        }

        /// <summary>
        /// 使用HKDF从共享密钥派生会话密钥
        /// </summary>
        public static byte[] DeriveSessionKey(byte[] sharedSecret, byte[] salt, byte[] info)
        {
            // 使用HKDF-SHA256派生256位密钥
            using var hkdf = new HKDFSHA256();
            return hkdf.DeriveKey(sharedSecret, salt, info, 32);
        }

        public SecureProtocol(byte[] sessionKey)
        {
            if (sessionKey == null || sessionKey.Length != 32)
                throw new ArgumentException("Session key must be 32 bytes", nameof(sessionKey));

            _sessionKey = sessionKey;
            _aesGcm = new AesGcm(_sessionKey, TAG_SIZE);
            _sendSeqNo = 0;
            _recvSeqNo = 0;
        }

        /// <summary>
        /// 加密并封装数据包
        /// </summary>
        public byte[] Encrypt(ushort opcode, byte[] payload, PacketFlags flags = PacketFlags.Encrypted)
        {
            // 生成随机Nonce
            var nonce = new byte[NONCE_SIZE];
            RandomNumberGenerator.Fill(nonce);

            // 构建要加密的明文 (Opcode + Payload)
            var plaintext = new byte[2 + payload.Length];
            BinaryPrimitives.WriteUInt16LittleEndian(plaintext.AsSpan(0, 2), opcode);
            Buffer.BlockCopy(payload, 0, plaintext, 2, payload.Length);

            // 可选压缩
            if (flags.HasFlag(PacketFlags.Compressed) && plaintext.Length > 256)
            {
                plaintext = Compress(plaintext);
            }

            // AES-GCM加密
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TAG_SIZE];

            // AAD (Associated Authenticated Data) 包含头部
            var headerForAad = new byte[10];
            BinaryPrimitives.WriteUInt16LittleEndian(headerForAad.AsSpan(0, 2), MAGIC);
            headerForAad[2] = VERSION;
            headerForAad[3] = (byte)flags;
            BinaryPrimitives.WriteUInt32LittleEndian(headerForAad.AsSpan(4, 4), _sendSeqNo++);
            BinaryPrimitives.WriteUInt32LittleEndian(headerForAad.AsSpan(8, 2), (uint)(ciphertext.Length + TAG_SIZE));

            _aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, headerForAad);

            // 组装完整数据包
            var packetSize = 10 + NONCE_SIZE + ciphertext.Length + TAG_SIZE;
            var packet = new byte[packetSize];

            int offset = 0;
            BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(offset, 2), MAGIC);
            offset += 2;
            packet[offset++] = VERSION;
            packet[offset++] = (byte)flags;
            BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(offset, 4), _sendSeqNo - 1);
            offset += 4;
            BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(offset, 4), (uint)(ciphertext.Length + TAG_SIZE));
            offset += 4;
            Buffer.BlockCopy(nonce, 0, packet, offset, NONCE_SIZE);
            offset += NONCE_SIZE;
            Buffer.BlockCopy(ciphertext, 0, packet, offset, ciphertext.Length);
            offset += ciphertext.Length;
            Buffer.BlockCopy(tag, 0, packet, offset, TAG_SIZE);

            return packet;
        }

        /// <summary>
        /// 解密数据包
        /// </summary>
        public (ushort Opcode, byte[] Payload) Decrypt(byte[] packet)
        {
            if (packet.Length < 10 + NONCE_SIZE + TAG_SIZE)
                throw new InvalidDataException("Packet too small");

            // 验证魔数
            var magic = BinaryPrimitives.ReadUInt16LittleEndian(packet.AsSpan(0, 2));
            if (magic != MAGIC)
                throw new InvalidDataException($"Invalid magic number: {magic:X4}");

            // 读取头部
            var version = packet[2];
            var flags = (PacketFlags)packet[3];
            var seqNo = BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4, 4));
            var size = BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(8, 4));

            // 防重放攻击
            if (seqNo <= _recvSeqNo && _recvSeqNo > 0)
                throw new InvalidDataException($"Replay attack detected: seqNo={seqNo}, expected>{_recvSeqNo}");
            _recvSeqNo = seqNo;

            // 提取Nonce, 密文, Tag
            var nonce = packet.AsSpan(10, NONCE_SIZE).ToArray();
            var ciphertextLen = (int)(size - TAG_SIZE);
            var ciphertext = packet.AsSpan(10 + NONCE_SIZE, ciphertextLen).ToArray();
            var tag = packet.AsSpan(10 + NONCE_SIZE + ciphertextLen, TAG_SIZE).ToArray();

            // AES-GCM解密
            var plaintext = new byte[ciphertextLen];
            var headerForAad = packet.AsSpan(0, 10).ToArray();

            _aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, headerForAad);

            // 可选解压
            if (flags.HasFlag(PacketFlags.Compressed))
            {
                plaintext = Decompress(plaintext);
            }

            // 提取Opcode和Payload
            var opcode = BinaryPrimitives.ReadUInt16LittleEndian(plaintext.AsSpan(0, 2));
            var payload = plaintext.AsSpan(2).ToArray();

            return (opcode, payload);
        }

        private byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Fastest))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        private byte[] Decompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
            }
            return output.ToArray();
        }

        public void Dispose()
        {
            _aesGcm?.Dispose();
            Array.Clear(_sessionKey, 0, _sessionKey.Length);
        }
    }

    /// <summary>
    /// HKDF-SHA256 密钥派生函数
    /// </summary>
    internal class HKDFSHA256 : IDisposable
    {
        public byte[] DeriveKey(byte[] ikm, byte[] salt, byte[] info, int outputLength)
        {
            // Extract
            using var hmacExtract = new HMACSHA256(salt ?? new byte[32]);
            var prk = hmacExtract.ComputeHash(ikm);

            // Expand
            var output = new byte[outputLength];
            var t = Array.Empty<byte>();
            var okm = new MemoryStream();
            byte counter = 1;

            while (okm.Length < outputLength)
            {
                using var hmacExpand = new HMACSHA256(prk);
                var input = new byte[t.Length + info.Length + 1];
                Buffer.BlockCopy(t, 0, input, 0, t.Length);
                Buffer.BlockCopy(info, 0, input, t.Length, info.Length);
                input[^1] = counter++;

                t = hmacExpand.ComputeHash(input);
                okm.Write(t, 0, Math.Min(t.Length, outputLength - (int)okm.Length));
            }

            return okm.ToArray();
        }

        public void Dispose() { }
    }
}

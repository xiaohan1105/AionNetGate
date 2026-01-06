using System.Buffers.Binary;
using System.Text;

namespace AionNetGate.Network.Protocol;

/// <summary>
/// 数据包写入器 - 用于构建服务器发送的数据包
/// 兼容老项目的 AbstractServerPacket 的写入方法
/// </summary>
public sealed class PacketWriter : IDisposable
{
    private readonly MemoryStream _stream;
    private readonly byte[] _tempBuffer;
    private bool _disposed;

    /// <summary>
    /// 创建数据包写入器
    /// </summary>
    /// <param name="initialCapacity">初始容量</param>
    public PacketWriter(int initialCapacity = 256)
    {
        _stream = new MemoryStream(initialCapacity);
        _tempBuffer = new byte[8]; // 用于临时存储数值类型
    }

    /// <summary>
    /// 当前写入位置（数据长度）
    /// </summary>
    public int Length => (int)_stream.Length;

    /// <summary>
    /// 写入一个字节 (writeC)
    /// </summary>
    public void WriteByte(byte value)
    {
        _stream.WriteByte(value);
    }

    /// <summary>
    /// 写入有符号短整数 (2字节, Little Endian)
    /// </summary>
    public void WriteInt16(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(_tempBuffer, value);
        _stream.Write(_tempBuffer, 0, 2);
    }

    /// <summary>
    /// 写入无符号短整数 (writeUH - 2字节, Little Endian)
    /// </summary>
    public void WriteUInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_tempBuffer, value);
        _stream.Write(_tempBuffer, 0, 2);
    }

    /// <summary>
    /// 写入有符号整数 (4字节, Little Endian)
    /// </summary>
    public void WriteInt32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_tempBuffer, value);
        _stream.Write(_tempBuffer, 0, 4);
    }

    /// <summary>
    /// 写入无符号整数 (4字节, Little Endian)
    /// </summary>
    public void WriteUInt32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_tempBuffer, value);
        _stream.Write(_tempBuffer, 0, 4);
    }

    /// <summary>
    /// 写入长整数 (8字节, Little Endian)
    /// </summary>
    public void WriteInt64(long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(_tempBuffer, value);
        _stream.Write(_tempBuffer, 0, 8);
    }

    /// <summary>
    /// 写入字符串 (writeS)
    /// 格式: [长度:2字节] [UTF-8内容:N字节]
    /// </summary>
    public void WriteString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteUInt16(0);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        if (bytes.Length > ushort.MaxValue)
            throw new ArgumentException($"字符串太长: {bytes.Length} 字节（最大 {ushort.MaxValue}）");

        WriteUInt16((ushort)bytes.Length);
        _stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 写入字节数组
    /// </summary>
    public void WriteBytes(byte[] bytes)
    {
        if (bytes != null && bytes.Length > 0)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// 写入字节数组
    /// </summary>
    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        if (!bytes.IsEmpty)
        {
            _stream.Write(bytes);
        }
    }

    /// <summary>
    /// 写入布尔值 (1字节, false=0, true=1)
    /// </summary>
    public void WriteBoolean(bool value)
    {
        WriteByte(value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// 获取写入的数据（不包含长度头和Opcode）
    /// </summary>
    public byte[] ToArray()
    {
        return _stream.ToArray();
    }

    /// <summary>
    /// 获取完整的数据包（包含长度头和Opcode）
    /// 格式: [Length:4字节] [Opcode:1字节] [Payload:N字节]
    /// </summary>
    /// <param name="opcode">操作码</param>
    /// <param name="encrypt">是否加密</param>
    public byte[] ToPacket(byte opcode, bool encrypt = true)
    {
        var payload = _stream.ToArray();
        var totalLength = 4 + 1 + payload.Length; // Length(4) + Opcode(1) + Payload
        var packet = new byte[totalLength];

        // 写入长度（不包含长度字段本身的4字节）
        var packetLength = 1 + payload.Length; // Opcode + Payload
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(0, 4), packetLength);

        // 写入 Opcode
        packet[4] = opcode;

        // 写入 Payload
        if (payload.Length > 0)
        {
            Buffer.BlockCopy(payload, 0, packet, 5, payload.Length);
        }

        // 加密（如果需要）
        if (encrypt)
        {
            PacketEncryption.Encrypt(packet.AsSpan());
        }

        return packet;
    }

    /// <summary>
    /// 重置写入器，清空所有数据
    /// </summary>
    public void Reset()
    {
        _stream.SetLength(0);
        _stream.Position = 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stream.Dispose();
            _disposed = true;
        }
    }
}

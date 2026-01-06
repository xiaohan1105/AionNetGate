using System.Buffers;
using System.Buffers.Binary;
using AionNetGate.Network.Packets;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Protocol;

/// <summary>
/// 兼容老协议的数据包序列化器
/// 老协议格式: [Length:4字节] [Opcode:1字节] [Payload:N字节]
/// 加密方式: XOR ^ 0x714C
/// </summary>
public class LegacyPacketSerializer
{
    private readonly ILogger<LegacyPacketSerializer>? _logger;

    /// <summary>
    /// 最大数据包大小 (20MB，支持大型桌面截图)
    /// </summary>
    public const int MaxPacketSize = 20 * 1024 * 1024;

    /// <summary>
    /// 头部大小（仅长度字段）
    /// </summary>
    public const int HeaderSize = 4;

    public LegacyPacketSerializer(ILogger<LegacyPacketSerializer>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 尝试从缓冲区读取一个完整的数据包
    /// </summary>
    /// <param name="buffer">输入缓冲区</param>
    /// <param name="opcode">输出操作码</param>
    /// <param name="payload">输出负载数据（已解密）</param>
    /// <param name="bytesConsumed">消耗的字节数</param>
    /// <returns>是否成功读取完整数据包</returns>
    public bool TryReadPacket(
        ReadOnlySequence<byte> buffer,
        out byte opcode,
        out ReadOnlyMemory<byte> payload,
        out int bytesConsumed)
    {
        opcode = 0;
        payload = ReadOnlyMemory<byte>.Empty;
        bytesConsumed = 0;

        // 至少需要 4 字节读取长度
        if (buffer.Length < HeaderSize)
            return false;

        // 读取长度（需要先解密这4字节）
        Span<byte> lengthBytes = stackalloc byte[4];
        buffer.Slice(0, 4).CopyTo(lengthBytes);
        PacketEncryption.Decrypt(lengthBytes);

        int packetLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);

        // 验证长度
        if (packetLength <= 0 || packetLength > MaxPacketSize)
        {
            _logger?.LogWarning("无效的数据包长度: {Length}", packetLength);
            return false;
        }

        // 总长度 = 4 (length) + packetLength (opcode + payload)
        int totalLength = HeaderSize + packetLength;

        // 检查是否有完整数据
        if (buffer.Length < totalLength)
            return false;

        // 读取并解密整个数据包内容（opcode + payload）
        byte[] packetData = new byte[packetLength];
        buffer.Slice(HeaderSize, packetLength).CopyTo(packetData);
        PacketEncryption.Decrypt(packetData);

        // 提取 opcode 和 payload
        opcode = packetData[0];
        payload = packetData.Length > 1
            ? new ReadOnlyMemory<byte>(packetData, 1, packetData.Length - 1)
            : ReadOnlyMemory<byte>.Empty;

        bytesConsumed = totalLength;
        return true;
    }

    /// <summary>
    /// 尝试从 Span 读取数据包
    /// </summary>
    public bool TryReadPacket(
        ReadOnlySpan<byte> buffer,
        out byte opcode,
        out byte[] payload,
        out int bytesConsumed)
    {
        opcode = 0;
        payload = Array.Empty<byte>();
        bytesConsumed = 0;

        if (buffer.Length < HeaderSize)
            return false;

        // 复制并解密长度字段
        Span<byte> lengthBytes = stackalloc byte[4];
        buffer.Slice(0, 4).CopyTo(lengthBytes);
        PacketEncryption.Decrypt(lengthBytes);

        int packetLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);

        if (packetLength <= 0 || packetLength > MaxPacketSize)
        {
            _logger?.LogWarning("无效的数据包长度: {Length}", packetLength);
            return false;
        }

        int totalLength = HeaderSize + packetLength;

        if (buffer.Length < totalLength)
            return false;

        // 复制并解密数据包内容
        byte[] packetData = new byte[packetLength];
        buffer.Slice(HeaderSize, packetLength).CopyTo(packetData);
        PacketEncryption.Decrypt(packetData);

        opcode = packetData[0];
        if (packetData.Length > 1)
        {
            payload = new byte[packetData.Length - 1];
            Array.Copy(packetData, 1, payload, 0, payload.Length);
        }

        bytesConsumed = totalLength;
        return true;
    }

    /// <summary>
    /// 构建发送数据包
    /// </summary>
    /// <param name="opcode">操作码</param>
    /// <param name="payload">负载数据</param>
    /// <returns>完整的加密数据包</returns>
    public byte[] BuildPacket(byte opcode, ReadOnlySpan<byte> payload)
    {
        // packetLength = opcode(1) + payload.Length
        int packetLength = 1 + payload.Length;
        int totalLength = HeaderSize + packetLength;

        byte[] packet = new byte[totalLength];

        // 写入长度
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(0, 4), packetLength);

        // 写入 opcode
        packet[4] = opcode;

        // 写入 payload
        if (!payload.IsEmpty)
        {
            payload.CopyTo(packet.AsSpan(5));
        }

        // 加密整个数据包
        PacketEncryption.Encrypt(packet);

        return packet;
    }

    /// <summary>
    /// 使用 PacketWriter 构建数据包
    /// </summary>
    public byte[] BuildPacket(byte opcode, Action<PacketWriter> writeAction)
    {
        using var writer = new PacketWriter();
        writeAction(writer);
        return BuildPacket(opcode, writer.ToArray());
    }
}

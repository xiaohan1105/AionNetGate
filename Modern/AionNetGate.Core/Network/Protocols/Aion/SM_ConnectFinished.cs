using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network.Protocols.Aion;

/// <summary>
/// 服务器连接完成Packet (Opcode: 0x00)
/// 服务器响应客户端连接请求
/// </summary>
public class SM_ConnectFinished : ServerPacket
{
    public override ushort Opcode => 0x00;

    /// <summary>
    /// 服务器版本
    /// </summary>
    public string ServerVersion { get; set; } = "1.0.0";

    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态 (0=成功, 1=版本不匹配, 2=服务器满)
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public override int GetEstimatedSize() => 256;

    public override void Deserialize(ReadOnlySpan<byte> buffer)
    {
        // 服务器Packet通常不需要反序列化
        throw new NotSupportedException("服务器Packet不支持反序列化");
    }

    public override int Serialize(Span<byte> buffer)
    {
        int offset = 0;

        // 写入服务器版本（固定32字节）
        WriteFixedString(buffer, ref offset, ServerVersion, 32);

        // 写入连接ID（固定64字节）
        WriteFixedString(buffer, ref offset, ConnectionId, 64);

        // 写入状态
        WriteByte(buffer, ref offset, Status);

        // 写入消息（带长度前缀）
        WriteString(buffer, ref offset, Message);

        return offset;
    }
}

using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network.Protocols.Aion;

/// <summary>
/// 服务器Pong Packet (Opcode: 0x06)
/// 服务器发送Ping请求给客户端
/// </summary>
public class SM_Pong : ServerPacket
{
    public override ushort Opcode => 0x06;

    /// <summary>
    /// Ping时间戳
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public override int GetEstimatedSize() => 8;

    public override void Deserialize(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException("服务器Packet不支持反序列化");
    }

    public override int Serialize(Span<byte> buffer)
    {
        int offset = 0;
        WriteInt64(buffer, ref offset, Timestamp);
        return offset;
    }
}

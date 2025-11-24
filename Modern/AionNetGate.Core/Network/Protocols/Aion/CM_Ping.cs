using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network.Protocols.Aion;

/// <summary>
/// 客户端Ping Packet (Opcode: 0x05)
/// 客户端响应服务器的Ping请求
/// </summary>
public class CM_Ping : ClientPacket
{
    public override ushort Opcode => 0x05;

    /// <summary>
    /// Ping时间戳
    /// </summary>
    public long Timestamp { get; set; }

    public override int GetEstimatedSize() => 8;

    public override void Deserialize(ReadOnlySpan<byte> buffer)
    {
        int offset = 0;
        Timestamp = ReadInt64(buffer, ref offset);
    }

    public override int Serialize(Span<byte> buffer)
    {
        throw new NotSupportedException("客户端Packet不支持序列化");
    }
}

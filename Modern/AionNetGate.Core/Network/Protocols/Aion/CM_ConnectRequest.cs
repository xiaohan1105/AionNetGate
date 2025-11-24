using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network.Protocols.Aion;

/// <summary>
/// 客户端连接请求Packet (Opcode: 0x00)
/// 客户端首次连接时发送此Packet
/// </summary>
public class CM_ConnectRequest : ClientPacket
{
    public override ushort Opcode => 0x00;

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// 硬件ID
    /// </summary>
    public string HardwareId { get; set; } = string.Empty;

    /// <summary>
    /// MAC地址
    /// </summary>
    public string MacAddress { get; set; } = string.Empty;

    public override int GetEstimatedSize() => 256;

    public override void Deserialize(ReadOnlySpan<byte> buffer)
    {
        int offset = 0;

        // 读取客户端版本（固定32字节）
        ClientVersion = ReadFixedString(buffer, ref offset, 32);

        // 读取硬件ID（固定64字节）
        HardwareId = ReadFixedString(buffer, ref offset, 64);

        // 读取MAC地址（固定18字节）
        MacAddress = ReadFixedString(buffer, ref offset, 18);
    }

    public override int Serialize(Span<byte> buffer)
    {
        // 客户端Packet通常不需要序列化
        throw new NotSupportedException("客户端Packet不支持序列化");
    }
}

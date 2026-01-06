using MessagePack;

namespace AionNetGate.Network.Packets.Client;

/// <summary>
/// 客户端连接请求 Packet
/// </summary>
[MessagePackObject]
public class CM_ConnectRequest : PacketBase
{
    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketOpcode Opcode => PacketOpcode.Connect;

    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketDirection Direction => PacketDirection.ClientToServer;

    /// <summary>
    /// 硬件 ID
    /// </summary>
    [Key(0)]
    public string HardwareId { get; set; } = string.Empty;

    /// <summary>
    /// 客户端版本
    /// </summary>
    [Key(1)]
    public string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统信息
    /// </summary>
    [Key(2)]
    public string OsInfo { get; set; } = string.Empty;

    /// <summary>
    /// CPU ID
    /// </summary>
    [Key(3)]
    public string? CpuId { get; set; }

    /// <summary>
    /// MAC 地址
    /// </summary>
    [Key(4)]
    public string? MacAddress { get; set; }

    /// <summary>
    /// 主板序列号
    /// </summary>
    [Key(5)]
    public string? MotherboardSerial { get; set; }

    /// <summary>
    /// 硬盘序列号
    /// </summary>
    [Key(6)]
    public string? DiskSerial { get; set; }
}

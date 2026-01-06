using MessagePack;

namespace AionNetGate.Network.Packets.Client;

/// <summary>
/// 客户端 Ping Packet
/// </summary>
[MessagePackObject]
public class CM_Ping : PacketBase
{
    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketOpcode Opcode => PacketOpcode.Ping;

    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketDirection Direction => PacketDirection.ClientToServer;

    /// <summary>
    /// 客户端时间戳（毫秒）
    /// </summary>
    [Key(0)]
    public long ClientTimestamp { get; set; }

    /// <summary>
    /// Ping 不需要加密
    /// </summary>
    [IgnoreMember]
    public override bool IsEncrypted => false;
}

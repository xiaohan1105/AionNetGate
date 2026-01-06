using MessagePack;

namespace AionNetGate.Network.Packets.Server;

/// <summary>
/// 服务器 Pong Packet
/// </summary>
[MessagePackObject]
public class SM_Pong : PacketBase
{
    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketOpcode Opcode => PacketOpcode.Ping;

    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketDirection Direction => PacketDirection.ServerToClient;

    /// <summary>
    /// 客户端时间戳（毫秒）
    /// </summary>
    [Key(0)]
    public long ClientTimestamp { get; set; }

    /// <summary>
    /// 服务器时间戳（毫秒）
    /// </summary>
    [Key(1)]
    public long ServerTimestamp { get; set; }

    /// <summary>
    /// Pong 不需要加密
    /// </summary>
    [IgnoreMember]
    public override bool IsEncrypted => false;
}

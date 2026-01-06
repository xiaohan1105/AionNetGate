using MessagePack;

namespace AionNetGate.Network.Packets.Server;

/// <summary>
/// 服务器连接响应 Packet
/// </summary>
[MessagePackObject]
public class SM_ConnectResponse : PacketBase
{
    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketOpcode Opcode => PacketOpcode.Connect;

    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketDirection Direction => PacketDirection.ServerToClient;

    /// <summary>
    /// 连接是否成功
    /// </summary>
    [Key(0)]
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [Key(1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 服务器时间（UTC）
    /// </summary>
    [Key(2)]
    public DateTime ServerTime { get; set; }

    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    [Key(3)]
    public int HeartbeatInterval { get; set; }
}

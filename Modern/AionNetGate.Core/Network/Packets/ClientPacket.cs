namespace AionNetGate.Core.Network.Packets;

/// <summary>
/// 客户端Packet基类
/// 所有从客户端发送到服务器的Packet都继承此类
/// </summary>
public abstract class ClientPacket : PacketBase
{
    /// <summary>
    /// 客户端Packet的连接ID（处理时由框架自动设置）
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 接收时间戳
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

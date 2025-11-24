namespace AionNetGate.Core.Network.Packets;

/// <summary>
/// 服务器Packet基类
/// 所有从服务器发送到客户端的Packet都继承此类
/// </summary>
public abstract class ServerPacket : PacketBase
{
    /// <summary>
    /// 发送时间戳
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

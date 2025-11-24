using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network;

/// <summary>
/// 连接上下文接口
/// 提供对当前连接的访问和操作
/// </summary>
public interface IConnectionContext
{
    /// <summary>
    /// 连接唯一ID
    /// </summary>
    string ConnectionId { get; }

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    string RemoteIpAddress { get; }

    /// <summary>
    /// 客户端端口
    /// </summary>
    int RemotePort { get; }

    /// <summary>
    /// 连接建立时间
    /// </summary>
    DateTime ConnectedAt { get; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    DateTime LastActivityAt { get; }

    /// <summary>
    /// 是否已认证
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 账号ID（认证后）
    /// </summary>
    int? AccountId { get; }

    /// <summary>
    /// 发送Packet到客户端
    /// </summary>
    Task SendPacketAsync(ServerPacket packet);

    /// <summary>
    /// 批量发送Packet
    /// </summary>
    Task SendPacketsAsync(IEnumerable<ServerPacket> packets);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync(string? reason = null);

    /// <summary>
    /// 更新活动时间
    /// </summary>
    void UpdateActivity();

    /// <summary>
    /// 设置认证状态
    /// </summary>
    void SetAuthenticated(int accountId);

    /// <summary>
    /// 获取或设置自定义属性
    /// </summary>
    IDictionary<string, object> Properties { get; }

    /// <summary>
    /// 检查连接是否超时
    /// </summary>
    bool IsTimeout(int timeoutSeconds);

    /// <summary>
    /// 获取连接时长（秒）
    /// </summary>
    long GetConnectionDuration();
}

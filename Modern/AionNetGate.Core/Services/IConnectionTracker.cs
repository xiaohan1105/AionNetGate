using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Services;

/// <summary>
/// 连接跟踪服务接口（内存中）
/// </summary>
public interface IConnectionTracker
{
    /// <summary>
    /// 添加连接
    /// </summary>
    void AddConnection(ConnectionInfo connection);

    /// <summary>
    /// 移除连接
    /// </summary>
    bool RemoveConnection(string connectionId);

    /// <summary>
    /// 获取连接信息
    /// </summary>
    ConnectionInfo? GetConnection(string connectionId);

    /// <summary>
    /// 根据账号ID获取连接
    /// </summary>
    ConnectionInfo? GetConnectionByAccountId(int accountId);

    /// <summary>
    /// 获取所有连接
    /// </summary>
    IReadOnlyCollection<ConnectionInfo> GetAllConnections();

    /// <summary>
    /// 更新连接的活动时间
    /// </summary>
    void UpdateActivity(string connectionId);

    /// <summary>
    /// 获取超时连接
    /// </summary>
    IReadOnlyCollection<ConnectionInfo> GetTimeoutConnections(int timeoutSeconds = 300);

    /// <summary>
    /// 获取当前在线连接数
    /// </summary>
    int GetOnlineCount();

    /// <summary>
    /// 清理所有连接
    /// </summary>
    void Clear();
}

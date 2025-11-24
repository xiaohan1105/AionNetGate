using AionNetGate.Core.Domain.Entities;
using System.Collections.Concurrent;

namespace AionNetGate.Core.Services;

/// <summary>
/// 连接跟踪服务实现（内存中）
/// 使用线程安全的 ConcurrentDictionary
/// </summary>
public class ConnectionTracker : IConnectionTracker
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections;
    private readonly ConcurrentDictionary<int, string> _accountIdToConnectionId;

    public ConnectionTracker()
    {
        _connections = new ConcurrentDictionary<string, ConnectionInfo>();
        _accountIdToConnectionId = new ConcurrentDictionary<int, string>();
    }

    /// <summary>
    /// 添加连接
    /// </summary>
    public void AddConnection(ConnectionInfo connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrEmpty(connection.ConnectionId))
            throw new ArgumentException("连接ID不能为空", nameof(connection));

        _connections[connection.ConnectionId] = connection;

        // 如果已认证，建立账号ID到连接ID的映射
        if (connection.IsAuthenticated && connection.AccountId.HasValue)
        {
            _accountIdToConnectionId[connection.AccountId.Value] = connection.ConnectionId;
        }
    }

    /// <summary>
    /// 移除连接
    /// </summary>
    public bool RemoveConnection(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
            return false;

        if (_connections.TryRemove(connectionId, out var connection))
        {
            // 同时移除账号ID映射
            if (connection.AccountId.HasValue)
            {
                _accountIdToConnectionId.TryRemove(connection.AccountId.Value, out _);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取连接信息
    /// </summary>
    public ConnectionInfo? GetConnection(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
            return null;

        _connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    /// <summary>
    /// 根据账号ID获取连接
    /// </summary>
    public ConnectionInfo? GetConnectionByAccountId(int accountId)
    {
        if (_accountIdToConnectionId.TryGetValue(accountId, out var connectionId))
        {
            return GetConnection(connectionId);
        }

        return null;
    }

    /// <summary>
    /// 获取所有连接
    /// </summary>
    public IReadOnlyCollection<ConnectionInfo> GetAllConnections()
    {
        return _connections.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// 更新连接的活动时间
    /// </summary>
    public void UpdateActivity(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
            return;

        if (_connections.TryGetValue(connectionId, out var connection))
        {
            connection.UpdateActivity();
        }
    }

    /// <summary>
    /// 获取超时连接
    /// </summary>
    public IReadOnlyCollection<ConnectionInfo> GetTimeoutConnections(int timeoutSeconds = 300)
    {
        return _connections.Values
            .Where(c => c.IsTimeout(timeoutSeconds))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// 获取当前在线连接数
    /// </summary>
    public int GetOnlineCount()
    {
        return _connections.Count;
    }

    /// <summary>
    /// 清理所有连接
    /// </summary>
    public void Clear()
    {
        _connections.Clear();
        _accountIdToConnectionId.Clear();
    }
}

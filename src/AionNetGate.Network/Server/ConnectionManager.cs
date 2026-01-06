using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Server;

/// <summary>
/// 连接管理器实现
/// </summary>
public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, IClientConnection> _connections = new();
    private readonly ILogger<ConnectionManager> _logger;

    /// <inheritdoc/>
    public int ConnectionCount => _connections.Count;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void RegisterConnection(IClientConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (_connections.TryAdd(connection.ConnectionId, connection))
        {
            _logger.LogInformation(
                "连接已注册: {ConnectionId}, 当前在线: {Count}",
                connection.ConnectionId,
                _connections.Count);
        }
        else
        {
            _logger.LogWarning("连接注册失败（ID 已存在）: {ConnectionId}", connection.ConnectionId);
        }
    }

    /// <inheritdoc/>
    public void UnregisterConnection(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        if (_connections.TryRemove(connectionId, out _))
        {
            _logger.LogInformation(
                "连接已注销: {ConnectionId}, 当前在线: {Count}",
                connectionId,
                _connections.Count);
        }
    }

    /// <inheritdoc/>
    public IClientConnection? GetConnection(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
            return null;

        return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
    }

    /// <inheritdoc/>
    public IEnumerable<IClientConnection> GetAllConnections()
    {
        return _connections.Values.ToList();
    }

    /// <inheritdoc/>
    public async Task DisconnectAllAsync()
    {
        _logger.LogInformation("正在断开所有连接，总计: {Count}", _connections.Count);

        var tasks = _connections.Values.Select(c => c.DisconnectAsync()).ToArray();
        await Task.WhenAll(tasks);

        _connections.Clear();

        _logger.LogInformation("所有连接已断开");
    }
}

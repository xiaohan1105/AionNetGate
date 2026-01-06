namespace AionNetGate.Network.Server;

/// <summary>
/// 连接管理器接口
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// 在线连接数
    /// </summary>
    int ConnectionCount { get; }

    /// <summary>
    /// 注册连接
    /// </summary>
    void RegisterConnection(IClientConnection connection);

    /// <summary>
    /// 注销连接
    /// </summary>
    void UnregisterConnection(string connectionId);

    /// <summary>
    /// 获取连接
    /// </summary>
    IClientConnection? GetConnection(string connectionId);

    /// <summary>
    /// 获取所有连接
    /// </summary>
    IEnumerable<IClientConnection> GetAllConnections();

    /// <summary>
    /// 断开所有连接
    /// </summary>
    Task DisconnectAllAsync();
}

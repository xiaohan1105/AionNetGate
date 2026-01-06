namespace AionNetGate.Launcher.Template.Services;

/// <summary>
/// 网关客户端接口
/// </summary>
public interface IGatewayClient
{
    /// <summary>
    /// 连接到网关服务器
    /// </summary>
    Task<bool> ConnectAsync(string host, int port, CancellationToken ct = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 获取连接状态
    /// </summary>
    bool IsConnected { get; }
}

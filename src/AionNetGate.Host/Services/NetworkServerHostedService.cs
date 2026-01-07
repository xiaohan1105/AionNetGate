using AionNetGate.Core.Configuration;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Options;

namespace AionNetGate.Host.Services;

/// <summary>
/// 网络服务器后台服务
/// </summary>
public class NetworkServerHostedService : BackgroundService
{
    private readonly NetworkServer _networkServer;
    private readonly ServerConfig _serverConfig;
    private readonly ILogger<NetworkServerHostedService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public NetworkServerHostedService(
        NetworkServer networkServer,
        IOptions<ServerConfig> serverConfig,
        ILogger<NetworkServerHostedService> logger)
    {
        _networkServer = networkServer ?? throw new ArgumentNullException(nameof(networkServer));
        _serverConfig = serverConfig?.Value ?? throw new ArgumentNullException(nameof(serverConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("网络服务器启动中...");
            _logger.LogInformation("绑定地址: {BindAddress}:{Port}", _serverConfig.BindAddress, _serverConfig.Port);

            await _networkServer.StartAsync(stoppingToken);

            _logger.LogInformation("网络服务器已启动，等待客户端连接...");

            // 保持运行直到取消
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("网络服务器正在关闭...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "网络服务器发生错误");
            throw;
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止网络服务器...");

        await _networkServer.StopAsync();

        _logger.LogInformation("网络服务器已停止");

        await base.StopAsync(cancellationToken);
    }
}

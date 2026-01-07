using AionNetGate.Core.Configuration;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AionNetGate.Host.Health;

/// <summary>
/// 网关服务健康检查
/// </summary>
public class GatewayHealthCheck : IHealthCheck
{
    private readonly GatewayServer _server;
    private readonly ServerConfig _config;
    private readonly ILogger<GatewayHealthCheck> _logger;

    public GatewayHealthCheck(
        GatewayServer server,
        IOptions<ServerConfig> config,
        ILogger<GatewayHealthCheck> logger)
    {
        _server = server;
        _config = config.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["server_running"] = _server.IsRunning,
            ["current_connections"] = _server.ConnectionCount,
            ["max_connections"] = _config.MaxConnections,
            ["listen_port"] = _config.Port,
            ["connection_utilization"] = _config.MaxConnections > 0
                ? Math.Round((double)_server.ConnectionCount / _config.MaxConnections * 100, 2)
                : 0
        };

        // 检查服务器是否运行
        if (!_server.IsRunning)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "网关服务器未运行",
                data: data));
        }

        // 检查连接数是否接近上限（>90%警告）
        var utilizationPercent = (double)_server.ConnectionCount / _config.MaxConnections * 100;
        if (utilizationPercent > 90)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"连接数接近上限: {_server.ConnectionCount}/{_config.MaxConnections} ({utilizationPercent:F1}%)",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"网关运行正常，当前连接: {_server.ConnectionCount}/{_config.MaxConnections}",
            data: data));
    }
}

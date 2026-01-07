using System.Diagnostics;
using AionNetGate.Network.Server;

namespace AionNetGate.Host.Monitoring;

/// <summary>
/// 指标收集后台服务 - 定期更新系统指标
/// </summary>
public class MetricsCollectorService : BackgroundService
{
    private readonly ILogger<MetricsCollectorService> _logger;
    private readonly GatewayServer _server;
    private readonly DateTime _startTime;
    private readonly Process _currentProcess;

    public MetricsCollectorService(
        ILogger<MetricsCollectorService> logger,
        GatewayServer server)
    {
        _logger = logger;
        _server = server;
        _startTime = DateTime.UtcNow;
        _currentProcess = Process.GetCurrentProcess();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("指标收集服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CollectMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "收集指标时出错");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("指标收集服务已停止");
    }

    private void CollectMetrics()
    {
        // 运行时间
        GatewayMetrics.UptimeSeconds.Set((DateTime.UtcNow - _startTime).TotalSeconds);

        // 连接数
        GatewayMetrics.CurrentConnections.Set(_server.ConnectionCount);

        // 内存使用
        _currentProcess.Refresh();
        GatewayMetrics.MemoryUsageBytes.Set(_currentProcess.WorkingSet64);

        // CPU使用（简化版本）
        var cpuTime = _currentProcess.TotalProcessorTime.TotalSeconds;
        var uptime = (DateTime.UtcNow - _startTime).TotalSeconds;
        var cpuPercent = uptime > 0 ? (cpuTime / uptime / Environment.ProcessorCount) * 100 : 0;
        GatewayMetrics.CpuUsagePercent.Set(Math.Min(cpuPercent, 100));
    }
}

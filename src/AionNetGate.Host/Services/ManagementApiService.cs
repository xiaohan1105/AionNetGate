using System.Net;
using System.Text;
using System.Text.Json;
using AionNetGate.Core.Configuration;
using AionNetGate.Host.Health;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Prometheus;

namespace AionNetGate.Host.Services;

/// <summary>
/// 管理API服务 - 提供健康检查、指标、状态查询等HTTP端点
/// </summary>
public class ManagementApiService : BackgroundService
{
    private readonly ILogger<ManagementApiService> _logger;
    private readonly GatewayServer _server;
    private readonly HealthCheckService _healthCheckService;
    private readonly ServerConfig _config;
    private HttpListener? _listener;

    /// <summary>
    /// 管理API端口（默认为网关端口+1000）
    /// </summary>
    public int ManagementPort { get; }

    public ManagementApiService(
        ILogger<ManagementApiService> logger,
        GatewayServer server,
        HealthCheckService healthCheckService,
        IOptions<ServerConfig> config)
    {
        _logger = logger;
        _server = server;
        _healthCheckService = healthCheckService;
        _config = config.Value;
        ManagementPort = _config.Port + 1000; // 例如：网关10001 -> 管理API 11001
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://+:{ManagementPort}/");

        try
        {
            _listener.Start();
            _logger.LogInformation("管理API服务已启动，端口: {Port}", ManagementPort);
            _logger.LogInformation("  - 健康检查: http://localhost:{Port}/health", ManagementPort);
            _logger.LogInformation("  - 指标端点: http://localhost:{Port}/metrics", ManagementPort);
            _logger.LogInformation("  - 状态信息: http://localhost:{Port}/status", ManagementPort);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync().WaitAsync(stoppingToken);
                    _ = Task.Run(() => HandleRequest(context, stoppingToken), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "处理HTTP请求时出错");
                }
            }
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 5) // Access Denied
        {
            _logger.LogError("管理API端口 {Port} 需要管理员权限，或运行: netsh http add urlacl url=http://+:{Port}/ user=Everyone",
                ManagementPort, ManagementPort);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理API服务启动失败");
        }
        finally
        {
            _listener?.Stop();
            _logger.LogInformation("管理API服务已停止");
        }
    }

    private async Task HandleRequest(HttpListenerContext context, CancellationToken ct)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var path = request.Url?.AbsolutePath?.ToLowerInvariant() ?? "/";

            (int statusCode, string contentType, byte[] body) result = path switch
            {
                "/health" or "/healthz" => await HandleHealthCheck(ct),
                "/health/live" => await HandleLivenessCheck(ct),
                "/health/ready" => await HandleReadinessCheck(ct),
                "/metrics" => await HandleMetrics(ct),
                "/status" => await HandleStatus(ct),
                "/" => HandleRoot(),
                _ => (404, "text/plain", Encoding.UTF8.GetBytes("Not Found"))
            };

            response.StatusCode = result.statusCode;
            response.ContentType = result.contentType;
            response.ContentLength64 = result.body.Length;
            await response.OutputStream.WriteAsync(result.body, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "处理请求失败: {Path}", request.Url?.AbsolutePath);
            response.StatusCode = 500;
            var error = Encoding.UTF8.GetBytes($"Internal Server Error: {ex.Message}");
            response.ContentLength64 = error.Length;
            await response.OutputStream.WriteAsync(error, ct);
        }
        finally
        {
            response.Close();
        }
    }

    private (int, string, byte[]) HandleRoot()
    {
        var html = $"""
            <!DOCTYPE html>
            <html>
            <head><title>AionNetGate 管理API</title></head>
            <body>
                <h1>AionNetGate 管理API</h1>
                <ul>
                    <li><a href="/health">健康检查</a></li>
                    <li><a href="/health/live">存活检查 (Liveness)</a></li>
                    <li><a href="/health/ready">就绪检查 (Readiness)</a></li>
                    <li><a href="/metrics">Prometheus 指标</a></li>
                    <li><a href="/status">服务器状态</a></li>
                </ul>
                <p>网关端口: {_config.Port} | 管理端口: {ManagementPort}</p>
            </body>
            </html>
            """;
        return (200, "text/html; charset=utf-8", Encoding.UTF8.GetBytes(html));
    }

    private async Task<(int, string, byte[])> HandleHealthCheck(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(ct);
        var result = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    data = e.Value.Data
                })
        };

        var statusCode = report.Status switch
        {
            HealthStatus.Healthy => 200,
            HealthStatus.Degraded => 200,
            _ => 503
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        return (statusCode, "application/json", Encoding.UTF8.GetBytes(json));
    }

    private Task<(int, string, byte[])> HandleLivenessCheck(CancellationToken ct)
    {
        // 存活检查 - 只要进程还在运行就返回成功
        var result = new { status = "Alive", timestamp = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(result);
        return Task.FromResult((200, "application/json", Encoding.UTF8.GetBytes(json)));
    }

    private Task<(int, string, byte[])> HandleReadinessCheck(CancellationToken ct)
    {
        // 就绪检查 - 检查服务器是否可以接受请求
        var isReady = _server.IsRunning && _server.ConnectionCount < _config.MaxConnections;
        var result = new
        {
            status = isReady ? "Ready" : "NotReady",
            serverRunning = _server.IsRunning,
            currentConnections = _server.ConnectionCount,
            maxConnections = _config.MaxConnections
        };

        var statusCode = isReady ? 200 : 503;
        var json = JsonSerializer.Serialize(result);
        return Task.FromResult((statusCode, "application/json", Encoding.UTF8.GetBytes(json)));
    }

    private async Task<(int, string, byte[])> HandleMetrics(CancellationToken ct)
    {
        using var stream = new MemoryStream();
        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(stream, ct);
        return (200, "text/plain; version=0.0.4", stream.ToArray());
    }

    private Task<(int, string, byte[])> HandleStatus(CancellationToken ct)
    {
        var sessions = _server.Sessions.Select(s => new
        {
            sessionId = s.SessionId,
            clientIp = s.ClientIp,
            accountId = s.AccountId,
            hardwareId = s.HardwareId,
            computerName = s.ComputerName,
            launcherVersion = s.LauncherVersion,
            isConnected = s.IsConnected,
            connectedAt = s.ConnectedAt
        }).ToList();

        var result = new
        {
            version = "2.0.0",
            serverRunning = _server.IsRunning,
            listenPort = _config.Port,
            managementPort = ManagementPort,
            connections = new
            {
                current = _server.ConnectionCount,
                max = _config.MaxConnections,
                utilizationPercent = Math.Round((double)_server.ConnectionCount / _config.MaxConnections * 100, 2)
            },
            sessions = sessions,
            uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return Task.FromResult((200, "application/json", Encoding.UTF8.GetBytes(json)));
    }

    public override void Dispose()
    {
        _listener?.Close();
        base.Dispose();
    }
}

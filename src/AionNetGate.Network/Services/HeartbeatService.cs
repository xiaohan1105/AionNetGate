using AionNetGate.Core.Configuration;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Network.Services;

/// <summary>
/// 心跳服务 - 定期检测客户端连接状态
/// </summary>
public class HeartbeatService : BackgroundService
{
    private readonly GatewayServer _server;
    private readonly ServerConfig _config;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly ConnectionHealthService _healthService;

    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    public int HeartbeatInterval => _config.HeartbeatInterval;

    /// <summary>
    /// 连接超时时间（秒）
    /// </summary>
    public int ConnectionTimeout => _config.ConnectionTimeout;

    public HeartbeatService(
        GatewayServer server,
        IOptions<ServerConfig> config,
        ConnectionHealthService healthService,
        ILogger<HeartbeatService> logger)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "心跳服务已启动 - 间隔: {Interval}秒, 超时: {Timeout}秒",
            HeartbeatInterval,
            ConnectionTimeout);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(HeartbeatInterval));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CheckAllConnectionsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常停止
        }

        _logger.LogInformation("心跳服务已停止");
    }

    /// <summary>
    /// 检查所有连接
    /// </summary>
    private async Task CheckAllConnectionsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var timeoutThreshold = now.AddSeconds(-ConnectionTimeout);
        var disconnectedCount = 0;
        var activeCount = 0;

        foreach (var session in _server.Sessions)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // 检查是否超时
                if (session.LastActivityAt < timeoutThreshold)
                {
                    _logger.LogWarning(
                        "会话 {SessionId} 心跳超时，上次活动: {LastActivity}",
                        session.SessionId,
                        session.LastActivityAt);

                    // 更新健康状态
                    _healthService.RecordTimeout(session.SessionId);

                    // 断开连接
                    await session.DisconnectAsync();
                    disconnectedCount++;
                    continue;
                }

                // 发送心跳包（Pong）
                await SendHeartbeatAsync(session, cancellationToken);
                activeCount++;

                // 更新健康状态
                _healthService.RecordHeartbeat(session.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查会话 {SessionId} 时出错", session.SessionId);
            }
        }

        if (disconnectedCount > 0)
        {
            _logger.LogInformation(
                "心跳检测完成 - 活动: {Active}, 超时断开: {Disconnected}",
                activeCount,
                disconnectedCount);
        }
    }

    /// <summary>
    /// 发送心跳包
    /// </summary>
    private async ValueTask SendHeartbeatAsync(ClientSession session, CancellationToken cancellationToken)
    {
        try
        {
            // 发送 Pong 包（Opcode 0x05）
            // 包含服务器时间戳，客户端可用于计算延迟
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var payload = BitConverter.GetBytes(timestamp);

            await session.SendPacketAsync(Opcodes.SM_PONG, payload);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "发送心跳包到会话 {SessionId} 失败", session.SessionId);
        }
    }

    /// <summary>
    /// 处理客户端 Ping 响应
    /// </summary>
    public void HandlePingResponse(int sessionId, long clientTimestamp)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var latency = now - clientTimestamp;

        _healthService.RecordLatency(sessionId, latency);

        _logger.LogDebug("会话 {SessionId} 延迟: {Latency}ms", sessionId, latency);
    }
}

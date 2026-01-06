using AionNetGate.Network.Handlers;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Services;

/// <summary>
/// 网关托管服务 - 管理网关服务器生命周期
/// </summary>
public class GatewayHostedService : IHostedService, IAsyncDisposable
{
    private readonly GatewayServer _server;
    private readonly PacketDispatcher _dispatcher;
    private readonly ILogger<GatewayHostedService> _logger;

    public GatewayHostedService(
        GatewayServer server,
        PacketDispatcher dispatcher,
        ILogger<GatewayHostedService> logger)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在启动网关托管服务...");

        // 订阅服务器事件
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;
        _server.PacketReceived += OnPacketReceived;

        // 在后台启动服务器（不阻塞）
        _ = Task.Run(async () =>
        {
            try
            {
                await _server.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "网关服务器运行出错");
            }
        }, cancellationToken);

        _logger.LogInformation("网关托管服务已启动");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止网关托管服务...");

        // 取消订阅
        _server.ClientConnected -= OnClientConnected;
        _server.ClientDisconnected -= OnClientDisconnected;
        _server.PacketReceived -= OnPacketReceived;

        // 停止服务器
        await _server.StopAsync();

        _logger.LogInformation("网关托管服务已停止");
    }

    /// <summary>
    /// 客户端连接事件
    /// </summary>
    private ValueTask OnClientConnected(ClientSession session)
    {
        _logger.LogInformation(
            "客户端已连接: SessionId={SessionId}, IP={IP}",
            session.SessionId, session.ClientIp);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 客户端断开事件
    /// </summary>
    private ValueTask OnClientDisconnected(ClientSession session)
    {
        _logger.LogInformation(
            "客户端已断开: SessionId={SessionId}, IP={IP}, AccountId={AccountId}",
            session.SessionId, session.ClientIp, session.AccountId);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 收到数据包事件 - 分发到对应处理器
    /// </summary>
    private async ValueTask OnPacketReceived(ClientSession session, byte opcode, ReadOnlyMemory<byte> payload)
    {
        await _dispatcher.DispatchAsync(session, opcode, payload);
    }

    public async ValueTask DisposeAsync()
    {
        await _server.DisposeAsync();
    }
}

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using AionNetGate.Core.Configuration;
using AionNetGate.Network.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Network.Server;

/// <summary>
/// 网关服务器 - 管理所有客户端连接
/// 对应老项目的 MainService
/// </summary>
public class GatewayServer : IAsyncDisposable
{
    private readonly ServerConfig _config;
    private readonly LegacyPacketSerializer _serializer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<GatewayServer> _logger;

    private Socket? _listenSocket;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private bool _disposed;

    /// <summary>
    /// 所有活动会话
    /// </summary>
    private readonly ConcurrentDictionary<int, ClientSession> _sessions = new();

    #region 事件

    /// <summary>
    /// 服务器启动事件
    /// </summary>
    public event Action? Started;

    /// <summary>
    /// 服务器停止事件
    /// </summary>
    public event Action? Stopped;

    /// <summary>
    /// 新客户端连接事件
    /// </summary>
    public event Func<ClientSession, ValueTask>? ClientConnected;

    /// <summary>
    /// 客户端断开事件
    /// </summary>
    public event Func<ClientSession, ValueTask>? ClientDisconnected;

    /// <summary>
    /// 收到数据包事件
    /// </summary>
    public event Func<ClientSession, byte, ReadOnlyMemory<byte>, ValueTask>? PacketReceived;

    #endregion

    #region 属性

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 当前连接数
    /// </summary>
    public int ConnectionCount => _sessions.Count;

    /// <summary>
    /// 获取所有会话
    /// </summary>
    public IEnumerable<ClientSession> Sessions => _sessions.Values;

    /// <summary>
    /// 监听端口
    /// </summary>
    public int Port => _config.Port;

    #endregion

    /// <summary>
    /// 创建网关服务器
    /// </summary>
    public GatewayServer(
        IOptions<ServerConfig> config,
        LegacyPacketSerializer serializer,
        ILoggerFactory loggerFactory)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<GatewayServer>();
    }

    /// <summary>
    /// 启动服务器
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("服务器已在运行中");
            return;
        }

        _logger.LogInformation("正在启动网关服务器...");
        _logger.LogInformation("监听地址: {Address}:{Port}", _config.BindAddress, _config.Port);
        _logger.LogInformation("最大连接数: {MaxConnections}", _config.MaxConnections);

        try
        {
            // 创建监听 Socket
            var endpoint = new IPEndPoint(
                IPAddress.Parse(_config.BindAddress),
                _config.Port);

            _listenSocket = new Socket(
                endpoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // 配置 Socket
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.ReceiveBufferSize = _config.ReceiveBufferSize;
            _listenSocket.SendBufferSize = _config.SendBufferSize;

            // 绑定并监听
            _listenSocket.Bind(endpoint);
            _listenSocket.Listen(_config.MaxConnections);

            _cts = new CancellationTokenSource();
            _isRunning = true;

            _logger.LogInformation("网关服务器启动成功，监听端口: {Port}", _config.Port);
            Started?.Invoke();

            // 开始接受连接
            await AcceptConnectionsAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "服务器启动失败");
            _isRunning = false;
            throw;
        }
    }

    /// <summary>
    /// 停止服务器
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("服务器未运行");
            return;
        }

        _logger.LogInformation("正在停止网关服务器...");

        try
        {
            // 停止接受新连接
            _cts?.Cancel();

            // 关闭监听 Socket
            _listenSocket?.Close();

            // 断开所有客户端
            var disconnectTasks = _sessions.Values
                .Select(s => s.DisconnectAsync().AsTask())
                .ToArray();

            await Task.WhenAll(disconnectTasks);

            // 释放所有会话
            var disposeTasks = _sessions.Values
                .Select(s => s.DisposeAsync().AsTask())
                .ToArray();

            await Task.WhenAll(disposeTasks);

            _sessions.Clear();
            _isRunning = false;

            _logger.LogInformation("网关服务器已停止");
            Stopped?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止服务器时出错");
        }
    }

    /// <summary>
    /// 接受客户端连接
    /// </summary>
    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("开始接受客户端连接...");

        while (!cancellationToken.IsCancellationRequested && _listenSocket != null)
        {
            try
            {
                var clientSocket = await _listenSocket.AcceptAsync(cancellationToken);

                // 检查连接数限制
                if (_sessions.Count >= _config.MaxConnections)
                {
                    _logger.LogWarning("已达最大连接数 {Max}，拒绝新连接", _config.MaxConnections);
                    clientSocket.Close();
                    continue;
                }

                // 在后台处理连接
                _ = Task.Run(() => HandleClientAsync(clientSocket, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("停止接受新连接");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接受连接时出错");
            }
        }
    }

    /// <summary>
    /// 处理单个客户端连接
    /// </summary>
    private async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var sessionLogger = _loggerFactory.CreateLogger<ClientSession>();
        var session = new ClientSession(clientSocket, _serializer, sessionLogger);

        try
        {
            // 注册会话
            if (!_sessions.TryAdd(session.SessionId, session))
            {
                _logger.LogWarning("无法注册会话: {SessionId}", session.SessionId);
                await session.DisposeAsync();
                return;
            }

            _logger.LogInformation(
                "新客户端连接: SessionId={SessionId}, IP={IP}, 当前连接数={Count}",
                session.SessionId, session.ClientIp, _sessions.Count);

            // 订阅会话事件
            session.PacketReceived += OnSessionPacketReceived;
            session.Disconnected += OnSessionDisconnected;

            // 通知连接事件
            if (ClientConnected != null)
            {
                await ClientConnected.Invoke(session);
            }

            // 处理会话（阻塞直到断开）
            await session.ProcessAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理客户端连接出错: SessionId={SessionId}", session.SessionId);
        }
        finally
        {
            // 清理
            _sessions.TryRemove(session.SessionId, out _);
            session.PacketReceived -= OnSessionPacketReceived;
            session.Disconnected -= OnSessionDisconnected;
            await session.DisposeAsync();

            _logger.LogInformation(
                "客户端已断开: SessionId={SessionId}, 当前连接数={Count}",
                session.SessionId, _sessions.Count);
        }
    }

    /// <summary>
    /// 会话收到数据包
    /// </summary>
    private async ValueTask OnSessionPacketReceived(ClientSession session, byte opcode, ReadOnlyMemory<byte> payload)
    {
        if (PacketReceived != null)
        {
            await PacketReceived.Invoke(session, opcode, payload);
        }
    }

    /// <summary>
    /// 会话断开
    /// </summary>
    private async ValueTask OnSessionDisconnected(ClientSession session)
    {
        if (ClientDisconnected != null)
        {
            await ClientDisconnected.Invoke(session);
        }
    }

    /// <summary>
    /// 根据 SessionId 获取会话
    /// </summary>
    public ClientSession? GetSession(int sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    /// <summary>
    /// 根据账号 ID 获取会话
    /// </summary>
    public ClientSession? GetSessionByAccountId(long accountId)
    {
        return _sessions.Values.FirstOrDefault(s => s.AccountId == accountId);
    }

    /// <summary>
    /// 根据硬件 ID 获取会话
    /// </summary>
    public IEnumerable<ClientSession> GetSessionsByHardwareId(string hardwareId)
    {
        return _sessions.Values.Where(s => s.HardwareId == hardwareId);
    }

    /// <summary>
    /// 广播数据包给所有客户端
    /// </summary>
    public async ValueTask BroadcastAsync(byte opcode, ReadOnlyMemory<byte> payload)
    {
        var tasks = _sessions.Values
            .Where(s => s.IsConnected)
            .Select(s => s.SendPacketAsync(opcode, payload).AsTask())
            .ToArray();

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 踢出指定会话
    /// </summary>
    public async ValueTask KickSessionAsync(int sessionId, string? reason = null)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogInformation(
                "踢出客户端: SessionId={SessionId}, Reason={Reason}",
                sessionId, reason ?? "无");

            await session.DisconnectAsync();
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await StopAsync();

        _cts?.Dispose();
        _listenSocket?.Dispose();

        _logger.LogInformation("网关服务器已释放");
    }
}

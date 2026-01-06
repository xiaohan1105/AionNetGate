using System.Net;
using System.Net.Sockets;
using AionNetGate.Core.Configuration;
using AionNetGate.Network.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Network.Server;

/// <summary>
/// 网络服务器（基于 TCP + Pipelines）
/// </summary>
public class NetworkServer : IDisposable
{
    private readonly ServerConfig _config;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<NetworkServer> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private Socket? _listenSocket;
    private CancellationTokenSource? _serverCts;
    private bool _disposed;
    private bool _isRunning;

    /// <summary>
    /// 服务器启动事件
    /// </summary>
    public event Action? Started;

    /// <summary>
    /// 服务器停止事件
    /// </summary>
    public event Action? Stopped;

    /// <summary>
    /// 新连接建立事件
    /// </summary>
    public event Func<IClientConnection, Task>? ClientConnected;

    /// <summary>
    /// 连接断开事件
    /// </summary>
    public event Func<string, Task>? ClientDisconnected;

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 构造函数
    /// </summary>
    public NetworkServer(
        IOptions<ServerConfig> config,
        IPacketSerializer packetSerializer,
        IConnectionManager connectionManager,
        ILoggerFactory loggerFactory)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _packetSerializer = packetSerializer ?? throw new ArgumentNullException(nameof(packetSerializer));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<NetworkServer>();
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

        _logger.LogInformation("正在启动服务器...");
        _logger.LogInformation("绑定地址: {BindAddress}:{Port}", _config.BindAddress, _config.Port);

        try
        {
            // 创建监听 Socket
            var endpoint = new IPEndPoint(IPAddress.Parse(_config.BindAddress), _config.Port);
            _listenSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 配置 Socket 选项
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenSocket.ReceiveBufferSize = _config.ReceiveBufferSize;
            _listenSocket.SendBufferSize = _config.SendBufferSize;

            // 绑定并开始监听
            _listenSocket.Bind(endpoint);
            _listenSocket.Listen(_config.MaxConnections);

            _serverCts = new CancellationTokenSource();
            _isRunning = true;

            _logger.LogInformation("服务器启动成功，监听端口: {Port}", _config.Port);
            Started?.Invoke();

            // 开始接受连接
            await AcceptConnectionsAsync(_serverCts.Token);
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

        _logger.LogInformation("正在停止服务器...");

        try
        {
            // 停止接受新连接
            _serverCts?.Cancel();

            // 关闭监听 Socket
            _listenSocket?.Close();

            // 断开所有客户端连接
            await _connectionManager.DisconnectAllAsync();

            _isRunning = false;

            _logger.LogInformation("服务器已停止");
            Stopped?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "服务器停止时出错");
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
                // 异步接受连接
                var clientSocket = await _listenSocket.AcceptAsync(cancellationToken);

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
        var connectionLogger = _loggerFactory.CreateLogger<ClientConnection>();
        var connection = new ClientConnection(clientSocket, _packetSerializer, connectionLogger);

        try
        {
            // 注册连接
            _connectionManager.RegisterConnection(connection);

            // 订阅断开事件
            connection.Disconnected += async () =>
            {
                _connectionManager.UnregisterConnection(connection.ConnectionId);

                if (ClientDisconnected != null)
                {
                    await ClientDisconnected.Invoke(connection.ConnectionId);
                }
            };

            // 通知新连接建立
            if (ClientConnected != null)
            {
                await ClientConnected.Invoke(connection);
            }

            // 处理连接（阻塞直到连接断开）
            await connection.ProcessAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理客户端连接时出错: {ConnectionId}", connection.ConnectionId);
        }
        finally
        {
            // 清理连接
            _connectionManager.UnregisterConnection(connection.ConnectionId);
            connection.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _serverCts?.Cancel();
        _serverCts?.Dispose();

        _listenSocket?.Close();
        _listenSocket?.Dispose();

        _logger.LogInformation("网络服务器已释放");
    }
}

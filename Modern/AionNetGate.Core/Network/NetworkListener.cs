using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace AionNetGate.Core.Network;

/// <summary>
/// 基于System.IO.Pipelines的高性能网络监听器
/// 负责接受客户端连接并创建PipelineConnection
/// </summary>
public class NetworkListener : IDisposable
{
    private readonly Socket _listenSocket;
    private readonly PacketProcessor _packetProcessor;
    private readonly ILogger<NetworkListener> _logger;
    private readonly ConcurrentDictionary<string, PipelineConnection> _connections;
    private readonly CancellationTokenSource _shutdownCts;
    private bool _disposed;

    public event EventHandler<ConnectionEventArgs>? ClientConnected;
    public event EventHandler<ConnectionEventArgs>? ClientDisconnected;

    public NetworkListener(
        int port,
        PacketProcessor packetProcessor,
        ILogger<NetworkListener> logger)
    {
        _packetProcessor = packetProcessor ?? throw new ArgumentNullException(nameof(packetProcessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connections = new ConcurrentDictionary<string, PipelineConnection>();
        _shutdownCts = new CancellationTokenSource();

        // 创建监听Socket
        _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        _listenSocket.Listen(120); // 连接队列大小

        _logger.LogInformation("网络监听器已初始化: Port={Port}", port);
    }

    /// <summary>
    /// 启动监听器并开始接受连接
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("开始监听客户端连接...");

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _shutdownCts.Token);

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                try
                {
                    // 异步接受新连接
                    var clientSocket = await _listenSocket.AcceptAsync(linkedCts.Token);

                    // 配置Socket选项
                    ConfigureClientSocket(clientSocket);

                    // 生成连接ID
                    var connectionId = Guid.NewGuid().ToString("N");

                    // 创建连接上下文
                    var context = new ConnectionContext(clientSocket, connectionId);

                    // 创建Pipeline连接处理器
                    var connection = new PipelineConnection(
                        context,
                        _packetProcessor,
                        _logger);

                    // 添加到连接表
                    if (_connections.TryAdd(connectionId, connection))
                    {
                        _logger.LogInformation(
                            "新客户端连接: ConnectionId={ConnectionId}, RemoteIP={RemoteIP}",
                            connectionId, context.RemoteIpAddress);

                        // 触发连接事件
                        ClientConnected?.Invoke(this, new ConnectionEventArgs(context));

                        // 启动连接处理循环（Fire-and-forget）
                        _ = HandleConnectionAsync(connection, linkedCts.Token);
                    }
                    else
                    {
                        _logger.LogWarning("连接ID冲突（极罕见）: {ConnectionId}", connectionId);
                        clientSocket.Close();
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常取消，退出循环
                    break;
                }
                catch (SocketException ex)
                {
                    _logger.LogError(ex, "Accept连接时发生Socket错误");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Accept连接时发生未知错误");
                }
            }
        }
        finally
        {
            _logger.LogInformation("监听器已停止");
        }
    }

    /// <summary>
    /// 处理单个连接的生命周期
    /// </summary>
    private async Task HandleConnectionAsync(PipelineConnection connection, CancellationToken cancellationToken)
    {
        var connectionId = connection.Context.ConnectionId;

        try
        {
            // 启动接收和发送循环
            await connection.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "连接处理异常: ConnectionId={ConnectionId}",
                connectionId);
        }
        finally
        {
            // 从连接表移除
            _connections.TryRemove(connectionId, out _);

            // 触发断开事件
            ClientDisconnected?.Invoke(this, new ConnectionEventArgs(connection.Context));

            // 释放连接资源
            connection.Dispose();

            _logger.LogInformation(
                "客户端断开: ConnectionId={ConnectionId}, Duration={Duration}s",
                connectionId, connection.Context.GetConnectionDuration());
        }
    }

    /// <summary>
    /// 配置客户端Socket选项
    /// </summary>
    private void ConfigureClientSocket(Socket socket)
    {
        // 禁用Nagle算法以减少延迟
        socket.NoDelay = true;

        // 设置KeepAlive
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        // 设置发送/接收缓冲区大小（可根据需要调整）
        socket.SendBufferSize = 64 * 1024;    // 64KB
        socket.ReceiveBufferSize = 64 * 1024; // 64KB
    }

    /// <summary>
    /// 获取当前连接数
    /// </summary>
    public int GetConnectionCount() => _connections.Count;

    /// <summary>
    /// 获取所有连接上下文（只读）
    /// </summary>
    public IReadOnlyCollection<IConnectionContext> GetConnections()
    {
        return _connections.Values.Select(c => c.Context).ToList();
    }

    /// <summary>
    /// 通过ConnectionId查找连接
    /// </summary>
    public IConnectionContext? GetConnection(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var connection)
            ? connection.Context
            : null;
    }

    /// <summary>
    /// 断开指定连接
    /// </summary>
    public async Task DisconnectAsync(string connectionId, string? reason = null)
    {
        if (_connections.TryGetValue(connectionId, out var connection))
        {
            await connection.Context.DisconnectAsync(reason);
        }
    }

    /// <summary>
    /// 优雅关闭监听器
    /// </summary>
    public async Task StopAsync()
    {
        _logger.LogInformation("正在停止监听器...");

        // 停止接受新连接
        _shutdownCts.Cancel();

        // 等待所有现有连接关闭（最多10秒）
        var timeout = TimeSpan.FromSeconds(10);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (_connections.Count > 0 && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(100);
        }

        // 强制关闭剩余连接
        foreach (var connection in _connections.Values)
        {
            try
            {
                await connection.Context.DisconnectAsync("服务器关闭");
                connection.Dispose();
            }
            catch
            {
                // 忽略关闭错误
            }
        }

        _connections.Clear();

        _logger.LogInformation("监听器已停止，所有连接已关闭");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _shutdownCts.Cancel();
        _shutdownCts.Dispose();

        _listenSocket.Close();
        _listenSocket.Dispose();

        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }

        _connections.Clear();
    }
}

/// <summary>
/// 连接事件参数
/// </summary>
public class ConnectionEventArgs : EventArgs
{
    public IConnectionContext Context { get; }

    public ConnectionEventArgs(IConnectionContext context)
    {
        Context = context;
    }
}

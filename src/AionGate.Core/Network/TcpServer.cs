using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace AionGate.Core.Network;

/// <summary>
/// 高性能 TCP 服务器 (基于 System.IO.Pipelines)
/// </summary>
public class TcpServer : IAsyncDisposable
{
    private readonly ILogger<TcpServer> _logger;
    private readonly TcpServerOptions _options;
    private readonly ObjectPool<SocketAsyncEventArgs> _socketArgsPool;
    private readonly Channel<TcpConnection> _acceptQueue;

    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;

    public event Func<TcpConnection, Task>? OnConnectionAccepted;
    public event Func<TcpConnection, Exception?, Task>? OnConnectionClosed;

    public TcpServer(
        ILogger<TcpServer> logger,
        TcpServerOptions options)
    {
        _logger = logger;
        _options = options;

        // 对象池提高性能
        _socketArgsPool = ObjectPool.Create(new SocketAsyncEventArgsPooledObjectPolicy());

        // 使用 Channel 处理连接队列
        _acceptQueue = Channel.CreateBounded<TcpConnection>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_listener != null)
        {
            throw new InvalidOperationException("Server is already running");
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _listener = new TcpListener(IPAddress.Any, _options.Port);

        // 配置 TCP 选项
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listener.Server.NoDelay = true; // 禁用 Nagle 算法

        _listener.Start(_options.Backlog);
        _logger.LogInformation("TCP Server started on port {Port}", _options.Port);

        // 启动接受连接任务
        _acceptTask = AcceptConnectionsAsync(_cts.Token);

        // 启动连接处理任务
        _ = ProcessConnectionsAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        if (_listener == null || _cts == null)
        {
            return;
        }

        _logger.LogInformation("Stopping TCP Server...");

        _cts.Cancel();
        _listener.Stop();

        if (_acceptTask != null)
        {
            try
            {
                await _acceptTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        _acceptQueue.Writer.Complete();

        _logger.LogInformation("TCP Server stopped");
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(cancellationToken);

                // 配置客户端 Socket
                client.NoDelay = true;
                client.ReceiveBufferSize = _options.ReceiveBufferSize;
                client.SendBufferSize = _options.SendBufferSize;

                var connection = new TcpConnection(client, _logger);

                // 添加到处理队列
                await _acceptQueue.Writer.WriteAsync(connection, cancellationToken);

                _logger.LogDebug("Accepted connection from {RemoteEndpoint}", client.Client.RemoteEndPoint);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting connection");
            }
        }
    }

    private async Task ProcessConnectionsAsync(CancellationToken cancellationToken)
    {
        await foreach (var connection in _acceptQueue.Reader.ReadAllAsync(cancellationToken))
        {
            // 在独立任务中处理每个连接
            _ = Task.Run(async () =>
            {
                try
                {
                    if (OnConnectionAccepted != null)
                    {
                        await OnConnectionAccepted(connection);
                    }

                    await connection.ProcessAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing connection {ConnectionId}", connection.Id);
                }
                finally
                {
                    if (OnConnectionClosed != null)
                    {
                        await OnConnectionClosed(connection, null);
                    }

                    await connection.DisposeAsync();
                }
            }, cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// TCP 连接封装
/// </summary>
public class TcpConnection : IAsyncDisposable
{
    private readonly TcpClient _client;
    private readonly ILogger _logger;
    private readonly PipeReader _reader;
    private readonly PipeWriter _writer;
    private readonly string _id;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public string Id => _id;
    public string RemoteAddress { get; }
    public DateTime ConnectedAt { get; }
    public bool IsConnected => _client.Connected;

    public TcpConnection(TcpClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
        _id = Guid.NewGuid().ToString("N");
        RemoteAddress = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        ConnectedAt = DateTime.UtcNow;

        // 使用 Pipelines 优化 I/O
        var stream = client.GetStream();
        _reader = PipeReader.Create(stream);
        _writer = PipeWriter.Create(stream);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            if (buffer.IsEmpty && result.IsCompleted)
            {
                break;
            }

            // 子类应重写此方法处理数据
            await ProcessBufferAsync(buffer, cancellationToken);

            _reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await _reader.CompleteAsync();
    }

    protected virtual Task ProcessBufferAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        // 子类实现数据包解析逻辑
        return Task.CompletedTask;
    }

    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await _writer.WriteAsync(data, cancellationToken);
            await _writer.FlushAsync(cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _reader.CompleteAsync();
        await _writer.CompleteAsync();
        _sendLock.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// TCP 服务器配置
/// </summary>
public class TcpServerOptions
{
    public int Port { get; set; } = 2107;
    public int Backlog { get; set; } = 100;
    public int ReceiveBufferSize { get; set; } = 8192;
    public int SendBufferSize { get; set; } = 8192;
    public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// SocketAsyncEventArgs 对象池策略
/// </summary>
internal class SocketAsyncEventArgsPooledObjectPolicy : IPooledObjectPolicy<SocketAsyncEventArgs>
{
    public SocketAsyncEventArgs Create()
    {
        return new SocketAsyncEventArgs();
    }

    public bool Return(SocketAsyncEventArgs obj)
    {
        obj.AcceptSocket = null;
        obj.SetBuffer(null, 0, 0);
        return true;
    }
}

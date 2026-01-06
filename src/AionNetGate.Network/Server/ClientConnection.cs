using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using AionNetGate.Network.Packets;
using AionNetGate.Network.Serialization;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Server;

/// <summary>
/// 客户端连接实现（基于 System.IO.Pipelines）
/// </summary>
public class ClientConnection : IClientConnection
{
    private readonly Socket _socket;
    private readonly IPacketSerializer _packetSerializer;
    private readonly ILogger<ClientConnection> _logger;
    private readonly Pipe _receivePipe;
    private readonly Pipe _sendPipe;
    private readonly CancellationTokenSource _disposeCts;
    private bool _disposed;

    /// <summary>
    /// 接收到 Packet 的事件
    /// </summary>
    public event Func<IPacket, Task>? PacketReceived;

    /// <summary>
    /// 连接断开的事件
    /// </summary>
    public event Func<Task>? Disconnected;

    /// <inheritdoc/>
    public string ConnectionId { get; }

    /// <inheritdoc/>
    public string ClientIp { get; }

    /// <inheritdoc/>
    public EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;

    /// <inheritdoc/>
    public DateTime ConnectedAt { get; }

    /// <inheritdoc/>
    public bool IsConnected => !_disposed && _socket.Connected;

    /// <inheritdoc/>
    public long? AccountId { get; set; }

    /// <inheritdoc/>
    public string? Username { get; set; }

    /// <inheritdoc/>
    public string? HardwareId { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ClientConnection(
        Socket socket,
        IPacketSerializer packetSerializer,
        ILogger<ClientConnection> logger)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _packetSerializer = packetSerializer ?? throw new ArgumentNullException(nameof(packetSerializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConnectionId = Guid.NewGuid().ToString("N");
        ClientIp = _socket.RemoteEndPoint?.ToString() ?? "Unknown";
        ConnectedAt = DateTime.UtcNow;

        _receivePipe = new Pipe();
        _sendPipe = new Pipe();
        _disposeCts = new CancellationTokenSource();

        _logger.LogInformation("客户端连接已建立: {ConnectionId} from {ClientIp}", ConnectionId, ClientIp);
    }

    /// <summary>
    /// 开始处理连接
    /// </summary>
    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
        var token = linkedCts.Token;

        try
        {
            // 启动三个并发任务
            var receiveTask = ReceiveFromSocketAsync(token);
            var sendTask = SendToSocketAsync(token);
            var processTask = ProcessPacketsAsync(token);

            await Task.WhenAll(receiveTask, sendTask, processTask);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("连接处理已取消: {ConnectionId}", ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接处理出错: {ConnectionId}", ConnectionId);
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    /// <summary>
    /// 从 Socket 接收数据到 Pipe
    /// </summary>
    private async Task ReceiveFromSocketAsync(CancellationToken cancellationToken)
    {
        var writer = _receivePipe.Writer;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 获取缓冲区
                var memory = writer.GetMemory(8192);

                // 从 Socket 接收数据
                var bytesReceived = await _socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);

                if (bytesReceived == 0)
                {
                    _logger.LogInformation("客户端已断开连接: {ConnectionId}", ConnectionId);
                    break;
                }

                // 通知 Pipe 写入了多少数据
                writer.Advance(bytesReceived);

                // 刷新到 Pipe
                var result = await writer.FlushAsync(cancellationToken);

                if (result.IsCompleted)
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "接收数据出错: {ConnectionId}", ConnectionId);
        }
        finally
        {
            await writer.CompleteAsync();
        }
    }

    /// <summary>
    /// 从 Pipe 发送数据到 Socket
    /// </summary>
    private async Task SendToSocketAsync(CancellationToken cancellationToken)
    {
        var reader = _sendPipe.Reader;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    // 发送所有数据
                    foreach (var segment in buffer)
                    {
                        await _socket.SendAsync(segment, SocketFlags.None, cancellationToken);
                    }

                    reader.AdvanceTo(buffer.End);
                }

                if (result.IsCompleted)
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送数据出错: {ConnectionId}", ConnectionId);
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    /// <summary>
    /// 处理接收到的 Packets
    /// </summary>
    private async Task ProcessPacketsAsync(CancellationToken cancellationToken)
    {
        var reader = _receivePipe.Reader;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                // 尝试读取完整的 Packet
                while (TryReadPacket(ref buffer, out var packet))
                {
                    if (packet != null)
                    {
                        await HandlePacketAsync(packet, cancellationToken);
                    }
                }

                // 通知 Pipe 已消费的位置
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Packet 出错: {ConnectionId}", ConnectionId);
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    /// <summary>
    /// 尝试从缓冲区读取完整的 Packet
    /// </summary>
    private bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out IPacket? packet)
    {
        packet = null;

        if (buffer.Length < PacketHeader.Size)
            return false;

        // 读取到连续内存
        Span<byte> headerSpan = stackalloc byte[PacketHeader.Size];
        buffer.Slice(0, PacketHeader.Size).CopyTo(headerSpan);

        // 尝试读取 Packet
        if (_packetSerializer.TryReadPacket(buffer.ToArray(), out packet, out var bytesConsumed))
        {
            buffer = buffer.Slice(bytesConsumed);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 处理单个 Packet
    /// </summary>
    private async Task HandlePacketAsync(IPacket packet, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("收到 Packet: {Opcode} from {ConnectionId}", packet.Opcode, ConnectionId);

            if (PacketReceived != null)
            {
                await PacketReceived.Invoke(packet);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Packet 失败: {Opcode} from {ConnectionId}", packet.Opcode, ConnectionId);
        }
    }

    /// <inheritdoc/>
    public async Task SendPacketAsync(IPacket packet, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ClientConnection));

        try
        {
            // 序列化 Packet
            var data = await _packetSerializer.SerializeAsync(packet, cancellationToken);

            // 写入到发送 Pipe
            var writer = _sendPipe.Writer;
            await writer.WriteAsync(data, cancellationToken);

            _logger.LogDebug("发送 Packet: {Opcode} to {ConnectionId}", packet.Opcode, ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送 Packet 失败: {Opcode} to {ConnectionId}", packet.Opcode, ConnectionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync()
    {
        if (_disposed)
            return;

        _logger.LogInformation("正在断开连接: {ConnectionId}", ConnectionId);

        try
        {
            _disposeCts.Cancel();

            if (Disconnected != null)
            {
                await Disconnected.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接时出错: {ConnectionId}", ConnectionId);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _disposeCts.Cancel();
        _disposeCts.Dispose();

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch { }

        _socket.Close();
        _socket.Dispose();

        _logger.LogInformation("连接已释放: {ConnectionId}", ConnectionId);
    }
}

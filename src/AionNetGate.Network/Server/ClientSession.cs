using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using AionNetGate.Network.Protocol;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Server;

/// <summary>
/// 客户端会话 - 兼容老协议
/// 对应老项目的 AionConnection + LauncherInfo
/// </summary>
public class ClientSession : IAsyncDisposable
{
    private readonly Socket _socket;
    private readonly LegacyPacketSerializer _serializer;
    private readonly ILogger<ClientSession> _logger;
    private readonly Pipe _receivePipe;
    private readonly ConcurrentQueue<byte[]> _sendQueue;
    private readonly SemaphoreSlim _sendLock;
    private readonly CancellationTokenSource _cts;
    private bool _disposed;
    private bool _isConnected;

    #region 会话信息（对应 LauncherInfo）

    /// <summary>
    /// 会话唯一标识
    /// </summary>
    public int SessionId { get; }

    /// <summary>
    /// 客户端 IP 地址
    /// </summary>
    public string ClientIp { get; }

    /// <summary>
    /// 客户端端口
    /// </summary>
    public int ClientPort { get; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    /// <summary>
    /// 计算机名
    /// </summary>
    public string? ComputerName { get; set; }

    /// <summary>
    /// 硬件指纹 ID
    /// </summary>
    public string? HardwareId { get; set; }

    /// <summary>
    /// 账号 ID
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// 玩家 ID
    /// </summary>
    public long? PlayerId { get; set; }

    /// <summary>
    /// 启动器版本
    /// </summary>
    public string? LauncherVersion { get; set; }

    /// <summary>
    /// 地理位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 是否已完成连接握手
    /// </summary>
    public bool IsHandshakeCompleted { get; set; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => !_disposed && _socket.Connected && _isConnected;

    #endregion

    #region 事件

    /// <summary>
    /// 收到数据包事件
    /// </summary>
    public event Func<ClientSession, byte, ReadOnlyMemory<byte>, ValueTask>? PacketReceived;

    /// <summary>
    /// 连接断开事件
    /// </summary>
    public event Func<ClientSession, ValueTask>? Disconnected;

    #endregion

    /// <summary>
    /// 创建客户端会话
    /// </summary>
    public ClientSession(Socket socket, LegacyPacketSerializer serializer, ILogger<ClientSession> logger)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _receivePipe = new Pipe(new PipeOptions(
            pauseWriterThreshold: 1024 * 1024,  // 1MB
            resumeWriterThreshold: 512 * 1024   // 512KB
        ));
        _sendQueue = new ConcurrentQueue<byte[]>();
        _sendLock = new SemaphoreSlim(1, 1);
        _cts = new CancellationTokenSource();

        // 初始化会话信息
        SessionId = socket.GetHashCode();
        var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
        ClientIp = remoteEndPoint?.Address.ToString() ?? "Unknown";
        ClientPort = remoteEndPoint?.Port ?? 0;
        ConnectedAt = DateTime.UtcNow;
        LastActivityAt = ConnectedAt;

        _logger.LogInformation(
            "客户端会话已创建: SessionId={SessionId}, IP={ClientIp}:{ClientPort}",
            SessionId, ClientIp, ClientPort);
    }

    /// <summary>
    /// 开始处理会话
    /// </summary>
    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        var token = linkedCts.Token;

        _isConnected = true;

        try
        {
            // 并行运行接收和处理任务
            var receiveTask = ReceiveDataAsync(token);
            var processTask = ProcessPacketsAsync(token);

            await Task.WhenAll(receiveTask, processTask);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("会话处理已取消: SessionId={SessionId}", SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "会话处理出错: SessionId={SessionId}", SessionId);
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    /// <summary>
    /// 从 Socket 接收数据
    /// </summary>
    private async Task ReceiveDataAsync(CancellationToken cancellationToken)
    {
        var writer = _receivePipe.Writer;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 获取缓冲区
                var memory = writer.GetMemory(8192);

                // 接收数据
                var bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);

                if (bytesRead == 0)
                {
                    _logger.LogInformation("客户端断开连接: SessionId={SessionId}", SessionId);
                    break;
                }

                // 更新活动时间
                LastActivityAt = DateTime.UtcNow;

                // 告知 Pipe 写入了多少数据
                writer.Advance(bytesRead);

                // 刷新
                var result = await writer.FlushAsync(cancellationToken);
                if (result.IsCompleted)
                    break;
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            _logger.LogInformation("客户端连接被重置: SessionId={SessionId}", SessionId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "接收数据出错: SessionId={SessionId}", SessionId);
        }
        finally
        {
            await writer.CompleteAsync();
        }
    }

    /// <summary>
    /// 处理接收到的数据包
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

                // 尝试读取并处理所有完整的数据包
                while (_serializer.TryReadPacket(buffer, out var opcode, out var payload, out var consumed))
                {
                    // 处理数据包
                    await HandlePacketAsync(opcode, payload);

                    // 移动缓冲区位置
                    buffer = buffer.Slice(consumed);
                }

                // 告知 Pipe 已消费的位置
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                    break;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "处理数据包出错: SessionId={SessionId}", SessionId);
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    /// <summary>
    /// 处理单个数据包
    /// </summary>
    private async ValueTask HandlePacketAsync(byte opcode, ReadOnlyMemory<byte> payload)
    {
        var opcodeName = Opcodes.GetName(opcode, isClientPacket: true);

        // 首个数据包必须是连接请求
        if (!IsHandshakeCompleted)
        {
            if (opcode != Opcodes.CM_CONNECT)
            {
                _logger.LogWarning(
                    "首个数据包不是连接请求，拒绝: SessionId={SessionId}, Opcode={Opcode}",
                    SessionId, opcodeName);
                await DisconnectAsync();
                return;
            }
        }

        _logger.LogDebug(
            "收到数据包: SessionId={SessionId}, Opcode={Opcode}, Size={Size}",
            SessionId, opcodeName, payload.Length);

        // 触发事件
        if (PacketReceived != null)
        {
            try
            {
                await PacketReceived.Invoke(this, opcode, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "处理数据包失败: SessionId={SessionId}, Opcode={Opcode}",
                    SessionId, opcodeName);
            }
        }
    }

    /// <summary>
    /// 发送数据包
    /// </summary>
    public async ValueTask SendPacketAsync(byte opcode, ReadOnlyMemory<byte> payload)
    {
        if (_disposed || !_socket.Connected)
            return;

        try
        {
            // 构建数据包
            var packet = _serializer.BuildPacket(opcode, payload.Span);

            // 发送
            await _sendLock.WaitAsync();
            try
            {
                await _socket.SendAsync(packet, SocketFlags.None);

                var opcodeName = Opcodes.GetName(opcode, isClientPacket: false);
                _logger.LogDebug(
                    "发送数据包: SessionId={SessionId}, Opcode={Opcode}, Size={Size}",
                    SessionId, opcodeName, packet.Length);
            }
            finally
            {
                _sendLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送数据包失败: SessionId={SessionId}", SessionId);
        }
    }

    /// <summary>
    /// 使用 PacketWriter 发送数据包
    /// </summary>
    public async ValueTask SendPacketAsync(byte opcode, Action<PacketWriter> writeAction)
    {
        using var writer = new PacketWriter();
        writeAction(writer);
        await SendPacketAsync(opcode, writer.ToArray());
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async ValueTask DisconnectAsync()
    {
        if (_disposed)
            return;

        _isConnected = false;
        _logger.LogInformation("正在断开连接: SessionId={SessionId}", SessionId);

        try
        {
            await _cts.CancelAsync();

            if (Disconnected != null)
            {
                await Disconnected.Invoke(this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接时出错: SessionId={SessionId}", SessionId);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await _cts.CancelAsync();
        _cts.Dispose();
        _sendLock.Dispose();

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch { }

        _socket.Close();
        _socket.Dispose();

        _logger.LogInformation("会话已释放: SessionId={SessionId}", SessionId);
    }
}

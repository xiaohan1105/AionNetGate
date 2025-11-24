using AionNetGate.Core.Network.Packets;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace AionNetGate.Core.Network;

/// <summary>
/// 基于System.IO.Pipelines的单连接处理器
/// 负责Socket的异步收发和Packet的序列化/反序列化
/// </summary>
public class PipelineConnection : IDisposable
{
    private readonly ConnectionContext _context;
    private readonly PacketProcessor _packetProcessor;
    private readonly ILogger _logger;
    private readonly Pipe _receivePipe;
    private readonly Pipe _sendPipe;
    private bool _disposed;

    public IConnectionContext Context => _context;

    public PipelineConnection(
        ConnectionContext context,
        PacketProcessor packetProcessor,
        ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _packetProcessor = packetProcessor ?? throw new ArgumentNullException(nameof(packetProcessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // 创建Pipelines（可配置缓冲区大小和阈值）
        var pipeOptions = new PipeOptions(
            pauseWriterThreshold: 128 * 1024,  // 128KB暂停写入阈值
            resumeWriterThreshold: 64 * 1024,  // 64KB恢复写入阈值
            readerScheduler: PipeScheduler.ThreadPool,
            writerScheduler: PipeScheduler.ThreadPool,
            useSynchronizationContext: false);

        _receivePipe = new Pipe(pipeOptions);
        _sendPipe = new Pipe(pipeOptions);
    }

    /// <summary>
    /// 运行连接的接收和发送循环
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // 启动3个并发任务：
        // 1. Socket → ReceivePipe (填充接收管道)
        // 2. ReceivePipe → PacketProcessor (处理接收数据)
        // 3. SendQueue → SendPipe → Socket (发送数据)

        var receiveFromSocketTask = ReceiveFromSocketAsync(cancellationToken);
        var processPacketsTask = ProcessPacketsAsync(cancellationToken);
        var sendToSocketTask = SendToSocketAsync(cancellationToken);

        try
        {
            // 等待任何一个任务完成（通常是错误或取消）
            await Task.WhenAny(receiveFromSocketTask, processPacketsTask, sendToSocketTask);
        }
        finally
        {
            // 标记管道完成
            await _receivePipe.Writer.CompleteAsync();
            await _receivePipe.Reader.CompleteAsync();
            await _sendPipe.Writer.CompleteAsync();
            await _sendPipe.Reader.CompleteAsync();

            // 等待所有任务完成（最多2秒）
            try
            {
                await Task.WhenAll(receiveFromSocketTask, processPacketsTask, sendToSocketTask)
                    .WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // 忽略超时或其他异常
            }
        }
    }

    /// <summary>
    /// 循环1: 从Socket接收数据到ReceivePipe
    /// </summary>
    private async Task ReceiveFromSocketAsync(CancellationToken cancellationToken)
    {
        var writer = _receivePipe.Writer;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 从管道获取至少512字节的内存（零拷贝）
                var memory = writer.GetMemory(512);

                // 接收数据到内存
                int bytesRead = await _context.Socket.ReceiveAsync(
                    memory,
                    SocketFlags.None,
                    cancellationToken);

                if (bytesRead == 0)
                {
                    // 连接关闭
                    _logger.LogDebug("Socket接收到0字节，连接已关闭: {ConnectionId}",
                        _context.ConnectionId);
                    break;
                }

                // 通知管道写入了多少字节
                writer.Advance(bytesRead);

                // 刷新数据到管道（让读取端可以处理）
                var result = await writer.FlushAsync(cancellationToken);

                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("接收循环已取消: {ConnectionId}", _context.ConnectionId);
        }
        catch (SocketException ex)
        {
            _logger.LogWarning(ex, "Socket接收错误: {ConnectionId}", _context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "接收循环异常: {ConnectionId}", _context.ConnectionId);
        }
        finally
        {
            await writer.CompleteAsync();
        }
    }

    /// <summary>
    /// 循环2: 从ReceivePipe读取并处理Packets
    /// </summary>
    private async Task ProcessPacketsAsync(CancellationToken cancellationToken)
    {
        var reader = _receivePipe.Reader;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 读取数据（可能包含多个完整Packet）
                var result = await reader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                // 处理缓冲区中的所有完整Packet
                var consumed = await ProcessBufferAsync(buffer);

                // 通知管道已消费的位置
                reader.AdvanceTo(consumed, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("处理循环已取消: {ConnectionId}", _context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理循环异常: {ConnectionId}", _context.ConnectionId);
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    /// <summary>
    /// 处理缓冲区并返回已消费的位置
    /// </summary>
    private async Task<SequencePosition> ProcessBufferAsync(ReadOnlySequence<byte> buffer)
    {
        var consumed = buffer.Start;
        var examined = buffer.End;

        // 协议格式：[2字节长度][2字节Opcode][数据]
        while (buffer.Length >= 4)
        {
            // 读取包长度（不移动position）
            short packetLength = ReadInt16(buffer);

            // 检查是否有完整的包（长度 + 数据）
            var totalLength = 2 + packetLength;
            if (buffer.Length < totalLength)
            {
                // 数据不完整，等待更多数据
                break;
            }

            // 提取完整的Packet（包括长度和Opcode）
            var packetBuffer = buffer.Slice(0, totalLength);

            // 调用PacketProcessor处理
            await _packetProcessor.ProcessAsync(packetBuffer.Slice(2), _context);

            // 移动到下一个包
            buffer = buffer.Slice(totalLength);
            consumed = buffer.Start;
        }

        return consumed;
    }

    /// <summary>
    /// 从ReadOnlySequence读取Int16（小端序）
    /// </summary>
    private static short ReadInt16(ReadOnlySequence<byte> buffer)
    {
        var lengthSpan = buffer.Slice(0, 2);

        if (lengthSpan.IsSingleSegment)
        {
            return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(
                lengthSpan.FirstSpan);
        }
        else
        {
            // 使用租用的数组代替stackalloc
            var tempBuffer = ArrayPool<byte>.Shared.Rent(2);
            try
            {
                lengthSpan.CopyTo(tempBuffer);
                return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(
                    tempBuffer.AsSpan(0, 2));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tempBuffer);
            }
        }
    }

    /// <summary>
    /// 循环3: 从SendQueue读取Packets并发送到Socket
    /// </summary>
    private async Task SendToSocketAsync(CancellationToken cancellationToken)
    {
        var channelReader = _context.SendQueueReader;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 从Channel读取待发送的Packet
                if (!await channelReader.WaitToReadAsync(cancellationToken))
                {
                    // Channel已关闭
                    break;
                }

                while (channelReader.TryRead(out var packet))
                {
                    try
                    {
                        // 序列化Packet
                        var packetData = _packetProcessor.SerializePacket(packet);

                        // 发送到Socket
                        await _context.Socket.SendAsync(
                            packetData,
                            SocketFlags.None,
                            cancellationToken);

                        _logger.LogTrace(
                            "发送Packet: Type={PacketType}, Size={Size}, ConnectionId={ConnectionId}",
                            packet.GetType().Name, packetData.Length, _context.ConnectionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "发送Packet失败: Type={PacketType}, ConnectionId={ConnectionId}",
                            packet.GetType().Name, _context.ConnectionId);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("发送循环已取消: {ConnectionId}", _context.ConnectionId);
        }
        catch (SocketException ex)
        {
            _logger.LogWarning(ex, "Socket发送错误: {ConnectionId}", _context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送循环异常: {ConnectionId}", _context.ConnectionId);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _context.Dispose();
    }
}

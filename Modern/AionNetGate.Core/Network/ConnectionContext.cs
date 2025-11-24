using AionNetGate.Core.Network.Packets;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace AionNetGate.Core.Network;

/// <summary>
/// 连接上下文实现
/// 封装单个客户端连接的所有状态和操作
/// </summary>
public class ConnectionContext : IConnectionContext, IDisposable
{
    private readonly Socket _socket;
    private readonly Channel<ServerPacket> _sendQueue;
    private readonly ConcurrentDictionary<string, object> _properties;
    private bool _disposed;

    public ConnectionContext(Socket socket, string connectionId)
    {
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));

        // 创建无界发送队列（实际生产环境可以使用有界队列防止内存泄漏）
        _sendQueue = Channel.CreateUnbounded<ServerPacket>(new UnboundedChannelOptions
        {
            SingleReader = true, // 只有一个发送线程
            SingleWriter = false // 可以有多个Handler同时写入
        });

        _properties = new ConcurrentDictionary<string, object>();

        // 获取远程地址
        if (_socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
        {
            RemoteIpAddress = remoteEndPoint.Address.ToString();
            RemotePort = remoteEndPoint.Port;
        }
        else
        {
            RemoteIpAddress = "Unknown";
            RemotePort = 0;
        }

        ConnectedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public string ConnectionId { get; }
    public string RemoteIpAddress { get; }
    public int RemotePort { get; }
    public DateTime ConnectedAt { get; }
    public DateTime LastActivityAt { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public int? AccountId { get; private set; }
    public IDictionary<string, object> Properties => _properties;

    /// <summary>
    /// 获取底层Socket（内部使用）
    /// </summary>
    internal Socket Socket => _socket;

    /// <summary>
    /// 获取发送队列的Reader（用于发送线程）
    /// </summary>
    internal ChannelReader<ServerPacket> SendQueueReader => _sendQueue.Reader;

    /// <summary>
    /// 发送单个Packet
    /// </summary>
    public async Task SendPacketAsync(ServerPacket packet)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionContext));

        await _sendQueue.Writer.WriteAsync(packet);
    }

    /// <summary>
    /// 批量发送Packet
    /// </summary>
    public async Task SendPacketsAsync(IEnumerable<ServerPacket> packets)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ConnectionContext));

        foreach (var packet in packets)
        {
            await _sendQueue.Writer.WriteAsync(packet);
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public Task DisconnectAsync(string? reason = null)
    {
        if (_disposed)
            return Task.CompletedTask;

        // 完成发送队列（不再接受新Packet）
        _sendQueue.Writer.Complete();

        // 关闭Socket
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        catch
        {
            // 忽略关闭错误
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新活动时间
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置认证状态
    /// </summary>
    public void SetAuthenticated(int accountId)
    {
        IsAuthenticated = true;
        AccountId = accountId;
    }

    /// <summary>
    /// 检查连接是否超时
    /// </summary>
    public bool IsTimeout(int timeoutSeconds)
    {
        return (DateTime.UtcNow - LastActivityAt).TotalSeconds > timeoutSeconds;
    }

    /// <summary>
    /// 获取连接时长（秒）
    /// </summary>
    public long GetConnectionDuration()
    {
        return (long)(DateTime.UtcNow - ConnectedAt).TotalSeconds;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _sendQueue.Writer.Complete();

        try
        {
            _socket.Dispose();
        }
        catch
        {
            // 忽略释放错误
        }

        _properties.Clear();
    }
}

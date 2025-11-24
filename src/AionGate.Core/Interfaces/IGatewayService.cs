using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AionGate.Core.Interfaces
{
    /// <summary>
    /// 网关服务接口
    /// 负责管理客户端连接和消息路由
    /// </summary>
    public interface IGatewayService
    {
        /// <summary>
        /// 服务是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 当前活跃会话数
        /// </summary>
        int ActiveSessionCount { get; }

        /// <summary>
        /// 所有活跃会话 (只读)
        /// </summary>
        IReadOnlyDictionary<string, ISession> ActiveSessions { get; }

        /// <summary>
        /// 启动网关服务
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 停止网关服务
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 向指定会话发送消息
        /// </summary>
        Task SendAsync(string sessionId, IPacket packet);

        /// <summary>
        /// 广播消息给所有会话
        /// </summary>
        Task BroadcastAsync(IPacket packet, Predicate<ISession>? filter = null);

        /// <summary>
        /// 断开指定会话
        /// </summary>
        Task DisconnectAsync(string sessionId, string reason);

        /// <summary>
        /// 新连接事件
        /// </summary>
        event EventHandler<SessionEventArgs>? SessionConnected;

        /// <summary>
        /// 断开连接事件
        /// </summary>
        event EventHandler<SessionEventArgs>? SessionDisconnected;

        /// <summary>
        /// 收到消息事件
        /// </summary>
        event EventHandler<PacketEventArgs>? PacketReceived;
    }

    /// <summary>
    /// 会话接口
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// 会话唯一标识
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        string RemoteAddress { get; }

        /// <summary>
        /// 连接时间
        /// </summary>
        DateTime ConnectedAt { get; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        DateTime LastActivityAt { get; }

        /// <summary>
        /// 是否已认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 关联的账号ID (认证后)
        /// </summary>
        long? AccountId { get; }

        /// <summary>
        /// 会话元数据
        /// </summary>
        IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// 发送数据包
        /// </summary>
        Task SendAsync(IPacket packet);

        /// <summary>
        /// 关闭会话
        /// </summary>
        Task CloseAsync(string reason = "");
    }

    /// <summary>
    /// 数据包接口
    /// </summary>
    public interface IPacket
    {
        /// <summary>
        /// 操作码
        /// </summary>
        ushort Opcode { get; }

        /// <summary>
        /// 序列化为字节数组
        /// </summary>
        byte[] Serialize();
    }

    /// <summary>
    /// 会话事件参数
    /// </summary>
    public class SessionEventArgs : EventArgs
    {
        public ISession Session { get; }
        public string? Reason { get; }

        public SessionEventArgs(ISession session, string? reason = null)
        {
            Session = session;
            Reason = reason;
        }
    }

    /// <summary>
    /// 数据包事件参数
    /// </summary>
    public class PacketEventArgs : EventArgs
    {
        public ISession Session { get; }
        public IPacket Packet { get; }

        public PacketEventArgs(ISession session, IPacket packet)
        {
            Session = session;
            Packet = packet;
        }
    }
}

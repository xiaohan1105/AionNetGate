using AionNetGate.Network.Packets;
using System.Net;

namespace AionNetGate.Network.Server;

/// <summary>
/// 客户端连接接口
/// </summary>
public interface IClientConnection : IDisposable
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    string ConnectionId { get; }

    /// <summary>
    /// 客户端 IP 地址
    /// </summary>
    string ClientIp { get; }

    /// <summary>
    /// 远程端点
    /// </summary>
    EndPoint? RemoteEndPoint { get; }

    /// <summary>
    /// 连接时间
    /// </summary>
    DateTime ConnectedAt { get; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 账号 ID（登录后设置）
    /// </summary>
    long? AccountId { get; set; }

    /// <summary>
    /// 用户名（登录后设置）
    /// </summary>
    string? Username { get; set; }

    /// <summary>
    /// 硬件 ID
    /// </summary>
    string? HardwareId { get; set; }

    /// <summary>
    /// 发送 Packet
    /// </summary>
    Task SendPacketAsync(IPacket packet, CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();
}

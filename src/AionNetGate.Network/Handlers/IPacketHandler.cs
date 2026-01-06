using AionNetGate.Network.Packets;
using AionNetGate.Network.Server;

namespace AionNetGate.Network.Handlers;

/// <summary>
/// Packet 处理器接口
/// </summary>
/// <typeparam name="TPacket">Packet 类型</typeparam>
public interface IPacketHandler<in TPacket> where TPacket : IPacket
{
    /// <summary>
    /// 处理 Packet
    /// </summary>
    /// <param name="packet">接收到的 Packet</param>
    /// <param name="connection">客户端连接</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task HandleAsync(TPacket packet, IClientConnection connection, CancellationToken cancellationToken = default);
}

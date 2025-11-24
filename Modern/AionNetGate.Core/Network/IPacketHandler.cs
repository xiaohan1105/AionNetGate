using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network;

/// <summary>
/// Packet处理器接口
/// 所有Packet处理器都必须实现此接口
/// </summary>
/// <typeparam name="TPacket">要处理的Packet类型</typeparam>
public interface IPacketHandler<in TPacket> where TPacket : ClientPacket
{
    /// <summary>
    /// 处理Packet
    /// </summary>
    /// <param name="packet">要处理的Packet</param>
    /// <param name="context">连接上下文</param>
    /// <returns>异步任务</returns>
    Task HandleAsync(TPacket packet, IConnectionContext context);
}

using AionNetGate.Core.Network.Packets;
using System.Collections.Concurrent;

namespace AionNetGate.Core.Network;

/// <summary>
/// Packet处理器注册表
/// 负责管理Packet类型到处理器的映射
/// </summary>
public class PacketHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, Type> _handlers;

    public PacketHandlerRegistry()
    {
        _handlers = new ConcurrentDictionary<Type, Type>();
    }

    /// <summary>
    /// 注册Packet处理器
    /// </summary>
    public void RegisterHandler<TPacket, THandler>()
        where TPacket : ClientPacket
        where THandler : IPacketHandler<TPacket>
    {
        var packetType = typeof(TPacket);
        var handlerType = typeof(THandler);

        if (!_handlers.TryAdd(packetType, handlerType))
        {
            throw new InvalidOperationException(
                $"Packet类型 {packetType.Name} 已注册处理器 {_handlers[packetType].Name}");
        }
    }

    /// <summary>
    /// 获取Packet的处理器类型
    /// </summary>
    public Type? GetHandlerType(Type packetType)
    {
        _handlers.TryGetValue(packetType, out var handlerType);
        return handlerType;
    }

    /// <summary>
    /// 检查Packet是否有处理器
    /// </summary>
    public bool HasHandler(Type packetType)
    {
        return _handlers.ContainsKey(packetType);
    }

    /// <summary>
    /// 获取所有已注册的Packet类型
    /// </summary>
    public IEnumerable<Type> GetRegisteredPacketTypes()
    {
        return _handlers.Keys;
    }

    /// <summary>
    /// 清空所有注册
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
    }
}

using System.Collections.Concurrent;
using AionNetGate.Network.Packets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers;

/// <summary>
/// Packet Handler 注册表
/// </summary>
public class PacketHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, Type> _handlerTypes = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PacketHandlerRegistry> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PacketHandlerRegistry(IServiceProvider serviceProvider, ILogger<PacketHandlerRegistry> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 注册 Packet Handler
    /// </summary>
    public void RegisterHandler<TPacket, THandler>()
        where TPacket : IPacket
        where THandler : IPacketHandler<TPacket>
    {
        var packetType = typeof(TPacket);
        var handlerType = typeof(THandler);

        if (_handlerTypes.TryAdd(packetType, handlerType))
        {
            _logger.LogDebug("注册 Handler: {PacketType} -> {HandlerType}", packetType.Name, handlerType.Name);
        }
        else
        {
            _logger.LogWarning("Handler 已注册: {PacketType}", packetType.Name);
        }
    }

    /// <summary>
    /// 获取 Packet Handler
    /// </summary>
    public object? GetHandler(Type packetType)
    {
        if (!_handlerTypes.TryGetValue(packetType, out var handlerType))
        {
            _logger.LogWarning("未找到 Handler: {PacketType}", packetType.Name);
            return null;
        }

        return _serviceProvider.GetService(handlerType);
    }

    /// <summary>
    /// 获取强类型 Packet Handler
    /// </summary>
    public IPacketHandler<TPacket>? GetHandler<TPacket>() where TPacket : IPacket
    {
        return GetHandler(typeof(TPacket)) as IPacketHandler<TPacket>;
    }
}

using System.Collections.Frozen;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers;

/// <summary>
/// 兼容老协议的数据包处理器基类
/// </summary>
public interface ILegacyPacketHandler
{
    /// <summary>
    /// 处理器对应的操作码
    /// </summary>
    byte Opcode { get; }

    /// <summary>
    /// 处理数据包
    /// </summary>
    /// <param name="session">客户端会话</param>
    /// <param name="payload">数据包负载（不含opcode）</param>
    ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload);
}

/// <summary>
/// 数据包处理器注册表 - 存储处理器类型而非实例
/// 使用 FrozenDictionary 优化读取性能（注册后冻结）
/// </summary>
public class LegacyPacketHandlerRegistry
{
    private readonly Dictionary<byte, Type> _handlerTypes = [];
    private FrozenDictionary<byte, Type>? _frozenHandlerTypes;

    /// <summary>
    /// 通过 opcode 和类型注册处理器
    /// </summary>
    public void Register(byte opcode, Type handlerType)
    {
        ArgumentNullException.ThrowIfNull(handlerType);

        if (_frozenHandlerTypes is not null)
            throw new InvalidOperationException("注册表已冻结，无法再注册新处理器");

        if (!typeof(ILegacyPacketHandler).IsAssignableFrom(handlerType))
            throw new ArgumentException($"类型 {handlerType.Name} 必须实现 ILegacyPacketHandler");

        _handlerTypes[opcode] = handlerType;
    }

    /// <summary>
    /// 冻结注册表（调用后不可再注册新处理器，但查询性能更高）
    /// </summary>
    public void Freeze()
    {
        _frozenHandlerTypes ??= _handlerTypes.ToFrozenDictionary();
    }

    /// <summary>
    /// 获取处理器类型
    /// </summary>
    public Type? GetHandlerType(byte opcode)
    {
        // 优先使用冻结字典（性能更好）
        if (_frozenHandlerTypes is not null)
            return _frozenHandlerTypes.GetValueOrDefault(opcode);

        return _handlerTypes.GetValueOrDefault(opcode);
    }

    /// <summary>
    /// 是否存在处理器
    /// </summary>
    public bool HasHandler(byte opcode) =>
        _frozenHandlerTypes?.ContainsKey(opcode) ?? _handlerTypes.ContainsKey(opcode);

    /// <summary>
    /// 获取所有已注册的操作码
    /// </summary>
    public IEnumerable<byte> RegisteredOpcodes =>
        _frozenHandlerTypes is not null
            ? _frozenHandlerTypes.Keys.AsEnumerable()
            : _handlerTypes.Keys;

    /// <summary>
    /// 是否已冻结
    /// </summary>
    public bool IsFrozen => _frozenHandlerTypes is not null;
}

/// <summary>
/// 数据包分发器 - 根据 Opcode 分发到对应处理器
/// </summary>
/// <param name="registry">处理器注册表</param>
/// <param name="serviceProvider">服务提供者</param>
/// <param name="logger">日志记录器</param>
public class PacketDispatcher(
    LegacyPacketHandlerRegistry registry,
    IServiceProvider serviceProvider,
    ILogger<PacketDispatcher> logger)
{
    /// <summary>
    /// 分发数据包
    /// </summary>
    public async ValueTask DispatchAsync(ClientSession session, byte opcode, ReadOnlyMemory<byte> payload)
    {
        var handlerType = registry.GetHandlerType(opcode);

        if (handlerType == null)
        {
            var opcodeName = Opcodes.GetName(opcode, isClientPacket: true);
            logger.LogWarning(
                "未找到处理器: Opcode={Opcode}, SessionId={SessionId}",
                opcodeName, session.SessionId);
            return;
        }

        try
        {
            // 创建 scope 来解析 scoped 服务
            using var scope = serviceProvider.CreateScope();
            var handler = (ILegacyPacketHandler)scope.ServiceProvider.GetRequiredService(handlerType);
            await handler.HandleAsync(session, payload);
        }
        catch (Exception ex)
        {
            var opcodeName = Opcodes.GetName(opcode, isClientPacket: true);
            logger.LogError(ex,
                "处理数据包失败: Opcode={Opcode}, SessionId={SessionId}",
                opcodeName, session.SessionId);
        }
    }
}

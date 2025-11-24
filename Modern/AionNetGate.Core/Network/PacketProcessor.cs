using AionNetGate.Core.Network.Packets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace AionNetGate.Core.Network;

/// <summary>
/// Packet处理器
/// 负责将字节流解析为Packet对象并分发给对应的Handler
/// </summary>
public class PacketProcessor
{
    private readonly PacketRegistry _packetRegistry;
    private readonly PacketHandlerRegistry _handlerRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PacketProcessor> _logger;

    public PacketProcessor(
        PacketRegistry packetRegistry,
        PacketHandlerRegistry handlerRegistry,
        IServiceProvider serviceProvider,
        ILogger<PacketProcessor> logger)
    {
        _packetRegistry = packetRegistry ?? throw new ArgumentNullException(nameof(packetRegistry));
        _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理接收到的数据
    /// </summary>
    /// <param name="buffer">接收缓冲区</param>
    /// <param name="context">连接上下文</param>
    public async Task ProcessAsync(ReadOnlySequence<byte> buffer, IConnectionContext context)
    {
        // 解析所有完整的包
        var packets = ParsePackets(buffer, context);

        // 异步处理所有包
        foreach (var (packet, packetType) in packets)
        {
            try
            {
                await DispatchToHandlerAsync(packet, packetType, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "处理Packet失败: Type={PacketType}, ConnectionId={ConnectionId}",
                    packetType.Name, context.ConnectionId);
            }
        }
    }

    /// <summary>
    /// 解析缓冲区中的所有完整Packet（非异步，可以使用SequenceReader）
    /// </summary>
    private List<(ClientPacket Packet, Type PacketType)> ParsePackets(
        ReadOnlySequence<byte> buffer,
        IConnectionContext context)
    {
        var result = new List<(ClientPacket, Type)>();

        // 协议格式：[2字节长度][2字节Opcode][数据]
        var reader = new SequenceReader<byte>(buffer);

        while (reader.Remaining >= 4) // 至少需要4字节（长度+Opcode）
        {
            // 读取包长度
            short packetLength;
            if (!reader.TryReadLittleEndian(out packetLength))
                break;

            // 检查是否有完整的包
            if (reader.Remaining < packetLength)
                break;

            // 读取Opcode
            short opcodeShort;
            if (!reader.TryReadLittleEndian(out opcodeShort))
                break;

            ushort opcode = (ushort)opcodeShort;

            // 包数据长度（不包括opcode）
            var packetDataLength = packetLength - 2;

            // 获取Packet类型
            var packetType = _packetRegistry.GetClientPacketType(opcode);
            if (packetType == null)
            {
                _logger.LogWarning(
                    "收到未注册的Opcode {Opcode:X4} from {ConnectionId}",
                    opcode, context.ConnectionId);

                // 跳过这个包的数据部分
                if (packetDataLength > 0 && reader.Remaining >= packetDataLength)
                {
                    reader.Advance(packetDataLength);
                }
                continue;
            }

            // 获取包数据（从当前reader位置开始）
            var packetData = reader.Sequence.Slice(reader.Position, packetDataLength);

            try
            {
                // 创建Packet实例
                var packet = Activator.CreateInstance(packetType) as ClientPacket;
                if (packet == null)
                {
                    _logger.LogError(
                        "无法创建Packet实例: {PacketType}",
                        packetType.Name);
                    reader.Advance(packetDataLength);
                    continue;
                }

                // 反序列化
                byte[] packetBytes;
                if (packetData.IsSingleSegment)
                {
                    packetBytes = packetData.FirstSpan.ToArray();
                }
                else
                {
                    packetBytes = packetData.ToArray();
                }

                packet.Deserialize(packetBytes);
                packet.ConnectionId = context.ConnectionId;
                packet.ReceivedAt = DateTime.UtcNow;

                // 更新活动时间
                context.UpdateActivity();

                // 添加到结果列表
                result.Add((packet, packetType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "解析Packet失败: Opcode={Opcode:X4}, Type={PacketType}, ConnectionId={ConnectionId}",
                    opcode, packetType.Name, context.ConnectionId);
            }

            // 移动到下一个包
            reader.Advance(packetDataLength);
        }

        return result;
    }

    /// <summary>
    /// 分发Packet到对应的Handler
    /// </summary>
    private async Task DispatchToHandlerAsync(
        ClientPacket packet,
        Type packetType,
        IConnectionContext context)
    {
        var handlerType = _handlerRegistry.GetHandlerType(packetType);
        if (handlerType == null)
        {
            _logger.LogWarning(
                "Packet {PacketType} 没有注册Handler",
                packetType.Name);
            return;
        }

        try
        {
            // 从DI容器获取Handler实例
            var handler = _serviceProvider.GetRequiredService(handlerType);

            // 调用HandleAsync方法
            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod != null)
            {
                var task = handleMethod.Invoke(handler, new object[] { packet, context }) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Handler执行失败: {HandlerType}, Packet={PacketType}, ConnectionId={ConnectionId}",
                handlerType.Name, packetType.Name, context.ConnectionId);
        }
    }

    /// <summary>
    /// 序列化ServerPacket为字节数组
    /// </summary>
    public byte[] SerializePacket(ServerPacket packet)
    {
        var opcode = _packetRegistry.GetServerPacketOpcode(packet.GetType());
        var estimatedSize = packet.GetEstimatedSize();

        // 从ArrayPool租用缓冲区
        var buffer = ArrayPool<byte>.Shared.Rent(estimatedSize + 4); // +4 for length and opcode

        try
        {
            int offset = 0;

            // 预留长度字段（稍后回填）
            offset += 2;

            // 写入Opcode
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
                buffer.AsSpan(offset), opcode);
            offset += 2;

            // 序列化Packet数据
            var dataLength = packet.Serialize(buffer.AsSpan(offset));
            offset += dataLength;

            // 回填总长度
            var totalLength = (short)(offset - 2); // 不包括长度字段本身
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(
                buffer.AsSpan(0), totalLength);

            // 复制到新数组并返回
            var result = new byte[offset];
            Array.Copy(buffer, result, offset);

            return result;
        }
        finally
        {
            // 归还缓冲区到ArrayPool
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

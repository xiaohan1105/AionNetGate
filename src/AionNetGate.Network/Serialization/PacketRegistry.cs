using AionNetGate.Network.Packets;
using AionNetGate.Network.Packets.Client;
using AionNetGate.Network.Packets.Server;
using System.Collections.Concurrent;

namespace AionNetGate.Network.Serialization;

/// <summary>
/// Packet 注册表（Opcode 到 Type 的映射）
/// </summary>
public class PacketRegistry
{
    private readonly ConcurrentDictionary<(PacketOpcode, PacketDirection), Type> _packetTypes = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    public PacketRegistry()
    {
        RegisterDefaultPackets();
    }

    /// <summary>
    /// 注册默认 Packet 类型
    /// </summary>
    private void RegisterDefaultPackets()
    {
        // 连接相关
        RegisterPacket<CM_ConnectRequest>();
        RegisterPacket<SM_ConnectResponse>();

        // 账号相关
        RegisterPacket<CM_AccountRequest>();
        RegisterPacket<SM_AccountResponse>();

        // 心跳相关
        RegisterPacket<CM_Ping>();
        RegisterPacket<SM_Pong>();

        // TODO: 注册其他 Packet 类型
    }

    /// <summary>
    /// 注册 Packet 类型
    /// </summary>
    public void RegisterPacket<TPacket>() where TPacket : IPacket, new()
    {
        var packet = new TPacket();
        var key = (packet.Opcode, packet.Direction);
        _packetTypes[key] = typeof(TPacket);
    }

    /// <summary>
    /// 获取 Packet 类型
    /// </summary>
    public Type? GetPacketType(PacketOpcode opcode, PacketDirection direction)
    {
        var key = (opcode, direction);
        return _packetTypes.TryGetValue(key, out var type) ? type : null;
    }

    /// <summary>
    /// 创建 Packet 实例
    /// </summary>
    public IPacket? CreatePacket(PacketOpcode opcode, PacketDirection direction)
    {
        var type = GetPacketType(opcode, direction);
        if (type == null)
            return null;

        return (IPacket?)Activator.CreateInstance(type);
    }
}

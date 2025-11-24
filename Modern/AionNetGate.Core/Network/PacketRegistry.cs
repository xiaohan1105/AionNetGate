using AionNetGate.Core.Network.Packets;
using System.Collections.Concurrent;

namespace AionNetGate.Core.Network;

/// <summary>
/// Packet注册表
/// 负责Opcode到Packet类型的映射
/// 支持线程安全的注册和查找
/// </summary>
public class PacketRegistry
{
    private readonly ConcurrentDictionary<ushort, Type> _clientPackets;
    private readonly ConcurrentDictionary<Type, ushort> _serverPackets;

    public PacketRegistry()
    {
        _clientPackets = new ConcurrentDictionary<ushort, Type>();
        _serverPackets = new ConcurrentDictionary<Type, ushort>();
    }

    /// <summary>
    /// 注册客户端Packet
    /// </summary>
    public void RegisterClientPacket<TPacket>(ushort opcode) where TPacket : ClientPacket
    {
        if (!_clientPackets.TryAdd(opcode, typeof(TPacket)))
        {
            throw new InvalidOperationException(
                $"Opcode {opcode:X4} 已被注册为 {_clientPackets[opcode].Name}");
        }
    }

    /// <summary>
    /// 注册服务器Packet
    /// </summary>
    public void RegisterServerPacket<TPacket>(ushort opcode) where TPacket : ServerPacket
    {
        if (!_serverPackets.TryAdd(typeof(TPacket), opcode))
        {
            throw new InvalidOperationException(
                $"Packet类型 {typeof(TPacket).Name} 已被注册为 Opcode {_serverPackets[typeof(TPacket)]:X4}");
        }
    }

    /// <summary>
    /// 根据Opcode获取客户端Packet类型
    /// </summary>
    public Type? GetClientPacketType(ushort opcode)
    {
        _clientPackets.TryGetValue(opcode, out var packetType);
        return packetType;
    }

    /// <summary>
    /// 根据类型获取服务器Packet的Opcode
    /// </summary>
    public ushort GetServerPacketOpcode(Type packetType)
    {
        if (_serverPackets.TryGetValue(packetType, out var opcode))
        {
            return opcode;
        }

        throw new InvalidOperationException($"未注册的Server Packet类型: {packetType.Name}");
    }

    /// <summary>
    /// 根据泛型类型获取服务器Packet的Opcode
    /// </summary>
    public ushort GetServerPacketOpcode<TPacket>() where TPacket : ServerPacket
    {
        return GetServerPacketOpcode(typeof(TPacket));
    }

    /// <summary>
    /// 检查客户端Opcode是否已注册
    /// </summary>
    public bool IsClientOpcodeRegistered(ushort opcode)
    {
        return _clientPackets.ContainsKey(opcode);
    }

    /// <summary>
    /// 检查服务器Packet类型是否已注册
    /// </summary>
    public bool IsServerPacketRegistered(Type packetType)
    {
        return _serverPackets.ContainsKey(packetType);
    }

    /// <summary>
    /// 获取所有已注册的客户端Opcode
    /// </summary>
    public IEnumerable<ushort> GetRegisteredClientOpcodes()
    {
        return _clientPackets.Keys;
    }

    /// <summary>
    /// 获取所有已注册的服务器Packet类型
    /// </summary>
    public IEnumerable<Type> GetRegisteredServerPacketTypes()
    {
        return _serverPackets.Keys;
    }

    /// <summary>
    /// 清空所有注册
    /// </summary>
    public void Clear()
    {
        _clientPackets.Clear();
        _serverPackets.Clear();
    }
}

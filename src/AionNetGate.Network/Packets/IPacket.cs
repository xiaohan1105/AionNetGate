namespace AionNetGate.Network.Packets;

/// <summary>
/// Packet 接口
/// </summary>
public interface IPacket
{
    /// <summary>
    /// Packet 操作码
    /// </summary>
    PacketOpcode Opcode { get; }

    /// <summary>
    /// Packet 方向
    /// </summary>
    PacketDirection Direction { get; }

    /// <summary>
    /// 是否需要加密
    /// </summary>
    bool IsEncrypted { get; }

    /// <summary>
    /// 序列化为字节数组
    /// </summary>
    Task<byte[]> SerializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 从字节数组反序列化
    /// </summary>
    Task DeserializeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);
}

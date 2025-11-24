namespace AionNetGate.Core.Network.Packets;

/// <summary>
/// Packet基础接口
/// 所有网络包都必须实现此接口
/// </summary>
public interface IPacket
{
    /// <summary>
    /// Packet的Opcode（操作码）
    /// 用于识别Packet类型
    /// </summary>
    ushort Opcode { get; }

    /// <summary>
    /// 序列化Packet到字节数组
    /// </summary>
    /// <param name="buffer">目标缓冲区</param>
    /// <returns>写入的字节数</returns>
    int Serialize(Span<byte> buffer);

    /// <summary>
    /// 从字节数组反序列化Packet
    /// </summary>
    /// <param name="buffer">源缓冲区</param>
    void Deserialize(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// 获取Packet的预估大小（用于优化内存分配）
    /// </summary>
    int GetEstimatedSize();
}

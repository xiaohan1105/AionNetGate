using AionNetGate.Network.Packets;

namespace AionNetGate.Network.Serialization;

/// <summary>
/// Packet 序列化器接口
/// </summary>
public interface IPacketSerializer
{
    /// <summary>
    /// 序列化 Packet 为字节数组
    /// </summary>
    /// <param name="packet">要序列化的 Packet</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完整的 Packet 字节数组（包括 header）</returns>
    Task<byte[]> SerializeAsync(IPacket packet, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从字节数组反序列化 Packet
    /// </summary>
    /// <param name="data">Packet 字节数组（包括 header）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>反序列化的 Packet</returns>
    Task<IPacket> DeserializeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 尝试从缓冲区读取完整的 Packet
    /// </summary>
    /// <param name="buffer">数据缓冲区</param>
    /// <param name="packet">读取到的 Packet（如果成功）</param>
    /// <param name="bytesConsumed">消耗的字节数</param>
    /// <returns>是否成功读取完整 Packet</returns>
    bool TryReadPacket(ReadOnlySpan<byte> buffer, out IPacket? packet, out int bytesConsumed);
}

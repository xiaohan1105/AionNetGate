using MessagePack;

namespace AionNetGate.Network.Packets;

/// <summary>
/// Packet 基类
/// </summary>
[MessagePackObject]
public abstract class PacketBase : IPacket
{
    /// <summary>
    /// Packet 操作码
    /// </summary>
    [IgnoreMember]
    public abstract PacketOpcode Opcode { get; }

    /// <summary>
    /// Packet 方向
    /// </summary>
    [IgnoreMember]
    public abstract PacketDirection Direction { get; }

    /// <summary>
    /// 是否需要加密（默认加密）
    /// </summary>
    [IgnoreMember]
    public virtual bool IsEncrypted => true;

    /// <summary>
    /// 序列化为字节数组
    /// </summary>
    public virtual Task<byte[]> SerializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            // 使用具体类型进行序列化，而不是抽象基类
            var payload = MessagePackSerializer.Serialize(GetType(), this, cancellationToken: cancellationToken);
            return payload;
        }, cancellationToken);
    }

    /// <summary>
    /// 从字节数组反序列化
    /// </summary>
    public virtual Task DeserializeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            // MessagePack 反序列化会自动填充对象属性
            // 子类需要重写此方法以处理特定的反序列化逻辑
            MessagePackSerializer.Deserialize(GetType(), data, cancellationToken: cancellationToken);
        }, cancellationToken);
    }
}

namespace AionNetGate.Network.Protocol;

/// <summary>
/// Legacy XOR 加密实现（包装静态 PacketEncryption）
/// 用于兼容老客户端
/// </summary>
[Obsolete("仅用于兼容老客户端，新客户端请使用 SecureEncryption")]
public sealed class LegacyEncryption : IPacketEncryption
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static readonly LegacyEncryption Instance = new();

    private LegacyEncryption() { }

    /// <inheritdoc/>
    public string ModeName => "Legacy-XOR";

    /// <inheritdoc/>
    public bool RequiresKeyExchange => false;

    /// <inheritdoc/>
    public void Encrypt(Span<byte> data)
    {
#pragma warning disable CS0618 // 忽略过时警告
        PacketEncryption.Encrypt(data);
#pragma warning restore CS0618
    }

    /// <inheritdoc/>
    public void Decrypt(Span<byte> data)
    {
#pragma warning disable CS0618
        PacketEncryption.Decrypt(data);
#pragma warning restore CS0618
    }

    /// <inheritdoc/>
    public byte[] EncryptToArray(ReadOnlySpan<byte> data)
    {
#pragma warning disable CS0618
        return PacketEncryption.EncryptToArray(data);
#pragma warning restore CS0618
    }

    /// <inheritdoc/>
    public byte[] DecryptToArray(ReadOnlySpan<byte> data)
    {
#pragma warning disable CS0618
        return PacketEncryption.DecryptToArray(data);
#pragma warning restore CS0618
    }
}

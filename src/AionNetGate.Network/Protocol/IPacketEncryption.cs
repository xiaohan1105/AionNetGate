namespace AionNetGate.Network.Protocol;

/// <summary>
/// 数据包加密接口
/// </summary>
public interface IPacketEncryption
{
    /// <summary>
    /// 加密模式名称
    /// </summary>
    string ModeName { get; }

    /// <summary>
    /// 是否需要密钥交换
    /// </summary>
    bool RequiresKeyExchange { get; }

    /// <summary>
    /// 加密数据（原地修改）
    /// </summary>
    /// <param name="data">要加密的数据</param>
    void Encrypt(Span<byte> data);

    /// <summary>
    /// 解密数据（原地修改）
    /// </summary>
    /// <param name="data">要解密的数据</param>
    void Decrypt(Span<byte> data);

    /// <summary>
    /// 加密数据并返回新数组
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <returns>加密后的数据</returns>
    byte[] EncryptToArray(ReadOnlySpan<byte> data);

    /// <summary>
    /// 解密数据并返回新数组
    /// </summary>
    /// <param name="data">加密数据</param>
    /// <returns>解密后的数据</returns>
    byte[] DecryptToArray(ReadOnlySpan<byte> data);
}

/// <summary>
/// 加密模式枚举
/// </summary>
public enum EncryptionMode
{
    /// <summary>
    /// Legacy XOR 加密（兼容老客户端）
    /// </summary>
    Legacy = 0,

    /// <summary>
    /// 混合模式：XOR + AES（过渡期）
    /// </summary>
    Hybrid = 1,

    /// <summary>
    /// 安全模式：ECDH + AES-256-GCM
    /// </summary>
    Secure = 2
}

using System.Security.Cryptography;

namespace AionNetGate.Network.Protocol;

/// <summary>
/// ECDH 密钥交换服务
/// 使用 NIST P-256 曲线进行安全密钥协商
/// </summary>
public sealed class KeyExchange : IDisposable
{
    /// <summary>
    /// 公钥大小（未压缩格式：04 + X + Y = 65 字节）
    /// </summary>
    public const int PublicKeySize = 65;

    /// <summary>
    /// 派生密钥大小（256 位）
    /// </summary>
    public const int DerivedKeySize = 32;

    private readonly ECDiffieHellman _ecdh;
    private bool _disposed;

    /// <summary>
    /// 创建新的密钥交换实例（生成新密钥对）
    /// </summary>
    public KeyExchange()
    {
        _ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
    }

    /// <summary>
    /// 获取本地公钥（用于发送给对方）
    /// </summary>
    public byte[] GetPublicKey()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _ecdh.PublicKey.ExportSubjectPublicKeyInfo();
    }

    /// <summary>
    /// 获取本地公钥（未压缩格式，兼容性更好）
    /// </summary>
    public byte[] GetPublicKeyUncompressed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var parameters = _ecdh.ExportParameters(false);
        var publicKey = new byte[PublicKeySize];

        // 未压缩格式: 04 || X || Y
        publicKey[0] = 0x04;
        parameters.Q.X!.CopyTo(publicKey, 1);
        parameters.Q.Y!.CopyTo(publicKey, 33);

        return publicKey;
    }

    /// <summary>
    /// 使用对方公钥派生共享密钥
    /// </summary>
    /// <param name="remotePublicKey">对方的公钥（SubjectPublicKeyInfo 格式）</param>
    /// <returns>派生的共享密钥（32 字节）</returns>
    public byte[] DeriveKey(byte[] remotePublicKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var remoteEcdh = ECDiffieHellman.Create();
        remoteEcdh.ImportSubjectPublicKeyInfo(remotePublicKey, out _);

        return _ecdh.DeriveKeyMaterial(remoteEcdh.PublicKey);
    }

    /// <summary>
    /// 使用对方公钥派生共享密钥（未压缩格式）
    /// </summary>
    /// <param name="remotePublicKeyUncompressed">对方的公钥（未压缩格式，65 字节）</param>
    /// <returns>派生的共享密钥（32 字节）</returns>
    public byte[] DeriveKeyFromUncompressed(byte[] remotePublicKeyUncompressed)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (remotePublicKeyUncompressed.Length != PublicKeySize || remotePublicKeyUncompressed[0] != 0x04)
        {
            throw new ArgumentException("无效的未压缩公钥格式", nameof(remotePublicKeyUncompressed));
        }

        // 解析未压缩格式
        var x = new byte[32];
        var y = new byte[32];
        Buffer.BlockCopy(remotePublicKeyUncompressed, 1, x, 0, 32);
        Buffer.BlockCopy(remotePublicKeyUncompressed, 33, y, 0, 32);

        var parameters = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = x, Y = y }
        };

        using var remoteEcdh = ECDiffieHellman.Create(parameters);
        return _ecdh.DeriveKeyMaterial(remoteEcdh.PublicKey);
    }

    /// <summary>
    /// 使用对方公钥派生共享密钥，并通过 HKDF 扩展
    /// </summary>
    /// <param name="remotePublicKey">对方的公钥</param>
    /// <param name="salt">盐值（可选）</param>
    /// <param name="info">上下文信息（可选）</param>
    /// <returns>派生的会话密钥（32 字节）</returns>
    public byte[] DeriveSessionKey(byte[] remotePublicKey, byte[]? salt = null, byte[]? info = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var sharedSecret = DeriveKey(remotePublicKey);

        // 使用 HKDF 派生最终会话密钥
        var sessionKey = new byte[DerivedKeySize];
        HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            sharedSecret,
            sessionKey,
            salt ?? Array.Empty<byte>(),
            info ?? "AionNetGate-SessionKey"u8.ToArray());

        // 清除共享密钥
        CryptographicOperations.ZeroMemory(sharedSecret);

        return sessionKey;
    }

    /// <summary>
    /// 执行完整的密钥交换流程（服务端）
    /// </summary>
    /// <param name="clientPublicKey">客户端公钥</param>
    /// <param name="serverPublicKey">输出：服务端公钥</param>
    /// <returns>派生的会话密钥</returns>
    public byte[] ServerKeyExchange(byte[] clientPublicKey, out byte[] serverPublicKey)
    {
        serverPublicKey = GetPublicKey();
        return DeriveSessionKey(clientPublicKey);
    }

    /// <summary>
    /// 执行完整的密钥交换流程（客户端）
    /// </summary>
    /// <param name="serverPublicKey">服务端公钥</param>
    /// <param name="clientPublicKey">输出：客户端公钥</param>
    /// <returns>派生的会话密钥</returns>
    public byte[] ClientKeyExchange(byte[] serverPublicKey, out byte[] clientPublicKey)
    {
        clientPublicKey = GetPublicKey();
        return DeriveSessionKey(serverPublicKey);
    }

    /// <summary>
    /// 创建密钥交换并导出公钥
    /// </summary>
    public static (KeyExchange Exchange, byte[] PublicKey) Create()
    {
        var exchange = new KeyExchange();
        return (exchange, exchange.GetPublicKey());
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _ecdh.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// 密钥交换结果
/// </summary>
public readonly struct KeyExchangeResult
{
    /// <summary>
    /// 会话密钥
    /// </summary>
    public required byte[] SessionKey { get; init; }

    /// <summary>
    /// 本地公钥（发送给对方）
    /// </summary>
    public required byte[] LocalPublicKey { get; init; }

    /// <summary>
    /// 创建的加密实例
    /// </summary>
    public SecureEncryption CreateEncryption() => new(SessionKey);
}

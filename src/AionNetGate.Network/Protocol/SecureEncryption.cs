using System.Buffers.Binary;
using System.Security.Cryptography;

namespace AionNetGate.Network.Protocol;

/// <summary>
/// 安全加密实现 - AES-256-GCM
/// </summary>
public sealed class SecureEncryption : IPacketEncryption, IDisposable
{
    /// <summary>
    /// AES 密钥大小（256 位）
    /// </summary>
    public const int KeySize = 32;

    /// <summary>
    /// GCM Nonce 大小（96 位）
    /// </summary>
    public const int NonceSize = 12;

    /// <summary>
    /// GCM Tag 大小（128 位）
    /// </summary>
    public const int TagSize = 16;

    /// <summary>
    /// 加密后数据的额外开销（Nonce + Tag）
    /// </summary>
    public const int Overhead = NonceSize + TagSize;

    private readonly byte[] _sessionKey;
    private long _encryptNonce;
    private long _decryptNonce;
    private bool _disposed;

    /// <inheritdoc/>
    public string ModeName => "AES-256-GCM";

    /// <inheritdoc/>
    public bool RequiresKeyExchange => true;

    /// <summary>
    /// 创建安全加密实例
    /// </summary>
    /// <param name="sessionKey">会话密钥（32 字节）</param>
    /// <exception cref="ArgumentException">密钥长度不正确</exception>
    public SecureEncryption(byte[] sessionKey)
    {
        if (sessionKey.Length != KeySize)
        {
            throw new ArgumentException($"密钥长度必须为 {KeySize} 字节", nameof(sessionKey));
        }

        _sessionKey = new byte[KeySize];
        sessionKey.CopyTo(_sessionKey, 0);
        _encryptNonce = 0;
        _decryptNonce = 0;
    }

    /// <summary>
    /// 创建安全加密实例（使用随机密钥）
    /// </summary>
    public SecureEncryption()
    {
        _sessionKey = new byte[KeySize];
        RandomNumberGenerator.Fill(_sessionKey);
        _encryptNonce = 0;
        _decryptNonce = 0;
    }

    /// <summary>
    /// 获取会话密钥（只读）
    /// </summary>
    public ReadOnlySpan<byte> SessionKey => _sessionKey;

    /// <inheritdoc/>
    public void Encrypt(Span<byte> data)
    {
        // 原地加密对于 GCM 模式不适用（需要额外空间存储 nonce 和 tag）
        // 此方法抛出异常，应使用 EncryptToArray
        throw new NotSupportedException("AES-GCM 不支持原地加密，请使用 EncryptToArray 方法");
    }

    /// <inheritdoc/>
    public void Decrypt(Span<byte> data)
    {
        // 原地解密对于 GCM 模式需要特殊处理
        throw new NotSupportedException("AES-GCM 不支持原地解密，请使用 DecryptToArray 方法");
    }

    /// <inheritdoc/>
    public byte[] EncryptToArray(ReadOnlySpan<byte> data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 结果格式: [Nonce 12字节][Ciphertext N字节][Tag 16字节]
        var result = new byte[NonceSize + data.Length + TagSize];
        var nonce = result.AsSpan(0, NonceSize);
        var ciphertext = result.AsSpan(NonceSize, data.Length);
        var tag = result.AsSpan(NonceSize + data.Length, TagSize);

        // 生成 nonce：使用递增计数器 + 随机填充
        var nonceValue = Interlocked.Increment(ref _encryptNonce);
        BinaryPrimitives.WriteInt64LittleEndian(nonce, nonceValue);
        RandomNumberGenerator.Fill(nonce.Slice(8, 4));

        // 执行加密
        using var aes = new AesGcm(_sessionKey, TagSize);
        aes.Encrypt(nonce, data, ciphertext, tag);

        return result;
    }

    /// <inheritdoc/>
    public byte[] DecryptToArray(ReadOnlySpan<byte> data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (data.Length < Overhead)
        {
            throw new ArgumentException($"数据太短，至少需要 {Overhead} 字节", nameof(data));
        }

        var nonce = data.Slice(0, NonceSize);
        var ciphertextLength = data.Length - Overhead;
        var ciphertext = data.Slice(NonceSize, ciphertextLength);
        var tag = data.Slice(NonceSize + ciphertextLength, TagSize);

        var plaintext = new byte[ciphertextLength];

        using var aes = new AesGcm(_sessionKey, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        Interlocked.Increment(ref _decryptNonce);

        return plaintext;
    }

    /// <summary>
    /// 尝试解密数据
    /// </summary>
    /// <param name="data">加密数据</param>
    /// <param name="plaintext">解密后的数据</param>
    /// <returns>是否成功</returns>
    public bool TryDecrypt(ReadOnlySpan<byte> data, out byte[]? plaintext)
    {
        try
        {
            plaintext = DecryptToArray(data);
            return true;
        }
        catch (AuthenticationTagMismatchException)
        {
            plaintext = null;
            return false;
        }
        catch (ArgumentException)
        {
            plaintext = null;
            return false;
        }
    }

    /// <summary>
    /// 生成新的随机会话密钥
    /// </summary>
    public static byte[] GenerateKey()
    {
        var key = new byte[KeySize];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CryptographicOperations.ZeroMemory(_sessionKey);
            _disposed = true;
        }
    }
}

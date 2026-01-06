using System.Security.Cryptography;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace AionNetGate.Infrastructure.Security;

/// <summary>
/// AES-256-GCM 加密服务实现
/// </summary>
public class AesGcmEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;  // 96 位 nonce (推荐大小)
    private const int TagSize = 16;    // 128 位认证标签
    private const int KeySize = 32;    // 256 位密钥

    private readonly byte[] _key;

    /// <summary>
    /// 构造函数 - 从配置读取密钥
    /// </summary>
    public AesGcmEncryptionService(IOptions<SecurityConfig> options)
    {
        var config = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // 从 Base64 配置读取密钥
        if (string.IsNullOrEmpty(config.EncryptionKey))
        {
            // 如果未配置，生成一个临时密钥（仅用于开发）
            _key = GenerateKey();
        }
        else
        {
            try
            {
                _key = Convert.FromBase64String(config.EncryptionKey);
            }
            catch
            {
                // 如果 Base64 解析失败，使用密钥字符串的 SHA256 哈希
                using var sha256 = SHA256.Create();
                _key = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(config.EncryptionKey));
            }
        }

        if (_key.Length != KeySize)
            throw new ArgumentException($"密钥必须是 {KeySize} 字节 (256 位)");
    }

    /// <summary>
    /// 构造函数 - 直接使用密钥
    /// </summary>
    /// <param name="key">256 位加密密钥 (32 字节)</param>
    public AesGcmEncryptionService(byte[] key)
    {
        if (key == null || key.Length != KeySize)
            throw new ArgumentException($"密钥必须是 {KeySize} 字节 (256 位)", nameof(key));

        _key = key;
    }

    /// <summary>
    /// 从 Base64 字符串创建加密服务
    /// </summary>
    public static AesGcmEncryptionService FromBase64Key(string base64Key)
    {
        var key = Convert.FromBase64String(base64Key);
        return new AesGcmEncryptionService(key);
    }

    /// <summary>
    /// 生成随机密钥
    /// </summary>
    public static byte[] GenerateKey()
    {
        return RandomNumberGenerator.GetBytes(KeySize);
    }

    /// <summary>
    /// 加密数据（异步）
    /// </summary>
    public async Task<byte[]> EncryptAsync(ReadOnlyMemory<byte> plaintext, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Encrypt(plaintext.Span), cancellationToken);
    }

    /// <summary>
    /// 解密数据（异步）
    /// </summary>
    public async Task<byte[]> DecryptAsync(ReadOnlyMemory<byte> ciphertext, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Decrypt(ciphertext.Span), cancellationToken);
    }

    /// <summary>
    /// 加密数据（同步）
    /// </summary>
    /// <remarks>
    /// 输出格式: [nonce(12)] [tag(16)] [ciphertext(N)]
    /// </remarks>
    public byte[] Encrypt(ReadOnlySpan<byte> plaintext)
    {
        // 生成随机 nonce
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);

        // 分配缓冲区
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        // 执行加密
        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        // 组合结果: [nonce] [tag] [ciphertext]
        var result = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

        return result;
    }

    /// <summary>
    /// 解密数据（同步）
    /// </summary>
    /// <remarks>
    /// 输入格式: [nonce(12)] [tag(16)] [ciphertext(N)]
    /// </remarks>
    public byte[] Decrypt(ReadOnlySpan<byte> ciphertext)
    {
        if (ciphertext.Length < NonceSize + TagSize)
            throw new ArgumentException("密文数据太短", nameof(ciphertext));

        // 提取组件
        var nonce = ciphertext.Slice(0, NonceSize);
        var tag = ciphertext.Slice(NonceSize, TagSize);
        var encryptedData = ciphertext.Slice(NonceSize + TagSize);

        // 分配明文缓冲区
        var plaintext = new byte[encryptedData.Length];

        // 执行解密
        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Decrypt(nonce, encryptedData, tag, plaintext);

        return plaintext;
    }
}

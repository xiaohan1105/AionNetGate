using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using NSec.Cryptography;

namespace AionGate.Core.Security;

/// <summary>
/// 加密服务 (使用现代加密库)
/// </summary>
public class CryptoService : ICryptoService
{
    private static readonly Algorithm AeadAlgorithm = AeadAlgorithm.Aes256Gcm;
    private static readonly KeyAgreementAlgorithm KeyAgreement = KeyAgreementAlgorithm.X25519;

    /// <summary>
    /// 生成密钥对 (ECDH X25519)
    /// </summary>
    public (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
    {
        using var key = Key.Create(KeyAgreement, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });

        var publicKey = key.Export(KeyBlobFormat.RawPublicKey);
        var privateKey = key.Export(KeyBlobFormat.RawPrivateKey);

        return (publicKey, privateKey);
    }

    /// <summary>
    /// 派生共享密钥
    /// </summary>
    public byte[] DeriveSharedSecret(byte[] privateKey, byte[] peerPublicKey)
    {
        using var ownKey = Key.Import(KeyAgreement, privateKey, KeyBlobFormat.RawPrivateKey);
        using var peerKey = PublicKey.Import(KeyAgreement, peerPublicKey, KeyBlobFormat.RawPublicKey);

        using var sharedSecret = KeyAgreement.Agree(ownKey, peerKey);

        // 使用 HKDF 派生 32 字节密钥
        return sharedSecret!.Export(SharedSecretBlobFormat.RawSharedSecret);
    }

    /// <summary>
    /// AES-256-GCM 加密
    /// </summary>
    public byte[] Encrypt(byte[] key, byte[] nonce, byte[] plaintext, byte[]? associatedData = null)
    {
        using var keyObj = Key.Import(AeadAlgorithm, key, KeyBlobFormat.RawSymmetricKey);

        var nonceObj = new Nonce(nonce, 0);
        var ciphertext = AeadAlgorithm.Encrypt(keyObj, nonceObj, associatedData, plaintext);

        return ciphertext;
    }

    /// <summary>
    /// AES-256-GCM 解密
    /// </summary>
    public byte[]? Decrypt(byte[] key, byte[] nonce, byte[] ciphertext, byte[]? associatedData = null)
    {
        try
        {
            using var keyObj = Key.Import(AeadAlgorithm, key, KeyBlobFormat.RawSymmetricKey);

            var nonceObj = new Nonce(nonce, 0);
            var plaintext = AeadAlgorithm.Decrypt(keyObj, nonceObj, associatedData, ciphertext);

            return plaintext;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 生成安全随机字节
    /// </summary>
    public byte[] GenerateRandomBytes(int length)
    {
        return RandomNumberGenerator.GetBytes(length);
    }

    /// <summary>
    /// Argon2id 密码哈希
    /// </summary>
    public async Task<string> HashPasswordAsync(string password, string? salt = null)
    {
        var saltBytes = salt != null
            ? Convert.FromBase64String(salt)
            : GenerateRandomBytes(16);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = saltBytes,
            DegreeOfParallelism = 8,
            MemorySize = 65536, // 64 MB
            Iterations = 4
        };

        var hash = await argon2.GetBytesAsync(32);

        // 格式: $argon2id$v=19$m=65536,t=4,p=8$salt$hash
        return $"$argon2id$v=19$m=65536,t=4,p=8${Convert.ToBase64String(saltBytes)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// 验证 Argon2id 密码
    /// </summary>
    public async Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        try
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 6 || parts[1] != "argon2id")
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[4]);
            var expectedHash = Convert.FromBase64String(parts[5]);

            // 解析参数
            var paramParts = parts[3].Split(',');
            var memory = int.Parse(paramParts[0].Split('=')[1]);
            var iterations = int.Parse(paramParts[1].Split('=')[1]);
            var parallelism = int.Parse(paramParts[2].Split('=')[1]);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = parallelism,
                MemorySize = memory,
                Iterations = iterations
            };

            var hash = await argon2.GetBytesAsync(32);
            return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 计算 SHA-256 哈希
    /// </summary>
    public byte[] ComputeSha256(byte[] data)
    {
        return SHA256.HashData(data);
    }

    /// <summary>
    /// 计算 HMAC-SHA256
    /// </summary>
    public byte[] ComputeHmacSha256(byte[] key, byte[] data)
    {
        return HMACSHA256.HashData(key, data);
    }
}

/// <summary>
/// 加密服务接口
/// </summary>
public interface ICryptoService
{
    (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair();
    byte[] DeriveSharedSecret(byte[] privateKey, byte[] peerPublicKey);
    byte[] Encrypt(byte[] key, byte[] nonce, byte[] plaintext, byte[]? associatedData = null);
    byte[]? Decrypt(byte[] key, byte[] nonce, byte[] ciphertext, byte[]? associatedData = null);
    byte[] GenerateRandomBytes(int length);
    Task<string> HashPasswordAsync(string password, string? salt = null);
    Task<bool> VerifyPasswordAsync(string password, string passwordHash);
    byte[] ComputeSha256(byte[] data);
    byte[] ComputeHmacSha256(byte[] key, byte[] data);
}

using System.Security.Cryptography;
using System.Text;
using AionNetGate.Core.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace AionNetGate.Infrastructure.Security;

/// <summary>
/// Argon2id 密码哈希实现
/// </summary>
public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;        // 128 位盐
    private const int HashSize = 32;        // 256 位哈希
    private const int Iterations = 4;       // 迭代次数
    private const int MemorySize = 65536;   // 64 MB 内存
    private const int Parallelism = 2;      // 并行度

    /// <summary>
    /// 哈希密码
    /// </summary>
    public (string passwordHash, string passwordSalt) HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // 生成随机盐
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // 配置 Argon2
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = Iterations,
            MemoryCost = MemorySize,
            Lanes = Parallelism,
            Threads = Parallelism,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt,
            HashLength = HashSize
        };

        // 生成哈希
        using var argon2 = new Argon2(config);
        using var hash = argon2.Hash();

        return (
            passwordHash: Convert.ToBase64String(hash.Buffer),
            passwordSalt: Convert.ToBase64String(salt)
        );
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash));
        if (string.IsNullOrEmpty(passwordSalt))
            throw new ArgumentNullException(nameof(passwordSalt));

        try
        {
            var salt = Convert.FromBase64String(passwordSalt);
            var expectedHash = Convert.FromBase64String(passwordHash);

            // 配置 Argon2
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing, // Argon2id
                Version = Argon2Version.Nineteen,
                TimeCost = Iterations,
                MemoryCost = MemorySize,
                Lanes = Parallelism,
                Threads = Parallelism,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = salt,
                HashLength = HashSize
            };

            // 生成哈希并比较
            using var argon2 = new Argon2(config);
            using var hash = argon2.Hash();

            return CryptographicOperations.FixedTimeEquals(hash.Buffer, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

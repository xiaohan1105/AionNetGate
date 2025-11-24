using System.Security.Cryptography;
using System.Text;

namespace AionNetGate.Core.Services;

/// <summary>
/// 密码哈希服务实现
/// 支持 SHA1（兼容旧系统）和 PBKDF2（新系统）
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int PBKDF2_ITERATIONS = 10000;
    private const int PBKDF2_SALT_SIZE = 32;
    private const int PBKDF2_HASH_SIZE = 32;
    private const string PBKDF2_PREFIX = "$pbkdf2$";

    /// <summary>
    /// 使用 SHA1 哈希密码（兼容旧系统）
    /// </summary>
    public string HashWithSHA1(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("密码不能为空", nameof(password));

        using var sha1 = SHA1.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha1.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 使用 PBKDF2 哈希密码（推荐用于新系统）
    /// </summary>
    public string HashWithPBKDF2(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("密码不能为空", nameof(password));

        // 生成随机盐值
        var salt = new byte[PBKDF2_SALT_SIZE];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // 使用 PBKDF2 生成哈希
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            PBKDF2_ITERATIONS,
            HashAlgorithmName.SHA256);

        var hash = pbkdf2.GetBytes(PBKDF2_HASH_SIZE);

        // 格式: $pbkdf2${iterations}${salt_base64}${hash_base64}
        return $"{PBKDF2_PREFIX}{PBKDF2_ITERATIONS}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// 验证密码（自动检测哈希类型）
    /// </summary>
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("密码不能为空", nameof(password));

        if (string.IsNullOrEmpty(hash))
            return false;

        // 检测哈希类型
        if (hash.StartsWith(PBKDF2_PREFIX))
        {
            return VerifyPBKDF2(password, hash);
        }
        else
        {
            // 假设是 SHA1（40个字符的十六进制字符串）
            return VerifySHA1(password, hash);
        }
    }

    /// <summary>
    /// 判断哈希值是否需要重新哈希（用于从旧算法迁移到新算法）
    /// </summary>
    public bool NeedsRehash(string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return true;

        // 如果不是 PBKDF2 格式，则需要重新哈希
        if (!hash.StartsWith(PBKDF2_PREFIX))
            return true;

        // 解析迭代次数
        try
        {
            var parts = hash.Split('$');
            if (parts.Length < 4)
                return true;

            var iterations = int.Parse(parts[2]);
            // 如果迭代次数低于当前标准，需要重新哈希
            return iterations < PBKDF2_ITERATIONS;
        }
        catch
        {
            return true;
        }
    }

    #region 私有方法

    private bool VerifySHA1(string password, string hash)
    {
        var computedHash = HashWithSHA1(password);
        return string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);
    }

    private bool VerifyPBKDF2(string password, string hash)
    {
        try
        {
            // 解析哈希字符串
            var parts = hash.Split('$');
            if (parts.Length != 5 || parts[1] != "pbkdf2")
                return false;

            var iterations = int.Parse(parts[2]);
            var salt = Convert.FromBase64String(parts[3]);
            var storedHash = Convert.FromBase64String(parts[4]);

            // 使用相同的盐值和迭代次数计算哈希
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var computedHash = pbkdf2.GetBytes(PBKDF2_HASH_SIZE);

            // 比较哈希值（使用恒定时间比较防止时序攻击）
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    #endregion
}

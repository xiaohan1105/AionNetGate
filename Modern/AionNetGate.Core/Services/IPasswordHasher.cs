namespace AionNetGate.Core.Services;

/// <summary>
/// 密码哈希服务接口
/// 支持多种哈希算法（SHA1用于兼容旧系统，BCrypt/PBKDF2用于新系统）
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 使用SHA1哈希密码（兼容旧系统）
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>SHA1哈希值（十六进制字符串）</returns>
    string HashWithSHA1(string password);

    /// <summary>
    /// 使用PBKDF2哈希密码（推荐用于新系统）
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>PBKDF2哈希值（包含盐值）</returns>
    string HashWithPBKDF2(string password);

    /// <summary>
    /// 验证密码（自动检测哈希类型）
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hash">存储的哈希值</param>
    /// <returns>是否匹配</returns>
    bool Verify(string password, string hash);

    /// <summary>
    /// 判断哈希值是否需要重新哈希（用于从旧算法迁移到新算法）
    /// </summary>
    /// <param name="hash">存储的哈希值</param>
    /// <returns>是否需要重新哈希</returns>
    bool NeedsRehash(string hash);
}

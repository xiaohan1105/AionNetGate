namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 密码哈希服务接口
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 哈希密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>元组 (passwordHash, passwordSalt)</returns>
    (string passwordHash, string passwordSalt) HashPassword(string password);

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="passwordHash">存储的密码哈希</param>
    /// <param name="passwordSalt">存储的密码盐</param>
    /// <returns>密码是否匹配</returns>
    bool VerifyPassword(string password, string passwordHash, string passwordSalt);
}

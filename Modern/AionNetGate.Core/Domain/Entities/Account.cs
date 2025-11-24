namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 账号实体 - 领域模型
/// </summary>
public class Account
{
    /// <summary>
    /// 账号ID（主键）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 账号名称（唯一）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希（使用 SHA1/BCrypt）
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 硬件ID（用于硬件绑定）
    /// </summary>
    public string? HardwareId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 最后登出时间
    /// </summary>
    public DateTime? LastLogoutAt { get; set; }

    /// <summary>
    /// 账号是否激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 最后登录IP
    /// </summary>
    public string? LastIpAddress { get; set; }

    /// <summary>
    /// 最后登录MAC地址
    /// </summary>
    public string? LastMacAddress { get; set; }

    /// <summary>
    /// 备注信息
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 判断账号当前是否在线
    /// </summary>
    public bool IsOnline()
    {
        if (LastLoginAt == null || LastLogoutAt == null)
            return false;

        return LastLoginAt > LastLogoutAt;
    }

    /// <summary>
    /// 验证账号名称格式
    /// </summary>
    public static bool IsValidAccountName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (name.Length < 4 || name.Length > 50)
            return false;

        // 只允许字母、数字和下划线
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$");
    }

    /// <summary>
    /// 验证邮箱格式
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true; // 邮箱可选

        return System.Text.RegularExpressions.Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
        );
    }
}

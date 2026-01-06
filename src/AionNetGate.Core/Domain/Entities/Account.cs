namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 账号实体
/// </summary>
public class Account
{
    /// <summary>
    /// 账号 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 密码哈希 (Argon2id)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 密码盐
    /// </summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>
    /// 账号状态 (0=禁用, 1=正常, 2=锁定)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 角色 (0=普通用户, 1=VIP, 10=GM, 99=管理员)
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// 登录失败次数
    /// </summary>
    public int LoginAttempts { get; set; }

    /// <summary>
    /// 锁定截止时间
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 最后登录 IP
    /// </summary>
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 导航属性：会话列表
    /// </summary>
    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    /// <summary>
    /// 导航属性：硬件指纹列表
    /// </summary>
    public ICollection<HardwareFingerprint> HardwareFingerprints { get; set; } = new List<HardwareFingerprint>();
}

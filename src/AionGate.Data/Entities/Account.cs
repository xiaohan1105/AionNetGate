using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AionGate.Data.Entities;

/// <summary>
/// 账号实体
/// </summary>
[Table("accounts")]
public class Account : ITimestampedEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    [Column("password_salt")]
    public string PasswordSalt { get; set; } = string.Empty;

    [Column("status")]
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    [Column("role")]
    public UserRole Role { get; set; } = UserRole.Player;

    [Column("login_attempts")]
    public int LoginAttempts { get; set; }

    [Column("locked_until")]
    public DateTime? LockedUntil { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [MaxLength(45)]
    [Column("last_login_ip")]
    public string? LastLoginIp { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // 导航属性
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<HardwareFingerprint> HardwareFingerprints { get; set; } = new List<HardwareFingerprint>();

    /// <summary>
    /// 账号是否被锁定
    /// </summary>
    public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// 账号是否可用
    /// </summary>
    public bool IsActive => Status == AccountStatus.Active && !IsLocked;
}

/// <summary>
/// 账号状态
/// </summary>
public enum AccountStatus : byte
{
    /// <summary>
    /// 禁用
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// 正常
    /// </summary>
    Active = 1,

    /// <summary>
    /// 锁定
    /// </summary>
    Locked = 2,

    /// <summary>
    /// 已删除
    /// </summary>
    Deleted = 99
}

/// <summary>
/// 用户角色
/// </summary>
public enum UserRole : byte
{
    /// <summary>
    /// 玩家
    /// </summary>
    Player = 0,

    /// <summary>
    /// GM
    /// </summary>
    GameMaster = 1,

    /// <summary>
    /// 管理员
    /// </summary>
    Admin = 2
}

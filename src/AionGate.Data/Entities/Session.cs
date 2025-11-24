using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AionGate.Data.Entities;

/// <summary>
/// 会话实体
/// </summary>
[Table("sessions")]
public class Session
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("account_id")]
    public long AccountId { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [MaxLength(64)]
    [Column("refresh_token_hash")]
    public string? RefreshTokenHash { get; set; }

    [Required]
    [MaxLength(45)]
    [Column("client_ip")]
    public string ClientIp { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("client_info")]
    public string? ClientInfo { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    // 导航属性
    [ForeignKey(nameof(AccountId))]
    public virtual Account? Account { get; set; }

    /// <summary>
    /// 会话是否有效
    /// </summary>
    public bool IsValid => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}

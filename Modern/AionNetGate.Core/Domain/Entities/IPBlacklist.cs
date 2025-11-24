namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// IP黑名单实体
/// </summary>
public class IPBlacklist
{
    /// <summary>
    /// 记录ID（主键）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 被封禁的IP地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 封禁原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 封禁时间
    /// </summary>
    public DateTime BlockedAt { get; set; }

    /// <summary>
    /// 过期时间（null表示永久封禁）
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 封禁操作员（管理员名称或系统）
    /// </summary>
    public string? BlockedBy { get; set; }

    /// <summary>
    /// 是否激活（允许手动解封）
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 判断封禁是否已过期
    /// </summary>
    public bool IsExpired()
    {
        if (!IsActive)
            return true;

        if (ExpiresAt == null)
            return false; // 永久封禁

        return DateTime.UtcNow > ExpiresAt.Value;
    }

    /// <summary>
    /// 获取剩余封禁时间（秒）
    /// </summary>
    public long? GetRemainingSeconds()
    {
        if (ExpiresAt == null)
            return null; // 永久封禁

        var remaining = (ExpiresAt.Value - DateTime.UtcNow).TotalSeconds;
        return remaining > 0 ? (long)remaining : 0;
    }
}

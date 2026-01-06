namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// IP 黑名单实体
/// </summary>
public class IpBlacklist
{
    /// <summary>
    /// 黑名单 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// IP 地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 封禁原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 是否永久封禁
    /// </summary>
    public bool IsPermanent { get; set; }

    /// <summary>
    /// 过期时间（临时封禁）
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

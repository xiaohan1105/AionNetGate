namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 会话实体
/// </summary>
public class Session
{
    /// <summary>
    /// 会话 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号 ID
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// 访问令牌 (JWT)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 客户端 IP
    /// </summary>
    public string ClientIp { get; set; } = string.Empty;

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 刷新令牌过期时间
    /// </summary>
    public DateTime RefreshExpiresAt { get; set; }

    /// <summary>
    /// 撤销时间
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 导航属性：账号
    /// </summary>
    public Account Account { get; set; } = null!;
}

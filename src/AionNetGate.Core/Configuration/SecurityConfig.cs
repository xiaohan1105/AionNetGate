namespace AionNetGate.Core.Configuration;

/// <summary>
/// 安全配置
/// </summary>
public class SecurityConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Security";

    /// <summary>
    /// AES-256 加密密钥（Base64）
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT 密钥
    /// </summary>
    public string JwtSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT 发行者
    /// </summary>
    public string JwtIssuer { get; set; } = "AionNetGate";

    /// <summary>
    /// JWT 受众
    /// </summary>
    public string JwtAudience { get; set; } = "AionClients";

    /// <summary>
    /// Access Token 过期时间（分钟）
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh Token 过期时间（天）
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// 最大登录失败次数
    /// </summary>
    public int MaxLoginAttempts { get; set; } = 5;

    /// <summary>
    /// 账号锁定时长（分钟）
    /// </summary>
    public int AccountLockoutMinutes { get; set; } = 30;

    /// <summary>
    /// 是否启用硬件指纹验证
    /// </summary>
    public bool EnableHardwareFingerprint { get; set; } = true;

    /// <summary>
    /// 是否启用 IP 黑名单
    /// </summary>
    public bool EnableIpBlacklist { get; set; } = true;
}

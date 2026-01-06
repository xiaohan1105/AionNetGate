namespace AionNetGate.Infrastructure.Caching;

/// <summary>
/// 缓存键常量
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// 账号缓存前缀
    /// </summary>
    public const string AccountPrefix = "account:";

    /// <summary>
    /// 会话缓存前缀
    /// </summary>
    public const string SessionPrefix = "session:";

    /// <summary>
    /// IP黑名单前缀
    /// </summary>
    public const string IpBlacklistPrefix = "ipblacklist:";

    /// <summary>
    /// 硬件指纹前缀
    /// </summary>
    public const string HardwareFingerprintPrefix = "hwid:";

    /// <summary>
    /// 登录尝试计数前缀
    /// </summary>
    public const string LoginAttemptPrefix = "loginattempt:";

    /// <summary>
    /// 获取账号缓存键 (按ID)
    /// </summary>
    public static string AccountById(long id) => $"{AccountPrefix}id:{id}";

    /// <summary>
    /// 获取账号缓存键 (按用户名)
    /// </summary>
    public static string AccountByUsername(string username) => $"{AccountPrefix}username:{username.ToLowerInvariant()}";

    /// <summary>
    /// 获取会话缓存键
    /// </summary>
    public static string Session(string token) => $"{SessionPrefix}{token}";

    /// <summary>
    /// 获取IP黑名单缓存键
    /// </summary>
    public static string IpBlacklist(string ip) => $"{IpBlacklistPrefix}{ip}";

    /// <summary>
    /// 获取所有IP黑名单缓存键
    /// </summary>
    public static string IpBlacklistAll => $"{IpBlacklistPrefix}all";

    /// <summary>
    /// 获取硬件指纹缓存键
    /// </summary>
    public static string HardwareFingerprint(long accountId, string hardwareId)
        => $"{HardwareFingerprintPrefix}{accountId}:{hardwareId}";

    /// <summary>
    /// 获取登录尝试计数键
    /// </summary>
    public static string LoginAttempts(string identifier) => $"{LoginAttemptPrefix}{identifier}";

    /// <summary>
    /// 缓存过期时间配置
    /// </summary>
    public static class Expiration
    {
        public static readonly TimeSpan Account = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan Session = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan IpBlacklist = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan HardwareFingerprint = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan LoginAttempts = TimeSpan.FromMinutes(5);
    }
}

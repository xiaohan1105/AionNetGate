using AionNetGate.Core.Results;

namespace AionNetGate.Core.Services;

/// <summary>
/// 防火墙服务接口
/// </summary>
public interface IFirewallService
{
    /// <summary>
    /// 将IP添加到白名单
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="reason">原因</param>
    /// <param name="expirationHours">过期时间（小时），0表示永不过期</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> AddToWhitelistAsync(string ipAddress, string reason = "玩家连接", int expirationHours = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从白名单移除IP
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> RemoveFromWhitelistAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将IP添加到黑名单
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="reason">原因</param>
    /// <param name="expirationHours">过期时间（小时），0表示永久封禁</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> AddToBlacklistAsync(string ipAddress, string reason = "攻击行为", int expirationHours = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从黑名单移除IP
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> RemoveFromBlacklistAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空黑名单
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> ClearBlacklistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空白名单
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> ClearWhitelistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查IP是否在白名单中
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> IsWhitelistedAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查IP是否在黑名单中
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> IsBlacklistedAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取白名单列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<FirewallEntry>> GetWhitelistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取黑名单列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<FirewallEntry>> GetBlacklistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 记录连接尝试并检测攻击
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否检测到攻击行为</returns>
    Task<bool> RecordConnectionAttemptAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查防火墙服务是否启用
    /// </summary>
    bool IsEnabled { get; }
}

/// <summary>
/// 防火墙条目
/// </summary>
public record FirewallEntry
{
    /// <summary>
    /// IP地址
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime AddedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 过期时间（null表示永不过期）
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}

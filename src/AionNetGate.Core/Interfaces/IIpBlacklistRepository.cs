using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// IP 黑名单仓储接口
/// </summary>
public interface IIpBlacklistRepository : IRepository<IpBlacklist>
{
    /// <summary>
    /// 根据 IP 地址获取黑名单记录
    /// </summary>
    Task<IpBlacklist?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查 IP 是否在黑名单中
    /// </summary>
    Task<bool> IsIpBlacklistedAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加 IP 到黑名单
    /// </summary>
    Task AddToBlacklistAsync(string ipAddress, string reason, bool isPermanent, TimeSpan? duration, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从黑名单移除 IP
    /// </summary>
    Task RemoveFromBlacklistAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理过期的临时黑名单记录
    /// </summary>
    Task CleanupExpiredEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有永久封禁的 IP
    /// </summary>
    Task<IEnumerable<IpBlacklist>> GetPermanentBlacklistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前有效的黑名单数量
    /// </summary>
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}

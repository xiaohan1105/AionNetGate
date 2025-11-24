using AionNetGate.Core.Data;
using AionNetGate.Core.Domain.Entities;
using System.Collections.Concurrent;

namespace AionNetGate.Core.Services;

/// <summary>
/// IP黑名单检查服务实现
/// 使用内存缓存提升性能
/// </summary>
public class IPBlacklistChecker : IIPBlacklistChecker
{
    private readonly IIPBlacklistRepository _repository;
    private readonly ConcurrentDictionary<string, IPBlacklist> _cache;
    private DateTime _lastReloadTime;
    private readonly object _reloadLock = new object();

    public IPBlacklistChecker(IIPBlacklistRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = new ConcurrentDictionary<string, IPBlacklist>();
        _lastReloadTime = DateTime.MinValue;
    }

    /// <summary>
    /// 检查IP是否被封禁
    /// </summary>
    public async Task<bool> IsBlacklistedAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        await EnsureCacheLoadedAsync();

        if (_cache.TryGetValue(ipAddress, out var blacklist))
        {
            return !blacklist.IsExpired();
        }

        return false;
    }

    /// <summary>
    /// 获取封禁原因
    /// </summary>
    public async Task<string?> GetBlockReasonAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return null;

        await EnsureCacheLoadedAsync();

        if (_cache.TryGetValue(ipAddress, out var blacklist))
        {
            if (!blacklist.IsExpired())
            {
                return blacklist.Reason;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取剩余封禁时间（秒）
    /// </summary>
    public async Task<long?> GetRemainingTimeAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return 0;

        await EnsureCacheLoadedAsync();

        if (_cache.TryGetValue(ipAddress, out var blacklist))
        {
            if (!blacklist.IsExpired())
            {
                return blacklist.GetRemainingSeconds();
            }
        }

        return 0;
    }

    /// <summary>
    /// 重新加载黑名单（从数据库）
    /// </summary>
    public async Task ReloadAsync()
    {
        var activeBlacklists = await _repository.GetActiveBlacklistAsync();

        _cache.Clear();

        foreach (var blacklist in activeBlacklists)
        {
            if (!blacklist.IsExpired())
            {
                _cache[blacklist.IpAddress] = blacklist;
            }
        }

        _lastReloadTime = DateTime.UtcNow;
    }

    #region 私有方法

    /// <summary>
    /// 确保缓存已加载（首次访问或超过5分钟自动重新加载）
    /// </summary>
    private Task EnsureCacheLoadedAsync()
    {
        // 如果缓存为空或超过5分钟未更新，则重新加载
        if (_cache.IsEmpty || (DateTime.UtcNow - _lastReloadTime).TotalMinutes > 5)
        {
            // 使用锁防止并发重新加载
            lock (_reloadLock)
            {
                // 双重检查
                if (_cache.IsEmpty || (DateTime.UtcNow - _lastReloadTime).TotalMinutes > 5)
                {
                    return ReloadAsync();
                }
            }
        }

        return Task.CompletedTask;
    }

    #endregion
}

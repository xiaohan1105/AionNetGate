using AionNetGate.Core.Domain.Entities;
using AionNetGate.Core.Interfaces;
using AionNetGate.Core.Results;
using AionNetGate.Core.Services;
using AionNetGate.Infrastructure.Caching;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Application.Services;

/// <summary>
/// IP黑名单服务 - 带缓存优化
/// </summary>
public interface IIpBlacklistService
{
    /// <summary>
    /// 检查IP是否被禁止
    /// </summary>
    Task<bool> IsBlockedAsync(string ipAddress, CancellationToken ct = default);

    /// <summary>
    /// 添加IP到黑名单
    /// </summary>
    Task<Result> BlockIpAsync(string ipAddress, string? reason, TimeSpan? duration, CancellationToken ct = default);

    /// <summary>
    /// 从黑名单移除IP
    /// </summary>
    Task<Result> UnblockIpAsync(string ipAddress, CancellationToken ct = default);

    /// <summary>
    /// 获取所有黑名单IP
    /// </summary>
    Task<IEnumerable<IpBlacklist>> GetAllBlockedAsync(CancellationToken ct = default);

    /// <summary>
    /// 刷新缓存
    /// </summary>
    Task RefreshCacheAsync(CancellationToken ct = default);
}

/// <summary>
/// IP黑名单服务实现
/// </summary>
public class IpBlacklistService : IIpBlacklistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<IpBlacklistService> _logger;

    public IpBlacklistService(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<IpBlacklistService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsBlockedAsync(string ipAddress, CancellationToken ct = default)
    {
        // 先从缓存检查
        var cacheKey = CacheKeys.IpBlacklist(ipAddress);
        var cached = _cache.Get<IpBlacklist>(cacheKey);

        if (cached != null)
        {
            // 检查是否已过期
            if (cached.ExpiresAt.HasValue && cached.ExpiresAt.Value < DateTime.UtcNow)
            {
                _cache.Remove(cacheKey);
                return false;
            }
            return true;
        }

        // 缓存未命中，查询数据库
        var blacklist = await _unitOfWork.IpBlacklists.GetByIpAddressAsync(ipAddress, ct);
        if (blacklist == null)
            return false;

        // 检查是否已过期
        if (blacklist.ExpiresAt.HasValue && blacklist.ExpiresAt.Value < DateTime.UtcNow)
        {
            // 异步删除过期记录
            _ = Task.Run(async () =>
            {
                await _unitOfWork.IpBlacklists.DeleteAsync(blacklist, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }, ct);
            return false;
        }

        // 更新缓存
        _cache.Set(cacheKey, blacklist, CacheKeys.Expiration.IpBlacklist);

        return true;
    }

    public async Task<Result> BlockIpAsync(string ipAddress, string? reason, TimeSpan? duration, CancellationToken ct = default)
    {
        try
        {
            var existing = await _unitOfWork.IpBlacklists.GetByIpAddressAsync(ipAddress, ct);
            if (existing != null)
            {
                // 更新现有记录
                existing.Reason = reason ?? string.Empty;
                existing.ExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null;
                existing.IsPermanent = !duration.HasValue;
                existing.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.IpBlacklists.UpdateAsync(existing, ct);
            }
            else
            {
                // 创建新记录
                var blacklist = new IpBlacklist
                {
                    IpAddress = ipAddress,
                    Reason = reason ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null,
                    IsPermanent = !duration.HasValue
                };

                await _unitOfWork.IpBlacklists.AddAsync(blacklist, ct);
                existing = blacklist;
            }

            await _unitOfWork.SaveChangesAsync(ct);

            // 更新缓存
            _cache.Set(CacheKeys.IpBlacklist(ipAddress), existing, CacheKeys.Expiration.IpBlacklist);
            _cache.Remove(CacheKeys.IpBlacklistAll);

            _logger.LogInformation("IP已加入黑名单: {IP}, 原因: {Reason}, 时长: {Duration}",
                ipAddress, reason, duration?.ToString() ?? "永久");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加IP黑名单失败: {IP}", ipAddress);
            return Result.Failure(Error.Internal("添加IP黑名单失败"));
        }
    }

    public async Task<Result> UnblockIpAsync(string ipAddress, CancellationToken ct = default)
    {
        try
        {
            var blacklist = await _unitOfWork.IpBlacklists.GetByIpAddressAsync(ipAddress, ct);
            if (blacklist == null)
                return Result.Failure(Error.NotFound("IP不在黑名单中"));

            await _unitOfWork.IpBlacklists.DeleteAsync(blacklist, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 清除缓存
            _cache.Remove(CacheKeys.IpBlacklist(ipAddress));
            _cache.Remove(CacheKeys.IpBlacklistAll);

            _logger.LogInformation("IP已从黑名单移除: {IP}", ipAddress);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除IP黑名单失败: {IP}", ipAddress);
            return Result.Failure(Error.Internal("移除IP黑名单失败"));
        }
    }

    public async Task<IEnumerable<IpBlacklist>> GetAllBlockedAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.IpBlacklistAll,
            async () => (await _unitOfWork.IpBlacklists.GetPermanentBlacklistAsync(ct)).ToList(),
            CacheKeys.Expiration.IpBlacklist,
            ct) ?? Enumerable.Empty<IpBlacklist>();
    }

    public Task RefreshCacheAsync(CancellationToken ct = default)
    {
        _cache.RemoveByPrefix(CacheKeys.IpBlacklistPrefix);
        _logger.LogInformation("IP黑名单缓存已刷新");
        return Task.CompletedTask;
    }
}

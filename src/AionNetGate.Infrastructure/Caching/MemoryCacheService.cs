using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Infrastructure.Caching;

/// <summary>
/// 基于 MemoryCache 的缓存服务实现
/// </summary>
public class MemoryCacheService : ICacheService, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys;
    private readonly SemaphoreSlim _lock;

    private long _hits;
    private long _misses;
    private bool _disposed;

    /// <summary>
    /// 默认过期时间
    /// </summary>
    public static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keys = new ConcurrentDictionary<string, byte>();
        _lock = new SemaphoreSlim(1, 1);
    }

    public T? Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            Interlocked.Increment(ref _hits);
            _logger.LogDebug("缓存命中: {Key}", key);
            return value;
        }

        Interlocked.Increment(ref _misses);
        _logger.LogDebug("缓存未命中: {Key}", key);
        return null;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        return Task.FromResult(Get<T>(key));
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default) where T : class
    {
        if (_cache.TryGetValue(key, out T? cached) && cached != null)
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        await _lock.WaitAsync(ct);
        try
        {
            // 双重检查
            if (_cache.TryGetValue(key, out cached) && cached != null)
            {
                Interlocked.Increment(ref _hits);
                return cached;
            }

            Interlocked.Increment(ref _misses);

            // 创建新值
            var value = await factory();
            if (value != null)
            {
                Set(key, value, expiration);
            }

            return value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
            Priority = CacheItemPriority.Normal
        };

        // 注册回调以跟踪键的移除
        options.RegisterPostEvictionCallback((k, v, reason, state) =>
        {
            _keys.TryRemove(k.ToString()!, out _);
            _logger.LogDebug("缓存项已移除: {Key}, 原因: {Reason}", k, reason);
        });

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);

        _logger.LogDebug("缓存已设置: {Key}, 过期时间: {Expiration}", key, expiration ?? DefaultExpiration);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class
    {
        Set(key, value, expiration);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("缓存已移除: {Key}", key);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }

        _logger.LogDebug("按前缀移除缓存: {Prefix}, 移除数量: {Count}", prefix, keysToRemove.Count);
    }

    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }

    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            TotalItems = _keys.Count,
            TotalHits = Interlocked.Read(ref _hits),
            TotalMisses = Interlocked.Read(ref _misses),
            MemoryUsageBytes = GC.GetTotalMemory(false)
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lock.Dispose();
    }
}

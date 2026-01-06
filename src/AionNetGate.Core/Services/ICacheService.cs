namespace AionNetGate.Core.Services;

/// <summary>
/// 缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存项
    /// </summary>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// 异步获取缓存项
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 获取或创建缓存项
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 设置缓存项
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// 异步设置缓存项
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 移除缓存项
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// 异步移除缓存项
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// 按模式移除缓存项
    /// </summary>
    void RemoveByPrefix(string prefix);

    /// <summary>
    /// 检查缓存项是否存在
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    CacheStatistics GetStatistics();
}

/// <summary>
/// 缓存统计信息
/// </summary>
public record CacheStatistics
{
    public long TotalItems { get; init; }
    public long TotalHits { get; init; }
    public long TotalMisses { get; init; }
    public double HitRatio => TotalHits + TotalMisses > 0
        ? (double)TotalHits / (TotalHits + TotalMisses) * 100
        : 0;
    public long MemoryUsageBytes { get; init; }
}

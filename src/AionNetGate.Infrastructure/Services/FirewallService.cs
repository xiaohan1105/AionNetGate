using System.Collections.Concurrent;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Results;
using AionNetGate.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Infrastructure.Services;

/// <summary>
/// 防火墙服务实现
/// </summary>
public class FirewallService : IFirewallService
{
    private readonly FirewallConfig _config;
    private readonly ILogger<FirewallService> _logger;

    // 内存存储（生产环境可替换为数据库）
    private readonly ConcurrentDictionary<string, FirewallEntry> _whitelist = new();
    private readonly ConcurrentDictionary<string, FirewallEntry> _blacklist = new();

    // 攻击检测
    private readonly ConcurrentDictionary<string, ConnectionAttemptTracker> _connectionAttempts = new();

    public FirewallService(IOptions<FirewallConfig> config, ILogger<FirewallService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public bool IsEnabled => _config.Enabled;

    public Task<Result> AddToWhitelistAsync(string ipAddress, string reason = "玩家连接", int expirationHours = 0, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(Result.Failure(Error.ServiceUnavailable("防火墙服务未启用")));
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return Task.FromResult(Result.Failure(Error.Validation("IP地址不能为空")));
        }

        var effectiveExpiration = expirationHours > 0 ? expirationHours : _config.WhitelistExpirationHours;
        var entry = new FirewallEntry
        {
            IpAddress = ipAddress,
            Reason = reason,
            AddedAt = DateTime.UtcNow,
            ExpiresAt = effectiveExpiration > 0 ? DateTime.UtcNow.AddHours(effectiveExpiration) : null
        };

        _whitelist.AddOrUpdate(ipAddress, entry, (_, _) => entry);

        _logger.LogInformation("IP已添加到白名单: {IpAddress}, 原因: {Reason}", ipAddress, reason);

        // 如果启用了Windows防火墙集成，这里可以调用netsh或Windows防火墙API
        // AddWindowsFirewallRule(ipAddress, true);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> RemoveFromWhitelistAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(Result.Failure(Error.ServiceUnavailable("防火墙服务未启用")));
        }

        if (_whitelist.TryRemove(ipAddress, out _))
        {
            _logger.LogInformation("IP已从白名单移除: {IpAddress}", ipAddress);
            // RemoveWindowsFirewallRule(ipAddress, true);
            return Task.FromResult(Result.Success());
        }

        return Task.FromResult(Result.Failure(Error.NotFound("IP不在白名单中")));
    }

    public Task<Result> AddToBlacklistAsync(string ipAddress, string reason = "攻击行为", int expirationHours = 0, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(Result.Failure(Error.ServiceUnavailable("防火墙服务未启用")));
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return Task.FromResult(Result.Failure(Error.Validation("IP地址不能为空")));
        }

        var effectiveExpiration = expirationHours > 0 ? expirationHours : _config.BlacklistExpirationHours;
        var entry = new FirewallEntry
        {
            IpAddress = ipAddress,
            Reason = reason,
            AddedAt = DateTime.UtcNow,
            ExpiresAt = effectiveExpiration > 0 ? DateTime.UtcNow.AddHours(effectiveExpiration) : null
        };

        _blacklist.AddOrUpdate(ipAddress, entry, (_, _) => entry);

        // 同时从白名单移除
        _whitelist.TryRemove(ipAddress, out _);

        _logger.LogWarning("IP已添加到黑名单: {IpAddress}, 原因: {Reason}", ipAddress, reason);

        // AddWindowsFirewallRule(ipAddress, false);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> RemoveFromBlacklistAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(Result.Failure(Error.ServiceUnavailable("防火墙服务未启用")));
        }

        if (_blacklist.TryRemove(ipAddress, out _))
        {
            _logger.LogInformation("IP已从黑名单移除: {IpAddress}", ipAddress);
            // RemoveWindowsFirewallRule(ipAddress, false);
            return Task.FromResult(Result.Success());
        }

        return Task.FromResult(Result.Failure(Error.NotFound("IP不在黑名单中")));
    }

    public Task<Result> ClearBlacklistAsync(CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(Result.Failure(Error.ServiceUnavailable("防火墙服务未启用")));
        }

        var count = _blacklist.Count;
        _blacklist.Clear();

        _logger.LogInformation("黑名单已清空，共移除 {Count} 条记录", count);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> ClearWhitelistAsync(CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(Result.Failure(Error.ServiceUnavailable("防火墙服务未启用")));
        }

        var count = _whitelist.Count;
        _whitelist.Clear();

        _logger.LogInformation("白名单已清空，共移除 {Count} 条记录", count);

        return Task.FromResult(Result.Success());
    }

    public Task<bool> IsWhitelistedAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(ipAddress))
        {
            return Task.FromResult(false);
        }

        if (_whitelist.TryGetValue(ipAddress, out var entry))
        {
            if (!entry.IsExpired)
            {
                return Task.FromResult(true);
            }

            // 已过期，移除
            _whitelist.TryRemove(ipAddress, out _);
        }

        return Task.FromResult(false);
    }

    public Task<bool> IsBlacklistedAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(ipAddress))
        {
            return Task.FromResult(false);
        }

        if (_blacklist.TryGetValue(ipAddress, out var entry))
        {
            if (!entry.IsExpired)
            {
                return Task.FromResult(true);
            }

            // 已过期，移除
            _blacklist.TryRemove(ipAddress, out _);
        }

        return Task.FromResult(false);
    }

    public Task<IReadOnlyList<FirewallEntry>> GetWhitelistAsync(CancellationToken cancellationToken = default)
    {
        // 清理过期条目
        var expiredKeys = _whitelist.Where(x => x.Value.IsExpired).Select(x => x.Key).ToList();
        foreach (var key in expiredKeys)
        {
            _whitelist.TryRemove(key, out _);
        }

        return Task.FromResult<IReadOnlyList<FirewallEntry>>(_whitelist.Values.ToList());
    }

    public Task<IReadOnlyList<FirewallEntry>> GetBlacklistAsync(CancellationToken cancellationToken = default)
    {
        // 清理过期条目
        var expiredKeys = _blacklist.Where(x => x.Value.IsExpired).Select(x => x.Key).ToList();
        foreach (var key in expiredKeys)
        {
            _blacklist.TryRemove(key, out _);
        }

        return Task.FromResult<IReadOnlyList<FirewallEntry>>(_blacklist.Values.ToList());
    }

    public async Task<bool> RecordConnectionAttemptAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || !_config.AutoBlockAttackers || string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var tracker = _connectionAttempts.GetOrAdd(ipAddress, _ => new ConnectionAttemptTracker());

        lock (tracker)
        {
            // 清理过期的记录
            var windowStart = now.AddSeconds(-_config.AttackDetectionWindowSeconds);
            tracker.Attempts.RemoveAll(t => t < windowStart);

            // 添加新记录
            tracker.Attempts.Add(now);

            // 检测是否超过阈值
            if (tracker.Attempts.Count > _config.MaxConnectionsPerSecond)
            {
                _logger.LogWarning("检测到可能的攻击行为: IP={IpAddress}, 连接数={Count}/{Window}秒",
                    ipAddress, tracker.Attempts.Count, _config.AttackDetectionWindowSeconds);

                // 自动加入黑名单
                _ = AddToBlacklistAsync(ipAddress, $"自动检测: {tracker.Attempts.Count}次连接/{_config.AttackDetectionWindowSeconds}秒",
                    _config.BlacklistExpirationHours, cancellationToken);

                return true;
            }
        }

        return false;
    }

    private class ConnectionAttemptTracker
    {
        public List<DateTime> Attempts { get; } = new();
    }
}

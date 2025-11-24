using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using AionGate.Data.Entities;

namespace AionGate.Data.Repositories;

/// <summary>
/// 账号仓储实现 (EF Core + Dapper + Redis)
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly AionGateDbContext _context;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public AccountRepository(AionGateDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Account?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"account:{id}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (cached != null)
        {
            return JsonSerializer.Deserialize<Account>(cached);
        }

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account != null)
        {
            await CacheAccountAsync(account, cancellationToken);
        }

        return account;
    }

    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"account:username:{username.ToLowerInvariant()}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (cached != null)
        {
            return JsonSerializer.Deserialize<Account>(cached);
        }

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == username, cancellationToken);

        if (account != null)
        {
            await CacheAccountAsync(account, cancellationToken);
        }

        return account;
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
    }

    public async Task<long> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        await CacheAccountAsync(account, cancellationToken);

        return account.Id;
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);

        await InvalidateCacheAsync(account.Id, account.Username, cancellationToken);
        await CacheAccountAsync(account, cancellationToken);
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken = default)
    {
        // 使用 Dapper 优化性能关键查询
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT COUNT(*)
            FROM accounts
            WHERE username = @Username
              AND password_hash = @PasswordHash
              AND status = 1";

        var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { Username = username, PasswordHash = passwordHash });

        return count > 0;
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Username == username, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Email == email, cancellationToken);
    }

    public async Task IncrementLoginAttemptsAsync(long accountId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            UPDATE accounts
            SET login_attempts = login_attempts + 1,
                updated_at = @Now
            WHERE id = @AccountId";

        await connection.ExecuteAsync(sql, new { AccountId = accountId, Now = DateTime.UtcNow });

        await InvalidateCacheAsync(accountId, null, cancellationToken);
    }

    public async Task ResetLoginAttemptsAsync(long accountId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            UPDATE accounts
            SET login_attempts = 0,
                locked_until = NULL,
                updated_at = @Now
            WHERE id = @AccountId";

        await connection.ExecuteAsync(sql, new { AccountId = accountId, Now = DateTime.UtcNow });

        await InvalidateCacheAsync(accountId, null, cancellationToken);
    }

    public async Task LockAccountAsync(long accountId, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var lockedUntil = DateTime.UtcNow.Add(duration);

        var connection = _context.Database.GetDbConnection();

        var sql = @"
            UPDATE accounts
            SET status = 2,
                locked_until = @LockedUntil,
                updated_at = @Now
            WHERE id = @AccountId";

        await connection.ExecuteAsync(sql, new
        {
            AccountId = accountId,
            LockedUntil = lockedUntil,
            Now = DateTime.UtcNow
        });

        await InvalidateCacheAsync(accountId, null, cancellationToken);
    }

    public async Task UpdateLastLoginAsync(long accountId, string ipAddress, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            UPDATE accounts
            SET last_login_at = @Now,
                last_login_ip = @IpAddress,
                login_attempts = 0,
                updated_at = @Now
            WHERE id = @AccountId";

        await connection.ExecuteAsync(sql, new
        {
            AccountId = accountId,
            IpAddress = ipAddress,
            Now = DateTime.UtcNow
        });

        await InvalidateCacheAsync(accountId, null, cancellationToken);
    }

    // 私有辅助方法
    private async Task CacheAccountAsync(Account account, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(account);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        };

        await _cache.SetStringAsync($"account:{account.Id}", json, options, cancellationToken);
        await _cache.SetStringAsync($"account:username:{account.Username.ToLowerInvariant()}", json, options, cancellationToken);
    }

    private async Task InvalidateCacheAsync(long accountId, string? username, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync($"account:{accountId}", cancellationToken);

        if (username != null)
        {
            await _cache.RemoveAsync($"account:username:{username.ToLowerInvariant()}", cancellationToken);
        }
    }
}

/// <summary>
/// 账号仓储接口
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task<bool> ValidateCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task IncrementLoginAttemptsAsync(long accountId, CancellationToken cancellationToken = default);
    Task ResetLoginAttemptsAsync(long accountId, CancellationToken cancellationToken = default);
    Task LockAccountAsync(long accountId, TimeSpan duration, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(long accountId, string ipAddress, CancellationToken cancellationToken = default);
}

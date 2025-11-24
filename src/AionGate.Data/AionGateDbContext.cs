using Microsoft.EntityFrameworkCore;
using AionGate.Data.Entities;

namespace AionGate.Data;

/// <summary>
/// AionGate 数据库上下文
/// </summary>
public class AionGateDbContext : DbContext
{
    public AionGateDbContext(DbContextOptions<AionGateDbContext> options)
        : base(options)
    {
    }

    // 实体集
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<IpBlacklist> IpBlacklists => Set<IpBlacklist>();
    public DbSet<HardwareFingerprint> HardwareFingerprints => Set<HardwareFingerprint>();
    public DbSet<CheatDetection> CheatDetections => Set<CheatDetection>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ServerConfig> ServerConfigs => Set<ServerConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置实体
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AionGateDbContext).Assembly);

        // 全局查询过滤器
        modelBuilder.Entity<Account>().HasQueryFilter(a => a.Status != AccountStatus.Deleted);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // 启用敏感数据日志（开发环境）
        #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        #endif

        // 性能优化
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// 优化的批量保存
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 自动设置时间戳
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ITimestampedEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            var entity = (ITimestampedEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
            }

            entity.UpdatedAt = now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// 时间戳接口
/// </summary>
public interface ITimestampedEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

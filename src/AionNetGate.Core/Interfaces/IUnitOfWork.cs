namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 工作单元接口，管理多个仓储并协调事务
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 账号仓储
    /// </summary>
    IAccountRepository Accounts { get; }

    /// <summary>
    /// 会话仓储
    /// </summary>
    ISessionRepository Sessions { get; }

    /// <summary>
    /// 硬件指纹仓储
    /// </summary>
    IHardwareFingerprintRepository HardwareFingerprints { get; }

    /// <summary>
    /// IP 黑名单仓储
    /// </summary>
    IIpBlacklistRepository IpBlacklists { get; }

    /// <summary>
    /// 游戏邮件仓储
    /// </summary>
    IGameMailRepository GameMails { get; }

    /// <summary>
    /// 游戏公告仓储
    /// </summary>
    IGameAnnouncementRepository GameAnnouncements { get; }

    /// <summary>
    /// 游戏物品发放仓储
    /// </summary>
    IGameItemGrantRepository GameItemGrants { get; }

    /// <summary>
    /// 保存所有更改
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 开始事务
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交事务
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 回滚事务
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

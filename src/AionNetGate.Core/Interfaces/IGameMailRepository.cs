using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 游戏邮件仓储接口
/// </summary>
public interface IGameMailRepository : IRepository<GameMail>
{
    /// <summary>
    /// 获取账号的邮件列表
    /// </summary>
    Task<IEnumerable<GameMail>> GetByAccountIdAsync(
        long accountId,
        bool includeRead = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取账号的未读邮件数量
    /// </summary>
    Task<int> GetUnreadCountAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记邮件为已读
    /// </summary>
    Task MarkAsReadAsync(long mailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记附件已领取
    /// </summary>
    Task MarkAttachmentsClaimedAsync(long mailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除过期邮件
    /// </summary>
    Task<int> DeleteExpiredMailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量发送邮件
    /// </summary>
    Task<IEnumerable<GameMail>> SendBulkMailAsync(
        IEnumerable<long> recipientAccountIds,
        string title,
        string content,
        string? attachmentsJson = null,
        long attachedGold = 0,
        int mailType = 1,
        int expirationDays = 30,
        CancellationToken cancellationToken = default);
}

using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 游戏公告仓储接口
/// </summary>
public interface IGameAnnouncementRepository : IRepository<GameAnnouncement>
{
    /// <summary>
    /// 获取当前有效的公告列表
    /// </summary>
    Task<IEnumerable<GameAnnouncement>> GetActiveAnnouncementsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定类型的公告
    /// </summary>
    Task<IEnumerable<GameAnnouncement>> GetByTypeAsync(
        int announcementType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取置顶公告
    /// </summary>
    Task<IEnumerable<GameAnnouncement>> GetPinnedAnnouncementsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置公告状态
    /// </summary>
    Task SetActiveStatusAsync(
        long announcementId,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置置顶状态
    /// </summary>
    Task SetPinnedStatusAsync(
        long announcementId,
        bool isPinned,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最新公告
    /// </summary>
    Task<IEnumerable<GameAnnouncement>> GetLatestAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
}

using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 游戏物品发放仓储接口
/// </summary>
public interface IGameItemGrantRepository : IRepository<GameItemGrant>
{
    /// <summary>
    /// 获取账号的物品发放记录
    /// </summary>
    Task<IEnumerable<GameItemGrant>> GetByAccountIdAsync(
        long accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取待处理的发放记录
    /// </summary>
    Task<IEnumerable<GameItemGrant>> GetPendingGrantsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新发放状态
    /// </summary>
    Task UpdateStatusAsync(
        long grantId,
        int status,
        string? statusMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取操作者的发放记录
    /// </summary>
    Task<IEnumerable<GameItemGrant>> GetByOperatorIdAsync(
        long operatorId,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取物品发放统计
    /// </summary>
    Task<Dictionary<int, long>> GetItemGrantStatisticsAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);
}

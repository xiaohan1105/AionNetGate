using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 会话仓储接口
/// </summary>
public interface ISessionRepository : IRepository<Session>
{
    /// <summary>
    /// 根据 Token 获取会话
    /// </summary>
    Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 Refresh Token 获取会话
    /// </summary>
    Task<Session?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 验证 Token 是否有效
    /// </summary>
    Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// 撤销 Token
    /// </summary>
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// 撤销账号的所有 Token
    /// </summary>
    Task RevokeAllUserTokensAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取账号的所有活动会话
    /// </summary>
    Task<IEnumerable<Session>> GetActiveSessionsByAccountIdAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 撤销会话
    /// </summary>
    Task RevokeSessionAsync(long sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 撤销账号的所有会话
    /// </summary>
    Task RevokeAllSessionsByAccountIdAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理过期会话
    /// </summary>
    Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前活跃会话数量
    /// </summary>
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据账号 ID 获取所有会话
    /// </summary>
    Task<IEnumerable<Session>> GetByAccountIdAsync(long accountId, CancellationToken cancellationToken = default);
}

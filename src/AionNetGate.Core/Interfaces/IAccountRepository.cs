using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 账号仓储接口
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// 根据用户名获取账号
    /// </summary>
    Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据邮箱获取账号
    /// </summary>
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 验证用户名和密码
    /// </summary>
    Task<bool> ValidateCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查邮箱是否存在
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 增加登录失败次数
    /// </summary>
    Task IncrementLoginAttemptsAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置登录失败次数
    /// </summary>
    Task ResetLoginAttemptsAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 锁定账号
    /// </summary>
    Task LockAccountAsync(long accountId, TimeSpan duration, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解锁账号
    /// </summary>
    Task UnlockAccountAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新最后登录时间
    /// </summary>
    Task UpdateLastLoginAsync(long accountId, string ipAddress, CancellationToken cancellationToken = default);
}

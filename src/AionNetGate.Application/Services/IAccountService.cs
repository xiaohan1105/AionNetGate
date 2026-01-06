using AionNetGate.Core.Domain.Entities;
using AionNetGate.Core.Results;

namespace AionNetGate.Application.Services;

/// <summary>
/// 账号服务接口
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// 注册新账号
    /// </summary>
    Task<Result<Account>> RegisterAsync(string username, string password, string? email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 登录
    /// </summary>
    Task<Result<(Account Account, string Token, string RefreshToken)>> LoginAsync(string username, string password, string clientIp, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新 Token
    /// </summary>
    Task<Result<(string Token, string RefreshToken)>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 登出
    /// </summary>
    Task<Result> LogoutAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// 修改密码
    /// </summary>
    Task<Result> ChangePasswordAsync(long accountId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置密码
    /// </summary>
    Task<Result> ResetPasswordAsync(string username, string email, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 验证 Token
    /// </summary>
    Task<Result<Account>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

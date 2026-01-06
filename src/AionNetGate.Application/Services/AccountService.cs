using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Domain.Entities;
using AionNetGate.Core.Interfaces;
using AionNetGate.Core.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AionNetGate.Application.Services;

/// <summary>
/// 账号服务实现
/// </summary>
public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SecurityConfig _securityConfig;
    private readonly ILogger<AccountService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AccountService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IOptions<SecurityConfig> securityConfig,
        ILogger<AccountService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _securityConfig = securityConfig?.Value ?? throw new ArgumentNullException(nameof(securityConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<Account>> RegisterAsync(string username, string password, string? email, CancellationToken cancellationToken = default)
    {
        try
        {
            // 验证用户名
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 20)
                return Result<Account>.Failure(Error.Validation("用户名长度必须在 3-20 之间"));

            // 验证密码
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return Result<Account>.Failure(Error.Validation("密码长度必须至少 6 位"));

            // 检查用户名是否已存在
            if (await _unitOfWork.Accounts.UsernameExistsAsync(username, cancellationToken))
                return Result<Account>.Failure(Error.Conflict("用户名已存在"));

            // 检查邮箱是否已存在
            if (!string.IsNullOrEmpty(email) && await _unitOfWork.Accounts.EmailExistsAsync(email, cancellationToken))
                return Result<Account>.Failure(Error.Conflict("邮箱已存在"));

            // 哈希密码
            var (passwordHash, passwordSalt) = _passwordHasher.HashPassword(password);

            // 创建账号
            var account = new Account
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = 1, // 正常
                Role = 0, // 普通用户
                LoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("账号注册成功: {Username}", username);

            return Result<Account>.Success(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "账号注册失败: {Username}", username);
            return Result<Account>.Failure(Error.Internal("账号注册失败"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<(Account Account, string Token, string RefreshToken)>> LoginAsync(
        string username, string password, string clientIp, CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取账号
            var account = await _unitOfWork.Accounts.GetByUsernameAsync(username, cancellationToken);
            if (account == null)
                return Result<(Account, string, string)>.Failure(Error.NotFound("账号不存在"));

            // 检查账号状态
            if (account.Status == 0)
                return Result<(Account, string, string)>.Failure(Error.Forbidden("账号已禁用"));

            // 检查是否被锁定
            if (account.LockedUntil.HasValue && account.LockedUntil.Value > DateTime.UtcNow)
            {
                var remainingTime = (account.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                return Result<(Account, string, string)>.Failure(
                    Error.Forbidden($"账号已锁定，剩余时间: {remainingTime:F0} 分钟"));
            }

            // 验证密码
            if (!_passwordHasher.VerifyPassword(password, account.PasswordHash, account.PasswordSalt))
            {
                // 增加登录失败次数
                await _unitOfWork.Accounts.IncrementLoginAttemptsAsync(account.Id, cancellationToken);

                // 检查是否需要锁定账号
                if (account.LoginAttempts + 1 >= _securityConfig.MaxLoginAttempts)
                {
                    var lockDuration = TimeSpan.FromMinutes(_securityConfig.AccountLockoutMinutes);
                    await _unitOfWork.Accounts.LockAccountAsync(account.Id, lockDuration, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("账号因多次登录失败被锁定: {Username}", username);
                    return Result<(Account, string, string)>.Failure(
                        Error.Forbidden($"密码错误次数过多，账号已锁定 {_securityConfig.AccountLockoutMinutes} 分钟"));
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<(Account, string, string)>.Failure(Error.Unauthorized("密码错误"));
            }

            // 登录成功，重置失败次数
            await _unitOfWork.Accounts.ResetLoginAttemptsAsync(account.Id, cancellationToken);
            await _unitOfWork.Accounts.UpdateLastLoginAsync(account.Id, clientIp, cancellationToken);

            // 生成 Token
            var (token, refreshToken) = GenerateTokens(account);

            // 保存 Session
            var session = new Session
            {
                AccountId = account.Id,
                Token = token,
                RefreshToken = refreshToken,
                ClientIp = clientIp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securityConfig.AccessTokenExpirationMinutes),
                RefreshExpiresAt = DateTime.UtcNow.AddDays(_securityConfig.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Sessions.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("账号登录成功: {Username} from {ClientIp}", username, clientIp);

            return Result<(Account, string, string)>.Success((account, token, refreshToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "账号登录失败: {Username}", username);
            return Result<(Account, string, string)>.Failure(Error.Internal("登录失败"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<(string Token, string RefreshToken)>> RefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // 查找 Session
            var session = await _unitOfWork.Sessions.GetByRefreshTokenAsync(refreshToken, cancellationToken);
            if (session == null)
                return Result<(string, string)>.Failure(Error.Unauthorized("无效的 Refresh Token"));

            // 检查是否过期
            if (session.RefreshExpiresAt < DateTime.UtcNow)
                return Result<(string, string)>.Failure(Error.Unauthorized("Refresh Token 已过期"));

            // 检查是否已撤销
            if (session.RevokedAt.HasValue)
                return Result<(string, string)>.Failure(Error.Unauthorized("Refresh Token 已撤销"));

            // 生成新 Token
            var (newToken, newRefreshToken) = GenerateTokens(session.Account);

            // 更新 Session
            session.Token = newToken;
            session.RefreshToken = newRefreshToken;
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(_securityConfig.AccessTokenExpirationMinutes);
            session.RefreshExpiresAt = DateTime.UtcNow.AddDays(_securityConfig.RefreshTokenExpirationDays);

            await _unitOfWork.Sessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token 刷新成功: AccountId={AccountId}", session.AccountId);

            return Result<(string, string)>.Success((newToken, newRefreshToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token 刷新失败");
            return Result<(string, string)>.Failure(Error.Internal("Token 刷新失败"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.Sessions.RevokeTokenAsync(token, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("用户登出成功");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户登出失败");
            return Result.Failure(Error.Internal("登出失败"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ChangePasswordAsync(
        long accountId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
            if (account == null)
                return Result.Failure(Error.NotFound("账号不存在"));

            // 验证旧密码
            if (!_passwordHasher.VerifyPassword(oldPassword, account.PasswordHash, account.PasswordSalt))
                return Result.Failure(Error.Unauthorized("旧密码错误"));

            // 验证新密码
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return Result.Failure(Error.Validation("新密码长度必须至少 6 位"));

            // 哈希新密码
            var (passwordHash, passwordSalt) = _passwordHasher.HashPassword(newPassword);

            // 更新密码
            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;
            account.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Accounts.UpdateAsync(account, cancellationToken);

            // 撤销所有现有 Session
            await _unitOfWork.Sessions.RevokeAllSessionsByAccountIdAsync(accountId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("密码修改成功: AccountId={AccountId}", accountId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密码修改失败: AccountId={AccountId}", accountId);
            return Result.Failure(Error.Internal("密码修改失败"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ResetPasswordAsync(
        string username, string email, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _unitOfWork.Accounts.GetByUsernameAsync(username, cancellationToken);
            if (account == null || account.Email != email)
                return Result.Failure(Error.NotFound("账号不存在或邮箱不匹配"));

            // 验证新密码
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return Result.Failure(Error.Validation("新密码长度必须至少 6 位"));

            // 哈希新密码
            var (passwordHash, passwordSalt) = _passwordHasher.HashPassword(newPassword);

            // 更新密码
            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;
            account.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Accounts.UpdateAsync(account, cancellationToken);
            await _unitOfWork.Sessions.RevokeAllSessionsByAccountIdAsync(account.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("密码重置成功: {Username}", username);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密码重置失败: {Username}", username);
            return Result.Failure(Error.Internal("密码重置失败"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Account>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByTokenAsync(token, cancellationToken);
            if (session == null)
                return Result<Account>.Failure(Error.Unauthorized("无效的 Token"));

            if (session.ExpiresAt < DateTime.UtcNow)
                return Result<Account>.Failure(Error.Unauthorized("Token 已过期"));

            if (session.RevokedAt.HasValue)
                return Result<Account>.Failure(Error.Unauthorized("Token 已撤销"));

            return Result<Account>.Success(session.Account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token 验证失败");
            return Result<Account>.Failure(Error.Internal("Token 验证失败"));
        }
    }

    /// <summary>
    /// 生成 JWT Token
    /// </summary>
    private (string Token, string RefreshToken) GenerateTokens(Account account)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.Username),
            new Claim("role", account.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityConfig.JwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _securityConfig.JwtIssuer,
            audience: _securityConfig.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_securityConfig.AccessTokenExpirationMinutes),
            signingCredentials: creds
        );

        var refreshToken = Guid.NewGuid().ToString("N");

        return (new JwtSecurityTokenHandler().WriteToken(token), refreshToken);
    }
}

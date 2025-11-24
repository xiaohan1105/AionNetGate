using AionNetGate.Core.Common;
using AionNetGate.Core.Data;
using AionNetGate.Core.Domain;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Core.Services;

/// <summary>
/// 登录服务
/// 处理用户登录认证和会话创建
/// </summary>
public class LoginService : ILoginService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILoginHistoryRepository _loginHistoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionManager _sessionManager;
    private readonly IIPBlacklistChecker _ipBlacklistChecker;
    private readonly ILogger<LoginService> _logger;

    public LoginService(
        IAccountRepository accountRepository,
        ILoginHistoryRepository loginHistoryRepository,
        IPasswordHasher passwordHasher,
        ISessionManager sessionManager,
        IIPBlacklistChecker ipBlacklistChecker,
        ILogger<LoginService> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _loginHistoryRepository = loginHistoryRepository ?? throw new ArgumentNullException(nameof(loginHistoryRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _ipBlacklistChecker = ipBlacklistChecker ?? throw new ArgumentNullException(nameof(ipBlacklistChecker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理用户登录
    /// </summary>
    public async Task<Result<Session>> LoginAsync(
        string username,
        string password,
        string connectionId,
        string ipAddress,
        string? hardwareId = null)
    {
        try
        {
            // 1. 检查IP黑名单
            if (await _ipBlacklistChecker.IsBlacklistedAsync(ipAddress))
            {
                _logger.LogWarning("IP地址在黑名单中: {IP}", ipAddress);
                return Result<Session>.Failure("IP地址已被封禁");
            }

            // 2. 查询账号
            var account = await _accountRepository.GetByUsernameAsync(username);
            if (account == null)
            {
                _logger.LogWarning("登录失败 - 账号不存在: Username={Username}, IP={IP}",
                    username, ipAddress);

                await RecordLoginHistoryAsync(0, username, ipAddress, false, "账号不存在");
                return Result<Session>.Failure("用户名或密码错误");
            }

            // 3. 检查账号状态
            if (account.Status != AccountStatus.Active)
            {
                _logger.LogWarning("登录失败 - 账号已被封禁: AccountId={AccountId}, Username={Username}",
                    account.Id, username);

                await RecordLoginHistoryAsync(account.Id, username, ipAddress, false, "账号已封禁");
                return Result<Session>.Failure("账号已被封禁，请联系客服");
            }

            // 4. 验证密码
            if (!_passwordHasher.VerifyPassword(password, account.PasswordHash))
            {
                _logger.LogWarning("登录失败 - 密码错误: AccountId={AccountId}, Username={Username}, IP={IP}",
                    account.Id, username, ipAddress);

                await RecordLoginHistoryAsync(account.Id, username, ipAddress, false, "密码错误");
                return Result<Session>.Failure("用户名或密码错误");
            }

            // 5. 检查是否已在线（踢下线）
            if (_sessionManager.IsAccountOnline(account.Id))
            {
                var existingSession = _sessionManager.GetSessionByAccountId(account.Id);
                if (existingSession != null)
                {
                    _logger.LogInformation(
                        "账号重复登录，踢下线旧会话: AccountId={AccountId}, OldSessionId={OldSessionId}",
                        account.Id, existingSession.SessionId);

                    _sessionManager.RemoveSession(existingSession.SessionId);
                }
            }

            // 6. 创建新会话
            var session = _sessionManager.CreateSession(
                account.Id,
                account.Username,
                connectionId,
                ipAddress,
                hardwareId);

            // 7. 更新账号最后登录信息
            account.UpdateLastLogin(ipAddress);
            await _accountRepository.UpdateAsync(account);

            // 8. 记录登录历史
            await RecordLoginHistoryAsync(account.Id, username, ipAddress, true, "登录成功");

            _logger.LogInformation(
                "登录成功: AccountId={AccountId}, Username={Username}, SessionId={SessionId}, IP={IP}",
                account.Id, username, session.SessionId, ipAddress);

            return Result<Session>.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录处理异常: Username={Username}, IP={IP}", username, ipAddress);
            return Result<Session>.Failure("登录失败，请稍后重试");
        }
    }

    /// <summary>
    /// 处理用户登出
    /// </summary>
    public async Task<Result> LogoutAsync(string connectionId)
    {
        try
        {
            var session = _sessionManager.GetSessionByConnectionId(connectionId);
            if (session == null)
            {
                _logger.LogWarning("登出失败 - 未找到会话: ConnectionId={ConnectionId}", connectionId);
                return Result.Failure("会话不存在");
            }

            _sessionManager.RemoveSession(session.SessionId);

            _logger.LogInformation(
                "登出成功: AccountId={AccountId}, SessionId={SessionId}",
                session.AccountId, session.SessionId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登出处理异常: ConnectionId={ConnectionId}", connectionId);
            return Result.Failure("登出失败");
        }
    }

    /// <summary>
    /// 验证会话
    /// </summary>
    public async Task<Result<Session>> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var session = _sessionManager.GetSessionBySessionId(sessionId);
            if (session == null)
            {
                return Result<Session>.Failure("会话不存在");
            }

            if (session.IsExpired(30))
            {
                _sessionManager.RemoveSession(sessionId);
                return Result<Session>.Failure("会话已过期");
            }

            // 更新活动时间
            _sessionManager.UpdateActivity(sessionId);

            return Result<Session>.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "会话验证异常: SessionId={SessionId}", sessionId);
            return Result<Session>.Failure("会话验证失败");
        }
    }

    /// <summary>
    /// 记录登录历史
    /// </summary>
    private async Task RecordLoginHistoryAsync(
        int accountId,
        string username,
        string ipAddress,
        bool success,
        string? failureReason = null)
    {
        try
        {
            var history = LoginHistory.Create(accountId, username, ipAddress, success, failureReason);
            await _loginHistoryRepository.AddAsync(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录登录历史失败: AccountId={AccountId}", accountId);
        }
    }
}

/// <summary>
/// 登录服务接口
/// </summary>
public interface ILoginService
{
    Task<Result<Session>> LoginAsync(string username, string password, string connectionId, string ipAddress, string? hardwareId = null);
    Task<Result> LogoutAsync(string connectionId);
    Task<Result<Session>> ValidateSessionAsync(string sessionId);
}

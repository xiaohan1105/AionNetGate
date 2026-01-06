using AionNetGate.Application.Services;
using AionNetGate.Network.Packets;
using AionNetGate.Network.Packets.Client;
using AionNetGate.Network.Packets.Server;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers;

/// <summary>
/// 账号请求处理器
/// </summary>
public class AccountRequestHandler : IPacketHandler<CM_AccountRequest>
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountRequestHandler> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AccountRequestHandler(
        IAccountService accountService,
        ILogger<AccountRequestHandler> logger)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task HandleAsync(
        CM_AccountRequest packet,
        IClientConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            SM_AccountResponse response = packet.OperationType switch
            {
                AccountOperationType.Register => await HandleRegisterAsync(packet, cancellationToken),
                AccountOperationType.Login => await HandleLoginAsync(packet, connection, cancellationToken),
                AccountOperationType.RefreshToken => await HandleRefreshTokenAsync(packet, cancellationToken),
                AccountOperationType.Logout => await HandleLogoutAsync(packet, cancellationToken),
                AccountOperationType.ChangePassword => await HandleChangePasswordAsync(packet, cancellationToken),
                AccountOperationType.ResetPassword => await HandleResetPasswordAsync(packet, cancellationToken),
                _ => SM_AccountResponse.CreateFailure(packet.OperationType, "未知的操作类型")
            };

            await connection.SendPacketAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理账号请求时发生错误: {OperationType}", packet.OperationType);

            var errorResponse = SM_AccountResponse.CreateFailure(
                packet.OperationType,
                "服务器内部错误");

            await connection.SendPacketAsync(errorResponse, cancellationToken);
        }
    }

    /// <summary>
    /// 处理注册请求
    /// </summary>
    private async Task<SM_AccountResponse> HandleRegisterAsync(
        CM_AccountRequest packet,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet.Username) || string.IsNullOrEmpty(packet.Password))
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.Register,
                "用户名或密码不能为空");
        }

        var result = await _accountService.RegisterAsync(
            packet.Username,
            packet.Password,
            packet.Email,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.Register,
                result.Error.Message);
        }

        var response = SM_AccountResponse.CreateSuccess(AccountOperationType.Register);
        response.AccountId = result.Value!.Id;
        response.Username = result.Value.Username;

        _logger.LogInformation("账号注册成功: {Username}", packet.Username);

        return response;
    }

    /// <summary>
    /// 处理登录请求
    /// </summary>
    private async Task<SM_AccountResponse> HandleLoginAsync(
        CM_AccountRequest packet,
        IClientConnection connection,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet.Username) || string.IsNullOrEmpty(packet.Password))
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.Login,
                "用户名或密码不能为空");
        }

        var result = await _accountService.LoginAsync(
            packet.Username,
            packet.Password,
            connection.RemoteEndPoint?.ToString() ?? "unknown",
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.Login,
                result.Error.Message);
        }

        var (account, token, refreshToken) = result.Value;

        // 设置连接的账号信息
        connection.AccountId = account.Id;
        connection.Username = account.Username;

        var response = SM_AccountResponse.CreateSuccess(AccountOperationType.Login);
        response.AccountId = account.Id;
        response.Username = account.Username;
        response.Token = token;
        response.RefreshToken = refreshToken;
        response.Role = account.Role;

        _logger.LogInformation("账号登录成功: {Username} from {IP}", packet.Username, connection.RemoteEndPoint);

        return response;
    }

    /// <summary>
    /// 处理刷新 Token 请求
    /// </summary>
    private async Task<SM_AccountResponse> HandleRefreshTokenAsync(
        CM_AccountRequest packet,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet.RefreshToken))
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.RefreshToken,
                "Refresh Token 不能为空");
        }

        var result = await _accountService.RefreshTokenAsync(
            packet.RefreshToken,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.RefreshToken,
                result.Error.Message);
        }

        var (token, refreshToken) = result.Value;

        var response = SM_AccountResponse.CreateSuccess(AccountOperationType.RefreshToken);
        response.Token = token;
        response.RefreshToken = refreshToken;

        _logger.LogInformation("Token 刷新成功");

        return response;
    }

    /// <summary>
    /// 处理登出请求
    /// </summary>
    private async Task<SM_AccountResponse> HandleLogoutAsync(
        CM_AccountRequest packet,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet.Token))
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.Logout,
                "Token 不能为空");
        }

        var result = await _accountService.LogoutAsync(
            packet.Token,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.Logout,
                result.Error.Message);
        }

        var response = SM_AccountResponse.CreateSuccess(AccountOperationType.Logout);

        _logger.LogInformation("用户登出成功");

        return response;
    }

    /// <summary>
    /// 处理修改密码请求
    /// </summary>
    private Task<SM_AccountResponse> HandleChangePasswordAsync(
        CM_AccountRequest packet,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet.OldPassword) || string.IsNullOrEmpty(packet.NewPassword))
        {
            return Task.FromResult(SM_AccountResponse.CreateFailure(
                AccountOperationType.ChangePassword,
                "旧密码或新密码不能为空"));
        }

        // 这里需要从连接中获取账号 ID
        // TODO: 从 Token 中解析账号 ID
        // 暂时返回错误
        return Task.FromResult(SM_AccountResponse.CreateFailure(
            AccountOperationType.ChangePassword,
            "功能未完全实现"));

        // var result = await _accountService.ChangePasswordAsync(
        //     accountId,
        //     packet.OldPassword,
        //     packet.NewPassword,
        //     cancellationToken);
        //
        // if (!result.IsSuccess)
        // {
        //     return SM_AccountResponse.CreateFailure(
        //         AccountOperationType.ChangePassword,
        //         result.Error.Message);
        // }
        //
        // return SM_AccountResponse.CreateSuccess(AccountOperationType.ChangePassword);
    }

    /// <summary>
    /// 处理重置密码请求
    /// </summary>
    private async Task<SM_AccountResponse> HandleResetPasswordAsync(
        CM_AccountRequest packet,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(packet.Username) ||
            string.IsNullOrEmpty(packet.Email) ||
            string.IsNullOrEmpty(packet.NewPassword))
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.ResetPassword,
                "用户名、邮箱或新密码不能为空");
        }

        var result = await _accountService.ResetPasswordAsync(
            packet.Username,
            packet.Email,
            packet.NewPassword,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return SM_AccountResponse.CreateFailure(
                AccountOperationType.ResetPassword,
                result.Error.Message);
        }

        var response = SM_AccountResponse.CreateSuccess(AccountOperationType.ResetPassword);

        _logger.LogInformation("密码重置成功: {Username}", packet.Username);

        return response;
    }
}

using AionNetGate.Core.Common;
using AionNetGate.Core.Domain;
using AionNetGate.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Core.Application.Commands;

/// <summary>
/// 登录Command处理器
/// 封装登录业务逻辑
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<Session>>
{
    private readonly ILoginService _loginService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        ILoginService loginService,
        ILogger<LoginCommandHandler> logger)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Session>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "处理登录请求: Username={Username}, IP={IP}",
            request.Username, request.IpAddress);

        var result = await _loginService.LoginAsync(
            request.Username,
            request.Password,
            request.ConnectionId,
            request.IpAddress,
            request.HardwareId);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "登录Command执行成功: Username={Username}, SessionId={SessionId}",
                request.Username, result.Value!.SessionId);
        }
        else
        {
            _logger.LogWarning(
                "登录Command执行失败: Username={Username}, Error={Error}",
                request.Username, result.Error);
        }

        return result;
    }
}

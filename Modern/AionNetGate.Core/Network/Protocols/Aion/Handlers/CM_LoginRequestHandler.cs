using AionNetGate.Core.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Core.Network.Protocols.Aion.Handlers;

/// <summary>
/// 处理客户端登录请求
/// 使用CQRS模式通过MediatR分发Command
/// </summary>
public class CM_LoginRequestHandler : IPacketHandler<CM_LoginRequest>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CM_LoginRequestHandler> _logger;

    public CM_LoginRequestHandler(
        IMediator mediator,
        ILogger<CM_LoginRequestHandler> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(CM_LoginRequest packet, IConnectionContext context)
    {
        _logger.LogInformation(
            "收到登录请求: Username={Username}, ConnectionId={ConnectionId}",
            packet.Username, context.ConnectionId);

        // 创建LoginCommand
        var command = new LoginCommand
        {
            Username = packet.Username,
            Password = packet.Password,
            ConnectionId = context.ConnectionId,
            IpAddress = context.RemoteIpAddress,
            HardwareId = packet.HardwareId
        };

        // 通过MediatR发送Command
        var result = await _mediator.Send(command);

        // 构建响应Packet
        var response = new SM_LoginResponse();

        if (result.IsSuccess)
        {
            var session = result.Value!;

            response.Status = 0; // 成功
            response.SessionId = session.SessionId;
            response.AccountId = session.AccountId;
            response.AccountName = session.AccountName;
            response.Message = "登录成功";

            // 标记连接为已认证
            context.SetAuthenticated(session.AccountId);

            _logger.LogInformation(
                "登录成功: Username={Username}, SessionId={SessionId}, AccountId={AccountId}",
                packet.Username, session.SessionId, session.AccountId);
        }
        else
        {
            // 根据错误类型设置不同的状态码
            response.Status = GetStatusCodeFromError(result.Error!);
            response.Message = result.Error!;

            _logger.LogWarning(
                "登录失败: Username={Username}, Error={Error}",
                packet.Username, result.Error);
        }

        // 发送响应
        await context.SendPacketAsync(response);
    }

    /// <summary>
    /// 根据错误信息映射状态码
    /// </summary>
    private byte GetStatusCodeFromError(string error)
    {
        if (error.Contains("封禁"))
            return 2; // 账号已封禁

        if (error.Contains("繁忙"))
            return 3; // 服务器繁忙

        return 1; // 用户名或密码错误
    }
}

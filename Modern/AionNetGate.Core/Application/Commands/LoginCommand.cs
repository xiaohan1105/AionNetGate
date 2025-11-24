using AionNetGate.Core.Common;
using AionNetGate.Core.Domain;
using MediatR;

namespace AionNetGate.Core.Application.Commands;

/// <summary>
/// 登录Command
/// 使用CQRS模式封装登录操作
/// </summary>
public class LoginCommand : IRequest<Result<Session>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? HardwareId { get; set; }
}

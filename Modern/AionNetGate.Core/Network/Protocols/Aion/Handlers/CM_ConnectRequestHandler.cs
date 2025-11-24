using Microsoft.Extensions.Logging;

namespace AionNetGate.Core.Network.Protocols.Aion.Handlers;

/// <summary>
/// 处理客户端连接请求
/// </summary>
public class CM_ConnectRequestHandler : IPacketHandler<CM_ConnectRequest>
{
    private readonly ILogger<CM_ConnectRequestHandler> _logger;

    public CM_ConnectRequestHandler(ILogger<CM_ConnectRequestHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(CM_ConnectRequest packet, IConnectionContext context)
    {
        _logger.LogInformation(
            "收到连接请求: ClientVersion={ClientVersion}, HardwareId={HardwareId}, Mac={Mac}, ConnectionId={ConnectionId}",
            packet.ClientVersion, packet.HardwareId, packet.MacAddress, context.ConnectionId);

        // TODO: 在这里进行版本检查、黑名单检查等

        // 示例：检查客户端版本
        if (packet.ClientVersion != "1.0.0")
        {
            _logger.LogWarning(
                "客户端版本不匹配: Expected=1.0.0, Actual={ClientVersion}, ConnectionId={ConnectionId}",
                packet.ClientVersion, context.ConnectionId);

            // 发送失败响应
            var errorResponse = new SM_ConnectFinished
            {
                ServerVersion = "1.0.0",
                ConnectionId = context.ConnectionId,
                Status = 1, // 版本不匹配
                Message = "客户端版本不匹配，请更新客户端"
            };

            await context.SendPacketAsync(errorResponse);
            await context.DisconnectAsync("版本不匹配");
            return;
        }

        // 保存客户端信息到连接属性
        context.Properties["ClientVersion"] = packet.ClientVersion;
        context.Properties["HardwareId"] = packet.HardwareId;
        context.Properties["MacAddress"] = packet.MacAddress;

        // 发送成功响应
        var response = new SM_ConnectFinished
        {
            ServerVersion = "1.0.0",
            ConnectionId = context.ConnectionId,
            Status = 0, // 成功
            Message = "连接成功"
        };

        await context.SendPacketAsync(response);

        _logger.LogInformation(
            "连接请求已接受: ConnectionId={ConnectionId}",
            context.ConnectionId);
    }
}

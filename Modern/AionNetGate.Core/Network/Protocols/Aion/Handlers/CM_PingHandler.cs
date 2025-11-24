using Microsoft.Extensions.Logging;

namespace AionNetGate.Core.Network.Protocols.Aion.Handlers;

/// <summary>
/// 处理客户端Ping请求（心跳）
/// </summary>
public class CM_PingHandler : IPacketHandler<CM_Ping>
{
    private readonly ILogger<CM_PingHandler> _logger;

    public CM_PingHandler(ILogger<CM_PingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(CM_Ping packet, IConnectionContext context)
    {
        _logger.LogTrace(
            "收到Ping: Timestamp={Timestamp}, ConnectionId={ConnectionId}",
            packet.Timestamp, context.ConnectionId);

        // 更新连接活动时间（已在PacketProcessor中自动调用）
        // context.UpdateActivity();

        // 发送Pong响应
        var response = new SM_Pong
        {
            Timestamp = packet.Timestamp // 回显客户端时间戳
        };

        await context.SendPacketAsync(response);

        _logger.LogTrace(
            "已发送Pong: Timestamp={Timestamp}, ConnectionId={ConnectionId}",
            response.Timestamp, context.ConnectionId);
    }
}

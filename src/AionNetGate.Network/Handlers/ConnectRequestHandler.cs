using AionNetGate.Network.Packets;
using AionNetGate.Network.Packets.Client;
using AionNetGate.Network.Packets.Server;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers;

/// <summary>
/// 连接请求 Handler
/// </summary>
public class ConnectRequestHandler : IPacketHandler<CM_ConnectRequest>
{
    private readonly ILogger<ConnectRequestHandler> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ConnectRequestHandler(ILogger<ConnectRequestHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task HandleAsync(CM_ConnectRequest packet, IClientConnection connection, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "收到连接请求: ConnectionId={ConnectionId}, HardwareId={HardwareId}, ClientVersion={ClientVersion}",
            connection.ConnectionId,
            packet.HardwareId,
            packet.ClientVersion);

        // TODO: 验证硬件指纹
        // TODO: 检查 IP 黑名单
        // TODO: 验证客户端版本

        // 发送连接响应
        var response = new SM_ConnectResponse
        {
            Success = true,
            Message = "连接成功",
            ServerTime = DateTime.UtcNow,
            HeartbeatInterval = 30
        };

        await connection.SendPacketAsync(response, cancellationToken);

        _logger.LogInformation("连接请求处理完成: {ConnectionId}", connection.ConnectionId);
    }
}

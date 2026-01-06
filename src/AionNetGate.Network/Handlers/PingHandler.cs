using AionNetGate.Network.Packets.Client;
using AionNetGate.Network.Packets.Server;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers;

/// <summary>
/// Ping Handler
/// </summary>
public class PingHandler : IPacketHandler<CM_Ping>
{
    private readonly ILogger<PingHandler> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PingHandler(ILogger<PingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task HandleAsync(CM_Ping packet, IClientConnection connection, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("收到 Ping: ConnectionId={ConnectionId}, Timestamp={Timestamp}",
            connection.ConnectionId, packet.ClientTimestamp);

        // 发送 Pong 响应
        var response = new SM_Pong
        {
            ClientTimestamp = packet.ClientTimestamp,
            ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await connection.SendPacketAsync(response, cancellationToken);
    }
}

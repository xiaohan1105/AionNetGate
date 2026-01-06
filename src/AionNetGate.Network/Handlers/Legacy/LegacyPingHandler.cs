using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// Ping 处理器 - 兼容老协议
/// 对应老项目 CM_PING
/// </summary>
/// <param name="logger">日志记录器</param>
public class LegacyPingHandler(ILogger<LegacyPingHandler> logger) : ILegacyPacketHandler
{
    public byte Opcode => Opcodes.CM_PING;

    public async ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 老协议 Ping 格式: [ClientTimestamp:long]
            var clientTimestamp = reader.ReadInt64();

            logger.LogDebug(
                "收到 Ping: SessionId={SessionId}, ClientTimestamp={Timestamp}",
                session.SessionId, clientTimestamp);

            // 发送 Pong 响应
            await session.SendPacketAsync(Opcodes.SM_PONG, writer =>
            {
                // Pong 格式: [ClientTimestamp:long] [ServerTimestamp:long]
                writer.WriteInt64(clientTimestamp);
                writer.WriteInt64(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "处理 Ping 失败: SessionId={SessionId}", session.SessionId);
        }
    }
}

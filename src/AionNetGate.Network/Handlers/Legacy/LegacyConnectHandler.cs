using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 连接请求处理器 - 兼容老协议
/// 对应老项目 CM_CONNECT
/// </summary>
/// <param name="logger">日志记录器</param>
public class LegacyConnectHandler(ILogger<LegacyConnectHandler> logger) : ILegacyPacketHandler
{
    public byte Opcode => Opcodes.CM_CONNECT;

    public async ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            // 创建读取器来解析数据
            var reader = new PacketReader(payload.Span);

            // 读取连接请求数据（与老协议格式一致）
            // 老协议: [ComputerName:string] [HardwareId:string] [LauncherVersion:string]
            var computerName = reader.ReadString();
            var hardwareId = reader.ReadString();
            var launcherVersion = reader.ReadString();

            // 更新会话信息
            session.ComputerName = computerName;
            session.HardwareId = hardwareId;
            session.LauncherVersion = launcherVersion;
            session.IsHandshakeCompleted = true;

            logger.LogInformation(
                "连接握手成功: SessionId={SessionId}, Computer={Computer}, HardwareId={HardwareId}, Version={Version}",
                session.SessionId, computerName, hardwareId, launcherVersion);

            // 发送连接确认响应
            await SendConnectResponseAsync(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理连接请求失败: SessionId={SessionId}", session.SessionId);
            await session.DisconnectAsync();
        }
    }

    /// <summary>
    /// 发送连接响应
    /// </summary>
    private async ValueTask SendConnectResponseAsync(ClientSession session)
    {
        await session.SendPacketAsync(Opcodes.SM_CONNECT, writer =>
        {
            // 响应格式: [Success:byte] [ServerTime:long] [HeartbeatInterval:int] [Message:string]
            writer.WriteByte(1); // 成功
            writer.WriteInt64(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // 服务器时间
            writer.WriteInt32(30); // 心跳间隔（秒）
            writer.WriteString("连接成功"); // 消息

            // 下发配置（与老协议一致）
            // TODO: 从配置读取外挂检测列表等
            writer.WriteString(""); // 外挂检测列表（空表示无）
        });

        logger.LogDebug("已发送连接响应: SessionId={SessionId}", session.SessionId);
    }
}

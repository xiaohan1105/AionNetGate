using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 远程电脑信息处理器 - 处理客户端上传的电脑信息
/// 对应老项目 CM_COMPUTER_INFO (0x04)
/// </summary>
/// <param name="remoteManagement">远程管理服务</param>
/// <param name="logger">日志记录器</param>
public sealed class LegacyComputerInfoHandler(
    RemoteManagementService remoteManagement,
    ILogger<LegacyComputerInfoHandler> logger) : ILegacyPacketHandler
{
    private const int MaxStringLength = 500;

    public byte Opcode => Opcodes.CM_COMPUTER_INFO;

    public ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 读取电脑信息
            var computerInfo = new RemoteComputerInfo
            {
                OsName = TruncateString(reader.ReadString()),
                SystemType = TruncateString(reader.ReadString()),
                ComputerName = TruncateString(reader.ReadString()),
                UserName = TruncateString(reader.ReadString()),
                CpuInfo = TruncateString(reader.ReadString()),
                MemoryInfo = TruncateString(reader.ReadString()),
                VideoCardInfo = TruncateString(reader.ReadString()),
                DriveInfo = TruncateString(reader.ReadString()),
                MainBoardInfo = TruncateString(reader.ReadString()),
                MacAddress = TruncateString(reader.ReadString()),
                IpAddress = TruncateString(reader.ReadString()),
                Location = TruncateString(reader.ReadString()),
                CollectedAt = DateTime.UtcNow
            };

            logger.LogInformation(
                "收到电脑信息: SessionId={SessionId}, Computer={ComputerName}, User={UserName}, OS={OS}",
                session.SessionId, computerInfo.ComputerName, computerInfo.UserName, computerInfo.OsName);

            // 更新会话的电脑名（如果还没设置）
            if (string.IsNullOrEmpty(session.ComputerName))
            {
                session.ComputerName = computerInfo.ComputerName;
            }

            // 通知服务处理
            remoteManagement.HandleComputerInfo(session.SessionId, computerInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理电脑信息失败: SessionId={SessionId}", session.SessionId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 截断过长的字符串
    /// </summary>
    private static string TruncateString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Length > MaxStringLength ? value[..MaxStringLength] : value;
    }
}

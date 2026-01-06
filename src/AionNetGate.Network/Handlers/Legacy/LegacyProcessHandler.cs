using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 远程进程处理器 - 处理客户端上传的进程列表
/// 对应老项目 CM_PROCESSES (0x03)
/// </summary>
/// <param name="remoteManagement">远程管理服务</param>
/// <param name="logger">日志记录器</param>
public sealed class LegacyProcessHandler(
    RemoteManagementService remoteManagement,
    ILogger<LegacyProcessHandler> logger) : ILegacyPacketHandler
{
    public byte Opcode => Opcodes.CM_PROCESSES;

    public ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 读取进程数量
            var processCount = reader.ReadInt32();

            // 验证数量
            if (processCount is < 0 or > RemoteProcessInfo.MaxProcessCount)
            {
                logger.LogWarning(
                    "无效的进程数量: SessionId={SessionId}, Count={Count}",
                    session.SessionId, processCount);
                return ValueTask.CompletedTask;
            }

            // 读取所有进程信息
            var processes = new List<RemoteProcessInfo>(processCount);
            for (var i = 0; i < processCount; i++)
            {
                var process = ReadProcessInfo(ref reader, session.SessionId);
                if (process is not null)
                {
                    processes.Add(process);
                }
            }

            logger.LogDebug(
                "收到进程列表: SessionId={SessionId}, Count={Count}",
                session.SessionId, processes.Count);

            // 通知服务处理
            remoteManagement.HandleProcessList(session.SessionId, processes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理进程列表失败: SessionId={SessionId}", session.SessionId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 读取单个进程信息
    /// </summary>
    private RemoteProcessInfo? ReadProcessInfo(ref PacketReader reader, int sessionId)
    {
        try
        {
            // 进程ID
            var processId = reader.ReadInt32();
            if (processId <= 0)
            {
                logger.LogWarning("无效的进程ID: SessionId={SessionId}, PID={PID}", sessionId, processId);
                return null;
            }

            // 进程名称
            var processName = reader.ReadString();
            if (string.IsNullOrWhiteSpace(processName))
                processName = "<unknown>";

            // 限制进程名长度
            if (processName.Length > 260)
                processName = processName[..260];

            // 窗口标题
            var windowTitle = reader.ReadString();
            if (windowTitle.Length > 500)
                windowTitle = windowTitle[..500];

            // 内存使用量（字节）
            var memoryUsage = reader.ReadInt64();
            if (memoryUsage < 0)
                memoryUsage = 0;

            // CPU使用率
            var cpuUsage = reader.ReadInt32() / 100.0; // 客户端传整数，转换为百分比
            cpuUsage = Math.Clamp(cpuUsage, 0, 100);

            // 进程路径
            var filePath = reader.ReadString();
            if (filePath.Length > 500)
                filePath = filePath[..500];

            // 图标数据（可选）
            byte[]? icon = null;
            var hasIcon = reader.ReadBoolean();
            if (hasIcon)
            {
                var iconLength = reader.ReadInt32();
                if (iconLength is > 0 and <= RemoteProcessInfo.MaxIconSize)
                {
                    icon = reader.ReadBytes(iconLength);
                }
                else if (iconLength > RemoteProcessInfo.MaxIconSize)
                {
                    // 跳过过大的图标数据
                    reader.Skip(iconLength);
                    logger.LogWarning(
                        "进程图标过大，已跳过: SessionId={SessionId}, PID={PID}, IconSize={Size}",
                        sessionId, processId, iconLength);
                }
            }

            return new RemoteProcessInfo
            {
                ProcessId = processId,
                ProcessName = processName,
                WindowTitle = windowTitle,
                MemoryUsage = memoryUsage,
                CpuUsage = cpuUsage,
                FilePath = filePath,
                Icon = icon
            };
        }
        catch (EndOfStreamException)
        {
            logger.LogWarning("读取进程信息时数据不足: SessionId={SessionId}", sessionId);
            return null;
        }
    }
}

using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 远程服务处理器 - 处理客户端上传的服务列表
/// 对应老项目 CM_SERVICES (0x09)
/// </summary>
/// <param name="remoteManagement">远程管理服务</param>
/// <param name="logger">日志记录器</param>
public sealed class LegacyServicesHandler(
    RemoteManagementService remoteManagement,
    ILogger<LegacyServicesHandler> logger) : ILegacyPacketHandler
{
    private const int MaxServiceCount = 10000;
    private const int MaxStringLength = 500;

    public byte Opcode => Opcodes.CM_SERVICES;

    public ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 读取操作类型
            var operationType = (ServiceOperationType)reader.ReadByte();

            // 读取操作结果
            var success = reader.ReadBoolean();
            var message = reader.ReadString();

            if (!success)
            {
                logger.LogWarning(
                    "服务操作失败: SessionId={SessionId}, Operation={Operation}, Message={Message}",
                    session.SessionId, operationType, message);
                return ValueTask.CompletedTask;
            }

            // 只有 List 操作需要解析服务列表
            if (operationType != ServiceOperationType.List)
            {
                logger.LogDebug(
                    "服务操作成功: SessionId={SessionId}, Operation={Operation}",
                    session.SessionId, operationType);
                return ValueTask.CompletedTask;
            }

            // 读取服务数量
            var serviceCount = reader.ReadInt32();

            // 验证数量
            if (serviceCount is < 0 or > MaxServiceCount)
            {
                logger.LogWarning(
                    "无效的服务数量: SessionId={SessionId}, Count={Count}",
                    session.SessionId, serviceCount);
                return ValueTask.CompletedTask;
            }

            // 读取服务列表
            var services = new List<RemoteServiceInfo>(serviceCount);
            for (var i = 0; i < serviceCount; i++)
            {
                var service = ReadServiceInfo(ref reader);
                if (service is not null)
                {
                    services.Add(service);
                }
            }

            logger.LogDebug(
                "收到服务列表: SessionId={SessionId}, Count={Count}",
                session.SessionId, services.Count);

            // 通知服务处理
            remoteManagement.HandleServiceList(session.SessionId, services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理服务列表失败: SessionId={SessionId}", session.SessionId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 读取单个服务信息
    /// </summary>
    private static RemoteServiceInfo? ReadServiceInfo(ref PacketReader reader)
    {
        try
        {
            var serviceName = reader.ReadString();
            if (string.IsNullOrWhiteSpace(serviceName))
                return null;

            var displayName = reader.ReadString();
            var status = (ServiceStatus)reader.ReadByte();
            var startType = (ServiceStartType)reader.ReadByte();
            var pathName = reader.ReadString();
            var description = reader.ReadString();

            // 截断过长的字符串
            return new RemoteServiceInfo
            {
                ServiceName = TruncateString(serviceName),
                DisplayName = TruncateString(displayName),
                Status = status,
                StartType = startType,
                PathName = TruncateString(pathName),
                Description = TruncateString(description)
            };
        }
        catch (EndOfStreamException)
        {
            return null;
        }
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

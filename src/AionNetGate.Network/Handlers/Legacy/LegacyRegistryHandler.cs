using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 远程注册表处理器 - 处理客户端上传的注册表信息
/// 对应老项目 CM_REGISTRY (0x08)
/// </summary>
/// <param name="remoteManagement">远程管理服务</param>
/// <param name="logger">日志记录器</param>
public sealed class LegacyRegistryHandler(
    RemoteManagementService remoteManagement,
    ILogger<LegacyRegistryHandler> logger) : ILegacyPacketHandler
{
    private const int MaxKeyPathLength = 500;
    private const int MaxValueDataLength = 10000;
    private const int MaxSubKeyCount = 10000;
    private const int MaxValueCount = 10000;

    public byte Opcode => Opcodes.CM_REGISTRY;

    public ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 读取操作类型
            var operationType = (RegistryOperationType)reader.ReadByte();

            // 读取操作结果
            var success = reader.ReadBoolean();
            var message = reader.ReadString();

            if (!success)
            {
                logger.LogWarning(
                    "注册表操作失败: SessionId={SessionId}, Operation={Operation}, Message={Message}",
                    session.SessionId, operationType, message);

                // 返回空结果通知失败
                remoteManagement.HandleRegistryData(session.SessionId, new RegistryEntry
                {
                    Name = "",
                    FullPath = "",
                    IsKey = false
                });
                return ValueTask.CompletedTask;
            }

            // 只有 List 操作需要解析数据
            if (operationType != RegistryOperationType.List)
            {
                logger.LogDebug(
                    "注册表操作成功: SessionId={SessionId}, Operation={Operation}",
                    session.SessionId, operationType);
                return ValueTask.CompletedTask;
            }

            // 读取注册表键信息
            var entry = ReadRegistryEntry(ref reader, session.SessionId);
            if (entry is not null)
            {
                logger.LogDebug(
                    "收到注册表数据: SessionId={SessionId}, Path={Path}, SubKeys={SubKeys}, Values={Values}",
                    session.SessionId, entry.FullPath,
                    entry.SubKeys?.Count ?? 0, entry.Values?.Count ?? 0);

                remoteManagement.HandleRegistryData(session.SessionId, entry);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理注册表响应失败: SessionId={SessionId}", session.SessionId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 读取注册表条目
    /// </summary>
    private RegistryEntry? ReadRegistryEntry(ref PacketReader reader, int sessionId)
    {
        try
        {
            // 键名和路径
            var name = reader.ReadString();
            var fullPath = reader.ReadString();

            if (name.Length > MaxKeyPathLength)
                name = name[..MaxKeyPathLength];
            if (fullPath.Length > MaxKeyPathLength)
                fullPath = fullPath[..MaxKeyPathLength];

            // 读取子键列表
            var subKeyCount = reader.ReadInt32();
            List<string>? subKeys = null;

            if (subKeyCount is > 0 and <= MaxSubKeyCount)
            {
                subKeys = new List<string>(subKeyCount);
                for (var i = 0; i < subKeyCount; i++)
                {
                    var subKeyName = reader.ReadString();
                    if (subKeyName.Length <= MaxKeyPathLength)
                    {
                        subKeys.Add(subKeyName);
                    }
                }
            }
            else if (subKeyCount > MaxSubKeyCount)
            {
                logger.LogWarning(
                    "子键数量过多: SessionId={SessionId}, Count={Count}",
                    sessionId, subKeyCount);
            }

            // 读取值列表
            var valueCount = reader.ReadInt32();
            List<RegistryEntry>? values = null;

            if (valueCount is > 0 and <= MaxValueCount)
            {
                values = new List<RegistryEntry>(valueCount);
                for (var i = 0; i < valueCount; i++)
                {
                    var value = ReadRegistryValue(ref reader);
                    if (value is not null)
                    {
                        values.Add(value);
                    }
                }
            }
            else if (valueCount > MaxValueCount)
            {
                logger.LogWarning(
                    "值数量过多: SessionId={SessionId}, Count={Count}",
                    sessionId, valueCount);
            }

            return new RegistryEntry
            {
                Name = name,
                FullPath = fullPath,
                IsKey = true,
                SubKeys = subKeys,
                Values = values
            };
        }
        catch (EndOfStreamException)
        {
            logger.LogWarning("读取注册表条目时数据不足: SessionId={SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>
    /// 读取单个注册表值
    /// </summary>
    private static RegistryEntry? ReadRegistryValue(ref PacketReader reader)
    {
        try
        {
            var valueName = reader.ReadString();
            var valueType = (RegistryValueType)reader.ReadByte();
            var valueData = reader.ReadString();

            if (valueName.Length > MaxKeyPathLength)
                valueName = valueName[..MaxKeyPathLength];
            if (valueData.Length > MaxValueDataLength)
                valueData = valueData[..MaxValueDataLength];

            return new RegistryEntry
            {
                Name = valueName,
                FullPath = "",
                IsKey = false,
                ValueType = valueType,
                ValueData = valueData
            };
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }
}

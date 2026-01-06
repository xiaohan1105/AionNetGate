using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 远程文件浏览处理器 - 处理客户端上传的文件/目录列表和操作结果
/// 对应老项目 CM_EXPLORER (0x07)
/// </summary>
/// <param name="remoteManagement">远程管理服务</param>
/// <param name="logger">日志记录器</param>
public sealed class LegacyFileExplorerHandler(
    RemoteManagementService remoteManagement,
    ILogger<LegacyFileExplorerHandler> logger) : ILegacyPacketHandler
{
    private const int MaxFileDataSize = 50 * 1024 * 1024; // 50MB

    public byte Opcode => Opcodes.CM_EXPLORER;

    public ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 读取操作类型
            var operationType = (FileOperationType)reader.ReadByte();

            // 读取操作结果状态
            var success = reader.ReadBoolean();
            var message = reader.ReadString();

            // 根据操作类型处理数据
            var result = operationType switch
            {
                FileOperationType.ShowDrives => HandleDrivesResponse(ref reader, success, message, session.SessionId),
                FileOperationType.ShowFilesAndDirs => HandleFilesResponse(ref reader, success, message, session.SessionId),
                FileOperationType.Download => HandleDownloadResponse(ref reader, success, message, session.SessionId),
                _ => HandleGenericResponse(success, message, operationType)
            };

            // 通知服务处理
            remoteManagement.HandleFileOperationResult(session.SessionId, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理文件浏览响应失败: SessionId={SessionId}", session.SessionId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 处理驱动器列表响应
    /// </summary>
    private FileOperationResult HandleDrivesResponse(ref PacketReader reader, bool success, string message, int sessionId)
    {
        if (!success)
        {
            return new FileOperationResult { Success = false, Message = message };
        }

        var driveCount = reader.ReadInt32();

        // 验证数量
        if (driveCount is < 0 or > FileSystemEntry.MaxDriveCount)
        {
            logger.LogWarning("无效的驱动器数量: SessionId={SessionId}, Count={Count}", sessionId, driveCount);
            return new FileOperationResult { Success = false, Message = "无效的驱动器数量" };
        }

        var drives = new List<DriveEntry>(driveCount);
        for (var i = 0; i < driveCount; i++)
        {
            var drive = ReadDriveEntry(ref reader);
            if (drive is not null)
            {
                drives.Add(drive);
            }
        }

        logger.LogDebug("收到驱动器列表: SessionId={SessionId}, Count={Count}", sessionId, drives.Count);

        return new FileOperationResult
        {
            Success = true,
            Message = message,
            Drives = drives
        };
    }

    /// <summary>
    /// 处理文件/目录列表响应
    /// </summary>
    private FileOperationResult HandleFilesResponse(ref PacketReader reader, bool success, string message, int sessionId)
    {
        if (!success)
        {
            return new FileOperationResult { Success = false, Message = message };
        }

        var entryCount = reader.ReadInt32();

        // 验证数量
        if (entryCount is < 0 or > FileSystemEntry.MaxEntryCount)
        {
            logger.LogWarning("无效的条目数量: SessionId={SessionId}, Count={Count}", sessionId, entryCount);
            return new FileOperationResult { Success = false, Message = "无效的条目数量" };
        }

        var entries = new List<FileSystemEntry>(entryCount);
        for (var i = 0; i < entryCount; i++)
        {
            var entry = ReadFileSystemEntry(ref reader);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        logger.LogDebug("收到文件列表: SessionId={SessionId}, Count={Count}", sessionId, entries.Count);

        return new FileOperationResult
        {
            Success = true,
            Message = message,
            Entries = entries
        };
    }

    /// <summary>
    /// 处理文件下载响应
    /// </summary>
    private FileOperationResult HandleDownloadResponse(ref PacketReader reader, bool success, string message, int sessionId)
    {
        if (!success)
        {
            return new FileOperationResult { Success = false, Message = message };
        }

        var dataLength = reader.ReadInt32();

        // 验证数据大小
        if (dataLength is < 0 or > MaxFileDataSize)
        {
            logger.LogWarning("无效的文件数据大小: SessionId={SessionId}, Size={Size}", sessionId, dataLength);
            return new FileOperationResult { Success = false, Message = "文件过大或数据无效" };
        }

        byte[]? data = null;
        if (dataLength > 0)
        {
            data = reader.ReadBytes(dataLength);
        }

        logger.LogDebug("收到文件数据: SessionId={SessionId}, Size={Size}", sessionId, dataLength);

        return new FileOperationResult
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 处理通用操作响应
    /// </summary>
    private static FileOperationResult HandleGenericResponse(bool success, string message, FileOperationType operationType)
    {
        return new FileOperationResult
        {
            Success = success,
            Message = string.IsNullOrEmpty(message)
                ? (success ? $"{operationType} 操作成功" : $"{operationType} 操作失败")
                : message
        };
    }

    /// <summary>
    /// 读取驱动器条目
    /// </summary>
    private DriveEntry? ReadDriveEntry(ref PacketReader reader)
    {
        try
        {
            var name = reader.ReadString();
            if (string.IsNullOrWhiteSpace(name) || name.Length > 10)
                return null;

            var driveType = reader.ReadString();
            var volumeLabel = reader.ReadString();
            var totalSize = reader.ReadInt64();
            var freeSpace = reader.ReadInt64();
            var fileSystem = reader.ReadString();
            var isReady = reader.ReadBoolean();

            return new DriveEntry
            {
                Name = name,
                DriveType = driveType.Length > 50 ? driveType[..50] : driveType,
                VolumeLabel = volumeLabel.Length > 100 ? volumeLabel[..100] : volumeLabel,
                TotalSize = Math.Max(0, totalSize),
                FreeSpace = Math.Max(0, freeSpace),
                FileSystem = fileSystem.Length > 20 ? fileSystem[..20] : fileSystem,
                IsReady = isReady
            };
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }

    /// <summary>
    /// 读取文件系统条目
    /// </summary>
    private FileSystemEntry? ReadFileSystemEntry(ref PacketReader reader)
    {
        try
        {
            var name = reader.ReadString();
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // 限制名称长度
            if (name.Length > 260)
                name = name[..260];

            var fullPath = reader.ReadString();
            if (fullPath.Length > 500)
                fullPath = fullPath[..500];

            var isDirectory = reader.ReadBoolean();
            var size = reader.ReadInt64();
            var lastModifiedTicks = reader.ReadInt64();
            var attributes = reader.ReadString();

            // 图标（可选）
            byte[]? icon = null;
            var hasIcon = reader.ReadBoolean();
            if (hasIcon)
            {
                var iconLength = reader.ReadInt32();
                if (iconLength is > 0 and <= 100 * 1024) // 100KB limit
                {
                    icon = reader.ReadBytes(iconLength);
                }
                else if (iconLength > 100 * 1024)
                {
                    reader.Skip(iconLength);
                }
            }

            return new FileSystemEntry
            {
                Name = name,
                FullPath = fullPath,
                IsDirectory = isDirectory,
                Size = Math.Max(0, size),
                LastModified = lastModifiedTicks > 0
                    ? DateTime.FromFileTimeUtc(lastModifiedTicks)
                    : DateTime.MinValue,
                Attributes = attributes.Length > 50 ? attributes[..50] : attributes,
                Icon = icon
            };
        }
        catch (EndOfStreamException)
        {
            return null;
        }
    }
}

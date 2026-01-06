using AionNetGate.Core.Results;
using AionNetGate.Network.Models;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Services;

/// <summary>
/// 远程管理服务实现
/// 提供对客户端机器的远程管理功能
/// </summary>
public sealed class RemoteManagementService(
    GatewayServer gatewayServer,
    ILogger<RemoteManagementService> logger) : IRemoteManagementService
{
    // 文件操作限制
    private const int MaxFileSize = 50 * 1024 * 1024; // 50MB
    private const int MaxPathLength = 260;

    #region 事件

    /// <inheritdoc/>
    public event Action<int, RemoteDesktopData>? DesktopReceived;

    /// <inheritdoc/>
    public event Action<int, IReadOnlyList<RemoteProcessInfo>>? ProcessListReceived;

    /// <inheritdoc/>
    public event Action<int, RemoteComputerInfo>? ComputerInfoReceived;

    /// <inheritdoc/>
    public event Action<int, FileOperationResult>? FileOperationResultReceived;

    /// <inheritdoc/>
    public event Action<int, RegistryEntry>? RegistryDataReceived;

    /// <inheritdoc/>
    public event Action<int, IReadOnlyList<RemoteServiceInfo>>? ServiceListReceived;

    #endregion

    #region 远程桌面

    /// <inheritdoc/>
    public async Task<Result> RequestDesktopAsync(int sessionId, int quality = 50, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        quality = Math.Clamp(quality, 1, 100);

        logger.LogInformation("请求桌面截图: SessionId={SessionId}, Quality={Quality}", sessionId, quality);

        await session.SendPacketAsync(Opcodes.SM_PICTURE, writer =>
        {
            writer.WriteByte(0); // 0 = 请求桌面
            writer.WriteByte((byte)quality);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> SendMouseEventAsync(int sessionId, int x, int y, MouseButton button, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        // 坐标验证
        if (x < 0 || y < 0 || x > RemoteDesktopData.MaxWidth || y > RemoteDesktopData.MaxHeight)
            return Result.Failure(Error.Validation("鼠标坐标超出有效范围"));

        logger.LogDebug("发送鼠标事件: SessionId={SessionId}, X={X}, Y={Y}, Button={Button}",
            sessionId, x, y, button);

        await session.SendPacketAsync(Opcodes.SM_PICTURE, writer =>
        {
            writer.WriteByte(1); // 1 = 鼠标事件
            writer.WriteInt32(x);
            writer.WriteInt32(y);
            writer.WriteByte((byte)button);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> SendKeyEventAsync(int sessionId, int keyCode, bool isKeyDown, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        // 键码验证 (Windows 虚拟键码范围 0-255)
        if (keyCode is < 0 or > 255)
            return Result.Failure(Error.Validation("键码超出有效范围 (0-255)"));

        logger.LogDebug("发送键盘事件: SessionId={SessionId}, KeyCode={KeyCode}, IsDown={IsDown}",
            sessionId, keyCode, isKeyDown);

        await session.SendPacketAsync(Opcodes.SM_PICTURE, writer =>
        {
            writer.WriteByte(2); // 2 = 键盘事件
            writer.WriteInt32(keyCode);
            writer.WriteBoolean(isKeyDown);
        });

        return Result.Success();
    }

    #endregion

    #region 进程管理

    /// <inheritdoc/>
    public async Task<Result> RequestProcessListAsync(int sessionId, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        logger.LogInformation("请求进程列表: SessionId={SessionId}", sessionId);

        await session.SendPacketAsync(Opcodes.SM_PROCESSES, writer =>
        {
            writer.WriteByte((byte)ProcessOperationType.List);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> KillProcessAsync(int sessionId, int processId, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        if (processId <= 0)
            return Result.Failure(Error.Validation("无效的进程ID"));

        logger.LogInformation("结束进程: SessionId={SessionId}, ProcessId={ProcessId}", sessionId, processId);

        await session.SendPacketAsync(Opcodes.SM_PROCESSES, writer =>
        {
            writer.WriteByte((byte)ProcessOperationType.Kill);
            writer.WriteInt32(processId);
        });

        return Result.Success();
    }

    #endregion

    #region 文件管理

    /// <inheritdoc/>
    public async Task<Result> RequestDrivesAsync(int sessionId, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        logger.LogInformation("请求驱动器列表: SessionId={SessionId}", sessionId);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.ShowDrives);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> RequestDirectoryAsync(int sessionId, string path, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(path);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogInformation("请求目录内容: SessionId={SessionId}, Path={Path}", sessionId, path);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.ShowFilesAndDirs);
            writer.WriteString(path);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> RequestFileDownloadAsync(int sessionId, string path, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(path);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogInformation("请求下载文件: SessionId={SessionId}, Path={Path}", sessionId, path);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.Download);
            writer.WriteString(path);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> UploadFileAsync(int sessionId, string remotePath, byte[] data, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(remotePath);
        if (validationResult.IsFailure)
            return validationResult;

        if (data is null || data.Length == 0)
            return Result.Failure(Error.Validation("文件数据不能为空"));

        if (data.Length > MaxFileSize)
            return Result.Failure(Error.Validation($"文件大小超出限制 ({MaxFileSize / 1024 / 1024}MB)"));

        logger.LogInformation("上传文件: SessionId={SessionId}, Path={Path}, Size={Size}",
            sessionId, remotePath, data.Length);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.Upload);
            writer.WriteString(remotePath);
            writer.WriteInt32(data.Length);
            writer.WriteBytes(data);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteFileAsync(int sessionId, string path, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(path);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogWarning("删除文件/目录: SessionId={SessionId}, Path={Path}", sessionId, path);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.Delete);
            writer.WriteString(path);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> ExecuteFileAsync(int sessionId, string path, string? arguments = null, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(path);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogWarning("执行文件: SessionId={SessionId}, Path={Path}, Args={Args}",
            sessionId, path, arguments ?? "(无)");

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.Execute);
            writer.WriteString(path);
            writer.WriteString(arguments ?? string.Empty);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> CreateFolderAsync(int sessionId, string path, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(path);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogInformation("创建文件夹: SessionId={SessionId}, Path={Path}", sessionId, path);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.NewFolder);
            writer.WriteString(path);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> RenameAsync(int sessionId, string oldPath, string newPath, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidatePath(oldPath);
        if (validationResult.IsFailure)
            return validationResult;

        validationResult = ValidatePath(newPath);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogInformation("重命名: SessionId={SessionId}, OldPath={OldPath}, NewPath={NewPath}",
            sessionId, oldPath, newPath);

        await session.SendPacketAsync(Opcodes.SM_EXPLORER, writer =>
        {
            writer.WriteByte((byte)FileOperationType.Rename);
            writer.WriteString(oldPath);
            writer.WriteString(newPath);
        });

        return Result.Success();
    }

    #endregion

    #region 电脑信息

    /// <inheritdoc/>
    public async Task<Result> RequestComputerInfoAsync(int sessionId, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        logger.LogInformation("请求电脑信息: SessionId={SessionId}", sessionId);

        await session.SendPacketAsync(Opcodes.SM_COMPUTER_INFO, _ => { });

        return Result.Success();
    }

    #endregion

    #region 注册表管理

    /// <inheritdoc/>
    public async Task<Result> RequestRegistryKeyAsync(int sessionId, string keyPath, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidateRegistryPath(keyPath);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogInformation("请求注册表键: SessionId={SessionId}, KeyPath={KeyPath}", sessionId, keyPath);

        await session.SendPacketAsync(Opcodes.SM_REGISTRY, writer =>
        {
            writer.WriteByte((byte)RegistryOperationType.List);
            writer.WriteString(keyPath);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> CreateRegistryKeyAsync(int sessionId, string keyPath, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidateRegistryPath(keyPath);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogWarning("创建注册表键: SessionId={SessionId}, KeyPath={KeyPath}", sessionId, keyPath);

        await session.SendPacketAsync(Opcodes.SM_REGISTRY, writer =>
        {
            writer.WriteByte((byte)RegistryOperationType.CreateKey);
            writer.WriteString(keyPath);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteRegistryKeyAsync(int sessionId, string keyPath, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidateRegistryPath(keyPath);
        if (validationResult.IsFailure)
            return validationResult;

        logger.LogWarning("删除注册表键: SessionId={SessionId}, KeyPath={KeyPath}", sessionId, keyPath);

        await session.SendPacketAsync(Opcodes.SM_REGISTRY, writer =>
        {
            writer.WriteByte((byte)RegistryOperationType.DeleteKey);
            writer.WriteString(keyPath);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> SetRegistryValueAsync(int sessionId, string keyPath, string valueName,
        RegistryValueType valueType, string data, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidateRegistryPath(keyPath);
        if (validationResult.IsFailure)
            return validationResult;

        if (string.IsNullOrWhiteSpace(valueName))
            return Result.Failure(Error.Validation("注册表值名称不能为空"));

        logger.LogWarning("设置注册表值: SessionId={SessionId}, KeyPath={KeyPath}, ValueName={ValueName}",
            sessionId, keyPath, valueName);

        await session.SendPacketAsync(Opcodes.SM_REGISTRY, writer =>
        {
            writer.WriteByte((byte)RegistryOperationType.SetValue);
            writer.WriteString(keyPath);
            writer.WriteString(valueName);
            writer.WriteByte((byte)valueType);
            writer.WriteString(data);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteRegistryValueAsync(int sessionId, string keyPath, string valueName, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        var validationResult = ValidateRegistryPath(keyPath);
        if (validationResult.IsFailure)
            return validationResult;

        if (string.IsNullOrWhiteSpace(valueName))
            return Result.Failure(Error.Validation("注册表值名称不能为空"));

        logger.LogWarning("删除注册表值: SessionId={SessionId}, KeyPath={KeyPath}, ValueName={ValueName}",
            sessionId, keyPath, valueName);

        await session.SendPacketAsync(Opcodes.SM_REGISTRY, writer =>
        {
            writer.WriteByte((byte)RegistryOperationType.DeleteValue);
            writer.WriteString(keyPath);
            writer.WriteString(valueName);
        });

        return Result.Success();
    }

    #endregion

    #region 服务管理

    /// <inheritdoc/>
    public async Task<Result> RequestServicesAsync(int sessionId, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        logger.LogInformation("请求服务列表: SessionId={SessionId}", sessionId);

        await session.SendPacketAsync(Opcodes.SM_SERVICES, writer =>
        {
            writer.WriteByte((byte)ServiceOperationType.List);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> StartServiceAsync(int sessionId, string serviceName, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        if (string.IsNullOrWhiteSpace(serviceName))
            return Result.Failure(Error.Validation("服务名称不能为空"));

        logger.LogInformation("启动服务: SessionId={SessionId}, ServiceName={ServiceName}", sessionId, serviceName);

        await session.SendPacketAsync(Opcodes.SM_SERVICES, writer =>
        {
            writer.WriteByte((byte)ServiceOperationType.Start);
            writer.WriteString(serviceName);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> StopServiceAsync(int sessionId, string serviceName, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        if (string.IsNullOrWhiteSpace(serviceName))
            return Result.Failure(Error.Validation("服务名称不能为空"));

        logger.LogWarning("停止服务: SessionId={SessionId}, ServiceName={ServiceName}", sessionId, serviceName);

        await session.SendPacketAsync(Opcodes.SM_SERVICES, writer =>
        {
            writer.WriteByte((byte)ServiceOperationType.Stop);
            writer.WriteString(serviceName);
        });

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> RestartServiceAsync(int sessionId, string serviceName, CancellationToken ct = default)
    {
        var session = GetSessionOrFail(sessionId);
        if (session is null)
            return Result.Failure(Error.NotFound($"会话 {sessionId} 不存在"));

        if (string.IsNullOrWhiteSpace(serviceName))
            return Result.Failure(Error.Validation("服务名称不能为空"));

        logger.LogInformation("重启服务: SessionId={SessionId}, ServiceName={ServiceName}", sessionId, serviceName);

        await session.SendPacketAsync(Opcodes.SM_SERVICES, writer =>
        {
            writer.WriteByte((byte)ServiceOperationType.Restart);
            writer.WriteString(serviceName);
        });

        return Result.Success();
    }

    #endregion

    #region 数据处理（供处理器调用）

    /// <summary>
    /// 处理收到的桌面数据
    /// </summary>
    internal void HandleDesktopData(int sessionId, RemoteDesktopData data)
    {
        logger.LogDebug("收到桌面数据: SessionId={SessionId}, Width={Width}, Height={Height}, Blocks={Blocks}",
            sessionId, data.Width, data.Height, data.Blocks.Count);
        DesktopReceived?.Invoke(sessionId, data);
    }

    /// <summary>
    /// 处理收到的进程列表
    /// </summary>
    internal void HandleProcessList(int sessionId, IReadOnlyList<RemoteProcessInfo> processes)
    {
        logger.LogDebug("收到进程列表: SessionId={SessionId}, Count={Count}", sessionId, processes.Count);
        ProcessListReceived?.Invoke(sessionId, processes);
    }

    /// <summary>
    /// 处理收到的电脑信息
    /// </summary>
    internal void HandleComputerInfo(int sessionId, RemoteComputerInfo info)
    {
        logger.LogDebug("收到电脑信息: SessionId={SessionId}, ComputerName={ComputerName}",
            sessionId, info.ComputerName);
        ComputerInfoReceived?.Invoke(sessionId, info);
    }

    /// <summary>
    /// 处理收到的文件操作结果
    /// </summary>
    internal void HandleFileOperationResult(int sessionId, FileOperationResult result)
    {
        logger.LogDebug("收到文件操作结果: SessionId={SessionId}, Success={Success}",
            sessionId, result.Success);
        FileOperationResultReceived?.Invoke(sessionId, result);
    }

    /// <summary>
    /// 处理收到的注册表数据
    /// </summary>
    internal void HandleRegistryData(int sessionId, RegistryEntry entry)
    {
        logger.LogDebug("收到注册表数据: SessionId={SessionId}, Path={Path}", sessionId, entry.FullPath);
        RegistryDataReceived?.Invoke(sessionId, entry);
    }

    /// <summary>
    /// 处理收到的服务列表
    /// </summary>
    internal void HandleServiceList(int sessionId, IReadOnlyList<RemoteServiceInfo> services)
    {
        logger.LogDebug("收到服务列表: SessionId={SessionId}, Count={Count}", sessionId, services.Count);
        ServiceListReceived?.Invoke(sessionId, services);
    }

    #endregion

    #region 私有方法

    private ClientSession? GetSessionOrFail(int sessionId)
    {
        var session = gatewayServer.GetSession(sessionId);
        if (session is null || !session.IsConnected)
        {
            logger.LogWarning("会话不存在或已断开: SessionId={SessionId}", sessionId);
            return null;
        }
        return session;
    }

    private static Result ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Result.Failure(Error.Validation("路径不能为空"));

        if (path.Length > MaxPathLength)
            return Result.Failure(Error.Validation($"路径长度超出限制 ({MaxPathLength} 字符)"));

        // 检测路径遍历攻击
        if (path.Contains(".."))
            return Result.Failure(Error.Validation("路径不能包含 '..'"));

        // 检测无效字符
        var invalidChars = new[] { '<', '>', '"', '|', '\0' };
        if (path.IndexOfAny(invalidChars) >= 0)
            return Result.Failure(Error.Validation("路径包含无效字符"));

        return Result.Success();
    }

    private static Result ValidateRegistryPath(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
            return Result.Failure(Error.Validation("注册表路径不能为空"));

        if (keyPath.Length > MaxPathLength)
            return Result.Failure(Error.Validation($"路径长度超出限制 ({MaxPathLength} 字符)"));

        // 检测有效的根键
        var validRoots = new[]
        {
            "HKEY_CLASSES_ROOT", "HKCR",
            "HKEY_CURRENT_USER", "HKCU",
            "HKEY_LOCAL_MACHINE", "HKLM",
            "HKEY_USERS", "HKU",
            "HKEY_CURRENT_CONFIG", "HKCC"
        };

        var upperPath = keyPath.ToUpperInvariant();
        var hasValidRoot = validRoots.Any(root => upperPath.StartsWith(root, StringComparison.Ordinal));
        if (!hasValidRoot)
            return Result.Failure(Error.Validation("无效的注册表根键"));

        return Result.Success();
    }

    #endregion
}

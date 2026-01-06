using AionNetGate.Core.Results;
using AionNetGate.Network.Models;
using AionNetGate.Network.Server;

namespace AionNetGate.Network.Services;

/// <summary>
/// 远程管理服务接口
/// 提供对客户端机器的远程管理功能
/// </summary>
public interface IRemoteManagementService
{
    #region 远程桌面

    /// <summary>
    /// 请求客户端桌面截图
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="quality">图像质量 (1-100)</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestDesktopAsync(int sessionId, int quality = 50, CancellationToken ct = default);

    /// <summary>
    /// 发送鼠标事件到客户端
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <param name="button">鼠标按键 (0=移动, 1=左键单击, 2=右键单击, 3=左键双击)</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> SendMouseEventAsync(int sessionId, int x, int y, MouseButton button, CancellationToken ct = default);

    /// <summary>
    /// 发送键盘事件到客户端
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="keyCode">按键码</param>
    /// <param name="isKeyDown">是否按下</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> SendKeyEventAsync(int sessionId, int keyCode, bool isKeyDown, CancellationToken ct = default);

    #endregion

    #region 进程管理

    /// <summary>
    /// 请求客户端进程列表
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestProcessListAsync(int sessionId, CancellationToken ct = default);

    /// <summary>
    /// 结束客户端进程
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="processId">进程ID</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> KillProcessAsync(int sessionId, int processId, CancellationToken ct = default);

    #endregion

    #region 文件管理

    /// <summary>
    /// 请求客户端驱动器列表
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestDrivesAsync(int sessionId, CancellationToken ct = default);

    /// <summary>
    /// 请求客户端目录内容
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="path">目录路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestDirectoryAsync(int sessionId, string path, CancellationToken ct = default);

    /// <summary>
    /// 请求下载客户端文件
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="path">文件路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestFileDownloadAsync(int sessionId, string path, CancellationToken ct = default);

    /// <summary>
    /// 上传文件到客户端
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="remotePath">远程路径</param>
    /// <param name="data">文件数据</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> UploadFileAsync(int sessionId, string remotePath, byte[] data, CancellationToken ct = default);

    /// <summary>
    /// 删除客户端文件或目录
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="path">路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> DeleteFileAsync(int sessionId, string path, CancellationToken ct = default);

    /// <summary>
    /// 执行客户端文件
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="path">文件路径</param>
    /// <param name="arguments">命令行参数</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> ExecuteFileAsync(int sessionId, string path, string? arguments = null, CancellationToken ct = default);

    /// <summary>
    /// 在客户端创建文件夹
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="path">文件夹路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> CreateFolderAsync(int sessionId, string path, CancellationToken ct = default);

    /// <summary>
    /// 重命名客户端文件或目录
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="oldPath">原路径</param>
    /// <param name="newPath">新路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RenameAsync(int sessionId, string oldPath, string newPath, CancellationToken ct = default);

    #endregion

    #region 电脑信息

    /// <summary>
    /// 请求客户端电脑信息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestComputerInfoAsync(int sessionId, CancellationToken ct = default);

    #endregion

    #region 注册表管理

    /// <summary>
    /// 请求客户端注册表键内容
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="keyPath">注册表路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestRegistryKeyAsync(int sessionId, string keyPath, CancellationToken ct = default);

    /// <summary>
    /// 在客户端创建注册表键
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="keyPath">注册表路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> CreateRegistryKeyAsync(int sessionId, string keyPath, CancellationToken ct = default);

    /// <summary>
    /// 删除客户端注册表键
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="keyPath">注册表路径</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> DeleteRegistryKeyAsync(int sessionId, string keyPath, CancellationToken ct = default);

    /// <summary>
    /// 设置客户端注册表值
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="keyPath">注册表路径</param>
    /// <param name="valueName">值名称</param>
    /// <param name="valueType">值类型</param>
    /// <param name="data">值数据</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> SetRegistryValueAsync(int sessionId, string keyPath, string valueName,
        RegistryValueType valueType, string data, CancellationToken ct = default);

    /// <summary>
    /// 删除客户端注册表值
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="keyPath">注册表路径</param>
    /// <param name="valueName">值名称</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> DeleteRegistryValueAsync(int sessionId, string keyPath, string valueName, CancellationToken ct = default);

    #endregion

    #region 服务管理

    /// <summary>
    /// 请求客户端服务列表
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RequestServicesAsync(int sessionId, CancellationToken ct = default);

    /// <summary>
    /// 启动客户端服务
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="serviceName">服务名称</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> StartServiceAsync(int sessionId, string serviceName, CancellationToken ct = default);

    /// <summary>
    /// 停止客户端服务
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="serviceName">服务名称</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> StopServiceAsync(int sessionId, string serviceName, CancellationToken ct = default);

    /// <summary>
    /// 重启客户端服务
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="serviceName">服务名称</param>
    /// <param name="ct">取消令牌</param>
    Task<Result> RestartServiceAsync(int sessionId, string serviceName, CancellationToken ct = default);

    #endregion

    #region 事件

    /// <summary>
    /// 收到桌面数据事件
    /// </summary>
    event Action<int, RemoteDesktopData>? DesktopReceived;

    /// <summary>
    /// 收到进程列表事件
    /// </summary>
    event Action<int, IReadOnlyList<RemoteProcessInfo>>? ProcessListReceived;

    /// <summary>
    /// 收到电脑信息事件
    /// </summary>
    event Action<int, RemoteComputerInfo>? ComputerInfoReceived;

    /// <summary>
    /// 收到文件操作结果事件
    /// </summary>
    event Action<int, FileOperationResult>? FileOperationResultReceived;

    /// <summary>
    /// 收到注册表数据事件
    /// </summary>
    event Action<int, RegistryEntry>? RegistryDataReceived;

    /// <summary>
    /// 收到服务列表事件
    /// </summary>
    event Action<int, IReadOnlyList<RemoteServiceInfo>>? ServiceListReceived;

    #endregion
}

/// <summary>
/// 鼠标按键类型
/// </summary>
public enum MouseButton : byte
{
    /// <summary>移动</summary>
    Move = 0,
    /// <summary>左键单击</summary>
    LeftClick = 1,
    /// <summary>右键单击</summary>
    RightClick = 2,
    /// <summary>左键双击</summary>
    DoubleClick = 3
}

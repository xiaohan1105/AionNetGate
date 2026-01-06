using AionNetGate.Admin.WPF.Models;

namespace AionNetGate.Admin.WPF.Services;

/// <summary>
/// 后端通信服务接口
/// </summary>
public interface IBackendCommunicationService
{
    /// <summary>
    /// 配置网关服务器地址
    /// </summary>
    void Configure(string host, int managementPort);

    /// <summary>
    /// 检查网关服务器是否在线
    /// </summary>
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取网关健康状态
    /// </summary>
    Task<GatewayHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取在线客户端列表
    /// </summary>
    Task<IEnumerable<ClientConnectionDto>> GetOnlineClientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开指定客户端连接
    /// </summary>
    Task DisconnectClientAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 请求客户端远程桌面截图
    /// </summary>
    Task<byte[]?> RequestDesktopScreenshotAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取客户端进程列表
    /// </summary>
    Task<IEnumerable<ProcessInfo>> GetClientProcessesAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 结束客户端进程
    /// </summary>
    Task KillClientProcessAsync(string connectionId, int processId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取客户端文件列表
    /// </summary>
    Task<IEnumerable<FileInfo>> GetClientFilesAsync(string connectionId, string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载客户端文件
    /// </summary>
    Task<byte[]?> DownloadFileAsync(string connectionId, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除客户端文件
    /// </summary>
    Task DeleteFileAsync(string connectionId, string filePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// 进程信息
/// </summary>
public class ProcessInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public long MemoryUsage { get; set; }
    public double CpuUsage { get; set; }
}

/// <summary>
/// 文件信息
/// </summary>
public class FileInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDirectory { get; set; }
}

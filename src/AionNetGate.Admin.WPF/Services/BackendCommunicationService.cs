using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using AionNetGate.Admin.WPF.Models;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.Services;

/// <summary>
/// 后端通信服务实现 - 通过HTTP API与网关服务器通信
/// </summary>
public class BackendCommunicationService : IBackendCommunicationService, IDisposable
{
    private readonly ILogger<BackendCommunicationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private string _gatewayHost = "localhost";
    private int _managementPort = 11001;

    public BackendCommunicationService(ILogger<BackendCommunicationService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// 配置网关服务器地址
    /// </summary>
    public void Configure(string host, int managementPort)
    {
        _gatewayHost = host;
        _managementPort = managementPort;
        _logger.LogInformation("网关地址已配置: {Host}:{Port}", host, managementPort);
    }

    private string BaseUrl => $"http://{_gatewayHost}:{_managementPort}";

    public async Task<IEnumerable<ClientConnectionDto>> GetOnlineClientsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("获取在线客户端列表");

        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/status", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var status = JsonSerializer.Deserialize<GatewayStatusResponse>(content, _jsonOptions);

            if (status?.Sessions == null)
                return Enumerable.Empty<ClientConnectionDto>();

            return status.Sessions.Select(s => new ClientConnectionDto
            {
                ConnectionId = s.SessionId.ToString(),
                HardwareId = s.HardwareId ?? string.Empty,
                Username = s.ComputerName ?? "未知",
                AccountId = s.AccountId ?? 0,
                RemoteAddress = s.ClientIp ?? string.Empty,
                ConnectedAt = s.ConnectedAt,
                ClientVersion = s.LauncherVersion ?? "未知",
                OsInfo = string.Empty,
                IsOnline = s.IsConnected
            }).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "无法连接到网关服务器: {BaseUrl}", BaseUrl);
            return Enumerable.Empty<ClientConnectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取在线客户端列表失败");
            return Enumerable.Empty<ClientConnectionDto>();
        }
    }

    public async Task DisconnectClientAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("断开客户端连接: {ConnectionId}", connectionId);

        try
        {
            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/api/clients/{connectionId}/disconnect",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("断开连接失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开客户端连接失败: {ConnectionId}", connectionId);
        }
    }

    public async Task<byte[]?> RequestDesktopScreenshotAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("请求远程桌面截图: {ConnectionId}", connectionId);

        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/api/clients/{connectionId}/desktop",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }

            _logger.LogWarning("获取桌面截图失败: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "请求远程桌面截图失败: {ConnectionId}", connectionId);
            return null;
        }
    }

    public async Task<IEnumerable<ProcessInfo>> GetClientProcessesAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取客户端进程列表: {ConnectionId}", connectionId);

        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/api/clients/{connectionId}/processes",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<ProcessInfo>>(content, _jsonOptions) ?? new List<ProcessInfo>();
            }

            _logger.LogWarning("获取进程列表失败: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<ProcessInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取客户端进程列表失败: {ConnectionId}", connectionId);
            return Enumerable.Empty<ProcessInfo>();
        }
    }

    public async Task KillClientProcessAsync(string connectionId, int processId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("结束客户端进程: {ConnectionId}, PID: {ProcessId}", connectionId, processId);

        try
        {
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/api/clients/{connectionId}/processes/{processId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("结束进程失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "结束客户端进程失败: {ConnectionId}, PID: {ProcessId}", connectionId, processId);
        }
    }

    public async Task<IEnumerable<FileInfo>> GetClientFilesAsync(string connectionId, string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取客户端文件列表: {ConnectionId}, Path: {Path}", connectionId, path);

        try
        {
            var encodedPath = Uri.EscapeDataString(path);
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/api/clients/{connectionId}/files?path={encodedPath}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<FileInfo>>(content, _jsonOptions) ?? new List<FileInfo>();
            }

            _logger.LogWarning("获取文件列表失败: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<FileInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取客户端文件列表失败: {ConnectionId}, Path: {Path}", connectionId, path);
            return Enumerable.Empty<FileInfo>();
        }
    }

    public async Task<byte[]?> DownloadFileAsync(string connectionId, string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("下载客户端文件: {ConnectionId}, File: {FilePath}", connectionId, filePath);

        try
        {
            var encodedPath = Uri.EscapeDataString(filePath);
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/api/clients/{connectionId}/files/download?path={encodedPath}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }

            _logger.LogWarning("下载文件失败: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载客户端文件失败: {ConnectionId}, File: {FilePath}", connectionId, filePath);
            return null;
        }
    }

    public async Task DeleteFileAsync(string connectionId, string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除客户端文件: {ConnectionId}, File: {FilePath}", connectionId, filePath);

        try
        {
            var encodedPath = Uri.EscapeDataString(filePath);
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/api/clients/{connectionId}/files?path={encodedPath}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("删除文件失败: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除客户端文件失败: {ConnectionId}, File: {FilePath}", connectionId, filePath);
        }
    }

    /// <summary>
    /// 检查网关服务器是否在线
    /// </summary>
    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/health/live", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取网关健康状态
    /// </summary>
    public async Task<GatewayHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/health", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<GatewayHealthResponse>(content, _jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取健康状态失败");
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

#region Response DTOs

/// <summary>
/// 网关状态响应
/// </summary>
public class GatewayStatusResponse
{
    public string Version { get; set; } = string.Empty;
    public bool ServerRunning { get; set; }
    public int ListenPort { get; set; }
    public int ManagementPort { get; set; }
    public ConnectionStats? Connections { get; set; }
    public List<SessionInfo>? Sessions { get; set; }
    public TimeSpan Uptime { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ConnectionStats
{
    public int Current { get; set; }
    public int Max { get; set; }
    public double UtilizationPercent { get; set; }
}

public class SessionInfo
{
    public int SessionId { get; set; }
    public string? ClientIp { get; set; }
    public long? AccountId { get; set; }
    public string? HardwareId { get; set; }
    public string? ComputerName { get; set; }
    public string? LauncherVersion { get; set; }
    public bool IsConnected { get; set; }
    public DateTime ConnectedAt { get; set; }
}

/// <summary>
/// 网关健康响应
/// </summary>
public class GatewayHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public double TotalDuration { get; set; }
    public Dictionary<string, HealthEntry>? Entries { get; set; }
}

public class HealthEntry
{
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Duration { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

#endregion

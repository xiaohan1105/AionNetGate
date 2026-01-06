using AionNetGate.Core.Configuration;
using AionNetGate.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AionNetGate.WebApi.Controllers;

/// <summary>
/// 系统设置控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 暂时简化，只需要登录即可
public class SettingsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ServerConfig _serverConfig;
    private readonly SecurityConfig _securityConfig;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        IConfiguration configuration,
        IOptions<ServerConfig> serverConfig,
        IOptions<SecurityConfig> securityConfig,
        ILogger<SettingsController> logger)
    {
        _configuration = configuration;
        _serverConfig = serverConfig.Value;
        _securityConfig = securityConfig.Value;
        _logger = logger;
    }

    /// <summary>
    /// 获取系统设置
    /// </summary>
    [HttpGet]
    public IActionResult GetSettings()
    {
        var settings = new SettingsDto
        {
            Server = new ServerSettingsDto
            {
                BindAddress = _serverConfig.BindAddress ?? "0.0.0.0",
                Port = _serverConfig.Port
            },
            Security = new SecuritySettingsDto
            {
                MaxLoginAttempts = _securityConfig.MaxLoginAttempts,
                AccountLockoutMinutes = _securityConfig.AccountLockoutMinutes,
                AccessTokenExpirationMinutes = _securityConfig.AccessTokenExpirationMinutes,
                RefreshTokenExpirationDays = _securityConfig.RefreshTokenExpirationDays
            }
        };

        return Ok(ApiResponse<SettingsDto>.Ok(settings));
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    public IActionResult GetSystemInfo()
    {
        var info = new SystemInfoDto
        {
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            MachineName = Environment.MachineName,
            OsVersion = Environment.OSVersion.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            WorkingSet = Environment.WorkingSet / 1024 / 1024, // MB
            StartTime = System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()
        };

        return Ok(ApiResponse<SystemInfoDto>.Ok(info));
    }

    /// <summary>
    /// 获取运行日志（最近100条）
    /// </summary>
    [HttpGet("logs")]
    public IActionResult GetLogs([FromQuery] int lines = 100)
    {
        lines = Math.Clamp(lines, 10, 500);

        var logPath = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logPath))
        {
            return Ok(ApiResponse<IEnumerable<string>>.Ok([], "无日志文件"));
        }

        var latestLog = Directory.GetFiles(logPath, "webapi-*.log")
            .OrderByDescending(f => f)
            .FirstOrDefault();

        if (latestLog == null)
        {
            return Ok(ApiResponse<IEnumerable<string>>.Ok([], "无日志文件"));
        }

        try
        {
            using var fs = new FileStream(latestLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            var allLines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var recentLines = allLines.TakeLast(lines).ToList();
            return Ok(ApiResponse<IEnumerable<string>>.Ok(recentLines));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取日志文件失败");
            return Ok(ApiResponse<IEnumerable<string>>.Ok([], "读取日志失败"));
        }
    }
}

public class SettingsDto
{
    public ServerSettingsDto Server { get; set; } = new();
    public SecuritySettingsDto Security { get; set; } = new();
}

public class ServerSettingsDto
{
    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 5000;
}

public class SecuritySettingsDto
{
    public int MaxLoginAttempts { get; set; } = 5;
    public int AccountLockoutMinutes { get; set; } = 30;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class SystemInfoDto
{
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public long WorkingSet { get; set; }
    public DateTime StartTime { get; set; }
}

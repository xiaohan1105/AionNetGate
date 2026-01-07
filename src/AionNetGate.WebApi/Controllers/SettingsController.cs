using System.Text.Json;
using System.Text.Json.Nodes;
using AionNetGate.Core.Configuration;
using AionNetGate.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AionNetGate.WebApi.Controllers;

/// <summary>
/// 日志条目
/// </summary>
public record LogEntryDto(string Time, string Level, string Message);

/// <summary>
/// 系统设置控制器 - 完整 CRUD 支持
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly IOptionsMonitor<SecurityConfig> _securityConfig;
    private readonly IOptionsMonitor<DatabaseConfig> _databaseConfig;
    private readonly IOptionsMonitor<GameDatabaseConfig> _gameDatabaseConfig;
    private readonly IOptionsMonitor<GatewayAdvancedConfig> _gatewayConfig;
    private readonly IOptionsMonitor<FirewallConfig> _firewallConfig;
    private readonly IOptionsMonitor<LauncherConfig> _launcherConfig;
    private readonly IOptionsMonitor<CheatDetectionConfig> _cheatDetectionConfig;
    private readonly IOptionsMonitor<EmailConfig> _emailConfig;
    private readonly IOptionsMonitor<LoggingConfig> _loggingConfig;
    private readonly ILogger<SettingsController> _logger;
    private readonly string _overrideFilePath;

    private const string SensitiveMask = "********";

    public SettingsController(
        IConfiguration configuration,
        IOptionsMonitor<ServerConfig> serverConfig,
        IOptionsMonitor<SecurityConfig> securityConfig,
        IOptionsMonitor<DatabaseConfig> databaseConfig,
        IOptionsMonitor<GameDatabaseConfig> gameDatabaseConfig,
        IOptionsMonitor<GatewayAdvancedConfig> gatewayConfig,
        IOptionsMonitor<FirewallConfig> firewallConfig,
        IOptionsMonitor<LauncherConfig> launcherConfig,
        IOptionsMonitor<CheatDetectionConfig> cheatDetectionConfig,
        IOptionsMonitor<EmailConfig> emailConfig,
        IOptionsMonitor<LoggingConfig> loggingConfig,
        ILogger<SettingsController> logger)
    {
        _configuration = configuration;
        _serverConfig = serverConfig;
        _securityConfig = securityConfig;
        _databaseConfig = databaseConfig;
        _gameDatabaseConfig = gameDatabaseConfig;
        _gatewayConfig = gatewayConfig;
        _firewallConfig = firewallConfig;
        _launcherConfig = launcherConfig;
        _cheatDetectionConfig = cheatDetectionConfig;
        _emailConfig = emailConfig;
        _loggingConfig = loggingConfig;
        _logger = logger;
        _overrideFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.override.json");
    }

    /// <summary>
    /// 获取所有配置分类
    /// </summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = new[]
        {
            new { id = "server", name = "服务器配置", description = "网络监听、连接管理" },
            new { id = "security", name = "安全配置", description = "认证、加密、访问控制" },
            new { id = "database", name = "数据库配置", description = "网关数据库连接" },
            new { id = "gameDatabase", name = "游戏数据库配置", description = "游戏服务器数据库" },
            new { id = "gateway", name = "网关高级配置", description = "双线支持、自动重启" },
            new { id = "firewall", name = "防火墙配置", description = "IP黑白名单、攻击防护" },
            new { id = "launcher", name = "启动器配置", description = "客户端启动器设置" },
            new { id = "cheatDetection", name = "外挂检测配置", description = "反作弊系统" },
            new { id = "email", name = "邮件配置", description = "SMTP 通知设置" },
            new { id = "logging", name = "日志配置", description = "日志级别和输出" }
        };

        return Ok(ApiResponse<object>.Ok(categories));
    }

    /// <summary>
    /// 获取所有配置
    /// </summary>
    [HttpGet]
    public IActionResult GetAllSettings()
    {
        var settings = new
        {
            Server = MapServerConfig(_serverConfig.CurrentValue),
            Security = MaskSensitiveFields(MapSecurityConfig(_securityConfig.CurrentValue)),
            Database = MaskSensitiveFields(MapDatabaseConfig(_databaseConfig.CurrentValue)),
            GameDatabase = MaskSensitiveFields(MapGameDatabaseConfig(_gameDatabaseConfig.CurrentValue)),
            Gateway = MapGatewayConfig(_gatewayConfig.CurrentValue),
            Firewall = MapFirewallConfig(_firewallConfig.CurrentValue),
            Launcher = MaskSensitiveFields(MapLauncherConfig(_launcherConfig.CurrentValue)),
            CheatDetection = MapCheatDetectionConfig(_cheatDetectionConfig.CurrentValue),
            Email = MaskSensitiveFields(MapEmailConfig(_emailConfig.CurrentValue)),
            Logging = MapLoggingConfig(_loggingConfig.CurrentValue)
        };

        return Ok(ApiResponse<object>.Ok(settings));
    }

    /// <summary>
    /// 获取指定分类配置
    /// </summary>
    [HttpGet("{category}")]
    public IActionResult GetCategory(string category)
    {
        object? config = category.ToLower() switch
        {
            "server" => MapServerConfig(_serverConfig.CurrentValue),
            "security" => MaskSensitiveFields(MapSecurityConfig(_securityConfig.CurrentValue)),
            "database" => MaskSensitiveFields(MapDatabaseConfig(_databaseConfig.CurrentValue)),
            "gamedatabase" => MaskSensitiveFields(MapGameDatabaseConfig(_gameDatabaseConfig.CurrentValue)),
            "gateway" => MapGatewayConfig(_gatewayConfig.CurrentValue),
            "firewall" => MapFirewallConfig(_firewallConfig.CurrentValue),
            "launcher" => MaskSensitiveFields(MapLauncherConfig(_launcherConfig.CurrentValue)),
            "cheatdetection" => MapCheatDetectionConfig(_cheatDetectionConfig.CurrentValue),
            "email" => MaskSensitiveFields(MapEmailConfig(_emailConfig.CurrentValue)),
            "logging" => MapLoggingConfig(_loggingConfig.CurrentValue),
            _ => null
        };

        if (config == null)
            return NotFound(ApiResponse<object>.Fail($"配置分类 '{category}' 不存在"));

        return Ok(ApiResponse<object>.Ok(config));
    }

    /// <summary>
    /// 更新指定分类配置
    /// </summary>
    [HttpPut("{category}")]
    public async Task<IActionResult> UpdateCategory(string category, [FromBody] JsonElement updates)
    {
        var sectionName = category.ToLower() switch
        {
            "server" => ServerConfig.SectionName,
            "security" => SecurityConfig.SectionName,
            "database" => DatabaseConfig.SectionName,
            "gamedatabase" => GameDatabaseConfig.SectionName,
            "gateway" => GatewayAdvancedConfig.SectionName,
            "firewall" => FirewallConfig.SectionName,
            "launcher" => LauncherConfig.SectionName,
            "cheatdetection" => CheatDetectionConfig.SectionName,
            "email" => EmailConfig.SectionName,
            "logging" => LoggingConfig.SectionName,
            _ => null
        };

        if (sectionName == null)
            return NotFound(ApiResponse<object>.Fail($"配置分类 '{category}' 不存在"));

        try
        {
            await SaveToOverrideFileAsync(sectionName, updates);
            _logger.LogInformation("配置分类 {Category} 已更新", category);
            return Ok(ApiResponse<object>.Ok(null, $"配置 {category} 已保存，重启后生效"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败: {Category}", category);
            return StatusCode(500, ApiResponse<object>.Fail("保存配置失败"));
        }
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    public IActionResult GetSystemInfo()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var info = new
        {
            Version = "2.0.0",
            Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            MachineName = System.Environment.MachineName,
            OsVersion = System.Environment.OSVersion.ToString(),
            ProcessorCount = System.Environment.ProcessorCount,
            WorkingSet = process.WorkingSet64 / 1024 / 1024,
            StartTime = process.StartTime.ToUniversalTime(),
            Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime()
        };

        return Ok(ApiResponse<object>.Ok(info));
    }

    /// <summary>
    /// 获取运行日志
    /// </summary>
    [HttpGet("logs")]
    public IActionResult GetLogs([FromQuery] int lines = 100, [FromQuery] string? level = null)
    {
        lines = Math.Clamp(lines, 10, 500);

        var logPath = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logPath))
            return Ok(ApiResponse<IEnumerable<object>>.Ok([], "无日志文件"));

        var latestLog = Directory.GetFiles(logPath, "*.log")
            .OrderByDescending(f => new FileInfo(f).LastWriteTime)
            .FirstOrDefault();

        if (latestLog == null)
            return Ok(ApiResponse<IEnumerable<object>>.Ok([], "无日志文件"));

        try
        {
            using var fs = new FileStream(latestLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            var allLines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var logEntries = allLines
                .TakeLast(lines * 2)
                .Select(ParseLogLine)
                .Where(e => e != null)
                .Cast<LogEntryDto>()
                .Where(e => string.IsNullOrEmpty(level) || string.Equals(e.Level, level, StringComparison.OrdinalIgnoreCase))
                .TakeLast(lines)
                .ToList();

            return Ok(ApiResponse<IEnumerable<LogEntryDto>>.Ok(logEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取日志文件失败");
            return Ok(ApiResponse<IEnumerable<object>>.Ok([], "读取日志失败"));
        }
    }

    #region Private Methods

    private async Task SaveToOverrideFileAsync(string sectionName, JsonElement updates)
    {
        JsonObject root;

        if (System.IO.File.Exists(_overrideFilePath))
        {
            var existingJson = await System.IO.File.ReadAllTextAsync(_overrideFilePath);
            root = JsonNode.Parse(existingJson)?.AsObject() ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        // 过滤掉敏感字段的掩码值
        var filteredUpdates = FilterMaskedValues(updates);
        root[sectionName] = JsonNode.Parse(filteredUpdates.GetRawText());

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = root.ToJsonString(options);
        await System.IO.File.WriteAllTextAsync(_overrideFilePath, json);
    }

    private static JsonElement FilterMaskedValues(JsonElement element)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(element.GetRawText()) ?? [];
        var filtered = new Dictionary<string, object?>();

        foreach (var (key, value) in dict)
        {
            if (value.ValueKind == JsonValueKind.String && value.GetString() == SensitiveMask)
                continue;
            filtered[key] = value;
        }

        var json = JsonSerializer.Serialize(filtered);
        return JsonDocument.Parse(json).RootElement;
    }

    private static object MaskSensitiveFields(object config)
    {
        var json = JsonSerializer.Serialize(config);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? [];

        var sensitiveKeys = new[] { "Password", "EncryptionKey", "JwtSecretKey", "ConnectionString", "ForwarderPassword" };
        foreach (var key in dict.Keys.ToList())
        {
            if (sensitiveKeys.Any(k => key.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                var value = dict[key]?.ToString();
                if (!string.IsNullOrEmpty(value))
                    dict[key] = SensitiveMask;
            }
        }

        return dict;
    }

    private static LogEntryDto? ParseLogLine(string line)
    {
        if (line.Length < 10) return null;

        var match = System.Text.RegularExpressions.Regex.Match(line, @"\[(\d{2}:\d{2}:\d{2})\s+(\w{3})\]\s+(.+)");
        if (match.Success)
        {
            var level = match.Groups[2].Value switch
            {
                "INF" => "Info",
                "WRN" => "Warning",
                "ERR" => "Error",
                "DBG" => "Debug",
                "FTL" => "Fatal",
                _ => match.Groups[2].Value
            };
            return new LogEntryDto(match.Groups[1].Value, level, match.Groups[3].Value.Trim());
        }

        return new LogEntryDto("", "Info", line.Trim());
    }

    #endregion

    #region Mapping Methods

    private static object MapServerConfig(ServerConfig c) => new
    {
        c.BindAddress,
        c.Port,
        c.MaxConnections,
        c.ConnectionTimeout,
        c.HeartbeatInterval,
        c.ReceiveBufferSize,
        c.SendBufferSize
    };

    private static object MapSecurityConfig(SecurityConfig c) => new
    {
        c.EncryptionKey,
        c.JwtSecretKey,
        c.JwtIssuer,
        c.JwtAudience,
        c.AccessTokenExpirationMinutes,
        c.RefreshTokenExpirationDays,
        c.MaxLoginAttempts,
        c.AccountLockoutMinutes,
        c.EnableHardwareFingerprint,
        c.EnableIpBlacklist
    };

    private static object MapDatabaseConfig(DatabaseConfig c) => new
    {
        c.Provider,
        c.ConnectionString,
        c.CommandTimeout,
        c.EnableSensitiveDataLogging,
        c.EnableDetailedErrors,
        c.MaxRetryCount,
        c.MaxRetryDelay
    };

    private static object MapGameDatabaseConfig(GameDatabaseConfig c) => new
    {
        c.Enabled,
        c.GameType,
        c.Provider,
        c.ServerAddress,
        c.ServerPort,
        c.Username,
        c.Password,
        c.AccountDatabase,
        c.WorldDatabase,
        c.ConnectionTimeout,
        c.CommandTimeout
    };

    private static object MapGatewayConfig(GatewayAdvancedConfig c) => new
    {
        c.SecondaryIpAddress,
        c.EnableDualLine,
        c.EnableAutoRestart,
        c.AutoRestartIntervalMinutes,
        c.MemoryThresholdMB,
        c.EnableDebugLogging,
        c.EnableRealOnlineCount,
        c.KickPlayersWithoutLauncher
    };

    private static object MapFirewallConfig(FirewallConfig c) => new
    {
        c.Enabled,
        c.AutoAddToWhitelist,
        c.AutoBlockAttackers,
        c.ProtectedPorts,
        c.WhitelistExpirationHours,
        c.BlacklistExpirationHours,
        c.MaxConnectionsPerSecond,
        c.AttackDetectionWindowSeconds
    };

    private static object MapLauncherConfig(LauncherConfig c) => new
    {
        c.Launcher32Url,
        c.Launcher64Url,
        c.PatchUrl,
        c.WebPageUrl,
        c.LauncherName,
        c.ClientProgram,
        c.MaxLauncherCount,
        c.MaxClientCount,
        c.ForwarderPassword,
        c.LaunchParameters,
        c.EnableDynamicUpdate
    };

    private static object MapCheatDetectionConfig(CheatDetectionConfig c) => new
    {
        c.Enabled,
        c.CheckIntervalSeconds,
        c.DetectionAction,
        c.BanDurationHours,
        c.ForbiddenProcesses,
        c.ForbiddenWindowClasses,
        c.EnableMemoryScan,
        c.ReportToServer
    };

    private static object MapEmailConfig(EmailConfig c) => new
    {
        c.Enabled,
        c.SmtpServer,
        c.SmtpPort,
        c.SenderEmail,
        c.SenderName,
        c.Username,
        c.Password,
        c.EnableSsl,
        c.Timeout,
        c.AllowPasswordRecovery
    };

    private static object MapLoggingConfig(LoggingConfig c) => new
    {
        c.MinimumLevel,
        c.LogFilePath,
        c.RollingInterval,
        c.RetainedFileCountLimit,
        c.EnableConsole,
        c.EnableStructuredLogging
    };

    #endregion
}

using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.Services;

/// <summary>
/// 配置管理服务 - 负责保存和加载各种配置
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 加载配置
    /// </summary>
    T? LoadConfig<T>(string configName) where T : class;

    /// <summary>
    /// 保存配置
    /// </summary>
    void SaveConfig<T>(string configName, T config) where T : class;

    /// <summary>
    /// 配置文件目录
    /// </summary>
    string ConfigDirectory { get; }
}

/// <summary>
/// 配置管理服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string ConfigDirectory { get; }

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // 配置目录：%AppData%/AionNetGate/Admin
        ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AionNetGate", "Admin");

        if (!Directory.Exists(ConfigDirectory))
        {
            Directory.CreateDirectory(ConfigDirectory);
            _logger.LogInformation("已创建配置目录: {ConfigDirectory}", ConfigDirectory);
        }
    }

    public T? LoadConfig<T>(string configName) where T : class
    {
        var filePath = GetConfigPath(configName);

        if (!File.Exists(filePath))
        {
            _logger.LogDebug("配置文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<T>(json, _jsonOptions);
            _logger.LogDebug("已加载配置: {ConfigName}", configName);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置失败: {ConfigName}", configName);
            return null;
        }
    }

    public void SaveConfig<T>(string configName, T config) where T : class
    {
        var filePath = GetConfigPath(configName);

        try
        {
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            File.WriteAllText(filePath, json);
            _logger.LogInformation("已保存配置: {ConfigName} -> {FilePath}", configName, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败: {ConfigName}", configName);
            throw;
        }
    }

    private string GetConfigPath(string configName)
    {
        return Path.Combine(ConfigDirectory, $"{configName}.json");
    }
}

#region 配置模型

/// <summary>
/// 服务器配置
/// </summary>
public class ServerConfigModel
{
    public string GatewayHost { get; set; } = "localhost";
    public int GatewayPort { get; set; } = 10001;
    public int ManagementPort { get; set; } = 11001;
    public int MaxConnections { get; set; } = 10000;
    public int ConnectionTimeout { get; set; } = 300;
    public int HeartbeatInterval { get; set; } = 30;
    public bool AutoStart { get; set; } = false;
    public bool AutoRestart { get; set; } = false;
    public int RestartDelay { get; set; } = 5;
}

/// <summary>
/// 数据库配置
/// </summary>
public class DatabaseConfigModel
{
    public string Provider { get; set; } = "SQLite";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = "aion_netgate";
    public string Username { get; set; } = "root";
    public string Password { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

/// <summary>
/// 邮件配置
/// </summary>
public class EmailConfigModel
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "AionNetGate";
    public bool EnableNotifications { get; set; } = false;
}

/// <summary>
/// 启动器配置
/// </summary>
public class LauncherConfigModel
{
    public string ServerIp { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 10001;
    public string GamePath { get; set; } = string.Empty;
    public string LauncherVersion { get; set; } = "1.0.0";
    public string BackgroundImage { get; set; } = string.Empty;
    public bool EnableHardwareFingerprint { get; set; } = true;
    public bool EnableAutoUpdate { get; set; } = true;
    public string UpdateUrl { get; set; } = string.Empty;
    public List<string> CheatDetectionProcesses { get; set; } = new();
    public List<string> CheatDetectionMd5 { get; set; } = new();
}

#endregion

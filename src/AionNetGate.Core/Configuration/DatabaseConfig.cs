namespace AionNetGate.Core.Configuration;

/// <summary>
/// 数据库配置
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// 数据库提供程序 (MySQL, MSSQL, SQLite)
    /// </summary>
    public string Provider { get; set; } = "SQLite";

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 命令超时时间（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 是否启用敏感数据日志
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// 是否启用详细错误信息
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 最大重试延迟（秒）
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;
}

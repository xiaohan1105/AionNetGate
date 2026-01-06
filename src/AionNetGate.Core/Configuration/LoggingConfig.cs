namespace AionNetGate.Core.Configuration;

/// <summary>
/// 日志配置
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Logging";

    /// <summary>
    /// 最小日志级别
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string LogFilePath { get; set; } = "logs/aionnetgate-.log";

    /// <summary>
    /// 日志文件滚动间隔
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// 日志保留天数
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 30;

    /// <summary>
    /// 是否输出到控制台
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// 是否启用结构化日志
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;
}

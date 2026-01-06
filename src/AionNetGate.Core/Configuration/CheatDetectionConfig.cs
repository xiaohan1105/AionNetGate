namespace AionNetGate.Core.Configuration;

/// <summary>
/// 外挂检测配置
/// </summary>
public class CheatDetectionConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "CheatDetection";

    /// <summary>
    /// 是否启用外挂检测
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 检测间隔（秒）
    /// </summary>
    public int CheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 检测到外挂时的处理方式：Warn（警告）, Kick（踢出）, Ban（封禁）
    /// </summary>
    public string DetectionAction { get; set; } = "Kick";

    /// <summary>
    /// 封禁时长（小时），0 表示永久封禁
    /// </summary>
    public int BanDurationHours { get; set; } = 0;

    /// <summary>
    /// 进程名检测列表（逗号分隔）
    /// 格式: 进程名.exe
    /// </summary>
    public string ForbiddenProcesses { get; set; } = string.Empty;

    /// <summary>
    /// 进程MD5检测列表
    /// 格式: MD5值，多个用逗号分隔
    /// </summary>
    public string ForbiddenProcessMd5 { get; set; } = string.Empty;

    /// <summary>
    /// 窗口类名检测列表（逗号分隔）
    /// </summary>
    public string ForbiddenWindowClasses { get; set; } = string.Empty;

    /// <summary>
    /// 窗口标题关键字检测列表（逗号分隔）
    /// </summary>
    public string ForbiddenWindowTitles { get; set; } = string.Empty;

    /// <summary>
    /// DLL注入检测列表（逗号分隔）
    /// 格式: DLL名称.dll
    /// </summary>
    public string ForbiddenDlls { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用内存扫描
    /// </summary>
    public bool EnableMemoryScan { get; set; } = false;

    /// <summary>
    /// 是否启用驱动级检测
    /// </summary>
    public bool EnableDriverDetection { get; set; } = false;

    /// <summary>
    /// 是否上报检测结果到服务器
    /// </summary>
    public bool ReportToServer { get; set; } = true;

    /// <summary>
    /// 白名单进程（不检测的进程，逗号分隔）
    /// </summary>
    public string WhitelistProcesses { get; set; } = string.Empty;

    /// <summary>
    /// 获取禁止进程名数组
    /// </summary>
    public string[] GetForbiddenProcessesArray()
    {
        if (string.IsNullOrWhiteSpace(ForbiddenProcesses))
            return [];

        return ForbiddenProcesses
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }

    /// <summary>
    /// 获取禁止进程MD5数组
    /// </summary>
    public string[] GetForbiddenProcessMd5Array()
    {
        if (string.IsNullOrWhiteSpace(ForbiddenProcessMd5))
            return [];

        return ForbiddenProcessMd5
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().ToUpperInvariant())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }

    /// <summary>
    /// 获取禁止窗口类名数组
    /// </summary>
    public string[] GetForbiddenWindowClassesArray()
    {
        if (string.IsNullOrWhiteSpace(ForbiddenWindowClasses))
            return [];

        return ForbiddenWindowClasses
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }

    /// <summary>
    /// 获取白名单进程数组
    /// </summary>
    public string[] GetWhitelistProcessesArray()
    {
        if (string.IsNullOrWhiteSpace(WhitelistProcesses))
            return [];

        return WhitelistProcesses
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }
}

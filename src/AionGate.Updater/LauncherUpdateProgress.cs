using System.Text.Json.Serialization;

namespace AionGate.Updater;

/// <summary>
/// 登录器更新进度系统
/// 提供详细的实时进度信息，让用户清楚知道当前正在做什么
/// </summary>
public class LauncherUpdateProgress
{
    /// <summary>
    /// 更新阶段
    /// </summary>
    [JsonPropertyName("stage")]
    public UpdateStage Stage { get; set; }

    /// <summary>
    /// 阶段名称（中文）
    /// </summary>
    [JsonPropertyName("stage_name")]
    public string StageName => Stage switch
    {
        UpdateStage.CheckingVersion => "正在检查版本...",
        UpdateStage.DownloadingManifest => "正在获取更新清单...",
        UpdateStage.ComparingFiles => "正在对比本地文件...",
        UpdateStage.DownloadingFiles => "正在下载更新文件...",
        UpdateStage.ExtractingFiles => "正在解压文件...",
        UpdateStage.VerifyingFiles => "正在校验文件完整性...",
        UpdateStage.ApplyingPatch => "正在应用补丁...",
        UpdateStage.CleaningUp => "正在清理临时文件...",
        UpdateStage.Completed => "更新完成",
        UpdateStage.Failed => "更新失败",
        _ => "准备中..."
    };

    /// <summary>
    /// 当前正在处理的文件
    /// </summary>
    [JsonPropertyName("current_file")]
    public string? CurrentFile { get; set; }

    /// <summary>
    /// 当前文件进度 (0-100)
    /// </summary>
    [JsonPropertyName("current_file_progress")]
    public int CurrentFileProgress { get; set; }

    /// <summary>
    /// 总文件数
    /// </summary>
    [JsonPropertyName("total_files")]
    public int TotalFiles { get; set; }

    /// <summary>
    /// 已完成文件数
    /// </summary>
    [JsonPropertyName("completed_files")]
    public int CompletedFiles { get; set; }

    /// <summary>
    /// 总进度 (0-100)
    /// </summary>
    [JsonPropertyName("overall_progress")]
    public int OverallProgress { get; set; }

    /// <summary>
    /// 总大小（字节）
    /// </summary>
    [JsonPropertyName("total_bytes")]
    public long TotalBytes { get; set; }

    /// <summary>
    /// 已下载大小（字节）
    /// </summary>
    [JsonPropertyName("downloaded_bytes")]
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// 下载速度（字节/秒）
    /// </summary>
    [JsonPropertyName("download_speed")]
    public long DownloadSpeed { get; set; }

    /// <summary>
    /// 下载速度（格式化）
    /// </summary>
    [JsonPropertyName("download_speed_text")]
    public string DownloadSpeedText => FormatSpeed(DownloadSpeed);

    /// <summary>
    /// 剩余时间（秒）
    /// </summary>
    [JsonPropertyName("remaining_seconds")]
    public int RemainingSeconds { get; set; }

    /// <summary>
    /// 剩余时间（格式化）
    /// </summary>
    [JsonPropertyName("remaining_time_text")]
    public string RemainingTimeText => FormatTime(RemainingSeconds);

    /// <summary>
    /// 已用时间（秒）
    /// </summary>
    [JsonPropertyName("elapsed_seconds")]
    public int ElapsedSeconds { get; set; }

    /// <summary>
    /// P2P下载比例 (0-100)
    /// </summary>
    [JsonPropertyName("p2p_ratio")]
    public int P2PRatio { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 是否可以取消
    /// </summary>
    [JsonPropertyName("can_cancel")]
    public bool CanCancel { get; set; } = true;

    /// <summary>
    /// 是否可以暂停
    /// </summary>
    [JsonPropertyName("can_pause")]
    public bool CanPause { get; set; } = true;

    /// <summary>
    /// 是否已暂停
    /// </summary>
    [JsonPropertyName("is_paused")]
    public bool IsPaused { get; set; }

    /// <summary>
    /// 扩展信息（供UI显示）
    /// </summary>
    [JsonPropertyName("extra_info")]
    public string? ExtraInfo { get; set; }

    /// <summary>
    /// 格式化速度
    /// </summary>
    private static string FormatSpeed(long bytesPerSecond)
    {
        if (bytesPerSecond == 0) return "0 B/s";

        string[] sizes = { "B/s", "KB/s", "MB/s", "GB/s" };
        double speed = bytesPerSecond;
        int order = 0;

        while (speed >= 1024 && order < sizes.Length - 1)
        {
            order++;
            speed /= 1024;
        }

        return $"{speed:F2} {sizes[order]}";
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    private static string FormatTime(int seconds)
    {
        if (seconds < 0) return "计算中...";
        if (seconds == 0) return "即将完成";

        var ts = TimeSpan.FromSeconds(seconds);

        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}小时{ts.Minutes}分钟";

        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}分钟{ts.Seconds}秒";

        return $"{ts.Seconds}秒";
    }
}

/// <summary>
/// 更新阶段
/// </summary>
public enum UpdateStage
{
    /// <summary>
    /// 准备中
    /// </summary>
    Preparing = 0,

    /// <summary>
    /// 检查版本
    /// </summary>
    CheckingVersion = 1,

    /// <summary>
    /// 下载更新清单
    /// </summary>
    DownloadingManifest = 2,

    /// <summary>
    /// 对比本地文件
    /// </summary>
    ComparingFiles = 3,

    /// <summary>
    /// 下载文件
    /// </summary>
    DownloadingFiles = 4,

    /// <summary>
    /// 解压文件
    /// </summary>
    ExtractingFiles = 5,

    /// <summary>
    /// 校验文件
    /// </summary>
    VerifyingFiles = 6,

    /// <summary>
    /// 应用补丁
    /// </summary>
    ApplyingPatch = 7,

    /// <summary>
    /// 清理临时文件
    /// </summary>
    CleaningUp = 8,

    /// <summary>
    /// 完成
    /// </summary>
    Completed = 9,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = -1
}

/// <summary>
/// 更新检查结果
/// </summary>
public class UpdateCheckResult
{
    /// <summary>
    /// 是否需要更新
    /// </summary>
    [JsonPropertyName("needs_update")]
    public bool NeedsUpdate { get; set; }

    /// <summary>
    /// 是否需要完整客户端
    /// </summary>
    [JsonPropertyName("needs_full_client")]
    public bool NeedsFullClient { get; set; }

    /// <summary>
    /// 当前版本
    /// </summary>
    [JsonPropertyName("current_version")]
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// 最新版本
    /// </summary>
    [JsonPropertyName("latest_version")]
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>
    /// 更新类型
    /// </summary>
    [JsonPropertyName("update_type")]
    public string UpdateType { get; set; } = "incremental"; // incremental|patch|hotfix

    /// <summary>
    /// 是否强制更新
    /// </summary>
    [JsonPropertyName("is_forced")]
    public bool IsForced { get; set; }

    /// <summary>
    /// 更新文件数
    /// </summary>
    [JsonPropertyName("file_count")]
    public int FileCount { get; set; }

    /// <summary>
    /// 更新大小（字节）
    /// </summary>
    [JsonPropertyName("download_size")]
    public long DownloadSize { get; set; }

    /// <summary>
    /// 更新大小（格式化）
    /// </summary>
    [JsonPropertyName("download_size_text")]
    public string DownloadSizeText { get; set; } = string.Empty;

    /// <summary>
    /// 预计时间（秒）
    /// </summary>
    [JsonPropertyName("estimated_time")]
    public int EstimatedTime { get; set; }

    /// <summary>
    /// 更新日志
    /// </summary>
    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }

    /// <summary>
    /// 完整客户端下载链接（如果需要）
    /// </summary>
    [JsonPropertyName("full_package_links")]
    public List<FullPackageLink>? FullPackageLinks { get; set; }
}

/// <summary>
/// 完整客户端下载链接
/// </summary>
public class FullPackageLink
{
    /// <summary>
    /// 下载类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // baidu|aliyun|thunder|115|mega

    /// <summary>
    /// 下载类型名称
    /// </summary>
    [JsonPropertyName("type_name")]
    public string TypeName => Type switch
    {
        "baidu" => "百度网盘",
        "aliyun" => "阿里云盘",
        "thunder" => "迅雷云盘",
        "115" => "115网盘",
        "mega" => "MEGA网盘",
        "direct" => "直链下载",
        _ => "其他"
    };

    /// <summary>
    /// 下载链接
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 提取码
    /// </summary>
    [JsonPropertyName("verification_code")]
    public string? VerificationCode { get; set; }

    /// <summary>
    /// 解压密码
    /// </summary>
    [JsonPropertyName("extraction_password")]
    public string? ExtractionPassword { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }

    /// <summary>
    /// 文件大小（格式化）
    /// </summary>
    [JsonPropertyName("file_size_text")]
    public string FileSizeText { get; set; } = string.Empty;

    /// <summary>
    /// 说明
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// 推荐标记
    /// </summary>
    [JsonPropertyName("is_recommended")]
    public bool IsRecommended => Priority >= 90;
}

/// <summary>
/// 更新进度回调
/// </summary>
public delegate void UpdateProgressCallback(LauncherUpdateProgress progress);

/// <summary>
/// 更新完成回调
/// </summary>
public delegate void UpdateCompletedCallback(bool success, string? errorMessage);

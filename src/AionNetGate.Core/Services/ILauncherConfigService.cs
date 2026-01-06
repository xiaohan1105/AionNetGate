using AionNetGate.Core.Configuration;
using AionNetGate.Core.Results;

namespace AionNetGate.Core.Services;

/// <summary>
/// 登录器配置服务接口
/// </summary>
public interface ILauncherConfigService
{
    /// <summary>
    /// 获取当前登录器配置
    /// </summary>
    LauncherConfig GetCurrentConfig();

    /// <summary>
    /// 获取登录器动态参数（供登录器连接时获取）
    /// </summary>
    LauncherDynamicParameters GetDynamicParameters();

    /// <summary>
    /// 更新登录器配置
    /// </summary>
    /// <param name="config">新配置</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> UpdateConfigAsync(LauncherConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成登录器（如果有对应功能）
    /// </summary>
    /// <param name="outputPath">输出路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result<string>> GenerateLauncherAsync(string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 验证登录器标识
    /// </summary>
    /// <param name="launcherId">登录器标识</param>
    /// <param name="is64Bit">是否为64位</param>
    bool ValidateLauncherId(string launcherId, bool is64Bit);

    /// <summary>
    /// 清空登录器标识
    /// </summary>
    /// <param name="is64Bit">是否为64位</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> ClearLauncherIdAsync(bool is64Bit, CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成新的登录器标识
    /// </summary>
    /// <param name="is64Bit">是否为64位</param>
    string GenerateNewLauncherId(bool is64Bit);
}

/// <summary>
/// 登录器动态参数（连接网关时获取）
/// </summary>
public record LauncherDynamicParameters
{
    /// <summary>
    /// 32位登录器下载地址
    /// </summary>
    public string Launcher32Url { get; init; } = string.Empty;

    /// <summary>
    /// 64位登录器下载地址
    /// </summary>
    public string Launcher64Url { get; init; } = string.Empty;

    /// <summary>
    /// 补丁地址
    /// </summary>
    public string PatchUrl { get; init; } = string.Empty;

    /// <summary>
    /// 网页地址
    /// </summary>
    public string WebPageUrl { get; init; } = string.Empty;

    /// <summary>
    /// 登录器名称
    /// </summary>
    public string LauncherName { get; init; } = string.Empty;

    /// <summary>
    /// 客户端程序路径
    /// </summary>
    public string ClientProgram { get; init; } = string.Empty;

    /// <summary>
    /// 允许的登录器数量
    /// </summary>
    public int MaxLauncherCount { get; init; }

    /// <summary>
    /// 允许的客户端数量
    /// </summary>
    public int MaxClientCount { get; init; }

    /// <summary>
    /// 游戏启动参数
    /// </summary>
    public string LaunchParameters { get; init; } = string.Empty;

    /// <summary>
    /// 是否禁用账号管理
    /// </summary>
    public bool DisableAccountManagement { get; init; }

    /// <summary>
    /// 外挂检测配置
    /// </summary>
    public CheatDetectionParameters? CheatDetection { get; init; }
}

/// <summary>
/// 外挂检测参数
/// </summary>
public record CheatDetectionParameters
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 检测间隔（秒）
    /// </summary>
    public int CheckIntervalSeconds { get; init; }

    /// <summary>
    /// 禁止的进程列表
    /// </summary>
    public string[] ForbiddenProcesses { get; init; } = [];

    /// <summary>
    /// 禁止的进程MD5列表
    /// </summary>
    public string[] ForbiddenProcessMd5 { get; init; } = [];

    /// <summary>
    /// 禁止的窗口类名列表
    /// </summary>
    public string[] ForbiddenWindowClasses { get; init; } = [];
}

namespace AionNetGate.Modules.LauncherBuilder.Models;

/// <summary>
/// 启动器构建配置
/// </summary>
public sealed class LauncherBuildConfig
{
    #region 基本信息

    /// <summary>
    /// 渠道代码（用于区分不同渠道的启动器，只允许字母、数字、下划线、连字符）
    /// </summary>
    public string ChannelCode { get; set; } = "default";

    /// <summary>
    /// 游戏标题
    /// </summary>
    public string GameTitle { get; set; } = "Aion Online";

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName { get; set; } = "官方服务器";

    /// <summary>
    /// 版本号（格式：X.Y.Z 或 X.Y.Z-suffix）
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    #endregion

    #region 网关配置

    /// <summary>
    /// 网关服务器地址
    /// </summary>
    public string GatewayHost { get; set; } = "127.0.0.1";

    /// <summary>
    /// 网关服务器端口（1-65535）
    /// </summary>
    public int GatewayPort { get; set; } = 10001;

    #endregion

    #region 游戏配置

    /// <summary>
    /// 游戏可执行文件路径（相对于启动器目录或绝对路径）
    /// </summary>
    public string GameExecutablePath { get; set; } = "bin64/aion.bin";

    /// <summary>
    /// 游戏工作目录（为空时使用启动器目录）
    /// </summary>
    public string GameWorkingDirectory { get; set; } = "";

    /// <summary>
    /// 游戏启动参数
    /// </summary>
    public string GameCommandLineArgs { get; set; } = "";

    /// <summary>
    /// LS端口（1-65535）
    /// </summary>
    public int LsPort { get; set; } = 2106;

    #endregion

    #region 更新配置

    /// <summary>
    /// 更新检查URL（留空则禁用更新检查）
    /// </summary>
    public string UpdateCheckUrl { get; set; } = "";

    /// <summary>
    /// 更新下载URL（留空则禁用更新下载）
    /// </summary>
    public string UpdateDownloadUrl { get; set; } = "";

    #endregion

    #region 反外挂配置

    /// <summary>
    /// 是否启用反外挂检测
    /// </summary>
    public bool AntiCheatEnabled { get; set; }

    /// <summary>
    /// 进程黑名单（外挂进程名，如 "hack.exe"）
    /// </summary>
    public List<string> ProcessBlacklist { get; set; } = [];

    /// <summary>
    /// 是否启用文件完整性检查
    /// </summary>
    public bool FileIntegrityCheck { get; set; }

    #endregion

    #region 界面配置

    /// <summary>
    /// 皮肤资源路径（包含背景图片、按钮图片等的目录）
    /// </summary>
    public string SkinPath { get; set; } = "";

    /// <summary>
    /// 是否显示内嵌浏览器
    /// </summary>
    public bool ShowWebBrowser { get; set; }

    /// <summary>
    /// 内嵌浏览器URL
    /// </summary>
    public string WebUrl { get; set; } = "";

    #endregion

    #region 构建配置

    /// <summary>
    /// 目标运行时（win-x64, win-x86, win-arm64）
    /// </summary>
    public string? TargetRuntime { get; set; } = "win-x64";

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputDirectory { get; set; } = "";

    /// <summary>
    /// 输出文件名（不含扩展名）
    /// </summary>
    public string OutputFileName { get; set; } = "AionLauncher";

    #endregion

    /// <summary>
    /// 创建配置的深拷贝
    /// </summary>
    public LauncherBuildConfig Clone() => new()
    {
        ChannelCode = ChannelCode,
        GameTitle = GameTitle,
        ServerName = ServerName,
        Version = Version,
        GatewayHost = GatewayHost,
        GatewayPort = GatewayPort,
        GameExecutablePath = GameExecutablePath,
        GameWorkingDirectory = GameWorkingDirectory,
        GameCommandLineArgs = GameCommandLineArgs,
        LsPort = LsPort,
        UpdateCheckUrl = UpdateCheckUrl,
        UpdateDownloadUrl = UpdateDownloadUrl,
        AntiCheatEnabled = AntiCheatEnabled,
        ProcessBlacklist = [..ProcessBlacklist],
        FileIntegrityCheck = FileIntegrityCheck,
        SkinPath = SkinPath,
        ShowWebBrowser = ShowWebBrowser,
        WebUrl = WebUrl,
        TargetRuntime = TargetRuntime,
        OutputDirectory = OutputDirectory,
        OutputFileName = OutputFileName
    };
}

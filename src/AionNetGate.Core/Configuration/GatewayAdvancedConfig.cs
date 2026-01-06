namespace AionNetGate.Core.Configuration;

/// <summary>
/// 网关高级配置
/// </summary>
public class GatewayAdvancedConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "GatewayAdvanced";

    /// <summary>
    /// 第二个IP地址（双线支持）
    /// </summary>
    public string SecondaryIpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用双线双IP支持
    /// </summary>
    public bool EnableDualLine { get; set; } = false;

    /// <summary>
    /// 是否启用自动重启网关
    /// </summary>
    public bool EnableAutoRestart { get; set; } = false;

    /// <summary>
    /// 自动重启间隔（分钟），0 表示不自动重启
    /// </summary>
    public int AutoRestartIntervalMinutes { get; set; } = 0;

    /// <summary>
    /// 内存超限自动重启阈值（MB），0 表示不监控
    /// </summary>
    public int MemoryThresholdMB { get; set; } = 0;

    /// <summary>
    /// 转发器自动重启间隔（分钟），0 表示不自动重启
    /// </summary>
    public int ForwarderRestartIntervalMinutes { get; set; } = 0;

    /// <summary>
    /// 是否开启调试日志
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// 是否统计游戏真实在线人数
    /// </summary>
    public bool EnableRealOnlineCount { get; set; } = false;

    /// <summary>
    /// 是否自动关联账号和角色到在线列表
    /// </summary>
    public bool AutoLinkAccountToOnlineList { get; set; } = true;

    /// <summary>
    /// 是否自动踢掉没开登录器的玩家
    /// </summary>
    public bool KickPlayersWithoutLauncher { get; set; } = false;

    /// <summary>
    /// 内存检查间隔（秒）
    /// </summary>
    public int MemoryCheckIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 运行时间显示刷新间隔（秒）
    /// </summary>
    public int RuntimeRefreshIntervalSeconds { get; set; } = 1;
}

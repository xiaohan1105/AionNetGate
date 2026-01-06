namespace AionNetGate.Core.Configuration;

/// <summary>
/// 登录器配置
/// </summary>
public class LauncherConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Launcher";

    #region 动态参数配置（登录器连接网关时读取）

    /// <summary>
    /// 32位登录器下载地址
    /// </summary>
    public string Launcher32Url { get; set; } = string.Empty;

    /// <summary>
    /// 64位登录器下载地址
    /// </summary>
    public string Launcher64Url { get; set; } = string.Empty;

    /// <summary>
    /// 32位登录器唯一标识
    /// </summary>
    public string Launcher32Id { get; set; } = string.Empty;

    /// <summary>
    /// 64位登录器唯一标识
    /// </summary>
    public string Launcher64Id { get; set; } = string.Empty;

    /// <summary>
    /// 补丁/更新文件地址
    /// </summary>
    public string PatchUrl { get; set; } = string.Empty;

    /// <summary>
    /// 登录器内嵌网页地址
    /// </summary>
    public string WebPageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 登录器名称
    /// </summary>
    public string LauncherName { get; set; } = "游戏登录器";

    /// <summary>
    /// 客户端程序路径（相对于游戏目录）
    /// </summary>
    public string ClientProgram { get; set; } = @"bin32\aion.bin";

    /// <summary>
    /// 允许的登录器数量（同一账号）
    /// </summary>
    public int MaxLauncherCount { get; set; } = 2;

    /// <summary>
    /// 允许的客户端数量（同一账号）
    /// </summary>
    public int MaxClientCount { get; set; } = 2;

    /// <summary>
    /// 转发器密码
    /// </summary>
    public string ForwarderPassword { get; set; } = string.Empty;

    /// <summary>
    /// 游戏启动参数
    /// </summary>
    public string LaunchParameters { get; set; } = "-ip:127.0.0.1 -port:2107 -cc:5 -lang:chs -noauthgg -noweb -nb";

    /// <summary>
    /// 是否启用静态参数动态更新
    /// </summary>
    public bool EnableDynamicUpdate { get; set; } = false;

    /// <summary>
    /// 是否禁用账号管理按钮
    /// </summary>
    public bool DisableAccountManagement { get; set; } = false;

    #endregion

    #region 静态参数配置（生成登录器时写入）

    /// <summary>
    /// 客户端目录文件限制配置
    /// </summary>
    public ClientDirectoryConfig ClientDirectory { get; set; } = new();

    /// <summary>
    /// 客户端文件MD5检查配置
    /// </summary>
    public ClientMd5CheckConfig Md5Check { get; set; } = new();

    /// <summary>
    /// 外挂进程查杀配置
    /// </summary>
    public CheatDetectionConfig CheatDetection { get; set; } = new();

    /// <summary>
    /// 云盾密钥（安卫士等）
    /// </summary>
    public string CloudShieldKey { get; set; } = string.Empty;

    #endregion
}

/// <summary>
/// 客户端目录文件限制配置
/// </summary>
public class ClientDirectoryConfig
{
    /// <summary>
    /// 是否启用目录限制
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 禁止存在的文件列表（相对路径，逗号分隔）
    /// </summary>
    public string ForbiddenFiles { get; set; } = string.Empty;

    /// <summary>
    /// 禁止存在的目录列表（相对路径，逗号分隔）
    /// </summary>
    public string ForbiddenDirectories { get; set; } = string.Empty;

    /// <summary>
    /// 必须存在的文件列表（相对路径，逗号分隔）
    /// </summary>
    public string RequiredFiles { get; set; } = string.Empty;
}

/// <summary>
/// 客户端文件MD5检查配置
/// </summary>
public class ClientMd5CheckConfig
{
    /// <summary>
    /// 是否启用MD5检查
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 需要检查的文件及其MD5值
    /// 格式: 文件路径=MD5值，多个用分号分隔
    /// 例如: bin32\aion.bin=ABC123;bin64\aion.bin=DEF456
    /// </summary>
    public string FileChecksums { get; set; } = string.Empty;

    /// <summary>
    /// MD5不匹配时的处理方式：Warn（警告）, Block（阻止）, Report（上报）
    /// </summary>
    public string MismatchAction { get; set; } = "Warn";
}

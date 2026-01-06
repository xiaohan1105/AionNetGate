namespace AionNetGate.Core.Configuration;

/// <summary>
/// 游戏数据库配置（用于连接游戏服务器的数据库）
/// </summary>
public class GameDatabaseConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "GameDatabase";

    /// <summary>
    /// 是否启用游戏数据库连接
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 游戏类型（如：永恒之塔、天堂2等）
    /// </summary>
    public string GameType { get; set; } = "Aion";

    /// <summary>
    /// 数据库类型：MySQL, MSSQL
    /// </summary>
    public string Provider { get; set; } = "MSSQL";

    /// <summary>
    /// 服务器地址
    /// </summary>
    public string ServerAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// 服务器端口
    /// </summary>
    public int ServerPort { get; set; } = 1433;

    /// <summary>
    /// 数据库账号
    /// </summary>
    public string Username { get; set; } = "sa";

    /// <summary>
    /// 数据库密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 账号数据库名称
    /// </summary>
    public string AccountDatabase { get; set; } = "AionAccountDB";

    /// <summary>
    /// 游戏世界/角色数据库名称
    /// </summary>
    public string WorldDatabase { get; set; } = "AionWorldLive";

    /// <summary>
    /// 字符编码
    /// </summary>
    public string Charset { get; set; } = "GB2312";

    /// <summary>
    /// 连接超时（秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// 命令超时（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 是否启用连接池
    /// </summary>
    public bool EnablePooling { get; set; } = true;

    /// <summary>
    /// 连接池最小连接数
    /// </summary>
    public int MinPoolSize { get; set; } = 1;

    /// <summary>
    /// 连接池最大连接数
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// 获取账号数据库连接字符串
    /// </summary>
    public string GetAccountConnectionString()
    {
        return Provider.ToUpper() switch
        {
            "MSSQL" => $"Server={ServerAddress},{ServerPort};Database={AccountDatabase};User Id={Username};Password={Password};Connect Timeout={ConnectionTimeout};",
            "MYSQL" => $"Server={ServerAddress};Port={ServerPort};Database={AccountDatabase};Uid={Username};Pwd={Password};Charset={Charset};Connection Timeout={ConnectionTimeout};Pooling={EnablePooling};Min Pool Size={MinPoolSize};Max Pool Size={MaxPoolSize};",
            _ => throw new NotSupportedException($"Unsupported database provider: {Provider}")
        };
    }

    /// <summary>
    /// 获取世界数据库连接字符串
    /// </summary>
    public string GetWorldConnectionString()
    {
        return Provider.ToUpper() switch
        {
            "MSSQL" => $"Server={ServerAddress},{ServerPort};Database={WorldDatabase};User Id={Username};Password={Password};Connect Timeout={ConnectionTimeout};",
            "MYSQL" => $"Server={ServerAddress};Port={ServerPort};Database={WorldDatabase};Uid={Username};Pwd={Password};Charset={Charset};Connection Timeout={ConnectionTimeout};Pooling={EnablePooling};Min Pool Size={MinPoolSize};Max Pool Size={MaxPoolSize};",
            _ => throw new NotSupportedException($"Unsupported database provider: {Provider}")
        };
    }
}

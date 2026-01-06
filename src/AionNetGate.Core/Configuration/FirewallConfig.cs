namespace AionNetGate.Core.Configuration;

/// <summary>
/// 防火墙配置
/// </summary>
public class FirewallConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Firewall";

    /// <summary>
    /// 是否启用Windows防火墙集成
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 是否自动将玩家IP添加到防火墙白名单
    /// </summary>
    public bool AutoAddToWhitelist { get; set; } = true;

    /// <summary>
    /// 是否自动将攻击IP列入黑名单
    /// </summary>
    public bool AutoBlockAttackers { get; set; } = true;

    /// <summary>
    /// 受保护的端口列表（逗号分隔）
    /// </summary>
    public string ProtectedPorts { get; set; } = "7777,10241,2106";

    /// <summary>
    /// 白名单规则名称前缀
    /// </summary>
    public string WhitelistRulePrefix { get; set; } = "AionNetGate_Whitelist_";

    /// <summary>
    /// 黑名单规则名称前缀
    /// </summary>
    public string BlacklistRulePrefix { get; set; } = "AionNetGate_Blacklist_";

    /// <summary>
    /// 白名单IP自动过期时间（小时），0 表示永不过期
    /// </summary>
    public int WhitelistExpirationHours { get; set; } = 24;

    /// <summary>
    /// 黑名单IP自动过期时间（小时），0 表示永不过期
    /// </summary>
    public int BlacklistExpirationHours { get; set; } = 0;

    /// <summary>
    /// 每秒最大连接数阈值（超过则认为是攻击）
    /// </summary>
    public int MaxConnectionsPerSecond { get; set; } = 100;

    /// <summary>
    /// 攻击检测时间窗口（秒）
    /// </summary>
    public int AttackDetectionWindowSeconds { get; set; } = 10;

    /// <summary>
    /// 获取受保护的端口数组
    /// </summary>
    public int[] GetProtectedPortsArray()
    {
        if (string.IsNullOrWhiteSpace(ProtectedPorts))
            return [];

        return ProtectedPorts
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => int.TryParse(p.Trim(), out var port) ? port : 0)
            .Where(p => p > 0)
            .ToArray();
    }
}

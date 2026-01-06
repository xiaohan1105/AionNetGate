namespace AionNetGate.Admin.WPF.Models;

/// <summary>
/// 客户端连接信息 DTO
/// </summary>
public class ClientConnectionDto
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 硬件 ID
    /// </summary>
    public string HardwareId { get; set; } = string.Empty;

    /// <summary>
    /// 账号名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 账号 ID
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// 客户端 IP 地址
    /// </summary>
    public string RemoteAddress { get; set; } = string.Empty;

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string OsInfo { get; set; } = string.Empty;

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; set; }
}

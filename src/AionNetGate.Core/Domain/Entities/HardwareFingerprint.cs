namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 硬件指纹实体（防多开/硬件封禁）
/// </summary>
public class HardwareFingerprint
{
    /// <summary>
    /// 指纹 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号 ID
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// 硬件 ID（组合指纹）
    /// </summary>
    public string HardwareId { get; set; } = string.Empty;

    /// <summary>
    /// CPU ID
    /// </summary>
    public string? CpuId { get; set; }

    /// <summary>
    /// MAC 地址
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// 主板序列号
    /// </summary>
    public string? MotherboardSerial { get; set; }

    /// <summary>
    /// 硬盘序列号
    /// </summary>
    public string? DiskSerial { get; set; }

    /// <summary>
    /// 是否被封禁
    /// </summary>
    public bool IsBanned { get; set; }

    /// <summary>
    /// 封禁原因
    /// </summary>
    public string? BanReason { get; set; }

    /// <summary>
    /// 封禁时间
    /// </summary>
    public DateTime? BannedAt { get; set; }

    /// <summary>
    /// 首次使用时间
    /// </summary>
    public DateTime FirstUsedAt { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// 导航属性：账号
    /// </summary>
    public Account Account { get; set; } = null!;
}

namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 登录历史记录实体
/// </summary>
public class LoginHistory
{
    /// <summary>
    /// 记录ID（主键）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 账号ID（外键）
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// 登录IP地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 地理位置（城市/地区）
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginAt { get; set; }

    /// <summary>
    /// 登出时间（可为空表示仍在线）
    /// </summary>
    public DateTime? LogoutAt { get; set; }

    /// <summary>
    /// 硬件ID
    /// </summary>
    public string? HardwareId { get; set; }

    /// <summary>
    /// MAC地址
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// 登录是否成功
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// 失败原因（如果登录失败）
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 获取在线时长（秒）
    /// </summary>
    public long GetOnlineDuration()
    {
        var endTime = LogoutAt ?? DateTime.UtcNow;
        return (long)(endTime - LoginAt).TotalSeconds;
    }
}

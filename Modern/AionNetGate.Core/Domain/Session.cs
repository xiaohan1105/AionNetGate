namespace AionNetGate.Core.Domain;

/// <summary>
/// 用户会话领域模型
/// 表示一个已认证的用户会话
/// </summary>
public class Session
{
    /// <summary>
    /// 会话ID (唯一标识)
    /// </summary>
    public string SessionId { get; private set; }

    /// <summary>
    /// 账号ID
    /// </summary>
    public int AccountId { get; private set; }

    /// <summary>
    /// 账号名称
    /// </summary>
    public string AccountName { get; private set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; private set; }

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    public string IpAddress { get; private set; }

    /// <summary>
    /// 硬件ID
    /// </summary>
    public string? HardwareId { get; private set; }

    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired(int timeoutMinutes)
    {
        return (DateTime.UtcNow - LastActivityAt).TotalMinutes > timeoutMinutes;
    }

    /// <summary>
    /// 更新活动时间
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    public static Session Create(
        int accountId,
        string accountName,
        string connectionId,
        string ipAddress,
        string? hardwareId = null)
    {
        return new Session
        {
            SessionId = Guid.NewGuid().ToString("N"),
            AccountId = accountId,
            AccountName = accountName,
            ConnectionId = connectionId,
            IpAddress = ipAddress,
            HardwareId = hardwareId,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };
    }

    private Session() { }
}

namespace AionNetGate.Core.Domain.Entities;

/// <summary>
/// 连接信息实体（内存中）
/// </summary>
public class ConnectionInfo
{
    /// <summary>
    /// 连接唯一ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 客户端端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 地理位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 连接建立时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// 账号名称（登录后）
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// 账号ID（登录后）
    /// </summary>
    public int? AccountId { get; set; }

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
    /// 计算机名称
    /// </summary>
    public string? ComputerName { get; set; }

    /// <summary>
    /// 是否已认证
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// 最后一次Ping时间
    /// </summary>
    public DateTime? LastPingAt { get; set; }

    /// <summary>
    /// 接收字节数
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 发送字节数
    /// </summary>
    public long BytesSent { get; set; }

    /// <summary>
    /// 接收包数量
    /// </summary>
    public long PacketsReceived { get; set; }

    /// <summary>
    /// 发送包数量
    /// </summary>
    public long PacketsSent { get; set; }

    /// <summary>
    /// 获取连接时长（秒）
    /// </summary>
    public long GetConnectionDuration()
    {
        return (long)(DateTime.UtcNow - ConnectedAt).TotalSeconds;
    }

    /// <summary>
    /// 获取空闲时间（秒）
    /// </summary>
    public long GetIdleSeconds()
    {
        return (long)(DateTime.UtcNow - LastActivityAt).TotalSeconds;
    }

    /// <summary>
    /// 判断连接是否超时（默认5分钟无活动）
    /// </summary>
    public bool IsTimeout(int timeoutSeconds = 300)
    {
        return GetIdleSeconds() > timeoutSeconds;
    }

    /// <summary>
    /// 更新活动时间
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }
}

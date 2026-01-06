namespace AionNetGate.Core.Configuration;

/// <summary>
/// 服务器配置
/// </summary>
public class ServerConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Server";

    /// <summary>
    /// 绑定地址
    /// </summary>
    public string BindAddress { get; set; } = "0.0.0.0";

    /// <summary>
    /// 监听端口
    /// </summary>
    public int Port { get; set; } = 10001;

    /// <summary>
    /// 最大连接数
    /// </summary>
    public int MaxConnections { get; set; } = 10000;

    /// <summary>
    /// 连接超时时间（秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 300;

    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    public int HeartbeatInterval { get; set; } = 30;

    /// <summary>
    /// 接收缓冲区大小（字节）
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 8192;

    /// <summary>
    /// 发送缓冲区大小（字节）
    /// </summary>
    public int SendBufferSize { get; set; } = 8192;
}

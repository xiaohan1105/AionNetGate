namespace AionNetGate.Network.Packets;

/// <summary>
/// Packet 操作码（保留原有 0x00-0x0A 编码）
/// </summary>
public enum PacketOpcode : byte
{
    /// <summary>
    /// 客户端连接请求 / 服务器连接确认
    /// </summary>
    Connect = 0x00,

    /// <summary>
    /// 账号操作请求 / 账号操作结果
    /// </summary>
    Account = 0x01,

    /// <summary>
    /// 上传桌面截图 / 请求桌面截图
    /// </summary>
    RemoteDesktop = 0x02,

    /// <summary>
    /// 上传进程列表 / 请求进程列表
    /// </summary>
    Process = 0x03,

    /// <summary>
    /// 电脑信息 / 请求电脑信息
    /// </summary>
    ComputerInfo = 0x04,

    /// <summary>
    /// Ping / Pong 心跳
    /// </summary>
    Ping = 0x05,

    /// <summary>
    /// 外挂检测信息 / 外挂配置下发
    /// </summary>
    AntiCheat = 0x06,

    /// <summary>
    /// 文件列表 / 请求文件列表
    /// </summary>
    FileSystem = 0x07,

    /// <summary>
    /// 注册表操作 / 请求注册表操作
    /// </summary>
    Registry = 0x08,

    /// <summary>
    /// 服务列表 / 请求服务列表
    /// </summary>
    Service = 0x09,

    /// <summary>
    /// 商城数据 / 商城操作
    /// </summary>
    Shop = 0x0A,

    /// <summary>
    /// 热更新 / 更新配置
    /// </summary>
    Update = 0x0B
}

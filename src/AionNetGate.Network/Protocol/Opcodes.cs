namespace AionNetGate.Network.Protocol;

/// <summary>
/// 数据包操作码定义
/// 与老项目 AionPackets.cs 保持一致
/// </summary>
public static class Opcodes
{
    // ==================== 客户端 -> 服务器 (CM_*) ====================

    /// <summary>连接请求</summary>
    public const byte CM_CONNECT = 0x00;

    /// <summary>账号操作（登录/注册/改密/找回）</summary>
    public const byte CM_ACCOUNT = 0x01;

    /// <summary>上传桌面截图</summary>
    public const byte CM_PICTURE = 0x02;

    /// <summary>上传进程列表</summary>
    public const byte CM_PROCESSES = 0x03;

    /// <summary>上传电脑信息</summary>
    public const byte CM_COMPUTER_INFO = 0x04;

    /// <summary>Ping 心跳</summary>
    public const byte CM_PING = 0x05;

    /// <summary>上传外挂检测结果</summary>
    public const byte CM_CHEAT_INFO = 0x06;

    /// <summary>上传文件列表</summary>
    public const byte CM_EXPLORER = 0x07;

    /// <summary>上传注册表信息</summary>
    public const byte CM_REGISTRY = 0x08;

    /// <summary>上传服务列表</summary>
    public const byte CM_SERVICES = 0x09;

    /// <summary>留言板请求</summary>
    public const byte CM_BULLETIN = 0x0A;

    // ==================== 服务器 -> 客户端 (SM_*) ====================

    /// <summary>连接确认 + 配置下发</summary>
    public const byte SM_CONNECT = 0x00;

    /// <summary>账号操作结果</summary>
    public const byte SM_ACCOUNT = 0x01;

    /// <summary>请求桌面截图</summary>
    public const byte SM_PICTURE = 0x02;

    /// <summary>请求进程列表</summary>
    public const byte SM_PROCESSES = 0x03;

    /// <summary>请求电脑信息</summary>
    public const byte SM_COMPUTER_INFO = 0x04;

    /// <summary>下发外挂检测配置</summary>
    public const byte SM_CHEAT_CONFIG = 0x05;

    /// <summary>Pong 心跳响应</summary>
    public const byte SM_PONG = 0x06;

    /// <summary>请求文件列表</summary>
    public const byte SM_EXPLORER = 0x07;

    /// <summary>请求注册表信息</summary>
    public const byte SM_REGISTRY = 0x08;

    /// <summary>请求服务列表</summary>
    public const byte SM_SERVICES = 0x09;

    /// <summary>留言板响应</summary>
    public const byte SM_BULLETIN = 0x0A;

    /// <summary>
    /// 获取操作码名称（用于日志）
    /// </summary>
    public static string GetName(byte opcode, bool isClientPacket)
    {
        if (isClientPacket)
        {
            return opcode switch
            {
                CM_CONNECT => "CM_CONNECT",
                CM_ACCOUNT => "CM_ACCOUNT",
                CM_PICTURE => "CM_PICTURE",
                CM_PROCESSES => "CM_PROCESSES",
                CM_COMPUTER_INFO => "CM_COMPUTER_INFO",
                CM_PING => "CM_PING",
                CM_CHEAT_INFO => "CM_CHEAT_INFO",
                CM_EXPLORER => "CM_EXPLORER",
                CM_REGISTRY => "CM_REGISTRY",
                CM_SERVICES => "CM_SERVICES",
                CM_BULLETIN => "CM_BULLETIN",
                _ => $"UNKNOWN_0x{opcode:X2}"
            };
        }
        else
        {
            return opcode switch
            {
                SM_CONNECT => "SM_CONNECT",
                SM_ACCOUNT => "SM_ACCOUNT",
                SM_PICTURE => "SM_PICTURE",
                SM_PROCESSES => "SM_PROCESSES",
                SM_COMPUTER_INFO => "SM_COMPUTER_INFO",
                SM_CHEAT_CONFIG => "SM_CHEAT_CONFIG",
                SM_PONG => "SM_PONG",
                SM_EXPLORER => "SM_EXPLORER",
                SM_REGISTRY => "SM_REGISTRY",
                SM_SERVICES => "SM_SERVICES",
                SM_BULLETIN => "SM_BULLETIN",
                _ => $"UNKNOWN_0x{opcode:X2}"
            };
        }
    }
}

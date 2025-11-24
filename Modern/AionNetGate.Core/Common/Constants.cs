namespace AionNetGate.Core.Common;

/// <summary>
/// 应用程序常量
/// </summary>
public static class Constants
{
    /// <summary>
    /// 账号相关常量
    /// </summary>
    public static class Account
    {
        /// <summary>
        /// 账号名称最小长度
        /// </summary>
        public const int NameMinLength = 4;

        /// <summary>
        /// 账号名称最大长度
        /// </summary>
        public const int NameMaxLength = 50;

        /// <summary>
        /// 密码最小长度
        /// </summary>
        public const int PasswordMinLength = 6;

        /// <summary>
        /// 密码最大长度
        /// </summary>
        public const int PasswordMaxLength = 100;

        /// <summary>
        /// 邮箱最大长度
        /// </summary>
        public const int EmailMaxLength = 254;
    }

    /// <summary>
    /// 连接相关常量
    /// </summary>
    public static class Connection
    {
        /// <summary>
        /// 默认超时时间（秒）
        /// </summary>
        public const int DefaultTimeoutSeconds = 300;

        /// <summary>
        /// Ping间隔（秒）
        /// </summary>
        public const int PingIntervalSeconds = 30;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public const int MaxConnections = 10000;

        /// <summary>
        /// 最大包大小（字节）
        /// </summary>
        public const int MaxPacketSize = 1024 * 1024; // 1MB
    }

    /// <summary>
    /// 安全相关常量
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// 最大登录失败次数
        /// </summary>
        public const int MaxLoginAttempts = 5;

        /// <summary>
        /// 登录失败锁定时间（分钟）
        /// </summary>
        public const int LockoutDurationMinutes = 30;

        /// <summary>
        /// 密码哈希迭代次数
        /// </summary>
        public const int PasswordHashIterations = 10000;

        /// <summary>
        /// 盐值长度（字节）
        /// </summary>
        public const int SaltLength = 32;
    }

    /// <summary>
    /// 网络相关常量
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// 默认服务器端口
        /// </summary>
        public const int DefaultPort = 2106;

        /// <summary>
        /// 缓冲区大小（字节）
        /// </summary>
        public const int BufferSize = 8192;

        /// <summary>
        /// 连接队列大小
        /// </summary>
        public const int ConnectionBacklog = 100;
    }

    /// <summary>
    /// 日志相关常量
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// 日志文件保留天数
        /// </summary>
        public const int LogRetentionDays = 30;

        /// <summary>
        /// 日志文件大小限制（MB）
        /// </summary>
        public const int LogFileSizeLimitMB = 100;
    }
}

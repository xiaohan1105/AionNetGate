using AionNetGate.Configs.Core;
using System;

namespace AionNetGate.Configs.Sections
{
    /// <summary>
    /// 日志配置
    /// </summary>
    public class LoggingConfig : IConfigurationSection
    {
        public string SectionName => "Logging";

        /// <summary>
        /// 是否启用详细日志
        /// </summary>
        public bool EnableVerboseLogging { get; set; } = false;

        /// <summary>
        /// 是否启用网络通讯日志
        /// </summary>
        public bool EnableNetworkLogging { get; set; } = true;

        /// <summary>
        /// 是否启用数据库日志
        /// </summary>
        public bool EnableDatabaseLogging { get; set; } = false;

        /// <summary>
        /// 日志保留天数
        /// </summary>
        public int LogRetentionDays { get; set; } = 30;

        /// <summary>
        /// 单个日志文件最大大小（MB）
        /// </summary>
        public int MaxLogFileSizeMB { get; set; } = 10;

        /// <summary>
        /// 日志目录路径
        /// </summary>
        public string LogDirectory { get; set; } = "./Logs";

        /// <summary>
        /// 是否启用性能监控日志
        /// </summary>
        public bool EnablePerformanceLogging { get; set; } = false;

        public bool IsValid()
        {
            if (LogRetentionDays < 1)
                return false;

            if (MaxLogFileSizeMB < 1)
                return false;

            if (StringHelper.IsNullOrWhiteSpace(LogDirectory))
                return false;

            return true;
        }

        public string GetValidationErrors()
        {
            var errors = new System.Text.StringBuilder();

            if (LogRetentionDays < 1)
                errors.AppendLine("日志保留天数必须大于0");

            if (MaxLogFileSizeMB < 1)
                errors.AppendLine("日志文件最大大小必须大于0MB");

            if (StringHelper.IsNullOrWhiteSpace(LogDirectory))
                errors.AppendLine("日志目录路径不能为空");

            return errors.ToString();
        }

        public void ResetToDefaults()
        {
            EnableVerboseLogging = false;
            EnableNetworkLogging = true;
            EnableDatabaseLogging = false;
            LogRetentionDays = 30;
            MaxLogFileSizeMB = 10;
            LogDirectory = "./Logs";
            EnablePerformanceLogging = false;
        }
    }
}
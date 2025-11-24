using AionNetGate.Configs.Core;
using System;
using System.Net;

namespace AionNetGate.Configs.Sections
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig : IConfigurationSection
    {
        public string SectionName => "Server";

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string IPAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; } = 10001;

        /// <summary>
        /// 是否启用双IP支持
        /// </summary>
        public bool EnableDualIP { get; set; } = false;

        /// <summary>
        /// 第二个IP地址
        /// </summary>
        public string SecondaryIPAddress { get; set; } = string.Empty;

        /// <summary>
        /// 最大并发连接数
        /// </summary>
        public int MaxConnections { get; set; } = 1000;

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        public bool IsValid()
        {
            // 验证IP地址格式
            if (!System.Net.IPAddress.TryParse(IPAddress, out _))
                return false;

            // 验证端口范围
            if (Port < 1 || Port > 65535)
                return false;

            // 如果启用双IP，验证第二个IP
            if (EnableDualIP && !System.Net.IPAddress.TryParse(SecondaryIPAddress, out _))
                return false;

            // 验证其他参数
            if (MaxConnections < 1 || ConnectionTimeout < 1)
                return false;

            return true;
        }

        public string GetValidationErrors()
        {
            var errors = new System.Text.StringBuilder();

            if (!System.Net.IPAddress.TryParse(IPAddress, out _))
                errors.AppendLine("无效的IP地址格式");

            if (Port < 1 || Port > 65535)
                errors.AppendLine("端口号必须在1-65535范围内");

            if (EnableDualIP && !System.Net.IPAddress.TryParse(SecondaryIPAddress, out _))
                errors.AppendLine("无效的第二IP地址格式");

            if (MaxConnections < 1)
                errors.AppendLine("最大连接数必须大于0");

            if (ConnectionTimeout < 1)
                errors.AppendLine("连接超时时间必须大于0秒");

            return errors.ToString();
        }

        public void ResetToDefaults()
        {
            IPAddress = "0.0.0.0";
            Port = 10001;
            EnableDualIP = false;
            SecondaryIPAddress = string.Empty;
            MaxConnections = 1000;
            ConnectionTimeout = 30;
        }
    }
}
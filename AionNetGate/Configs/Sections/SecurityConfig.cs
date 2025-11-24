using AionNetGate.Configs.Core;
using System;
using System.Collections.Generic;

namespace AionNetGate.Configs.Sections
{
    /// <summary>
    /// 安全配置
    /// </summary>
    public class SecurityConfig : IConfigurationSection
    {
        public string SectionName => "Security";

        /// <summary>
        /// 是否启用增强安全检查
        /// </summary>
        public bool EnableEnhancedSecurity { get; set; } = true;

        /// <summary>
        /// 是否自动封禁攻击IP
        /// </summary>
        public bool EnableAutoIPBan { get; set; } = true;

        /// <summary>
        /// 每分钟最大连接数限制
        /// </summary>
        public int MaxConnectionsPerMinute { get; set; } = 10;

        /// <summary>
        /// 暴力破解阈值
        /// </summary>
        public int BruteForceThreshold { get; set; } = 5;

        /// <summary>
        /// 锁定持续时间（分钟）
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;

        /// <summary>
        /// 是否启用地理位置阻止
        /// </summary>
        public bool EnableGeoBlocking { get; set; } = false;

        /// <summary>
        /// 是否启用白名单模式
        /// </summary>
        public bool EnableWhitelistMode { get; set; } = false;

        /// <summary>
        /// IP白名单
        /// </summary>
        public List<string> IPWhitelist { get; set; } = new List<string>();

        /// <summary>
        /// IP黑名单
        /// </summary>
        public List<string> IPBlacklist { get; set; } = new List<string>();

        /// <summary>
        /// 是否启用硬件指纹验证
        /// </summary>
        public bool EnableHardwareFingerprintValidation { get; set; } = true;

        public bool IsValid()
        {
            if (MaxConnectionsPerMinute < 1)
                return false;

            if (BruteForceThreshold < 1)
                return false;

            if (LockoutDurationMinutes < 1)
                return false;

            // 验证IP地址格式
            foreach (var ip in IPWhitelist)
            {
                if (!IsValidIPAddress(ip))
                    return false;
            }

            foreach (var ip in IPBlacklist)
            {
                if (!IsValidIPAddress(ip))
                    return false;
            }

            return true;
        }

        public string GetValidationErrors()
        {
            var errors = new System.Text.StringBuilder();

            if (MaxConnectionsPerMinute < 1)
                errors.AppendLine("每分钟最大连接数必须大于0");

            if (BruteForceThreshold < 1)
                errors.AppendLine("暴力破解阈值必须大于0");

            if (LockoutDurationMinutes < 1)
                errors.AppendLine("锁定持续时间必须大于0分钟");

            // 验证IP地址格式
            foreach (var ip in IPWhitelist)
            {
                if (!IsValidIPAddress(ip))
                    errors.AppendLine("白名单中包含无效IP地址: " + ip);
            }

            foreach (var ip in IPBlacklist)
            {
                if (!IsValidIPAddress(ip))
                    errors.AppendLine("黑名单中包含无效IP地址: " + ip);
            }

            return errors.ToString();
        }

        public void ResetToDefaults()
        {
            EnableEnhancedSecurity = true;
            EnableAutoIPBan = true;
            MaxConnectionsPerMinute = 10;
            BruteForceThreshold = 5;
            LockoutDurationMinutes = 15;
            EnableGeoBlocking = false;
            EnableWhitelistMode = false;
            IPWhitelist = new List<string>();
            IPBlacklist = new List<string>();
            EnableHardwareFingerprintValidation = true;
        }

        /// <summary>
        /// 验证IP地址格式
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否有效</returns>
        private bool IsValidIPAddress(string ip)
        {
            if (StringHelper.IsNullOrWhiteSpace(ip))
                return false;

            // 支持CIDR格式 (例如: 192.168.1.0/24)
            if (ip.Contains("/"))
            {
                var parts = ip.Split('/');
                if (parts.Length != 2)
                    return false;

                if (!System.Net.IPAddress.TryParse(parts[0], out _))
                    return false;

                int cidr;
                if (!int.TryParse(parts[1], out cidr) || cidr < 0 || cidr > 32)
                    return false;

                return true;
            }

            return System.Net.IPAddress.TryParse(ip, out _);
        }

        /// <summary>
        /// 检查IP是否在白名单中
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否在白名单中</returns>
        public bool IsIPWhitelisted(string ip)
        {
            if (!EnableWhitelistMode)
                return true;

            return IPWhitelist.Contains(ip) || IPWhitelist.Contains("127.0.0.1") && ip == "127.0.0.1";
        }

        /// <summary>
        /// 检查IP是否在黑名单中
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否在黑名单中</returns>
        public bool IsIPBlacklisted(string ip)
        {
            return IPBlacklist.Contains(ip);
        }
    }
}
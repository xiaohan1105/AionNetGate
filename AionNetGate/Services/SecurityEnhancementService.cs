using AionCommons.LogEngine;
using AionNetGate.Configs;
using AionNetGate.Netwok;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AionNetGate.Services
{
    /// <summary>
    /// 安全增强服务 - 提供高级安全功能
    /// </summary>
    internal class SecurityEnhancementService
    {
        private static readonly Logger Logger = LoggerFactory.getLogger();
        private static readonly Dictionary<string, int> ConnectionAttempts = new Dictionary<string, int>();
        private static readonly Dictionary<string, DateTime> LastAttemptTime = new Dictionary<string, DateTime>();
        private static readonly HashSet<string> WhitelistedIPs = new HashSet<string>();
        private static readonly HashSet<string> BlacklistedIPs = new HashSet<string>();
        private static readonly Dictionary<string, List<DateTime>> RateLimitTracker = new Dictionary<string, List<DateTime>>();

        static SecurityEnhancementService()
        {
            LoadSecurityConfiguration();
        }

        /// <summary>
        /// 加载安全配置
        /// </summary>
        private static void LoadSecurityConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "security.conf");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("WHITELIST:"))
                        {
                            WhitelistedIPs.Add(line.Substring(10).Trim());
                        }
                        else if (line.StartsWith("BLACKLIST:"))
                        {
                            BlacklistedIPs.Add(line.Substring(10).Trim());
                        }
                    }
                    Logger.info($"安全配置加载完成，白名单: {WhitelistedIPs.Count}, 黑名单: {BlacklistedIPs.Count}");
                }
            }
            catch (Exception ex)
            {
                Logger.error("加载安全配置失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 保存安全配置
        /// </summary>
        public static void SaveSecurityConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "security.conf");
                using (StreamWriter writer = new StreamWriter(configPath))
                {
                    writer.WriteLine("# 安全配置文件");
                    writer.WriteLine("# 格式: WHITELIST:IP地址 或 BLACKLIST:IP地址");
                    writer.WriteLine();

                    foreach (string ip in WhitelistedIPs)
                    {
                        writer.WriteLine($"WHITELIST:{ip}");
                    }

                    foreach (string ip in BlacklistedIPs)
                    {
                        writer.WriteLine($"BLACKLIST:{ip}");
                    }
                }
                Logger.info("安全配置保存成功");
            }
            catch (Exception ex)
            {
                Logger.error("保存安全配置失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 高级IP检查
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否允许连接</returns>
        public static bool AdvancedIPCheck(string ip)
        {
            try
            {
                // 检查白名单
                if (WhitelistedIPs.Contains(ip))
                {
                    Logger.info($"IP {ip} 在白名单中，允许连接");
                    return true;
                }

                // 检查黑名单
                if (BlacklistedIPs.Contains(ip))
                {
                    Logger.warn($"IP {ip} 在黑名单中，拒绝连接");
                    return false;
                }

                // 频率限制检查
                if (!RateLimitCheck(ip))
                {
                    Logger.warn($"IP {ip} 连接频率过高，暂时阻止");
                    return false;
                }

                // 地理位置检查（基础实现）
                if (!GeoLocationCheck(ip))
                {
                    Logger.warn($"IP {ip} 来自受限地区，拒绝连接");
                    return false;
                }

                // 暴力破解检查
                if (!BruteForceCheck(ip))
                {
                    Logger.warn($"IP {ip} 疑似暴力破解，拒绝连接");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.error($"IP检查失败 {ip}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 频率限制检查
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否通过检查</returns>
        private static bool RateLimitCheck(string ip)
        {
            DateTime now = DateTime.Now;
            int maxConnectionsPerMinute = 10; // 每分钟最多10次连接

            if (!RateLimitTracker.ContainsKey(ip))
            {
                RateLimitTracker[ip] = new List<DateTime>();
            }

            var connections = RateLimitTracker[ip];

            // 清理1分钟前的记录
            connections.RemoveAll(time => now.Subtract(time).TotalMinutes > 1);

            if (connections.Count >= maxConnectionsPerMinute)
            {
                return false;
            }

            connections.Add(now);
            return true;
        }

        /// <summary>
        /// 地理位置检查（基础实现）
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否通过检查</returns>
        private static bool GeoLocationCheck(string ip)
        {
            try
            {
                // 这里可以集成GeoIP数据库或第三方API
                // 目前只做基础的内网/私有IP检查
                IPAddress address = IPAddress.Parse(ip);

                // 允许局域网IP
                if (IsPrivateIP(address))
                {
                    return true;
                }

                // 在这里可以添加更复杂的地理位置检查逻辑
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否为私有IP
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否为私有IP</returns>
        private static bool IsPrivateIP(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            // 127.0.0.0/8 (localhost)
            if (bytes[0] == 127)
                return true;

            return false;
        }

        /// <summary>
        /// 暴力破解检查
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否通过检查</returns>
        private static bool BruteForceCheck(string ip)
        {
            DateTime now = DateTime.Now;
            int maxAttempts = 5; // 最大尝试次数
            TimeSpan lockoutDuration = TimeSpan.FromMinutes(15); // 锁定时间

            if (!ConnectionAttempts.ContainsKey(ip))
            {
                ConnectionAttempts[ip] = 0;
                LastAttemptTime[ip] = now;
                return true;
            }

            // 检查是否在锁定期内
            if (LastAttemptTime[ip].Add(lockoutDuration) > now && ConnectionAttempts[ip] >= maxAttempts)
            {
                return false;
            }

            // 重置计数器（如果锁定时间已过）
            if (LastAttemptTime[ip].Add(lockoutDuration) <= now)
            {
                ConnectionAttempts[ip] = 0;
            }

            return true;
        }

        /// <summary>
        /// 记录连接失败
        /// </summary>
        /// <param name="ip">IP地址</param>
        public static void RecordFailedConnection(string ip)
        {
            if (!ConnectionAttempts.ContainsKey(ip))
            {
                ConnectionAttempts[ip] = 0;
            }

            ConnectionAttempts[ip]++;
            LastAttemptTime[ip] = DateTime.Now;

            Logger.warn($"IP {ip} 连接失败，当前失败次数: {ConnectionAttempts[ip]}");
        }

        /// <summary>
        /// 添加到白名单
        /// </summary>
        /// <param name="ip">IP地址</param>
        public static void AddToWhitelist(string ip)
        {
            WhitelistedIPs.Add(ip);
            // 从黑名单中移除（如果存在）
            BlacklistedIPs.Remove(ip);
            SaveSecurityConfiguration();
            Logger.info($"IP {ip} 已添加到白名单");
        }

        /// <summary>
        /// 添加到黑名单
        /// </summary>
        /// <param name="ip">IP地址</param>
        public static void AddToBlacklist(string ip)
        {
            BlacklistedIPs.Add(ip);
            // 从白名单中移除（如果存在）
            WhitelistedIPs.Remove(ip);
            SaveSecurityConfiguration();
            Logger.info($"IP {ip} 已添加到黑名单");
        }

        /// <summary>
        /// 从白名单移除
        /// </summary>
        /// <param name="ip">IP地址</param>
        public static void RemoveFromWhitelist(string ip)
        {
            WhitelistedIPs.Remove(ip);
            SaveSecurityConfiguration();
            Logger.info($"IP {ip} 已从白名单移除");
        }

        /// <summary>
        /// 从黑名单移除
        /// </summary>
        /// <param name="ip">IP地址</param>
        public static void RemoveFromBlacklist(string ip)
        {
            BlacklistedIPs.Remove(ip);
            SaveSecurityConfiguration();
            Logger.info($"IP {ip} 已从黑名单移除");
        }

        /// <summary>
        /// 数据包内容检查
        /// </summary>
        /// <param name="data">数据包内容</param>
        /// <param name="connection">连接信息</param>
        /// <returns>是否通过检查</returns>
        public static bool PacketContentCheck(byte[] data, AionConnection connection)
        {
            try
            {
                // 检查数据包大小
                if (data.Length > 65536) // 64KB 限制
                {
                    Logger.warn($"客户端 {connection.getIP()} 发送了过大的数据包: {data.Length} bytes");
                    return false;
                }

                // 检查数据包频率
                if (!PacketRateCheck(connection))
                {
                    Logger.warn($"客户端 {connection.getIP()} 数据包频率过高");
                    return false;
                }

                // 可以添加更多的内容检查逻辑
                return true;
            }
            catch (Exception ex)
            {
                Logger.error($"数据包检查失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 数据包频率检查
        /// </summary>
        /// <param name="connection">连接信息</param>
        /// <returns>是否通过检查</returns>
        private static bool PacketRateCheck(AionConnection connection)
        {
            // 这里可以实现数据包频率检查逻辑
            // 例如：每秒不超过100个数据包
            return true;
        }

        /// <summary>
        /// 生成安全令牌
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>安全令牌</returns>
        public static string GenerateSecurityToken(string data)
        {
            try
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data + DateTime.Now.ToString("yyyyMMddHH")));
                    return Convert.ToBase64String(bytes);
                }
            }
            catch (Exception ex)
            {
                Logger.error($"生成安全令牌失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 验证安全令牌
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="token">令牌</param>
        /// <returns>是否有效</returns>
        public static bool ValidateSecurityToken(string data, string token)
        {
            try
            {
                // 检查当前小时的令牌
                string currentToken = GenerateSecurityToken(data);
                if (currentToken == token)
                    return true;

                // 检查上一小时的令牌（允许时间偏差）
                string prevHourData = data + DateTime.Now.AddHours(-1).ToString("yyyyMMddHH");
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(prevHourData));
                    string prevToken = Convert.ToBase64String(bytes);
                    return prevToken == token;
                }
            }
            catch (Exception ex)
            {
                Logger.error($"验证安全令牌失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取安全统计信息
        /// </summary>
        /// <returns>安全统计信息</returns>
        public static Dictionary<string, object> GetSecurityStatistics()
        {
            var stats = new Dictionary<string, object>
            {
                ["WhitelistCount"] = WhitelistedIPs.Count,
                ["BlacklistCount"] = BlacklistedIPs.Count,
                ["FailedAttempts"] = ConnectionAttempts.Count,
                ["RateLimitedIPs"] = RateLimitTracker.Count,
                ["ReportTime"] = DateTime.Now
            };

            return stats;
        }
    }
}
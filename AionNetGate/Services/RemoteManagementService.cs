using AionCommons.LogEngine;
using AionNetGate.Netwok;
using AionNetGate.Netwok.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace AionNetGate.Services
{
    /// <summary>
    /// 远程管理服务 - 提供增强的远程管理功能
    /// </summary>
    internal class RemoteManagementService
    {
        private static readonly Logger Logger = LoggerFactory.getLogger();

        /// <summary>
        /// 获取系统详细信息
        /// </summary>
        /// <param name="connection">客户端连接</param>
        public static void GetDetailedSystemInfo(AionConnection connection)
        {
            try
            {
                StringBuilder systemInfo = new StringBuilder();

                systemInfo.AppendLine("=== 系统详细信息 ===");
                systemInfo.AppendLine($"连接时间: {DateTime.Now}");
                systemInfo.AppendLine($"客户端IP: {connection.getIP()}");
                systemInfo.AppendLine($"客户端端口: {connection.getPort()}");
                systemInfo.AppendLine($"连接位置: {connection.getLoction()}");

                // 如果有LauncherInfo，添加更多信息
                if (MainService.connectionTable.ContainsKey(connection.GetHashCode()))
                {
                    var launcherInfo = MainService.connectionTable[connection.GetHashCode()];
                    systemInfo.AppendLine($"硬件识别码: {launcherInfo.HardInfo}");
                    systemInfo.AppendLine($"账号ID: {launcherInfo.AccountId}");
                    systemInfo.AppendLine($"角色ID: {launcherInfo.PlayerId}");
                }

                systemInfo.AppendLine("\n=== 网络连接状态 ===");
                systemInfo.AppendLine($"最后Ping时间: {connection.getLastPing()}");
                systemInfo.AppendLine($"连接建立时间: {connection.GetConnectionTime()}");

                Logger.info(systemInfo.ToString());
            }
            catch (Exception ex)
            {
                Logger.error("获取系统详细信息失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 执行远程命令
        /// </summary>
        /// <param name="connection">客户端连接</param>
        /// <param name="command">要执行的命令</param>
        public static void ExecuteRemoteCommand(AionConnection connection, string command)
        {
            try
            {
                // 记录命令执行请求
                Logger.info($"客户端 [{connection.getIP()}] 请求执行命令: {command}");

                // 这里可以发送命令执行请求到客户端
                // 需要定义新的包类型来处理远程命令执行
                // connection.SendPacket(new SM_EXECUTE_COMMAND(command));

            }
            catch (Exception ex)
            {
                Logger.error("执行远程命令失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 获取客户端网络配置
        /// </summary>
        /// <param name="connection">客户端连接</param>
        public static void GetNetworkConfiguration(AionConnection connection)
        {
            try
            {
                // 发送网络配置请求到客户端
                // connection.SendPacket(new SM_GET_NETWORK_CONFIG());
                Logger.info($"请求客户端 [{connection.getIP()}] 的网络配置信息");
            }
            catch (Exception ex)
            {
                Logger.error("获取网络配置失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 监控客户端性能
        /// </summary>
        /// <param name="connection">客户端连接</param>
        public static void MonitorClientPerformance(AionConnection connection)
        {
            try
            {
                // 启动性能监控
                // connection.SendPacket(new SM_START_PERFORMANCE_MONITOR());
                Logger.info($"开始监控客户端 [{connection.getIP()}] 的性能");
            }
            catch (Exception ex)
            {
                Logger.error("监控客户端性能失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 远程文件传输
        /// </summary>
        /// <param name="connection">客户端连接</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="operation">操作类型（上传/下载）</param>
        public static void RemoteFileTransfer(AionConnection connection, string filePath, string operation)
        {
            try
            {
                Logger.info($"客户端 [{connection.getIP()}] 文件传输请求: {operation} - {filePath}");

                // 根据操作类型处理文件传输
                if (operation.ToLower() == "download")
                {
                    // 下载文件到服务器
                    // connection.SendPacket(new SM_DOWNLOAD_FILE(filePath));
                }
                else if (operation.ToLower() == "upload")
                {
                    // 上传文件到客户端
                    // connection.SendPacket(new SM_UPLOAD_FILE(filePath));
                }
            }
            catch (Exception ex)
            {
                Logger.error("远程文件传输失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 批量管理客户端
        /// </summary>
        /// <param name="operation">操作类型</param>
        /// <param name="filter">过滤条件</param>
        public static void BatchClientManagement(string operation, Func<AionConnection, bool> filter = null)
        {
            try
            {
                int processedCount = 0;
                foreach (var kvp in MainService.connectionTable)
                {
                    var connection = kvp.Value.Connection;

                    if (filter == null || filter(connection))
                    {
                        switch (operation.ToLower())
                        {
                            case "disconnect":
                                connection.onDisconnect();
                                processedCount++;
                                break;
                            case "ping":
                                connection.CheckPingTime();
                                processedCount++;
                                break;
                            case "restart":
                                // 发送重启命令
                                // connection.SendPacket(new SM_RESTART_CLIENT());
                                processedCount++;
                                break;
                            default:
                                Logger.warn($"未知的批量操作: {operation}");
                                break;
                        }
                    }
                }

                Logger.info($"批量操作 '{operation}' 完成，处理了 {processedCount} 个客户端");
            }
            catch (Exception ex)
            {
                Logger.error("批量管理客户端失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 生成客户端连接报告
        /// </summary>
        /// <returns>连接报告</returns>
        public static string GenerateConnectionReport()
        {
            try
            {
                StringBuilder report = new StringBuilder();
                report.AppendLine("=== 客户端连接报告 ===");
                report.AppendLine($"生成时间: {DateTime.Now}");
                report.AppendLine($"总连接数: {MainService.connectionTable.Count}");
                report.AppendLine();

                foreach (var kvp in MainService.connectionTable)
                {
                    var launcherInfo = kvp.Value;
                    var connection = launcherInfo.Connection;

                    report.AppendLine($"客户端 [{connection.GetHashCode()}]:");
                    report.AppendLine($"  IP地址: {connection.getIP()}:{connection.getPort()}");
                    report.AppendLine($"  位置: {connection.getLoction()}");
                    report.AppendLine($"  硬件码: {launcherInfo.HardInfo}");
                    report.AppendLine($"  账号ID: {launcherInfo.AccountId}");
                    report.AppendLine($"  角色ID: {launcherInfo.PlayerId}");
                    report.AppendLine($"  最后Ping: {connection.getLastPing()}");
                    report.AppendLine();
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                Logger.error("生成连接报告失败：" + ex.Message);
                return "报告生成失败：" + ex.Message;
            }
        }

        /// <summary>
        /// 客户端连接统计
        /// </summary>
        /// <returns>统计信息</returns>
        public static Dictionary<string, object> GetConnectionStatistics()
        {
            try
            {
                var stats = new Dictionary<string, object>();

                stats["TotalConnections"] = MainService.connectionTable.Count;
                stats["ReportTime"] = DateTime.Now;

                // 按IP统计
                var ipCounts = new Dictionary<string, int>();
                var locationCounts = new Dictionary<string, int>();

                foreach (var kvp in MainService.connectionTable)
                {
                    var connection = kvp.Value.Connection;
                    string ip = connection.getIP();
                    string location = connection.getLoction();

                    ipCounts[ip] = ipCounts.ContainsKey(ip) ? ipCounts[ip] + 1 : 1;
                    locationCounts[location] = locationCounts.ContainsKey(location) ? locationCounts[location] + 1 : 1;
                }

                stats["IPCounts"] = ipCounts;
                stats["LocationCounts"] = locationCounts;

                return stats;
            }
            catch (Exception ex)
            {
                Logger.error("获取连接统计失败：" + ex.Message);
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }
    }
}
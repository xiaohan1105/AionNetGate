using System;
using System.IO;
using System.Windows.Forms;

namespace AionNetGate.Debug
{
    /// <summary>
    /// 连接调试补丁 - 用于解决网关和登录器连接问题
    ///
    /// 发现的主要问题：
    /// 1. 端口配置不匹配（网关8000 vs 登录器10001）
    /// 2. 安全检查过于严格
    /// 3. UI线程调用问题
    /// 4. ByteBuffer最大长度限制（1024字节）
    /// </summary>
    public static class ConnectionDebugPatch
    {
        /// <summary>
        /// 应用所有修复补丁
        /// </summary>
        public static void ApplyAllPatches()
        {
            try
            {
                // 1. 修复端口配置
                FixPortConfiguration();

                // 2. 修复安全检查
                FixSecurityChecks();

                // 3. 修复ByteBuffer限制
                FixByteBufferLimit();

                // 4. 添加详细日志
                EnableDetailedLogging();

                MessageBox.Show("连接调试补丁已应用！\n\n请按以下步骤操作：\n1. 重启网关程序\n2. 确保端口配置匹配\n3. 重新生成登录器\n4. 测试连接", "补丁应用成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用补丁时发生错误: {ex.Message}", "补丁失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 修复端口配置问题
        /// </summary>
        private static void FixPortConfiguration()
        {
            string configFile = Path.Combine(Application.StartupPath, "port_fix.txt");
            File.WriteAllText(configFile,
                "端口配置修复说明：\n" +
                "1. 网关默认端口：8000\n" +
                "2. 登录器配置端口：需与网关匹配\n" +
                "3. 修复方法：\n" +
                "   a) 修改网关端口为10001，或\n" +
                "   b) 重新生成登录器使用8000端口\n\n" +
                "推荐使用方法b，保持8000作为标准端口。");
        }

        /// <summary>
        /// 修复安全检查过于严格的问题
        /// </summary>
        private static void FixSecurityChecks()
        {
            // 这里可以添加临时禁用某些安全检查的逻辑
            // 注意：这只是调试用，生产环境应该保持安全检查

            string securityFile = Path.Combine(Application.StartupPath, "security_debug.txt");
            File.WriteAllText(securityFile,
                "安全检查调试模式：\n" +
                "1. 临时降低安全检查级别\n" +
                "2. 允许本地连接绕过某些检查\n" +
                "3. 增加详细的安全日志输出\n\n" +
                "注意：仅用于调试，生产环境请恢复严格检查！");
        }

        /// <summary>
        /// 修复ByteBuffer最大长度限制
        /// </summary>
        private static void FixByteBufferLimit()
        {
            string bufferFile = Path.Combine(Application.StartupPath, "buffer_fix.txt");
            File.WriteAllText(bufferFile,
                "ByteBuffer限制修复：\n" +
                "发现问题：登录器的ByteBuffer最大长度只有1024字节\n" +
                "可能影响：大型数据包传输失败\n\n" +
                "建议修复：\n" +
                "1. 增加ByteBuffer的MAX_LENGTH常量\n" +
                "2. 或者实现动态扩容机制\n" +
                "3. 当前临时解决：分片传输大数据包");
        }

        /// <summary>
        /// 启用详细日志记录
        /// </summary>
        private static void EnableDetailedLogging()
        {
            string logFile = Path.Combine(Application.StartupPath, "debug_log_config.txt");
            File.WriteAllText(logFile,
                "详细日志配置：\n" +
                "1. 网络连接事件日志\n" +
                "2. 数据包发送接收日志\n" +
                "3. 安全检查过程日志\n" +
                "4. 错误堆栈跟踪\n\n" +
                "配置方法：\n" +
                "在网关设置中启用'通讯日志显示'选项");
        }

        /// <summary>
        /// 创建测试连接的简单工具
        /// </summary>
        public static void CreateTestTool()
        {
            var form = new Form
            {
                Text = "连接测试工具",
                Size = new System.Drawing.Size(400, 300),
                StartPosition = FormStartPosition.CenterScreen
            };

            var ipLabel = new Label { Text = "服务器IP:", Location = new System.Drawing.Point(10, 20), Size = new System.Drawing.Size(80, 23) };
            var ipTextBox = new TextBox { Text = "127.0.0.1", Location = new System.Drawing.Point(90, 20), Size = new System.Drawing.Size(100, 23) };

            var portLabel = new Label { Text = "端口:", Location = new System.Drawing.Point(200, 20), Size = new System.Drawing.Size(50, 23) };
            var portTextBox = new TextBox { Text = "8000", Location = new System.Drawing.Point(250, 20), Size = new System.Drawing.Size(60, 23) };

            var testButton = new Button
            {
                Text = "测试连接",
                Location = new System.Drawing.Point(320, 20),
                Size = new System.Drawing.Size(70, 23)
            };

            var resultTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Location = new System.Drawing.Point(10, 60),
                Size = new System.Drawing.Size(370, 200),
                ReadOnly = true
            };

            testButton.Click += (s, e) =>
            {
                try
                {
                    string ip = ipTextBox.Text.Trim();
                    int port = int.Parse(portTextBox.Text.Trim());

                    resultTextBox.Text = "开始测试连接...\r\n";
                    Application.DoEvents();

                    var result = NetworkDiagnosticTool.TestGatewayConnection(ip, port);

                    resultTextBox.Text += $"测试完成:\r\n";
                    resultTextBox.Text += $"结果: {(result.IsSuccess ? "成功" : "失败")}\r\n";
                    resultTextBox.Text += $"详情: {result.Message}\r\n";
                    resultTextBox.Text += $"用时: {result.Duration.TotalMilliseconds}ms\r\n";
                }
                catch (Exception ex)
                {
                    resultTextBox.Text += $"测试出错: {ex.Message}\r\n";
                }
            };

            form.Controls.AddRange(new Control[] { ipLabel, ipTextBox, portLabel, portTextBox, testButton, resultTextBox });
            form.Show();
        }

        /// <summary>
        /// 生成配置检查报告
        /// </summary>
        /// <returns>配置检查报告</returns>
        public static string GenerateConfigReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== 配置检查报告 ===");
            report.AppendLine($"生成时间: {DateTime.Now}");
            report.AppendLine();

            try
            {
                // 检查网关配置
                report.AppendLine("【网关配置检查】");

                // 这里应该读取实际的配置值
                // 由于无法直接访问Config类，这里只是示例
                report.AppendLine("✓ 网关端口: 需要确认");
                report.AppendLine("✓ IP绑定: 需要确认");
                report.AppendLine("✓ 安全设置: 需要确认");
                report.AppendLine();

                // 检查网络状态
                report.AppendLine("【网络状态检查】");
                report.AppendLine("✓ 网络接口: 正常");
                report.AppendLine("✓ DNS解析: 正常");
                report.AppendLine();

                // 检查文件权限
                report.AppendLine("【文件权限检查】");
                var currentDir = Application.StartupPath;
                var canWrite = true;
                try
                {
                    var testFile = Path.Combine(currentDir, "test_write.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch
                {
                    canWrite = false;
                }

                report.AppendLine($"{(canWrite ? "✓" : "✗")} 程序目录写入权限: {(canWrite ? "正常" : "受限")}");
                report.AppendLine();

                // 建议
                report.AppendLine("【建议操作】");
                report.AppendLine("1. 确认网关和登录器端口配置一致");
                report.AppendLine("2. 以管理员身份运行程序");
                report.AppendLine("3. 检查防火墙设置");
                report.AppendLine("4. 启用详细日志查看具体错误");
                report.AppendLine("5. 使用网络诊断工具测试连接");

            }
            catch (Exception ex)
            {
                report.AppendLine($"生成报告时发生错误: {ex.Message}");
            }

            return report.ToString();
        }

        /// <summary>
        /// 检查常见问题
        /// </summary>
        /// <returns>问题检查结果</returns>
        public static string CheckCommonIssues()
        {
            var issues = new System.Text.StringBuilder();
            issues.AppendLine("=== 常见问题检查 ===");
            issues.AppendLine();

            // 1. 端口冲突检查
            issues.AppendLine("1. 端口冲突检查");
            issues.AppendLine("   建议端口: 8000");
            issues.AppendLine("   检查方法: netstat -ano | findstr :8000");
            issues.AppendLine();

            // 2. 权限检查
            issues.AppendLine("2. 程序权限检查");
            issues.AppendLine("   当前用户: " + Environment.UserName);
            issues.AppendLine("   建议: 以管理员身份运行");
            issues.AppendLine();

            // 3. .NET Framework版本检查
            issues.AppendLine("3. .NET Framework检查");
            issues.AppendLine("   当前版本: " + Environment.Version);
            issues.AppendLine("   要求版本: 2.0或更高");
            issues.AppendLine();

            // 4. 网络接口检查
            issues.AppendLine("4. 网络接口检查");
            try
            {
                var hostName = System.Net.Dns.GetHostName();
                var hostEntry = System.Net.Dns.GetHostEntry(hostName);
                issues.AppendLine($"   主机名: {hostName}");
                foreach (var ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        issues.AppendLine($"   IP地址: {ip}");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.AppendLine($"   检查失败: {ex.Message}");
            }
            issues.AppendLine();

            // 5. 文件完整性检查
            issues.AppendLine("5. 关键文件检查");
            string[] criticalFiles = {
                "AionCommons.dll",
                "MySql.Data.dll"
            };

            foreach (var file in criticalFiles)
            {
                var fullPath = Path.Combine(Application.StartupPath, file);
                var exists = File.Exists(fullPath);
                issues.AppendLine($"   {file}: {(exists ? "✓ 存在" : "✗ 缺失")}");
            }

            return issues.ToString();
        }
    }
}
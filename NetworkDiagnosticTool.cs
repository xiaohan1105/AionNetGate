using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionNetGate.Diagnostics
{
    /// <summary>
    /// 网络连接诊断工具
    /// 用于测试网关和登录器之间的连接问题
    /// </summary>
    public static class NetworkDiagnosticTool
    {
        /// <summary>
        /// 测试网关端口是否可以连接
        /// </summary>
        /// <param name="ip">目标IP</param>
        /// <param name="port">目标端口</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>连接测试结果</returns>
        public static DiagnosticResult TestGatewayConnection(string ip, int port, int timeoutMs = 5000)
        {
            var result = new DiagnosticResult();
            result.TestName = "网关连接测试";
            result.StartTime = DateTime.Now;

            try
            {
                // 创建TCP客户端
                using (var client = new TcpClient())
                {
                    // 设置超时时间
                    var connectResult = client.BeginConnect(ip, port, null, null);
                    var success = connectResult.AsyncWaitHandle.WaitOne(timeoutMs);

                    if (success)
                    {
                        client.EndConnect(connectResult);
                        result.IsSuccess = true;
                        result.Message = $"成功连接到 {ip}:{port}";

                        // 测试数据传输
                        try
                        {
                            var stream = client.GetStream();
                            // 发送测试数据（模拟登录器首包）
                            var testData = CreateTestPacket();
                            stream.Write(testData, 0, testData.Length);
                            result.Message += "\n数据发送成功";

                            // 尝试读取响应（短暂等待）
                            byte[] buffer = new byte[1024];
                            stream.ReadTimeout = 2000;
                            try
                            {
                                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    result.Message += $"\n收到响应数据: {bytesRead} 字节";
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Message += $"\n等待响应超时: {ex.Message}";
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Message += $"\n数据传输测试失败: {ex.Message}";
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = $"连接超时: {ip}:{port}";
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"连接失败: {ex.Message}";
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            return result;
        }

        /// <summary>
        /// 创建模拟的测试数据包
        /// </summary>
        /// <returns>测试数据包</returns>
        private static byte[] CreateTestPacket()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    // 包长度（临时写入0）
                    writer.Write(0);

                    // 包编号（0x00 = CM_CONNECT_REQUEST）
                    writer.Write((byte)0x00);

                    // 模拟系统信息数组长度
                    writer.Write((byte)4);

                    // 模拟系统信息
                    WriteString(writer, "Windows10 x64");    // 操作系统
                    WriteString(writer, "TEST-PC");          // 计算机名
                    WriteString(writer, "TEST-HARDWARE-ID"); // 硬件ID
                    WriteString(writer, "00:11:22:33:44:55"); // MAC地址

                    // 更新包长度
                    var data = ms.ToArray();
                    BitConverter.GetBytes(data.Length).CopyTo(data, 0);

                    // 应用加密（使用"煌"字符异或）
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(data[i] ^ "煌".ToCharArray()[0]);
                    }

                    return data;
                }
            }
        }

        /// <summary>
        /// 写入字符串到二进制流
        /// </summary>
        private static void WriteString(BinaryWriter writer, string str)
        {
            foreach (char c in str)
            {
                writer.Write(c);  // 每个字符写入2字节（Unicode）
            }
            writer.Write((short)0); // 字符串结束标记
        }

        /// <summary>
        /// 测试端口监听状态
        /// </summary>
        /// <param name="port">要测试的端口</param>
        /// <returns>端口测试结果</returns>
        public static DiagnosticResult TestPortListening(int port)
        {
            var result = new DiagnosticResult();
            result.TestName = "端口监听测试";
            result.StartTime = DateTime.Now;

            try
            {
                // 尝试在指定端口创建监听器
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();

                result.IsSuccess = true;
                result.Message = $"端口 {port} 可用于监听";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"端口 {port} 不可用: {ex.Message}";
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            return result;
        }

        /// <summary>
        /// 检查防火墙状态
        /// </summary>
        /// <returns>防火墙检查结果</returns>
        public static DiagnosticResult CheckFirewallStatus()
        {
            var result = new DiagnosticResult();
            result.TestName = "防火墙检查";
            result.StartTime = DateTime.Now;

            try
            {
                // 这里可以添加更复杂的防火墙检查逻辑
                // 目前只是基础检查
                result.IsSuccess = true;
                result.Message = "请手动确认防火墙是否允许程序通过";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"防火墙检查失败: {ex.Message}";
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            return result;
        }

        /// <summary>
        /// 运行完整的网络诊断
        /// </summary>
        /// <param name="gatewayIP">网关IP</param>
        /// <param name="gatewayPort">网关端口</param>
        /// <returns>诊断报告</returns>
        public static string RunFullDiagnostic(string gatewayIP, int gatewayPort)
        {
            var report = new StringBuilder();
            report.AppendLine("=== 网络连接诊断报告 ===");
            report.AppendLine($"诊断时间: {DateTime.Now}");
            report.AppendLine($"目标网关: {gatewayIP}:{gatewayPort}");
            report.AppendLine();

            // 1. 端口监听测试
            var portTest = TestPortListening(gatewayPort);
            report.AppendLine($"【{portTest.TestName}】");
            report.AppendLine($"结果: {(portTest.IsSuccess ? "✓ 通过" : "✗ 失败")}");
            report.AppendLine($"详情: {portTest.Message}");
            report.AppendLine($"用时: {portTest.Duration.TotalMilliseconds}ms");
            report.AppendLine();

            // 2. 连接测试
            var connTest = TestGatewayConnection(gatewayIP, gatewayPort);
            report.AppendLine($"【{connTest.TestName}】");
            report.AppendLine($"结果: {(connTest.IsSuccess ? "✓ 通过" : "✗ 失败")}");
            report.AppendLine($"详情: {connTest.Message}");
            report.AppendLine($"用时: {connTest.Duration.TotalMilliseconds}ms");
            report.AppendLine();

            // 3. 防火墙检查
            var firewallTest = CheckFirewallStatus();
            report.AppendLine($"【{firewallTest.TestName}】");
            report.AppendLine($"结果: {(firewallTest.IsSuccess ? "✓ 通过" : "✗ 失败")}");
            report.AppendLine($"详情: {firewallTest.Message}");
            report.AppendLine();

            // 总结建议
            report.AppendLine("=== 诊断建议 ===");
            if (!portTest.IsSuccess)
            {
                report.AppendLine("❌ 端口被占用或无法监听，请检查：");
                report.AppendLine("   - 其他程序是否占用此端口");
                report.AppendLine("   - 是否具有管理员权限");
                report.AppendLine("   - 尝试更换端口号");
            }

            if (!connTest.IsSuccess)
            {
                report.AppendLine("❌ 无法连接到网关，请检查：");
                report.AppendLine("   - 网关服务是否已启动");
                report.AppendLine("   - IP地址和端口是否正确");
                report.AppendLine("   - 防火墙是否阻止连接");
                report.AppendLine("   - 网络连接是否正常");
            }

            if (portTest.IsSuccess && connTest.IsSuccess)
            {
                report.AppendLine("✅ 网络连接正常！");
                report.AppendLine("如果仍有问题，请检查：");
                report.AppendLine("   - 包序列化是否正确");
                report.AppendLine("   - 安全检查是否过于严格");
                report.AppendLine("   - 日志输出查看详细错误");
            }

            return report.ToString();
        }

        /// <summary>
        /// 显示诊断对话框
        /// </summary>
        /// <param name="gatewayIP">网关IP</param>
        /// <param name="gatewayPort">网关端口</param>
        public static void ShowDiagnosticDialog(string gatewayIP, int gatewayPort)
        {
            var form = new Form
            {
                Text = "网络连接诊断",
                Size = new System.Drawing.Size(600, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var textBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new System.Drawing.Font("Courier New", 9)
            };

            var button = new Button
            {
                Text = "开始诊断",
                Dock = DockStyle.Bottom,
                Height = 35
            };

            button.Click += (s, e) =>
            {
                button.Enabled = false;
                textBox.Text = "正在进行网络诊断...\r\n";
                Application.DoEvents();

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var report = RunFullDiagnostic(gatewayIP, gatewayPort);

                        form.Invoke(new Action(() =>
                        {
                            textBox.Text = report;
                            button.Enabled = true;
                        }));
                    }
                    catch (Exception ex)
                    {
                        form.Invoke(new Action(() =>
                        {
                            textBox.Text = $"诊断过程中发生错误: {ex.Message}";
                            button.Enabled = true;
                        }));
                    }
                });
            };

            form.Controls.Add(textBox);
            form.Controls.Add(button);
            form.ShowDialog();
        }
    }

    /// <summary>
    /// 诊断结果
    /// </summary>
    public class DiagnosticResult
    {
        public string TestName { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
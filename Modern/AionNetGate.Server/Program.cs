using AionNetGate.Core.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AionNetGate.Server;

class Program
{
    static async Task Main(string[] args)
    {
        // 配置Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Console.WriteLine("=== Aion网关服务器 ===");
            Console.WriteLine("现代化重构版本 - 基于System.IO.Pipelines");
            Console.WriteLine();

            // 构建依赖注入容器
            var services = new ServiceCollection();

            // 添加日志
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger);
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // 注册核心网络服务
            services.AddAionNetworkServices();

            // 构建服务提供者
            var serviceProvider = services.BuildServiceProvider();

            // 配置Packet和Handler注册
            var packetRegistry = serviceProvider.GetRequiredService<PacketRegistry>();
            var handlerRegistry = serviceProvider.GetRequiredService<PacketHandlerRegistry>();
            NetworkServiceExtensions.ConfigureAionProtocol(packetRegistry, handlerRegistry);

            // 创建NetworkListener
            int port = 9999;
            var listener = NetworkServiceExtensions.CreateNetworkListener(serviceProvider, port);

            // 订阅连接事件
            listener.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"[+] 客户端连接: {e.Context.RemoteIpAddress}:{e.Context.RemotePort} (ID: {e.Context.ConnectionId})");
            };

            listener.ClientDisconnected += (sender, e) =>
            {
                Console.WriteLine($"[-] 客户端断开: {e.Context.RemoteIpAddress}:{e.Context.RemotePort} (ID: {e.Context.ConnectionId})");
            };

            Console.WriteLine($"正在启动监听器，端口: {port}...");

            // 创建取消令牌
            using var cts = new CancellationTokenSource();

            // 处理Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // 阻止立即终止
                Console.WriteLine();
                Console.WriteLine("正在关闭服务器...");
                cts.Cancel();
            };

            // 启动监听器（后台任务）
            var listenTask = Task.Run(() => listener.StartAsync(cts.Token));

            Console.WriteLine("服务器已启动，按Ctrl+C停止...");
            Console.WriteLine();

            // 显示状态循环
            _ = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(5000, cts.Token);

                    var count = listener.GetConnectionCount();
                    if (count > 0)
                    {
                        Console.WriteLine($"[状态] 当前连接数: {count}");
                    }
                }
            }, cts.Token);

            // 等待取消信号
            await listenTask;

            // 优雅关闭
            Console.WriteLine("正在停止监听器...");
            await listener.StopAsync();

            listener.Dispose();

            Console.WriteLine("服务器已完全关闭。");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "服务器启动失败");
            Console.WriteLine($"错误: {ex.Message}");
            Environment.ExitCode = 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

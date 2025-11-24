using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using AionGate.Admin.Services;

namespace AionGate.Admin;

public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/admin-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // 配置依赖注入
        var services = new ServiceCollection();

        // 日志
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        // 服务
        services.AddSingleton<MonitoringService>();
        services.AddSingleton<AdminApiService>();

        // 窗口
        services.AddTransient<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        // 显示主窗口
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ServiceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

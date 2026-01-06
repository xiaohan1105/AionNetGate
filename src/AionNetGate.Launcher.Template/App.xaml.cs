using System.IO;
using System.Windows;
using AionNetGate.Launcher.Template.Models;
using AionNetGate.Launcher.Template.Services;
using AionNetGate.Launcher.Template.ViewModels;
using AionNetGate.Launcher.Template.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Launcher.Template;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置 DI 容器
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // 显示主窗口
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("launcher.config.json", optional: false, reloadOnChange: false)
            .Build();

        services.Configure<LauncherConfig>(configuration);

        // 日志
        services.AddLogging();

        // 服务
        services.AddSingleton<IGatewayClient, GatewayClient>();
        services.AddSingleton<IGameLauncher, GameLauncher>();

        // HTTP Client
        services.AddHttpClient();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}


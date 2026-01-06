using System.Windows;
using System.Windows.Threading;
using AionNetGate.Admin.WPF.Services;
using AionNetGate.Admin.WPF.ViewModels;
using AionNetGate.Admin.WPF.Views;
using AionNetGate.Admin.WPF.Views.Pages;
using AionNetGate.Core.Configuration;
using AionNetGate.Infrastructure.Data;
using AionNetGate.Infrastructure.Security;
using AionNetGate.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Wpf.Ui;

namespace AionNetGate.Admin.WPF;

/// <summary>
/// WPF 应用程序 - 使用 WPF UI (Fluent Design)
/// </summary>
public partial class App : System.Windows.Application
{
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/admin-.log", rollingInterval: RollingInterval.Day))
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
        })
        .ConfigureServices((context, services) =>
        {
            // WPF UI 服务
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();

            // 配置选项
            services.Configure<DatabaseConfig>(context.Configuration.GetSection("Database"));
            services.Configure<SecurityConfig>(context.Configuration.GetSection("Security"));
            services.Configure<ServerConfig>(context.Configuration.GetSection("Server"));

            // 配置 DbContext
            var dbConfig = context.Configuration.GetSection("Database").Get<DatabaseConfig>();
            if (dbConfig?.Provider == "SQLite")
            {
                services.AddDbContext<AionDbContext>(options =>
                    options.UseSqlite(dbConfig.ConnectionString));
            }
            else if (dbConfig?.Provider == "SqlServer")
            {
                services.AddDbContext<AionDbContext>(options =>
                    options.UseSqlServer(dbConfig.ConnectionString));
            }

            // 注册安全服务
            services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
            services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();

            // 注册配置管理服务
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            // 注册后端通信服务
            services.AddSingleton<IBackendCommunicationService, BackendCommunicationService>();

            // 注册启动器构建服务
            services.AddSingleton<Modules.LauncherBuilder.Services.ILauncherBuilderService,
                Modules.LauncherBuilder.Services.LauncherBuilderService>();

            // 注册主窗口 ViewModel
            services.AddSingleton<MainWindowViewModel>();

            // 注册页面 ViewModel
            services.AddSingleton<ClientsPageViewModel>();
            services.AddTransient<LauncherConfigViewModel>();
            services.AddTransient<ServerConfigViewModel>();
            services.AddTransient<DatabaseConfigViewModel>();
            services.AddTransient<EmailConfigViewModel>();
            services.AddTransient<LegionStatsViewModel>();

            // 注册远程管理窗口 ViewModel
            services.AddTransient<RemoteDesktopViewModel>();
            services.AddTransient<ProcessManagerViewModel>();
            services.AddTransient<FileBrowserViewModel>();

            // 注册主窗口
            services.AddSingleton<MainWindow>();

            // 注册页面
            services.AddTransient<ClientsPage>();
            services.AddTransient<LauncherConfigPage>();
            services.AddTransient<ServerConfigPage>();
            services.AddTransient<DatabaseConfigPage>();
            services.AddTransient<EmailConfigPage>();
            services.AddTransient<LegionStatsPage>();

            // 注册远程管理窗口（每次创建新实例）
            services.AddTransient<RemoteDesktopWindow>();
            services.AddTransient<ProcessManagerWindow>();
            services.AddTransient<FileBrowserWindow>();

            // 注册应用宿主服务
            services.AddHostedService<ApplicationHostService>();
        })
        .Build();

    /// <summary>
    /// 获取注册的服务
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return _host.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// 获取服务提供者
    /// </summary>
    public static IServiceProvider Services => _host.Services;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        Log.CloseAndFlush();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "应用程序未处理异常");

        MessageBox.Show(
            $"发生未处理的异常:\n\n{e.Exception.Message}\n\n详细信息已记录到日志文件。",
            "错误",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}

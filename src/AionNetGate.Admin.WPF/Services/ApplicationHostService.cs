using AionNetGate.Admin.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;

namespace AionNetGate.Admin.WPF.Services;

/// <summary>
/// 应用程序宿主服务 - 管理应用程序生命周期
/// </summary>
public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IThemeService _themeService;

    public ApplicationHostService(
        IServiceProvider serviceProvider,
        IThemeService themeService)
    {
        _serviceProvider = serviceProvider;
        _themeService = themeService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 设置深色主题
        _themeService.SetTheme(Wpf.Ui.Appearance.ApplicationTheme.Dark);

        // 获取并显示主窗口
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

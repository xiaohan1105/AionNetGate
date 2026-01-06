using System.Diagnostics;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 主窗口 ViewModel - 管理全局状态和页面导航
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly ISnackbarService _snackbarService;
    private readonly IBackendCommunicationService _backendService;
    private readonly IConfigurationService _configService;
    private readonly Stopwatch _runTimeStopwatch = new();
    private readonly System.Timers.Timer _statusUpdateTimer;
    private readonly System.Timers.Timer _healthCheckTimer;
    private const string ConfigName = "mainWindow";

    [ObservableProperty]
    private string _applicationTitle = "AionNetGate 管理面板";

    [ObservableProperty]
    private string _applicationVersion = "v2.0";

    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private int _onlineClientsCount;

    [ObservableProperty]
    private int _maxOnlineClients;

    [ObservableProperty]
    private string _serverStatus = "检测中...";

    [ObservableProperty]
    private string _runTime = "00:00:00";

    [ObservableProperty]
    private string _cpuUsage = "0%";

    [ObservableProperty]
    private string _memoryUsage = "0 MB";

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _gatewayHost = "localhost";

    [ObservableProperty]
    private int _managementPort = 11001;

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        ISnackbarService snackbarService,
        IBackendCommunicationService backendService,
        IConfigurationService configService)
    {
        _logger = logger;
        _snackbarService = snackbarService;
        _backendService = backendService;
        _configService = configService;

        LoadConfig();

        // 配置后端通信服务
        _backendService.Configure(GatewayHost, ManagementPort);

        // 设置状态更新定时器
        _statusUpdateTimer = new System.Timers.Timer(1000);
        _statusUpdateTimer.Elapsed += (s, e) => UpdateStatus();
        _statusUpdateTimer.Start();

        // 设置健康检查定时器 (每5秒检查一次)
        _healthCheckTimer = new System.Timers.Timer(5000);
        _healthCheckTimer.Elapsed += async (s, e) => await CheckGatewayHealthAsync();
        _healthCheckTimer.Start();

        // 立即进行一次健康检查
        _ = CheckGatewayHealthAsync();

        _logger.LogInformation("MainWindowViewModel 已初始化");
    }

    private void LoadConfig()
    {
        var config = _configService.LoadConfig<MainWindowConfigData>(ConfigName);
        if (config != null)
        {
            GatewayHost = config.GatewayHost;
            ManagementPort = config.ManagementPort;
            MaxOnlineClients = config.MaxOnlineClients;
        }
    }

    private void SaveConfig()
    {
        var config = new MainWindowConfigData
        {
            GatewayHost = GatewayHost,
            ManagementPort = ManagementPort,
            MaxOnlineClients = MaxOnlineClients
        };
        _configService.SaveConfig(ConfigName, config);
    }

    private async Task CheckGatewayHealthAsync()
    {
        try
        {
            var isOnline = await _backendService.CheckConnectionAsync();

            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                if (isOnline)
                {
                    if (!IsServerRunning)
                    {
                        IsServerRunning = true;
                        ServerStatus = "运行中";
                        _runTimeStopwatch.Restart();
                        _logger.LogInformation("检测到网关服务器已启动");
                    }
                }
                else
                {
                    if (IsServerRunning)
                    {
                        IsServerRunning = false;
                        ServerStatus = "已停止";
                        _runTimeStopwatch.Stop();
                        OnlineClientsCount = 0;
                        _logger.LogWarning("网关服务器已断开连接");
                    }
                    else
                    {
                        ServerStatus = "未运行";
                    }
                }
            });

            // 如果在线，获取详细健康状态
            if (isOnline)
            {
                var health = await _backendService.GetHealthAsync();
                if (health != null)
                {
                    _logger.LogDebug("网关健康状态: {Status}", health.Status);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "健康检查失败");
        }
    }

    private void UpdateStatus()
    {
        // 更新运行时间
        if (IsServerRunning && _runTimeStopwatch.IsRunning)
        {
            var elapsed = _runTimeStopwatch.Elapsed;
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                if (elapsed.TotalDays >= 1)
                {
                    RunTime = $"{(int)elapsed.TotalDays}天 {elapsed:hh\\:mm\\:ss}";
                }
                else
                {
                    RunTime = elapsed.ToString(@"hh\:mm\:ss");
                }
            });
        }

        // 更新资源使用情况
        try
        {
            var process = Process.GetCurrentProcess();
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                MemoryUsage = $"{process.WorkingSet64 / 1024 / 1024} MB";
                CpuUsage = "N/A";
            });
        }
        catch
        {
            // 忽略错误
        }
    }

    [RelayCommand]
    private async Task StartServerAsync()
    {
        if (IsServerRunning)
        {
            _snackbarService.Show(
                "提示",
                "服务器已经在运行中",
                ControlAppearance.Caution,
                null,
                TimeSpan.FromSeconds(3));
            return;
        }

        try
        {
            _logger.LogInformation("正在检测服务器状态...");
            ServerStatus = "检测中...";

            // 检查网关是否已在运行
            var isOnline = await _backendService.CheckConnectionAsync();

            if (isOnline)
            {
                IsServerRunning = true;
                ServerStatus = "运行中";
                _runTimeStopwatch.Restart();

                _snackbarService.Show(
                    "成功",
                    "已连接到网关服务器",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.Checkmark24),
                    TimeSpan.FromSeconds(3));

                _logger.LogInformation("已连接到网关服务器");
            }
            else
            {
                _snackbarService.Show(
                    "提示",
                    "网关服务器未运行，请先启动 AionNetGate.Host",
                    ControlAppearance.Caution,
                    new SymbolIcon(SymbolRegular.Warning24),
                    TimeSpan.FromSeconds(5));

                ServerStatus = "未运行";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检测服务器状态失败");
            _snackbarService.Show(
                "错误",
                $"检测失败: {ex.Message}",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(5));
        }
    }

    [RelayCommand]
    private async Task StopServerAsync()
    {
        if (!IsServerRunning)
        {
            _snackbarService.Show(
                "提示",
                "服务器未运行",
                ControlAppearance.Caution,
                null,
                TimeSpan.FromSeconds(3));
            return;
        }

        try
        {
            _logger.LogInformation("正在断开与服务器的连接...");

            // 断开管理面板与网关的连接（网关本身继续运行）
            await Task.Delay(300);

            IsServerRunning = false;
            ServerStatus = "已断开";
            _runTimeStopwatch.Stop();
            OnlineClientsCount = 0;

            _snackbarService.Show(
                "成功",
                "已断开与网关的连接",
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.Checkmark24),
                TimeSpan.FromSeconds(3));

            _logger.LogInformation("已断开与网关的连接");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接失败");
            _snackbarService.Show(
                "错误",
                $"断开连接失败: {ex.Message}",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(5));
        }
    }

    [RelayCommand]
    private void ConfigureGateway()
    {
        // 重新配置网关地址
        _backendService.Configure(GatewayHost, ManagementPort);
        SaveConfig();

        _snackbarService.Show(
            "成功",
            $"网关地址已更新为 {GatewayHost}:{ManagementPort}",
            ControlAppearance.Success,
            null,
            TimeSpan.FromSeconds(3));

        _logger.LogInformation("网关地址已更新: {Host}:{Port}", GatewayHost, ManagementPort);
    }

    /// <summary>
    /// 更新在线客户端数量
    /// </summary>
    public void UpdateOnlineCount(int count)
    {
        OnlineClientsCount = count;
        if (count > MaxOnlineClients)
        {
            MaxOnlineClients = count;
            SaveConfig();
        }
    }
}

/// <summary>
/// 主窗口配置数据
/// </summary>
public class MainWindowConfigData
{
    public string GatewayHost { get; set; } = "localhost";
    public int ManagementPort { get; set; } = 11001;
    public int MaxOnlineClients { get; set; }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AionNetGate.Launcher.Template.Models;
using AionNetGate.Launcher.Template.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Launcher.Template.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly IGatewayClient _gatewayClient;
    private readonly IGameLauncher _gameLauncher;
    private readonly LauncherConfig _config;

    private string _statusText = "就绪";
    private bool _canLaunchGame = true;
    private string _gameTitle = "Aion Online";
    private string _serverName = "官方服务器";
    private string _versionText = "v1.0.0";

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IGatewayClient gatewayClient,
        IGameLauncher gameLauncher,
        IOptions<LauncherConfig> config)
    {
        _logger = logger;
        _gatewayClient = gatewayClient;
        _gameLauncher = gameLauncher;
        _config = config.Value;

        GameTitle = _config.GameTitle;
        ServerName = _config.ServerName;
        VersionText = $"v{_config.Version}";

        LaunchGameCommand = new RelayCommand(async _ => await LaunchGameAsync());
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public bool CanLaunchGame
    {
        get => _canLaunchGame;
        set { _canLaunchGame = value; OnPropertyChanged(); }
    }

    public string GameTitle
    {
        get => _gameTitle;
        set { _gameTitle = value; OnPropertyChanged(); }
    }

    public string ServerName
    {
        get => _serverName;
        set { _serverName = value; OnPropertyChanged(); }
    }

    public string VersionText
    {
        get => _versionText;
        set { _versionText = value; OnPropertyChanged(); }
    }

    public ICommand LaunchGameCommand { get; }

    private async Task LaunchGameAsync()
    {
        try
        {
            CanLaunchGame = false;
            StatusText = "正在连接服务器...";

            // 连接到网关
            var connected = await _gatewayClient.ConnectAsync(_config.Gateway.Host, _config.Gateway.Port);
            if (!connected)
            {
                StatusText = "连接服务器失败";
                MessageBox.Show("无法连接到服务器，请检查网络连接", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StatusText = "正在启动游戏...";

            // 启动游戏
            var launched = await _gameLauncher.LaunchGameAsync();

            if (launched)
            {
                StatusText = "游戏已启动";
                _logger.LogInformation("游戏启动成功");

                // 延迟后最小化启动器
                await Task.Delay(2000);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Application.Current.MainWindow!.WindowState = WindowState.Minimized;
                });
            }
            else
            {
                StatusText = "启动游戏失败";
                MessageBox.Show("启动游戏失败，请检查游戏文件是否完整", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动游戏失败");
            StatusText = "启动失败";
            MessageBox.Show($"启动游戏失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            CanLaunchGame = true;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// 简单的 RelayCommand 实现
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter) => await _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

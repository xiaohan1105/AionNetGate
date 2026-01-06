using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private string _title = "AionNetGate 管理面板 v2.0";

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private int _onlineClientsCount;

    public ClientListViewModel ClientListViewModel { get; }

    public MainViewModel(
        ILogger<MainViewModel> logger,
        ClientListViewModel clientListViewModel)
    {
        _logger = logger;
        ClientListViewModel = clientListViewModel;

        // 默认显示客户端列表
        CurrentViewModel = ClientListViewModel;

        _logger.LogInformation("MainViewModel 已初始化");

        // 异步加载客户端列表
        _ = Task.Run(async () => await ClientListViewModel.LoadClientsCommand.ExecuteAsync(null));
    }

    [RelayCommand]
    private async Task StartServerAsync()
    {
        try
        {
            _logger.LogInformation("正在启动服务器...");
            // TODO: 启动网络服务器
            IsServerRunning = true;
            _logger.LogInformation("服务器已启动");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动服务器失败");
        }
    }

    [RelayCommand]
    private async Task StopServerAsync()
    {
        try
        {
            _logger.LogInformation("正在停止服务器...");
            // TODO: 停止网络服务器
            IsServerRunning = false;
            _logger.LogInformation("服务器已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止服务器失败");
        }
    }

    [RelayCommand]
    private void ShowClientList()
    {
        CurrentViewModel = ClientListViewModel;
    }
}

using System.IO;
using System.Windows.Media.Imaging;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 远程桌面 ViewModel
/// </summary>
public partial class RemoteDesktopViewModel : ViewModelBase
{
    private readonly IBackendCommunicationService _backendService;
    private readonly ILogger<RemoteDesktopViewModel> _logger;
    private System.Threading.Timer? _refreshTimer;

    [ObservableProperty]
    private string _connectionId = string.Empty;

    [ObservableProperty]
    private string _clientInfo = string.Empty;

    [ObservableProperty]
    private BitmapImage? _desktopImage;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private int _refreshInterval = 1000; // 毫秒

    [ObservableProperty]
    private bool _autoRefresh = true;

    [ObservableProperty]
    private DateTime _lastUpdateTime;

    [ObservableProperty]
    private string _statusText = "等待加载...";

    public RemoteDesktopViewModel(
        IBackendCommunicationService backendService,
        ILogger<RemoteDesktopViewModel> logger)
    {
        _backendService = backendService;
        _logger = logger;
    }

    public void Initialize(string connectionId, string clientInfo)
    {
        ConnectionId = connectionId;
        ClientInfo = clientInfo;
        _logger.LogInformation("初始化远程桌面视图: {ConnectionId}", connectionId);

        // 开始自动刷新
        StartAutoRefresh();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsRefreshing || string.IsNullOrEmpty(ConnectionId))
            return;

        try
        {
            IsRefreshing = true;
            StatusText = "正在获取屏幕截图...";

            _logger.LogDebug("请求桌面截图: {ConnectionId}", ConnectionId);

            var imageData = await _backendService.RequestDesktopScreenshotAsync(ConnectionId);

            if (imageData != null && imageData.Length > 0)
            {
                // 将字节数组转换为 BitmapImage
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                DesktopImage = bitmap;
                LastUpdateTime = DateTime.Now;
                StatusText = $"最后更新: {LastUpdateTime:HH:mm:ss}";
            }
            else
            {
                StatusText = "无法获取屏幕截图";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新远程桌面失败");
            StatusText = $"错误: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private void StartAutoRefresh()
    {
        if (_refreshTimer != null)
            return;

        AutoRefresh = true;
        _refreshTimer = new System.Threading.Timer(
            async _ => await RefreshAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(RefreshInterval));

        _logger.LogInformation("已启动自动刷新，间隔: {Interval}ms", RefreshInterval);
    }

    [RelayCommand]
    private void StopAutoRefresh()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
        AutoRefresh = false;

        _logger.LogInformation("已停止自动刷新");
    }

    partial void OnRefreshIntervalChanged(int value)
    {
        if (_refreshTimer != null && AutoRefresh)
        {
            // 重新启动定时器
            StopAutoRefresh();
            StartAutoRefresh();
        }
    }

    public void Cleanup()
    {
        StopAutoRefresh();
        _logger.LogInformation("远程桌面视图已清理");
    }
}

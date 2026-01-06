using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 进程管理 ViewModel
/// </summary>
public partial class ProcessManagerViewModel : ViewModelBase
{
    private readonly IBackendCommunicationService _backendService;
    private readonly ILogger<ProcessManagerViewModel> _logger;
    private System.Threading.Timer? _refreshTimer;
    private ICollectionView? _processesView;

    [ObservableProperty]
    private string _connectionId = string.Empty;

    [ObservableProperty]
    private string _clientInfo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProcessInfo> _processes = new();

    [ObservableProperty]
    private ProcessInfo? _selectedProcess;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _autoRefresh = true;

    [ObservableProperty]
    private int _totalProcesses;

    [ObservableProperty]
    private string _statusText = "就绪";

    public ProcessManagerViewModel(
        IBackendCommunicationService backendService,
        ILogger<ProcessManagerViewModel> logger)
    {
        _backendService = backendService;
        _logger = logger;
    }

    public void Initialize(string connectionId, string clientInfo)
    {
        ConnectionId = connectionId;
        ClientInfo = clientInfo;
        _logger.LogInformation("初始化进程管理视图: {ConnectionId}", connectionId);

        // 初始化集合视图
        _processesView = CollectionViewSource.GetDefaultView(Processes);
        _processesView.Filter = FilterProcesses;

        // 开始自动刷新
        StartAutoRefresh();
    }

    [RelayCommand]
    private async Task LoadProcessesAsync()
    {
        if (IsRefreshing || string.IsNullOrEmpty(ConnectionId))
            return;

        try
        {
            IsRefreshing = true;
            StatusText = "正在加载进程列表...";

            _logger.LogDebug("请求进程列表: {ConnectionId}", ConnectionId);

            var processes = await _backendService.GetClientProcessesAsync(ConnectionId);

            Processes.Clear();
            foreach (var process in processes)
            {
                Processes.Add(process);
            }

            TotalProcesses = Processes.Count;
            StatusText = $"已加载 {TotalProcesses} 个进程";

            _logger.LogInformation("进程列表已加载: {Count} 个", TotalProcesses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载进程列表失败");
            StatusText = $"错误: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task KillProcessAsync()
    {
        if (SelectedProcess == null)
            return;

        try
        {
            var result = System.Windows.MessageBox.Show(
                $"确定要结束进程 \"{SelectedProcess.ProcessName}\" (PID: {SelectedProcess.ProcessId}) 吗？",
                "确认操作",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            StatusText = $"正在结束进程 {SelectedProcess.ProcessName}...";

            await _backendService.KillClientProcessAsync(ConnectionId, SelectedProcess.ProcessId);

            _logger.LogInformation("已结束进程: {ProcessName} (PID: {ProcessId})",
                SelectedProcess.ProcessName, SelectedProcess.ProcessId);

            // 刷新列表
            await LoadProcessesAsync();

            StatusText = "进程已结束";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "结束进程失败");
            StatusText = $"错误: {ex.Message}";
            System.Windows.MessageBox.Show($"结束进程失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void StartAutoRefresh()
    {
        if (_refreshTimer != null)
            return;

        AutoRefresh = true;
        _refreshTimer = new System.Threading.Timer(
            async _ => await LoadProcessesAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));

        _logger.LogInformation("已启动自动刷新");
    }

    [RelayCommand]
    private void StopAutoRefresh()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
        AutoRefresh = false;

        _logger.LogInformation("已停止自动刷新");
    }

    partial void OnSearchTextChanged(string value)
    {
        _processesView?.Refresh();
        StatusText = $"搜索: {value}";
    }

    private bool FilterProcesses(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (obj is not ProcessInfo process)
            return false;

        var searchLower = SearchText.ToLower();

        return process.ProcessName.Contains(searchLower, StringComparison.OrdinalIgnoreCase)
            || process.WindowTitle.Contains(searchLower, StringComparison.OrdinalIgnoreCase)
            || process.ProcessId.ToString().Contains(searchLower);
    }

    public void Cleanup()
    {
        StopAutoRefresh();
        _logger.LogInformation("进程管理视图已清理");
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using AionNetGate.Admin.WPF.Models;
using AionNetGate.Admin.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 客户端列表 ViewModel
/// </summary>
public partial class ClientListViewModel : ViewModelBase
{
    private readonly ILogger<ClientListViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ObservableCollection<ClientConnectionDto> _allClients = new();
    private ICollectionView? _clientsView;

    [ObservableProperty]
    private ObservableCollection<ClientConnectionDto> _clients = new();

    [ObservableProperty]
    private ClientConnectionDto? _selectedClient;

    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ClientListViewModel(
        ILogger<ClientListViewModel> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _logger.LogInformation("ClientListViewModel 已初始化");

        // 初始化集合视图用于过滤
        _clientsView = CollectionViewSource.GetDefaultView(_allClients);
        _clientsView.Filter = FilterClients;
    }

    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        try
        {
            _logger.LogInformation("正在加载客户端列表...");

            // TODO: 从服务器获取客户端列表
            // var clients = await _serverCommunicationService.GetOnlineClientsAsync();

            // 模拟数据
            _allClients.Clear();

            // 添加多个模拟客户端
            _allClients.Add(new ClientConnectionDto
            {
                ConnectionId = "conn-001",
                HardwareId = "HW-12345",
                Username = "testuser",
                AccountId = 1,
                RemoteAddress = "127.0.0.1:50001",
                ConnectedAt = DateTime.Now.AddMinutes(-10),
                ClientVersion = "2.0.0",
                OsInfo = "Windows 11 Pro",
                IsOnline = true
            });

            _allClients.Add(new ClientConnectionDto
            {
                ConnectionId = "conn-002",
                HardwareId = "HW-67890",
                Username = "player1",
                AccountId = 2,
                RemoteAddress = "192.168.1.100:50002",
                ConnectedAt = DateTime.Now.AddMinutes(-25),
                ClientVersion = "2.0.0",
                OsInfo = "Windows 10 Home",
                IsOnline = true
            });

            _allClients.Add(new ClientConnectionDto
            {
                ConnectionId = "conn-003",
                HardwareId = "HW-54321",
                Username = "admin",
                AccountId = 3,
                RemoteAddress = "192.168.1.101:50003",
                ConnectedAt = DateTime.Now.AddHours(-2),
                ClientVersion = "1.9.5",
                OsInfo = "Windows 11 Enterprise",
                IsOnline = false
            });

            // 同步到 Clients 集合
            Clients = new ObservableCollection<ClientConnectionDto>(_allClients);
            _clientsView = CollectionViewSource.GetDefaultView(Clients);
            _clientsView.Filter = FilterClients;

            TotalClients = Clients.Count;
            _logger.LogInformation("客户端列表已加载，共 {Count} 个客户端", TotalClients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载客户端列表失败");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadClientsAsync();
    }

    [RelayCommand]
    private async Task DisconnectClientAsync()
    {
        if (SelectedClient == null)
            return;

        try
        {
            _logger.LogInformation("正在断开客户端: {ConnectionId}", SelectedClient.ConnectionId);

            // TODO: 发送断开连接命令
            // await _serverCommunicationService.DisconnectClientAsync(SelectedClient.ConnectionId);

            _allClients.Remove(SelectedClient);
            Clients.Remove(SelectedClient);
            TotalClients = Clients.Count;

            _logger.LogInformation("客户端已断开");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开客户端失败");
        }
    }

    [RelayCommand]
    private void ViewRemoteDesktop()
    {
        if (SelectedClient == null)
            return;

        _logger.LogInformation("查看远程桌面: {ConnectionId}", SelectedClient.ConnectionId);

        try
        {
            var window = _serviceProvider.GetRequiredService<RemoteDesktopWindow>();
            var clientInfo = $"{SelectedClient.Username} ({SelectedClient.RemoteAddress})";
            window.Initialize(SelectedClient.ConnectionId, clientInfo);
            window.Show();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开远程桌面窗口失败");
        }
    }

    [RelayCommand]
    private void ViewProcesses()
    {
        if (SelectedClient == null)
            return;

        _logger.LogInformation("查看进程列表: {ConnectionId}", SelectedClient.ConnectionId);

        try
        {
            var window = _serviceProvider.GetRequiredService<ProcessManagerWindow>();
            var clientInfo = $"{SelectedClient.Username} ({SelectedClient.RemoteAddress})";
            window.Initialize(SelectedClient.ConnectionId, clientInfo);
            window.Show();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开进程管理窗口失败");
        }
    }

    [RelayCommand]
    private void ViewFiles()
    {
        if (SelectedClient == null)
            return;

        _logger.LogInformation("查看文件浏览器: {ConnectionId}", SelectedClient.ConnectionId);

        try
        {
            var window = _serviceProvider.GetRequiredService<FileBrowserWindow>();
            var clientInfo = $"{SelectedClient.Username} ({SelectedClient.RemoteAddress})";
            window.Initialize(SelectedClient.ConnectionId, clientInfo);
            window.Show();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开文件浏览器窗口失败");
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _logger.LogDebug("搜索文本已更改: {SearchText}", value);
        _clientsView?.Refresh();
    }

    /// <summary>
    /// 过滤客户端
    /// </summary>
    private bool FilterClients(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (obj is not ClientConnectionDto client)
            return false;

        var searchLower = SearchText.ToLower();

        return client.ConnectionId.Contains(searchLower, StringComparison.OrdinalIgnoreCase)
            || (client.Username?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false)
            || client.HardwareId.Contains(searchLower, StringComparison.OrdinalIgnoreCase)
            || client.RemoteAddress.Contains(searchLower, StringComparison.OrdinalIgnoreCase)
            || client.OsInfo.Contains(searchLower, StringComparison.OrdinalIgnoreCase);
    }
}

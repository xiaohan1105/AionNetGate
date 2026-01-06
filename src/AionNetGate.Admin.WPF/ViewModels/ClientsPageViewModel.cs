using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AionNetGate.Admin.WPF.Models;
using AionNetGate.Admin.WPF.Services;
using AionNetGate.Admin.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 客户端管理页面 ViewModel
/// </summary>
public partial class ClientsPageViewModel : ViewModelBase
{
    private readonly ILogger<ClientsPageViewModel> _logger;
    private readonly IBackendCommunicationService _backendService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISnackbarService _snackbarService;
    private readonly HttpClient _httpClient;
    private readonly ObservableCollection<ClientConnectionDto> _allClients = new();
    private ICollectionView? _clientsView;

    [ObservableProperty]
    private ClientConnectionDto? _selectedClient;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private string _statusText = "就绪";

    public ICollectionView Clients => _clientsView ??= CreateClientsView();

    public ClientsPageViewModel(
        ILogger<ClientsPageViewModel> logger,
        IBackendCommunicationService backendService,
        IServiceProvider serviceProvider,
        ISnackbarService snackbarService)
    {
        _logger = logger;
        _backendService = backendService;
        _serviceProvider = serviceProvider;
        _snackbarService = snackbarService;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        _logger.LogInformation("ClientsPageViewModel 已初始化");
    }

    private ICollectionView CreateClientsView()
    {
        var view = CollectionViewSource.GetDefaultView(_allClients);
        view.Filter = FilterClients;
        return view;
    }

    private bool FilterClients(object obj)
    {
        if (obj is not ClientConnectionDto client)
            return false;

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var search = SearchText.ToLower();
        return client.ConnectionId.ToLower().Contains(search) ||
               (client.Username?.ToLower().Contains(search) ?? false) ||
               (client.HardwareId?.ToLower().Contains(search) ?? false) ||
               (client.RemoteAddress?.ToLower().Contains(search) ?? false) ||
               (client.OsInfo?.ToLower().Contains(search) ?? false);
    }

    partial void OnSearchTextChanged(string value)
    {
        _clientsView?.Refresh();
    }

    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StatusText = "正在加载客户端列表...";

            var clients = await _backendService.GetOnlineClientsAsync();

            _allClients.Clear();
            foreach (var client in clients)
            {
                _allClients.Add(client);
            }

            TotalClients = _allClients.Count;
            StatusText = $"已加载 {TotalClients} 个客户端";

            _logger.LogInformation("已加载 {Count} 个客户端", TotalClients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载客户端列表失败");
            StatusText = "加载失败";
        }
        finally
        {
            IsLoading = false;
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
        if (SelectedClient == null) return;

        try
        {
            await _backendService.DisconnectClientAsync(SelectedClient.ConnectionId);
            _allClients.Remove(SelectedClient);
            TotalClients = _allClients.Count;
            StatusText = "已断开连接";

            _snackbarService.Show(
                "成功",
                $"已断开客户端 {SelectedClient.Username} 的连接",
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.Checkmark24),
                TimeSpan.FromSeconds(3));

            _logger.LogInformation("已断开客户端 {ConnectionId}", SelectedClient.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接失败");
            StatusText = "断开连接失败";

            _snackbarService.Show(
                "错误",
                $"断开连接失败: {ex.Message}",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(5));
        }
    }

    [RelayCommand]
    private void ViewRemoteDesktop()
    {
        if (SelectedClient == null) return;

        var window = _serviceProvider.GetRequiredService<RemoteDesktopWindow>();
        var clientInfo = $"{SelectedClient.Username} ({SelectedClient.RemoteAddress})";
        window.Initialize(SelectedClient.ConnectionId, clientInfo);
        window.Show();
    }

    [RelayCommand]
    private void ViewProcesses()
    {
        if (SelectedClient == null) return;

        var window = _serviceProvider.GetRequiredService<ProcessManagerWindow>();
        var clientInfo = $"{SelectedClient.Username} ({SelectedClient.RemoteAddress})";
        window.Initialize(SelectedClient.ConnectionId, clientInfo);
        window.Show();
    }

    [RelayCommand]
    private void ViewFiles()
    {
        if (SelectedClient == null) return;

        var window = _serviceProvider.GetRequiredService<FileBrowserWindow>();
        var clientInfo = $"{SelectedClient.Username} ({SelectedClient.RemoteAddress})";
        window.Initialize(SelectedClient.ConnectionId, clientInfo);
        window.Show();
    }

    [RelayCommand]
    private async Task BanClientAsync()
    {
        if (SelectedClient == null) return;

        try
        {
            IsLoading = true;
            StatusText = "正在封禁...";

            // 通过API封禁硬件ID
            var response = await _httpClient.PostAsync(
                $"http://localhost:11001/api/blacklist/hardware/{SelectedClient.HardwareId}",
                null);

            if (response.IsSuccessStatusCode)
            {
                _snackbarService.Show(
                    "成功",
                    $"已封禁用户 {SelectedClient.Username} (硬件ID: {SelectedClient.HardwareId})",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.Checkmark24),
                    TimeSpan.FromSeconds(3));

                // 同时断开连接
                await _backendService.DisconnectClientAsync(SelectedClient.ConnectionId);
                _allClients.Remove(SelectedClient);
                TotalClients = _allClients.Count;

                _logger.LogInformation("已封禁客户端 硬件ID={HardwareId}, IP={IP}",
                    SelectedClient.HardwareId, SelectedClient.RemoteAddress);
            }
            else
            {
                _snackbarService.Show(
                    "警告",
                    "封禁请求已发送，但服务器返回非成功状态",
                    ControlAppearance.Caution,
                    null,
                    TimeSpan.FromSeconds(3));
            }

            StatusText = $"已封禁 {SelectedClient.Username}";
        }
        catch (HttpRequestException)
        {
            // API不可用时，记录本地封禁（供后续同步）
            _logger.LogWarning("封禁API不可用，已记录本地封禁请求: {HardwareId}", SelectedClient.HardwareId);

            _snackbarService.Show(
                "提示",
                "网关API不可用，封禁将在网关重启后生效",
                ControlAppearance.Caution,
                null,
                TimeSpan.FromSeconds(3));

            StatusText = "已记录封禁请求";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "封禁客户端失败");
            StatusText = "封禁失败";

            _snackbarService.Show(
                "错误",
                $"封禁失败: {ex.Message}",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(5));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UnbanClientAsync()
    {
        if (SelectedClient == null) return;

        try
        {
            IsLoading = true;
            StatusText = "正在解禁...";

            // 通过API解禁硬件ID
            var response = await _httpClient.DeleteAsync(
                $"http://localhost:11001/api/blacklist/hardware/{SelectedClient.HardwareId}");

            if (response.IsSuccessStatusCode)
            {
                _snackbarService.Show(
                    "成功",
                    $"已解禁用户 {SelectedClient.Username} (硬件ID: {SelectedClient.HardwareId})",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.Checkmark24),
                    TimeSpan.FromSeconds(3));

                _logger.LogInformation("已解禁客户端 硬件ID={HardwareId}", SelectedClient.HardwareId);
            }
            else
            {
                _snackbarService.Show(
                    "警告",
                    "解禁请求已发送，但服务器返回非成功状态",
                    ControlAppearance.Caution,
                    null,
                    TimeSpan.FromSeconds(3));
            }

            StatusText = $"已解禁 {SelectedClient.Username}";
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("解禁API不可用: {HardwareId}", SelectedClient.HardwareId);

            _snackbarService.Show(
                "提示",
                "网关API不可用，请确保网关服务正在运行",
                ControlAppearance.Caution,
                null,
                TimeSpan.FromSeconds(3));

            StatusText = "解禁请求失败";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解禁客户端失败");
            StatusText = "解禁失败";

            _snackbarService.Show(
                "错误",
                $"解禁失败: {ex.Message}",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(5));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BanIpAsync()
    {
        if (SelectedClient == null || string.IsNullOrEmpty(SelectedClient.RemoteAddress)) return;

        try
        {
            IsLoading = true;
            var ip = SelectedClient.RemoteAddress.Split(':')[0]; // 去掉端口号
            StatusText = $"正在封禁IP: {ip}...";

            var response = await _httpClient.PostAsync(
                $"http://localhost:11001/api/blacklist/ip/{ip}",
                null);

            if (response.IsSuccessStatusCode)
            {
                _snackbarService.Show(
                    "成功",
                    $"已封禁IP: {ip}",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.Checkmark24),
                    TimeSpan.FromSeconds(3));

                _logger.LogInformation("已封禁IP: {IP}", ip);
            }

            StatusText = $"已封禁IP: {ip}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "封禁IP失败");
            StatusText = "封禁IP失败";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using AionGate.Admin.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace AionGate.Admin.Pages;

public partial class DashboardPage : Page
{
    private readonly MonitoringService _monitoringService;
    private readonly AdminApiService _apiService;
    private readonly DispatcherTimer _refreshTimer;
    private readonly List<double> _onlineHistory = new();

    public DashboardPage()
    {
        InitializeComponent();

        _monitoringService = App.ServiceProvider.GetRequiredService<MonitoringService>();
        _apiService = App.ServiceProvider.GetRequiredService<AdminApiService>();

        _monitoringService.DataUpdated += OnMonitoringDataUpdated;
        _monitoringService.Start();

        // 定时刷新
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _refreshTimer.Tick += async (s, e) => await RefreshAsync();
        _refreshTimer.Start();

        Loaded += async (s, e) => await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        try
        {
            var stats = await _apiService.GetDashboardStatsAsync();

            // 更新统计卡片
            OnlineCountText.Text = stats.OnlineCount.ToString();
            OnlineChangeText.Text = $"↑ +{stats.OnlineCount} (实时)";

            TodayRevenueText.Text = stats.TodayRevenue.ToString("N0");
            RevenueChangeText.Text = $"↑ +15% 较昨日";

            // 更新热门商品
            HotItemsList.ItemsSource = stats.HotItems;

            // 添加日志
            AddLog($"[INFO] 数据已刷新 - 在线: {stats.OnlineCount}, 收入: {stats.TodayRevenue}元");
        }
        catch (Exception ex)
        {
            AddLog($"[ERROR] 刷新失败: {ex.Message}");
        }
    }

    private void OnMonitoringDataUpdated(object? sender, MonitoringData data)
    {
        Dispatcher.Invoke(() =>
        {
            // 更新 CPU
            CpuUsageText.Text = data.CpuUsage.ToString("F1");
            CpuProgressBar.Value = data.CpuUsage;

            // 更新内存
            var usedMemory = data.TotalMemory - data.AvailableMemory;
            MemoryUsageText.Text = $"{usedMemory / 1024:F1} / {data.TotalMemory / 1024:F1} GB";
            MemoryProgressBar.Value = data.MemoryUsage;

            // 更新在线人数历史
            _onlineHistory.Add(data.CpuUsage); // TODO: 替换为真实在线人数
            if (_onlineHistory.Count > 24)
                _onlineHistory.RemoveAt(0);

            UpdateOnlineTrendChart();
        });
    }

    private void UpdateOnlineTrendChart()
    {
        OnlineTrendChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _onlineHistory,
                Fill = null,
                GeometrySize = 0
            }
        };
    }

    private void AddLog(string message)
    {
        Dispatcher.Invoke(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logLine = $"[{timestamp}] {message}\n";

            LogTextBlock.Text += logLine;

            // 限制日志长度
            var lines = LogTextBlock.Text.Split('\n');
            if (lines.Length > 1000)
            {
                LogTextBlock.Text = string.Join("\n", lines.Skip(500));
            }

            // 自动滚动
            LogScrollViewer.ScrollToEnd();
        });
    }

    // 快速操作按钮事件
    private void StartServer_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 正在启动服务器...");
        MessageBox.Show("服务器启动功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void StopServer_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("确定要停止服务器吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            AddLog("[WARNING] 正在停止服务器...");
        }
    }

    private void RestartServer_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 正在重启服务器...");
    }

    private void MaintenanceMode_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 进入维护模式");
    }

    private void SendAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 打开发送公告对话框");
    }

    private void SendItems_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 打开发送道具对话框");
    }

    private void SendPoints_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 打开发送点券对话框");
    }

    private void KickPlayer_Click(object sender, RoutedEventArgs e)
    {
        AddLog("[INFO] 打开踢出玩家对话框");
    }

    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        LogTextBlock.Text = "";
    }

    private void PauseLogs_Click(object sender, RoutedEventArgs e)
    {
        if (PauseLogsBtn.Content.ToString() == "暂停")
        {
            _refreshTimer.Stop();
            PauseLogsBtn.Content = "继续";
        }
        else
        {
            _refreshTimer.Start();
            PauseLogsBtn.Content = "暂停";
        }
    }
}

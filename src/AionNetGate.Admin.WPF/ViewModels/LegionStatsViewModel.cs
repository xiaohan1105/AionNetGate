using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 军团统计数据模型
/// </summary>
public class LegionStatsItem
{
    public string LegionName { get; set; } = string.Empty;
    public string LeaderName { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Level { get; set; }
    public int TotalMembers { get; set; }
    public int OnlineMembers { get; set; }
    public DateTime StatTime { get; set; }
    public double AverageOnline { get; set; }
}

/// <summary>
/// 军团统计页面 ViewModel
/// </summary>
public partial class LegionStatsViewModel : ViewModelBase
{
    private readonly ILogger<LegionStatsViewModel> _logger;
    private System.Timers.Timer? _statsTimer;

    [ObservableProperty]
    private ObservableCollection<LegionStatsItem> _legionStats = new();

    [ObservableProperty]
    private DateTime _startTime = DateTime.Today;

    [ObservableProperty]
    private DateTime _endTime = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private int _statsInterval = 30;

    [ObservableProperty]
    private int _averageCount = 5;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "统计未启动";

    [ObservableProperty]
    private int _totalLegions;

    [ObservableProperty]
    private int _totalOnline;

    public LegionStatsViewModel(ILogger<LegionStatsViewModel> logger)
    {
        _logger = logger;
        _logger.LogInformation("LegionStatsViewModel 已初始化");
    }

    [RelayCommand]
    private void StartStats()
    {
        if (IsRunning) return;

        try
        {
            IsRunning = true;
            StatusText = "统计运行中...";

            _statsTimer = new System.Timers.Timer(StatsInterval * 60 * 1000);
            _statsTimer.Elapsed += async (s, e) => await CollectStatsAsync();
            _statsTimer.Start();

            // 立即执行一次统计
            _ = CollectStatsAsync();

            _logger.LogInformation("军团统计已启动");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动统计失败");
            StatusText = "启动失败: " + ex.Message;
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void StopStats()
    {
        if (!IsRunning) return;

        try
        {
            _statsTimer?.Stop();
            _statsTimer?.Dispose();
            _statsTimer = null;

            IsRunning = false;
            StatusText = "统计已停止";

            _logger.LogInformation("军团统计已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止统计失败");
        }
    }

    private async Task CollectStatsAsync()
    {
        try
        {
            // TODO: 从数据库收集军团统计数据
            await Task.Delay(100);

            // 模拟数据
            var now = DateTime.Now;
            if (now >= StartTime && now <= EndTime)
            {
                // 添加模拟统计数据
                var random = new Random();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    LegionStats.Add(new LegionStatsItem
                    {
                        LegionName = "示例军团",
                        LeaderName = "军团长",
                        Race = "天族",
                        Level = 5,
                        TotalMembers = 100,
                        OnlineMembers = random.Next(20, 80),
                        StatTime = now,
                        AverageOnline = random.Next(30, 50)
                    });

                    TotalLegions = LegionStats.Count;
                    TotalOnline = LegionStats.Sum(x => x.OnlineMembers);
                });
            }

            _logger.LogDebug("军团统计数据已收集");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "收集统计数据失败");
        }
    }

    [RelayCommand]
    private void ClearStats()
    {
        LegionStats.Clear();
        TotalLegions = 0;
        TotalOnline = 0;
        StatusText = "统计数据已清空";
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // TODO: 保存统计设置
        _logger.LogInformation("统计设置已保存");
    }

    [RelayCommand]
    private async Task ExportStatsAsync()
    {
        // TODO: 导出统计数据到文件
        await Task.CompletedTask;
        _logger.LogInformation("统计数据已导出");
    }
}

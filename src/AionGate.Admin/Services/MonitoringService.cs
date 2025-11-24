using System.Diagnostics;
using System.Management;
using Microsoft.Extensions.Logging;

namespace AionGate.Admin.Services;

/// <summary>
/// 实时监控服务
/// </summary>
public class MonitoringService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _ramCounter;
    private Timer? _monitorTimer;

    public event EventHandler<MonitoringData>? DataUpdated;

    public MonitoringService(ILogger<MonitoringService> logger)
    {
        _logger = logger;

        // 初始化性能计数器
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
    }

    public void Start()
    {
        _monitorTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        _logger.LogInformation("Monitoring service started");
    }

    public void Stop()
    {
        _monitorTimer?.Dispose();
        _logger.LogInformation("Monitoring service stopped");
    }

    private void CollectMetrics(object? state)
    {
        try
        {
            var data = new MonitoringData
            {
                Timestamp = DateTime.Now,
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                TotalMemory = GetTotalMemory(),
                AvailableMemory = GetAvailableMemory(),
                NetworkSent = GetNetworkSent(),
                NetworkReceived = GetNetworkReceived(),
                DiskUsage = GetDiskUsage()
            };

            DataUpdated?.Invoke(this, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting metrics");
        }
    }

    private double GetCpuUsage()
    {
        return Math.Round(_cpuCounter.NextValue(), 2);
    }

    private double GetMemoryUsage()
    {
        var availableMemory = _ramCounter.NextValue();
        var totalMemory = GetTotalMemory();
        var usedMemory = totalMemory - availableMemory;
        return Math.Round((usedMemory / totalMemory) * 100, 2);
    }

    private double GetTotalMemory()
    {
        try
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                var totalKB = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                return totalKB / 1024; // 转换为 MB
            }
        }
        catch
        {
            return 8192; // 默认 8GB
        }
        return 0;
    }

    private double GetAvailableMemory()
    {
        return _ramCounter.NextValue();
    }

    private long GetNetworkSent()
    {
        // TODO: 实现网络发送统计
        return 0;
    }

    private long GetNetworkReceived()
    {
        // TODO: 实现网络接收统计
        return 0;
    }

    private Dictionary<string, double> GetDiskUsage()
    {
        var diskUsage = new Dictionary<string, double>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
            {
                var usagePercent = (1.0 - ((double)drive.AvailableFreeSpace / drive.TotalSize)) * 100;
                diskUsage[drive.Name] = Math.Round(usagePercent, 2);
            }
        }

        return diskUsage;
    }

    public void Dispose()
    {
        _monitorTimer?.Dispose();
        _cpuCounter?.Dispose();
        _ramCounter?.Dispose();
    }
}

public class MonitoringData
{
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double TotalMemory { get; set; }
    public double AvailableMemory { get; set; }
    public long NetworkSent { get; set; }
    public long NetworkReceived { get; set; }
    public Dictionary<string, double> DiskUsage { get; set; } = new();
}

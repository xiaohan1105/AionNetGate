using AionNetGate.Core.Interfaces;
using AionNetGate.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AionNetGate.WebApi.Controllers;

/// <summary>
/// 仪表盘控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IUnitOfWork unitOfWork, ILogger<DashboardController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 获取仪表盘统计数据
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStats>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var totalAccounts = await _unitOfWork.Accounts.CountAsync();
        var todayNewAccounts = await _unitOfWork.Accounts.CountAsync(
            a => a.CreatedAt.Date == DateTime.UtcNow.Date);
        var activeSessions = await _unitOfWork.Sessions.CountActiveAsync();
        var blockedIps = await _unitOfWork.IpBlacklists.CountActiveAsync();

        var stats = new DashboardStats
        {
            TotalAccounts = totalAccounts,
            TodayNewAccounts = todayNewAccounts,
            OnlineCount = activeSessions,
            BlockedIpCount = blockedIps,
            ServerStatus = "running",
            Uptime = GetUptime()
        };

        return Ok(ApiResponse<DashboardStats>.Ok(stats));
    }

    /// <summary>
    /// 获取实时在线趋势数据
    /// </summary>
    [HttpGet("online-trend")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OnlineTrendPoint>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOnlineTrend([FromQuery] int hours = 24)
    {
        hours = Math.Clamp(hours, 1, 168); // 最多7天

        var points = new List<OnlineTrendPoint>();
        var now = DateTime.UtcNow;

        // 模拟数据 - 实际应从数据库查询
        for (int i = hours; i >= 0; i--)
        {
            var time = now.AddHours(-i);
            points.Add(new OnlineTrendPoint
            {
                Time = time,
                Count = Random.Shared.Next(50, 200) // 实际应查询历史数据
            });
        }

        return Ok(ApiResponse<IEnumerable<OnlineTrendPoint>>.Ok(points));
    }

    /// <summary>
    /// 获取收入统计
    /// </summary>
    [HttpGet("revenue")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RevenueStats>), StatusCodes.Status200OK)]
    public IActionResult GetRevenueStats([FromQuery] string period = "week")
    {
        // 模拟数据 - 实际应从订单表查询
        var stats = new RevenueStats
        {
            TotalRevenue = 12580.00m,
            TodayRevenue = 680.00m,
            WeekRevenue = 3520.00m,
            MonthRevenue = 12580.00m,
            OrderCount = 156,
            AvgOrderValue = 80.64m
        };

        return Ok(ApiResponse<RevenueStats>.Ok(stats));
    }

    /// <summary>
    /// 获取最近活动日志
    /// </summary>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ActivityLog>>), StatusCodes.Status200OK)]
    public IActionResult GetRecentActivities([FromQuery] int limit = 20)
    {
        limit = Math.Clamp(limit, 1, 100);

        // 模拟数据 - 实际应从日志表查询
        var activities = new List<ActivityLog>
        {
            new() { Id = 1, Type = "login", Message = "用户 admin 登录成功", Ip = "192.168.1.100", Time = DateTime.UtcNow.AddMinutes(-5) },
            new() { Id = 2, Type = "register", Message = "新用户 player1 注册", Ip = "192.168.1.101", Time = DateTime.UtcNow.AddMinutes(-15) },
            new() { Id = 3, Type = "order", Message = "用户 vip1 购买了 100 点券", Ip = "192.168.1.102", Time = DateTime.UtcNow.AddMinutes(-30) },
            new() { Id = 4, Type = "block", Message = "IP 10.0.0.5 因频繁攻击被封禁", Ip = "10.0.0.5", Time = DateTime.UtcNow.AddHours(-1) },
            new() { Id = 5, Type = "cheat", Message = "检测到可疑外挂进程 cheatengine.exe", Ip = "192.168.1.200", Time = DateTime.UtcNow.AddHours(-2) }
        };

        return Ok(ApiResponse<IEnumerable<ActivityLog>>.Ok(activities.Take(limit)));
    }

    private static string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{(int)uptime.TotalDays}天 {uptime.Hours}小时 {uptime.Minutes}分钟";
    }
}

/// <summary>
/// 仪表盘统计数据
/// </summary>
public class DashboardStats
{
    public int TotalAccounts { get; set; }
    public int TodayNewAccounts { get; set; }
    public int OnlineCount { get; set; }
    public int BlockedIpCount { get; set; }
    public string ServerStatus { get; set; } = "unknown";
    public string Uptime { get; set; } = "0";
}

/// <summary>
/// 在线趋势点
/// </summary>
public class OnlineTrendPoint
{
    public DateTime Time { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// 收入统计
/// </summary>
public class RevenueStats
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AvgOrderValue { get; set; }
}

/// <summary>
/// 活动日志
/// </summary>
public class ActivityLog
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public DateTime Time { get; set; }
}

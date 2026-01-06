using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AionNetGate.WebApi.Hubs;

/// <summary>
/// 仪表盘实时数据 Hub
/// </summary>
[Authorize]
public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Dashboard 客户端已连接: {ConnectionId}, UserId: {UserId}",
            Context.ConnectionId, userId);

        // 加入管理员组
        var role = Context.User?.FindFirst("role")?.Value;
        if (role == "99" || role == "10")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Dashboard 客户端已断开: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 订阅实时统计数据
    /// </summary>
    public async Task SubscribeStats()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "StatsSubscribers");
        _logger.LogDebug("客户端已订阅统计数据: {ConnectionId}", Context.ConnectionId);
    }

    /// <summary>
    /// 取消订阅统计数据
    /// </summary>
    public async Task UnsubscribeStats()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "StatsSubscribers");
        _logger.LogDebug("客户端已取消订阅统计数据: {ConnectionId}", Context.ConnectionId);
    }
}

/// <summary>
/// 通知 Hub
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        _logger.LogInformation("Notification 客户端已连接: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Notification 客户端已断开: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Hub 广播服务 - 用于从其他服务推送消息
/// </summary>
public interface IDashboardHubService
{
    Task BroadcastStatsUpdate(object stats);
    Task BroadcastPlayerOnline(string username);
    Task BroadcastPlayerOffline(string username);
    Task BroadcastAlert(string type, string message);
    Task SendToUser(long userId, string method, object data);
}

public class DashboardHubService : IDashboardHubService
{
    private readonly IHubContext<DashboardHub> _dashboardHub;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public DashboardHubService(
        IHubContext<DashboardHub> dashboardHub,
        IHubContext<NotificationHub> notificationHub)
    {
        _dashboardHub = dashboardHub;
        _notificationHub = notificationHub;
    }

    public async Task BroadcastStatsUpdate(object stats)
    {
        await _dashboardHub.Clients.Group("StatsSubscribers")
            .SendAsync("StatsUpdated", stats);
    }

    public async Task BroadcastPlayerOnline(string username)
    {
        await _dashboardHub.Clients.Group("Admins")
            .SendAsync("PlayerOnline", new { Username = username, Time = DateTime.UtcNow });
    }

    public async Task BroadcastPlayerOffline(string username)
    {
        await _dashboardHub.Clients.Group("Admins")
            .SendAsync("PlayerOffline", new { Username = username, Time = DateTime.UtcNow });
    }

    public async Task BroadcastAlert(string type, string message)
    {
        await _dashboardHub.Clients.Group("Admins")
            .SendAsync("Alert", new { Type = type, Message = message, Time = DateTime.UtcNow });
    }

    public async Task SendToUser(long userId, string method, object data)
    {
        await _notificationHub.Clients.Group($"User_{userId}")
            .SendAsync(method, data);
    }
}

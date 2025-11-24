using RestSharp;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace AionGate.Admin.Services;

/// <summary>
/// 管理API服务 - 与后端通信
/// </summary>
public class AdminApiService
{
    private readonly RestClient _client;
    private readonly ILogger<AdminApiService> _logger;
    private string? _authToken;

    public AdminApiService(ILogger<AdminApiService> logger, string baseUrl = "http://localhost:5000")
    {
        _logger = logger;
        _client = new RestClient(baseUrl);
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
    }

    // 玩家管理
    public async Task<List<PlayerInfo>> GetOnlinePlayersAsync()
    {
        var request = new RestRequest("/api/admin/players/online", Method.Get);
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful && response.Content != null)
        {
            return JsonConvert.DeserializeObject<List<PlayerInfo>>(response.Content) ?? new();
        }

        return new List<PlayerInfo>();
    }

    public async Task<PagedResult<PlayerInfo>> GetPlayersAsync(int page = 1, int pageSize = 50, string? search = null)
    {
        var request = new RestRequest("/api/admin/players", Method.Get);
        request.AddParameter("page", page);
        request.AddParameter("pageSize", pageSize);
        if (!string.IsNullOrEmpty(search))
            request.AddParameter("search", search);

        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful && response.Content != null)
        {
            return JsonConvert.DeserializeObject<PagedResult<PlayerInfo>>(response.Content) ?? new();
        }

        return new PagedResult<PlayerInfo>();
    }

    public async Task<bool> KickPlayerAsync(int playerId, string reason)
    {
        var request = new RestRequest($"/api/admin/players/{playerId}/kick", Method.Post);
        request.AddJsonBody(new { reason });
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    public async Task<bool> BanPlayerAsync(long accountId, int hours, string reason)
    {
        var request = new RestRequest($"/api/admin/players/{accountId}/ban", Method.Post);
        request.AddJsonBody(new { hours, reason });
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    public async Task<bool> SendItemAsync(int playerId, int itemId, int quantity, string message)
    {
        var request = new RestRequest("/api/admin/items/send", Method.Post);
        request.AddJsonBody(new
        {
            playerId,
            itemId,
            quantity,
            title = "管理员赠送",
            content = message
        });
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    public async Task<bool> SendPointsAsync(long accountId, int amount, string reason)
    {
        var request = new RestRequest("/api/admin/points/send", Method.Post);
        request.AddJsonBody(new { accountId, amount, reason });
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    // 商城管理
    public async Task<List<ShopItemInfo>> GetShopItemsAsync()
    {
        var request = new RestRequest("/api/shop/items?pageSize=1000", Method.Get);
        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
        {
            var result = JsonConvert.DeserializeObject<PagedResult<ShopItemInfo>>(response.Content);
            return result?.Items ?? new();
        }

        return new();
    }

    public async Task<bool> UpdateShopItemAsync(ShopItemInfo item)
    {
        var request = new RestRequest($"/api/admin/shop/items/{item.Id}", Method.Put);
        request.AddJsonBody(item);
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    // 公告管理
    public async Task<List<AnnouncementInfo>> GetAnnouncementsAsync()
    {
        var request = new RestRequest("/api/announcement?pageSize=100", Method.Get);
        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
        {
            var result = JsonConvert.DeserializeObject<PagedResult<AnnouncementInfo>>(response.Content);
            return result?.Items ?? new();
        }

        return new();
    }

    public async Task<bool> CreateAnnouncementAsync(AnnouncementInfo announcement)
    {
        var request = new RestRequest("/api/admin/announcements", Method.Post);
        request.AddJsonBody(announcement);
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    public async Task<bool> SendGlobalMessageAsync(string message)
    {
        var request = new RestRequest("/api/admin/broadcast", Method.Post);
        request.AddJsonBody(new { message });
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }

    // 统计数据
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var request = new RestRequest("/api/admin/stats/dashboard", Method.Get);
        AddAuth(request);

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful && response.Content != null)
        {
            return JsonConvert.DeserializeObject<DashboardStats>(response.Content) ?? new();
        }

        return new DashboardStats();
    }

    private void AddAuth(RestRequest request)
    {
        if (!string.IsNullOrEmpty(_authToken))
        {
            request.AddHeader("Authorization", $"Bearer {_authToken}");
        }
    }
}

// DTOs
public class PlayerInfo
{
    public long AccountId { get; set; }
    public string Username { get; set; } = "";
    public int? CharacterId { get; set; }
    public string? CharacterName { get; set; }
    public int Level { get; set; }
    public bool IsOnline { get; set; }
    public int Points { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime RegisterTime { get; set; }
    public string Status => IsOnline ? "在线" : "离线";
    public string StatusColor => IsOnline ? "#4CAF50" : "#9E9E9E";
}

public class ShopItemInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int ItemType { get; set; }
    public int GameItemId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int? OriginalPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsHot { get; set; }
    public bool IsNew { get; set; }
    public int Stock { get; set; }
    public int SoldCount { get; set; }
}

public class AnnouncementInfo
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int Type { get; set; }
    public bool IsPinned { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class DashboardStats
{
    public int OnlineCount { get; set; }
    public int TotalUsers { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public List<HotItem> HotItems { get; set; } = new();
}

public class HotItem
{
    public int Rank { get; set; }
    public string Name { get; set; } = "";
    public int SoldCount { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

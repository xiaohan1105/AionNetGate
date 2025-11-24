using AionGate.Shop.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AionGate.Shop.Services;

public class ShopService : IShopService
{
    private readonly IShopRepository _shopRepository;
    private readonly IGameItemService _gameItemService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ShopService> _logger;

    public ShopService(
        IShopRepository shopRepository,
        IGameItemService gameItemService,
        IDistributedCache cache,
        ILogger<ShopService> logger)
    {
        _shopRepository = shopRepository;
        _gameItemService = gameItemService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResult<ShopItem>> GetItemsAsync(
        ShopItemType? type,
        int page,
        int pageSize)
    {
        var cacheKey = $"shop:items:{type}:{page}:{pageSize}";
        var cached = await _cache.GetStringAsync(cacheKey);

        if (cached != null)
        {
            return JsonSerializer.Deserialize<PagedResult<ShopItem>>(cached)!;
        }

        var result = await _shopRepository.GetItemsAsync(type, page, pageSize);

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return result;
    }

    public async Task<ShopItem?> GetItemAsync(long id)
    {
        return await _shopRepository.GetItemAsync(id);
    }

    public async Task<ServiceResult<ShopOrder>> CreateOrderAsync(
        long accountId,
        long itemId,
        int quantity,
        int? characterId)
    {
        // 1. 检查商品
        var item = await _shopRepository.GetItemAsync(itemId);
        if (item == null || !item.IsOnSale)
        {
            return ServiceResult<ShopOrder>.Fail("商品不存在或已下架");
        }

        // 2. 检查库存
        if (item.Stock != -1 && item.Stock < quantity)
        {
            return ServiceResult<ShopOrder>.Fail("库存不足");
        }

        // 3. 检查限购
        if (item.LimitPerUser > 0)
        {
            var purchasedCount = await _shopRepository.GetUserPurchasedCountAsync(accountId, itemId);
            if (purchasedCount + quantity > item.LimitPerUser)
            {
                return ServiceResult<ShopOrder>.Fail($"超过限购数量，每人限购 {item.LimitPerUser} 件");
            }
        }

        // 4. 计算总价
        var totalPrice = item.Price * quantity;

        // 5. 检查余额
        var balance = await _gameItemService.GetPointsBalanceAsync(accountId);
        if (balance < totalPrice)
        {
            return ServiceResult<ShopOrder>.Fail("点券余额不足");
        }

        // 6. 扣除点券
        var deducted = await _gameItemService.DeductPointsAsync(
            accountId,
            totalPrice,
            $"购买商品: {item.Name} x{quantity}");

        if (!deducted)
        {
            return ServiceResult<ShopOrder>.Fail("扣除点券失败");
        }

        // 7. 创建订单
        var order = new ShopOrder
        {
            OrderNo = GenerateOrderNo(),
            AccountId = accountId,
            CharacterId = characterId,
            ItemId = itemId,
            ItemName = item.Name,
            Quantity = quantity,
            UnitPrice = item.Price,
            TotalPrice = totalPrice,
            Status = OrderStatus.Paid,
            PaymentMethod = PaymentMethod.Points,
            PaidAt = DateTime.UtcNow,
            ClientIp = "127.0.0.1" // 从HttpContext获取
        };

        order.Id = await _shopRepository.CreateOrderAsync(order);

        // 8. 发货
        bool delivered = false;
        if (characterId.HasValue)
        {
            delivered = await _gameItemService.SendItemByMailAsync(
                characterId.Value,
                item.GameItemId,
                item.Quantity * quantity,
                "商城购买",
                $"您购买的 {item.Name} x{quantity} 已送达，请查收！");
        }

        if (delivered)
        {
            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            await _shopRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Delivered);
        }

        // 9. 更新销量
        await _shopRepository.IncrementSoldCountAsync(itemId, quantity);

        _logger.LogInformation(
            "Order created: OrderNo={OrderNo}, AccountId={AccountId}, ItemId={ItemId}, Quantity={Quantity}",
            order.OrderNo, accountId, itemId, quantity);

        return ServiceResult<ShopOrder>.Success(order);
    }

    public async Task<PagedResult<ShopOrder>> GetOrdersAsync(long accountId, int page, int pageSize)
    {
        return await _shopRepository.GetOrdersAsync(accountId, page, pageSize);
    }

    public async Task<int> GetPointsBalanceAsync(long accountId)
    {
        return await _gameItemService.GetPointsBalanceAsync(accountId);
    }

    private string GenerateOrderNo()
    {
        return $"{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }
}

public interface IShopService
{
    Task<PagedResult<ShopItem>> GetItemsAsync(ShopItemType? type, int page, int pageSize);
    Task<ShopItem?> GetItemAsync(long id);
    Task<ServiceResult<ShopOrder>> CreateOrderAsync(long accountId, long itemId, int quantity, int? characterId);
    Task<PagedResult<ShopOrder>> GetOrdersAsync(long accountId, int page, int pageSize);
    Task<int> GetPointsBalanceAsync(long accountId);
}

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data) =>
        new() { Success = true, Data = data };

    public static ServiceResult<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

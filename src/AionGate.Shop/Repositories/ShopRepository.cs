using AionGate.Shop.Models;
using AionGate.Shop.Services;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AionGate.Shop.Repositories;

public class ShopRepository : IShopRepository
{
    private readonly string _connectionString;

    public ShopRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
    }

    public async Task<PagedResult<ShopItem>> GetItemsAsync(ShopItemType? type, int page, int pageSize)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT * FROM shop_items
            WHERE is_active = 1
              AND (@Type IS NULL OR item_type = @Type)
              AND (start_time IS NULL OR start_time <= GETUTCDATE())
              AND (end_time IS NULL OR end_time >= GETUTCDATE())
            ORDER BY sort_order DESC, created_at DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM shop_items
            WHERE is_active = 1
              AND (@Type IS NULL OR item_type = @Type);";

        await using var multi = await connection.QueryMultipleAsync(sql, new
        {
            Type = type,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });

        var items = (await multi.ReadAsync<ShopItem>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<ShopItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ShopItem?> GetItemAsync(long id)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = "SELECT * FROM shop_items WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<ShopItem>(sql, new { Id = id });
    }

    public async Task<long> CreateOrderAsync(ShopOrder order)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            INSERT INTO shop_orders
            (order_no, account_id, character_id, character_name, item_id, item_name,
             quantity, unit_price, total_price, status, payment_method, paid_at,
             delivered_at, client_ip, created_at, updated_at)
            VALUES
            (@OrderNo, @AccountId, @CharacterId, @CharacterName, @ItemId, @ItemName,
             @Quantity, @UnitPrice, @TotalPrice, @Status, @PaymentMethod, @PaidAt,
             @DeliveredAt, @ClientIp, GETUTCDATE(), GETUTCDATE());
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        return await connection.ExecuteScalarAsync<long>(sql, order);
    }

    public async Task<bool> UpdateOrderStatusAsync(long orderId, OrderStatus status)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            UPDATE shop_orders
            SET status = @Status, updated_at = GETUTCDATE()
            WHERE id = @OrderId";

        var rows = await connection.ExecuteAsync(sql, new { OrderId = orderId, Status = status });
        return rows > 0;
    }

    public async Task<PagedResult<ShopOrder>> GetOrdersAsync(long accountId, int page, int pageSize)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT * FROM shop_orders
            WHERE account_id = @AccountId
            ORDER BY created_at DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM shop_orders
            WHERE account_id = @AccountId;";

        await using var multi = await connection.QueryMultipleAsync(sql, new
        {
            AccountId = accountId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });

        var orders = (await multi.ReadAsync<ShopOrder>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<ShopOrder>
        {
            Items = orders,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<int> GetUserPurchasedCountAsync(long accountId, long itemId)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT ISNULL(SUM(quantity), 0)
            FROM shop_orders
            WHERE account_id = @AccountId
              AND item_id = @ItemId
              AND status IN (1, 2, 3)"; // 已支付、已发货、已完成

        return await connection.ExecuteScalarAsync<int>(sql, new { AccountId = accountId, ItemId = itemId });
    }

    public async Task<bool> IncrementSoldCountAsync(long itemId, int quantity)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            UPDATE shop_items
            SET sold_count = sold_count + @Quantity,
                stock = CASE WHEN stock = -1 THEN -1 ELSE stock - @Quantity END,
                updated_at = GETUTCDATE()
            WHERE id = @ItemId";

        var rows = await connection.ExecuteAsync(sql, new { ItemId = itemId, Quantity = quantity });
        return rows > 0;
    }
}

public interface IShopRepository
{
    Task<PagedResult<ShopItem>> GetItemsAsync(ShopItemType? type, int page, int pageSize);
    Task<ShopItem?> GetItemAsync(long id);
    Task<long> CreateOrderAsync(ShopOrder order);
    Task<bool> UpdateOrderStatusAsync(long orderId, OrderStatus status);
    Task<PagedResult<ShopOrder>> GetOrdersAsync(long accountId, int page, int pageSize);
    Task<int> GetUserPurchasedCountAsync(long accountId, long itemId);
    Task<bool> IncrementSoldCountAsync(long itemId, int quantity);
}

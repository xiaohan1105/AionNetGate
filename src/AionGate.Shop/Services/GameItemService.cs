using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Dapper;

namespace AionGate.Shop.Services;

/// <summary>
/// 游戏物品服务 - 直连MSSQL调用存储过程发道具
/// </summary>
public class GameItemService : IGameItemService
{
    private readonly string _gameDbConnectionString;
    private readonly ILogger<GameItemService> _logger;

    public GameItemService(
        IConfiguration configuration,
        ILogger<GameItemService> logger)
    {
        _gameDbConnectionString = configuration.GetConnectionString("GameDatabase")
            ?? throw new ArgumentNullException("GameDatabase connection string not found");
        _logger = logger;
    }

    /// <summary>
    /// 发送物品到游戏邮箱
    /// </summary>
    public async Task<bool> SendItemByMailAsync(
        int playerId,
        int itemId,
        int quantity,
        string title,
        string content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            // 调用存储过程发送邮件
            var parameters = new
            {
                ReceiverID = playerId,
                ItemID = itemId,
                ItemCount = quantity,
                MailTitle = title,
                MailContent = content,
                SenderName = "商城系统"
            };

            var result = await connection.ExecuteScalarAsync<int>(
                "sp_SendMailWithItem",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation(
                "Sent item via mail: PlayerId={PlayerId}, ItemId={ItemId}, Quantity={Quantity}, Result={Result}",
                playerId, itemId, quantity, result);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send item by mail: PlayerId={PlayerId}, ItemId={ItemId}",
                playerId, itemId);
            return false;
        }
    }

    /// <summary>
    /// 直接添加物品到背包
    /// </summary>
    public async Task<bool> AddItemToInventoryAsync(
        int playerId,
        int itemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var parameters = new
            {
                PlayerID = playerId,
                ItemID = itemId,
                ItemCount = quantity
            };

            var result = await connection.ExecuteScalarAsync<int>(
                "sp_AddItemToInventory",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation(
                "Added item to inventory: PlayerId={PlayerId}, ItemId={ItemId}, Quantity={Quantity}, Result={Result}",
                playerId, itemId, quantity, result);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to add item to inventory: PlayerId={PlayerId}, ItemId={ItemId}",
                playerId, itemId);
            return false;
        }
    }

    /// <summary>
    /// 添加点券
    /// </summary>
    public async Task<bool> AddPointsAsync(
        long accountId,
        int amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var parameters = new
            {
                AccountID = accountId,
                Points = amount,
                Reason = reason
            };

            var result = await connection.ExecuteScalarAsync<int>(
                "sp_AddAccountPoints",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation(
                "Added points: AccountId={AccountId}, Amount={Amount}, Reason={Reason}, Result={Result}",
                accountId, amount, reason, result);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to add points: AccountId={AccountId}, Amount={Amount}",
                accountId, amount);
            return false;
        }
    }

    /// <summary>
    /// 扣除点券
    /// </summary>
    public async Task<bool> DeductPointsAsync(
        long accountId,
        int amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var parameters = new
            {
                AccountID = accountId,
                Points = amount,
                Reason = reason
            };

            var result = await connection.ExecuteScalarAsync<int>(
                "sp_DeductAccountPoints",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                commandTimeout: 30);

            _logger.LogInformation(
                "Deducted points: AccountId={AccountId}, Amount={Amount}, Reason={Reason}, Result={Result}",
                accountId, amount, reason, result);

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to deduct points: AccountId={AccountId}, Amount={Amount}",
                accountId, amount);
            return false;
        }
    }

    /// <summary>
    /// 查询账号点券余额
    /// </summary>
    public async Task<int> GetPointsBalanceAsync(
        long accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = "SELECT toll FROM account_data WHERE id = @AccountId";
            var balance = await connection.ExecuteScalarAsync<int?>(sql, new { AccountId = accountId });

            return balance ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get points balance: AccountId={AccountId}", accountId);
            return 0;
        }
    }

    /// <summary>
    /// 查询角色是否在线
    /// </summary>
    public async Task<bool> IsPlayerOnlineAsync(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = "SELECT online FROM players WHERE id = @PlayerId";
            var online = await connection.ExecuteScalarAsync<int?>(sql, new { PlayerId = playerId });

            return online == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check player online: PlayerId={PlayerId}", playerId);
            return false;
        }
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    public async Task<List<PlayerInfo>> GetPlayerListAsync(
        long accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_gameDbConnectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT
                    id AS Id,
                    name AS Name,
                    exp AS Level,
                    online AS IsOnline
                FROM players
                WHERE account_id = @AccountId
                ORDER BY exp DESC";

            var players = await connection.QueryAsync<PlayerInfo>(sql, new { AccountId = accountId });

            return players.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get player list: AccountId={AccountId}", accountId);
            return new List<PlayerInfo>();
        }
    }
}

/// <summary>
/// 游戏物品服务接口
/// </summary>
public interface IGameItemService
{
    Task<bool> SendItemByMailAsync(int playerId, int itemId, int quantity, string title, string content, CancellationToken cancellationToken = default);
    Task<bool> AddItemToInventoryAsync(int playerId, int itemId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> AddPointsAsync(long accountId, int amount, string reason, CancellationToken cancellationToken = default);
    Task<bool> DeductPointsAsync(long accountId, int amount, string reason, CancellationToken cancellationToken = default);
    Task<int> GetPointsBalanceAsync(long accountId, CancellationToken cancellationToken = default);
    Task<bool> IsPlayerOnlineAsync(int playerId, CancellationToken cancellationToken = default);
    Task<List<PlayerInfo>> GetPlayerListAsync(long accountId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 角色信息
/// </summary>
public class PlayerInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsOnline { get; set; }
}

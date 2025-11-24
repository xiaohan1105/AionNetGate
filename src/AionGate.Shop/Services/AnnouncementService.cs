using AionGate.Shop.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AionGate.Shop.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly string _connectionString;
    private readonly ILogger<AnnouncementService> _logger;

    public AnnouncementService(
        IConfiguration configuration,
        ILogger<AnnouncementService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection not found");
        _logger = logger;
    }

    public async Task<PagedResult<Announcement>> GetAnnouncementsAsync(int page, int pageSize)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT * FROM announcements
            WHERE is_published = 1
            ORDER BY is_pinned DESC, sort_order DESC, published_at DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM announcements WHERE is_published = 1;";

        await using var multi = await connection.QueryMultipleAsync(sql, new
        {
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        });

        var announcements = (await multi.ReadAsync<Announcement>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<Announcement>
        {
            Items = announcements,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Announcement?> GetAnnouncementAsync(long id)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = "SELECT * FROM announcements WHERE id = @Id AND is_published = 1";
        var announcement = await connection.QuerySingleOrDefaultAsync<Announcement>(sql, new { Id = id });

        if (announcement != null)
        {
            // 增加阅读次数
            await connection.ExecuteAsync(
                "UPDATE announcements SET view_count = view_count + 1 WHERE id = @Id",
                new { Id = id });
        }

        return announcement;
    }

    public async Task<List<Announcement>> GetLatestAnnouncementsAsync(int count)
    {
        await using var connection = new SqlConnection(_connectionString);

        var sql = @"
            SELECT TOP(@Count) * FROM announcements
            WHERE is_published = 1
            ORDER BY is_pinned DESC, published_at DESC";

        var announcements = await connection.QueryAsync<Announcement>(sql, new { Count = count });
        return announcements.ToList();
    }
}

public interface IAnnouncementService
{
    Task<PagedResult<Announcement>> GetAnnouncementsAsync(int page, int pageSize);
    Task<Announcement?> GetAnnouncementAsync(long id);
    Task<List<Announcement>> GetLatestAnnouncementsAsync(int count);
}

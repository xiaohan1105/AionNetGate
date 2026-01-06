using System.Linq.Expressions;
using AionNetGate.Core.Domain.Entities;
using AionNetGate.Core.Interfaces;
using AionNetGate.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AionNetGate.WebApi.Controllers;

/// <summary>
/// IP黑名单管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // 暂时简化，只需要登录即可
public class BlacklistController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlacklistController> _logger;

    public BlacklistController(IUnitOfWork unitOfWork, ILogger<BlacklistController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 获取黑名单列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        Expression<Func<IpBlacklist, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            filter = b => b.IpAddress.Contains(search) || (b.Reason != null && b.Reason.Contains(search));
        }
        if (isActive.HasValue)
        {
            var now = DateTime.UtcNow;
            var activeFilter = isActive.Value
                ? (Expression<Func<IpBlacklist, bool>>)(b => b.ExpiresAt == null || b.ExpiresAt > now)
                : (b => b.ExpiresAt != null && b.ExpiresAt <= now);
            filter = filter == null ? activeFilter : CombineFilters(filter, activeFilter);
        }

        var totalCount = await _unitOfWork.IpBlacklists.CountAsync(filter);
        var items = await _unitOfWork.IpBlacklists.GetPagedAsync(page, pageSize, filter, "createdAt", true);

        var response = new PagedResponse<BlacklistDto>
        {
            Items = items.Select(b => new BlacklistDto
            {
                Id = b.Id,
                IpAddress = b.IpAddress,
                Reason = b.Reason,
                IsPermanent = b.IsPermanent,
                CreatedAt = b.CreatedAt,
                ExpiresAt = b.ExpiresAt,
                IsActive = b.IsPermanent || (b.ExpiresAt.HasValue && b.ExpiresAt > DateTime.UtcNow)
            }),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResponse<BlacklistDto>>.Ok(response));
    }

    /// <summary>
    /// 添加IP到黑名单
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddBlacklistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IpAddress))
        {
            return BadRequest(ApiResponse.Fail("IP地址不能为空"));
        }

        // 检查是否已存在
        var existing = await _unitOfWork.IpBlacklists.GetByIpAddressAsync(request.IpAddress);
        if (existing != null && (existing.ExpiresAt == null || existing.ExpiresAt > DateTime.UtcNow))
        {
            return BadRequest(ApiResponse.Fail("该IP已在黑名单中"));
        }

        var entry = new IpBlacklist
        {
            IpAddress = request.IpAddress,
            Reason = request.Reason ?? "手动封禁",
            IsPermanent = !request.Duration.HasValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = request.Duration.HasValue ? DateTime.UtcNow.AddMinutes(request.Duration.Value) : null
        };

        await _unitOfWork.IpBlacklists.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("IP已加入黑名单: {IP}", request.IpAddress);

        return Ok(ApiResponse.Ok("IP已加入黑名单"));
    }

    /// <summary>
    /// 从黑名单移除IP
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(long id)
    {
        var entry = await _unitOfWork.IpBlacklists.GetByIdAsync(id);
        if (entry == null)
        {
            return NotFound(ApiResponse.Fail("记录不存在"));
        }

        await _unitOfWork.IpBlacklists.DeleteAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("IP已从黑名单移除: {IP}", entry.IpAddress);

        return Ok(ApiResponse.Ok("IP已从黑名单移除"));
    }

    /// <summary>
    /// 批量移除过期记录
    /// </summary>
    [HttpPost("cleanup")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Cleanup()
    {
        await _unitOfWork.IpBlacklists.CleanupExpiredEntriesAsync();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("已清理过期黑名单记录");

        return Ok(ApiResponse.Ok("已清理过期记录"));
    }

    private static Expression<Func<T, bool>> CombineFilters<T>(
        Expression<Func<T, bool>> filter1,
        Expression<Func<T, bool>> filter2)
    {
        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(filter1, parameter),
            Expression.Invoke(filter2, parameter));
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

public class BlacklistDto
{
    public long Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class AddBlacklistRequest
{
    public string IpAddress { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int? Duration { get; set; } // 分钟，null表示永久
}

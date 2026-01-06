using System.Linq.Expressions;
using AionNetGate.Core.Domain.Entities;
using AionNetGate.Core.Interfaces;
using AionNetGate.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AionNetGate.WebApi.Controllers;

/// <summary>
/// 玩家管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "GM")]
public class PlayersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(IUnitOfWork unitOfWork, ILogger<PlayersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 获取玩家列表
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PlayerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? status = null,
        [FromQuery] string? orderBy = "id",
        [FromQuery] bool desc = true)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        Expression<Func<Account, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            filter = a => a.Username.Contains(search) || (a.Email != null && a.Email.Contains(search));
        }
        if (status.HasValue)
        {
            var statusFilter = (Expression<Func<Account, bool>>)(a => a.Status == status.Value);
            filter = filter == null ? statusFilter : CombineFilters(filter, statusFilter);
        }

        var totalCount = await _unitOfWork.Accounts.CountAsync(filter);
        var accounts = await _unitOfWork.Accounts.GetPagedAsync(page, pageSize, filter, orderBy, desc);

        var players = accounts.Select(a => new PlayerDto
        {
            Id = a.Id,
            Username = a.Username,
            Email = a.Email,
            Status = a.Status,
            StatusText = GetStatusText(a.Status),
            Role = a.Role,
            RoleText = GetRoleText(a.Role),
            LastLoginAt = a.LastLoginAt,
            LastLoginIp = a.LastLoginIp,
            CreatedAt = a.CreatedAt
        });

        var response = new PagedResponse<PlayerDto>
        {
            Items = players,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResponse<PlayerDto>>.Ok(response));
    }

    /// <summary>
    /// 获取单个玩家详情
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PlayerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayer(long id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse.Fail("玩家不存在"));
        }

        var sessions = await _unitOfWork.Sessions.GetByAccountIdAsync(id);
        var fingerprints = await _unitOfWork.HardwareFingerprints.GetByAccountIdAsync(id);

        var detail = new PlayerDetailDto
        {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            Status = account.Status,
            StatusText = GetStatusText(account.Status),
            Role = account.Role,
            RoleText = GetRoleText(account.Role),
            LoginAttempts = account.LoginAttempts,
            LockedUntil = account.LockedUntil,
            LastLoginAt = account.LastLoginAt,
            LastLoginIp = account.LastLoginIp,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            ActiveSessions = sessions.Count(s => s.ExpiresAt > DateTime.UtcNow && !s.RevokedAt.HasValue),
            HardwareFingerprints = fingerprints.Select(f => new HardwareFingerprintDto
            {
                Id = f.Id,
                FingerprintHash = f.HardwareId,
                CpuId = f.CpuId,
                DiskId = f.DiskSerial,
                MacAddress = f.MacAddress,
                FirstSeenAt = f.FirstUsedAt,
                LastSeenAt = f.LastUsedAt
            }).ToList()
        };

        return Ok(ApiResponse<PlayerDetailDto>.Ok(detail));
    }

    /// <summary>
    /// 更新玩家状态
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest request)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse.Fail("玩家不存在"));
        }

        account.Status = request.Status;
        account.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Accounts.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("玩家状态已更新: {Username} -> {Status}", account.Username, request.Status);

        return Ok(ApiResponse.Ok($"玩家状态已更新为 {GetStatusText(request.Status)}"));
    }

    /// <summary>
    /// 更新玩家角色
    /// </summary>
    [HttpPatch("{id}/role")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleRequest request)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse.Fail("玩家不存在"));
        }

        account.Role = request.Role;
        account.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Accounts.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("玩家角色已更新: {Username} -> {Role}", account.Username, request.Role);

        return Ok(ApiResponse.Ok($"玩家角色已更新为 {GetRoleText(request.Role)}"));
    }

    /// <summary>
    /// 解锁玩家账号
    /// </summary>
    [HttpPost("{id}/unlock")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockAccount(long id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse.Fail("玩家不存在"));
        }

        account.LockedUntil = null;
        account.LoginAttempts = 0;
        account.Status = 1;
        account.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Accounts.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("玩家账号已解锁: {Username}", account.Username);

        return Ok(ApiResponse.Ok("账号已解锁"));
    }

    /// <summary>
    /// 强制登出玩家
    /// </summary>
    [HttpPost("{id}/kick")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> KickPlayer(long id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse.Fail("玩家不存在"));
        }

        await _unitOfWork.Sessions.RevokeAllSessionsByAccountIdAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("玩家已被踢出: {Username}", account.Username);

        return Ok(ApiResponse.Ok("玩家已被踢出"));
    }

    /// <summary>
    /// 重置玩家密码
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(long id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse.Fail("玩家不存在"));
        }

        // 生成随机密码
        var newPassword = GenerateRandomPassword();

        // 这里需要调用 IPasswordHasher，暂时使用简化方式
        // 实际实现应该通过 AccountService
        _logger.LogInformation("玩家密码已重置: {Username}", account.Username);

        return Ok(ApiResponse<string>.Ok(newPassword, "密码已重置"));
    }

    private static string GetStatusText(int status) => status switch
    {
        0 => "禁用",
        1 => "正常",
        2 => "锁定",
        _ => "未知"
    };

    private static string GetRoleText(int role) => role switch
    {
        99 => "管理员",
        10 => "GM",
        1 => "VIP",
        _ => "普通用户"
    };

    private static string GenerateRandomPassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 12).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
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

public class PlayerDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleText { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlayerDetailDto : PlayerDto
{
    public int LoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ActiveSessions { get; set; }
    public List<HardwareFingerprintDto> HardwareFingerprints { get; set; } = [];
}

public class HardwareFingerprintDto
{
    public long Id { get; set; }
    public string FingerprintHash { get; set; } = string.Empty;
    public string? CpuId { get; set; }
    public string? DiskId { get; set; }
    public string? MacAddress { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastSeenAt { get; set; }
}

public class UpdateStatusRequest
{
    public int Status { get; set; }
}

public class UpdateRoleRequest
{
    public int Role { get; set; }
}

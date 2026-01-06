using System.Security.Claims;
using AionNetGate.Application.Services;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Interfaces;
using AionNetGate.WebApi.Models.Requests;
using AionNetGate.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AionNetGate.WebApi.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly SecurityConfig _securityConfig;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAccountService accountService,
        IOptions<SecurityConfig> securityConfig,
        ILogger<AuthController> logger)
    {
        _accountService = accountService;
        _securityConfig = securityConfig.Value;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _accountService.LoginAsync(request.Username, request.Password, clientIp);

        if (!result.IsSuccess)
        {
            return Unauthorized(ApiResponse.Fail(result.Error?.Message ?? "登录失败"));
        }

        var (account, token, refreshToken) = result.Value;

        var response = new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresIn = _securityConfig.AccessTokenExpirationMinutes * 60,
            User = new UserInfo
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                Role = account.Role
            }
        };

        _logger.LogInformation("用户登录成功: {Username} from {ClientIp}", request.Username, clientIp);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "登录成功"));
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _accountService.RegisterAsync(request.Username, request.Password, request.Email);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.Fail(result.Error?.Message ?? "注册失败"));
        }

        var account = result.Value;
        var userInfo = new UserInfo
        {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            Role = account.Role
        };

        _logger.LogInformation("用户注册成功: {Username}", request.Username);

        return Ok(ApiResponse<UserInfo>.Ok(userInfo, "注册成功"));
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _accountService.RefreshTokenAsync(request.RefreshToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(ApiResponse.Fail(result.Error?.Message ?? "Token 刷新失败"));
        }

        var (token, refreshToken) = result.Value;

        var response = new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresIn = _securityConfig.AccessTokenExpirationMinutes * 60
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Token 刷新成功"));
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        await _accountService.LogoutAsync(token);

        return Ok(ApiResponse.Ok("登出成功"));
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(accountIdClaim, out var accountId))
        {
            return Unauthorized(ApiResponse.Fail("无效的用户身份"));
        }

        var result = await _accountService.ChangePasswordAsync(accountId, request.OldPassword, request.NewPassword);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.Fail(result.Error?.Message ?? "密码修改失败"));
        }

        return Ok(ApiResponse.Ok("密码修改成功，请重新登录"));
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var userInfo = new UserInfo
        {
            Id = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            Username = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Role = int.Parse(User.FindFirst("role")?.Value ?? "0")
        };

        return Ok(ApiResponse<UserInfo>.Ok(userInfo));
    }
}

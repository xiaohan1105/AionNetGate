using System.ComponentModel.DataAnnotations;

namespace AionNetGate.WebApi.Models.Requests;

/// <summary>
/// 登录请求
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "用户名长度必须在 3-20 之间")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须至少 6 位")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 注册请求
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "用户名长度必须在 3-20 之间")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须至少 6 位")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }
}

/// <summary>
/// 刷新 Token 请求
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    [Required(ErrorMessage = "Refresh Token 不能为空")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 修改密码请求
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Required(ErrorMessage = "旧密码不能为空")]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "新密码长度必须至少 6 位")]
    public string NewPassword { get; set; } = string.Empty;
}

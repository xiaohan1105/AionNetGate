using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AionGate.Core.Interfaces
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 用户名密码认证
        /// </summary>
        Task<AuthResult> AuthenticateAsync(string username, string password, AuthContext context);

        /// <summary>
        /// 验证访问令牌
        /// </summary>
        Task<AuthResult> ValidateTokenAsync(string accessToken);

        /// <summary>
        /// 刷新令牌
        /// </summary>
        Task<AuthResult> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 撤销令牌 (登出)
        /// </summary>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// 撤销账号所有令牌 (强制下线)
        /// </summary>
        Task<int> RevokeAllTokensAsync(long accountId);

        /// <summary>
        /// 注册新账号
        /// </summary>
        Task<RegisterResult> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 修改密码
        /// </summary>
        Task<bool> ChangePasswordAsync(long accountId, string oldPassword, string newPassword);

        /// <summary>
        /// 重置密码 (忘记密码)
        /// </summary>
        Task<bool> ResetPasswordAsync(string email, string newPassword, string resetToken);

        /// <summary>
        /// 发送密码重置邮件
        /// </summary>
        Task<bool> SendPasswordResetEmailAsync(string email);
    }

    /// <summary>
    /// 认证上下文
    /// </summary>
    public class AuthContext
    {
        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIp { get; set; } = "";

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 硬件指纹
        /// </summary>
        public string? HardwareFingerprint { get; set; }

        /// <summary>
        /// MAC地址
        /// </summary>
        public string? MacAddress { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public IDictionary<string, string>? Extra { get; set; }
    }

    /// <summary>
    /// 认证结果
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public AuthErrorCode ErrorCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 账号ID
        /// </summary>
        public long? AccountId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 访问令牌
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// 令牌过期时间
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static AuthResult Succeeded(long accountId, string username, string accessToken, string refreshToken, DateTime expiresAt, UserRole role = UserRole.Player)
        {
            return new AuthResult
            {
                Success = true,
                AccountId = accountId,
                Username = username,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                Role = role
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static AuthResult Failed(AuthErrorCode code, string? message = null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorCode = code,
                ErrorMessage = message ?? GetDefaultMessage(code)
            };
        }

        private static string GetDefaultMessage(AuthErrorCode code) => code switch
        {
            AuthErrorCode.InvalidCredentials => "用户名或密码错误",
            AuthErrorCode.AccountLocked => "账号已被锁定，请稍后再试",
            AuthErrorCode.AccountDisabled => "账号已被禁用",
            AuthErrorCode.TooManyAttempts => "登录尝试次数过多",
            AuthErrorCode.TokenExpired => "令牌已过期",
            AuthErrorCode.TokenInvalid => "无效的令牌",
            AuthErrorCode.IpBlocked => "您的IP已被封禁",
            AuthErrorCode.HardwareBanned => "您的设备已被封禁",
            _ => "认证失败"
        };
    }

    /// <summary>
    /// 认证错误码
    /// </summary>
    public enum AuthErrorCode
    {
        None = 0,
        InvalidCredentials = 1,
        AccountLocked = 2,
        AccountDisabled = 3,
        TooManyAttempts = 4,
        TokenExpired = 5,
        TokenInvalid = 6,
        IpBlocked = 7,
        HardwareBanned = 8,
        ServerError = 99
    }

    /// <summary>
    /// 用户角色
    /// </summary>
    public enum UserRole
    {
        Player = 0,
        GM = 1,
        Admin = 2
    }

    /// <summary>
    /// 注册请求
    /// </summary>
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Email { get; set; }
        public string ClientIp { get; set; } = "";
        public string? HardwareFingerprint { get; set; }
    }

    /// <summary>
    /// 注册结果
    /// </summary>
    public class RegisterResult
    {
        public bool Success { get; set; }
        public RegisterErrorCode ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public long? AccountId { get; set; }

        public static RegisterResult Succeeded(long accountId)
        {
            return new RegisterResult { Success = true, AccountId = accountId };
        }

        public static RegisterResult Failed(RegisterErrorCode code, string? message = null)
        {
            return new RegisterResult
            {
                Success = false,
                ErrorCode = code,
                ErrorMessage = message ?? GetDefaultMessage(code)
            };
        }

        private static string GetDefaultMessage(RegisterErrorCode code) => code switch
        {
            RegisterErrorCode.UsernameExists => "用户名已存在",
            RegisterErrorCode.EmailExists => "邮箱已被注册",
            RegisterErrorCode.InvalidUsername => "用户名格式不正确",
            RegisterErrorCode.InvalidPassword => "密码不符合要求",
            RegisterErrorCode.InvalidEmail => "邮箱格式不正确",
            RegisterErrorCode.RegistrationDisabled => "注册功能已关闭",
            RegisterErrorCode.IpBlocked => "您的IP已被封禁",
            _ => "注册失败"
        };
    }

    /// <summary>
    /// 注册错误码
    /// </summary>
    public enum RegisterErrorCode
    {
        None = 0,
        UsernameExists = 1,
        EmailExists = 2,
        InvalidUsername = 3,
        InvalidPassword = 4,
        InvalidEmail = 5,
        RegistrationDisabled = 6,
        IpBlocked = 7,
        ServerError = 99
    }
}

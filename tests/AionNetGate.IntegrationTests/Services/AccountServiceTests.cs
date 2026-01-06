using AionNetGate.Application.Services;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.IntegrationTests.Services;

/// <summary>
/// AccountService 集成测试
/// </summary>
public class AccountServiceTests : TestBase
{
    private readonly IAccountService _accountService;

    public AccountServiceTests()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<AccountService>>();
        var passwordHasher = ServiceProvider.GetRequiredService<IPasswordHasher>();
        var securityConfig = ServiceProvider.GetRequiredService<IOptions<SecurityConfig>>();

        _accountService = new AccountService(UnitOfWork, passwordHasher, securityConfig, logger);
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var email = "test@example.com";

        // Act
        var result = await _accountService.RegisterAsync(username, password, email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(username);
        result.Value.Email.Should().Be(email);
        result.Value.Status.Should().Be(1);
        result.Value.Role.Should().Be(0);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        await _accountService.RegisterAsync(username, "password123", "test1@example.com");

        // Act
        var result = await _accountService.RegisterAsync(username, "password456", "test2@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("用户名已存在");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldFail()
    {
        // Arrange
        var email = "test@example.com";
        await _accountService.RegisterAsync("user1", "password123", email);

        // Act
        var result = await _accountService.RegisterAsync("user2", "password456", email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("邮箱已存在");
    }

    [Fact]
    public async Task RegisterAsync_InvalidUsername_ShouldFail()
    {
        // Act
        var result = await _accountService.RegisterAsync("ab", "password123", "test@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("用户名长度");
    }

    [Fact]
    public async Task RegisterAsync_InvalidPassword_ShouldFail()
    {
        // Act
        var result = await _accountService.RegisterAsync("testuser", "12345", "test@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("密码长度");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        await _accountService.RegisterAsync(username, password, "test@example.com");

        // Act
        var result = await _accountService.LoginAsync(username, password, "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Account.Should().NotBeNull();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        await _accountService.RegisterAsync(username, "password123", "test@example.com");

        // Act
        var result = await _accountService.LoginAsync(username, "wrongpassword", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("密码错误");
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ShouldFail()
    {
        // Act
        var result = await _accountService.LoginAsync("nonexistent", "password", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("账号不存在");
    }

    [Fact]
    public async Task LoginAsync_MaxFailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        await _accountService.RegisterAsync(username, password, "test@example.com");

        var securityConfig = ServiceProvider.GetRequiredService<IOptions<SecurityConfig>>().Value;

        // Act - 达到最大失败次数
        for (int i = 0; i < securityConfig.MaxLoginAttempts; i++)
        {
            await _accountService.LoginAsync(username, "wrongpassword", "127.0.0.1");
        }

        var lockedResult = await _accountService.LoginAsync(username, password, "127.0.0.1");

        // Assert
        lockedResult.IsSuccess.Should().BeFalse();
        lockedResult.Error.Message.Should().Contain("账号已锁定");
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        await _accountService.RegisterAsync(username, password, "test@example.com");
        var loginResult = await _accountService.LoginAsync(username, password, "127.0.0.1");
        var refreshToken = loginResult.Value.RefreshToken;

        // Act
        var result = await _accountService.RefreshTokenAsync(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.Token.Should().NotBe(loginResult.Value.Token);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ShouldFail()
    {
        // Act
        var result = await _accountService.RefreshTokenAsync("invalid-refresh-token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("无效的 Refresh Token");
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        await _accountService.RegisterAsync(username, password, "test@example.com");
        var loginResult = await _accountService.LoginAsync(username, password, "127.0.0.1");
        var token = loginResult.Value.Token;

        // Act
        var result = await _accountService.LogoutAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 验证 Token 已被撤销
        var validateResult = await _accountService.ValidateTokenAsync(token);
        validateResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidOldPassword_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var oldPassword = "password123";
        var newPassword = "newpassword456";
        var registerResult = await _accountService.RegisterAsync(username, oldPassword, "test@example.com");
        var accountId = registerResult.Value!.Id;

        // Act
        var result = await _accountService.ChangePasswordAsync(accountId, oldPassword, newPassword);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 验证新密码可以登录
        var loginResult = await _accountService.LoginAsync(username, newPassword, "127.0.0.1");
        loginResult.IsSuccess.Should().BeTrue();

        // 验证旧密码不能登录
        var oldLoginResult = await _accountService.LoginAsync(username, oldPassword, "127.0.0.1");
        oldLoginResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongOldPassword_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var registerResult = await _accountService.RegisterAsync(username, password, "test@example.com");
        var accountId = registerResult.Value!.Id;

        // Act
        var result = await _accountService.ChangePasswordAsync(accountId, "wrongpassword", "newpassword456");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("旧密码错误");
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidEmailAndUsername_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var oldPassword = "password123";
        var newPassword = "newpassword456";
        await _accountService.RegisterAsync(username, oldPassword, email);

        // Act
        var result = await _accountService.ResetPasswordAsync(username, email, newPassword);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 验证新密码可以登录
        var loginResult = await _accountService.LoginAsync(username, newPassword, "127.0.0.1");
        loginResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_WrongEmail_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        await _accountService.RegisterAsync(username, "password123", "test@example.com");

        // Act
        var result = await _accountService.ResetPasswordAsync(username, "wrong@example.com", "newpassword");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("账号不存在或邮箱不匹配");
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        await _accountService.RegisterAsync(username, password, "test@example.com");
        var loginResult = await _accountService.LoginAsync(username, password, "127.0.0.1");
        var token = loginResult.Value.Token;

        // Act
        var result = await _accountService.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be(username);
    }

    [Fact]
    public async Task ValidateTokenAsync_RevokedToken_ShouldFail()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        await _accountService.RegisterAsync(username, password, "test@example.com");
        var loginResult = await _accountService.LoginAsync(username, password, "127.0.0.1");
        var token = loginResult.Value.Token;

        // 撤销 Token
        await _accountService.LogoutAsync(token);

        // Act
        var result = await _accountService.ValidateTokenAsync(token);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Token 已撤销");
    }
}

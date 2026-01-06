using AionNetGate.Infrastructure.Security;

namespace AionNetGate.UnitTests.Security;

public class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _hasher;

    public Argon2PasswordHasherTests()
    {
        _hasher = new Argon2PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnValidHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (passwordHash, passwordSalt) = _hasher.HashPassword(password);

        // Assert
        Assert.NotNull(passwordHash);
        Assert.NotNull(passwordSalt);
        Assert.NotEmpty(passwordHash);
        Assert.NotEmpty(passwordSalt);

        // 验证 Base64 格式
        Assert.NotNull(Convert.FromBase64String(passwordHash));
        Assert.NotNull(Convert.FromBase64String(passwordSalt));
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = _hasher.HashPassword(password);
        var (hash2, salt2) = _hasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var (passwordHash, passwordSalt) = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, passwordHash, passwordSalt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var (passwordHash, passwordSalt) = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(wrongPassword, passwordHash, passwordSalt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => _hasher.HashPassword(string.Empty));
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var (passwordHash, passwordSalt) = _hasher.HashPassword("ValidPassword");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _hasher.VerifyPassword(string.Empty, passwordHash, passwordSalt));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("verylongpasswordwithmanychars123456789")]
    [InlineData("P@ssw0rd!")]
    [InlineData("密码123")]
    public void HashPassword_WithVariousPasswords_ShouldWork(string password)
    {
        // Act
        var (passwordHash, passwordSalt) = _hasher.HashPassword(password);
        var verified = _hasher.VerifyPassword(password, passwordHash, passwordSalt);

        // Assert
        Assert.True(verified);
    }
}

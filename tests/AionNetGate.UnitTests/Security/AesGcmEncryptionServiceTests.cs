using System.Text;
using AionNetGate.Infrastructure.Security;

namespace AionNetGate.UnitTests.Security;

public class AesGcmEncryptionServiceTests
{
    [Fact]
    public void GenerateKey_ShouldReturn32Bytes()
    {
        // Act
        var key = AesGcmEncryptionService.GenerateKey();

        // Assert
        Assert.NotNull(key);
        Assert.Equal(32, key.Length);
    }

    [Fact]
    public void Constructor_WithValidKey_ShouldNotThrow()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();

        // Act & Assert
        var service = new AesGcmEncryptionService(key);
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithInvalidKey_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidKey = new byte[16]; // 错误的密钥长度

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new AesGcmEncryptionService(invalidKey));
    }

    [Fact]
    public void Encrypt_ShouldReturnCiphertext()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var ciphertext = service.Encrypt(plaintext);

        // Assert
        Assert.NotNull(ciphertext);
        Assert.True(ciphertext.Length > plaintext.Length); // 包含 nonce 和 tag
        Assert.NotEqual(plaintext, ciphertext);
    }

    [Fact]
    public void Decrypt_WithValidCiphertext_ShouldReturnPlaintext()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var originalPlaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var ciphertext = service.Encrypt(originalPlaintext);

        // Act
        var decryptedPlaintext = service.Decrypt(ciphertext);

        // Assert
        Assert.Equal(originalPlaintext, decryptedPlaintext);
    }

    [Fact]
    public void Encrypt_ShouldProduceDifferentCiphertextForSamePlaintext()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var ciphertext1 = service.Encrypt(plaintext);
        var ciphertext2 = service.Encrypt(plaintext);

        // Assert
        Assert.NotEqual(ciphertext1, ciphertext2); // 由于 nonce 不同
    }

    [Fact]
    public void Decrypt_WithWrongKey_ShouldThrowCryptographicException()
    {
        // Arrange
        var key1 = AesGcmEncryptionService.GenerateKey();
        var key2 = AesGcmEncryptionService.GenerateKey();
        var service1 = new AesGcmEncryptionService(key1);
        var service2 = new AesGcmEncryptionService(key2);
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var ciphertext = service1.Encrypt(plaintext);

        // Act & Assert
        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() =>
            service2.Decrypt(ciphertext));
    }

    [Fact]
    public void Decrypt_WithTamperedCiphertext_ShouldThrowCryptographicException()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var ciphertext = service.Encrypt(plaintext);

        // 篡改密文
        ciphertext[^1] ^= 0xFF;

        // Act & Assert
        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() =>
            service.Decrypt(ciphertext));
    }

    [Fact]
    public async Task EncryptAsync_ShouldReturnCiphertext()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var ciphertext = await service.EncryptAsync(plaintext);

        // Assert
        Assert.NotNull(ciphertext);
        Assert.True(ciphertext.Length > plaintext.Length);
    }

    [Fact]
    public async Task DecryptAsync_WithValidCiphertext_ShouldReturnPlaintext()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var originalPlaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var ciphertext = await service.EncryptAsync(originalPlaintext);

        // Act
        var decryptedPlaintext = await service.DecryptAsync(ciphertext);

        // Assert
        Assert.Equal(originalPlaintext, decryptedPlaintext);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("Hello, World!")]
    [InlineData("这是一段中文测试文本")]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")]
    public void EncryptDecrypt_WithVariousInputs_ShouldWork(string input)
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var service = new AesGcmEncryptionService(key);
        var plaintext = Encoding.UTF8.GetBytes(input);

        // Act
        var ciphertext = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        Assert.Equal(plaintext, decrypted);
        Assert.Equal(input, Encoding.UTF8.GetString(decrypted));
    }

    [Fact]
    public void FromBase64Key_ShouldCreateValidService()
    {
        // Arrange
        var key = AesGcmEncryptionService.GenerateKey();
        var base64Key = Convert.ToBase64String(key);

        // Act
        var service = AesGcmEncryptionService.FromBase64Key(base64Key);
        var plaintext = Encoding.UTF8.GetBytes("Test");
        var ciphertext = service.Encrypt(plaintext);
        var decrypted = service.Decrypt(ciphertext);

        // Assert
        Assert.Equal(plaintext, decrypted);
    }
}

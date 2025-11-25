using Xunit;
using FluentAssertions;
using AionGate.Updater;

namespace AionGate.Updater.Tests;

public class CDNUrlSignerTests
{
    [Fact]
    public void GenerateSignedUrl_AliOSS_ShouldReturnValidUrl()
    {
        // Arrange
        var config = new CDNConfig
        {
            Provider = CDNProvider.AliOSS,
            BucketName = "test-bucket",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Endpoint = "oss-cn-hangzhou.aliyuncs.com",
            Domain = "cdn.test.com"
        };

        var signer = new CDNUrlSigner(config);

        // Act
        var url = signer.GenerateSignedUrl("Data/test.pak", 60);

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("cdn.test.com");
        url.Should().Contain("Data/test.pak");
        url.Should().Contain("OSSAccessKeyId=");
        url.Should().Contain("Expires=");
        url.Should().Contain("Signature=");
    }

    [Fact]
    public void GenerateSignedUrl_TencentCOS_ShouldReturnValidUrl()
    {
        // Arrange
        var config = new CDNConfig
        {
            Provider = CDNProvider.TencentCOS,
            BucketName = "test-bucket",
            AccessKey = "test-secret-id",
            SecretKey = "test-secret-key",
            Region = "ap-beijing",
            Endpoint = "cos.ap-beijing.myqcloud.com",
            Domain = "cdn.test.com"
        };

        var signer = new CDNUrlSigner(config);

        // Act
        var url = signer.GenerateSignedUrl("Data/test.pak", 60);

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("cdn.test.com");
        url.Should().Contain("Data/test.pak");
        url.Should().Contain("q-sign-algorithm=");
        url.Should().Contain("q-ak=");
        url.Should().Contain("q-sign-time=");
    }

    [Fact]
    public void GenerateSignedUrl_AWSS3_ShouldReturnValidUrl()
    {
        // Arrange
        var config = new CDNConfig
        {
            Provider = CDNProvider.AWSS3,
            BucketName = "test-bucket",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Region = "us-east-1",
            Endpoint = "s3.amazonaws.com",
            Domain = "cdn.test.com"
        };

        var signer = new CDNUrlSigner(config);

        // Act
        var url = signer.GenerateSignedUrl("Data/test.pak", 60);

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("cdn.test.com");
        url.Should().Contain("Data/test.pak");
        url.Should().Contain("X-Amz-Algorithm=");
        url.Should().Contain("X-Amz-Credential=");
        url.Should().Contain("X-Amz-Date=");
        url.Should().Contain("X-Amz-Signature=");
    }

    [Fact]
    public void GenerateSignedUrl_CloudflareR2_ShouldReturnValidUrl()
    {
        // Arrange
        var config = new CDNConfig
        {
            Provider = CDNProvider.CloudflareR2,
            BucketName = "test-bucket",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Region = "auto",
            Endpoint = "r2.cloudflarestorage.com",
            Domain = "cdn.test.com"
        };

        var signer = new CDNUrlSigner(config);

        // Act
        var url = signer.GenerateSignedUrl("Data/test.pak", 60);

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("cdn.test.com");
        url.Should().Contain("Data/test.pak");
        url.Should().Contain("X-Amz-Algorithm=");
    }

    [Fact]
    public void GenerateSignedUrl_WithCustomExpiration_ShouldUseCorrectExpiration()
    {
        // Arrange
        var config = new CDNConfig
        {
            Provider = CDNProvider.AliOSS,
            BucketName = "test-bucket",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            Endpoint = "oss-cn-hangzhou.aliyuncs.com"
        };

        var signer = new CDNUrlSigner(config);

        // Act
        var url = signer.GenerateSignedUrl("Data/test.pak", 120); // 120分钟

        // Assert
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("Expires=");

        // 解析Expires参数
        var expiresMatch = System.Text.RegularExpressions.Regex.Match(url, @"Expires=(\d+)");
        if (expiresMatch.Success)
        {
            var expires = long.Parse(expiresMatch.Groups[1].Value);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 允许1分钟误差
            (expires - now).Should().BeInRange(120 * 60 - 60, 120 * 60 + 60);
        }
    }

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new CDNUrlSigner(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateSignedUrl_WithUnsupportedProvider_ShouldThrowNotSupportedException()
    {
        // Arrange
        var config = new CDNConfig
        {
            Provider = (CDNProvider)999, // 不存在的Provider
            BucketName = "test-bucket",
            AccessKey = "test-key",
            SecretKey = "test-secret"
        };

        var signer = new CDNUrlSigner(config);

        // Act & Assert
        var act = () => signer.GenerateSignedUrl("test.pak");
        act.Should().Throw<NotSupportedException>();
    }
}

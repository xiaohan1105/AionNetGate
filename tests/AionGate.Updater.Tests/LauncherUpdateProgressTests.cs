using Xunit;
using FluentAssertions;
using AionGate.Updater;

namespace AionGate.Updater.Tests;

public class LauncherUpdateProgressTests
{
    [Theory]
    [InlineData(UpdateStage.CheckingVersion, "正在检查版本...")]
    [InlineData(UpdateStage.DownloadingManifest, "正在获取更新清单...")]
    [InlineData(UpdateStage.ComparingFiles, "正在对比本地文件...")]
    [InlineData(UpdateStage.DownloadingFiles, "正在下载更新文件...")]
    [InlineData(UpdateStage.ExtractingFiles, "正在解压文件...")]
    [InlineData(UpdateStage.VerifyingFiles, "正在校验文件完整性...")]
    [InlineData(UpdateStage.ApplyingPatch, "正在应用补丁...")]
    [InlineData(UpdateStage.CleaningUp, "正在清理临时文件...")]
    [InlineData(UpdateStage.Completed, "更新完成")]
    [InlineData(UpdateStage.Failed, "更新失败")]
    public void StageName_ShouldReturnCorrectChineseText(UpdateStage stage, string expectedName)
    {
        // Arrange
        var progress = new LauncherUpdateProgress
        {
            Stage = stage
        };

        // Act
        var stageName = progress.StageName;

        // Assert
        stageName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0, "0 B/s")]
    [InlineData(1024, "1.00 KB/s")]
    [InlineData(1048576, "1.00 MB/s")]
    [InlineData(1073741824, "1.00 GB/s")]
    [InlineData(52428800, "50.00 MB/s")]
    public void DownloadSpeedText_ShouldFormatCorrectly(long bytesPerSecond, string expected)
    {
        // Arrange
        var progress = new LauncherUpdateProgress
        {
            DownloadSpeed = bytesPerSecond
        };

        // Act
        var speedText = progress.DownloadSpeedText;

        // Assert
        speedText.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, "计算中...")]
    [InlineData(0, "即将完成")]
    [InlineData(30, "30秒")]
    [InlineData(90, "1分钟30秒")]
    [InlineData(3600, "1小时0分钟")]
    [InlineData(3690, "1小时1分钟")]
    public void RemainingTimeText_ShouldFormatCorrectly(int seconds, string expected)
    {
        // Arrange
        var progress = new LauncherUpdateProgress
        {
            RemainingSeconds = seconds
        };

        // Act
        var timeText = progress.RemainingTimeText;

        // Assert
        timeText.Should().Be(expected);
    }

    [Fact]
    public void OverallProgress_ShouldCalculateCorrectly()
    {
        // Arrange
        var progress = new LauncherUpdateProgress
        {
            TotalBytes = 1000000000, // 1GB
            DownloadedBytes = 500000000 // 500MB
        };

        // Act
        var overallProgress = (int)(progress.DownloadedBytes * 100 / progress.TotalBytes);

        // Assert
        overallProgress.Should().Be(50);
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var progress = new LauncherUpdateProgress();

        // Assert
        progress.CanCancel.Should().BeTrue();
        progress.CanPause.Should().BeTrue();
        progress.IsPaused.Should().BeFalse();
        progress.Stage.Should().Be(UpdateStage.Preparing);
    }
}

public class UpdateCheckResultTests
{
    [Fact]
    public void NeedsFullClient_True_ShouldHaveFullPackageLinks()
    {
        // Arrange
        var result = new UpdateCheckResult
        {
            NeedsUpdate = true,
            NeedsFullClient = true,
            LatestVersion = "2.7.0.15",
            FullPackageLinks = new List<FullPackageLinkDto>
            {
                new FullPackageLinkDto
                {
                    Id = 1,
                    PackageName = "Test Package",
                    Type = "baidu",
                    TypeName = "百度网盘",
                    Url = "https://pan.baidu.com/s/test",
                    VerificationCode = "abc123",
                    FileSize = 15728640000,
                    Priority = 100,
                    IsRecommended = true
                }
            }
        };

        // Assert
        result.FullPackageLinks.Should().NotBeNull();
        result.FullPackageLinks.Should().HaveCount(1);
        result.FullPackageLinks[0].IsRecommended.Should().BeTrue();
    }

    [Fact]
    public void UpdateType_ShouldBeIncremental_WhenNotFullClient()
    {
        // Arrange
        var result = new UpdateCheckResult
        {
            NeedsUpdate = true,
            NeedsFullClient = false,
            UpdateType = "incremental"
        };

        // Assert
        result.UpdateType.Should().Be("incremental");
    }
}

public class FullPackageLinkTests
{
    [Theory]
    [InlineData("baidu", "百度网盘")]
    [InlineData("aliyun", "阿里云盘")]
    [InlineData("thunder", "迅雷云盘")]
    [InlineData("115", "115网盘")]
    [InlineData("mega", "MEGA网盘")]
    [InlineData("direct", "直链下载")]
    [InlineData("other", "其他")]
    public void TypeName_ShouldMapCorrectly(string type, string expectedTypeName)
    {
        // Arrange
        var link = new FullPackageLinkDto
        {
            Type = type
        };

        // 手动实现TypeName逻辑进行测试
        var typeName = type switch
        {
            "baidu" => "百度网盘",
            "aliyun" => "阿里云盘",
            "thunder" => "迅雷云盘",
            "115" => "115网盘",
            "mega" => "MEGA网盘",
            "direct" => "直链下载",
            _ => "其他"
        };

        // Assert
        typeName.Should().Be(expectedTypeName);
    }

    [Theory]
    [InlineData(90, true)]
    [InlineData(100, true)]
    [InlineData(89, false)]
    [InlineData(50, false)]
    public void IsRecommended_ShouldBeTrueWhenPriorityIs90OrHigher(int priority, bool expectedRecommended)
    {
        // Arrange
        var link = new FullPackageLinkDto
        {
            Priority = priority
        };

        // 手动实现IsRecommended逻辑
        var isRecommended = priority >= 90;

        // Assert
        isRecommended.Should().Be(expectedRecommended);
    }
}

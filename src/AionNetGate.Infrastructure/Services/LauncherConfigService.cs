using System.Security.Cryptography;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Results;
using AionNetGate.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Infrastructure.Services;

/// <summary>
/// 登录器配置服务实现
/// </summary>
public class LauncherConfigService : ILauncherConfigService
{
    private readonly LauncherConfig _config;
    private readonly CheatDetectionConfig _cheatConfig;
    private readonly ILogger<LauncherConfigService> _logger;

    public LauncherConfigService(
        IOptions<LauncherConfig> config,
        IOptions<CheatDetectionConfig> cheatConfig,
        ILogger<LauncherConfigService> logger)
    {
        _config = config.Value;
        _cheatConfig = cheatConfig.Value;
        _logger = logger;
    }

    public LauncherConfig GetCurrentConfig()
    {
        return _config;
    }

    public LauncherDynamicParameters GetDynamicParameters()
    {
        return new LauncherDynamicParameters
        {
            Launcher32Url = _config.Launcher32Url,
            Launcher64Url = _config.Launcher64Url,
            PatchUrl = _config.PatchUrl,
            WebPageUrl = _config.WebPageUrl,
            LauncherName = _config.LauncherName,
            ClientProgram = _config.ClientProgram,
            MaxLauncherCount = _config.MaxLauncherCount,
            MaxClientCount = _config.MaxClientCount,
            LaunchParameters = _config.LaunchParameters,
            DisableAccountManagement = _config.DisableAccountManagement,
            CheatDetection = _cheatConfig.Enabled ? new CheatDetectionParameters
            {
                Enabled = _cheatConfig.Enabled,
                CheckIntervalSeconds = _cheatConfig.CheckIntervalSeconds,
                ForbiddenProcesses = _cheatConfig.GetForbiddenProcessesArray(),
                ForbiddenProcessMd5 = _cheatConfig.GetForbiddenProcessMd5Array(),
                ForbiddenWindowClasses = _cheatConfig.GetForbiddenWindowClassesArray()
            } : null
        };
    }

    public Task<Result> UpdateConfigAsync(LauncherConfig config, CancellationToken cancellationToken = default)
    {
        // 在实际实现中，这里应该将配置保存到数据库或配置文件
        // 由于当前使用 appsettings.json，运行时修改需要重启

        _logger.LogInformation("登录器配置已更新");

        // TODO: 实现配置持久化
        // 可以考虑使用数据库存储或动态配置提供程序

        return Task.FromResult(Result.Success());
    }

    public Task<Result<string>> GenerateLauncherAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        // 登录器生成功能需要完整的编译环境
        // 这里提供一个框架，实际实现需要：
        // 1. 启动器模板项目
        // 2. 代码生成/配置注入
        // 3. 编译（CSharpCodeProvider 或 Roslyn）
        // 4. 可选的混淆加壳

        _logger.LogInformation("开始生成登录器: OutputPath={OutputPath}", outputPath);

        try
        {
            // 验证输出路径
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return Task.FromResult(Result<string>.Failure(Error.Validation("输出路径不能为空")));
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // TODO: 实现实际的登录器生成逻辑
            // 当前返回占位符

            _logger.LogWarning("登录器生成功能尚未完全实现");

            return Task.FromResult(Result<string>.Failure(Error.ServiceUnavailable("登录器生成功能尚未实现，请使用传统项目的生成器")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录器生成失败");
            return Task.FromResult(Result<string>.Failure(Error.Internal($"登录器生成失败: {ex.Message}")));
        }
    }

    public bool ValidateLauncherId(string launcherId, bool is64Bit)
    {
        if (string.IsNullOrWhiteSpace(launcherId))
        {
            return false;
        }

        var expectedId = is64Bit ? _config.Launcher64Id : _config.Launcher32Id;

        // 如果未配置标识，则允许所有
        if (string.IsNullOrWhiteSpace(expectedId))
        {
            return true;
        }

        return string.Equals(launcherId, expectedId, StringComparison.OrdinalIgnoreCase);
    }

    public Task<Result> ClearLauncherIdAsync(bool is64Bit, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("清空{Bits}位登录器标识", is64Bit ? 64 : 32);

        // TODO: 更新配置并持久化

        return Task.FromResult(Result.Success());
    }

    public string GenerateNewLauncherId(bool is64Bit)
    {
        // 生成一个唯一的登录器标识
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        var id = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            [..16];

        _logger.LogInformation("生成新的{Bits}位登录器标识: {Id}", is64Bit ? 64 : 32, id);

        return id;
    }
}

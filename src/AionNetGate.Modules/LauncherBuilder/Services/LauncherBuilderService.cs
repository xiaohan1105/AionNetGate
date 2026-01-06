using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AionNetGate.Core.Results;
using AionNetGate.Modules.LauncherBuilder.Models;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Modules.LauncherBuilder.Services;

/// <summary>
/// 启动器构建服务实现
/// </summary>
/// <param name="logger">日志记录器</param>
public class LauncherBuilderService(ILogger<LauncherBuilderService> logger) : ILauncherBuilderService
{
    private static readonly string[] SkipDirectories = ["bin", "obj", ".vs", ".git"];
    private static readonly string[] SupportedRuntimes = ["win-x64", "win-x86", "win-arm64"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _templatePath = FindTemplatePath();

    /// <inheritdoc/>
    public IReadOnlyList<string> GetAvailableRuntimes() => SupportedRuntimes;

    /// <inheritdoc/>
    public Result ValidateBuildConfig(LauncherBuildConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        List<string> errors = [];

        // 必填字段验证
        if (string.IsNullOrWhiteSpace(config.ChannelCode))
            errors.Add("渠道代码不能为空");

        if (string.IsNullOrWhiteSpace(config.GameTitle))
            errors.Add("游戏标题不能为空");

        if (string.IsNullOrWhiteSpace(config.OutputDirectory))
            errors.Add("输出目录不能为空");

        if (string.IsNullOrWhiteSpace(config.OutputFileName))
            errors.Add("输出文件名不能为空");

        // 渠道代码格式验证（只允许字母、数字、下划线、连字符）
        if (!string.IsNullOrWhiteSpace(config.ChannelCode) &&
            !System.Text.RegularExpressions.Regex.IsMatch(config.ChannelCode, @"^[a-zA-Z0-9_\-]+$"))
        {
            errors.Add("渠道代码只能包含字母、数字、下划线和连字符");
        }

        // 端口范围验证
        if (config.GatewayPort is < 1 or > 65535)
            errors.Add("网关端口必须在 1-65535 范围内");

        if (config.LsPort is < 1 or > 65535)
            errors.Add("LS端口必须在 1-65535 范围内");

        // 版本号格式验证
        if (!string.IsNullOrWhiteSpace(config.Version) &&
            !System.Text.RegularExpressions.Regex.IsMatch(config.Version, @"^\d+\.\d+\.\d+(-\w+)?$"))
        {
            errors.Add("版本号格式无效，应为 X.Y.Z 或 X.Y.Z-suffix");
        }

        // URL 格式验证
        if (!string.IsNullOrWhiteSpace(config.UpdateCheckUrl) &&
            !Uri.TryCreate(config.UpdateCheckUrl, UriKind.Absolute, out _))
        {
            errors.Add("更新检查URL格式无效");
        }

        if (!string.IsNullOrWhiteSpace(config.UpdateDownloadUrl) &&
            !Uri.TryCreate(config.UpdateDownloadUrl, UriKind.Absolute, out _))
        {
            errors.Add("更新下载URL格式无效");
        }

        if (!string.IsNullOrWhiteSpace(config.WebUrl) &&
            !Uri.TryCreate(config.WebUrl, UriKind.Absolute, out _))
        {
            errors.Add("内嵌浏览器URL格式无效");
        }

        // 皮肤路径验证
        if (!string.IsNullOrWhiteSpace(config.SkinPath) && !Directory.Exists(config.SkinPath))
            errors.Add($"皮肤资源目录不存在: {config.SkinPath}");

        // 模板路径验证
        if (!Directory.Exists(_templatePath))
            errors.Add($"启动器模板目录不存在: {_templatePath}");

        // 输出文件名安全检查（防止路径遍历）
        if (!string.IsNullOrWhiteSpace(config.OutputFileName))
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            if (config.OutputFileName.IndexOfAny(invalidChars) >= 0)
                errors.Add("输出文件名包含非法字符");
        }

        // 输出目录路径遍历检查
        if (!string.IsNullOrWhiteSpace(config.OutputDirectory))
        {
            try
            {
                var fullPath = Path.GetFullPath(config.OutputDirectory);
                // 确保不会写入系统关键目录
                var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
                if (fullPath.Equals(systemDrive, StringComparison.OrdinalIgnoreCase) ||
                    fullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("不能将输出目录设置为系统目录");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"输出目录路径无效: {ex.Message}");
            }
        }

        if (errors.Count > 0)
            return Result.Failure(Error.Validation(string.Join("; ", errors)));

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<string>> BuildLauncherAsync(
        LauncherBuildConfig config,
        IProgress<BuildProgress>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        var runtime = config.TargetRuntime ?? "win-x64";
        if (!SupportedRuntimes.Contains(runtime))
        {
            return Result<string>.Failure(Error.Validation($"不支持的运行时: {runtime}，支持的运行时: {string.Join(", ", SupportedRuntimes)}"));
        }

        try
        {
            logger.LogInformation("开始构建启动器: 渠道={ChannelCode}, 运行时={Runtime}",
                config.ChannelCode, runtime);

            // 1. 验证配置
            ReportProgress(progress, "验证配置", 0, "正在验证构建配置...");
            var validationResult = ValidateBuildConfig(config);
            if (!validationResult.IsSuccess)
                return Result<string>.Failure(validationResult.Error);

            // 2. 创建临时构建目录
            ReportProgress(progress, "准备环境", 10, "正在创建临时构建目录...");
            var tempBuildDir = Path.Combine(
                Path.GetTempPath(),
                $"AionLauncher_{config.ChannelCode}_{Guid.NewGuid():N}");

            Directory.CreateDirectory(tempBuildDir);
            logger.LogDebug("临时构建目录: {TempDir}", tempBuildDir);

            try
            {
                // 3. 复制模板项目
                ReportProgress(progress, "复制模板", 20, "正在复制启动器模板...");
                await CopyDirectoryAsync(_templatePath, tempBuildDir, ct);

                // 4. 生成配置文件（强类型序列化）
                ReportProgress(progress, "生成配置", 40, "正在生成启动器配置文件...");
                await GenerateConfigFileAsync(tempBuildDir, config, ct);

                // 5. 复制皮肤资源
                if (!string.IsNullOrEmpty(config.SkinPath) && Directory.Exists(config.SkinPath))
                {
                    ReportProgress(progress, "复制皮肤", 50, "正在复制皮肤资源...");
                    var skinTargetPath = Path.Combine(tempBuildDir, "Resources", "Skins", "Default");
                    Directory.CreateDirectory(skinTargetPath);
                    await CopyDirectoryAsync(config.SkinPath, skinTargetPath, ct);
                }

                // 6. 编译发布
                ReportProgress(progress, "编译发布", 60, $"正在编译启动器 ({runtime})...");
                var publishResult = await PublishLauncherAsync(tempBuildDir, runtime, ct);
                if (!publishResult.IsSuccess)
                    return Result<string>.Failure(publishResult.Error);

                // 7. 查找并复制输出文件
                ReportProgress(progress, "复制输出", 90, "正在复制输出文件...");
                var outputResult = await CopyOutputFileAsync(tempBuildDir, config, runtime, ct);
                if (!outputResult.IsSuccess)
                    return Result<string>.Failure(outputResult.Error);

                ReportProgress(progress, "完成", 100, "启动器构建完成");
                logger.LogInformation("启动器构建成功: {OutputPath}", outputResult.Value);

                return outputResult;
            }
            finally
            {
                // 清理临时目录
                await CleanupTempDirectoryAsync(tempBuildDir);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("构建启动器已取消");
            return Result<string>.Failure(Error.Cancelled("构建已取消"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "构建启动器失败");
            return Result<string>.Failure(Error.Internal($"构建失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 生成配置文件（使用强类型序列化）
    /// </summary>
    private async Task GenerateConfigFileAsync(string buildDir, LauncherBuildConfig config, CancellationToken ct)
    {
        // 构建强类型配置对象
        var launcherConfig = new LauncherConfigDto
        {
            GameTitle = config.GameTitle,
            ServerName = config.ServerName,
            Version = config.Version,
            Gateway = new GatewayConfigDto
            {
                Host = config.GatewayHost,
                Port = config.GatewayPort
            },
            Game = new GameConfigDto
            {
                ExecutablePath = config.GameExecutablePath,
                WorkingDirectory = config.GameWorkingDirectory,
                CommandLineArgs = config.GameCommandLineArgs,
                LsPort = config.LsPort
            },
            Update = new UpdateConfigDto
            {
                CheckUrl = config.UpdateCheckUrl ?? "",
                DownloadUrl = config.UpdateDownloadUrl ?? ""
            },
            AntiCheat = new AntiCheatConfigDto
            {
                Enabled = config.AntiCheatEnabled,
                ProcessBlacklist = config.ProcessBlacklist ?? [],
                FileIntegrityCheck = config.FileIntegrityCheck
            },
            Skin = new SkinConfigDto
            {
                BackgroundImage = "Resources/Skins/Default/background.png",
                ButtonImages = new Dictionary<string, string>
                {
                    ["Start"] = "Resources/Skins/Default/button_start.png",
                    ["Close"] = "Resources/Skins/Default/button_close.png"
                },
                ShowWebBrowser = config.ShowWebBrowser,
                WebUrl = config.WebUrl ?? ""
            }
        };

        var configPath = Path.Combine(buildDir, "launcher.config.json");
        var jsonContent = JsonSerializer.Serialize(launcherConfig, JsonOptions);

        await File.WriteAllTextAsync(configPath, jsonContent, Encoding.UTF8, ct);
        logger.LogDebug("配置文件已生成: {ConfigPath}", configPath);
    }

    /// <summary>
    /// 编译发布启动器
    /// </summary>
    private async Task<Result> PublishLauncherAsync(string projectPath, string runtime, CancellationToken ct)
    {
        try
        {
            var arguments = new StringBuilder();
            arguments.Append("publish ");
            arguments.Append("-c Release ");
            arguments.Append($"-r {runtime} ");
            arguments.Append("--self-contained true ");
            arguments.Append("-p:PublishSingleFile=true ");
            arguments.Append("-p:IncludeNativeLibrariesForSelfExtract=true ");
            arguments.Append("-p:EnableCompressionInSingleFile=true ");
            arguments.Append("--nologo ");
            arguments.Append("-v minimal");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments.ToString(),
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            logger.LogDebug("执行命令: dotnet {Arguments}", arguments);

            using var process = new Process { StartInfo = startInfo };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    logger.LogDebug("[dotnet] {Output}", e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    // MSBuild 警告也会输出到 stderr，只有真正的错误才记录为警告
                    if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase))
                        logger.LogWarning("[dotnet error] {Error}", e.Data);
                    else
                        logger.LogDebug("[dotnet] {Error}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString();
                logger.LogError("dotnet publish 失败，退出码: {ExitCode}\n{Output}\n{Error}",
                    process.ExitCode, outputBuilder, errorMessage);
                return Result.Failure(Error.Internal($"编译失败 (退出码: {process.ExitCode}): {errorMessage}"));
            }

            logger.LogInformation("dotnet publish 成功，运行时: {Runtime}", runtime);
            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "执行 dotnet publish 时发生异常");
            return Result.Failure(Error.Internal($"编译失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 复制输出文件到目标目录
    /// </summary>
    private async Task<Result<string>> CopyOutputFileAsync(
        string buildDir, LauncherBuildConfig config, string runtime, CancellationToken ct)
    {
        // 构建可能的输出路径
        string[] possiblePaths =
        [
            Path.Combine(buildDir, "bin", "Release", "net9.0-windows", runtime, "publish", "AionLauncher.exe"),
            Path.Combine(buildDir, "bin", "Release", "net9.0-windows", runtime, "publish", "AionNetGate.Launcher.Template.exe"),
            Path.Combine(buildDir, "bin", "Release", $"net9.0-windows", "publish", "AionLauncher.exe")
        ];

        string? publishedExe = null;
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                publishedExe = path;
                break;
            }
        }

        if (publishedExe is null)
        {
            // 尝试搜索
            var searchDir = Path.Combine(buildDir, "bin", "Release");
            if (Directory.Exists(searchDir))
            {
                var exeFiles = Directory.GetFiles(searchDir, "*.exe", SearchOption.AllDirectories);
                publishedExe = exeFiles.FirstOrDefault(f =>
                    !Path.GetFileName(f).StartsWith("createdump", StringComparison.OrdinalIgnoreCase));
            }
        }

        if (publishedExe is null)
        {
            return Result<string>.Failure(Error.NotFound("找不到编译输出文件"));
        }

        logger.LogDebug("找到编译输出: {PublishedExe}", publishedExe);

        Directory.CreateDirectory(config.OutputDirectory);

        var targetExe = Path.Combine(config.OutputDirectory, $"{config.OutputFileName}.exe");

        // 如果目标文件已存在，先删除
        if (File.Exists(targetExe))
        {
            File.Delete(targetExe);
        }

        // 异步复制大文件
        await using var sourceStream = new FileStream(publishedExe, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        await using var targetStream = new FileStream(targetExe, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await sourceStream.CopyToAsync(targetStream, ct);

        return Result<string>.Success(targetExe);
    }

    /// <summary>
    /// 异步复制目录（优化版本）
    /// </summary>
    private async Task CopyDirectoryAsync(string sourceDir, string targetDir, CancellationToken ct)
    {
        Directory.CreateDirectory(targetDir);

        // 并行复制文件
        var files = Directory.GetFiles(sourceDir);
        var copyTasks = files.Select(async file =>
        {
            ct.ThrowIfCancellationRequested();
            var targetFile = Path.Combine(targetDir, Path.GetFileName(file));

            await using var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            await using var targetStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await sourceStream.CopyToAsync(targetStream, ct);
        });

        await Task.WhenAll(copyTasks);

        // 递归复制子目录
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            ct.ThrowIfCancellationRequested();

            var dirName = Path.GetFileName(dir);

            // 跳过构建目录和隐藏目录
            if (ShouldSkipDirectory(dirName))
                continue;

            var targetSubDir = Path.Combine(targetDir, dirName);
            await CopyDirectoryAsync(dir, targetSubDir, ct);
        }
    }

    /// <summary>
    /// 判断是否应该跳过目录
    /// </summary>
    private static bool ShouldSkipDirectory(string dirName) =>
        SkipDirectories.Any(skip => dirName.Equals(skip, StringComparison.OrdinalIgnoreCase)) ||
        dirName.StartsWith('.');

    /// <summary>
    /// 清理临时目录
    /// </summary>
    private async Task CleanupTempDirectoryAsync(string tempDir)
    {
        try
        {
            if (Directory.Exists(tempDir))
            {
                await Task.Run(() => Directory.Delete(tempDir, true));
                logger.LogDebug("临时目录已清理: {TempDir}", tempDir);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "清理临时目录失败: {TempDir}", tempDir);
        }
    }

    /// <summary>
    /// 报告构建进度
    /// </summary>
    private static void ReportProgress(IProgress<BuildProgress>? progress, string step, int percent, string message)
    {
        progress?.Report(new BuildProgress(step, percent, message));
    }

    /// <summary>
    /// 查找模板路径
    /// </summary>
    private static string FindTemplatePath()
    {
        // 尝试多种方式查找模板路径
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;

        // 1. 从解决方案根目录查找
        var solutionRoot = FindSolutionRoot(currentDir);
        if (solutionRoot is not null)
        {
            var templatePath = Path.Combine(solutionRoot, "src", "AionNetGate.Launcher.Template");
            if (Directory.Exists(templatePath))
                return templatePath;
        }

        // 2. 相对于当前目录
        var relativePath = Path.Combine(currentDir, "..", "..", "..", "AionNetGate.Launcher.Template");
        if (Directory.Exists(relativePath))
            return Path.GetFullPath(relativePath);

        // 3. 默认路径
        return Path.Combine(currentDir, "LauncherTemplate");
    }

    /// <summary>
    /// 查找解决方案根目录
    /// </summary>
    private static string? FindSolutionRoot(string startDir)
    {
        var currentDir = startDir;
        while (currentDir is not null)
        {
            if (Directory.GetFiles(currentDir, "*.sln").Length > 0)
                return currentDir;

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }

    #region 配置 DTO（用于 JSON 序列化）

    private sealed class LauncherConfigDto
    {
        public string GameTitle { get; init; } = "";
        public string ServerName { get; init; } = "";
        public string Version { get; init; } = "";
        public GatewayConfigDto Gateway { get; init; } = new();
        public GameConfigDto Game { get; init; } = new();
        public UpdateConfigDto Update { get; init; } = new();
        public AntiCheatConfigDto AntiCheat { get; init; } = new();
        public SkinConfigDto Skin { get; init; } = new();
    }

    private sealed class GatewayConfigDto
    {
        public string Host { get; init; } = "";
        public int Port { get; init; }
    }

    private sealed class GameConfigDto
    {
        public string ExecutablePath { get; init; } = "";
        public string WorkingDirectory { get; init; } = "";
        public string CommandLineArgs { get; init; } = "";
        public int LsPort { get; init; }
    }

    private sealed class UpdateConfigDto
    {
        public string CheckUrl { get; init; } = "";
        public string DownloadUrl { get; init; } = "";
    }

    private sealed class AntiCheatConfigDto
    {
        public bool Enabled { get; init; }
        public List<string> ProcessBlacklist { get; init; } = [];
        public bool FileIntegrityCheck { get; init; }
    }

    private sealed class SkinConfigDto
    {
        public string BackgroundImage { get; init; } = "";
        public Dictionary<string, string> ButtonImages { get; init; } = [];
        public bool ShowWebBrowser { get; init; }
        public string WebUrl { get; init; } = "";
    }

    #endregion
}

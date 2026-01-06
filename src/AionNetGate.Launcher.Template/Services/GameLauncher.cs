using System.Diagnostics;
using System.IO;
using AionNetGate.Launcher.Template.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Launcher.Template.Services;

/// <summary>
/// 游戏启动器实现
/// </summary>
public class GameLauncher : IGameLauncher
{
    private readonly ILogger<GameLauncher> _logger;
    private readonly LauncherConfig _config;

    public GameLauncher(
        ILogger<GameLauncher> logger,
        IOptions<LauncherConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task<bool> LaunchGameAsync(CancellationToken ct = default)
    {
        try
        {
            var gamePath = GetGamePath();

            if (!File.Exists(gamePath))
            {
                _logger.LogError("游戏可执行文件不存在: {Path}", gamePath);
                return false;
            }

            _logger.LogInformation("正在启动游戏: {Path}", gamePath);

            var workingDir = string.IsNullOrEmpty(_config.Game.WorkingDirectory)
                ? Path.GetDirectoryName(gamePath)
                : _config.Game.WorkingDirectory;

            var startInfo = new ProcessStartInfo
            {
                FileName = gamePath,
                Arguments = _config.Game.CommandLineArgs,
                WorkingDirectory = workingDir ?? string.Empty,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);

            if (process == null)
            {
                _logger.LogError("启动游戏进程失败");
                return false;
            }

            _logger.LogInformation("游戏已启动，进程 ID: {ProcessId}", process.Id);

            // 等待一小段时间确保游戏正常启动
            await Task.Delay(1000, ct);

            if (process.HasExited)
            {
                _logger.LogError("游戏进程异常退出，退出码: {ExitCode}", process.ExitCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动游戏失败");
            return false;
        }
    }

    public bool IsGameInstalled()
    {
        var gamePath = GetGamePath();
        return File.Exists(gamePath);
    }

    public string GetGamePath()
    {
        if (Path.IsPathRooted(_config.Game.ExecutablePath))
        {
            return _config.Game.ExecutablePath;
        }

        // 相对路径，从启动器目录开始
        var launcherDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(launcherDir, _config.Game.ExecutablePath);
    }
}

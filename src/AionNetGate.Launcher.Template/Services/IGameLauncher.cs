namespace AionNetGate.Launcher.Template.Services;

/// <summary>
/// 游戏启动器接口
/// </summary>
public interface IGameLauncher
{
    /// <summary>
    /// 启动游戏
    /// </summary>
    Task<bool> LaunchGameAsync(CancellationToken ct = default);

    /// <summary>
    /// 检查游戏是否已安装
    /// </summary>
    bool IsGameInstalled();

    /// <summary>
    /// 获取游戏可执行文件路径
    /// </summary>
    string GetGamePath();
}

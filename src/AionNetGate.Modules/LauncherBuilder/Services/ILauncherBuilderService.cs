using AionNetGate.Core.Results;
using AionNetGate.Modules.LauncherBuilder.Models;

namespace AionNetGate.Modules.LauncherBuilder.Services;

/// <summary>
/// 启动器构建服务接口
/// </summary>
public interface ILauncherBuilderService
{
    /// <summary>
    /// 构建启动器
    /// </summary>
    /// <param name="config">构建配置</param>
    /// <param name="progress">构建进度报告</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>构建结果，包含输出文件路径</returns>
    Task<Result<string>> BuildLauncherAsync(
        LauncherBuildConfig config,
        IProgress<BuildProgress>? progress = null,
        CancellationToken ct = default);

    /// <summary>
    /// 验证构建配置
    /// </summary>
    Result ValidateBuildConfig(LauncherBuildConfig config);

    /// <summary>
    /// 获取可用的目标运行时
    /// </summary>
    IReadOnlyList<string> GetAvailableRuntimes();
}

/// <summary>
/// 构建进度（不可变记录）
/// </summary>
/// <param name="CurrentStep">当前步骤名称</param>
/// <param name="ProgressPercent">进度百分比 (0-100)</param>
/// <param name="Message">进度消息</param>
public readonly record struct BuildProgress(
    string CurrentStep,
    int ProgressPercent,
    string Message);

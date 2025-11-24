namespace AionNetGate.Core.Services;

/// <summary>
/// IP黑名单检查服务接口
/// </summary>
public interface IIPBlacklistChecker
{
    /// <summary>
    /// 检查IP是否被封禁
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <returns>是否被封禁</returns>
    Task<bool> IsBlacklistedAsync(string ipAddress);

    /// <summary>
    /// 获取封禁原因
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <returns>封禁原因（如果未封禁则返回null）</returns>
    Task<string?> GetBlockReasonAsync(string ipAddress);

    /// <summary>
    /// 获取剩余封禁时间（秒）
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <returns>剩余秒数（null表示永久封禁，0表示未封禁）</returns>
    Task<long?> GetRemainingTimeAsync(string ipAddress);

    /// <summary>
    /// 重新加载黑名单（从数据库）
    /// </summary>
    Task ReloadAsync();
}

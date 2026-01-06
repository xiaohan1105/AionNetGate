using AionNetGate.Core.Domain.Entities;

namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 硬件指纹仓储接口
/// </summary>
public interface IHardwareFingerprintRepository : IRepository<HardwareFingerprint>
{
    /// <summary>
    /// 根据硬件 ID 获取指纹
    /// </summary>
    Task<HardwareFingerprint?> GetByHardwareIdAsync(string hardwareId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取账号的所有硬件指纹
    /// </summary>
    Task<IEnumerable<HardwareFingerprint>> GetByAccountIdAsync(long accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查硬件 ID 是否被封禁
    /// </summary>
    Task<bool> IsHardwareIdBannedAsync(string hardwareId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 封禁硬件 ID
    /// </summary>
    Task BanHardwareIdAsync(string hardwareId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解封硬件 ID
    /// </summary>
    Task UnbanHardwareIdAsync(string hardwareId, CancellationToken cancellationToken = default);
}

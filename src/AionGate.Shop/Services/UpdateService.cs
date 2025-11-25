using AionGate.Shop.Models;
using AionGate.Shop.Repositories;
using AionGate.Updater;

namespace AionGate.Shop.Services;

/// <summary>
/// 更新服务
/// 处理版本检查、清单生成、进度跟踪等业务逻辑
/// </summary>
public class UpdateService
{
    private readonly UpdateRepository _repository;
    private readonly CDNUrlSigner _cdnSigner;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(
        UpdateRepository repository,
        CDNUrlSigner cdnSigner,
        ILogger<UpdateService> logger)
    {
        _repository = repository;
        _cdnSigner = cdnSigner;
        _logger = logger;
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    public async Task<UpdateCheckResponse> CheckForUpdateAsync(
        string clientVersion,
        string? channelCode,
        string clientIp)
    {
        var result = await _repository.CheckForUpdateAsync(clientVersion, channelCode);

        // 如果需要完整客户端，获取网盘下载链接
        if (result.NeedsFullClient)
        {
            result.FullPackageLinks = await GetFullPackageLinksAsync(result.LatestVersion);
            _logger.LogInformation(
                "客户端 {Version} 需要完整包，返回 {Count} 个下载链接",
                clientVersion,
                result.FullPackageLinks?.Count ?? 0
            );
        }
        else if (result.NeedsUpdate)
        {
            _logger.LogInformation(
                "客户端 {Version} 需要增量更新到 {LatestVersion}，文件数: {FileCount}，大小: {Size}",
                clientVersion,
                result.LatestVersion,
                result.FileCount,
                result.DownloadSizeText
            );
        }

        return result;
    }

    /// <summary>
    /// 获取版本清单（增量更新文件列表）
    /// </summary>
    public async Task<VersionManifestResponse?> GetVersionManifestAsync(
        string versionCode,
        string? fromVersion = null)
    {
        var manifest = await _repository.GetVersionManifestAsync(versionCode);

        if (manifest == null)
        {
            return null;
        }

        // 如果提供了fromVersion，计算增量差异
        if (!string.IsNullOrEmpty(fromVersion) && fromVersion != "0.0.0.0")
        {
            var diffFiles = await _repository.GetDiffFilesAsync(fromVersion, versionCode);
            if (diffFiles != null && diffFiles.Count > 0)
            {
                manifest.Files = diffFiles;
                _logger.LogInformation(
                    "生成增量清单: {From} -> {To}，差异文件数: {Count}",
                    fromVersion,
                    versionCode,
                    diffFiles.Count
                );
            }
        }

        // 为所有文件生成CDN签名URL
        foreach (var file in manifest.Files)
        {
            if (!string.IsNullOrEmpty(file.FilePath))
            {
                try
                {
                    // 生成60分钟有效期的签名URL
                    file.CdnUrl = _cdnSigner.GenerateSignedUrl(file.FilePath, 60);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "生成CDN URL失败: {FilePath}", file.FilePath);
                }
            }
        }

        return manifest;
    }

    /// <summary>
    /// 开始更新（记录日志）
    /// </summary>
    public async Task<long> StartUpdateAsync(
        long? accountId,
        string? channelCode,
        string? fromVersion,
        string toVersion,
        int totalFiles,
        long totalSize,
        string clientIp,
        bool useP2P)
    {
        var logId = await _repository.StartUpdateAsync(
            accountId,
            channelCode,
            fromVersion,
            toVersion,
            totalFiles,
            totalSize,
            clientIp,
            useP2P
        );

        _logger.LogInformation(
            "更新开始: LogId={LogId}, {From} -> {To}, Files={Files}, Size={Size}MB",
            logId,
            fromVersion ?? "新安装",
            toVersion,
            totalFiles,
            totalSize / 1048576.0
        );

        return logId;
    }

    /// <summary>
    /// 更新进度
    /// </summary>
    public async Task UpdateProgressAsync(
        long logId,
        int downloadedFiles,
        long downloadedSize,
        byte status,
        double? downloadSpeed,
        decimal? p2pRatio,
        string? errorMessage)
    {
        await _repository.UpdateProgressAsync(
            logId,
            downloadedFiles,
            downloadedSize,
            status,
            downloadSpeed,
            p2pRatio,
            errorMessage
        );

        if (status == 1) // 完成
        {
            _logger.LogInformation(
                "更新完成: LogId={LogId}, 文件数={Files}, 速度={Speed:F2}MB/s",
                logId,
                downloadedFiles,
                downloadSpeed ?? 0
            );
        }
        else if (status == 2) // 失败
        {
            _logger.LogWarning(
                "更新失败: LogId={LogId}, 错误={Error}",
                logId,
                errorMessage ?? "未知错误"
            );
        }
    }

    /// <summary>
    /// 获取完整客户端下载链接
    /// </summary>
    public async Task<List<FullPackageLinkDto>> GetFullPackageLinksAsync(string versionCode)
    {
        return await _repository.GetFullPackageLinksAsync(versionCode);
    }

    /// <summary>
    /// 记录下载次数
    /// </summary>
    public async Task IncrementDownloadCountAsync(long linkId)
    {
        await _repository.IncrementDownloadCountAsync(linkId);
        _logger.LogInformation("记录完整客户端下载: LinkId={LinkId}", linkId);
    }

    /// <summary>
    /// 获取活跃的CDN节点列表
    /// </summary>
    public async Task<List<CDNNodeDto>> GetActiveCDNNodesAsync()
    {
        return await _repository.GetActiveCDNNodesAsync();
    }
}

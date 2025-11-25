using System.Text.Json.Serialization;

namespace AionGate.Shop.Models;

/// <summary>
/// 更新检查请求
/// </summary>
public class UpdateCheckRequest
{
    [JsonPropertyName("client_version")]
    public string ClientVersion { get; set; } = "0.0.0.0";

    [JsonPropertyName("channel_code")]
    public string? ChannelCode { get; set; }

    [JsonPropertyName("hardware_id")]
    public string? HardwareId { get; set; }
}

/// <summary>
/// 更新检查响应
/// </summary>
public class UpdateCheckResponse
{
    [JsonPropertyName("needs_update")]
    public bool NeedsUpdate { get; set; }

    [JsonPropertyName("needs_full_client")]
    public bool NeedsFullClient { get; set; }

    [JsonPropertyName("current_version")]
    public string CurrentVersion { get; set; } = string.Empty;

    [JsonPropertyName("latest_version")]
    public string LatestVersion { get; set; } = string.Empty;

    [JsonPropertyName("update_type")]
    public string UpdateType { get; set; } = "incremental";

    [JsonPropertyName("is_forced")]
    public bool IsForced { get; set; }

    [JsonPropertyName("file_count")]
    public int FileCount { get; set; }

    [JsonPropertyName("download_size")]
    public long DownloadSize { get; set; }

    [JsonPropertyName("download_size_text")]
    public string DownloadSizeText { get; set; } = string.Empty;

    [JsonPropertyName("estimated_time")]
    public int EstimatedTime { get; set; }

    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }

    [JsonPropertyName("full_package_links")]
    public List<FullPackageLinkDto>? FullPackageLinks { get; set; }
}

/// <summary>
/// 完整客户端下载链接
/// </summary>
public class FullPackageLinkDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("version_code")]
    public string VersionCode { get; set; } = string.Empty;

    [JsonPropertyName("package_name")]
    public string PackageName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("type_name")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("verification_code")]
    public string? VerificationCode { get; set; }

    [JsonPropertyName("extraction_password")]
    public string? ExtractionPassword { get; set; }

    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }

    [JsonPropertyName("file_size_text")]
    public string FileSizeText { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("is_recommended")]
    public bool IsRecommended { get; set; }

    [JsonPropertyName("download_count")]
    public int DownloadCount { get; set; }
}

/// <summary>
/// 版本清单响应
/// </summary>
public class VersionManifestResponse
{
    [JsonPropertyName("version_code")]
    public string VersionCode { get; set; } = string.Empty;

    [JsonPropertyName("version_name")]
    public string VersionName { get; set; } = string.Empty;

    [JsonPropertyName("file_count")]
    public int FileCount { get; set; }

    [JsonPropertyName("total_size")]
    public long TotalSize { get; set; }

    [JsonPropertyName("download_size")]
    public long DownloadSize { get; set; }

    [JsonPropertyName("is_forced")]
    public bool IsForced { get; set; }

    [JsonPropertyName("manifest_hash")]
    public string? ManifestHash { get; set; }

    [JsonPropertyName("files")]
    public List<UpdateFileDto> Files { get; set; } = new();
}

/// <summary>
/// 更新文件信息
/// </summary>
public class UpdateFileDto
{
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }

    [JsonPropertyName("file_hash")]
    public string FileHash { get; set; } = string.Empty;

    [JsonPropertyName("file_crc32")]
    public string FileCrc32 { get; set; } = string.Empty;

    [JsonPropertyName("compressed_size")]
    public long? CompressedSize { get; set; }

    [JsonPropertyName("compression_type")]
    public string? CompressionType { get; set; }

    [JsonPropertyName("cdn_url")]
    public string? CdnUrl { get; set; }

    [JsonPropertyName("is_critical")]
    public bool IsCritical { get; set; }

    [JsonPropertyName("download_priority")]
    public int DownloadPriority { get; set; }
}

/// <summary>
/// 更新开始请求
/// </summary>
public class UpdateStartRequest
{
    [JsonPropertyName("account_id")]
    public long? AccountId { get; set; }

    [JsonPropertyName("channel_code")]
    public string? ChannelCode { get; set; }

    [JsonPropertyName("from_version")]
    public string? FromVersion { get; set; }

    [JsonPropertyName("to_version")]
    public string ToVersion { get; set; } = string.Empty;

    [JsonPropertyName("total_files")]
    public int TotalFiles { get; set; }

    [JsonPropertyName("total_size")]
    public long TotalSize { get; set; }

    [JsonPropertyName("use_p2p")]
    public bool UseP2P { get; set; }
}

/// <summary>
/// 更新开始响应
/// </summary>
public class UpdateStartResponse
{
    [JsonPropertyName("log_id")]
    public long LogId { get; set; }
}

/// <summary>
/// 更新进度请求
/// </summary>
public class UpdateProgressRequest
{
    [JsonPropertyName("log_id")]
    public long LogId { get; set; }

    [JsonPropertyName("downloaded_files")]
    public int DownloadedFiles { get; set; }

    [JsonPropertyName("downloaded_size")]
    public long DownloadedSize { get; set; }

    [JsonPropertyName("status")]
    public byte Status { get; set; }

    [JsonPropertyName("download_speed")]
    public double? DownloadSpeed { get; set; }

    [JsonPropertyName("p2p_ratio")]
    public decimal? P2PRatio { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// CDN节点信息
/// </summary>
public class CDNNodeDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("node_name")]
    public string NodeName { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [JsonPropertyName("cdn_domain")]
    public string? CdnDomain { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("bandwidth_limit")]
    public int? BandwidthLimit { get; set; }
}

using System.Data;
using Microsoft.Data.SqlClient;
using AionGate.Shop.Models;

namespace AionGate.Shop.Repositories;

/// <summary>
/// 更新数据仓库
/// 负责所有更新相关的数据库操作
/// </summary>
public class UpdateRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UpdateRepository> _logger;

    public UpdateRepository(IConfiguration configuration, ILogger<UpdateRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("AionDB")
            ?? throw new InvalidOperationException("数据库连接字符串未配置");
        _logger = logger;
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    public async Task<UpdateCheckResponse> CheckForUpdateAsync(string clientVersion, string? channelCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("sp_CheckForUpdate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ClientVersion", clientVersion);
        command.Parameters.AddWithValue("@ChannelCode", (object?)channelCode ?? DBNull.Value);

        var response = new UpdateCheckResponse();

        using var reader = await command.ExecuteReaderAsync();

        // 第一个结果集：更新信息
        if (await reader.ReadAsync())
        {
            response.NeedsUpdate = reader.GetBoolean(reader.GetOrdinal("needs_update"));
            response.NeedsFullClient = reader.GetBoolean(reader.GetOrdinal("needs_full_client"));
            response.CurrentVersion = reader.GetString(reader.GetOrdinal("current_version"));
            response.LatestVersion = reader.GetString(reader.GetOrdinal("latest_version"));
            response.UpdateType = reader.GetString(reader.GetOrdinal("update_type"));
            response.IsForced = reader.GetBoolean(reader.GetOrdinal("is_forced"));
            response.FileCount = reader.GetInt32(reader.GetOrdinal("file_count"));
            response.DownloadSize = reader.GetInt64(reader.GetOrdinal("download_size"));
            response.DownloadSizeText = reader.GetString(reader.GetOrdinal("download_size_text"));
            response.EstimatedTime = reader.IsDBNull(reader.GetOrdinal("estimated_time"))
                ? 0
                : reader.GetInt32(reader.GetOrdinal("estimated_time"));
            response.Changelog = reader.IsDBNull(reader.GetOrdinal("changelog"))
                ? null
                : reader.GetString(reader.GetOrdinal("changelog"));
        }

        // 第二个结果集：完整客户端链接（如果需要）
        if (response.NeedsFullClient && await reader.NextResultAsync())
        {
            response.FullPackageLinks = new List<FullPackageLinkDto>();

            while (await reader.ReadAsync())
            {
                response.FullPackageLinks.Add(new FullPackageLinkDto
                {
                    Id = reader.GetInt64(reader.GetOrdinal("id")),
                    VersionCode = reader.GetString(reader.GetOrdinal("version_code")),
                    PackageName = reader.GetString(reader.GetOrdinal("package_name")),
                    Type = reader.GetString(reader.GetOrdinal("type")),
                    TypeName = reader.GetString(reader.GetOrdinal("type_name")),
                    Url = reader.GetString(reader.GetOrdinal("url")),
                    VerificationCode = reader.IsDBNull(reader.GetOrdinal("verification_code"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("verification_code")),
                    ExtractionPassword = reader.IsDBNull(reader.GetOrdinal("extraction_password"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("extraction_password")),
                    FileSize = reader.GetInt64(reader.GetOrdinal("file_size")),
                    FileSizeText = reader.GetString(reader.GetOrdinal("file_size_text")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("description")),
                    Priority = reader.GetInt32(reader.GetOrdinal("priority")),
                    IsRecommended = reader.GetInt32(reader.GetOrdinal("is_recommended")) == 1
                });
            }
        }

        return response;
    }

    /// <summary>
    /// 获取版本清单
    /// </summary>
    public async Task<VersionManifestResponse?> GetVersionManifestAsync(string versionCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // 获取版本ID
        long? versionId = null;
        using (var cmd = new SqlCommand(
            "SELECT id FROM update_versions WHERE version_code = @VersionCode AND is_published = 1",
            connection))
        {
            cmd.Parameters.AddWithValue("@VersionCode", versionCode);
            var result = await cmd.ExecuteScalarAsync();
            if (result != null)
            {
                versionId = Convert.ToInt64(result);
            }
        }

        if (!versionId.HasValue)
        {
            return null;
        }

        using var command = new SqlCommand("sp_GenerateVersionManifest", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@VersionID", versionId.Value);

        var manifest = new VersionManifestResponse();

        using var reader = await command.ExecuteReaderAsync();

        // 第一个结果集：版本信息
        if (await reader.ReadAsync())
        {
            manifest.VersionCode = reader.GetString(reader.GetOrdinal("version_code"));
            manifest.VersionName = reader.GetString(reader.GetOrdinal("version_name"));
            manifest.FileCount = reader.GetInt32(reader.GetOrdinal("file_count"));
            manifest.TotalSize = reader.GetInt64(reader.GetOrdinal("total_size"));
            manifest.DownloadSize = reader.GetInt64(reader.GetOrdinal("download_size"));
            manifest.IsForced = reader.GetBoolean(reader.GetOrdinal("is_forced"));
            manifest.ManifestHash = reader.IsDBNull(reader.GetOrdinal("manifest_hash"))
                ? null
                : reader.GetString(reader.GetOrdinal("manifest_hash"));
        }

        // 第二个结果集：文件列表
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                manifest.Files.Add(new UpdateFileDto
                {
                    FilePath = reader.GetString(reader.GetOrdinal("file_path")),
                    FileSize = reader.GetInt64(reader.GetOrdinal("file_size")),
                    FileHash = reader.GetString(reader.GetOrdinal("file_hash")),
                    FileCrc32 = reader.GetString(reader.GetOrdinal("file_crc32")),
                    CompressedSize = reader.IsDBNull(reader.GetOrdinal("compressed_size"))
                        ? null
                        : reader.GetInt64(reader.GetOrdinal("compressed_size")),
                    CompressionType = reader.IsDBNull(reader.GetOrdinal("compression_type"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("compression_type")),
                    IsCritical = reader.GetBoolean(reader.GetOrdinal("is_critical")),
                    DownloadPriority = reader.GetInt32(reader.GetOrdinal("download_priority"))
                });
            }
        }

        return manifest;
    }

    /// <summary>
    /// 获取增量差异文件
    /// </summary>
    public async Task<List<UpdateFileDto>?> GetDiffFilesAsync(string fromVersion, string toVersion)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // 获取版本ID
        long? fromVersionId = null, toVersionId = null;

        using (var cmd = new SqlCommand(
            "SELECT id, version_code FROM update_versions WHERE version_code IN (@FromVersion, @ToVersion) AND is_published = 1",
            connection))
        {
            cmd.Parameters.AddWithValue("@FromVersion", fromVersion);
            cmd.Parameters.AddWithValue("@ToVersion", toVersion);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt64(0);
                var code = reader.GetString(1);

                if (code == fromVersion) fromVersionId = id;
                if (code == toVersion) toVersionId = id;
            }
        }

        if (!fromVersionId.HasValue || !toVersionId.HasValue)
        {
            return null;
        }

        // 计算差异
        using var command = new SqlCommand("sp_CalculateDiffUpdate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@FromVersionID", fromVersionId.Value);
        command.Parameters.AddWithValue("@ToVersionID", toVersionId.Value);

        await command.ExecuteNonQueryAsync();

        // 获取差异文件列表（新增和修改的文件）
        var files = new List<UpdateFileDto>();

        using (var cmd = new SqlCommand(@"
            SELECT f.file_path, f.file_size, f.file_hash, f.file_crc32,
                   f.compressed_size, f.compression_type, f.is_critical, f.download_priority
            FROM update_file_diffs d
            INNER JOIN update_files f ON f.file_path = d.file_path AND f.version_id = @ToVersionID
            WHERE d.from_version_id = @FromVersionID
              AND d.to_version_id = @ToVersionID
              AND d.diff_type IN (1, 2)
            ORDER BY f.download_priority DESC, f.file_size ASC",
            connection))
        {
            cmd.Parameters.AddWithValue("@FromVersionID", fromVersionId.Value);
            cmd.Parameters.AddWithValue("@ToVersionID", toVersionId.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(new UpdateFileDto
                {
                    FilePath = reader.GetString(0),
                    FileSize = reader.GetInt64(1),
                    FileHash = reader.GetString(2),
                    FileCrc32 = reader.GetString(3),
                    CompressedSize = reader.IsDBNull(4) ? null : reader.GetInt64(4),
                    CompressionType = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsCritical = reader.GetBoolean(6),
                    DownloadPriority = reader.GetInt32(7)
                });
            }
        }

        return files;
    }

    /// <summary>
    /// 开始更新
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
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("sp_StartClientUpdate", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@AccountID", (object?)accountId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChannelCode", (object?)channelCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@FromVersion", (object?)fromVersion ?? DBNull.Value);
        command.Parameters.AddWithValue("@ToVersion", toVersion);
        command.Parameters.AddWithValue("@UpdateType", string.IsNullOrEmpty(fromVersion) ? 1 : 2);
        command.Parameters.AddWithValue("@TotalFiles", totalFiles);
        command.Parameters.AddWithValue("@TotalSize", totalSize);
        command.Parameters.AddWithValue("@ClientIP", clientIp);
        command.Parameters.AddWithValue("@UseP2P", useP2P);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
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
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("sp_UpdateClientUpdateProgress", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@LogID", logId);
        command.Parameters.AddWithValue("@DownloadedFiles", downloadedFiles);
        command.Parameters.AddWithValue("@DownloadedSize", downloadedSize);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@DownloadSpeed", (object?)downloadSpeed ?? DBNull.Value);
        command.Parameters.AddWithValue("@P2PRatio", (object?)p2pRatio ?? DBNull.Value);
        command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 获取完整客户端下载链接
    /// </summary>
    public async Task<List<FullPackageLinkDto>> GetFullPackageLinksAsync(string versionCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("sp_GetFullPackageLinks", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@VersionCode", versionCode);

        var links = new List<FullPackageLinkDto>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            links.Add(new FullPackageLinkDto
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                VersionCode = reader.GetString(reader.GetOrdinal("version_code")),
                PackageName = reader.GetString(reader.GetOrdinal("package_name")),
                Type = reader.GetString(reader.GetOrdinal("download_type")),
                TypeName = reader.GetString(reader.GetOrdinal("type_name")),
                Url = reader.GetString(reader.GetOrdinal("download_url")),
                VerificationCode = reader.IsDBNull(reader.GetOrdinal("verification_code"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("verification_code")),
                ExtractionPassword = reader.IsDBNull(reader.GetOrdinal("extraction_password"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("extraction_password")),
                FileSize = reader.GetInt64(reader.GetOrdinal("file_size")),
                FileSizeText = reader.GetString(reader.GetOrdinal("file_size_text")),
                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("description")),
                Priority = reader.GetInt32(reader.GetOrdinal("priority")),
                IsRecommended = reader.GetInt32(reader.GetOrdinal("is_recommended")) == 1,
                DownloadCount = reader.GetInt32(reader.GetOrdinal("download_count"))
            });
        }

        return links;
    }

    /// <summary>
    /// 增加下载计数
    /// </summary>
    public async Task IncrementDownloadCountAsync(long linkId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("sp_IncrementDownloadCount", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ID", linkId);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 获取活跃CDN节点
    /// </summary>
    public async Task<List<CDNNodeDto>> GetActiveCDNNodesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(@"
            SELECT id, node_name, provider, region, endpoint, cdn_domain, priority, bandwidth_limit
            FROM cdn_nodes
            WHERE is_enabled = 1
            ORDER BY priority DESC",
            connection);

        var nodes = new List<CDNNodeDto>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            nodes.Add(new CDNNodeDto
            {
                Id = reader.GetInt64(0),
                NodeName = reader.GetString(1),
                Provider = reader.GetString(2),
                Region = reader.IsDBNull(3) ? null : reader.GetString(3),
                Endpoint = reader.GetString(4),
                CdnDomain = reader.IsDBNull(5) ? null : reader.GetString(5),
                Priority = reader.GetInt32(6),
                BandwidthLimit = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            });
        }

        return nodes;
    }
}

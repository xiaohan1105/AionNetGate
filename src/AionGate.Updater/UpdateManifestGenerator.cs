using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AionGate.Updater;

/// <summary>
/// 版本清单生成器 - 扫描游戏文件并生成更新清单
/// </summary>
public class UpdateManifestGenerator
{
    private readonly string _gameDirectory;
    private readonly string[] _excludePatterns;

    public UpdateManifestGenerator(string gameDirectory, string[]? excludePatterns = null)
    {
        _gameDirectory = gameDirectory ?? throw new ArgumentNullException(nameof(gameDirectory));
        _excludePatterns = excludePatterns ?? new[]
        {
            "*.log", "*.tmp", "*.cache",
            "Screenshots/*", "Logs/*", "Config/user_*.ini"
        };
    }

    /// <summary>
    /// 扫描目录生成版本清单
    /// </summary>
    public async Task<VersionManifest> GenerateManifestAsync(
        string versionCode,
        string versionName,
        CancellationToken cancellationToken = default)
    {
        var manifest = new VersionManifest
        {
            VersionCode = versionCode,
            VersionName = versionName,
            GeneratedAt = DateTime.UtcNow,
            Files = new List<FileEntry>()
        };

        var files = Directory.GetFiles(_gameDirectory, "*", SearchOption.AllDirectories);
        var totalFiles = files.Length;
        var processedFiles = 0;

        Console.WriteLine($"扫描游戏目录: {_gameDirectory}");
        Console.WriteLine($"找到 {totalFiles} 个文件");

        foreach (var filePath in files)
        {
            if (ShouldExclude(filePath))
                continue;

            var entry = await CreateFileEntryAsync(filePath, cancellationToken);
            manifest.Files.Add(entry);

            processedFiles++;
            if (processedFiles % 100 == 0)
            {
                Console.WriteLine($"进度: {processedFiles}/{totalFiles} ({(processedFiles * 100.0 / totalFiles):F1}%)");
            }
        }

        manifest.FileCount = manifest.Files.Count;
        manifest.TotalSize = manifest.Files.Sum(f => f.FileSize);
        manifest.ManifestHash = ComputeManifestHash(manifest);

        Console.WriteLine($"清单生成完成:");
        Console.WriteLine($"  - 文件数: {manifest.FileCount}");
        Console.WriteLine($"  - 总大小: {FormatSize(manifest.TotalSize)}");
        Console.WriteLine($"  - 清单Hash: {manifest.ManifestHash}");

        return manifest;
    }

    /// <summary>
    /// 比对两个版本，生成差分清单
    /// </summary>
    public DiffManifest GenerateDiffManifest(
        VersionManifest oldManifest,
        VersionManifest newManifest)
    {
        var diff = new DiffManifest
        {
            FromVersion = oldManifest.VersionCode,
            ToVersion = newManifest.VersionCode,
            GeneratedAt = DateTime.UtcNow,
            NewFiles = new List<FileEntry>(),
            ModifiedFiles = new List<FileEntry>(),
            DeletedFiles = new List<string>()
        };

        var oldFiles = oldManifest.Files.ToDictionary(f => f.FilePath, f => f);
        var newFiles = newManifest.Files.ToDictionary(f => f.FilePath, f => f);

        // 查找新增和修改的文件
        foreach (var (path, newFile) in newFiles)
        {
            if (!oldFiles.TryGetValue(path, out var oldFile))
            {
                // 新增文件
                diff.NewFiles.Add(newFile);
            }
            else if (oldFile.FileHash != newFile.FileHash)
            {
                // 修改文件
                diff.ModifiedFiles.Add(newFile);
            }
        }

        // 查找删除的文件
        foreach (var path in oldFiles.Keys)
        {
            if (!newFiles.ContainsKey(path))
            {
                diff.DeletedFiles.Add(path);
            }
        }

        diff.TotalDiffs = diff.NewFiles.Count + diff.ModifiedFiles.Count + diff.DeletedFiles.Count;
        diff.DownloadSize = diff.NewFiles.Sum(f => f.FileSize) + diff.ModifiedFiles.Sum(f => f.FileSize);

        Console.WriteLine($"差分清单生成完成:");
        Console.WriteLine($"  - 新增: {diff.NewFiles.Count} 个文件");
        Console.WriteLine($"  - 修改: {diff.ModifiedFiles.Count} 个文件");
        Console.WriteLine($"  - 删除: {diff.DeletedFiles.Count} 个文件");
        Console.WriteLine($"  - 下载大小: {FormatSize(diff.DownloadSize)}");

        return diff;
    }

    /// <summary>
    /// 创建文件条目
    /// </summary>
    private async Task<FileEntry> CreateFileEntryAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(filePath);
        var relativePath = Path.GetRelativePath(_gameDirectory, filePath);

        // 计算文件Hash和CRC32
        await using var stream = File.OpenRead(filePath);
        var (sha256, crc32) = await ComputeHashesAsync(stream, cancellationToken);

        var entry = new FileEntry
        {
            FilePath = relativePath.Replace('\\', '/'), // 统一使用正斜杠
            FileSize = fileInfo.Length,
            FileHash = sha256,
            FileCrc32 = crc32,
            IsCritical = IsCriticalFile(relativePath),
            DownloadPriority = CalculatePriority(relativePath, fileInfo.Length)
        };

        // 大文件启用分块下载
        if (fileInfo.Length > 50 * 1024 * 1024) // 50MB
        {
            entry.ChunkSize = 10 * 1024 * 1024; // 10MB chunks
            entry.ChunkCount = (int)Math.Ceiling(fileInfo.Length / (double)entry.ChunkSize);
        }

        return entry;
    }

    /// <summary>
    /// 高性能计算文件Hash和CRC32
    /// </summary>
    private static async Task<(string sha256, string crc32)> ComputeHashesAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var crc32 = new Crc32();

        var buffer = new byte[81920]; // 80KB buffer
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            crc32.Append(buffer.AsSpan(0, bytesRead));
        }

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        var sha256Hash = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
        var crc32Hash = Convert.ToHexString(crc32.GetCurrentHash()).ToLowerInvariant();

        return (sha256Hash, crc32Hash);
    }

    /// <summary>
    /// 计算清单Hash
    /// </summary>
    private static string ComputeManifestHash(VersionManifest manifest)
    {
        var json = JsonSerializer.Serialize(manifest.Files.OrderBy(f => f.FilePath));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// 判断文件是否应该排除
    /// </summary>
    private bool ShouldExclude(string filePath)
    {
        var relativePath = Path.GetRelativePath(_gameDirectory, filePath);

        foreach (var pattern in _excludePatterns)
        {
            // 简单的通配符匹配
            if (pattern.Contains('*'))
            {
                var regex = new System.Text.RegularExpressions.Regex(
                    "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (regex.IsMatch(relativePath))
                    return true;
            }
            else if (relativePath.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断是否为核心文件（必须下载）
    /// </summary>
    private static bool IsCriticalFile(string relativePath)
    {
        var criticalPaths = new[]
        {
            "bin32/", "bin64/",
            "Data/Levels/",
            "Data/Objects/",
            "l10n/"
        };

        return criticalPaths.Any(p => relativePath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 计算下载优先级
    /// </summary>
    private static int CalculatePriority(string relativePath, long fileSize)
    {
        // 可执行文件最高优先级
        if (relativePath.EndsWith(".exe") || relativePath.EndsWith(".dll"))
            return 100;

        // 核心文件高优先级
        if (IsCriticalFile(relativePath))
            return 80;

        // 小文件优先下载（快速进入游戏）
        if (fileSize < 1024 * 1024) // < 1MB
            return 70;

        // 大文件低优先级（可在后台下载）
        if (fileSize > 100 * 1024 * 1024) // > 100MB
            return 30;

        return 50; // 默认优先级
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}

/// <summary>
/// 版本清单
/// </summary>
public class VersionManifest
{
    [JsonPropertyName("version_code")]
    public string VersionCode { get; set; } = string.Empty;

    [JsonPropertyName("version_name")]
    public string VersionName { get; set; } = string.Empty;

    [JsonPropertyName("generated_at")]
    public DateTime GeneratedAt { get; set; }

    [JsonPropertyName("file_count")]
    public int FileCount { get; set; }

    [JsonPropertyName("total_size")]
    public long TotalSize { get; set; }

    [JsonPropertyName("manifest_hash")]
    public string ManifestHash { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<FileEntry> Files { get; set; } = new();
}

/// <summary>
/// 文件条目
/// </summary>
public class FileEntry
{
    [JsonPropertyName("path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long FileSize { get; set; }

    [JsonPropertyName("hash")]
    public string FileHash { get; set; } = string.Empty;

    [JsonPropertyName("crc32")]
    public string FileCrc32 { get; set; } = string.Empty;

    [JsonPropertyName("critical")]
    public bool IsCritical { get; set; }

    [JsonPropertyName("priority")]
    public int DownloadPriority { get; set; }

    [JsonPropertyName("chunk_size")]
    public int? ChunkSize { get; set; }

    [JsonPropertyName("chunk_count")]
    public int? ChunkCount { get; set; }
}

/// <summary>
/// 差分清单
/// </summary>
public class DiffManifest
{
    [JsonPropertyName("from_version")]
    public string FromVersion { get; set; } = string.Empty;

    [JsonPropertyName("to_version")]
    public string ToVersion { get; set; } = string.Empty;

    [JsonPropertyName("generated_at")]
    public DateTime GeneratedAt { get; set; }

    [JsonPropertyName("total_diffs")]
    public int TotalDiffs { get; set; }

    [JsonPropertyName("download_size")]
    public long DownloadSize { get; set; }

    [JsonPropertyName("new_files")]
    public List<FileEntry> NewFiles { get; set; } = new();

    [JsonPropertyName("modified_files")]
    public List<FileEntry> ModifiedFiles { get; set; } = new();

    [JsonPropertyName("deleted_files")]
    public List<string> DeletedFiles { get; set; } = new();
}

# 登录器集成指南

本指南详细说明如何在游戏启动器中集成 AionGate 热更新系统。

---

## 目录

1. [架构概览](#架构概览)
2. [UI设计建议](#ui设计建议)
3. [实现步骤](#实现步骤)
4. [进度显示](#进度显示)
5. [错误处理](#错误处理)
6. [测试清单](#测试清单)
7. [常见问题](#常见问题)

---

## 架构概览

```
┌──────────────────────────────────────────────────────────────┐
│                       游戏启动器                              │
│                                                              │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │ 版本检查器 │──▶│  更新管理器  │──▶│  进度UI组件      │    │
│  └────────────┘  └──────────────┘  └──────────────────┘    │
│        │                 │                    │              │
│        │                 │                    │              │
│        ▼                 ▼                    ▼              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              AionGate 更新 API                       │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## UI设计建议

### 1. 新用户安装界面

```
┌────────────────────────────────────────────────────────────────┐
│  🎮 欢迎来到永恒之塔                                            │
│                                                                │
│  请选择下载方式下载完整客户端（14.65 GB）：                      │
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  ⭐ 百度网盘（推荐）                    下载次数: 1,523   │ │
│  │  下载速度快，推荐优先使用                                  │ │
│  │                                                            │ │
│  │  提取码: abc123    解压密码: aion2024                     │ │
│  │                                                            │ │
│  │  [📥 点击下载]  [📋 复制提取码]                           │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  ⭐ 阿里云盘（推荐）                    下载次数: 892     │ │
│  │  下载速度快，推荐                                          │ │
│  │                                                            │ │
│  │  提取码: xyz789    解压密码: aion2024                     │ │
│  │                                                            │ │
│  │  [📥 点击下载]  [📋 复制提取码]                           │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  迅雷云盘                              下载次数: 342     │ │
│  │  支持迅雷加速下载                                          │ │
│  │                                                            │ │
│  │  解压密码: aion2024                                       │ │
│  │                                                            │ │
│  │  [📥 点击下载]                                            │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
│  💡 下载完成后，解压到任意目录，再次运行本启动器即可。         │
│                                                                │
│  [❓ 下载教程]  [💬 联系客服]                                  │
└────────────────────────────────────────────────────────────────┘
```

### 2. 增量更新界面

```
┌────────────────────────────────────────────────────────────────┐
│  🔄 发现新版本 2.7.0.16                                         │
│                                                                │
│  【更新日志】                                                   │
│  ├─ 新增: 全新副本「永恒之塔」                                  │
│  ├─ 优化: 提升战斗流畅度                                        │
│  └─ 修复: 修复已知BUG                                          │
│                                                                │
│  更新大小: 1.75 GB                                             │
│  预计时间: 5分钟                                               │
│                                                                │
│  [🚀 立即更新]  [❌ 稍后提醒]                                  │
└────────────────────────────────────────────────────────────────┘
```

### 3. 更新进度界面

```
┌────────────────────────────────────────────────────────────────┐
│  🔄 正在更新... 2.7.0.15 → 2.7.0.16                            │
│                                                                │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 50%             │
│                                                                │
│  正在下载更新文件...                                            │
│  当前文件: Data/Levels/level1.pak                              │
│  文件进度: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 75%           │
│                                                                │
│  已完成: 92/184 文件                                           │
│  已下载: 939 MB / 1.75 GB                                      │
│  下载速度: 52.5 MB/s                                           │
│  剩余时间: 约 2分30秒                                          │
│  P2P加速: 25% (节省CDN流量)                                    │
│                                                                │
│  [⏸️ 暂停]  [❌ 取消]                                          │
│                                                                │
│  💡 提示: 请勿关闭启动器，更新完成后自动启动游戏                │
└────────────────────────────────────────────────────────────────┘
```

---

## 实现步骤

### 步骤1：初始化配置

```csharp
public class UpdateConfig
{
    public string ApiBaseUrl { get; set; } = "https://your-gateway.com/api/update";
    public string LocalGamePath { get; set; } = @"D:\Games\Aion";
    public string VersionFile { get; set; } = "version.txt";
    public int MaxConcurrentDownloads { get; set; } = 8;
    public int RetryCount { get; set; } = 3;
    public bool EnableP2P { get; set; } = true;
}
```

### 步骤2：实现版本检查器

```csharp
public class VersionChecker
{
    private readonly UpdateConfig _config;
    private readonly HttpClient _httpClient;

    public VersionChecker(UpdateConfig config)
    {
        _config = config;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.ApiBaseUrl)
        };
    }

    public async Task<UpdateCheckResponse> CheckForUpdateAsync()
    {
        // 读取本地版本号
        var localVersion = GetLocalVersion();

        // 请求服务器检查更新
        var request = new
        {
            client_version = localVersion,
            channel_code = "official",
            hardware_id = GetHardwareId()
        };

        var response = await _httpClient.PostAsJsonAsync("/check", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UpdateCheckResponse>();
    }

    private string GetLocalVersion()
    {
        var versionFilePath = Path.Combine(_config.LocalGamePath, _config.VersionFile);

        if (!File.Exists(versionFilePath))
            return "0.0.0.0"; // 首次安装

        return File.ReadAllText(versionFilePath).Trim();
    }

    private string GetHardwareId()
    {
        // 生成硬件唯一标识（用于统计）
        var cpuId = GetCpuId();
        var diskId = GetDiskSerial();

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(
            Encoding.UTF8.GetBytes($"{cpuId}{diskId}")
        );

        return Convert.ToHexString(hash);
    }
}
```

### 步骤3：实现更新管理器

```csharp
public class UpdateManager
{
    private readonly UpdateConfig _config;
    private readonly HttpClient _httpClient;
    private long _updateLogId;

    public event Action<LauncherUpdateProgress> ProgressChanged;

    public UpdateManager(UpdateConfig config)
    {
        _config = config;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.ApiBaseUrl)
        };
    }

    public async Task<bool> ExecuteUpdateAsync(
        UpdateCheckResponse updateInfo,
        CancellationToken cancellationToken = default)
    {
        if (!updateInfo.NeedsUpdate)
            return true;

        if (updateInfo.NeedsFullClient)
        {
            // 显示网盘下载界面
            ShowFullClientDownloadUI(updateInfo.FullPackageLinks);
            return false; // 等待用户手动下载完整包
        }

        try
        {
            // 获取版本清单
            var manifest = await GetVersionManifestAsync(
                updateInfo.LatestVersion,
                updateInfo.CurrentVersion
            );

            // 记录更新开始
            _updateLogId = await StartUpdateLoggingAsync(
                updateInfo.CurrentVersion,
                updateInfo.LatestVersion,
                manifest
            );

            // 下载文件
            await DownloadFilesAsync(manifest, cancellationToken);

            // 更新版本号
            SaveLocalVersion(updateInfo.LatestVersion);

            // 上报完成
            await ReportProgressAsync(
                manifest.Files.Count,
                manifest.TotalSize,
                UpdateStatus.Completed
            );

            return true;
        }
        catch (Exception ex)
        {
            // 上报失败
            await ReportProgressAsync(
                0,
                0,
                UpdateStatus.Failed,
                ex.Message
            );

            throw;
        }
    }

    private async Task<VersionManifestResponse> GetVersionManifestAsync(
        string toVersion,
        string fromVersion)
    {
        var url = $"/manifest/{toVersion}?fromVersion={fromVersion}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<VersionManifestResponse>();
    }

    private async Task<long> StartUpdateLoggingAsync(
        string fromVersion,
        string toVersion,
        VersionManifestResponse manifest)
    {
        var request = new
        {
            from_version = fromVersion,
            to_version = toVersion,
            total_files = manifest.FileCount,
            total_size = manifest.TotalSize,
            use_p2p = _config.EnableP2P
        };

        var response = await _httpClient.PostAsJsonAsync("/start", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UpdateStartResponse>();
        return result.LogId;
    }

    private async Task DownloadFilesAsync(
        VersionManifestResponse manifest,
        CancellationToken cancellationToken)
    {
        // 按优先级排序
        var files = manifest.Files
            .OrderByDescending(f => f.DownloadPriority)
            .ThenBy(f => f.FileSize)
            .ToList();

        var downloadedFiles = 0;
        var downloadedSize = 0L;
        var startTime = DateTime.UtcNow;

        // 并发下载（8个线程）
        var semaphore = new SemaphoreSlim(_config.MaxConcurrentDownloads);
        var tasks = files.Select(async file =>
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                var localPath = Path.Combine(_config.LocalGamePath, file.FilePath);

                // 创建目录
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                // 下载文件
                await DownloadFileAsync(file.CdnUrl, localPath, file.FileSize, cancellationToken);

                // 校验文件
                await VerifyFileAsync(localPath, file.FileHash, file.FileCrc32);

                // 更新进度
                Interlocked.Increment(ref downloadedFiles);
                Interlocked.Add(ref downloadedSize, file.FileSize);

                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                var speed = downloadedSize / 1048576.0 / elapsed; // MB/s
                var remaining = (manifest.TotalSize - downloadedSize) / speed; // 秒

                var progress = new LauncherUpdateProgress
                {
                    Stage = UpdateStage.DownloadingFiles,
                    CurrentFile = file.FilePath,
                    CurrentFileProgress = 100,
                    TotalFiles = files.Count,
                    CompletedFiles = downloadedFiles,
                    OverallProgress = (int)(downloadedSize * 100 / manifest.TotalSize),
                    TotalBytes = manifest.TotalSize,
                    DownloadedBytes = downloadedSize,
                    DownloadSpeed = (long)(speed * 1048576),
                    RemainingSeconds = (int)remaining
                };

                ProgressChanged?.Invoke(progress);

                // 每5个文件上报一次进度
                if (downloadedFiles % 5 == 0)
                {
                    await ReportProgressAsync(
                        downloadedFiles,
                        downloadedSize,
                        UpdateStatus.InProgress,
                        downloadSpeed: speed
                    );
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task DownloadFileAsync(
        string url,
        string localPath,
        long fileSize,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();

        for (int retry = 0; retry < _config.RetryCount; retry++)
        {
            try
            {
                // 检查是否已有部分下载
                var existingLength = 0L;
                if (File.Exists(localPath))
                {
                    existingLength = new FileInfo(localPath).Length;
                }

                // 断点续传
                if (existingLength > 0 && existingLength < fileSize)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);

                    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var fileStream = new FileStream(localPath, FileMode.Append, FileAccess.Write, FileShare.None);

                    await stream.CopyToAsync(fileStream, cancellationToken);
                }
                else
                {
                    // 全新下载
                    using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None);

                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                return; // 成功
            }
            catch (Exception ex) when (retry < _config.RetryCount - 1)
            {
                // 重试
                await Task.Delay(1000 * (retry + 1), cancellationToken);
            }
        }
    }

    private async Task VerifyFileAsync(string filePath, string expectedHash, string expectedCrc32)
    {
        using var stream = File.OpenRead(filePath);

        // 计算SHA256
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream);
        var hashStr = "sha256:" + Convert.ToHexString(hash).ToLowerInvariant();

        if (hashStr != expectedHash)
        {
            throw new InvalidDataException($"文件校验失败: {filePath}");
        }

        // 计算CRC32
        stream.Position = 0;
        var crc32 = new Crc32();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            crc32.Append(buffer.AsSpan(0, bytesRead));
        }

        var crc32Str = Convert.ToHexString(crc32.GetCurrentHash()).ToLowerInvariant();

        if (crc32Str != expectedCrc32)
        {
            throw new InvalidDataException($"CRC32校验失败: {filePath}");
        }
    }

    private async Task ReportProgressAsync(
        int downloadedFiles,
        long downloadedSize,
        UpdateStatus status,
        string errorMessage = null,
        double? downloadSpeed = null)
    {
        var request = new
        {
            log_id = _updateLogId,
            downloaded_files = downloadedFiles,
            downloaded_size = downloadedSize,
            status = (byte)status,
            download_speed = downloadSpeed,
            error_message = errorMessage
        };

        await _httpClient.PostAsJsonAsync("/progress", request);
    }

    private void SaveLocalVersion(string version)
    {
        var versionFilePath = Path.Combine(_config.LocalGamePath, _config.VersionFile);
        File.WriteAllText(versionFilePath, version);
    }
}

public enum UpdateStatus : byte
{
    InProgress = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}
```

### 步骤4：UI绑定

```csharp
public partial class LauncherWindow : Window
{
    private readonly VersionChecker _versionChecker;
    private readonly UpdateManager _updateManager;
    private CancellationTokenSource _updateCts;

    public LauncherWindow()
    {
        InitializeComponent();

        var config = new UpdateConfig
        {
            ApiBaseUrl = "https://your-gateway.com/api/update",
            LocalGamePath = @"D:\Games\Aion"
        };

        _versionChecker = new VersionChecker(config);
        _updateManager = new UpdateManager(config);

        // 订阅进度事件
        _updateManager.ProgressChanged += OnUpdateProgressChanged;

        Loaded += LauncherWindow_Loaded;
    }

    private async void LauncherWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // 检查更新
            var updateInfo = await _versionChecker.CheckForUpdateAsync();

            if (!updateInfo.NeedsUpdate)
            {
                // 无需更新，启动游戏
                StartGame();
                return;
            }

            if (updateInfo.NeedsFullClient)
            {
                // 显示完整包下载界面
                ShowFullClientDownloadPage(updateInfo.FullPackageLinks);
                return;
            }

            // 显示更新确认对话框
            var dialogResult = MessageBox.Show(
                $"发现新版本 {updateInfo.LatestVersion}\n\n" +
                $"更新大小: {updateInfo.DownloadSizeText}\n" +
                $"预计时间: {updateInfo.EstimatedTime / 60}分钟\n\n" +
                $"{updateInfo.Changelog}\n\n" +
                $"是否立即更新？",
                "发现更新",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (dialogResult == MessageBoxResult.Yes)
            {
                // 显示进度界面
                ShowUpdateProgressPage();

                // 执行更新
                _updateCts = new CancellationTokenSource();
                var success = await _updateManager.ExecuteUpdateAsync(updateInfo, _updateCts.Token);

                if (success)
                {
                    MessageBox.Show("更新完成！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    StartGame();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"更新失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUpdateProgressChanged(LauncherUpdateProgress progress)
    {
        // 在UI线程更新进度
        Dispatcher.Invoke(() =>
        {
            TxtStageName.Text = progress.StageName;
            TxtCurrentFile.Text = progress.CurrentFile ?? "";
            ProgressBarOverall.Value = progress.OverallProgress;
            ProgressBarCurrentFile.Value = progress.CurrentFileProgress;
            TxtFilesProgress.Text = $"{progress.CompletedFiles}/{progress.TotalFiles} 文件";
            TxtSizeProgress.Text = $"{FormatBytes(progress.DownloadedBytes)} / {FormatBytes(progress.TotalBytes)}";
            TxtDownloadSpeed.Text = progress.DownloadSpeedText;
            TxtRemainingTime.Text = $"剩余时间: {progress.RemainingTimeText}";
        });
    }

    private void BtnPause_Click(object sender, RoutedEventArgs e)
    {
        // TODO: 实现暂停功能
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        _updateCts?.Cancel();
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double size = bytes;
        int order = 0;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:F2} {sizes[order]}";
    }
}
```

---

## 进度显示

使用 `LauncherUpdateProgress` 类跟踪所有进度信息：

```csharp
public class LauncherUpdateProgress
{
    // 更新阶段
    public UpdateStage Stage { get; set; }
    public string StageName => GetStageName(Stage);

    // 当前文件
    public string CurrentFile { get; set; }
    public int CurrentFileProgress { get; set; } // 0-100

    // 总进度
    public int TotalFiles { get; set; }
    public int CompletedFiles { get; set; }
    public int OverallProgress { get; set; } // 0-100

    // 大小信息
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }

    // 速度信息
    public long DownloadSpeed { get; set; }
    public string DownloadSpeedText => FormatSpeed(DownloadSpeed);

    // 时间信息
    public int RemainingSeconds { get; set; }
    public string RemainingTimeText => FormatTime(RemainingSeconds);

    // P2P信息
    public int P2PRatio { get; set; } // 0-100

    // 控制
    public bool CanCancel { get; set; } = true;
    public bool CanPause { get; set; } = true;
    public bool IsPaused { get; set; }
}

public enum UpdateStage
{
    Preparing,
    CheckingVersion,
    DownloadingManifest,
    ComparingFiles,
    DownloadingFiles,
    ExtractingFiles,
    VerifyingFiles,
    ApplyingPatch,
    CleaningUp,
    Completed,
    Failed
}
```

---

## 错误处理

### 常见错误及处理

```csharp
try
{
    await _updateManager.ExecuteUpdateAsync(updateInfo);
}
catch (HttpRequestException ex)
{
    // 网络错误
    MessageBox.Show(
        "网络连接失败，请检查网络后重试",
        "错误",
        MessageBoxButton.OK,
        MessageBoxImage.Error
    );
}
catch (InvalidDataException ex)
{
    // 文件校验失败
    MessageBox.Show(
        "文件校验失败，请重新下载",
        "错误",
        MessageBoxButton.OK,
        MessageBoxImage.Error
    );
}
catch (OperationCanceledException)
{
    // 用户取消
    MessageBox.Show(
        "更新已取消",
        "提示",
        MessageBoxButton.OK,
        MessageBoxImage.Information
    );
}
catch (Exception ex)
{
    // 未知错误
    MessageBox.Show(
        $"更新失败: {ex.Message}\n\n请联系客服",
        "错误",
        MessageBoxButton.OK,
        MessageBoxImage.Error
    );
}
```

---

## 测试清单

- [ ] **新用户首次安装**
  - [ ] 显示网盘下载链接
  - [ ] 点击链接正确跳转
  - [ ] 提取码和解压密码正确显示
  - [ ] 下载次数正确统计

- [ ] **老用户增量更新**
  - [ ] 正确检测版本差异
  - [ ] 清单下载成功
  - [ ] 文件并发下载（8线程）
  - [ ] 进度实时更新
  - [ ] Hash校验通过
  - [ ] 版本号正确更新

- [ ] **断点续传**
  - [ ] 中途中断后可继续
  - [ ] 已下载的文件不重复下载

- [ ] **错误恢复**
  - [ ] 网络中断自动重试
  - [ ] 校验失败重新下载
  - [ ] 最多重试3次

- [ ] **用户体验**
  - [ ] 进度条流畅更新
  - [ ] 速度显示准确
  - [ ] 剩余时间估算准确
  - [ ] 可暂停/取消

---

## 常见问题

### Q1: 如何处理CDN URL过期？

**A**: CDN URL有效期1小时。如果下载超过1小时，需要重新请求清单获取新的URL。

```csharp
private async Task<string> GetFreshCdnUrlAsync(string filePath, string version)
{
    var manifest = await GetVersionManifestAsync(version, null);
    var file = manifest.Files.FirstOrDefault(f => f.FilePath == filePath);
    return file?.CdnUrl;
}
```

### Q2: 如何优化下载速度？

**A**:
1. 并发下载8个文件
2. 启用P2P分流
3. 使用最近的CDN节点
4. 按优先级下载，核心文件优先

### Q3: 如何减少内存占用？

**A**: 使用流式下载，不要一次性加载整个文件到内存。

```csharp
await using var stream = await response.Content.ReadAsStreamAsync();
await using var fileStream = new FileStream(localPath, FileMode.Create);
await stream.CopyToAsync(fileStream);
```

### Q4: 如何实现暂停功能？

**A**: 使用 `CancellationToken` 和断点续传结合：

```csharp
// 暂停
_updateCts?.Cancel();

// 继续（重新开始，会自动断点续传）
_updateCts = new CancellationTokenSource();
await _updateManager.ExecuteUpdateAsync(updateInfo, _updateCts.Token);
```

---

## 示例项目

完整示例代码请参考：
- C# WPF示例: `examples/launcher-wpf/`
- C# WinForms示例: `examples/launcher-winforms/`

---

## 技术支持

如有问题，请：
1. 查看 [API文档](./UPDATE_API.md)
2. 查看 [README](../README.md)
3. 提交 [Issue](https://github.com/xiaohan1105/AionNetGate/issues)

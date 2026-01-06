using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 文件浏览器 ViewModel
/// </summary>
public partial class FileBrowserViewModel : ViewModelBase
{
    private readonly IBackendCommunicationService _backendService;
    private readonly ILogger<FileBrowserViewModel> _logger;
    private ICollectionView? _filesView;

    [ObservableProperty]
    private string _connectionId = string.Empty;

    [ObservableProperty]
    private string _clientInfo = string.Empty;

    [ObservableProperty]
    private string _currentPath = "C:\\";

    [ObservableProperty]
    private ObservableCollection<Services.FileInfo> _files = new();

    [ObservableProperty]
    private Services.FileInfo? _selectedFile;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalFiles;

    [ObservableProperty]
    private int _totalDirectories;

    [ObservableProperty]
    private string _statusText = "就绪";

    public FileBrowserViewModel(
        IBackendCommunicationService backendService,
        ILogger<FileBrowserViewModel> logger)
    {
        _backendService = backendService;
        _logger = logger;
    }

    public void Initialize(string connectionId, string clientInfo)
    {
        ConnectionId = connectionId;
        ClientInfo = clientInfo;
        _logger.LogInformation("初始化文件浏览器视图: {ConnectionId}", connectionId);

        // 初始化集合视图
        _filesView = CollectionViewSource.GetDefaultView(Files);
        _filesView.Filter = FilterFiles;

        // 加载初始目录
        _ = LoadFilesAsync();
    }

    [RelayCommand]
    private async Task LoadFilesAsync()
    {
        if (IsLoading || string.IsNullOrEmpty(ConnectionId))
            return;

        try
        {
            IsLoading = true;
            StatusText = $"正在加载 {CurrentPath}...";

            _logger.LogDebug("请求文件列表: {ConnectionId}, Path: {Path}", ConnectionId, CurrentPath);

            var files = await _backendService.GetClientFilesAsync(ConnectionId, CurrentPath);

            Files.Clear();
            foreach (var file in files)
            {
                Files.Add(file);
            }

            TotalFiles = Files.Count(f => !f.IsDirectory);
            TotalDirectories = Files.Count(f => f.IsDirectory);

            StatusText = $"{TotalDirectories} 个文件夹, {TotalFiles} 个文件";

            _logger.LogInformation("文件列表已加载: {Directories} 个文件夹, {Files} 个文件",
                TotalDirectories, TotalFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载文件列表失败");
            StatusText = $"错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateAsync()
    {
        if (SelectedFile == null || !SelectedFile.IsDirectory)
            return;

        if (SelectedFile.Name == "..")
        {
            // 返回上一级
            var parent = System.IO.Path.GetDirectoryName(CurrentPath);
            if (!string.IsNullOrEmpty(parent))
            {
                CurrentPath = parent;
            }
        }
        else
        {
            CurrentPath = SelectedFile.FullPath;
        }

        await LoadFilesAsync();
    }

    [RelayCommand]
    private async Task DownloadFileAsync()
    {
        if (SelectedFile == null || SelectedFile.IsDirectory)
            return;

        try
        {
            StatusText = $"正在下载 {SelectedFile.Name}...";

            var fileData = await _backendService.DownloadFileAsync(ConnectionId, SelectedFile.FullPath);

            if (fileData != null)
            {
                // 让用户选择保存位置
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = SelectedFile.Name,
                    Filter = "所有文件 (*.*)|*.*"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllBytesAsync(saveDialog.FileName, fileData);
                    StatusText = $"文件已保存到: {saveDialog.FileName}";

                    _logger.LogInformation("文件已下载: {FileName}", saveDialog.FileName);
                }
                else
                {
                    StatusText = "下载已取消";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载文件失败");
            StatusText = $"错误: {ex.Message}";
            System.Windows.MessageBox.Show($"下载文件失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteFileAsync()
    {
        if (SelectedFile == null || SelectedFile.Name == "..")
            return;

        try
        {
            var fileType = SelectedFile.IsDirectory ? "文件夹" : "文件";
            var result = System.Windows.MessageBox.Show(
                $"确定要删除{fileType} \"{SelectedFile.Name}\" 吗？\n\n此操作无法撤销！",
                "确认删除",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            StatusText = $"正在删除 {SelectedFile.Name}...";

            await _backendService.DeleteFileAsync(ConnectionId, SelectedFile.FullPath);

            _logger.LogInformation("文件已删除: {FileName}", SelectedFile.FullPath);

            // 刷新列表
            await LoadFilesAsync();

            StatusText = $"{fileType}已删除";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除文件失败");
            StatusText = $"错误: {ex.Message}";
            System.Windows.MessageBox.Show($"删除失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task GoToPathAsync(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        CurrentPath = path;
        await LoadFilesAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _filesView?.Refresh();
    }

    private bool FilterFiles(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (obj is not Services.FileInfo file)
            return false;

        return file.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    public void Cleanup()
    {
        _logger.LogInformation("文件浏览器视图已清理");
    }
}

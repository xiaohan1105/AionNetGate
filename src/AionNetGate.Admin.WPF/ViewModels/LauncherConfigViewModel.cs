using System.IO;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 登录器配置页面 ViewModel
/// </summary>
public partial class LauncherConfigViewModel : ViewModelBase
{
    private readonly ILogger<LauncherConfigViewModel> _logger;
    private readonly IConfigurationService _configService;
    private const string ConfigName = "launcher";

    // 基本信息
    [ObservableProperty]
    private string _launcherName = "Aion Online";

    [ObservableProperty]
    private string _bin32Path = "bin32/aion.bin";

    [ObservableProperty]
    private string _bin64Path = "bin64/aion.bin";

    [ObservableProperty]
    private string _launchArgs = "-cc:5 -lang:chs";

    [ObservableProperty]
    private int _lsPort = 2106;

    [ObservableProperty]
    private string _webUrl = string.Empty;

    [ObservableProperty]
    private bool _allowMultipleInstances;

    // 补丁配置
    [ObservableProperty]
    private string _patchUrl = string.Empty;

    [ObservableProperty]
    private string _updateUrl = string.Empty;

    [ObservableProperty]
    private string _launcherMd5 = string.Empty;

    // 安全配置
    [ObservableProperty]
    private string _clientFilesRestriction = string.Empty;

    [ObservableProperty]
    private string _fileMd5List = string.Empty;

    [ObservableProperty]
    private string _cheatProcessList = string.Empty;

    [ObservableProperty]
    private bool _autoCloseOnCheatDetected = true;

    // 皮肤配置
    [ObservableProperty]
    private string _skinPath = string.Empty;

    [ObservableProperty]
    private string _outputDirectory = string.Empty;

    [ObservableProperty]
    private string _outputFileName = "AionLauncher";

    // 构建状态
    [ObservableProperty]
    private bool _isBuilding;

    [ObservableProperty]
    private int _buildProgress;

    [ObservableProperty]
    private string _buildStatusText = "就绪";

    public LauncherConfigViewModel(
        ILogger<LauncherConfigViewModel> logger,
        IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        LoadConfig();
        _logger.LogInformation("LauncherConfigViewModel 已初始化");
    }

    private void LoadConfig()
    {
        var config = _configService.LoadConfig<LauncherConfigData>(ConfigName);
        if (config != null)
        {
            LauncherName = config.LauncherName;
            Bin32Path = config.Bin32Path;
            Bin64Path = config.Bin64Path;
            LaunchArgs = config.LaunchArgs;
            LsPort = config.LsPort;
            WebUrl = config.WebUrl;
            AllowMultipleInstances = config.AllowMultipleInstances;
            PatchUrl = config.PatchUrl;
            UpdateUrl = config.UpdateUrl;
            LauncherMd5 = config.LauncherMd5;
            ClientFilesRestriction = config.ClientFilesRestriction;
            FileMd5List = config.FileMd5List;
            CheatProcessList = config.CheatProcessList;
            AutoCloseOnCheatDetected = config.AutoCloseOnCheatDetected;
            SkinPath = config.SkinPath;
            OutputDirectory = config.OutputDirectory;
            OutputFileName = config.OutputFileName;
            BuildStatusText = "配置已加载";
            _logger.LogInformation("启动器配置已从文件加载");
        }
    }

    [RelayCommand]
    private void SelectSkinPath()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择皮肤目录"
        };

        if (dialog.ShowDialog() == true)
        {
            SkinPath = dialog.FolderName;
        }
    }

    [RelayCommand]
    private void SelectOutputDirectory()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择输出目录"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = dialog.FolderName;
        }
    }

    [RelayCommand]
    private async Task BuildLauncherAsync()
    {
        if (IsBuilding) return;

        try
        {
            IsBuilding = true;
            BuildProgress = 0;
            BuildStatusText = "正在构建登录器...";

            // 模拟构建过程
            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(200);
                BuildProgress = i;
            }

            BuildStatusText = "构建完成";
            _logger.LogInformation("登录器构建完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "构建登录器失败");
            BuildStatusText = "构建失败: " + ex.Message;
        }
        finally
        {
            IsBuilding = false;
        }
    }

    [RelayCommand]
    private void GenerateMd5()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择要计算 MD5 的文件",
            Filter = "所有文件|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                using var stream = File.OpenRead(dialog.FileName);
                var hash = md5.ComputeHash(stream);
                LauncherMd5 = BitConverter.ToString(hash).Replace("-", "").ToLower();
                _logger.LogInformation("已计算 MD5: {Md5}", LauncherMd5);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算 MD5 失败");
            }
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        LauncherName = "Aion Online";
        Bin32Path = "bin32/aion.bin";
        Bin64Path = "bin64/aion.bin";
        LaunchArgs = "-cc:5 -lang:chs";
        LsPort = 2106;
        WebUrl = string.Empty;
        AllowMultipleInstances = false;
        PatchUrl = string.Empty;
        UpdateUrl = string.Empty;
        LauncherMd5 = string.Empty;
        ClientFilesRestriction = string.Empty;
        FileMd5List = string.Empty;
        CheatProcessList = string.Empty;
        AutoCloseOnCheatDetected = true;
        SkinPath = string.Empty;
        OutputDirectory = string.Empty;
        OutputFileName = "AionLauncher";

        BuildStatusText = "已恢复默认配置";
        _logger.LogInformation("已恢复默认配置");
    }

    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            var config = new LauncherConfigData
            {
                LauncherName = LauncherName,
                Bin32Path = Bin32Path,
                Bin64Path = Bin64Path,
                LaunchArgs = LaunchArgs,
                LsPort = LsPort,
                WebUrl = WebUrl,
                AllowMultipleInstances = AllowMultipleInstances,
                PatchUrl = PatchUrl,
                UpdateUrl = UpdateUrl,
                LauncherMd5 = LauncherMd5,
                ClientFilesRestriction = ClientFilesRestriction,
                FileMd5List = FileMd5List,
                CheatProcessList = CheatProcessList,
                AutoCloseOnCheatDetected = AutoCloseOnCheatDetected,
                SkinPath = SkinPath,
                OutputDirectory = OutputDirectory,
                OutputFileName = OutputFileName
            };

            _configService.SaveConfig(ConfigName, config);
            BuildStatusText = "配置已保存";
            _logger.LogInformation("启动器配置已保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败");
            BuildStatusText = "保存失败: " + ex.Message;
        }
    }
}

/// <summary>
/// 启动器配置数据模型
/// </summary>
public class LauncherConfigData
{
    public string LauncherName { get; set; } = "Aion Online";
    public string Bin32Path { get; set; } = "bin32/aion.bin";
    public string Bin64Path { get; set; } = "bin64/aion.bin";
    public string LaunchArgs { get; set; } = "-cc:5 -lang:chs";
    public int LsPort { get; set; } = 2106;
    public string WebUrl { get; set; } = string.Empty;
    public bool AllowMultipleInstances { get; set; }
    public string PatchUrl { get; set; } = string.Empty;
    public string UpdateUrl { get; set; } = string.Empty;
    public string LauncherMd5 { get; set; } = string.Empty;
    public string ClientFilesRestriction { get; set; } = string.Empty;
    public string FileMd5List { get; set; } = string.Empty;
    public string CheatProcessList { get; set; } = string.Empty;
    public bool AutoCloseOnCheatDetected { get; set; } = true;
    public string SkinPath { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public string OutputFileName { get; set; } = "AionLauncher";
}

using System.IO;
using System.Windows;
using AionNetGate.Modules.LauncherBuilder.Models;
using AionNetGate.Modules.LauncherBuilder.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 启动器生成器 ViewModel
/// </summary>
public partial class LauncherGeneratorViewModel : ObservableObject
{
    private readonly ILogger<LauncherGeneratorViewModel> _logger;
    private readonly ILauncherBuilderService _launcherBuilder;

    [ObservableProperty]
    private string _channelCode = "default";

    [ObservableProperty]
    private string _gameTitle = "Aion Online";

    [ObservableProperty]
    private string _serverName = "官方服务器";

    [ObservableProperty]
    private string _version = "2.0.0";

    [ObservableProperty]
    private string _gatewayHost = "127.0.0.1";

    [ObservableProperty]
    private int _gatewayPort = 10001;

    [ObservableProperty]
    private string _gameExecutablePath = "bin64/aion.bin";

    [ObservableProperty]
    private string _gameCommandLineArgs = "-cc:5 -lang:chs";

    [ObservableProperty]
    private int _lsPort = 2106;

    [ObservableProperty]
    private string _skinPath = "";

    [ObservableProperty]
    private string _outputDirectory = "";

    [ObservableProperty]
    private string _outputFileName = "AionLauncher";

    [ObservableProperty]
    private bool _isBuilding;

    [ObservableProperty]
    private double _buildProgress;

    [ObservableProperty]
    private string _buildStatusText = "就绪";

    public LauncherGeneratorViewModel(
        ILogger<LauncherGeneratorViewModel> logger,
        ILauncherBuilderService launcherBuilder)
    {
        _logger = logger;
        _launcherBuilder = launcherBuilder;

        // 设置默认输出目录
        OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Launchers");
    }

    [RelayCommand]
    private void SelectSkinPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择皮肤目录（选择皮肤目录中的任意文件）",
            Filter = "所有文件|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            SkinPath = Path.GetDirectoryName(dialog.FileName) ?? "";
        }
    }

    [RelayCommand]
    private void SelectOutputDirectory()
    {
        var dialog = new SaveFileDialog
        {
            Title = "选择输出目录和文件名",
            Filter = "可执行文件|*.exe",
            FileName = $"{OutputFileName}.exe"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = Path.GetDirectoryName(dialog.FileName) ?? "";
            OutputFileName = Path.GetFileNameWithoutExtension(dialog.FileName);
        }
    }

    [RelayCommand]
    private async Task BuildLauncherAsync()
    {
        try
        {
            IsBuilding = true;
            BuildProgress = 0;
            BuildStatusText = "正在准备构建...";

            var config = new LauncherBuildConfig
            {
                ChannelCode = ChannelCode,
                GameTitle = GameTitle,
                ServerName = ServerName,
                Version = Version,
                GatewayHost = GatewayHost,
                GatewayPort = GatewayPort,
                GameExecutablePath = GameExecutablePath,
                GameCommandLineArgs = GameCommandLineArgs,
                LsPort = LsPort,
                SkinPath = SkinPath,
                OutputDirectory = OutputDirectory,
                OutputFileName = OutputFileName
            };

            // 验证配置
            var validationResult = _launcherBuilder.ValidateBuildConfig(config);
            if (!validationResult.IsSuccess)
            {
                MessageBox.Show(validationResult.Error.Message, "配置错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 构建启动器
            var progress = new Progress<BuildProgress>(p =>
            {
                BuildProgress = p.ProgressPercent;
                BuildStatusText = p.Message;
            });

            var result = await _launcherBuilder.BuildLauncherAsync(config, progress);

            if (result.IsSuccess)
            {
                BuildStatusText = "构建完成";
                MessageBox.Show($"启动器构建成功！\n\n输出路径：{result.Value}", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _logger.LogInformation("启动器构建成功: {OutputPath}", result.Value);
            }
            else
            {
                BuildStatusText = "构建失败";
                MessageBox.Show($"启动器构建失败：{result.Error.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                _logger.LogError("启动器构建失败: {Error}", result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "构建启动器时发生异常");
            BuildStatusText = "构建异常";
            MessageBox.Show($"构建时发生异常：{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBuilding = false;
        }
    }
}

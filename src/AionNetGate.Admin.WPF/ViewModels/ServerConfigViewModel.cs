using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 服务器配置页面 ViewModel
/// </summary>
public partial class ServerConfigViewModel : ViewModelBase
{
    private readonly ILogger<ServerConfigViewModel> _logger;
    private readonly IConfigurationService _configService;
    private const string ConfigName = "server";

    // 网关配置
    [ObservableProperty]
    private string _gatewayIp = "127.0.0.1";

    [ObservableProperty]
    private bool _enableDualIp;

    [ObservableProperty]
    private string _gatewayIp2 = string.Empty;

    [ObservableProperty]
    private int _gatewayPort = 10001;

    [ObservableProperty]
    private int _downloadPort = 10002;

    [ObservableProperty]
    private string _forwardPassword = string.Empty;

    // 自动重启设置
    [ObservableProperty]
    private bool _enableGatewayAutoRestart;

    [ObservableProperty]
    private int _gatewayRestartInterval = 60;

    [ObservableProperty]
    private bool _enableForwarderAutoRestart;

    [ObservableProperty]
    private int _forwarderRestartInterval = 60;

    // 安全设置
    [ObservableProperty]
    private bool _autoBanOnAttack = true;

    [ObservableProperty]
    private bool _enableCommunicationLog;

    [ObservableProperty]
    private bool _disableEarlyLogin;

    [ObservableProperty]
    private bool _disablePasswordRecovery;

    // 远程桌面设置
    [ObservableProperty]
    private int _imageCompressRate = 50;

    [ObservableProperty]
    private int _imageBlockWidth = 800;

    [ObservableProperty]
    private int _imageBlockHeight = 600;

    [ObservableProperty]
    private string _statusText = "就绪";

    public ServerConfigViewModel(
        ILogger<ServerConfigViewModel> logger,
        IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        LoadConfig();
        _logger.LogInformation("ServerConfigViewModel 已初始化");
    }

    private void LoadConfig()
    {
        var config = _configService.LoadConfig<ServerConfigData>(ConfigName);
        if (config != null)
        {
            GatewayIp = config.GatewayIp;
            EnableDualIp = config.EnableDualIp;
            GatewayIp2 = config.GatewayIp2;
            GatewayPort = config.GatewayPort;
            DownloadPort = config.DownloadPort;
            ForwardPassword = config.ForwardPassword;
            EnableGatewayAutoRestart = config.EnableGatewayAutoRestart;
            GatewayRestartInterval = config.GatewayRestartInterval;
            EnableForwarderAutoRestart = config.EnableForwarderAutoRestart;
            ForwarderRestartInterval = config.ForwarderRestartInterval;
            AutoBanOnAttack = config.AutoBanOnAttack;
            EnableCommunicationLog = config.EnableCommunicationLog;
            DisableEarlyLogin = config.DisableEarlyLogin;
            DisablePasswordRecovery = config.DisablePasswordRecovery;
            ImageCompressRate = config.ImageCompressRate;
            ImageBlockWidth = config.ImageBlockWidth;
            ImageBlockHeight = config.ImageBlockHeight;
            StatusText = "配置已加载";
            _logger.LogInformation("服务器配置已从文件加载");
        }
    }

    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            var config = new ServerConfigData
            {
                GatewayIp = GatewayIp,
                EnableDualIp = EnableDualIp,
                GatewayIp2 = GatewayIp2,
                GatewayPort = GatewayPort,
                DownloadPort = DownloadPort,
                ForwardPassword = ForwardPassword,
                EnableGatewayAutoRestart = EnableGatewayAutoRestart,
                GatewayRestartInterval = GatewayRestartInterval,
                EnableForwarderAutoRestart = EnableForwarderAutoRestart,
                ForwarderRestartInterval = ForwarderRestartInterval,
                AutoBanOnAttack = AutoBanOnAttack,
                EnableCommunicationLog = EnableCommunicationLog,
                DisableEarlyLogin = DisableEarlyLogin,
                DisablePasswordRecovery = DisablePasswordRecovery,
                ImageCompressRate = ImageCompressRate,
                ImageBlockWidth = ImageBlockWidth,
                ImageBlockHeight = ImageBlockHeight
            };

            _configService.SaveConfig(ConfigName, config);
            StatusText = "配置已保存";
            _logger.LogInformation("服务器配置已保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败");
            StatusText = "保存失败: " + ex.Message;
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        GatewayIp = "127.0.0.1";
        EnableDualIp = false;
        GatewayIp2 = string.Empty;
        GatewayPort = 10001;
        DownloadPort = 10002;
        ForwardPassword = string.Empty;
        EnableGatewayAutoRestart = false;
        GatewayRestartInterval = 60;
        EnableForwarderAutoRestart = false;
        ForwarderRestartInterval = 60;
        AutoBanOnAttack = true;
        EnableCommunicationLog = false;
        DisableEarlyLogin = false;
        DisablePasswordRecovery = false;
        ImageCompressRate = 50;
        ImageBlockWidth = 800;
        ImageBlockHeight = 600;

        StatusText = "已恢复默认配置";
        _logger.LogInformation("已恢复默认配置");
    }
}

/// <summary>
/// 服务器配置数据模型
/// </summary>
public class ServerConfigData
{
    public string GatewayIp { get; set; } = "127.0.0.1";
    public bool EnableDualIp { get; set; }
    public string GatewayIp2 { get; set; } = string.Empty;
    public int GatewayPort { get; set; } = 10001;
    public int DownloadPort { get; set; } = 10002;
    public string ForwardPassword { get; set; } = string.Empty;
    public bool EnableGatewayAutoRestart { get; set; }
    public int GatewayRestartInterval { get; set; } = 60;
    public bool EnableForwarderAutoRestart { get; set; }
    public int ForwarderRestartInterval { get; set; } = 60;
    public bool AutoBanOnAttack { get; set; } = true;
    public bool EnableCommunicationLog { get; set; }
    public bool DisableEarlyLogin { get; set; }
    public bool DisablePasswordRecovery { get; set; }
    public int ImageCompressRate { get; set; } = 50;
    public int ImageBlockWidth { get; set; } = 800;
    public int ImageBlockHeight { get; set; } = 600;
}

using System.Net;
using System.Net.Mail;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 邮件通知配置页面 ViewModel
/// </summary>
public partial class EmailConfigViewModel : ViewModelBase
{
    private readonly ILogger<EmailConfigViewModel> _logger;
    private readonly IConfigurationService _configService;
    private const string ConfigName = "email";

    [ObservableProperty]
    private string _senderEmail = string.Empty;

    [ObservableProperty]
    private string _emailPassword = string.Empty;

    [ObservableProperty]
    private string _smtpServer = "smtp.qq.com";

    [ObservableProperty]
    private int _smtpPort = 587;

    [ObservableProperty]
    private bool _enableSsl = true;

    [ObservableProperty]
    private string _testRecipient = string.Empty;

    [ObservableProperty]
    private bool _enableNotifications;

    [ObservableProperty]
    private bool _notifyOnServerDown = true;

    [ObservableProperty]
    private bool _notifyOnAttack = true;

    [ObservableProperty]
    private bool _notifyOnNewUser;

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private bool _sendSuccess;

    public EmailConfigViewModel(
        ILogger<EmailConfigViewModel> logger,
        IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        LoadConfig();
        _logger.LogInformation("EmailConfigViewModel 已初始化");
    }

    private void LoadConfig()
    {
        var config = _configService.LoadConfig<EmailConfigData>(ConfigName);
        if (config != null)
        {
            SenderEmail = config.SenderEmail;
            EmailPassword = config.EmailPassword;
            SmtpServer = config.SmtpServer;
            SmtpPort = config.SmtpPort;
            EnableSsl = config.EnableSsl;
            TestRecipient = config.TestRecipient;
            EnableNotifications = config.EnableNotifications;
            NotifyOnServerDown = config.NotifyOnServerDown;
            NotifyOnAttack = config.NotifyOnAttack;
            NotifyOnNewUser = config.NotifyOnNewUser;
            StatusText = "配置已加载";
            _logger.LogInformation("邮件配置已从文件加载");
        }
    }

    [RelayCommand]
    private async Task SendTestEmailAsync()
    {
        if (IsSending) return;

        if (string.IsNullOrWhiteSpace(SenderEmail) ||
            string.IsNullOrWhiteSpace(EmailPassword) ||
            string.IsNullOrWhiteSpace(TestRecipient))
        {
            StatusText = "请填写完整的邮箱配置和测试收件人";
            return;
        }

        try
        {
            IsSending = true;
            SendSuccess = false;
            StatusText = "正在发送测试邮件...";

            using var client = new SmtpClient(SmtpServer, SmtpPort)
            {
                Credentials = new NetworkCredential(SenderEmail, EmailPassword),
                EnableSsl = EnableSsl,
                Timeout = 30000
            };

            var mailMessage = new MailMessage(
                SenderEmail,
                TestRecipient,
                "AionNetGate 测试邮件",
                $"这是一封来自 AionNetGate 网关管理系统的测试邮件。\n\n发送时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n如果您收到此邮件，说明邮件配置正确。");

            await client.SendMailAsync(mailMessage);

            SendSuccess = true;
            StatusText = "测试邮件发送成功！";
            _logger.LogInformation("测试邮件已发送至: {Recipient}", TestRecipient);
        }
        catch (SmtpException ex)
        {
            SendSuccess = false;
            StatusText = $"SMTP错误: {ex.Message}";
            _logger.LogError(ex, "SMTP发送失败");
        }
        catch (Exception ex)
        {
            SendSuccess = false;
            StatusText = "发送失败: " + ex.Message;
            _logger.LogError(ex, "测试邮件发送失败");
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            var config = new EmailConfigData
            {
                SenderEmail = SenderEmail,
                EmailPassword = EmailPassword,
                SmtpServer = SmtpServer,
                SmtpPort = SmtpPort,
                EnableSsl = EnableSsl,
                TestRecipient = TestRecipient,
                EnableNotifications = EnableNotifications,
                NotifyOnServerDown = NotifyOnServerDown,
                NotifyOnAttack = NotifyOnAttack,
                NotifyOnNewUser = NotifyOnNewUser
            };

            _configService.SaveConfig(ConfigName, config);
            StatusText = "配置已保存";
            _logger.LogInformation("邮件配置已保存");
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
        SenderEmail = string.Empty;
        EmailPassword = string.Empty;
        SmtpServer = "smtp.qq.com";
        SmtpPort = 587;
        EnableSsl = true;
        TestRecipient = string.Empty;
        EnableNotifications = false;
        NotifyOnServerDown = true;
        NotifyOnAttack = true;
        NotifyOnNewUser = false;
        StatusText = "已恢复默认配置";
        _logger.LogInformation("已恢复默认邮件配置");
    }
}

/// <summary>
/// 邮件配置数据模型
/// </summary>
public class EmailConfigData
{
    public string SenderEmail { get; set; } = string.Empty;
    public string EmailPassword { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = "smtp.qq.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string TestRecipient { get; set; } = string.Empty;
    public bool EnableNotifications { get; set; }
    public bool NotifyOnServerDown { get; set; } = true;
    public bool NotifyOnAttack { get; set; } = true;
    public bool NotifyOnNewUser { get; set; }
}

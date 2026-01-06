using System.Net;
using System.Net.Mail;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Results;
using AionNetGate.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.Infrastructure.Services;

/// <summary>
/// 邮件服务实现
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailConfig _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailConfig> config, ILogger<EmailService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public bool IsEnabled => _config.Enabled && !string.IsNullOrEmpty(_config.SenderEmail);

    public async Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Result.Failure(Error.ServiceUnavailable("邮件服务未启用"));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            return Result.Failure(Error.Validation("收件人地址不能为空"));
        }

        try
        {
            using var client = CreateSmtpClient();
            using var message = new MailMessage
            {
                From = new MailAddress(_config.SenderEmail, _config.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(to);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("邮件发送成功: To={To}, Subject={Subject}", to, subject);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件发送失败: To={To}, Subject={Subject}", to, subject);
            return Result.Failure(Error.Internal($"邮件发送失败: {ex.Message}"));
        }
    }

    public async Task<Result> SendPasswordResetAsync(string to, string username, string newPassword, string gameName, CancellationToken cancellationToken = default)
    {
        if (!_config.AllowPasswordRecovery)
        {
            return Result.Failure(Error.Forbidden("密码找回功能已禁用"));
        }

        var subject = _config.PasswordResetSubject
            .Replace("{GameName}", gameName)
            .Replace("{Username}", username);

        var body = _config.PasswordResetTemplate
            .Replace("{GameName}", gameName)
            .Replace("{Username}", username)
            .Replace("{NewPassword}", newPassword)
            .Replace("{DateTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        return await SendAsync(to, subject, body, false, cancellationToken);
    }

    public async Task<Result> SendRegistrationConfirmationAsync(string to, string username, string gameName, CancellationToken cancellationToken = default)
    {
        var subject = $"[{gameName}] 账号注册成功";
        var body = $@"
亲爱的玩家 {username}：

恭喜您成功注册 {gameName} 账号！

您的用户名为：{username}

请妥善保管您的账号信息，祝您游戏愉快！

{gameName} 运营团队
{DateTime.Now:yyyy-MM-dd HH:mm:ss}
";

        return await SendAsync(to, subject, body, false, cancellationToken);
    }

    public async Task<Result> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return Result.Failure(Error.ServiceUnavailable("邮件服务未启用"));
        }

        try
        {
            using var client = CreateSmtpClient();

            // 尝试连接SMTP服务器
            // SmtpClient 没有直接的连接测试方法，我们通过发送测试邮件来验证
            using var message = new MailMessage
            {
                From = new MailAddress(_config.SenderEmail, _config.SenderName),
                Subject = "[AionNetGate] 邮件服务测试",
                Body = $"这是一封测试邮件，发送时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                IsBodyHtml = false
            };
            message.To.Add(_config.SenderEmail);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("邮件服务测试成功");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件服务测试失败");
            return Result.Failure(Error.Internal($"连接测试失败: {ex.Message}"));
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
        {
            EnableSsl = _config.EnableSsl,
            Timeout = _config.Timeout * 1000,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrEmpty(_config.Username))
        {
            client.Credentials = new NetworkCredential(_config.Username, _config.Password);
        }

        return client;
    }
}

namespace AionNetGate.Core.Configuration;

/// <summary>
/// 邮件配置
/// </summary>
public class EmailConfig
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// 是否启用邮件服务
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// SMTP 服务器地址
    /// </summary>
    public string SmtpServer { get; set; } = "smtp.163.com";

    /// <summary>
    /// SMTP 端口
    /// </summary>
    public int SmtpPort { get; set; } = 25;

    /// <summary>
    /// 发送邮箱地址
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// 发送者显示名称
    /// </summary>
    public string SenderName { get; set; } = "AionNetGate";

    /// <summary>
    /// 邮箱账号（通常与发送邮箱相同）
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱密码/授权码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 是否使用 SSL/TLS 连接
    /// </summary>
    public bool EnableSsl { get; set; } = false;

    /// <summary>
    /// 邮件发送超时时间（秒）
    /// </summary>
    public int Timeout { get; set; } = 30;

    /// <summary>
    /// 是否允许密码找回功能
    /// </summary>
    public bool AllowPasswordRecovery { get; set; } = true;

    /// <summary>
    /// 密码重置邮件主题模板
    /// </summary>
    public string PasswordResetSubject { get; set; } = "[{GameName}] 密码重置通知";

    /// <summary>
    /// 密码重置邮件内容模板
    /// </summary>
    public string PasswordResetTemplate { get; set; } = @"
亲爱的玩家 {Username}：

您好！您正在进行密码重置操作。

您的新密码为：{NewPassword}

请登录后及时修改密码，以确保账号安全。

如果这不是您本人的操作，请忽略此邮件或联系客服。

祝您游戏愉快！

{GameName} 运营团队
{DateTime}
";
}

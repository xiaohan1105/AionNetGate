using AionNetGate.Core.Results;

namespace AionNetGate.Core.Services;

/// <summary>
/// 邮件服务接口
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人地址</param>
    /// <param name="subject">主题</param>
    /// <param name="body">正文内容</param>
    /// <param name="isHtml">是否为HTML格式</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送密码重置邮件
    /// </summary>
    /// <param name="to">收件人地址</param>
    /// <param name="username">用户名</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="gameName">游戏名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> SendPasswordResetAsync(string to, string username, string newPassword, string gameName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送账号注册确认邮件
    /// </summary>
    /// <param name="to">收件人地址</param>
    /// <param name="username">用户名</param>
    /// <param name="gameName">游戏名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> SendRegistrationConfirmationAsync(string to, string username, string gameName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 测试邮件连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<Result> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查邮件服务是否可用
    /// </summary>
    bool IsEnabled { get; }
}

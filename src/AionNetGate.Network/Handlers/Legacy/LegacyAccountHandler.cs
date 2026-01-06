using AionNetGate.Application.Services;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using Microsoft.Extensions.Logging;

namespace AionNetGate.Network.Handlers.Legacy;

/// <summary>
/// 账号操作类型 - 兼容老协议
/// </summary>
public enum LegacyAccountOperation : byte
{
    /// <summary>注册</summary>
    Register = 0,
    /// <summary>登录</summary>
    Login = 1,
    /// <summary>修改密码</summary>
    ChangePassword = 2,
    /// <summary>找回密码</summary>
    ResetPassword = 3
}

/// <summary>
/// 账号处理器 - 兼容老协议
/// 对应老项目 CM_ACCOUNT
/// </summary>
/// <param name="accountService">账号服务</param>
/// <param name="logger">日志记录器</param>
public class LegacyAccountHandler(
    IAccountService accountService,
    ILogger<LegacyAccountHandler> logger) : ILegacyPacketHandler
{
    public byte Opcode => Opcodes.CM_ACCOUNT;

    public async ValueTask HandleAsync(ClientSession session, ReadOnlyMemory<byte> payload)
    {
        try
        {
            var reader = new PacketReader(payload.Span);

            // 老协议格式: [Operation:byte] [Data:...]
            var operation = (LegacyAccountOperation)reader.ReadByte();

            logger.LogDebug(
                "收到账号请求: SessionId={SessionId}, Operation={Operation}",
                session.SessionId, operation);

            // 提取剩余数据供子方法使用
            var remainingData = payload.Slice(1); // 跳过已读的 operation 字节

            switch (operation)
            {
                case LegacyAccountOperation.Register:
                    await HandleRegisterAsync(session, remainingData);
                    break;
                case LegacyAccountOperation.Login:
                    await HandleLoginAsync(session, remainingData);
                    break;
                case LegacyAccountOperation.ChangePassword:
                    await HandleChangePasswordAsync(session, remainingData);
                    break;
                case LegacyAccountOperation.ResetPassword:
                    await HandleResetPasswordAsync(session, remainingData);
                    break;
                default:
                    await SendAccountResponseAsync(session, operation, false, "未知的操作类型");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理账号请求失败: SessionId={SessionId}", session.SessionId);
            await SendAccountResponseAsync(session, 0, false, "服务器内部错误");
        }
    }

    /// <summary>
    /// 处理注册请求
    /// </summary>
    private async ValueTask HandleRegisterAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        var reader = new PacketReader(data.Span);

        // 注册格式: [Username:string] [Password:string] [Email:string]
        var username = reader.ReadString();
        var password = reader.ReadString();
        var email = reader.ReadString();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.Register, false, "用户名或密码不能为空");
            return;
        }

        var result = await accountService.RegisterAsync(username, password, email, CancellationToken.None);

        if (!result.IsSuccess)
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.Register, false, result.Error.Message);
            return;
        }

        logger.LogInformation("账号注册成功: {Username}, SessionId={SessionId}", username, session.SessionId);

        await session.SendPacketAsync(Opcodes.SM_ACCOUNT, writer =>
        {
            writer.WriteByte((byte)LegacyAccountOperation.Register);
            writer.WriteBoolean(true); // 成功
            writer.WriteString("注册成功");
            writer.WriteInt64(result.Value!.Id); // 账号 ID
            writer.WriteString(username);
        });
    }

    /// <summary>
    /// 处理登录请求
    /// </summary>
    private async ValueTask HandleLoginAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        var reader = new PacketReader(data.Span);

        // 登录格式: [Username:string] [Password:string]
        var username = reader.ReadString();
        var password = reader.ReadString();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.Login, false, "用户名或密码不能为空");
            return;
        }

        var result = await accountService.LoginAsync(username, password, session.ClientIp, CancellationToken.None);

        if (!result.IsSuccess)
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.Login, false, result.Error.Message);
            return;
        }

        var (account, token, refreshToken) = result.Value;

        // 设置会话信息
        session.AccountId = account.Id;

        logger.LogInformation(
            "账号登录成功: {Username}, SessionId={SessionId}, IP={IP}",
            username, session.SessionId, session.ClientIp);

        await session.SendPacketAsync(Opcodes.SM_ACCOUNT, writer =>
        {
            writer.WriteByte((byte)LegacyAccountOperation.Login);
            writer.WriteBoolean(true); // 成功
            writer.WriteString("登录成功");
            writer.WriteInt64(account.Id); // 账号 ID
            writer.WriteString(username);
            writer.WriteString(token); // JWT Token
            writer.WriteString(refreshToken); // Refresh Token
            writer.WriteByte((byte)account.Role); // 角色
        });
    }

    /// <summary>
    /// 处理修改密码请求
    /// </summary>
    private async ValueTask HandleChangePasswordAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        var reader = new PacketReader(data.Span);

        // 修改密码格式: [OldPassword:string] [NewPassword:string]
        var oldPassword = reader.ReadString();
        var newPassword = reader.ReadString();

        if (!session.AccountId.HasValue)
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.ChangePassword, false, "请先登录");
            return;
        }

        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.ChangePassword, false, "密码不能为空");
            return;
        }

        var result = await accountService.ChangePasswordAsync(session.AccountId.Value, oldPassword, newPassword, CancellationToken.None);

        if (!result.IsSuccess)
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.ChangePassword, false, result.Error.Message);
            return;
        }

        logger.LogInformation("密码修改成功: AccountId={AccountId}", session.AccountId);

        await SendAccountResponseAsync(session, LegacyAccountOperation.ChangePassword, true, "密码修改成功");
    }

    /// <summary>
    /// 处理找回密码请求
    /// </summary>
    private async ValueTask HandleResetPasswordAsync(ClientSession session, ReadOnlyMemory<byte> data)
    {
        var reader = new PacketReader(data.Span);

        // 找回密码格式: [Username:string] [Email:string] [NewPassword:string]
        var username = reader.ReadString();
        var email = reader.ReadString();
        var newPassword = reader.ReadString();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.ResetPassword, false, "信息不完整");
            return;
        }

        var result = await accountService.ResetPasswordAsync(username, email, newPassword, CancellationToken.None);

        if (!result.IsSuccess)
        {
            await SendAccountResponseAsync(session, LegacyAccountOperation.ResetPassword, false, result.Error.Message);
            return;
        }

        logger.LogInformation("密码重置成功: {Username}", username);

        await SendAccountResponseAsync(session, LegacyAccountOperation.ResetPassword, true, "密码重置成功，请使用新密码登录");
    }

    /// <summary>
    /// 发送账号响应
    /// </summary>
    private async ValueTask SendAccountResponseAsync(
        ClientSession session,
        LegacyAccountOperation operation,
        bool success,
        string message)
    {
        await session.SendPacketAsync(Opcodes.SM_ACCOUNT, writer =>
        {
            writer.WriteByte((byte)operation);
            writer.WriteBoolean(success);
            writer.WriteString(message);
        });
    }
}

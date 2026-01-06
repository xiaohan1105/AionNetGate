namespace AionNetGate.Network.Packets;

/// <summary>
/// 账号操作类型
/// </summary>
public enum AccountOperationType : byte
{
    /// <summary>
    /// 注册
    /// </summary>
    Register = 1,

    /// <summary>
    /// 登录
    /// </summary>
    Login = 2,

    /// <summary>
    /// 刷新 Token
    /// </summary>
    RefreshToken = 3,

    /// <summary>
    /// 登出
    /// </summary>
    Logout = 4,

    /// <summary>
    /// 修改密码
    /// </summary>
    ChangePassword = 5,

    /// <summary>
    /// 重置密码
    /// </summary>
    ResetPassword = 6
}

using MessagePack;

namespace AionNetGate.Network.Packets.Client;

/// <summary>
/// 客户端账号请求包
/// </summary>
[MessagePackObject]
public class CM_AccountRequest : PacketBase
{
    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketOpcode Opcode => PacketOpcode.Account;

    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketDirection Direction => PacketDirection.ClientToServer;

    /// <summary>
    /// 操作类型
    /// </summary>
    [Key(0)]
    public AccountOperationType OperationType { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Key(1)]
    public string? Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Key(2)]
    public string? Password { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Key(3)]
    public string? Email { get; set; }

    /// <summary>
    /// Token (用于登出、刷新等操作)
    /// </summary>
    [Key(4)]
    public string? Token { get; set; }

    /// <summary>
    /// Refresh Token
    /// </summary>
    [Key(5)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 旧密码 (修改密码时使用)
    /// </summary>
    [Key(6)]
    public string? OldPassword { get; set; }

    /// <summary>
    /// 新密码 (修改密码、重置密码时使用)
    /// </summary>
    [Key(7)]
    public string? NewPassword { get; set; }
}

using MessagePack;

namespace AionNetGate.Network.Packets.Server;

/// <summary>
/// 服务器账号响应包
/// </summary>
[MessagePackObject]
public class SM_AccountResponse : PacketBase
{
    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketOpcode Opcode => PacketOpcode.Account;

    /// <inheritdoc/>
    [IgnoreMember]
    public override PacketDirection Direction => PacketDirection.ServerToClient;

    /// <summary>
    /// 操作类型
    /// </summary>
    [Key(0)]
    public AccountOperationType OperationType { get; set; }

    /// <summary>
    /// 操作是否成功
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    /// 错误消息 (失败时)
    /// </summary>
    [Key(2)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 账号 ID (登录、注册成功时)
    /// </summary>
    [Key(3)]
    public long AccountId { get; set; }

    /// <summary>
    /// 用户名 (登录成功时)
    /// </summary>
    [Key(4)]
    public string? Username { get; set; }

    /// <summary>
    /// Access Token (登录、刷新成功时)
    /// </summary>
    [Key(5)]
    public string? Token { get; set; }

    /// <summary>
    /// Refresh Token (登录、刷新成功时)
    /// </summary>
    [Key(6)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 账号角色 (登录成功时)
    /// </summary>
    [Key(7)]
    public int Role { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static SM_AccountResponse CreateSuccess(AccountOperationType operationType)
    {
        return new SM_AccountResponse
        {
            OperationType = operationType,
            Success = true
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static SM_AccountResponse CreateFailure(AccountOperationType operationType, string errorMessage)
    {
        return new SM_AccountResponse
        {
            OperationType = operationType,
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

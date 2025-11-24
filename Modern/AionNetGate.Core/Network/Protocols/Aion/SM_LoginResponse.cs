using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network.Protocols.Aion;

/// <summary>
/// 服务器登录响应 Packet (Opcode: 0x01)
/// </summary>
public class SM_LoginResponse : ServerPacket
{
    public override ushort Opcode => 0x01;

    /// <summary>
    /// 登录状态 (0=成功, 1=用户名或密码错误, 2=账号已封禁, 3=服务器繁忙)
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 账号ID
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// 账号名称
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public override int GetEstimatedSize() => 256;

    public override void Deserialize(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException("服务器Packet不支持反序列化");
    }

    public override int Serialize(Span<byte> buffer)
    {
        int offset = 0;

        // 写入状态
        WriteByte(buffer, ref offset, Status);

        // 写入会话ID（固定64字节）
        WriteFixedString(buffer, ref offset, SessionId, 64);

        // 写入账号ID
        WriteInt32(buffer, ref offset, AccountId);

        // 写入账号名称（固定64字节）
        WriteFixedString(buffer, ref offset, AccountName, 64);

        // 写入消息（带长度前缀）
        WriteString(buffer, ref offset, Message);

        return offset;
    }
}

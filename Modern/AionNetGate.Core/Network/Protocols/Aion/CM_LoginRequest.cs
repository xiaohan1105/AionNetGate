using AionNetGate.Core.Network.Packets;

namespace AionNetGate.Core.Network.Protocols.Aion;

/// <summary>
/// 客户端登录请求 Packet (Opcode: 0x01)
/// </summary>
public class CM_LoginRequest : ClientPacket
{
    public override ushort Opcode => 0x01;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码（已加密）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 硬件ID
    /// </summary>
    public string HardwareId { get; set; } = string.Empty;

    public override int GetEstimatedSize() => 256;

    public override void Deserialize(ReadOnlySpan<byte> buffer)
    {
        int offset = 0;

        // 读取用户名（固定64字节）
        Username = ReadFixedString(buffer, ref offset, 64);

        // 读取密码（固定64字节）
        Password = ReadFixedString(buffer, ref offset, 64);

        // 读取硬件ID（固定64字节）
        HardwareId = ReadFixedString(buffer, ref offset, 64);
    }

    public override int Serialize(Span<byte> buffer)
    {
        throw new NotSupportedException("客户端Packet不支持序列化");
    }
}

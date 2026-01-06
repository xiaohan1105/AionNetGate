namespace AionNetGate.Network.Packets;

/// <summary>
/// Packet 头部结构
/// 格式: [PacketSize:4] [Version:1] [Opcode:1] [Flags:1] [Reserved:1]
/// </summary>
public struct PacketHeader
{
    /// <summary>
    /// 头部大小（字节）
    /// </summary>
    public const int Size = 8;

    /// <summary>
    /// 协议版本
    /// </summary>
    public const byte ProtocolVersion = 1;

    /// <summary>
    /// Packet 总大小（包括头部）
    /// </summary>
    public int PacketSize { get; set; }

    /// <summary>
    /// 协议版本号
    /// </summary>
    public byte Version { get; set; }

    /// <summary>
    /// 操作码
    /// </summary>
    public PacketOpcode Opcode { get; set; }

    /// <summary>
    /// 标志位（bit 0: 是否加密, bit 1: 方向 0=C2S 1=S2C）
    /// </summary>
    public byte Flags { get; set; }

    /// <summary>
    /// 保留字节
    /// </summary>
    public byte Reserved { get; set; }

    /// <summary>
    /// 是否加密
    /// </summary>
    public readonly bool IsEncrypted => (Flags & 0x01) != 0;

    /// <summary>
    /// Packet 方向
    /// </summary>
    public readonly PacketDirection Direction => (Flags & 0x02) != 0 ? PacketDirection.ServerToClient : PacketDirection.ClientToServer;

    /// <summary>
    /// Payload 大小
    /// </summary>
    public readonly int PayloadSize => PacketSize - Size;

    /// <summary>
    /// 写入到字节数组
    /// </summary>
    public readonly void WriteTo(Span<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"缓冲区太小，需要至少 {Size} 字节", nameof(buffer));

        BitConverter.TryWriteBytes(buffer[0..4], PacketSize);
        buffer[4] = Version;
        buffer[5] = (byte)Opcode;
        buffer[6] = Flags;
        buffer[7] = Reserved;
    }

    /// <summary>
    /// 从字节数组读取
    /// </summary>
    public static PacketHeader ReadFrom(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"缓冲区太小，需要至少 {Size} 字节", nameof(buffer));

        return new PacketHeader
        {
            PacketSize = BitConverter.ToInt32(buffer[0..4]),
            Version = buffer[4],
            Opcode = (PacketOpcode)buffer[5],
            Flags = buffer[6],
            Reserved = buffer[7]
        };
    }

    /// <summary>
    /// 创建新的 Packet 头部
    /// </summary>
    public static PacketHeader Create(PacketOpcode opcode, int payloadSize, bool isEncrypted, PacketDirection direction)
    {
        byte flags = 0;
        if (isEncrypted) flags |= 0x01;
        if (direction == PacketDirection.ServerToClient) flags |= 0x02;

        return new PacketHeader
        {
            PacketSize = Size + payloadSize,
            Version = ProtocolVersion,
            Opcode = opcode,
            Flags = flags,
            Reserved = 0
        };
    }
}

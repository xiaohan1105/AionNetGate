using System.Buffers;

namespace AionNetGate.Core.Network.Packets;

/// <summary>
/// Packet抽象基类
/// 提供通用的序列化/反序列化辅助方法
/// </summary>
public abstract class PacketBase : IPacket
{
    /// <summary>
    /// Packet的Opcode
    /// </summary>
    public abstract ushort Opcode { get; }

    /// <summary>
    /// 序列化Packet
    /// </summary>
    public abstract int Serialize(Span<byte> buffer);

    /// <summary>
    /// 反序列化Packet
    /// </summary>
    public abstract void Deserialize(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// 获取预估大小
    /// </summary>
    public virtual int GetEstimatedSize() => 256; // 默认256字节

    #region 辅助方法 - 读取基本类型

    /// <summary>
    /// 从缓冲区读取byte
    /// </summary>
    protected static byte ReadByte(ReadOnlySpan<byte> buffer, ref int offset)
    {
        return buffer[offset++];
    }

    /// <summary>
    /// 从缓冲区读取short
    /// </summary>
    protected static short ReadInt16(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var value = System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(buffer.Slice(offset));
        offset += 2;
        return value;
    }

    /// <summary>
    /// 从缓冲区读取ushort
    /// </summary>
    protected static ushort ReadUInt16(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var value = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(offset));
        offset += 2;
        return value;
    }

    /// <summary>
    /// 从缓冲区读取int
    /// </summary>
    protected static int ReadInt32(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var value = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
        offset += 4;
        return value;
    }

    /// <summary>
    /// 从缓冲区读取long
    /// </summary>
    protected static long ReadInt64(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var value = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(offset));
        offset += 8;
        return value;
    }

    /// <summary>
    /// 从缓冲区读取UTF8字符串（带长度前缀）
    /// </summary>
    protected static string ReadString(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var length = ReadInt16(buffer, ref offset);
        if (length <= 0)
            return string.Empty;

        var stringBytes = buffer.Slice(offset, length);
        offset += length;

        return System.Text.Encoding.UTF8.GetString(stringBytes);
    }

    /// <summary>
    /// 从缓冲区读取固定长度字符串
    /// </summary>
    protected static string ReadFixedString(ReadOnlySpan<byte> buffer, ref int offset, int length)
    {
        var stringBytes = buffer.Slice(offset, length);
        offset += length;

        // 找到第一个null字节
        var nullIndex = stringBytes.IndexOf((byte)0);
        if (nullIndex >= 0)
        {
            stringBytes = stringBytes.Slice(0, nullIndex);
        }

        return System.Text.Encoding.UTF8.GetString(stringBytes);
    }

    #endregion

    #region 辅助方法 - 写入基本类型

    /// <summary>
    /// 向缓冲区写入byte
    /// </summary>
    protected static void WriteByte(Span<byte> buffer, ref int offset, byte value)
    {
        buffer[offset++] = value;
    }

    /// <summary>
    /// 向缓冲区写入short
    /// </summary>
    protected static void WriteInt16(Span<byte> buffer, ref int offset, short value)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(offset), value);
        offset += 2;
    }

    /// <summary>
    /// 向缓冲区写入ushort
    /// </summary>
    protected static void WriteUInt16(Span<byte> buffer, ref int offset, ushort value)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset), value);
        offset += 2;
    }

    /// <summary>
    /// 向缓冲区写入int
    /// </summary>
    protected static void WriteInt32(Span<byte> buffer, ref int offset, int value)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), value);
        offset += 4;
    }

    /// <summary>
    /// 向缓冲区写入long
    /// </summary>
    protected static void WriteInt64(Span<byte> buffer, ref int offset, long value)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset), value);
        offset += 8;
    }

    /// <summary>
    /// 向缓冲区写入UTF8字符串（带长度前缀）
    /// </summary>
    protected static void WriteString(Span<byte> buffer, ref int offset, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            WriteInt16(buffer, ref offset, 0);
            return;
        }

        var byteCount = System.Text.Encoding.UTF8.GetByteCount(value);
        WriteInt16(buffer, ref offset, (short)byteCount);

        System.Text.Encoding.UTF8.GetBytes(value, buffer.Slice(offset));
        offset += byteCount;
    }

    /// <summary>
    /// 向缓冲区写入固定长度字符串（不足补0）
    /// </summary>
    protected static void WriteFixedString(Span<byte> buffer, ref int offset, string value, int maxLength)
    {
        var stringBuffer = buffer.Slice(offset, maxLength);
        stringBuffer.Clear(); // 填充0

        if (!string.IsNullOrEmpty(value))
        {
            var byteCount = Math.Min(
                System.Text.Encoding.UTF8.GetByteCount(value),
                maxLength - 1 // 保留最后一个字节为null
            );

            System.Text.Encoding.UTF8.GetBytes(value, stringBuffer);
        }

        offset += maxLength;
    }

    #endregion
}

using System.Buffers.Binary;
using System.Text;

namespace AionNetGate.Network.Protocol;

/// <summary>
/// 数据包读取器 - 用于解析客户端发送的数据包
/// 兼容老项目的 AbstractClientPacket 的读取方法
/// </summary>
public ref struct PacketReader
{
    private readonly ReadOnlySpan<byte> _buffer;
    private int _position;

    /// <summary>
    /// 创建数据包读取器
    /// </summary>
    /// <param name="buffer">数据缓冲区（不包含长度和opcode头）</param>
    public PacketReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    /// <summary>
    /// 当前读取位置
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// 剩余可读字节数
    /// </summary>
    public int Remaining => _buffer.Length - _position;

    /// <summary>
    /// 缓冲区总长度
    /// </summary>
    public int Length => _buffer.Length;

    /// <summary>
    /// 读取一个字节 (readC)
    /// </summary>
    public byte ReadByte()
    {
        if (_position >= _buffer.Length)
            throw new EndOfStreamException("尝试读取超出缓冲区范围");

        return _buffer[_position++];
    }

    /// <summary>
    /// 读取有符号短整数 (2字节, Little Endian)
    /// </summary>
    public short ReadInt16()
    {
        EnsureAvailable(2);
        var value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Slice(_position, 2));
        _position += 2;
        return value;
    }

    /// <summary>
    /// 读取无符号短整数 (readUH - 2字节, Little Endian)
    /// </summary>
    public ushort ReadUInt16()
    {
        EnsureAvailable(2);
        var value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Slice(_position, 2));
        _position += 2;
        return value;
    }

    /// <summary>
    /// 读取有符号整数 (4字节, Little Endian)
    /// </summary>
    public int ReadInt32()
    {
        EnsureAvailable(4);
        var value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(_position, 4));
        _position += 4;
        return value;
    }

    /// <summary>
    /// 读取无符号整数 (4字节, Little Endian)
    /// </summary>
    public uint ReadUInt32()
    {
        EnsureAvailable(4);
        var value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Slice(_position, 4));
        _position += 4;
        return value;
    }

    /// <summary>
    /// 读取长整数 (8字节, Little Endian)
    /// </summary>
    public long ReadInt64()
    {
        EnsureAvailable(8);
        var value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(_position, 8));
        _position += 8;
        return value;
    }

    /// <summary>
    /// 读取字符串 (readS)
    /// 格式: [长度:2字节] [UTF-8内容:N字节]
    /// </summary>
    public string ReadString()
    {
        ushort length = ReadUInt16();

        if (length == 0)
            return string.Empty;

        EnsureAvailable(length);
        var stringBytes = _buffer.Slice(_position, length);
        _position += length;

        return Encoding.UTF8.GetString(stringBytes);
    }

    /// <summary>
    /// 读取指定长度的字节数组
    /// </summary>
    public byte[] ReadBytes(int count)
    {
        EnsureAvailable(count);
        var bytes = _buffer.Slice(_position, count).ToArray();
        _position += count;
        return bytes;
    }

    /// <summary>
    /// 读取剩余所有字节
    /// </summary>
    public byte[] ReadRemainingBytes()
    {
        var remaining = _buffer.Slice(_position).ToArray();
        _position = _buffer.Length;
        return remaining;
    }

    /// <summary>
    /// 读取布尔值 (1字节, 0=false, 非0=true)
    /// </summary>
    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    /// <summary>
    /// 跳过指定字节数
    /// </summary>
    public void Skip(int count)
    {
        EnsureAvailable(count);
        _position += count;
    }

    /// <summary>
    /// 确保有足够的可读字节
    /// </summary>
    private void EnsureAvailable(int count)
    {
        if (_position + count > _buffer.Length)
            throw new EndOfStreamException($"需要读取 {count} 字节，但只剩 {Remaining} 字节");
    }
}

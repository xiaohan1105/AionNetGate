using AionNetGate.Core.Interfaces;
using AionNetGate.Network.Packets;
using MessagePack;

namespace AionNetGate.Network.Serialization;

/// <summary>
/// Packet 序列化器实现
/// </summary>
public class PacketSerializer : IPacketSerializer
{
    private readonly IEncryptionService? _encryptionService;
    private readonly PacketRegistry _registry;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="registry">Packet 注册表</param>
    /// <param name="encryptionService">加密服务（可选）</param>
    public PacketSerializer(PacketRegistry registry, IEncryptionService? encryptionService = null)
    {
        _registry = registry;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// 序列化 Packet 为字节数组
    /// </summary>
    public async Task<byte[]> SerializeAsync(IPacket packet, CancellationToken cancellationToken = default)
    {
        // 1. 序列化 payload
        var payload = await packet.SerializeAsync(cancellationToken);

        // 2. 加密 payload（如果需要）
        if (packet.IsEncrypted && _encryptionService != null)
        {
            payload = await _encryptionService.EncryptAsync(payload, cancellationToken);
        }

        // 3. 创建 header
        var header = PacketHeader.Create(packet.Opcode, payload.Length, packet.IsEncrypted, packet.Direction);

        // 4. 组合 header + payload
        var result = new byte[PacketHeader.Size + payload.Length];
        header.WriteTo(result.AsSpan(0, PacketHeader.Size));
        payload.AsSpan().CopyTo(result.AsSpan(PacketHeader.Size));

        return result;
    }

    /// <summary>
    /// 从字节数组反序列化 Packet
    /// </summary>
    public async Task<IPacket> DeserializeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (data.Length < PacketHeader.Size)
            throw new ArgumentException("数据太短，无法解析 Packet Header", nameof(data));

        // 1. 读取 header
        var header = PacketHeader.ReadFrom(data.Span[0..PacketHeader.Size]);

        // 2. 验证版本
        if (header.Version != PacketHeader.ProtocolVersion)
            throw new InvalidOperationException($"不支持的协议版本: {header.Version}");

        // 3. 验证大小
        if (data.Length < header.PacketSize)
            throw new ArgumentException($"数据不完整，需要 {header.PacketSize} 字节，实际 {data.Length} 字节", nameof(data));

        // 4. 提取 payload
        var payload = data.Slice(PacketHeader.Size, header.PayloadSize);

        // 5. 解密 payload（如果需要）
        byte[] decryptedPayload;
        if (header.IsEncrypted && _encryptionService != null)
        {
            decryptedPayload = await _encryptionService.DecryptAsync(payload, cancellationToken);
        }
        else
        {
            decryptedPayload = payload.ToArray();
        }

        // 6. 反序列化为 Packet 对象，使用 header 中的方向信息
        var packetType = _registry.GetPacketType(header.Opcode, header.Direction);

        if (packetType == null)
            throw new InvalidOperationException($"未注册的 Opcode: {header.Opcode}, Direction: {header.Direction}");

        var packet = (IPacket?)MessagePackSerializer.Deserialize(packetType, decryptedPayload, cancellationToken: cancellationToken);
        return packet ?? throw new InvalidOperationException($"反序列化失败: Opcode={header.Opcode}");
    }

    /// <summary>
    /// 尝试从缓冲区读取完整的 Packet
    /// </summary>
    public bool TryReadPacket(ReadOnlySpan<byte> buffer, out IPacket? packet, out int bytesConsumed)
    {
        packet = null;
        bytesConsumed = 0;

        // 1. 检查是否有足够的数据读取 header
        if (buffer.Length < PacketHeader.Size)
            return false;

        // 2. 读取 header
        var header = PacketHeader.ReadFrom(buffer[0..PacketHeader.Size]);

        // 3. 验证版本
        if (header.Version != PacketHeader.ProtocolVersion)
            throw new InvalidOperationException($"不支持的协议版本: {header.Version}");

        // 4. 检查是否有完整的 packet
        if (buffer.Length < header.PacketSize)
            return false;

        // 5. 同步反序列化（性能考虑）
        try
        {
            var packetData = buffer[0..header.PacketSize];
            var payload = packetData[PacketHeader.Size..];

            // 解密
            byte[] decryptedPayload;
            if (header.IsEncrypted && _encryptionService != null)
            {
                decryptedPayload = _encryptionService.Decrypt(payload);
            }
            else
            {
                decryptedPayload = payload.ToArray();
            }

            // 反序列化，使用 header 中的方向信息
            var packetType = _registry.GetPacketType(header.Opcode, header.Direction);

            if (packetType == null)
                throw new InvalidOperationException($"未注册的 Opcode: {header.Opcode}, Direction: {header.Direction}");

            packet = (IPacket?)MessagePackSerializer.Deserialize(packetType, decryptedPayload)
                ?? throw new InvalidOperationException($"反序列化失败: Opcode={header.Opcode}");
            bytesConsumed = header.PacketSize;
            return true;
        }
        catch
        {
            return false;
        }
    }
}

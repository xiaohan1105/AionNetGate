using System.Net.Sockets;

Console.WriteLine("=== Aion网关测试客户端 ===");
Console.WriteLine();

try
{
    // 连接到服务器
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 9999);
    Console.WriteLine("✓ 已连接到服务器 127.0.0.1:9999");

    var stream = client.GetStream();

    // 发送CM_ConnectRequest (Opcode 0x00)
    // 协议格式: [2字节长度][2字节Opcode][数据]
    Console.WriteLine("发送连接请求...");

    // 准备数据
    var clientVersion = "1.0.0";
    var hardwareId = "TEST-HW-12345";
    var macAddress = "00:11:22:33:44:55";

    // 计算数据长度 (32 + 64 + 32 = 128字节)
    var dataLength = 128;
    var totalLength = (short)(2 + dataLength); // Opcode(2) + 数据

    // 构建packet
    var packet = new byte[2 + totalLength];
    int offset = 0;

    // 写入长度
    BitConverter.GetBytes(totalLength).CopyTo(packet, offset);
    offset += 2;

    // 写入Opcode (0x00)
    BitConverter.GetBytes((short)0x00).CopyTo(packet, offset);
    offset += 2;

    // 写入ClientVersion (32字节固定)
    var versionBytes = System.Text.Encoding.UTF8.GetBytes(clientVersion);
    Array.Copy(versionBytes, 0, packet, offset, Math.Min(versionBytes.Length, 32));
    offset += 32;

    // 写入HardwareId (64字节固定)
    var hwBytes = System.Text.Encoding.UTF8.GetBytes(hardwareId);
    Array.Copy(hwBytes, 0, packet, offset, Math.Min(hwBytes.Length, 64));
    offset += 64;

    // 写入MacAddress (32字节固定)
    var macBytes = System.Text.Encoding.UTF8.GetBytes(macAddress);
    Array.Copy(macBytes, 0, packet, offset, Math.Min(macBytes.Length, 32));

    // 发送
    await stream.WriteAsync(packet);
    Console.WriteLine($"✓ 已发送连接请求 ({packet.Length} 字节)");

    // 接收响应
    Console.WriteLine("等待服务器响应...");
    var responseBuffer = new byte[1024];
    var bytesRead = await stream.ReadAsync(responseBuffer.AsMemory(0, responseBuffer.Length));

    if (bytesRead > 0)
    {
        Console.WriteLine($"✓ 收到服务器响应 ({bytesRead} 字节)");

        // 简单解析响应
        var respLength = BitConverter.ToInt16(responseBuffer, 0);
        var respOpcode = BitConverter.ToUInt16(responseBuffer, 2);
        Console.WriteLine($"  响应长度: {respLength}");
        Console.WriteLine($"  响应Opcode: 0x{respOpcode:X4}");
    }

    // 发送Ping (Opcode 0x05)
    Console.WriteLine();
    Console.WriteLine("发送Ping...");
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var pingPacket = new byte[12]; // 2(长度) + 2(opcode) + 8(timestamp)
    BitConverter.GetBytes((short)10).CopyTo(pingPacket, 0); // 长度=10 (opcode+data)
    BitConverter.GetBytes((short)0x05).CopyTo(pingPacket, 2);
    BitConverter.GetBytes(timestamp).CopyTo(pingPacket, 4);

    await stream.WriteAsync(pingPacket);
    Console.WriteLine($"✓ 已发送Ping (timestamp={timestamp})");

    // 接收Pong
    bytesRead = await stream.ReadAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
    if (bytesRead > 0)
    {
        var pongOpcode = BitConverter.ToUInt16(responseBuffer, 2);
        var pongTimestamp = BitConverter.ToInt64(responseBuffer, 4);
        Console.WriteLine($"✓ 收到Pong (Opcode=0x{pongOpcode:X4}, timestamp={pongTimestamp})");

        var rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
        Console.WriteLine($"  往返时间: {rtt}ms");
    }

    Console.WriteLine();
    Console.WriteLine("测试完成!按任意键关闭连接...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

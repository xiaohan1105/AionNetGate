using AionNetGate.Network.Packets;
using AionNetGate.Network.Packets.Client;
using AionNetGate.Network.Packets.Server;
using AionNetGate.Network.Serialization;
using FluentAssertions;

namespace AionNetGate.IntegrationTests.Network;

/// <summary>
/// Packet 序列化测试
/// </summary>
public class PacketSerializationTests
{
    private readonly PacketRegistry _packetRegistry;
    private readonly PacketSerializer _packetSerializer;

    public PacketSerializationTests()
    {
        _packetRegistry = new PacketRegistry();
        _packetSerializer = new PacketSerializer(_packetRegistry);
    }

    [Fact]
    public async Task ConnectRequest_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new CM_ConnectRequest
        {
            HardwareId = "HW-12345",
            ClientVersion = "1.0.0",
            OsInfo = "Windows 11",
            CpuId = "CPU-123",
            MacAddress = "00:11:22:33:44:55",
            MotherboardSerial = "MB-456",
            DiskSerial = "DISK-789"
        };

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<CM_ConnectRequest>();

        var packet = (CM_ConnectRequest)deserialized!;
        packet.HardwareId.Should().Be(originalPacket.HardwareId);
        packet.ClientVersion.Should().Be(originalPacket.ClientVersion);
        packet.OsInfo.Should().Be(originalPacket.OsInfo);
        packet.CpuId.Should().Be(originalPacket.CpuId);
        packet.MacAddress.Should().Be(originalPacket.MacAddress);
    }

    [Fact]
    public async Task ConnectResponse_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new SM_ConnectResponse
        {
            Success = true,
            Message = "Connected successfully",
            ServerTime = DateTime.UtcNow
        };

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SM_ConnectResponse>();

        var packet = (SM_ConnectResponse)deserialized!;
        packet.Success.Should().Be(originalPacket.Success);
        packet.Message.Should().Be(originalPacket.Message);
        packet.ServerTime.Should().BeCloseTo(originalPacket.ServerTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AccountRequest_Register_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new CM_AccountRequest
        {
            OperationType = AccountOperationType.Register,
            Username = "testuser",
            Password = "password123",
            Email = "test@example.com"
        };

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<CM_AccountRequest>();

        var packet = (CM_AccountRequest)deserialized!;
        packet.OperationType.Should().Be(AccountOperationType.Register);
        packet.Username.Should().Be(originalPacket.Username);
        packet.Password.Should().Be(originalPacket.Password);
        packet.Email.Should().Be(originalPacket.Email);
    }

    [Fact]
    public async Task AccountRequest_Login_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new CM_AccountRequest
        {
            OperationType = AccountOperationType.Login,
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<CM_AccountRequest>();

        var packet = (CM_AccountRequest)deserialized!;
        packet.OperationType.Should().Be(AccountOperationType.Login);
        packet.Username.Should().Be(originalPacket.Username);
        packet.Password.Should().Be(originalPacket.Password);
    }

    [Fact]
    public async Task AccountResponse_Success_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new SM_AccountResponse
        {
            OperationType = AccountOperationType.Login,
            Success = true,
            AccountId = 12345,
            Username = "testuser",
            Token = "access-token-123",
            RefreshToken = "refresh-token-456",
            Role = 1
        };

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SM_AccountResponse>();

        var packet = (SM_AccountResponse)deserialized!;
        packet.OperationType.Should().Be(AccountOperationType.Login);
        packet.Success.Should().BeTrue();
        packet.AccountId.Should().Be(originalPacket.AccountId);
        packet.Username.Should().Be(originalPacket.Username);
        packet.Token.Should().Be(originalPacket.Token);
        packet.RefreshToken.Should().Be(originalPacket.RefreshToken);
        packet.Role.Should().Be(originalPacket.Role);
    }

    [Fact]
    public async Task AccountResponse_Failure_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = SM_AccountResponse.CreateFailure(
            AccountOperationType.Login,
            "用户名或密码错误"
        );

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SM_AccountResponse>();

        var packet = (SM_AccountResponse)deserialized!;
        packet.Success.Should().BeFalse();
        packet.ErrorMessage.Should().Be("用户名或密码错误");
    }

    [Fact]
    public async Task Ping_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new CM_Ping();

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<CM_Ping>();
    }

    [Fact]
    public async Task Pong_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange
        var originalPacket = new SM_Pong();

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<SM_Pong>();
    }

    [Fact]
    public void PacketRegistry_ShouldRegisterAllPackets()
    {
        // Act & Assert
        var connectRequestType = _packetRegistry.GetPacketType(PacketOpcode.Connect, PacketDirection.ClientToServer);
        connectRequestType.Should().Be(typeof(CM_ConnectRequest));

        var connectResponseType = _packetRegistry.GetPacketType(PacketOpcode.Connect, PacketDirection.ServerToClient);
        connectResponseType.Should().Be(typeof(SM_ConnectResponse));

        var accountRequestType = _packetRegistry.GetPacketType(PacketOpcode.Account, PacketDirection.ClientToServer);
        accountRequestType.Should().Be(typeof(CM_AccountRequest));

        var accountResponseType = _packetRegistry.GetPacketType(PacketOpcode.Account, PacketDirection.ServerToClient);
        accountResponseType.Should().Be(typeof(SM_AccountResponse));
    }

    [Fact]
    public void PacketRegistry_CreatePacket_ShouldWork()
    {
        // Act
        var connectRequest = _packetRegistry.CreatePacket(PacketOpcode.Connect, PacketDirection.ClientToServer);
        var accountResponse = _packetRegistry.CreatePacket(PacketOpcode.Account, PacketDirection.ServerToClient);

        // Assert
        connectRequest.Should().NotBeNull();
        connectRequest.Should().BeOfType<CM_ConnectRequest>();

        accountResponse.Should().NotBeNull();
        accountResponse.Should().BeOfType<SM_AccountResponse>();
    }

    [Fact]
    public async Task LargePacket_SerializeAndDeserialize_ShouldWork()
    {
        // Arrange - 创建一个较大的数据包
        var originalPacket = new CM_AccountRequest
        {
            OperationType = AccountOperationType.Register,
            Username = new string('A', 1000),  // 长用户名
            Password = new string('B', 1000),  // 长密码
            Email = new string('C', 1000) + "@example.com"
        };

        // Act
        var serialized = await _packetSerializer.SerializeAsync(originalPacket);
        var deserialized = await _packetSerializer.DeserializeAsync(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        var packet = (CM_AccountRequest)deserialized!;
        packet.Username.Should().HaveLength(1000);
        packet.Password.Should().HaveLength(1000);
    }
}

using AionNetGate.Core.Network.Protocols.Aion;
using AionNetGate.Core.Network.Protocols.Aion.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace AionNetGate.Core.Network;

/// <summary>
/// 网络服务的依赖注入扩展
/// </summary>
public static class NetworkServiceExtensions
{
    /// <summary>
    /// 注册核心网络服务
    /// </summary>
    public static IServiceCollection AddAionNetworkServices(
        this IServiceCollection services)
    {
        // 注册核心组件
        services.AddSingleton<PacketRegistry>();
        services.AddSingleton<PacketHandlerRegistry>();
        services.AddSingleton<PacketProcessor>();

        // 注册所有Aion Protocol的PacketHandlers
        services.AddTransient<CM_ConnectRequestHandler>();
        services.AddTransient<CM_PingHandler>();

        // TODO: 注册更多Handler...

        return services;
    }

    /// <summary>
    /// 配置Aion协议的Packet和Handler注册
    /// </summary>
    public static void ConfigureAionProtocol(
        PacketRegistry packetRegistry,
        PacketHandlerRegistry handlerRegistry)
    {
        if (packetRegistry == null)
            throw new ArgumentNullException(nameof(packetRegistry));
        if (handlerRegistry == null)
            throw new ArgumentNullException(nameof(handlerRegistry));

        // ========== 注册客户端Packets ==========
        packetRegistry.RegisterClientPacket<CM_ConnectRequest>(0x00);
        packetRegistry.RegisterClientPacket<CM_Ping>(0x05);
        // TODO: 注册更多客户端packets...

        // ========== 注册服务器Packets ==========
        packetRegistry.RegisterServerPacket<SM_ConnectFinished>(0x00);
        packetRegistry.RegisterServerPacket<SM_Pong>(0x06);
        // TODO: 注册更多服务器packets...

        // ========== 注册PacketHandlers ==========
        handlerRegistry.RegisterHandler<CM_ConnectRequest, CM_ConnectRequestHandler>();
        handlerRegistry.RegisterHandler<CM_Ping, CM_PingHandler>();
        // TODO: 注册更多handlers...
    }

    /// <summary>
    /// 创建并配置NetworkListener
    /// </summary>
    public static NetworkListener CreateNetworkListener(
        IServiceProvider serviceProvider,
        int port)
    {
        var packetProcessor = serviceProvider.GetRequiredService<PacketProcessor>();
        var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NetworkListener>>();

        return new NetworkListener(port, packetProcessor, logger);
    }
}

using AionNetGate.Network.Handlers;
using AionNetGate.Network.Handlers.Legacy;
using AionNetGate.Network.Protocol;
using AionNetGate.Network.Server;
using AionNetGate.Network.Services;
using Microsoft.Extensions.DependencyInjection;
using static AionNetGate.Network.Protocol.Opcodes;

namespace AionNetGate.Network.Extensions;

/// <summary>
/// Network 层服务注册扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加网络服务
    /// </summary>
    public static IServiceCollection AddNetworkServices(this IServiceCollection services)
    {
        // 协议相关
        services.AddSingleton<LegacyPacketSerializer>();

        // 处理器注册表和分发器
        services.AddSingleton<LegacyPacketHandlerRegistry>();
        services.AddSingleton<PacketDispatcher>();

        // 网关服务器
        services.AddSingleton<GatewayServer>();

        // 远程管理服务
        services.AddSingleton<RemoteManagementService>();
        services.AddSingleton<IRemoteManagementService>(sp => sp.GetRequiredService<RemoteManagementService>());

        // 托管服务
        services.AddHostedService<GatewayHostedService>();

        // 注册所有老协议处理器
        services.AddLegacyPacketHandlers();

        return services;
    }

    /// <summary>
    /// 注册老协议数据包处理器
    /// </summary>
    private static IServiceCollection AddLegacyPacketHandlers(this IServiceCollection services)
    {
        // 基础处理器
        services.AddScoped<LegacyConnectHandler>();
        services.AddScoped<LegacyPingHandler>();
        services.AddScoped<LegacyAccountHandler>();
        services.AddScoped<LegacyBulletinHandler>();

        // 远程管理处理器
        services.AddScoped<LegacyRemoteDesktopHandler>();
        services.AddScoped<LegacyProcessHandler>();
        services.AddScoped<LegacyComputerInfoHandler>();
        services.AddScoped<LegacyFileExplorerHandler>();
        services.AddScoped<LegacyRegistryHandler>();
        services.AddScoped<LegacyServicesHandler>();

        return services;
    }

    /// <summary>
    /// 初始化处理器注册表 - 注册 opcode 到处理器类型的映射
    /// </summary>
    public static IServiceProvider InitializePacketHandlers(this IServiceProvider serviceProvider)
    {
        var registry = serviceProvider.GetRequiredService<LegacyPacketHandlerRegistry>();

        // 基础处理器
        registry.Register(CM_CONNECT, typeof(LegacyConnectHandler));
        registry.Register(CM_PING, typeof(LegacyPingHandler));
        registry.Register(CM_ACCOUNT, typeof(LegacyAccountHandler));
        registry.Register(CM_BULLETIN, typeof(LegacyBulletinHandler));

        // 远程管理处理器
        registry.Register(CM_PICTURE, typeof(LegacyRemoteDesktopHandler));
        registry.Register(CM_PROCESSES, typeof(LegacyProcessHandler));
        registry.Register(CM_COMPUTER_INFO, typeof(LegacyComputerInfoHandler));
        registry.Register(CM_EXPLORER, typeof(LegacyFileExplorerHandler));
        registry.Register(CM_REGISTRY, typeof(LegacyRegistryHandler));
        registry.Register(CM_SERVICES, typeof(LegacyServicesHandler));

        // 冻结注册表，优化后续查询性能（使用 FrozenDictionary）
        registry.Freeze();

        return serviceProvider;
    }
}

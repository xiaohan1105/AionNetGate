using AionNetGate.Core.Data;
using AionNetGate.Core.Network;
using AionNetGate.Core.Network.Protocols.Aion;
using AionNetGate.Core.Network.Protocols.Aion.Handlers;
using AionNetGate.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AionNetGate.Core.Extensions;

/// <summary>
/// 服务集合扩展方法
/// 集中注册所有服务
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册AionNetGate核心服务
    /// </summary>
    public static IServiceCollection AddAionNetGateCoreServices(
        this IServiceCollection services,
        string connectionString,
        bool useMySql = true)
    {
        // ========== 数据访问层 ==========
        // 注册DbConnectionFactory
        if (useMySql)
        {
            services.AddSingleton<IDbConnectionFactory>(sp =>
                new MySqlConnectionFactory(connectionString));
        }
        else
        {
            services.AddSingleton<IDbConnectionFactory>(sp =>
                new MSSqlConnectionFactory(connectionString));
        }

        // 注册Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
        services.AddScoped<IIPBlacklistRepository, IPBlacklistRepository>();

        // ========== 领域服务 ==========
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IConnectionTracker, ConnectionTracker>();
        services.AddSingleton<IIPBlacklistChecker, IPBlacklistChecker>();

        // ========== 应用服务 ==========
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddScoped<ILoginService, LoginService>();

        // ========== MediatR (CQRS) ==========
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        // ========== 网络层 ==========
        services.AddSingleton<PacketRegistry>();
        services.AddSingleton<PacketHandlerRegistry>();
        services.AddSingleton<PacketProcessor>();

        // 注册所有PacketHandlers
        services.AddTransient<CM_ConnectRequestHandler>();
        services.AddTransient<CM_PingHandler>();
        services.AddTransient<CM_LoginRequestHandler>();

        return services;
    }

    /// <summary>
    /// 配置Aion协议
    /// </summary>
    public static void ConfigureAionProtocol(
        IServiceProvider serviceProvider)
    {
        var packetRegistry = serviceProvider.GetRequiredService<PacketRegistry>();
        var handlerRegistry = serviceProvider.GetRequiredService<PacketHandlerRegistry>();

        // ========== 注册客户端Packets ==========
        packetRegistry.RegisterClientPacket<CM_ConnectRequest>(0x00);
        packetRegistry.RegisterClientPacket<CM_LoginRequest>(0x01);
        packetRegistry.RegisterClientPacket<CM_Ping>(0x05);

        // ========== 注册服务器Packets ==========
        packetRegistry.RegisterServerPacket<SM_ConnectFinished>(0x00);
        packetRegistry.RegisterServerPacket<SM_LoginResponse>(0x01);
        packetRegistry.RegisterServerPacket<SM_Pong>(0x06);

        // ========== 注册PacketHandlers ==========
        handlerRegistry.RegisterHandler<CM_ConnectRequest, CM_ConnectRequestHandler>();
        handlerRegistry.RegisterHandler<CM_LoginRequest, CM_LoginRequestHandler>();
        handlerRegistry.RegisterHandler<CM_Ping, CM_PingHandler>();
    }
}

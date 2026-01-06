using AionNetGate.Core.Configuration;
using AionNetGate.Core.Interfaces;
using AionNetGate.Infrastructure.Data;
using AionNetGate.Infrastructure.Data.Repositories;
using AionNetGate.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AionNetGate.IntegrationTests;

/// <summary>
/// 集成测试基类
/// </summary>
public abstract class TestBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; }
    protected AionDbContext DbContext { get; }
    protected IUnitOfWork UnitOfWork { get; }

    protected TestBase()
    {
        var services = new ServiceCollection();

        // 配置日志
        services.AddLogging(builder => builder.AddConsole());

        // 配置内存数据库（InMemory 数据库不支持事务，需要忽略警告）
        services.AddDbContext<AionDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

        // 配置选项
        services.Configure<SecurityConfig>(options =>
        {
            options.JwtSecretKey = "TestSecretKeyForJwtTokenGenerationTesting12345678";
            options.JwtIssuer = "AionNetGateTest";
            options.JwtAudience = "AionClientsTest";
            options.AccessTokenExpirationMinutes = 60;
            options.RefreshTokenExpirationDays = 7;
            options.MaxLoginAttempts = 5;
            options.AccountLockoutMinutes = 30;
        });

        services.Configure<DatabaseConfig>(options =>
        {
            options.Provider = "InMemory";
            options.ConnectionString = "";
        });

        services.Configure<ServerConfig>(options =>
        {
            options.BindAddress = "127.0.0.1";
            options.Port = 10001;
        });

        // 注册核心服务
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();

        // 注册仓储和工作单元
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IHardwareFingerprintRepository, HardwareFingerprintRepository>();
        services.AddScoped<IIpBlacklistRepository, IpBlacklistRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        ServiceProvider = services.BuildServiceProvider();

        // 获取 DbContext 和 UnitOfWork
        DbContext = ServiceProvider.GetRequiredService<AionDbContext>();
        UnitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();

        // 确保数据库已创建
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        ServiceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}

using AionNetGate.Application.Services;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Interfaces;
using AionNetGate.Core.Services;
using AionNetGate.Host.Health;
using AionNetGate.Host.Monitoring;
using AionNetGate.Host.Services;
using AionNetGate.Infrastructure.Caching;
using AionNetGate.Infrastructure.Data;
using AionNetGate.Infrastructure.Data.Repositories;
using AionNetGate.Infrastructure.Security;
using AionNetGate.Infrastructure.Services;
using AionNetGate.Network.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/aionnetgate-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// ==================== 配置选项 ====================

// 基础配置
builder.Services.Configure<ServerConfig>(builder.Configuration.GetSection(ServerConfig.SectionName));
builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection(DatabaseConfig.SectionName));
builder.Services.Configure<SecurityConfig>(builder.Configuration.GetSection(SecurityConfig.SectionName));
builder.Services.Configure<LoggingConfig>(builder.Configuration.GetSection(LoggingConfig.SectionName));

// 新增配置
builder.Services.Configure<GatewayAdvancedConfig>(builder.Configuration.GetSection(GatewayAdvancedConfig.SectionName));
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection(EmailConfig.SectionName));
builder.Services.Configure<FirewallConfig>(builder.Configuration.GetSection(FirewallConfig.SectionName));
builder.Services.Configure<LauncherConfig>(builder.Configuration.GetSection(LauncherConfig.SectionName));
builder.Services.Configure<CheatDetectionConfig>(builder.Configuration.GetSection(CheatDetectionConfig.SectionName));
builder.Services.Configure<GameDatabaseConfig>(builder.Configuration.GetSection(GameDatabaseConfig.SectionName));

// ==================== 数据库配置 ====================

var databaseConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>();
if (databaseConfig?.Provider == "MySQL")
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
    builder.Services.AddDbContext<AionDbContext>(options =>
        options.UseMySql(databaseConfig.ConnectionString, serverVersion));
}
else if (databaseConfig?.Provider == "MSSQL")
{
    builder.Services.AddDbContext<AionDbContext>(options =>
        options.UseSqlServer(databaseConfig.ConnectionString));
}
else
{
    // 默认使用 SQLite（仅用于开发/测试）
    builder.Services.AddDbContext<AionDbContext>(options =>
        options.UseSqlite("Data Source=aionnetgate.db"));
}

// ==================== 核心服务 ====================

builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();

// ==================== 缓存服务 ====================

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// ==================== 仓储和工作单元 ====================

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IHardwareFingerprintRepository, HardwareFingerprintRepository>();
builder.Services.AddScoped<IIpBlacklistRepository, IpBlacklistRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==================== 应用服务 ====================

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IIpBlacklistService, IpBlacklistService>();

// 新增服务
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IFirewallService, FirewallService>();
builder.Services.AddSingleton<ILauncherConfigService, LauncherConfigService>();

// ==================== 网络服务 ====================

builder.Services.AddNetworkServices();

// ==================== 健康检查 ====================

builder.Services.AddHealthChecks()
    .AddCheck<GatewayHealthCheck>("gateway", tags: new[] { "ready" })
    .AddDbContextCheck<AionDbContext>("database", tags: new[] { "ready" });

// ==================== 监控服务 ====================

builder.Services.AddHostedService<MetricsCollectorService>();
builder.Services.AddHostedService<ManagementApiService>();

// ==================== 构建应用 ====================

var host = builder.Build();

// 初始化处理器注册表
host.Services.InitializePacketHandlers();

// 确保数据库已创建
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AionDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    Log.Information("数据库已初始化");
}

// 读取配置信息
var serverConfig = builder.Configuration.GetSection("Server").Get<ServerConfig>();
var gatewayAdvanced = builder.Configuration.GetSection("GatewayAdvanced").Get<GatewayAdvancedConfig>();
var emailConfig = builder.Configuration.GetSection("Email").Get<EmailConfig>();
var firewallConfig = builder.Configuration.GetSection("Firewall").Get<FirewallConfig>();
var launcherConfig = builder.Configuration.GetSection("Launcher").Get<LauncherConfig>();
var cheatConfig = builder.Configuration.GetSection("CheatDetection").Get<CheatDetectionConfig>();

var managementPort = (serverConfig?.Port ?? 10001) + 1000;

// ==================== 启动信息 ====================

Log.Information("========================================");
Log.Information("  AionNetGate 网关服务器");
Log.Information("  版本: 2.1.0 (Modern/.NET 9)");
Log.Information("  协议: 兼容老启动器");
Log.Information("----------------------------------------");
Log.Information("  网关端口: {Port}", serverConfig?.Port ?? 10001);
Log.Information("  管理端口: {ManagementPort}", managementPort);

if (gatewayAdvanced?.EnableDualLine == true && !string.IsNullOrEmpty(gatewayAdvanced.SecondaryIpAddress))
{
    Log.Information("  双线支持: 已启用 ({SecondaryIp})", gatewayAdvanced.SecondaryIpAddress);
}

Log.Information("----------------------------------------");
Log.Information("  功能状态:");
Log.Information("    邮件服务: {Status}", emailConfig?.Enabled == true ? "已启用" : "未启用");
Log.Information("    防火墙: {Status}", firewallConfig?.Enabled == true ? "已启用" : "未启用");
Log.Information("    外挂检测: {Status}", cheatConfig?.Enabled == true ? "已启用" : "未启用");
Log.Information("    密码找回: {Status}", emailConfig?.AllowPasswordRecovery == true ? "已启用" : "未启用");

if (!string.IsNullOrEmpty(launcherConfig?.LauncherName))
{
    Log.Information("    登录器: {Name}", launcherConfig.LauncherName);
}

Log.Information("----------------------------------------");
Log.Information("  健康检查: http://localhost:{Port}/health", managementPort);
Log.Information("  Prometheus: http://localhost:{Port}/metrics", managementPort);
Log.Information("========================================");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序启动失败");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

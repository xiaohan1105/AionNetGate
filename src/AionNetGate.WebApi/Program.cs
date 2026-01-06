using System.Text;
using AionNetGate.Application.Services;
using AionNetGate.Core.Configuration;
using AionNetGate.Core.Interfaces;
using AionNetGate.Infrastructure.Data;
using AionNetGate.Infrastructure.Data.Repositories;
using AionNetGate.Infrastructure.Security;
using AionNetGate.WebApi.Hubs;
using AionNetGate.WebApi.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

using System.IdentityModel.Tokens.Jwt;

// 禁用 JWT claim 类型映射
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/webapi-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 添加配置
builder.Services.Configure<ServerConfig>(builder.Configuration.GetSection("Server"));
builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<SecurityConfig>(builder.Configuration.GetSection("Security"));

// 配置数据库
var dbConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>()!;
builder.Services.AddDbContext<AionDbContext>(options =>
{
    switch (dbConfig.Provider?.ToLower())
    {
        case "mysql":
            var mysqlVersion = new MySqlServerVersion(new Version(8, 0, 36));
            options.UseMySql(dbConfig.ConnectionString, mysqlVersion);
            break;
        case "sqlserver":
        case "mssql":
            options.UseSqlServer(dbConfig.ConnectionString);
            break;
        default:
            options.UseSqlite(dbConfig.ConnectionString ?? "Data Source=aiongate.db");
            break;
    }
});

// 注册仓储和服务
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<IAccountService, AccountService>();

// 配置 JWT 认证
var securityConfig = builder.Configuration.GetSection("Security").Get<SecurityConfig>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = securityConfig.JwtIssuer,
            ValidAudience = securityConfig.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(securityConfig.JwtSecretKey ?? "AionNetGate-Default-Secret-Key-2024!"))
        };

        // SignalR JWT 配置
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("role", "99"));
    options.AddPolicy("GM", policy => policy.RequireClaim("role", "10", "99"));
    options.AddPolicy("VIP", policy => policy.RequireClaim("role", "1", "10", "99"));
});

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite 默认端口
                "http://localhost:3000",  // 备用端口
                "http://127.0.0.1:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// 添加控制器
builder.Services.AddControllers();

// 配置 SignalR
builder.Services.AddSignalR();

// 配置 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AionNetGate API",
        Version = "v1",
        Description = "AionNetGate 游戏网关管理 API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AionDbContext>("database");

var app = builder.Build();

// 确保数据目录存在
var dataDir = Path.Combine(app.Environment.ContentRootPath, "data");
if (!Directory.Exists(dataDir))
{
    Directory.CreateDirectory(dataDir);
}

// 自动迁移数据库
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AionDbContext>();
    await db.Database.EnsureCreatedAsync();

    // 创建默认管理员账号
    var accountRepo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Accounts;
    if (!await accountRepo.UsernameExistsAsync("admin"))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var (hash, salt) = passwordHasher.HashPassword("admin123");
        await accountRepo.AddAsync(new AionNetGate.Core.Domain.Entities.Account
        {
            Username = "admin",
            Email = "admin@localhost",
            PasswordHash = hash,
            PasswordSalt = salt,
            Status = 1,
            Role = 99, // 管理员
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        Log.Information("已创建默认管理员账号: admin / admin123");
    }
}

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AionNetGate API v1");
        c.RoutePrefix = "swagger";
    });
}

// 请求日志
app.UseSerilogRequestLogging();

// 全局异常处理
app.UseMiddleware<ExceptionMiddleware>();

// CORS
app.UseCors("AllowVueApp");

// 静态文件托管 (前端) - 必须在路由之前
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (Directory.Exists(wwwrootPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// 认证授权
app.UseAuthentication();
app.UseAuthorization();

// Prometheus 指标
app.UseHttpMetrics();

// 健康检查端点
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// 控制器路由
app.MapControllers();

// SignalR Hub
app.MapHub<DashboardHub>("/hubs/dashboard");

// Prometheus 指标端点
app.MapMetrics();

// SPA 回退路由 - 只对非API路径生效
if (Directory.Exists(wwwrootPath))
{
    app.MapFallback(context =>
    {
        // API 路径不回退到 index.html
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Path.StartsWithSegments("/hubs") ||
            context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/metrics") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        }
        context.Request.Path = "/index.html";
        return context.Response.SendFileAsync(Path.Combine(wwwrootPath, "index.html"));
    });
}

Log.Information("AionNetGate WebAPI starting on {Urls}", string.Join(", ", app.Urls));

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

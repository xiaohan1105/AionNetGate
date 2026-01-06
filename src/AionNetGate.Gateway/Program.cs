using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==================== YARP 反向代理 ====================

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ==================== 速率限制 ====================

builder.Services.AddRateLimiter(options =>
{
    // API 通用限制：100 请求/分钟
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 10;
    });

    // 认证接口限制：20 请求/分钟（防暴力破解）
    options.AddSlidingWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 20;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.SegmentsPerWindow = 4;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 5;
    });

    // 监控接口限制：200 请求/分钟
    options.AddTokenBucketLimiter("monitoring", limiter =>
    {
        limiter.TokenLimit = 200;
        limiter.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        limiter.TokensPerPeriod = 200;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 20;
    });

    // 全局回退策略
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? retryAfterValue.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "请求过于频繁",
            message = $"请在 {retryAfter} 秒后重试",
            retryAfter
        }, cancellationToken: token);
    };
});

// ==================== OpenTelemetry ====================

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "AionNetGate.Gateway",
            serviceVersion: "2.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// ==================== 健康检查 ====================

builder.Services.AddHealthChecks();

var app = builder.Build();

// ==================== 中间件管道 ====================

// 请求日志
app.UseSerilogRequestLogging();

// 速率限制
app.UseRateLimiter();

// 健康检查端点
app.MapHealthChecks("/health");

// YARP 反向代理
app.MapReverseProxy();

// ==================== 启动信息 ====================

var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:5100";
Log.Information("========================================");
Log.Information("  AionNetGate API Gateway (YARP)");
Log.Information("  版本: 2.0.0");
Log.Information("----------------------------------------");
Log.Information("  监听地址: {Urls}", urls);
Log.Information("  健康检查: {Urls}/health", urls);
Log.Information("========================================");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway 启动失败");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

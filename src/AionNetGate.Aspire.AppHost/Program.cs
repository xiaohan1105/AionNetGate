var builder = DistributedApplication.CreateBuilder(args);

// ==================== 基础设施资源 ====================

// MySQL 数据库
var mysql = builder.AddMySql("mysql")
    .WithDataVolume("aionnetgate-mysql-data")
    .AddDatabase("aionnetgate");

// Redis 缓存
var redis = builder.AddRedis("redis")
    .WithDataVolume("aionnetgate-redis-data");

// ==================== 应用服务 ====================

// 网关主服务 (TCP 服务器)
var gateway = builder.AddProject<Projects.AionNetGate_Host>("gateway")
    .WithReference(mysql)
    .WaitFor(mysql)
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

// Web API 服务
var webapi = builder.AddProject<Projects.AionNetGate_WebApi>("webapi")
    .WithReference(mysql)
    .WaitFor(mysql)
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

// YARP API Gateway
var apigateway = builder.AddProject<Projects.AionNetGate_Gateway>("apigateway")
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithExternalHttpEndpoints();

// ==================== 构建并运行 ====================

builder.Build().Run();

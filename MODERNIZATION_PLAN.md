# AionNetGate 现代化改造方案

## 📋 执行摘要

本文档提供了 AionNetGate 项目从传统 .NET Framework 2.0 架构向现代化、高性能、高可靠性架构演进的全面规划。

**项目现状**: 基于 .NET Framework 2.0 的 WinForms 游戏网关应用，存在诸多技术债务
**目标状态**: 现代化的 .NET 8 应用，采用微服务架构、异步编程、依赖注入等现代设计模式

---

## 🔍 第一部分：技术债务分析

### 1.1 框架层面问题

#### 🔴 严重问题
1. **.NET Framework 2.0 过时**
   - 已停止官方支持（2011年）
   - 缺乏现代性能优化
   - 无法使用现代 C# 语法特性（async/await、LINQ、模式匹配等）
   - 安全漏洞风险

2. **同步阻塞 I/O 模型**
   - 使用传统的 `BeginSend/EndSend` 异步模式（APM）
   - 大量线程阻塞导致资源浪费
   - 无法充分利用现代异步 I/O（TPL、async/await）

#### 🟡 中等问题
3. **WinForms UI 与业务逻辑耦合**
   - UI 直接操作连接管理（`MainForm.Instance.RemoveClientFromList`）
   - 违反关注点分离原则
   - 难以进行单元测试
   - 无法实现无头部署（headless）

### 1.2 架构设计问题

#### 🔴 严重问题
1. **全局静态单例滥用**
   ```csharp
   internal static MainService Instance = new MainService();
   internal static DefenseService Instance = new DefenseService();
   public static MainForm Instance;
   ```
   - 违反依赖倒置原则（DIP）
   - 全局状态导致测试困难
   - 并发安全风险
   - 无法实现模块化隔离

2. **紧耦合的组件依赖**
   - `AionConnection` 直接持有 Form 引用（`DeskPictureForm`, `ProcessForm` 等）
   - 网络层与展示层强耦合
   - 服务间循环依赖

3. **缺乏抽象层**
   - 直接使用具体类型，无接口定义
   - 数据库访问代码直接嵌入服务层
   - 无法进行依赖注入和模拟测试

#### 🟡 中等问题
4. **配置管理混乱**
   - 存在两套配置系统（传统 `Config.cs` 和新的 `ConfigurationManager`）
   - 基于注册表存储配置（不便于容器化）
   - 硬编码配置值散落各处

5. **缺乏日志结构化**
   - 简单的 `LogHelper` 实现
   - 无日志级别管理
   - 无结构化日志支持
   - 无分布式追踪能力

### 1.3 安全性问题

#### 🔴 严重问题
1. **SQL 注入风险**
   ```csharp
   sql = string.Format("SELECT * FROM account_data WHERE name = '{0}'", name);
   ```
   - 大量字符串拼接 SQL
   - 虽然部分使用了参数化，但不一致

2. **简单的加密方案**
   ```csharp
   byte newbyte = (byte)(bs[i] ^ "煌".ToCharArray()[0]);
   ```
   - 使用简单 XOR 加密
   - 硬编码密钥
   - 不符合现代加密标准

3. **基础的攻击防护**
   - `DefenseService` 仅实现简单的频率限制
   - 无 DDoS 防护机制
   - 缺乏请求速率限制（Rate Limiting）
   - 无 IP 白名单/黑名单持久化

#### 🟡 中等问题
4. **异常处理不完善**
   ```csharp
   catch (Exception)
   {
       // 空catch块
   }
   ```
   - 大量空 catch 块吞噬异常
   - 异常信息丢失
   - 无异常监控和告警

### 1.4 性能和可扩展性问题

#### 🔴 严重问题
1. **连接管理效率低**
   - 使用 `Dictionary<int, LauncherInfo>` 全局存储
   - 无连接池管理
   - 无限制的连接数可能导致资源耗尽

2. **数据库连接管理**
   - 每次操作创建新连接
   - 未使用连接池（虽然 ADO.NET 有内置连接池）
   - 长时间持有连接对象

3. **内存管理**
   - `Image` 对象直接存储在连接中
   - 大量 `ref` 参数传递 Form 引用
   - 无明确的资源清理策略

#### 🟡 中等问题
4. **缺乏监控和度量**
   - 无性能指标收集
   - 无健康检查端点
   - 无可观测性（Observability）

### 1.5 代码质量问题

#### 🟡 中等问题
1. **命名不规范**
   - 目录拼写错误：`Netwok` 应为 `Network`
   - 混合使用中文和英文命名
   - 不一致的命名风格

2. **代码重复**
   - MySQL 和 MSSQL 代码大量重复
   - 相似的错误处理逻辑重复出现

3. **缺乏文档和注释**
   - XML 文档注释不完整
   - 复杂逻辑缺少说明

---

## 🎯 第二部分：现代化架构设计

### 2.1 技术栈升级

#### 核心框架
- **.NET 8** (LTS 版本，支持到 2026)
  - 跨平台支持（Windows/Linux/macOS）
  - 高性能运行时
  - 原生 AOT 编译支持
  - 现代 C# 12 语法

#### 应用框架
- **ASP.NET Core 8** - Web API 和服务托管
- **gRPC** - 高性能 RPC 通信（替代自定义二进制协议）
- **SignalR** - 实时双向通信
- **Entity Framework Core 8** - 现代 ORM

#### UI 技术（可选）
- **Blazor Hybrid** - 现代化桌面应用
- **Avalonia UI** - 跨平台 XAML UI
- **Web 管理面板** - 基于 Blazor/Vue.js

### 2.2 架构模式

#### 整体架构：微服务 + 事件驱动

```
┌─────────────────────────────────────────────────────────────┐
│                        API Gateway                          │
│                     (YARP / Ocelot)                        │
└────────────────────┬────────────────────────────────────────┘
                     │
    ┌────────────────┼────────────────┬────────────────┐
    │                │                │                │
┌───▼────┐  ┌───────▼──────┐  ┌─────▼─────┐  ┌──────▼──────┐
│ Auth   │  │ Connection   │  │  Remote   │  │   Account   │
│Service │  │   Service    │  │  Manage   │  │   Service   │
└───┬────┘  └──────┬───────┘  └─────┬─────┘  └──────┬──────┘
    │              │                 │                │
    └──────────────┼─────────────────┼────────────────┘
                   │                 │
            ┌──────▼─────────────────▼──────┐
            │     Message Bus (RabbitMQ)    │
            └───────────────────────────────┘
                   │
            ┌──────▼──────┐
            │    Redis    │
            │   (Cache)   │
            └─────────────┘
```

#### 服务划分

1. **网关服务 (Gateway Service)**
   - 统一入口
   - 路由和负载均衡
   - 认证和授权
   - 速率限制

2. **连接管理服务 (Connection Service)**
   - 客户端连接管理
   - WebSocket/gRPC 长连接
   - 心跳检测
   - 连接状态维护

3. **认证授权服务 (Auth Service)**
   - 用户认证
   - JWT Token 生成和验证
   - 权限管理
   - 密码加密和验证

4. **账号服务 (Account Service)**
   - 账号注册
   - 密码找回
   - 账号信息管理
   - 邮件通知

5. **远程管理服务 (Remote Management Service)**
   - 远程桌面查看
   - 进程监控
   - 文件浏览
   - 注册表访问
   - 服务管理

6. **防御服务 (Defense Service)**
   - IP 黑白名单
   - DDoS 防护
   - 请求速率限制
   - 异常检测

7. **监控服务 (Monitoring Service)**
   - 性能指标收集
   - 健康检查
   - 日志聚合
   - 告警通知

### 2.3 设计模式应用

#### 1. 依赖注入 (Dependency Injection)
```csharp
// 服务注册
services.AddScoped<IConnectionManager, ConnectionManager>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IDefenseService, DefenseService>();

// 使用依赖注入
public class ConnectionService
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<ConnectionService> _logger;

    public ConnectionService(
        IConnectionManager connectionManager,
        ILogger<ConnectionService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }
}
```

#### 2. 仓储模式 (Repository Pattern)
```csharp
public interface IAccountRepository
{
    Task<Account?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> CreateAsync(Account account, CancellationToken ct = default);
    Task<bool> UpdatePasswordAsync(string name, string password, CancellationToken ct = default);
}

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Name == name, ct);
    }
}
```

#### 3. 工作单元模式 (Unit of Work)
```csharp
public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    IPlayerRepository Players { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

#### 4. CQRS (命令查询职责分离)
```csharp
// 命令
public record RegisterAccountCommand(string Name, string Password, string Email);

public class RegisterAccountHandler : IRequestHandler<RegisterAccountCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;

    public async Task<Result<int>> Handle(RegisterAccountCommand request, CancellationToken ct)
    {
        // 业务逻辑
    }
}

// 查询
public record GetAccountQuery(string Name);

public class GetAccountHandler : IRequestHandler<GetAccountQuery, AccountDto?>
{
    private readonly IAccountRepository _repository;

    public async Task<AccountDto?> Handle(GetAccountQuery request, CancellationToken ct)
    {
        // 查询逻辑
    }
}
```

#### 5. 中介者模式 (Mediator Pattern)
使用 MediatR 库：
```csharp
// 控制器
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterAccountCommand(request.Name, request.Password, request.Email);
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

### 2.4 数据层设计

#### Entity Framework Core 实体设计
```csharp
public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? HardwareId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }

    // 导航属性
    public ICollection<LoginHistory> LoginHistory { get; set; } = new List<LoginHistory>();
}

public class LoginHistory
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime LoginAt { get; set; }

    public Account Account { get; set; } = null!;
}
```

#### 数据库上下文
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<LoginHistory> LoginHistory { get; set; }
    public DbSet<IPBlacklist> IPBlacklists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
        });

        // 配置关系
        modelBuilder.Entity<LoginHistory>()
            .HasOne(l => l.Account)
            .WithMany(a => a.LoginHistory)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 2.5 网络通信现代化

#### gRPC 服务定义
```protobuf
// connection.proto
syntax = "proto3";

package aiongate.v1;

service ConnectionService {
  rpc Connect(ConnectRequest) returns (ConnectResponse);
  rpc Disconnect(DisconnectRequest) returns (DisconnectResponse);
  rpc SendHeartbeat(HeartbeatRequest) returns (HeartbeatResponse);
  rpc StreamData(stream DataPacket) returns (stream DataPacket);
}

message ConnectRequest {
  string client_version = 1;
  string hardware_id = 2;
  string client_ip = 3;
}

message ConnectResponse {
  bool success = 1;
  string session_token = 2;
  string message = 3;
}

message DataPacket {
  int32 opcode = 1;
  bytes payload = 2;
  int64 timestamp = 3;
}
```

#### SignalR Hub 实现
```csharp
public class GameHub : Hub
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<GameHub> _logger;

    public GameHub(IConnectionManager connectionManager, ILogger<GameHub> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

        await _connectionManager.AddConnectionAsync(connectionId, ip);
        _logger.LogInformation("Client {ConnectionId} connected from {IP}", connectionId, ip);

        await base.OnConnectedAsync();
    }

    public async Task SendData(byte[] data)
    {
        // 处理客户端数据
        await Clients.Caller.SendAsync("ReceiveData", data);
    }
}
```

### 2.6 安全性增强

#### 1. JWT 认证
```csharp
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public string GenerateToken(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Name),
            new Claim(ClaimTypes.Email, account.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### 2. 密码哈希（使用 BCrypt）
```csharp
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

#### 3. 速率限制
```csharp
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
});
```

#### 4. SQL 注入防护
```csharp
// ✅ 正确：使用参数化查询
public async Task<Account?> GetAccountAsync(string name)
{
    return await _context.Accounts
        .FirstOrDefaultAsync(a => a.Name == name);
}

// ✅ 正确：使用存储过程
public async Task<Account?> GetAccountBySPAsync(string name)
{
    return await _context.Accounts
        .FromSqlInterpolated($"EXEC GetAccountByName {name}")
        .FirstOrDefaultAsync();
}
```

### 2.7 配置管理现代化

#### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AionGate;User Id=sa;Password=***;",
    "RedisConnection": "localhost:6379"
  },
  "JwtSettings": {
    "Key": "your-secret-key-min-32-chars-long",
    "Issuer": "AionNetGate",
    "Audience": "AionClient",
    "ExpirationHours": 24
  },
  "ServerSettings": {
    "Port": 10001,
    "MaxConnections": 10000,
    "EnableTwoIpSupport": false,
    "SecondIp": "0.0.0.0"
  },
  "SecuritySettings": {
    "EnableEnhancedSecurity": true,
    "AutoBanIp": true,
    "MaxConnectionsPerIp": 5,
    "BanDurationMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "FromEmail": "noreply@aiongate.com"
  }
}
```

#### 强类型配置
```csharp
public class ServerSettings
{
    public const string SectionName = "ServerSettings";

    public int Port { get; set; }
    public int MaxConnections { get; set; }
    public bool EnableTwoIpSupport { get; set; }
    public string SecondIp { get; set; } = "0.0.0.0";
}

// 注册配置
services.Configure<ServerSettings>(configuration.GetSection(ServerSettings.SectionName));

// 使用配置
public class ConnectionService
{
    private readonly ServerSettings _settings;

    public ConnectionService(IOptions<ServerSettings> options)
    {
        _settings = options.Value;
    }
}
```

### 2.8 日志和监控

#### 结构化日志（Serilog）
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341")  // 可选：发送到 Seq 日志服务器
    .CreateLogger();

// 使用
_logger.LogInformation("Client {ConnectionId} connected from {IP} at {Timestamp}",
    connectionId, ip, DateTime.UtcNow);
```

#### 健康检查
```csharp
services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddRedis(configuration.GetConnectionString("RedisConnection"))
    .AddCheck<ConnectionHealthCheck>("connection_health")
    .AddCheck<DiskStorageHealthCheck>("disk_storage");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

#### 应用度量（Prometheus）
```csharp
services.AddOpenTelemetryMetrics(options =>
{
    options.AddPrometheusExporter();
    options.AddMeter("AionNetGate");
    options.AddAspNetCoreInstrumentation();
});

// 自定义指标
public class ConnectionMetrics
{
    private readonly Counter<long> _connectionCounter;
    private readonly Histogram<double> _requestDuration;

    public ConnectionMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("AionNetGate");
        _connectionCounter = meter.CreateCounter<long>("connections_total");
        _requestDuration = meter.CreateHistogram<double>("request_duration_seconds");
    }

    public void RecordConnection()
    {
        _connectionCounter.Add(1);
    }
}
```

### 2.9 缓存策略

#### Redis 缓存
```csharp
public class CachedAccountRepository : IAccountRepository
{
    private readonly IAccountRepository _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedAccountRepository> _logger;

    public async Task<Account?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var cacheKey = $"account:{name}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (cached != null)
        {
            _logger.LogDebug("Cache hit for account {Name}", name);
            return JsonSerializer.Deserialize<Account>(cached);
        }

        var account = await _inner.GetByNameAsync(name, ct);

        if (account != null)
        {
            var json = JsonSerializer.Serialize(account);
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            }, ct);
        }

        return account;
    }
}
```

### 2.10 异步编程最佳实践

#### 完全异步的服务
```csharp
public class ModernConnectionService : IConnectionService
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections = new();
    private readonly ILogger<ModernConnectionService> _logger;

    public async Task<Result> AddConnectionAsync(
        string connectionId,
        string ip,
        CancellationToken ct = default)
    {
        try
        {
            // 异步验证 IP
            if (!await _defenseService.IsIpAllowedAsync(ip, ct))
            {
                return Result.Failure("IP is blocked");
            }

            var connectionInfo = new ConnectionInfo
            {
                ConnectionId = connectionId,
                IpAddress = ip,
                ConnectedAt = DateTime.UtcNow
            };

            _connections.TryAdd(connectionId, connectionInfo);

            // 异步记录日志
            await _auditService.LogConnectionAsync(connectionInfo, ct);

            _logger.LogInformation("Connection {ConnectionId} added successfully", connectionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add connection {ConnectionId}", connectionId);
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task ProcessDataAsync(
        string connectionId,
        byte[] data,
        CancellationToken ct = default)
    {
        if (!_connections.TryGetValue(connectionId, out var connection))
        {
            _logger.LogWarning("Connection {ConnectionId} not found", connectionId);
            return;
        }

        // 使用 Channels 进行异步数据处理
        await connection.DataChannel.Writer.WriteAsync(data, ct);
    }
}
```

---

## 📅 第三部分：实施路线图

### 阶段 0：准备阶段（1-2 周）

#### 目标
- 建立现代化开发环境
- 搭建基础设施
- 团队培训

#### 任务清单
- [ ] 安装 .NET 8 SDK
- [ ] 配置 Docker 和 Docker Compose
- [ ] 搭建版本控制和 CI/CD 流水线
- [ ] 设置代码质量工具（SonarQube, Roslyn Analyzers）
- [ ] 团队 C# 现代特性培训
- [ ] 设计文档评审和确认

### 阶段 1：核心基础设施（2-3 周）

#### 目标
- 创建新的 .NET 8 解决方案结构
- 实现核心抽象和接口
- 建立数据访问层

#### 任务清单
1. **项目结构**
   ```
   AionNetGate.sln
   ├── src/
   │   ├── AionNetGate.Domain/              # 领域模型
   │   ├── AionNetGate.Application/         # 应用层（业务逻辑）
   │   ├── AionNetGate.Infrastructure/      # 基础设施层
   │   ├── AionNetGate.WebApi/             # Web API
   │   └── AionNetGate.GrpcService/        # gRPC 服务
   ├── tests/
   │   ├── AionNetGate.UnitTests/
   │   ├── AionNetGate.IntegrationTests/
   │   └── AionNetGate.PerformanceTests/
   └── docker/
       ├── docker-compose.yml
       └── Dockerfile
   ```

2. **核心接口定义**
   - [ ] `IConnectionManager`
   - [ ] `IAuthService`
   - [ ] `IAccountRepository`
   - [ ] `IDefenseService`
   - [ ] `IPacketHandler`

3. **数据访问层**
   - [ ] 定义 Entity Framework Core 实体
   - [ ] 创建 DbContext
   - [ ] 实现仓储模式
   - [ ] 配置数据库迁移

4. **配置系统**
   - [ ] 迁移配置到 appsettings.json
   - [ ] 实现强类型配置类
   - [ ] 支持环境变量覆盖

### 阶段 2：认证和授权服务（2 周）

#### 目标
- 实现现代化的认证系统
- 替换旧的账号管理逻辑

#### 任务清单
- [ ] 实现 JWT Token 服务
- [ ] 密码哈希（BCrypt/Argon2）
- [ ] 账号注册 API
- [ ] 登录认证 API
- [ ] 密码找回功能
- [ ] 刷新 Token 机制
- [ ] 单元测试覆盖率 > 80%

### 阶段 3：连接管理服务（3 周）

#### 目标
- 实现高性能连接管理
- 支持 WebSocket/gRPC/SignalR

#### 任务清单
- [ ] gRPC 服务定义和实现
- [ ] SignalR Hub 实现
- [ ] 连接池管理
- [ ] 心跳检测机制
- [ ] 连接状态追踪
- [ ] 自动重连策略
- [ ] 性能测试（支持 10000+ 并发连接）

### 阶段 4：数据迁移工具（1-2 周）

#### 目标
- 平滑迁移现有数据
- 支持 MySQL 和 MSSQL

#### 任务清单
- [ ] 数据库架构映射工具
- [ ] 账号数据迁移脚本
- [ ] 数据验证和一致性检查
- [ ] 回滚机制
- [ ] 迁移文档

### 阶段 5：防御和安全服务（2 周）

#### 目标
- 增强安全防护能力
- 实现现代化攻击防护

#### 任务清单
- [ ] IP 黑白名单管理 API
- [ ] 速率限制中间件
- [ ] DDoS 防护策略
- [ ] 异常检测算法
- [ ] 安全事件日志
- [ ] 实时告警系统

### 阶段 6：远程管理服务（3-4 周）

#### 目标
- 重构远程管理功能
- 实现高效的数据传输

#### 任务清单
- [ ] 远程桌面流式传输（使用 WebRTC）
- [ ] 进程监控 API
- [ ] 文件管理 API（上传/下载/浏览）
- [ ] 注册表访问 API（带权限控制）
- [ ] 服务管理 API
- [ ] 实时数据推送

### 阶段 7：监控和可观测性（2 周）

#### 目标
- 建立完整的监控体系
- 实现可观测性

#### 任务清单
- [ ] 结构化日志（Serilog + Seq/ELK）
- [ ] 应用度量（Prometheus + Grafana）
- [ ] 分布式追踪（OpenTelemetry）
- [ ] 健康检查端点
- [ ] 自定义仪表盘
- [ ] 告警规则配置

### 阶段 8：前端现代化（2-3 周）

#### 目标
- 提供现代化管理界面
- 可选择桌面或 Web

#### 选项 A：Web 管理面板
- [ ] Blazor Server/WASM
- [ ] 或 Vue.js/React SPA
- [ ] 实时连接监控
- [ ] 账号管理界面
- [ ] 安全配置界面

#### 选项 B：桌面应用
- [ ] Avalonia UI
- [ ] WPF with MVVM
- [ ] 与后端服务通信

### 阶段 9：测试和质量保证（持续）

#### 目标
- 确保代码质量
- 高测试覆盖率

#### 任务清单
- [ ] 单元测试（目标 > 80% 覆盖率）
- [ ] 集成测试
- [ ] 性能测试
  - 并发连接测试
  - 吞吐量测试
  - 延迟测试
- [ ] 安全测试
  - 渗透测试
  - SQL 注入测试
  - XSS 测试
- [ ] 负载测试（使用 k6 或 JMeter）

### 阶段 10：部署和上线（1-2 周）

#### 目标
- 容器化部署
- 自动化运维

#### 任务清单
- [ ] Docker 镜像构建
- [ ] Docker Compose 配置
- [ ] Kubernetes 部署配置（可选）
- [ ] CI/CD 流水线
- [ ] 灰度发布策略
- [ ] 监控和告警配置
- [ ] 运维文档

### 阶段 11：遗留系统兼容（2 周）

#### 目标
- 确保平滑过渡
- 支持旧客户端

#### 任务清单
- [ ] 协议适配层（支持旧的二进制协议）
- [ ] 双栈运行模式（新旧系统并行）
- [ ] 数据同步机制
- [ ] 逐步迁移策略
- [ ] 回滚方案

---

## 🛠️ 第四部分：技术实现细节

### 4.1 项目模板和代码生成器

#### Scaffold 新服务
```bash
# 使用 .NET CLI 创建新的 Web API 项目
dotnet new webapi -n AionNetGate.AuthService -o src/AionNetGate.AuthService

# 添加必要的包
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Serilog.AspNetCore
dotnet add package MediatR
```

#### 代码生成器脚本
```powershell
# CreateService.ps1 - 生成微服务模板
param(
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
)

$ServiceNamespace = "AionNetGate.$ServiceName"
$ServicePath = "src/$ServiceNamespace"

# 创建项目结构
dotnet new webapi -n $ServiceNamespace -o $ServicePath
dotnet sln add $ServicePath

# 添加标准依赖
Push-Location $ServicePath
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Serilog.AspNetCore
dotnet add package MediatR
Pop-Location

Write-Host "Service $ServiceName created successfully!" -ForegroundColor Green
```

### 4.2 Docker 配置

#### Dockerfile
```dockerfile
# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["src/AionNetGate.WebApi/AionNetGate.WebApi.csproj", "src/AionNetGate.WebApi/"]
COPY ["src/AionNetGate.Application/AionNetGate.Application.csproj", "src/AionNetGate.Application/"]
COPY ["src/AionNetGate.Domain/AionNetGate.Domain.csproj", "src/AionNetGate.Domain/"]
COPY ["src/AionNetGate.Infrastructure/AionNetGate.Infrastructure.csproj", "src/AionNetGate.Infrastructure/"]

# 还原依赖
RUN dotnet restore "src/AionNetGate.WebApi/AionNetGate.WebApi.csproj"

# 复制所有源代码
COPY . .

# 构建
WORKDIR "/src/src/AionNetGate.WebApi"
RUN dotnet build "AionNetGate.WebApi.csproj" -c Release -o /app/build

# 发布
FROM build AS publish
RUN dotnet publish "AionNetGate.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 10001

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AionNetGate.WebApi.dll"]
```

#### docker-compose.yml
```yaml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourStrong@Passw0rd
      MSSQL_PID: Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - aiongate-network

  # Redis
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - aiongate-network

  # RabbitMQ
  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - aiongate-network

  # Seq (日志服务器)
  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
    environment:
      ACCEPT_EULA: Y
    volumes:
      - seq-data:/data
    networks:
      - aiongate-network

  # 认证服务
  auth-service:
    build:
      context: .
      dockerfile: src/AionNetGate.AuthService/Dockerfile
    ports:
      - "5001:80"
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=AionGate;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
      ConnectionStrings__RedisConnection: "redis:6379"
      Serilog__WriteTo__1__Args__serverUrl: "http://seq:5341"
    depends_on:
      - sqlserver
      - redis
      - seq
    networks:
      - aiongate-network

  # 连接管理服务
  connection-service:
    build:
      context: .
      dockerfile: src/AionNetGate.ConnectionService/Dockerfile
    ports:
      - "5002:80"
      - "10001:10001"
    environment:
      ConnectionStrings__RedisConnection: "redis:6379"
      RabbitMQ__Host: "rabbitmq"
      Serilog__WriteTo__1__Args__serverUrl: "http://seq:5341"
    depends_on:
      - redis
      - rabbitmq
      - seq
    networks:
      - aiongate-network

  # API 网关
  api-gateway:
    build:
      context: .
      dockerfile: src/AionNetGate.ApiGateway/Dockerfile
    ports:
      - "5000:80"
    environment:
      Routes__AuthService: "http://auth-service"
      Routes__ConnectionService: "http://connection-service"
      Serilog__WriteTo__1__Args__serverUrl: "http://seq:5341"
    depends_on:
      - auth-service
      - connection-service
    networks:
      - aiongate-network

  # Prometheus (监控)
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
    networks:
      - aiongate-network

  # Grafana (可视化)
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      GF_SECURITY_ADMIN_PASSWORD: admin
    volumes:
      - grafana-data:/var/lib/grafana
    depends_on:
      - prometheus
    networks:
      - aiongate-network

volumes:
  sqlserver-data:
  redis-data:
  rabbitmq-data:
  seq-data:
  prometheus-data:
  grafana-data:

networks:
  aiongate-network:
    driver: bridge
```

### 4.3 CI/CD 配置

#### GitHub Actions
```yaml
# .github/workflows/build-and-test.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"

    - name: Code Coverage Report
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'

    - name: SonarCloud Scan
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}

    - name: Build Docker Images
      run: |
        docker build -t aiongate/auth-service:${{ github.sha }} -f src/AionNetGate.AuthService/Dockerfile .
        docker build -t aiongate/connection-service:${{ github.sha }} -f src/AionNetGate.ConnectionService/Dockerfile .

    - name: Push to Docker Hub
      if: github.ref == 'refs/heads/main'
      run: |
        echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
        docker push aiongate/auth-service:${{ github.sha }}
        docker push aiongate/connection-service:${{ github.sha }}
```

### 4.4 数据库迁移策略

#### 迁移脚本示例
```csharp
public class InitialMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 账号表
        migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 50, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 255, nullable: false),
                Email = table.Column<string>(maxLength: 100, nullable: true),
                HardwareId = table.Column<string>(maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                LastLoginAt = table.Column<DateTime>(nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Accounts", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Accounts_Name",
            table: "Accounts",
            column: "Name",
            unique: true);

        // 登录历史表
        migrationBuilder.CreateTable(
            name: "LoginHistory",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountId = table.Column<int>(nullable: false),
                IpAddress = table.Column<string>(maxLength: 45, nullable: false),
                Location = table.Column<string>(maxLength: 200, nullable: true),
                LoginAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LoginHistory", x => x.Id);
                table.ForeignKey(
                    name: "FK_LoginHistory_Accounts_AccountId",
                    column: x => x.AccountId,
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // IP 黑名单表
        migrationBuilder.CreateTable(
            name: "IPBlacklists",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                IpAddress = table.Column<string>(maxLength: 45, nullable: false),
                Reason = table.Column<string>(maxLength: 500, nullable: true),
                BlockedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                ExpiresAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IPBlacklists", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IPBlacklists_IpAddress",
            table: "IPBlacklists",
            column: "IpAddress");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LoginHistory");
        migrationBuilder.DropTable(name: "IPBlacklists");
        migrationBuilder.DropTable(name: "Accounts");
    }
}
```

#### 数据迁移工具
```csharp
public class LegacyDataMigrator
{
    private readonly ILogger<LegacyDataMigrator> _logger;
    private readonly AppDbContext _newDb;
    private readonly string _oldDbConnectionString;

    public async Task MigrateAccountsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting account migration...");

        using var oldConnection = new SqlConnection(_oldDbConnectionString);
        await oldConnection.OpenAsync(ct);

        var command = new SqlCommand("SELECT * FROM account_data", oldConnection);
        using var reader = await command.ExecuteReaderAsync(ct);

        var migratedCount = 0;
        var errorCount = 0;

        while (await reader.ReadAsync(ct))
        {
            try
            {
                var account = new Account
                {
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("password")),
                    Email = reader.IsDBNull(reader.GetOrdinal("email"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("email")),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("create_time"))
                        ? DateTime.UtcNow
                        : reader.GetDateTime(reader.GetOrdinal("create_time")),
                    IsActive = true
                };

                _newDb.Accounts.Add(account);
                migratedCount++;

                if (migratedCount % 1000 == 0)
                {
                    await _newDb.SaveChangesAsync(ct);
                    _logger.LogInformation("Migrated {Count} accounts...", migratedCount);
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogError(ex, "Error migrating account");
            }
        }

        await _newDb.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Account migration completed. Migrated: {Migrated}, Errors: {Errors}",
            migratedCount,
            errorCount);
    }
}
```

---

## 📊 第五部分：性能优化

### 5.1 异步 I/O 优化

#### 使用 Pipelines
```csharp
public class PipelinePacketProcessor
{
    private readonly Pipe _pipe = new Pipe();
    private readonly IPacketHandler _handler;

    public async Task ProcessConnectionAsync(NetworkStream stream, CancellationToken ct)
    {
        var readTask = ReadFromStreamAsync(stream, _pipe.Writer, ct);
        var processTask = ProcessPacketsAsync(_pipe.Reader, ct);

        await Task.WhenAll(readTask, processTask);
    }

    private async Task ReadFromStreamAsync(NetworkStream stream, PipeWriter writer, CancellationToken ct)
    {
        const int minimumBufferSize = 512;

        while (!ct.IsCancellationRequested)
        {
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);

            try
            {
                int bytesRead = await stream.ReadAsync(memory, ct);
                if (bytesRead == 0)
                    break;

                writer.Advance(bytesRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from stream");
                break;
            }

            FlushResult result = await writer.FlushAsync(ct);

            if (result.IsCompleted)
                break;
        }

        await writer.CompleteAsync();
    }

    private async Task ProcessPacketsAsync(PipeReader reader, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            ReadResult result = await reader.ReadAsync(ct);
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryParsePacket(ref buffer, out ReadOnlySequence<byte> packet))
            {
                await _handler.HandlePacketAsync(packet, ct);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }

    private bool TryParsePacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        if (buffer.Length < 4)
        {
            packet = default;
            return false;
        }

        // 读取包大小（前4字节）
        Span<byte> lengthBytes = stackalloc byte[4];
        buffer.Slice(0, 4).CopyTo(lengthBytes);
        int packetLength = BitConverter.ToInt32(lengthBytes);

        if (buffer.Length < packetLength)
        {
            packet = default;
            return false;
        }

        packet = buffer.Slice(0, packetLength);
        buffer = buffer.Slice(packetLength);
        return true;
    }
}
```

### 5.2 内存优化

#### 使用 ArrayPool
```csharp
public class OptimizedPacketHandler
{
    private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

    public async Task HandleLargeDataAsync(ReadOnlySequence<byte> data, CancellationToken ct)
    {
        byte[] buffer = _arrayPool.Rent((int)data.Length);

        try
        {
            data.CopyTo(buffer);

            // 处理数据
            await ProcessDataAsync(buffer.AsMemory(0, (int)data.Length), ct);
        }
        finally
        {
            _arrayPool.Return(buffer);
        }
    }
}
```

#### 使用 Span<T> 和 Memory<T>
```csharp
public class ZeroCopyPacketParser
{
    public PacketInfo ParsePacket(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            throw new ArgumentException("Invalid packet size");

        // 零拷贝读取前4字节（包长度）
        int length = BitConverter.ToInt32(data);

        // 零拷贝读取opcode
        byte opcode = data[4];

        // 零拷贝获取payload
        ReadOnlySpan<byte> payload = data.Slice(5);

        return new PacketInfo
        {
            Length = length,
            Opcode = opcode,
            Payload = payload.ToArray() // 仅在必要时才复制
        };
    }
}
```

### 5.3 并发优化

#### 使用 Channels 进行生产者-消费者模式
```csharp
public class PacketQueue
{
    private readonly Channel<Packet> _channel;
    private readonly IPacketProcessor _processor;

    public PacketQueue(int capacity = 10000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<Packet>(options);
    }

    public async ValueTask EnqueueAsync(Packet packet, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(packet, ct);
    }

    public async Task StartProcessingAsync(int workerCount, CancellationToken ct)
    {
        var workers = Enumerable.Range(0, workerCount)
            .Select(i => ProcessPacketsAsync(i, ct))
            .ToArray();

        await Task.WhenAll(workers);
    }

    private async Task ProcessPacketsAsync(int workerId, CancellationToken ct)
    {
        await foreach (var packet in _channel.Reader.ReadAllAsync(ct))
        {
            try
            {
                await _processor.ProcessAsync(packet, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker {WorkerId} failed to process packet", workerId);
            }
        }
    }
}
```

### 5.4 数据库性能优化

#### 批量操作
```csharp
public class OptimizedAccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public async Task BulkInsertAsync(IEnumerable<Account> accounts, CancellationToken ct = default)
    {
        // 使用 EF Core 的 BulkExtensions
        await _context.BulkInsertAsync(accounts, cancellationToken: ct);
    }

    public async Task<List<Account>> GetActiveAccountsAsync(CancellationToken ct = default)
    {
        // 使用编译查询提升性能
        return await CompiledQueries.GetActiveAccounts(_context, ct);
    }
}

public static class CompiledQueries
{
    public static readonly Func<AppDbContext, CancellationToken, Task<List<Account>>> GetActiveAccounts =
        EF.CompileAsyncQuery((AppDbContext context, CancellationToken ct) =>
            context.Accounts.Where(a => a.IsActive).ToList());
}
```

#### 连接池配置
```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});
```

---

## 🔒 第六部分：安全最佳实践

### 6.1 安全检查清单

#### 应用层安全
- [x] 使用 HTTPS (TLS 1.2+)
- [x] 实施 JWT 认证和授权
- [x] 密码使用强哈希算法（BCrypt/Argon2）
- [x] 实施速率限制
- [x] 输入验证和清理
- [x] 参数化查询防止 SQL 注入
- [x] XSS 防护
- [x] CSRF 防护
- [x] 安全的会话管理

#### 网络层安全
- [x] IP 白名单/黑名单
- [x] DDoS 防护
- [x] 请求大小限制
- [x] 连接数限制
- [x] 超时配置

#### 数据安全
- [x] 数据加密（传输和静态）
- [x] 敏感数据脱敏
- [x] 审计日志
- [x] 数据备份和恢复

### 6.2 安全编码示例

#### 输入验证
```csharp
public class RegisterAccountRequest
{
    [Required]
    [StringLength(50, MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "仅允许字母、数字和下划线")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
}

public class RegisterAccountValidator : AbstractValidator<RegisterAccountRequest>
{
    public RegisterAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("账号名不能为空")
            .Length(4, 50).WithMessage("账号名长度必须在4-50字符之间")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("账号名仅允许字母、数字和下划线")
            .MustAsync(BeUniqueNameAsync).WithMessage("账号名已存在");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(8).WithMessage("密码至少8个字符")
            .Matches(@"[A-Z]").WithMessage("密码必须包含至少一个大写字母")
            .Matches(@"[a-z]").WithMessage("密码必须包含至少一个小写字母")
            .Matches(@"[0-9]").WithMessage("密码必须包含至少一个数字")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("密码必须包含至少一个特殊字符");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");
    }

    private async Task<bool> BeUniqueNameAsync(string name, CancellationToken ct)
    {
        // 检查账号名是否已存在
        return !await _accountRepository.ExistsAsync(name, ct);
    }
}
```

#### API 授权
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("connections")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetConnections()
    {
        // 仅管理员可访问
        return Ok(await _connectionService.GetAllConnectionsAsync());
    }

    [HttpPost("ban-ip")]
    [Authorize(Policy = "CanManageSecurity")]
    public async Task<IActionResult> BanIp([FromBody] BanIpRequest request)
    {
        // 需要安全管理权限
        await _defenseService.BanIpAsync(request.IpAddress, request.Reason);
        return Ok();
    }
}
```

---

## 📈 第七部分：监控和运维

### 7.1 关键指标

#### 应用指标
- 活跃连接数
- 请求吞吐量（RPS）
- 平均响应时间
- 错误率
- CPU 和内存使用率

#### 业务指标
- 在线用户数
- 新注册账号数
- 登录成功/失败率
- 被封禁 IP 数量

### 7.2 告警规则

```yaml
# Prometheus 告警规则
groups:
  - name: aiongate_alerts
    interval: 30s
    rules:
      # 高错误率
      - alert: HighErrorRate
        expr: rate(http_requests_errors_total[5m]) > 0.05
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "高错误率检测"
          description: "服务 {{ $labels.service }} 错误率超过 5%"

      # 高响应时间
      - alert: HighResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "响应时间过长"
          description: "95% 请求响应时间超过 1 秒"

      # 服务不可用
      - alert: ServiceDown
        expr: up{job="aiongate"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "服务宕机"
          description: "服务 {{ $labels.instance }} 已宕机"

      # 数据库连接池耗尽
      - alert: DbConnectionPoolExhausted
        expr: db_connection_pool_used / db_connection_pool_size > 0.9
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "数据库连接池即将耗尽"
          description: "连接池使用率超过 90%"
```

### 7.3 日志聚合

#### ELK Stack 配置
```yaml
# docker-compose.override.yml
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    networks:
      - aiongate-network

  logstash:
    image: docker.elastic.co/logstash/logstash:8.11.0
    volumes:
      - ./logstash/config:/usr/share/logstash/pipeline
    ports:
      - "5000:5000"
    depends_on:
      - elasticsearch
    networks:
      - aiongate-network

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    ports:
      - "5601:5601"
    environment:
      ELASTICSEARCH_URL: http://elasticsearch:9200
    depends_on:
      - elasticsearch
    networks:
      - aiongate-network
```

---

## 🎓 第八部分：团队培训和文档

### 8.1 培训计划

#### 第一周：.NET 8 基础
- 新的 C# 12 特性
- async/await 深入理解
- Span<T> 和 Memory<T>
- System.Threading.Channels

#### 第二周：架构模式
- 依赖注入
- CQRS 和 MediatR
- 仓储模式
- 领域驱动设计基础

#### 第三周：微服务实践
- Docker 和容器化
- gRPC 和 SignalR
- 消息队列（RabbitMQ）
- API 网关

#### 第四周：运维和监控
- Prometheus 和 Grafana
- 结构化日志（Serilog）
- CI/CD 流水线
- 故障排查

### 8.2 文档结构

```
docs/
├── architecture/
│   ├── system-overview.md
│   ├── microservices-design.md
│   ├── data-flow.md
│   └── security-architecture.md
├── api/
│   ├── authentication-api.md
│   ├── connection-api.md
│   ├── account-api.md
│   └── grpc-services.md
├── deployment/
│   ├── docker-deployment.md
│   ├── kubernetes-deployment.md
│   └── monitoring-setup.md
├── development/
│   ├── coding-standards.md
│   ├── git-workflow.md
│   ├── testing-guide.md
│   └── troubleshooting.md
└── operations/
    ├── runbook.md
    ├── incident-response.md
    ├── backup-restore.md
    └── performance-tuning.md
```

---

## ✅ 第九部分：验收标准

### 9.1 功能验收

- [ ] 所有原有功能正常工作
- [ ] 用户认证和授权正确
- [ ] 连接管理稳定
- [ ] 远程管理功能可用
- [ ] 数据一致性检查通过

### 9.2 性能验收

- [ ] 支持 10000+ 并发连接
- [ ] API 响应时间 < 100ms (P95)
- [ ] 吞吐量 > 10000 RPS
- [ ] CPU 使用率 < 70%
- [ ] 内存使用率 < 80%

### 9.3 质量验收

- [ ] 代码覆盖率 > 80%
- [ ] 无严重安全漏洞
- [ ] 无内存泄漏
- [ ] 日志完整且可查询
- [ ] 文档完整

### 9.4 运维验收

- [ ] 自动化部署流程
- [ ] 监控和告警配置完成
- [ ] 备份和恢复流程测试通过
- [ ] 灾难恢复计划就位

---

## 📝 第十部分：风险和缓解措施

### 10.1 技术风险

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|----------|
| 新技术栈学习曲线陡峭 | 高 | 中 | 提供充分培训，渐进式迁移 |
| 性能不达标 | 中 | 高 | 早期性能测试，预留优化时间 |
| 数据迁移失败 | 中 | 高 | 完整的备份和回滚计划 |
| 兼容性问题 | 中 | 中 | 保留适配层，逐步迁移 |

### 10.2 业务风险

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|----------|
| 服务中断 | 低 | 高 | 蓝绿部署，灰度发布 |
| 用户流失 | 低 | 高 | 保持向后兼容，平滑过渡 |
| 预算超支 | 中 | 中 | 严格的项目管理，阶段性交付 |

### 10.3 应急预案

#### 回滚策略
1. 数据库快照和备份
2. 保留旧系统运行
3. 流量切换机制
4. 快速回滚脚本

#### 灾难恢复
1. 异地备份
2. 热备数据库
3. 负载均衡和故障转移
4. 定期演练

---

## 🎯 总结

这份全面的现代化改造方案涵盖了从技术债务分析、架构设计、实施路线图到运维监控的完整流程。

### 核心亮点

1. **渐进式升级**：采用分阶段实施，降低风险
2. **现代化架构**：微服务、异步编程、事件驱动
3. **高性能**：Pipelines、Channels、内存优化
4. **高可靠性**：完善的监控、日志、告警体系
5. **安全性**：JWT、速率限制、多层防护
6. **可扩展性**：容器化、微服务、云原生

### 预期收益

- **性能提升**：10倍以上吞吐量提升
- **可维护性**：模块化设计，易于扩展
- **可观测性**：完整的监控和日志体系
- **安全性**：现代化的安全防护机制
- **团队效能**：现代化开发工具和流程

### 下一步行动

1. 评审和确认本方案
2. 组建项目团队
3. 启动第一阶段实施
4. 定期回顾和调整

---

**文档版本**: 1.0
**创建日期**: 2025-01-11
**作者**: Claude (Anthropic AI)
**审核状态**: 待审核

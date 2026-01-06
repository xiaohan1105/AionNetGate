# AionNetGate.Modern - 单体应用架构设计

## 一、项目愿景

将现有的 AionNetGate 网关项目现代化为基于 **.NET 9 的单体应用**，具备：

1. **高性能网络** - 异步 TCP 服务器，支持万级并发
2. **分层架构** - 清晰的代码组织，便于测试和维护
3. **多数据库支持** - SQLite/MySQL/MSSQL
4. **现代化配置** - appsettings.json + 环境变量

---

## 二、架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                    AionNetGate.Modern (单体应用)                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                    AionNetGate.Host                      │   │
│   │                   (Worker Service)                       │   │
│   │                      入口点                               │   │
│   └─────────────────────────────────────────────────────────┘   │
│                              │                                   │
│   ═══════════════════════════╪═══════════════════════════════   │
│                              ▼                                   │
│   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐        │
│   │ Application │    │   Network   │    │Infrastructure│        │
│   │    Layer    │    │    Layer    │    │    Layer    │        │
│   │             │    │             │    │             │        │
│   │ • Services  │    │ • TCP Server│    │ • DbContext │        │
│   │ • Handlers  │    │ • Protocols │    │ • Repository│        │
│   │             │    │ • Packets   │    │ • Security  │        │
│   └──────┬──────┘    └──────┬──────┘    └──────┬──────┘        │
│          │                  │                  │                │
│          └──────────────────┼──────────────────┘                │
│                             ▼                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                    AionNetGate.Core                      │   │
│   │                      核心领域层                           │   │
│   │                                                         │   │
│   │  ┌──────────┐  ┌──────────┐  ┌──────────┐              │   │
│   │  │ Entities │  │Interfaces│  │  Config  │              │   │
│   │  │          │  │          │  │          │              │   │
│   │  │• Account │  │• IRepo   │  │• Server  │              │   │
│   │  │• Session │  │• IService│  │• Database│              │   │
│   │  │• IpBlock │  │• IHash   │  │• Security│              │   │
│   │  └──────────┘  └──────────┘  └──────────┘              │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                               │
    ═══════════════════════════╪═══════════════════════════════════
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                          数据存储层                               │
│                                                                 │
│   ┌───────────────┐  ┌───────────────┐  ┌───────────────┐      │
│   │    SQLite     │  │     MySQL     │  │    MSSQL      │      │
│   │   (开发环境)   │  │   (生产可选)   │  │  (游戏主库)    │      │
│   └───────────────┘  └───────────────┘  └───────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                               │
    ═══════════════════════════╪═══════════════════════════════════
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                        游戏启动器客户端                           │
│                                                                 │
│   ┌───────────────────────────────────────────────────────┐    │
│   │                   AionLauncher                         │    │
│   │                                                       │    │
│   │  • 连接网关                                            │    │
│   │  • 文件校验                                            │    │
│   │  • 外挂检测                                            │    │
│   │  • 账号登录                                            │    │
│   └───────────────────────────────────────────────────────┘    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 三、技术栈

| 组件 | 技术选型 | 说明 |
|------|----------|------|
| **运行时** | .NET 9 | 最新 LTS 版本 |
| **主机模型** | Worker Service | 后台服务，无 UI |
| **ORM** | EF Core 9 | 多数据库支持 |
| **日志** | Serilog | 结构化日志 |
| **配置** | appsettings.json | 支持环境变量覆盖 |
| **DI** | Microsoft.Extensions.DependencyInjection | 内置依赖注入 |

---

## 四、项目结构

```
AionNetGate.Modern.sln
│
├── src/
│   │
│   ├── AionNetGate.Core/               # 核心领域层 (无依赖)
│   │   ├── Domain/
│   │   │   └── Entities/
│   │   │       ├── Account.cs          # 账号实体
│   │   │       ├── Session.cs          # 会话实体
│   │   │       ├── HardwareFingerprint.cs
│   │   │       └── IpBlacklist.cs
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs          # 通用仓储接口
│   │   │   ├── IAccountRepository.cs
│   │   │   ├── ISessionRepository.cs
│   │   │   ├── IPasswordHasher.cs
│   │   │   └── IEncryptionService.cs
│   │   │
│   │   ├── Configuration/
│   │   │   ├── ServerConfig.cs
│   │   │   ├── DatabaseConfig.cs
│   │   │   ├── SecurityConfig.cs
│   │   │   └── LoggingConfig.cs
│   │   │
│   │   └── Results/
│   │       ├── Result.cs               # Result 模式
│   │       └── Error.cs
│   │
│   ├── AionNetGate.Application/        # 应用服务层
│   │   └── Services/
│   │       ├── IAccountService.cs
│   │       ├── AccountService.cs
│   │       ├── ISessionService.cs
│   │       └── SessionService.cs
│   │
│   ├── AionNetGate.Infrastructure/     # 基础设施层
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs         # EF Core DbContext
│   │   │   └── Configurations/         # 实体配置
│   │   │
│   │   ├── Repositories/
│   │   │   ├── AccountRepository.cs
│   │   │   ├── SessionRepository.cs
│   │   │   └── IpBlacklistRepository.cs
│   │   │
│   │   └── Security/
│   │       ├── Argon2PasswordHasher.cs
│   │       └── AesEncryptionService.cs
│   │
│   ├── AionNetGate.Network/            # 网络通信层
│   │   ├── Server/
│   │   │   ├── TcpGatewayServer.cs     # TCP 服务器
│   │   │   └── ClientConnection.cs     # 客户端连接
│   │   │
│   │   ├── Protocols/
│   │   │   ├── Packet.cs               # 数据包基类
│   │   │   ├── PacketReader.cs
│   │   │   └── PacketWriter.cs
│   │   │
│   │   └── Handlers/
│   │       ├── IPacketHandler.cs
│   │       ├── ConnectionHandler.cs
│   │       ├── AccountHandler.cs
│   │       └── PingHandler.cs
│   │
│   └── AionNetGate.Host/               # 主机入口
│       ├── Program.cs                  # 入口点
│       ├── GatewayWorker.cs            # 后台服务
│       ├── appsettings.json
│       └── appsettings.Development.json
│
└── tests/
    ├── AionNetGate.UnitTests/
    └── AionNetGate.IntegrationTests/
```

---

## 五、分层职责

### 5.1 Core 层 (AionNetGate.Core)

**职责**: 定义领域实体、接口和配置，不依赖任何外部库

```csharp
// 实体示例
public class Account
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 接口示例
public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByUsernameAsync(string username);
    Task<bool> ExistsAsync(string username);
}
```

### 5.2 Application 层 (AionNetGate.Application)

**职责**: 业务逻辑和用例实现

```csharp
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepo;
    private readonly IPasswordHasher _passwordHasher;

    public async Task<Result<Account>> LoginAsync(string username, string password)
    {
        var account = await _accountRepo.GetByUsernameAsync(username);
        if (account == null)
            return Result<Account>.Failure(Error.NotFound("Account not found"));

        if (!_passwordHasher.Verify(password, account.PasswordHash))
            return Result<Account>.Failure(Error.Unauthorized("Invalid password"));

        return Result<Account>.Success(account);
    }
}
```

### 5.3 Infrastructure 层 (AionNetGate.Infrastructure)

**职责**: 数据访问、外部服务集成

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<IpBlacklist> IpBlacklist => Set<IpBlacklist>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### 5.4 Network 层 (AionNetGate.Network)

**职责**: TCP 服务器和协议处理

```csharp
public class TcpGatewayServer
{
    private readonly TcpListener _listener;
    private readonly IServiceProvider _services;

    public async Task StartAsync(CancellationToken ct)
    {
        _listener.Start();
        while (!ct.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(ct);
            _ = HandleClientAsync(client, ct);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using var connection = new ClientConnection(client, _services);
        await connection.ProcessAsync(ct);
    }
}
```

### 5.5 Host 层 (AionNetGate.Host)

**职责**: 组装和启动应用

```csharp
var builder = Host.CreateApplicationBuilder(args);

// 配置
builder.Services.Configure<ServerConfig>(
    builder.Configuration.GetSection("Server"));
builder.Services.Configure<DatabaseConfig>(
    builder.Configuration.GetSection("Database"));

// 依赖注入
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddSingleton<TcpGatewayServer>();
builder.Services.AddHostedService<GatewayWorker>();

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

await builder.Build().RunAsync();
```

---

## 六、配置文件

### appsettings.json

```json
{
  "Server": {
    "ListenAddress": "0.0.0.0",
    "ListenPort": 9000,
    "MaxConnections": 10000,
    "ConnectionTimeout": 300
  },
  "Database": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=aiongate.db"
  },
  "Security": {
    "EnableIpBlacklist": true,
    "MaxLoginAttempts": 5,
    "LockoutDuration": 300,
    "EnableHardwareFingerprint": true
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/gateway-.log", "rollingInterval": "Day" } }
    ]
  }
}
```

### 多数据库支持

```json
// SQLite (开发环境)
{
  "Database": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=aiongate.db"
  }
}

// MySQL (生产环境)
{
  "Database": {
    "Provider": "MySql",
    "ConnectionString": "Server=localhost;Database=aiongate;User=root;Password=xxx"
  }
}

// MSSQL (游戏主库)
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=aion_gs;User=sa;Password=xxx"
  }
}
```

---

## 七、通信协议

### 数据包格式

```
┌────────────────────────────────────────┐
│ [Length:4字节] [Opcode:1字节] [Data:N字节] │
└────────────────────────────────────────┘

加密方式: XOR ^ 0x714C ("煌")
```

### Opcode 定义

| Opcode | 方向 | 说明 |
|--------|------|------|
| 0x00 | C→S / S→C | 连接握手 |
| 0x01 | C→S / S→C | 账号操作 |
| 0x02 | C→S / S→C | 桌面截图 |
| 0x03 | C→S / S→C | 进程列表 |
| 0x04 | C→S / S→C | 电脑信息 |
| 0x05 | C→S / S→C | Ping/Pong |
| 0x06 | C→S / S→C | 外挂检测 |
| 0x07 | C→S / S→C | 文件列表 |
| 0x08 | C→S / S→C | 注册表 |
| 0x09 | C→S / S→C | 服务列表 |

---

## 八、构建和运行

### 开发环境

```bash
# 恢复依赖
dotnet restore AionNetGate.Modern.sln

# 构建
dotnet build

# 运行
dotnet run --project src/AionNetGate.Host

# 运行测试
dotnet test
```

### 发布部署

```bash
# 发布单文件
dotnet publish src/AionNetGate.Host \
    -c Release \
    -r win-x64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -o publish

# 输出
publish/
└── AionNetGate.Host.exe  # 单个可执行文件
```

### 作为 Windows 服务运行

```bash
# 安装服务
sc create AionNetGate binPath= "C:\path\to\AionNetGate.Host.exe"

# 启动
sc start AionNetGate

# 停止
sc stop AionNetGate
```

---

## 九、优势对比

| 特性 | 传统项目 | Modern 单体 |
|------|---------|-------------|
| 框架 | .NET Framework 2.0 | .NET 9 |
| 架构 | 无分层 | Clean Architecture |
| 配置 | 注册表 | appsettings.json |
| 日志 | 自定义 | Serilog |
| 数据库 | MySQL/MSSQL | SQLite/MySQL/MSSQL |
| 测试 | 无 | 单元测试 + 集成测试 |
| 部署 | 手动 | 单文件 / Windows 服务 |
| 性能 | 一般 | 高性能异步 I/O |

---

## 十、扩展计划

未来可添加的功能：

1. **Web API** - 添加 ASP.NET Core 支持管理接口
2. **SignalR** - 实时推送在线状态
3. **WPF 管理界面** - 使用 AionNetGate.Admin.WPF
4. **监控指标** - Prometheus + Grafana

这些都可以在单体应用中逐步添加，无需拆分为微服务。

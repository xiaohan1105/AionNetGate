# AionNetGate 现代化架构设计

> 版本: 2.0 | 作者: Claude Architect | 日期: 2025-11

## 一、架构愿景

### 目标
- **开服简单**: 一键部署，配置向导，自动化运维
- **运行可靠**: 高可用，自动恢复，完善监控
- **安全可信**: 现代加密，防注入，审计追踪
- **易于维护**: 模块化，可测试，文档完善

### 设计原则
1. **关注点分离** - 每个模块只做一件事
2. **依赖倒置** - 依赖抽象而非具体实现
3. **配置外部化** - 零硬编码，环境隔离
4. **安全默认** - 安全选项默认开启
5. **可观测性** - 日志、指标、追踪三位一体

---

## 二、系统总览架构

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              AionGate Platform 2.0                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                         管理层 (Management Plane)                        │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  │    │
│  │  │  Web 控制台   │  │  CLI 工具    │  │  REST API   │  │ 配置向导    │  │    │
│  │  │  (Vue.js)    │  │  (aionctl)   │  │  (ASP.NET)  │  │ (Wizard)   │  │    │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────┘  │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                        │                                         │
│                                        ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                          核心层 (Core Services)                          │    │
│  │                                                                          │    │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐             │    │
│  │  │  Gateway       │  │  Auth          │  │  Launcher      │             │    │
│  │  │  Service       │  │  Service       │  │  Builder       │             │    │
│  │  │                │  │                │  │                │             │    │
│  │  │ • 连接管理     │  │ • 账号认证     │  │ • UI 设计器    │             │    │
│  │  │ • 协议处理     │  │ • 权限管理     │  │ • 动态编译     │             │    │
│  │  │ • 负载均衡     │  │ • JWT 令牌     │  │ • 皮肤系统     │             │    │
│  │  │ • 会话管理     │  │ • 2FA 支持     │  │ • 版本管理     │             │    │
│  │  └───────┬────────┘  └───────┬────────┘  └───────┬────────┘             │    │
│  │          │                   │                   │                       │    │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐             │    │
│  │  │  Defense       │  │  Monitor       │  │  Remote        │             │    │
│  │  │  Service       │  │  Service       │  │  Admin         │             │    │
│  │  │                │  │                │  │                │             │    │
│  │  │ • DDoS 防护    │  │ • 健康检查     │  │ • 桌面查看     │             │    │
│  │  │ • 速率限制     │  │ • 性能指标     │  │ • 进程管理     │             │    │
│  │  │ • IP 黑名单    │  │ • 告警系统     │  │ • 文件浏览     │             │    │
│  │  │ • 行为分析     │  │ • 审计日志     │  │ • 远程命令     │             │    │
│  │  └───────┬────────┘  └───────┬────────┘  └───────┬────────┘             │    │
│  │          │                   │                   │                       │    │
│  └──────────┴───────────────────┴───────────────────┴───────────────────────┘    │
│                                        │                                         │
│                                        ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                        基础设施层 (Infrastructure)                       │    │
│  │                                                                          │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │    │
│  │  │ Database    │  │ Cache       │  │ Message     │  │ Config          │ │    │
│  │  │ (MySQL/PG)  │  │ (Redis)     │  │ Queue       │  │ (Consul/File)   │ │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────────┘ │    │
│  │                                                                          │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │    │
│  │  │ Logging     │  │ Metrics     │  │ Tracing     │  │ Secrets         │ │    │
│  │  │ (Serilog)   │  │ (Prometheus)│  │ (Jaeger)    │  │ (Vault/DPAPI)   │ │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────────┘ │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 三、模块化服务设计

### 3.1 项目结构

```
AionGate/
├── src/
│   ├── AionGate.Core/              # 核心抽象层
│   │   ├── Interfaces/             # 服务接口定义
│   │   ├── Models/                 # 领域模型
│   │   ├── Events/                 # 事件定义
│   │   └── Extensions/             # 扩展方法
│   │
│   ├── AionGate.Gateway/           # 网关服务
│   │   ├── Network/                # 网络层
│   │   │   ├── Protocols/          # 协议实现
│   │   │   ├── Handlers/           # 消息处理器
│   │   │   └── Security/           # 通信安全
│   │   ├── Sessions/               # 会话管理
│   │   └── LoadBalancing/          # 负载均衡
│   │
│   ├── AionGate.Auth/              # 认证服务
│   │   ├── Providers/              # 认证提供者
│   │   ├── Tokens/                 # 令牌管理
│   │   └── MFA/                    # 多因素认证
│   │
│   ├── AionGate.Defense/           # 防御服务
│   │   ├── RateLimiting/           # 速率限制
│   │   ├── AntiDDoS/               # DDoS防护
│   │   └── Blacklist/              # 黑名单管理
│   │
│   ├── AionGate.Monitor/           # 监控服务
│   │   ├── HealthChecks/           # 健康检查
│   │   ├── Metrics/                # 指标收集
│   │   └── Alerting/               # 告警系统
│   │
│   ├── AionGate.LauncherBuilder/   # 启动器生成器
│   │   ├── Designer/               # UI设计器
│   │   ├── Compiler/               # 动态编译
│   │   ├── Skins/                  # 皮肤系统
│   │   └── Signing/                # 代码签名
│   │
│   ├── AionGate.Data/              # 数据访问层
│   │   ├── Repositories/           # 仓储实现
│   │   ├── Migrations/             # 数据库迁移
│   │   └── Caching/                # 缓存策略
│   │
│   ├── AionGate.Infrastructure/    # 基础设施
│   │   ├── Configuration/          # 配置管理
│   │   ├── Logging/                # 日志系统
│   │   ├── Security/               # 安全工具
│   │   └── Scheduling/             # 任务调度
│   │
│   └── AionGate.Server/            # 主机程序
│       ├── Program.cs              # 入口点
│       └── Startup.cs              # 依赖注入配置
│
├── launcher/
│   └── AionGate.Launcher/          # 客户端启动器
│       ├── Network/                # 网络通信
│       ├── Services/               # 业务服务
│       └── UI/                     # 用户界面
│
├── tools/
│   ├── aionctl/                    # CLI 管理工具
│   └── migrator/                   # 数据迁移工具
│
├── deploy/
│   ├── docker/                     # Docker 配置
│   ├── kubernetes/                 # K8s 配置
│   └── scripts/                    # 部署脚本
│
├── config/
│   ├── appsettings.json           # 默认配置
│   ├── appsettings.Production.json
│   └── appsettings.Development.json
│
└── tests/
    ├── AionGate.Tests.Unit/        # 单元测试
    └── AionGate.Tests.Integration/ # 集成测试
```

### 3.2 核心接口设计

```csharp
// AionGate.Core/Interfaces/IGatewayService.cs
public interface IGatewayService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task<ISession> AcceptConnectionAsync();
    Task BroadcastAsync<T>(T message) where T : IPacket;
    IReadOnlyDictionary<string, ISession> ActiveSessions { get; }
}

// AionGate.Core/Interfaces/IAuthService.cs
public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(string username, string password);
    Task<AuthResult> ValidateTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
}

// AionGate.Core/Interfaces/IDefenseService.cs
public interface IDefenseService
{
    Task<bool> IsAllowedAsync(string ipAddress);
    Task BlockAsync(string ipAddress, TimeSpan duration, string reason);
    Task UnblockAsync(string ipAddress);
    Task<RateLimitResult> CheckRateLimitAsync(string identifier);
}

// AionGate.Core/Interfaces/IConfigProvider.cs
public interface IConfigProvider
{
    T Get<T>(string section) where T : class, new();
    Task ReloadAsync();
    IDisposable OnChange(Action<string> callback);
}
```

---

## 四、安全架构

### 4.1 加密体系

```
┌─────────────────────────────────────────────────────────────────┐
│                        加密层次架构                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  传输层安全 (Transport Security)                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  TLS 1.3 + 证书双向认证                                  │    │
│  │  • 服务器证书: 自签名或Let's Encrypt                     │    │
│  │  • 客户端证书: 启动器内置 (可选)                         │    │
│  │  • 密钥交换: ECDHE                                       │    │
│  │  • 加密套件: TLS_AES_256_GCM_SHA384                     │    │
│  └─────────────────────────────────────────────────────────┘    │
│                              │                                   │
│                              ▼                                   │
│  应用层加密 (Application Security)                               │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  消息加密: AES-256-GCM                                   │    │
│  │  • 密钥派生: HKDF-SHA256                                 │    │
│  │  • 会话密钥: 每连接唯一                                  │    │
│  │  • Nonce: 递增计数器 + 随机前缀                          │    │
│  └─────────────────────────────────────────────────────────┘    │
│                              │                                   │
│                              ▼                                   │
│  数据层加密 (Data Security)                                      │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  密码存储: Argon2id (内存硬函数)                         │    │
│  │  敏感配置: DPAPI/Vault 加密                              │    │
│  │  数据库: TDE 透明加密 (可选)                             │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 新协议设计

```
旧协议 (不安全):
┌──────────────────────────────────────┐
│  [Size:4] [Opcode:1] [Data:N]        │  XOR "煌" 加密
└──────────────────────────────────────┘

新协议 v2.0:
┌────────────────────────────────────────────────────────────────┐
│  [Magic:2] [Version:1] [Flags:1] [SeqNo:4] [Size:4]           │
│  [Nonce:12] [Opcode:2] [EncryptedPayload:N] [Tag:16]          │
└────────────────────────────────────────────────────────────────┘

字段说明:
- Magic: 0xAE01 (协议标识)
- Version: 协议版本 (当前: 0x02)
- Flags:
  - bit0: 是否压缩
  - bit1: 是否需要ACK
  - bit2-7: 保留
- SeqNo: 序列号 (防重放)
- Size: 加密后载荷大小
- Nonce: AES-GCM nonce
- Opcode: 操作码 (扩展到2字节)
- EncryptedPayload: AES-256-GCM加密的数据
- Tag: GCM认证标签
```

### 4.3 认证流程

```
客户端                                           服务器
   │                                                │
   │──────── ClientHello (公钥, 随机数) ───────────>│
   │                                                │
   │<─────── ServerHello (公钥, 随机数, 证书) ──────│
   │                                                │
   │         [ECDHE 密钥交换, 派生会话密钥]          │
   │                                                │
   │──────── AuthRequest (加密: 用户名, 密码哈希) ──>│
   │                                                │
   │         [服务器验证, 生成JWT令牌]               │
   │                                                │
   │<─────── AuthResponse (JWT AccessToken,         │
   │                       RefreshToken,            │
   │                       服务器配置)              │
   │                                                │
   │──────── [后续请求携带JWT] ─────────────────────>│
   │                                                │
```

### 4.4 SQL注入防护

```csharp
// 旧代码 (危险!)
sql = string.Format("SELECT * FROM users WHERE name = '{0}'", name);

// 新代码 (安全)
public class AccountRepository : IAccountRepository
{
    private readonly IDbConnection _db;

    public async Task<Account?> GetByNameAsync(string name)
    {
        const string sql = "SELECT * FROM accounts WHERE name = @Name";
        return await _db.QueryFirstOrDefaultAsync<Account>(sql, new { Name = name });
    }

    public async Task<bool> ValidateAsync(string name, string passwordHash)
    {
        const string sql = @"
            SELECT COUNT(1) FROM accounts
            WHERE name = @Name AND password_hash = @PasswordHash";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { Name = name, PasswordHash = passwordHash });
        return count > 0;
    }
}
```

---

## 五、配置管理系统

### 5.1 配置文件结构

```yaml
# config/appsettings.yaml (主配置)

server:
  name: "我的永恒之塔"
  port: 10001
  max_connections: 1000
  worker_threads: 4

network:
  protocol_version: 2
  encryption: aes-256-gcm
  compression: true
  heartbeat_interval: 30s
  connection_timeout: 5m

auth:
  token_expiry: 24h
  refresh_token_expiry: 7d
  max_login_attempts: 5
  lockout_duration: 15m
  password_policy:
    min_length: 8
    require_uppercase: true
    require_number: true

database:
  provider: mysql  # mysql | postgresql | mssql
  host: ${DB_HOST:localhost}
  port: ${DB_PORT:3306}
  name: ${DB_NAME:aiongate}
  user: ${DB_USER:root}
  password: ${DB_PASSWORD}  # 从环境变量或Vault读取
  pool_size: 20

defense:
  rate_limit:
    requests_per_second: 100
    burst_size: 200
  ddos:
    enabled: true
    threshold: 1000
  blacklist:
    auto_block: true
    block_duration: 1h

launcher:
  game_path: bin32/aion.bin
  args: "-cc:5 -lang:chs -noweb -nowebshop"
  update_url: https://update.example.com/
  version_check: true

monitoring:
  health_check_port: 9090
  metrics_port: 9091
  log_level: info

anti_cheat:
  enabled: true
  process_scan_interval: 5s
  processes:
    - name: cheatengine.exe
    - name: speedhack.exe
    - md5: "1234567890ABCDEF..."
```

### 5.2 配置热重载

```csharp
// AionGate.Infrastructure/Configuration/ConfigurationService.cs
public class ConfigurationService : IConfigProvider, IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly FileSystemWatcher _watcher;
    private readonly ConcurrentDictionary<string, object> _cache;
    private readonly Subject<string> _changeSubject;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _cache = new ConcurrentDictionary<string, object>();
        _changeSubject = new Subject<string>();

        // 监听配置文件变化
        _watcher = new FileSystemWatcher("config", "*.yaml");
        _watcher.Changed += OnConfigFileChanged;
    }

    public T Get<T>(string section) where T : class, new()
    {
        return _cache.GetOrAdd(section, _ =>
            _configuration.GetSection(section).Get<T>() ?? new T()) as T;
    }

    public async Task ReloadAsync()
    {
        _cache.Clear();
        ((IConfigurationRoot)_configuration).Reload();
        _changeSubject.OnNext("*");
        await Task.CompletedTask;
    }

    public IDisposable OnChange(Action<string> callback)
    {
        return _changeSubject.Subscribe(callback);
    }
}
```

---

## 六、部署架构

### 6.1 Docker 部署

```dockerfile
# deploy/docker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 10001 9090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/", "src/"]
RUN dotnet restore "src/AionGate.Server/AionGate.Server.csproj"
RUN dotnet publish "src/AionGate.Server/AionGate.Server.csproj" \
    -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# 创建非root用户
RUN adduser -D -u 1000 aiongate
USER aiongate

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD wget -q --spider http://localhost:9090/health || exit 1

ENTRYPOINT ["dotnet", "AionGate.Server.dll"]
```

```yaml
# deploy/docker/docker-compose.yml
version: '3.8'

services:
  aiongate:
    build: .
    container_name: aiongate
    restart: unless-stopped
    ports:
      - "10001:10001"   # 游戏端口
      - "9090:9090"     # 健康检查
      - "9091:9091"     # Prometheus指标
    volumes:
      - ./config:/app/config:ro
      - ./logs:/app/logs
      - ./data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_HOST=mysql
      - DB_PASSWORD=${DB_PASSWORD}
    depends_on:
      mysql:
        condition: service_healthy
      redis:
        condition: service_started
    networks:
      - aiongate-network
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G

  mysql:
    image: mysql:8.0
    container_name: aiongate-mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
      MYSQL_DATABASE: aiongate
    volumes:
      - mysql-data:/var/lib/mysql
      - ./deploy/sql/init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - aiongate-network

  redis:
    image: redis:7-alpine
    container_name: aiongate-redis
    restart: unless-stopped
    volumes:
      - redis-data:/data
    networks:
      - aiongate-network

  prometheus:
    image: prom/prometheus:latest
    container_name: aiongate-prometheus
    volumes:
      - ./deploy/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    ports:
      - "9092:9090"
    networks:
      - aiongate-network

  grafana:
    image: grafana/grafana:latest
    container_name: aiongate-grafana
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
    volumes:
      - ./deploy/grafana/dashboards:/var/lib/grafana/dashboards
      - grafana-data:/var/lib/grafana
    ports:
      - "3000:3000"
    networks:
      - aiongate-network

volumes:
  mysql-data:
  redis-data:
  prometheus-data:
  grafana-data:

networks:
  aiongate-network:
    driver: bridge
```

### 6.2 一键部署脚本

```bash
#!/bin/bash
# deploy/scripts/install.sh - 一键部署脚本

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# 检查依赖
check_dependencies() {
    log_info "检查系统依赖..."

    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装，正在安装..."
        curl -fsSL https://get.docker.com | sh
        systemctl enable docker
        systemctl start docker
    fi

    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose 未安装，正在安装..."
        curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" \
            -o /usr/local/bin/docker-compose
        chmod +x /usr/local/bin/docker-compose
    fi

    log_info "依赖检查完成"
}

# 配置向导
config_wizard() {
    log_info "启动配置向导..."

    echo ""
    echo "=========================================="
    echo "      AionGate 2.0 配置向导"
    echo "=========================================="
    echo ""

    # 服务器名称
    read -p "请输入服务器名称 [我的永恒之塔]: " SERVER_NAME
    SERVER_NAME=${SERVER_NAME:-"我的永恒之塔"}

    # 端口配置
    read -p "请输入网关端口 [10001]: " GATEWAY_PORT
    GATEWAY_PORT=${GATEWAY_PORT:-10001}

    # 数据库配置
    read -p "请输入MySQL密码: " -s MYSQL_PASSWORD
    echo ""

    # 管理员配置
    read -p "请输入管理员用户名 [admin]: " ADMIN_USER
    ADMIN_USER=${ADMIN_USER:-admin}
    read -p "请输入管理员密码: " -s ADMIN_PASSWORD
    echo ""

    # 生成随机密钥
    JWT_SECRET=$(openssl rand -base64 32)
    ENCRYPTION_KEY=$(openssl rand -base64 32)

    # 写入环境变量文件
    cat > "$PROJECT_DIR/.env" << EOF
# AionGate 环境配置 - 自动生成于 $(date)
SERVER_NAME=$SERVER_NAME
GATEWAY_PORT=$GATEWAY_PORT
DB_PASSWORD=$MYSQL_PASSWORD
MYSQL_ROOT_PASSWORD=$MYSQL_PASSWORD
ADMIN_USER=$ADMIN_USER
ADMIN_PASSWORD=$ADMIN_PASSWORD
JWT_SECRET=$JWT_SECRET
ENCRYPTION_KEY=$ENCRYPTION_KEY
GRAFANA_PASSWORD=$(openssl rand -base64 12)
EOF

    chmod 600 "$PROJECT_DIR/.env"
    log_info "配置文件已生成: $PROJECT_DIR/.env"
}

# 启动服务
start_services() {
    log_info "启动服务..."

    cd "$PROJECT_DIR/deploy/docker"

    # 拉取镜像
    docker-compose pull

    # 构建并启动
    docker-compose up -d --build

    # 等待服务就绪
    log_info "等待服务启动..."
    sleep 10

    # 健康检查
    for i in {1..30}; do
        if curl -s http://localhost:9090/health | grep -q "healthy"; then
            log_info "服务启动成功!"
            break
        fi
        sleep 2
    done
}

# 显示状态
show_status() {
    echo ""
    echo "=========================================="
    echo "      AionGate 2.0 部署完成"
    echo "=========================================="
    echo ""
    echo "服务状态:"
    docker-compose -f "$PROJECT_DIR/deploy/docker/docker-compose.yml" ps
    echo ""
    echo "访问地址:"
    echo "  - 游戏网关:    tcp://localhost:$GATEWAY_PORT"
    echo "  - 健康检查:    http://localhost:9090/health"
    echo "  - Grafana:     http://localhost:3000"
    echo "  - Prometheus:  http://localhost:9092"
    echo ""
    echo "管理命令:"
    echo "  - 查看日志:    docker-compose logs -f aiongate"
    echo "  - 重启服务:    docker-compose restart"
    echo "  - 停止服务:    docker-compose down"
    echo ""
}

# 主流程
main() {
    echo ""
    echo "=========================================="
    echo "      AionGate 2.0 一键部署"
    echo "=========================================="
    echo ""

    check_dependencies
    config_wizard
    start_services
    show_status
}

main "$@"
```

---

## 七、监控和运维架构

### 7.1 健康检查

```csharp
// AionGate.Monitor/HealthChecks/GatewayHealthCheck.cs
public class GatewayHealthCheck : IHealthCheck
{
    private readonly IGatewayService _gateway;
    private readonly IDbConnection _db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        var checks = new Dictionary<string, object>();

        // 检查网关状态
        checks["gateway_running"] = _gateway.IsRunning;
        checks["active_connections"] = _gateway.ActiveSessions.Count;

        // 检查数据库连接
        try
        {
            await _db.ExecuteScalarAsync<int>("SELECT 1");
            checks["database"] = "connected";
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex, checks);
        }

        // 检查内存使用
        var process = Process.GetCurrentProcess();
        checks["memory_mb"] = process.WorkingSet64 / 1024 / 1024;

        if (!_gateway.IsRunning)
        {
            return HealthCheckResult.Unhealthy("Gateway not running", data: checks);
        }

        return HealthCheckResult.Healthy("All systems operational", checks);
    }
}
```

### 7.2 指标收集

```csharp
// AionGate.Monitor/Metrics/GatewayMetrics.cs
public static class GatewayMetrics
{
    private static readonly Counter ConnectionsTotal = Metrics.CreateCounter(
        "aiongate_connections_total",
        "Total number of connections",
        new CounterConfiguration { LabelNames = new[] { "status" } });

    private static readonly Gauge ActiveConnections = Metrics.CreateGauge(
        "aiongate_active_connections",
        "Current number of active connections");

    private static readonly Histogram PacketLatency = Metrics.CreateHistogram(
        "aiongate_packet_latency_seconds",
        "Packet processing latency",
        new HistogramConfiguration
        {
            LabelNames = new[] { "opcode" },
            Buckets = new[] { .001, .005, .01, .05, .1, .5, 1 }
        });

    private static readonly Counter PacketsTotal = Metrics.CreateCounter(
        "aiongate_packets_total",
        "Total packets processed",
        new CounterConfiguration { LabelNames = new[] { "opcode", "direction" } });

    private static readonly Counter AuthAttempts = Metrics.CreateCounter(
        "aiongate_auth_attempts_total",
        "Authentication attempts",
        new CounterConfiguration { LabelNames = new[] { "result" } });

    public static void RecordConnection(string status) => ConnectionsTotal.WithLabels(status).Inc();
    public static void SetActiveConnections(int count) => ActiveConnections.Set(count);
    public static IDisposable MeasurePacketLatency(string opcode) => PacketLatency.WithLabels(opcode).NewTimer();
    public static void RecordPacket(string opcode, string direction) => PacketsTotal.WithLabels(opcode, direction).Inc();
    public static void RecordAuth(string result) => AuthAttempts.WithLabels(result).Inc();
}
```

### 7.3 Grafana 仪表板配置

```json
{
  "dashboard": {
    "title": "AionGate Monitor",
    "panels": [
      {
        "title": "Active Connections",
        "type": "stat",
        "targets": [
          { "expr": "aiongate_active_connections" }
        ]
      },
      {
        "title": "Connections/min",
        "type": "graph",
        "targets": [
          { "expr": "rate(aiongate_connections_total[1m])" }
        ]
      },
      {
        "title": "Packet Latency (p99)",
        "type": "graph",
        "targets": [
          { "expr": "histogram_quantile(0.99, rate(aiongate_packet_latency_seconds_bucket[5m]))" }
        ]
      },
      {
        "title": "Auth Success Rate",
        "type": "gauge",
        "targets": [
          { "expr": "rate(aiongate_auth_attempts_total{result=\"success\"}[5m]) / rate(aiongate_auth_attempts_total[5m]) * 100" }
        ]
      }
    ]
  }
}
```

---

## 八、迁移路径

### 8.1 渐进式迁移策略

```
阶段 1: 安全加固 (1-2周)
├── 修复SQL注入漏洞
├── 升级密码哈希算法
├── 实现参数化查询
└── 配置外部化

阶段 2: 架构重构 (2-4周)
├── 抽象服务接口
├── 实现依赖注入
├── 分离网络层
└── 添加单元测试

阶段 3: 协议升级 (2-3周)
├── 实现新加密协议
├── 保持向后兼容
├── 客户端升级支持
└── 灰度发布

阶段 4: 容器化部署 (1-2周)
├── 编写Dockerfile
├── 配置docker-compose
├── 一键部署脚本
└── 监控集成

阶段 5: 高级功能 (持续)
├── Web管理界面
├── 集群支持
├── 自动扩缩容
└── 灾备方案
```

### 8.2 向后兼容

```csharp
// 支持新旧协议的连接处理
public class ProtocolNegotiator
{
    public async Task<IProtocolHandler> NegotiateAsync(Stream stream)
    {
        var buffer = new byte[2];
        await stream.ReadAsync(buffer, 0, 2);

        // 检查魔数判断协议版本
        if (buffer[0] == 0xAE && buffer[1] == 0x01)
        {
            // 新协议 v2.0
            return new SecureProtocolHandler();
        }
        else
        {
            // 旧协议 (XOR) - 向后兼容
            return new LegacyProtocolHandler();
        }
    }
}
```

---

## 九、总结

### 关键改进

| 维度 | 现状 | 目标 |
|------|------|------|
| 安全 | XOR加密, SQL拼接, SHA1密码 | AES-GCM, 参数化查询, Argon2 |
| 可靠 | 单点故障, 无监控 | 健康检查, 自动恢复, 告警 |
| 部署 | 手动, 注册表配置 | Docker一键, YAML配置 |
| 扩展 | 单体耦合 | 模块化, 可插拔 |
| 运维 | 手动检查 | Grafana仪表板, 日志聚合 |

### 预期效果

- **开服时间**: 从数小时 → 5分钟
- **故障恢复**: 从手动 → 自动 (MTTR < 1分钟)
- **安全等级**: 从易攻破 → 工业级
- **运维成本**: 降低 80%

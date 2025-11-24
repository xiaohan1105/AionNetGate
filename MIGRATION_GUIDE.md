# AionGate 迁移指南

> 从 1.x 版本迁移到 2.0 现代化架构

## 迁移概述

### 为什么要迁移？

| 问题 | 1.x 版本 | 2.0 版本 |
|------|---------|---------|
| 安全性 | XOR加密，SQL拼接 | AES-GCM，参数化查询 |
| 部署 | 手动复制，注册表配置 | Docker一键，YAML配置 |
| 监控 | 无 | Prometheus + Grafana |
| 可靠性 | 单点故障 | 健康检查，自动恢复 |

### 迁移路径

```
阶段 0: 准备工作 (1天)
    ↓
阶段 1: 安全加固 (1-2周)
    ↓
阶段 2: 架构重构 (2-4周)
    ↓
阶段 3: 协议升级 (2-3周)
    ↓
阶段 4: 容器化部署 (1-2周)
```

---

## 阶段 0: 准备工作

### 0.1 备份现有数据

```bash
# 备份数据库
mysqldump -u root -p aion_game > backup_$(date +%Y%m%d).sql

# 备份配置 (导出注册表)
reg export "HKEY_CURRENT_USER\software\AionRoy\AionNetGate" config_backup.reg

# 备份程序文件
tar -czf aiongate_backup.tar.gz /path/to/AionNetGate
```

### 0.2 评估当前状态

检查清单：
- [ ] 当前在线人数
- [ ] 账号总数
- [ ] 数据库大小
- [ ] 服务器配置 (CPU/内存)
- [ ] 网络带宽

---

## 阶段 1: 安全加固 (紧急)

### 1.1 修复 SQL 注入漏洞

**修改前 (危险!):**
```csharp
// AccountService.cs
sql = string.Format("SELECT * FROM accounts WHERE name = '{0}'", name);
```

**修改后 (安全):**
```csharp
// AccountService.cs
const string sql = "SELECT * FROM accounts WHERE name = @Name";
using var cmd = new MySqlCommand(sql, connection);
cmd.Parameters.AddWithValue("@Name", name);
```

**影响文件:**
- `AionNetGate/Services/AccountService.cs`: 第 215, 231, 314, 383, 753 行

### 1.2 升级密码哈希

**修改前 (不安全):**
```csharp
// 使用 SHA1 无盐哈希
SHA1 sha1 = new SHA1CryptoServiceProvider();
byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
```

**修改后 (安全):**
```csharp
// 使用 BCrypt
using BCrypt.Net;
string hash = BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
bool valid = BCrypt.Verify(password, hash);
```

**数据迁移脚本:**
```sql
-- 添加新的密码列
ALTER TABLE accounts ADD COLUMN password_hash_v2 VARCHAR(255);
ALTER TABLE accounts ADD COLUMN password_salt VARCHAR(64);

-- 标记需要迁移的账号
ALTER TABLE accounts ADD COLUMN password_migrated BOOLEAN DEFAULT FALSE;
```

**迁移逻辑:**
```csharp
// 用户下次登录时自动迁移密码
public async Task<bool> MigratePasswordAsync(string username, string plainPassword)
{
    // 1. 验证旧密码
    var account = await GetAccountAsync(username);
    if (!VerifyLegacyPassword(plainPassword, account.PasswordHash))
        return false;

    // 2. 生成新哈希
    var newHash = BCrypt.HashPassword(plainPassword);

    // 3. 更新数据库
    await UpdatePasswordHashAsync(account.Id, newHash);
    return true;
}
```

### 1.3 移除硬编码密钥

**修改前:**
```csharp
// AES.cs
private static string pancher = "METICSOFT";
private static byte[] arrDESKey = new byte[] { 42, 16, 93, 156, 78, 4, 218, 32 };
```

**修改后:**
```csharp
// 从配置读取
public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration config)
    {
        _key = Convert.FromBase64String(config["Encryption:Key"]);
    }
}
```

---

## 阶段 2: 架构重构

### 2.1 抽象服务接口

**步骤:**

1. 创建 `AionGate.Core` 项目
2. 定义接口 (见 `src/AionGate.Core/Interfaces/`)
3. 实现依赖注入

```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);

// 注册服务
builder.Services.AddSingleton<IGatewayService, GatewayService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IDefenseService, DefenseService>();

// 数据库
builder.Services.AddDbContext<AionGateDbContext>();

var host = builder.Build();
await host.RunAsync();
```

### 2.2 分离网络层

**原结构:**
```
AionNetGate/
└── Netwok/           # 网络和业务逻辑混在一起
    ├── AionConnection.cs
    ├── Client/
    └── Server/
```

**新结构:**
```
AionGate.Gateway/
├── Network/
│   ├── TcpServer.cs        # 纯 TCP 服务器
│   ├── Connection.cs       # 连接抽象
│   └── Protocols/
│       ├── IProtocol.cs    # 协议接口
│       ├── LegacyProtocol.cs  # 旧协议 (兼容)
│       └── SecureProtocol.cs  # 新协议 (AES-GCM)
├── Handlers/               # 消息处理器
│   ├── IPacketHandler.cs
│   ├── AuthHandler.cs
│   └── ...
└── Sessions/               # 会话管理
    └── SessionManager.cs
```

### 2.3 数据库访问重构

**使用 Repository 模式:**

```csharp
// IAccountRepository.cs
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(long id);
    Task<Account?> GetByUsernameAsync(string username);
    Task<long> CreateAsync(Account account);
    Task UpdateAsync(Account account);
    Task<bool> ValidateCredentialsAsync(string username, string passwordHash);
}

// AccountRepository.cs
public class AccountRepository : IAccountRepository
{
    private readonly AionGateDbContext _db;

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _db.Accounts
            .FirstOrDefaultAsync(a => a.Username == username);
    }
}
```

---

## 阶段 3: 协议升级

### 3.1 协议版本协商

新连接建立时自动检测协议版本：

```csharp
public async Task HandleConnectionAsync(TcpClient client)
{
    var stream = client.GetStream();
    var buffer = new byte[2];
    await stream.ReadAsync(buffer, 0, 2);

    IProtocolHandler handler;
    if (buffer[0] == 0xAE && buffer[1] == 0x01)
    {
        // 新协议 v2.0
        handler = new SecureProtocolHandler(_sessionKey);
    }
    else
    {
        // 旧协议 - 向后兼容
        handler = new LegacyProtocolHandler();
    }

    await ProcessConnectionAsync(client, handler);
}
```

### 3.2 启动器升级策略

**方案 A: 强制升级**
- 设置截止日期
- 旧版启动器连接时提示更新
- 截止日期后拒绝旧协议

**方案 B: 灰度升级 (推荐)**
- 同时支持新旧协议
- 逐步引导用户升级
- 通过配置控制

```yaml
# appsettings.yaml
network:
  protocol_version: 2
  legacy_support: true       # 支持旧协议
  legacy_deprecation_date: "2025-03-01"  # 旧协议废弃日期
```

### 3.3 客户端启动器更新

启动器需要同步更新以支持新协议：

```csharp
// 新启动器连接代码
public async Task ConnectAsync(string host, int port)
{
    using var client = new TcpClient();
    await client.ConnectAsync(host, port);

    // 发送协议魔数
    var magic = new byte[] { 0xAE, 0x01 };
    await client.GetStream().WriteAsync(magic);

    // 密钥交换
    await PerformKeyExchangeAsync(client);

    // 使用安全协议通信
    _protocol = new SecureProtocol(_sessionKey);
}
```

---

## 阶段 4: 容器化部署

### 4.1 准备环境

```bash
# 安装 Docker
curl -fsSL https://get.docker.com | sh
systemctl enable docker
systemctl start docker

# 安装 Docker Compose
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" \
    -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
```

### 4.2 数据迁移

```bash
# 1. 导出旧数据库
mysqldump -u root -p --single-transaction aion_accounts > accounts_export.sql

# 2. 启动新数据库容器
docker-compose up -d mysql

# 3. 导入数据
docker exec -i aiongate-mysql mysql -uroot -p$MYSQL_ROOT_PASSWORD aiongate < accounts_export.sql

# 4. 运行迁移脚本
docker exec -i aiongate-mysql mysql -uroot -p$MYSQL_ROOT_PASSWORD aiongate < deploy/sql/migration.sql
```

### 4.3 部署新服务

```bash
# 创建环境配置
cp .env.example .env
vim .env  # 编辑配置

# 启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f aiongate

# 健康检查
curl http://localhost:9090/health
```

### 4.4 DNS 切换

```bash
# 1. 测试新服务正常
curl http://new-server:9090/health

# 2. 更新 DNS 或负载均衡器指向新服务
# 如果使用 Cloudflare/阿里云 DNS，修改 A 记录

# 3. 监控新服务
watch -n 5 'curl -s http://localhost:9091/metrics | grep aiongate_active_connections'

# 4. 确认无问题后停止旧服务
```

---

## 回滚计划

如果迁移出现问题，按以下步骤回滚：

```bash
# 1. 停止新服务
docker-compose down

# 2. 恢复 DNS 指向旧服务

# 3. 恢复数据库
mysql -u root -p aion_game < backup_YYYYMMDD.sql

# 4. 恢复注册表配置
reg import config_backup.reg

# 5. 启动旧服务
```

---

## 常见问题

### Q: 迁移需要停服吗？
A: 阶段 1-2 可以热更新，无需停服。阶段 3-4 需要短暂停服（约 30 分钟）进行切换。

### Q: 旧的启动器还能用吗？
A: 可以，新架构支持向后兼容。但建议尽快引导用户升级到新启动器。

### Q: 账号密码需要重新设置吗？
A: 不需要，密码会在用户下次登录时自动迁移到新的哈希算法。

### Q: 可以只做部分迁移吗？
A: 可以，每个阶段相对独立。但强烈建议至少完成阶段 1（安全加固）。

---

## 支持

遇到问题？
- 查看详细文档: `ARCHITECTURE.md`
- 提交 Issue: [GitHub Issues]

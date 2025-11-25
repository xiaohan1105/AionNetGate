# AionGate 部署指南

本指南详细说明如何在生产环境中部署 AionGate 服务。

---

## 目录

1. [系统要求](#系统要求)
2. [准备工作](#准备工作)
3. [数据库部署](#数据库部署)
4. [服务端部署](#服务端部署)
5. [CDN配置](#cdn配置)
6. [启动器配置](#启动器配置)
7. [监控和运维](#监控和运维)
8. [故障排查](#故障排查)
9. [安全加固](#安全加固)

---

## 系统要求

### 硬件要求

**最低配置**：
- CPU: 4核心
- 内存: 8GB
- 硬盘: 100GB SSD
- 带宽: 100Mbps

**推荐配置**（1000在线）：
- CPU: 8核心+
- 内存: 16GB+
- 硬盘: 500GB NVMe SSD
- 带宽: 1Gbps

### 软件要求

- **操作系统**: Windows Server 2019/2022 或 Windows 10/11 Pro
- **.NET Runtime**: .NET 9.0 Runtime
- **数据库**: MSSQL Server 2019+
- **Redis**: Redis 7.0+ (可选，用于缓存)
- **CDN**: 阿里云OSS / 腾讯云COS / AWS S3 / Cloudflare R2

---

## 准备工作

### 1. 安装 .NET 9.0 Runtime

```powershell
# 下载并安装 .NET 9.0 Runtime
# https://dotnet.microsoft.com/download/dotnet/9.0

# 验证安装
dotnet --version
```

### 2. 安装 MSSQL Server

```powershell
# 下载 MSSQL Server 2019+
# https://www.microsoft.com/sql-server/sql-server-downloads

# 启用混合认证模式
# 设置 sa 密码
# 启用 TCP/IP 协议（端口 1433）
```

### 3. 安装 Redis (可选)

```powershell
# 下载 Redis for Windows
# https://github.com/microsoftarchive/redis/releases

# 或使用 Memurai (Windows原生Redis)
# https://www.memurai.com/
```

---

## 数据库部署

### 1. 创建数据库

```sql
-- 在 SSMS 中执行
CREATE DATABASE AionGameDB;
GO

USE AionGameDB;
GO
```

### 2. 执行初始化脚本

**按顺序执行以下脚本：**

```sql
-- 1. 网关基础表
C:\AionGate\deploy\sql\init_mssql.sql

-- 2. 商城系统表
C:\AionGate\deploy\sql\shop_init.sql

-- 3. 渠道分发系统表
C:\AionGate\deploy\sql\channel_system.sql

-- 4. 热更新系统表
C:\AionGate\deploy\sql\update_system.sql
```

**如果是升级已有系统：**

```sql
-- 执行升级脚本
C:\AionGate\deploy\sql\upgrade_to_update_system.sql
```

### 3. 验证表创建

```sql
-- 检查表是否创建成功
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 应该包含以下表:
-- - client_full_packages (网盘链接)
-- - update_versions (版本管理)
-- - update_files (文件清单)
-- - cdn_nodes (CDN配置)
-- ... 等
```

---

## 服务端部署

### 1. 目录结构

```
C:\AionGate\
├── AionGate.Gateway.exe       # 网关服务
├── AionGate.Shop.exe          # Shop API服务
├── AionGate.Admin.exe         # 管理界面
├── appsettings.json           # 配置文件
├── logs\                      # 日志目录
├── temp\                      # 临时文件
└── wwwroot\                   # 静态文件
```

### 2. 配置文件

编辑 `appsettings.json`：

```json
{
  "ConnectionStrings": {
    "AionDB": "Server=localhost,1433;Database=AionGameDB;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;",
    "Redis": "localhost:6379,password=yourRedisPassword,defaultDatabase=0"
  },

  "Jwt": {
    "Secret": "YourSuperSecretKeyForJwtToken-ChangeThis-MustBe256BitsLong!",
    "Issuer": "AionGate",
    "Audience": "AionClient",
    "ExpiresInMinutes": 1440
  },

  "CDN": {
    "Provider": "AliOSS",
    "BucketName": "aion-updates",
    "AccessKey": "LTAI5txxxxxxxxxxxxxxxx",
    "SecretKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
    "Region": "cn-hangzhou",
    "Domain": "cdn.yourgame.com"
  },

  "Update": {
    "ManifestCacheDuration": 300,
    "SignedUrlDuration": 60,
    "MaxConcurrentDownloads": 8,
    "EnableP2P": true
  }
}
```

**重要配置说明：**

- `ConnectionStrings:AionDB`: 数据库连接字符串
- `Jwt:Secret`: JWT密钥，必须足够复杂（建议256位）
- `CDN:*`: CDN配置（见下文CDN配置章节）

### 3. 注册为Windows服务

```powershell
# 使用 NSSM (Non-Sucking Service Manager)

# 下载 NSSM
# https://nssm.cc/download

# 注册 Gateway 服务
nssm install AionGate "C:\AionGate\AionGate.Gateway.exe"
nssm set AionGate AppDirectory "C:\AionGate"
nssm set AionGate Description "AionGate Gateway Service"
nssm set AionGate Start SERVICE_AUTO_START

# 注册 Shop API 服务
nssm install AionGateShop "C:\AionGate\AionGate.Shop.exe"
nssm set AionGateShop AppDirectory "C:\AionGate"
nssm set AionGateShop Description "AionGate Shop API Service"
nssm set AionGateShop Start SERVICE_AUTO_START

# 启动服务
nssm start AionGate
nssm start AionGateShop

# 查看状态
nssm status AionGate
nssm status AionGateShop
```

### 4. 配置防火墙

```powershell
# 允许网关端口 (默认 10001)
netsh advfirewall firewall add rule name="AionGate Gateway" dir=in action=allow protocol=TCP localport=10001

# 允许Shop API端口 (默认 5000)
netsh advfirewall firewall add rule name="AionGate Shop API" dir=in action=allow protocol=TCP localport=5000
```

---

## CDN配置

### 阿里云 OSS

#### 1. 创建Bucket

```bash
# 登录阿里云控制台
# 对象存储OSS > Bucket列表 > 创建Bucket

# Bucket配置:
- 名称: aion-updates
- 区域: 华东1 (cn-hangzhou)
- 存储类型: 标准存储
- 读写权限: 私有
- 版本控制: 关闭
```

#### 2. 开启CDN加速

```bash
# CDN > 域名管理 > 添加域名

# 域名配置:
- 加速域名: cdn.yourgame.com
- 业务类型: 下载分发
- 源站类型: OSS域名
- 源站域名: aion-updates.oss-cn-hangzhou.aliyuncs.com
```

#### 3. 获取AccessKey

```bash
# 访问控制 > 用户 > 创建用户

# 权限策略:
{
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "oss:GetObject",
        "oss:PutObject"
      ],
      "Resource": [
        "acs:oss:*:*:aion-updates/*"
      ]
    }
  ],
  "Version": "1"
}

# 保存 AccessKeyId 和 AccessKeySecret
```

#### 4. 上传文件到OSS

```bash
# 使用 ossutil 上传游戏文件

# 下载 ossutil
# https://help.aliyun.com/document_detail/120075.html

# 配置
ossutil config
  Endpoint: oss-cn-hangzhou.aliyuncs.com
  AccessKeyId: LTAI5txxxxxxxxxxxxxxxx
  AccessKeySecret: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# 批量上传
ossutil cp -r "D:\GameServer\Aion\Data" oss://aion-updates/Data/
```

### 腾讯云 COS

```bash
# 类似流程，使用 coscmd 工具
pip install coscmd

coscmd config -a <SecretId> -s <SecretKey> -b aion-updates-1234567890 -r ap-beijing

coscmd upload -r "D:\GameServer\Aion\Data" /Data/
```

### Cloudflare R2

```bash
# 使用 rclone 或 AWS CLI
aws configure --profile r2

aws s3 sync "D:\GameServer\Aion\Data" s3://aion-updates/Data/ --endpoint-url=https://xxx.r2.cloudflarestorage.com --profile r2
```

---

## 启动器配置

### 1. 修改启动器配置

在启动器配置中设置API地址：

```csharp
// config.json 或 代码中
{
  "ApiBaseUrl": "https://api.yourgame.com/api/update",
  "LocalGamePath": "D:\\Games\\Aion"
}
```

### 2. 添加网盘下载链接

通过管理界面添加完整客户端下载链接：

```
1. 打开 AionGate.Admin.exe
2. 导航到 热更新管理 > 网盘链接
3. 选择版本 (如 2.7.0.15)
4. 点击 "添加下载链接"
5. 填写信息:
   - 包名称: Aion 2.7 完整客户端 (百度网盘)
   - 网盘类型: 百度网盘
   - 下载链接: https://pan.baidu.com/s/xxxxxx
   - 提取码: abc123
   - 解压密码: aion2024
   - 文件大小: 15728640000
   - 优先级: 100 (推荐)
6. 保存
```

### 3. 生成版本清单

```
1. 在管理界面中，导航到 热更新管理 > 版本管理
2. 点击 "扫描游戏目录"
3. 选择游戏目录 (如 D:\GameServer\Aion)
4. 输入版本号 (如 2.7.0.15)
5. 等待扫描完成 (可能需要几分钟)
6. 点击 "发布版本"
```

---

## 监控和运维

### 1. 日志查看

```powershell
# 日志位置
C:\AionGate\logs\

# 实时查看日志
Get-Content C:\AionGate\logs\gateway-20241125.log -Wait -Tail 50
```

### 2. 性能监控

**SQL Server监控：**

```sql
-- 查看活跃连接
SELECT
    session_id,
    login_name,
    host_name,
    program_name,
    status,
    last_request_start_time
FROM sys.dm_exec_sessions
WHERE is_user_process = 1
ORDER BY last_request_start_time DESC;

-- 查看更新统计
EXEC sp_GetUpdateStatistics @StartDate = '2024-11-01', @EndDate = '2024-11-25';
```

**CDN监控：**

```sql
-- 查看CDN流量
SELECT
    stat_date,
    version_code,
    total_downloaded / 1073741824.0 AS total_gb,
    cdn_downloaded / 1073741824.0 AS cdn_gb,
    p2p_downloaded / 1073741824.0 AS p2p_gb,
    CAST(p2p_downloaded AS FLOAT) / NULLIF(total_downloaded, 0) * 100 AS p2p_ratio
FROM update_daily_stats
WHERE stat_date >= DATEADD(DAY, -30, GETDATE())
ORDER BY stat_date DESC;
```

### 3. 自动备份

```powershell
# 创建备份脚本 backup.ps1

$backupPath = "D:\Backups\AionGate"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$dbName = "AionGameDB"

# 数据库备份
sqlcmd -S localhost -Q "BACKUP DATABASE [$dbName] TO DISK = '$backupPath\${dbName}_$timestamp.bak' WITH COMPRESSION"

# 保留30天的备份
Get-ChildItem $backupPath -Filter "*.bak" | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item

# 添加到计划任务
schtasks /create /tn "AionGate Database Backup" /tr "powershell.exe -File C:\AionGate\backup.ps1" /sc daily /st 02:00
```

---

## 故障排查

### 问题1：无法连接数据库

**症状**: 启动服务失败，日志显示 "Login failed for user 'sa'"

**解决方案**:

```sql
-- 1. 检查SQL Server是否启动
services.msc -> SQL Server (MSSQLSERVER)

-- 2. 检查混合认证模式
-- SSMS > 服务器属性 > 安全性 > SQL Server和Windows身份验证模式

-- 3. 重置sa密码
ALTER LOGIN sa ENABLE;
ALTER LOGIN sa WITH PASSWORD = 'YourNewPassword';

-- 4. 检查TCP/IP协议
-- SQL Server配置管理器 > SQL Server网络配置 > MSSQLSERVER的协议 > TCP/IP > 已启用
```

### 问题2：CDN URL签名失败

**症状**: 客户端下载失败，返回 403 Forbidden

**解决方案**:

```csharp
// 1. 检查AccessKey/SecretKey是否正确
// 2. 检查系统时间是否同步 (CDN签名对时间敏感)
// 3. 检查Bucket权限

// 测试签名
var signer = new CDNUrlSigner(config);
var url = signer.GenerateSignedUrl("test.txt", 60);
Console.WriteLine(url);

// 手动访问URL测试
```

### 问题3：更新进度卡住

**症状**: 更新进度停在某个百分比不动

**解决方案**:

```sql
-- 1. 检查更新日志
SELECT TOP 10 *
FROM client_update_logs
WHERE status = 0 -- 下载中
  AND started_at < DATEADD(HOUR, -2, GETUTCDATE()) -- 超过2小时
ORDER BY started_at DESC;

-- 2. 手动标记为失败
UPDATE client_update_logs
SET status = 2, error_message = '超时'
WHERE id = <log_id>;

-- 3. 检查CDN连接
SELECT * FROM cdn_nodes WHERE is_enabled = 1;
```

### 问题4：内存占用过高

**症状**: 服务内存占用超过2GB

**解决方案**:

```csharp
// 1. 检查是否有内存泄漏
// 2. 优化并发下载数量
// appsettings.json
"Update": {
  "MaxConcurrentDownloads": 4 // 从8降到4
}

// 3. 启用流式下载（已实现）
// 4. 定期重启服务
schtasks /create /tn "Restart AionGate" /tr "nssm restart AionGate" /sc daily /st 04:00
```

---

## 安全加固

### 1. 数据库安全

```sql
-- 1. 禁用sa账户
ALTER LOGIN sa DISABLE;

-- 2. 创建专用账户
CREATE LOGIN aiongate_user WITH PASSWORD = 'ComplexPassword123!';
USE AionGameDB;
CREATE USER aiongate_user FOR LOGIN aiongate_user;

-- 3. 授予最小权限
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO aiongate_user;
GRANT EXECUTE ON SCHEMA::dbo TO aiongate_user;

-- 4. 启用审计
-- SSMS > 数据库 > 安全性 > 审核
```

### 2. API安全

```csharp
// appsettings.json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": 60 // 60秒内最多100个请求
  },

  "Cors": {
    "AllowedOrigins": [
      "https://www.yourgame.com"
    ]
  }
}
```

### 3. CDN安全

```bash
# 1. 启用Referer防盗链
# 阿里云OSS > Bucket > 访问控制 > 防盗链 > 添加Referer白名单
# 允许空Referer: 否
# 白名单: https://www.yourgame.com

# 2. 限制IP访问频率
# CDN > 域名管理 > 访问控制 > IP黑白名单

# 3. 启用HTTPS
# CDN > 域名管理 > HTTPS配置 > 免费证书
```

### 4. 服务器加固

```powershell
# 1. 启用Windows防火墙
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled True

# 2. 禁用不必要的服务
Get-Service | Where-Object {$_.Status -eq "Running"} | Select-Object Name, DisplayName

# 3. 启用自动更新
# Windows Update > 高级选项 > 自动下载并安装更新

# 4. 安装杀毒软件
# 推荐: Windows Defender, Kaspersky, Bitdefender
```

---

## 检查清单

部署完成后，请检查以下项目：

- [ ] 数据库连接正常
- [ ] 所有表和存储过程创建成功
- [ ] 服务启动正常（Gateway + Shop API）
- [ ] 防火墙端口开放
- [ ] CDN配置正确，可以生成签名URL
- [ ] 网盘下载链接已添加
- [ ] 版本清单已生成并发布
- [ ] 启动器可以正常检查更新
- [ ] 日志正常写入
- [ ] 监控告警配置完成
- [ ] 备份任务已创建
- [ ] 安全加固措施已实施

---

## 参考文档

- [README.md](../README.md) - 项目概览
- [UPDATE_API.md](./UPDATE_API.md) - 更新API文档
- [LAUNCHER_INTEGRATION.md](./LAUNCHER_INTEGRATION.md) - 启动器集成指南

---

## 技术支持

如有问题，请：
1. 查看日志文件
2. 查看本文档故障排查章节
3. 提交 [Issue](https://github.com/xiaohan1105/AionNetGate/issues)

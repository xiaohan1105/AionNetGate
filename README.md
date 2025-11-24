# AionGate 2.0

> 现代化、高性能、安全的永恒之塔游戏网关服务器

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/Windows-Server-0078D6?logo=windows)](https://www.microsoft.com/)

## ✨ 核心特性

- ⚡ **高性能** - 基于 .NET 9.0 + System.IO.Pipelines，支持 10,000+ 并发连接
- 🔒 **安全可靠** - AES-256-GCM 加密、X25519 密钥交换、Argon2id 密码哈希
- 🗄️ **统一数据库** - 直接使用游戏服务器的 MSSQL 数据库，简化架构
- 🎮 **完整功能** - 账号认证、商城系统、公告管理、外挂检测
- 🎨 **现代GUI** - WPF管理界面，实时监控和管理
- 💰 **渠道分发** - 登录器渠道管理、推广追踪、自动分成结算
- 🚀 **开箱即用** - Windows 直接部署，无需容器化

## 🏗️ 系统架构

```
┌─────────────┐     ┌──────────────┐     ┌───────────────┐
│   客户端    │────▶│  AionGate    │────▶│  游戏服务器   │
│  (启动器)   │◀────│   网关服务   │◀────│   (Aion)      │
└─────────────┘     └──────────────┘     └───────────────┘
      │                    │                      │
      │渠道追踪            ├──▶ MSSQL (统一数据库)
      │                    ├──▶ Redis (缓存/会话)
      │                    └──▶ WPF GUI (管理界面)
      │
      └──▶ 渠道分发系统 (推广/分成)
```

## 📦 系统要求

### 运行环境
- **操作系统**: Windows Server 2019+ 或 Windows 10/11
- **.NET Runtime**: .NET 9.0 Runtime (或 SDK)
- **数据库**: MSSQL Server 2019+ (游戏服务器已有)
- **缓存** (可选): Redis 7.0+
- **内存**: 最低 2GB，推荐 4GB+
- **磁盘**: 1GB 可用空间

### 开发环境
- Visual Studio 2022 17.8+ 或 JetBrains Rider 2024.1+
- .NET 9.0 SDK
- MSSQL Server Management Studio (SSMS)

## 🚀 快速开始

### 方式一：二进制部署 (推荐)

1. **下载发布版本**
   ```bash
   # 从 Releases 页面下载最新版本
   # 或直接运行发布版本
   ```

2. **初始化数据库**
   ```bash
   # 在 SSMS 中执行 SQL 脚本
   # 1. 创建网关基础表
   deploy/sql/init_mssql.sql

   # 2. 创建商城系统表
   deploy/sql/shop_init.sql

   # 3. 创建渠道分发系统表
   deploy/sql/channel_system.sql
   ```

3. **配置服务**
   ```yaml
   # 编辑 config/appsettings.yaml
   server:
     name: "我的永恒之塔"
     port: 10001

   database:
     host: localhost
     port: 1433
     name: AionGameDB  # 游戏数据库名
     user: sa
     password: "你的密码"
   ```

4. **启动服务**
   ```bash
   # 启动网关服务
   AionGate.Gateway.exe

   # 启动管理界面
   AionGate.Admin.exe
   ```

### 方式二：源码编译

```bash
# 1. 克隆仓库
git clone https://github.com/xiaohan1105/AionNetGate.git
cd AionNetGate

# 2. 恢复依赖
dotnet restore

# 3. 编译项目
dotnet build --configuration Release

# 4. 发布
dotnet publish src/AionGate.Gateway -c Release -o publish/gateway
dotnet publish src/AionGate.Admin -c Release -o publish/admin

# 5. 运行
cd publish/gateway
./AionGate.Gateway.exe
```

## 📊 管理界面

AionGate 提供功能完善的 WPF 管理界面：

### 🎛️ 仪表板
- 实时在线人数监控
- CPU、内存使用率
- 24小时在线趋势图
- 今日收入统计
- 热门商品排行
- 实时日志流

### 👥 玩家管理
- 在线玩家实时显示
- 玩家搜索 (账号/角色名/IP)
- 快速操作:
  - 🔨 踢出玩家
  - 🚫 封禁账号 (支持时长设置)
  - 🎁 发送道具 (邮件系统)
  - 💰 发送点券
  - 📧 发送邮件

### 🛒 商城管理
- 商品管理 (添加/编辑/删除)
- 价格设置 (支持折扣)
- 库存管理和限购设置
- 销售数据统计
- 热门商品排行

### 📢 公告管理
- 富文本编辑器
- 公告分类 (系统/活动/维护/更新/新闻)
- 置顶功能
- 定时发布
- 全服推送

### 🔗 渠道分发
- 渠道/代理商管理
- 推广链接生成和追踪
- 用户来源统计
- 充值分成自动计算
- 结算单生成和审核
- 渠道数据报表

### 📊 数据统计
- 在线人数趋势
- 收入统计图
- 玩家活跃度
- 商品销售排行
- 渠道转化率分析

### 🛡️ 外挂检测
- 实时进程扫描
- MD5 文件校验
- 窗口类名检测
- 自动封禁规则
- 检测日志和统计

### ⚙️ 系统工具
- 服务器启动/停止/重启
- 维护模式切换
- 数据库备份工具
- 配置在线编辑

## 💰 渠道分发系统

### 功能特点
- **灵活分成** - 支持不同渠道设置不同的分成比例 (0-100%)
- **自动追踪** - 用户注册时自动绑定渠道来源
- **实时统计** - 实时查看渠道的用户数、充值额、分成金额
- **自动结算** - 支持日结/周结/月结，自动生成结算单
- **推广工具** - 生成专属推广链接和二维码
- **数据分析** - 完整的渠道转化漏斗和ROI分析

### 使用流程

1. **创建渠道**
   - 在管理界面添加渠道/代理商
   - 设置渠道代码 (如: PARTNER01)
   - 配置分成比例 (如: 15%)
   - 选择结算周期 (日结/周结/月结)

2. **生成登录器**
   - 为每个渠道生成专属登录器
   - 登录器内嵌渠道代码
   - 用户使用该登录器注册时自动绑定渠道

3. **推广追踪**
   - 生成推广链接: `https://你的域名/download?ref=PARTNER01`
   - 追踪点击、下载、注册转化
   - 生成推广二维码

4. **自动分成**
   - 用户充值时自动记录渠道
   - 按照设定比例计算分成金额
   - 累加到渠道的未结算分成

5. **结算管理**
   - 按周期自动生成结算单
   - 管理员审核确认
   - 标记支付状态
   - 导出Excel报表

### 数据库设计

渠道系统包含以下核心表：
- `channels` - 渠道/代理商信息
- `user_channels` - 用户渠道绑定
- `channel_recharges` - 渠道充值记录
- `channel_settlements` - 渠道结算单
- `channel_links` - 推广链接
- `channel_daily_stats` - 每日统计数据

### API接口

```csharp
// 用户注册时绑定渠道
EXEC sp_BindUserToChannel
    @AccountID = 10001,
    @ChannelCode = 'PARTNER01',
    @RegisterIP = '192.168.1.100';

// 用户充值时记录分成
EXEC sp_RecordChannelRecharge
    @AccountID = 10001,
    @OrderNo = 'ORD20241125001',
    @Amount = 100.00;

// 生成渠道结算单
EXEC sp_GenerateChannelSettlement
    @ChannelID = 1,
    @PeriodStart = '2024-11-01',
    @PeriodEnd = '2024-11-30',
    @SettlementPeriod = '2024-11';

// 查询渠道统计
EXEC sp_GetChannelStatistics
    @ChannelID = 1,
    @StartDate = '2024-11-01',
    @EndDate = '2024-11-30';
```

## 🛠️ 技术栈

- **框架**: .NET 9.0, ASP.NET Core 9.0
- **数据库**: MSSQL Server 2019+ (统一使用游戏数据库)
- **缓存**: Redis 7.4 (可选)
- **界面**: WPF + ModernWPF UI
- **图表**: LiveChartsCore 2.0
- **加密**: NSec (X25519, AES-GCM), Argon2id
- **ORM**: Entity Framework Core 9.0, Dapper
- **日志**: Serilog

[查看完整技术栈](TECHNOLOGY_STACK.md)

## 📖 文档

- [架构设计](ARCHITECTURE.md) - 完整系统架构文档
- [技术栈](TECHNOLOGY_STACK.md) - 技术栈详情和性能指标
- [迁移指南](MIGRATION_GUIDE.md) - 从 1.x 迁移到 2.0
- [商城系统](src/AionGate.Shop/README.md) - 商城系统文档
- [管理界面](src/AionGate.Admin/README.md) - 管理界面使用指南

## 📊 性能指标

基于 .NET 9.0 + MSSQL Server 2019 + Redis 7.4:

| 指标 | 数值 |
|------|------|
| 并发连接 | 10,000+ |
| 每秒请求 (RPS) | 50,000+ |
| 平均延迟 | <5ms |
| P99 延迟 | <20ms |
| 内存占用 | ~500MB (空载) |
| CPU 占用 | ~5% (空载) |

## 🔐 安全特性

- ✅ AES-256-GCM 通信加密 (替代 XOR)
- ✅ X25519 ECDH 密钥交换
- ✅ Argon2id 密码哈希 (替代 SHA1)
- ✅ JWT 令牌认证
- ✅ 参数化查询 (防止 SQL 注入)
- ✅ 速率限制 (防止 DDoS)
- ✅ IP 黑白名单
- ✅ 硬件指纹识别 (防多开)
- ✅ 外挂进程检测

## 🗺️ 路线图

- [x] 核心网关功能
- [x] 商城系统
- [x] 管理界面
- [x] 渠道分发系统
- [ ] 自动更新系统
- [ ] 多线路支持
- [ ] 数据备份工具
- [ ] Web 管理后台
- [ ] 移动端管理 APP

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

本项目采用 [MIT](LICENSE) 许可证。

## 💬 联系方式

- **问题反馈**: [GitHub Issues](https://github.com/xiaohan1105/AionNetGate/issues)
- **功能建议**: [GitHub Discussions](https://github.com/xiaohan1105/AionNetGate/discussions)

---

⭐ 如果这个项目对你有帮助，请给个 Star 支持一下！

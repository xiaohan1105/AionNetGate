# AionGate 2.0

> 现代化、高性能、安全的永恒之塔游戏网关服务器

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/docker-ready-2496ED?logo=docker)](https://www.docker.com/)

## 特性

- ⚡ **高性能** - 基于 .NET 9.0 + System.IO.Pipelines，支持 10,000+ 并发连接
- 🔒 **安全可靠** - AES-256-GCM 加密、X25519 密钥交换、Argon2id 密码哈希
- 🐳 **容器化** - Docker + Docker Compose 一键部署
- 📊 **可观测性** - Prometheus + Grafana + OpenTelemetry 完整监控
- 🗄️ **灵活数据库** - 支持 PostgreSQL (推荐) 和 MySQL
- 🚀 **开箱即用** - 3 步完成部署

## 快速开始

### 前置要求

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) 和 [Docker Compose](https://docs.docker.com/compose/)

### 部署步骤

```bash
# 1. 克隆仓库
git clone https://github.com/YOUR_USERNAME/AionNetGate.git
cd AionNetGate

# 2. 配置环境变量
cd deploy/docker
cp .env.example .env
# 编辑 .env 文件，设置密码和密钥
vim .env

# 3. 启动服务
docker-compose up -d

# 4. 查看日志
docker-compose logs -f aiongate

# 5. 健康检查
curl http://localhost:9090/health
```

访问监控面板: http://localhost:3000 (默认用户名/密码: admin/admin)

## 架构

```
┌─────────────┐     ┌──────────────┐     ┌───────────────┐
│   客户端    │────▶│  AionGate    │────▶│  游戏服务器   │
│  (启动器)   │◀────│   网关服务   │◀────│   (Aion)      │
└─────────────┘     └──────────────┘     └───────────────┘
                           │
                           ├──▶ PostgreSQL (账号数据)
                           ├──▶ Redis (缓存/会话)
                           ├──▶ Prometheus (指标)
                           └──▶ Grafana (监控面板)
```

## 技术栈

- **框架**: .NET 9.0, ASP.NET Core
- **数据库**: PostgreSQL 17 / MySQL 9
- **缓存**: Redis 7.4
- **监控**: Prometheus 3.0, Grafana 11.4, OpenTelemetry 1.10
- **加密**: NSec (X25519, AES-GCM), Argon2id
- **ORM**: Entity Framework Core 9.0, Dapper

[查看完整技术栈](TECHNOLOGY_STACK.md)

## 文档

- [架构设计](ARCHITECTURE.md) - 完整系统架构文档
- [技术栈](TECHNOLOGY_STACK.md) - 技术栈详情和性能指标
- [迁移指南](MIGRATION_GUIDE.md) - 从 1.x 迁移到 2.0
- [API 文档](docs/API.md) - REST API 接口文档
- [开发指南](docs/DEVELOPMENT.md) - 开发者指南

## 性能指标

基于 .NET 9.0 + PostgreSQL 17 + Redis 7.4:

| 指标 | 数值 |
|------|------|
| 并发连接 | 10,000+ |
| 每秒请求 (RPS) | 50,000+ |
| 平均延迟 | <5ms |
| P99 延迟 | <20ms |
| 内存占用 | ~500MB |

## 安全特性

- ✅ AES-256-GCM 通信加密 (替代 XOR)
- ✅ X25519 ECDH 密钥交换
- ✅ Argon2id 密码哈希 (替代 SHA1)
- ✅ JWT 令牌认证
- ✅ 参数化 SQL 查询 (防注入)
- ✅ IP 黑名单和限流
- ✅ 硬件指纹检测
- ✅ 外挂检测系统

## 开发

```bash
# 克隆项目
git clone https://github.com/YOUR_USERNAME/AionNetGate.git
cd AionNetGate

# 恢复依赖
dotnet restore

# 构建项目
dotnet build

# 运行测试
dotnet test

# 启动开发服务器
cd src/AionGate.Gateway
dotnet run
```

## 项目结构

```
AionNetGate/
├── src/
│   ├── AionGate.Core/          # 核心库 (接口、网络、安全)
│   ├── AionGate.Gateway/       # 网关服务主程序
│   └── AionGate.Data/          # 数据访问层
├── deploy/
│   ├── docker/                 # Docker 配置
│   └── sql/                    # 数据库脚本
├── config/
│   └── appsettings.yaml        # 配置文件
├── docs/                       # 文档
├── ARCHITECTURE.md             # 架构文档
├── MIGRATION_GUIDE.md          # 迁移指南
└── TECHNOLOGY_STACK.md         # 技术栈文档
```

## 贡献

欢迎贡献！请查看 [CONTRIBUTING.md](CONTRIBUTING.md) 了解详情。

## 许可

[MIT License](LICENSE)

## 致谢

- [Aion-unique](https://github.com/AionEmu/aion-unique) - 原始 Aion 服务器模拟器
- [.NET](https://github.com/dotnet/runtime) - 强大的运行时平台
- 所有贡献者

---

Made with ❤️ for the Aion community

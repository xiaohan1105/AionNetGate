# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

本仓库包含 AionNetGate 游戏网关系统的两个版本：

1. **AionNetGate** (传统项目) - .NET Framework 2.0 WinForms 网关应用
2. **AionNetGate.Modern** (现代化项目) - .NET 9 单体应用架构

## 构建命令

### 传统项目 (AionNetGate)

使用 Visual Studio 和 MSBuild 构建 .NET Framework 2.0 C# WinForms 应用程序。

```bash
# 构建主项目
msbuild AionNetGate.sln /p:Configuration=Debug
msbuild AionNetGate.sln /p:Configuration=Release

# 构建特定平台
msbuild AionNetGate.sln /p:Configuration=Release /p:Platform=x64

# 构建子项目
msbuild AionLanucher\AionLanucher.sln /p:Configuration=Release
msbuild NetGateReg\NetGateReg.sln /p:Configuration=Release
```

**输出位置：**
- Debug: `AionNetGate\bin\Debug\`
- Release: `AionNetGate\bin\Release\`
- 加壳版本: `AionNetGate\bin\Release\已加壳\`

### 现代化项目 (AionNetGate.Modern)

使用 .NET 9 CLI 构建单体应用。

```bash
# 恢复依赖
dotnet restore AionNetGate.Modern.sln

# 构建
dotnet build AionNetGate.Modern.sln --configuration Debug
dotnet build AionNetGate.Modern.sln --configuration Release

# 运行主机服务
dotnet run --project src/AionNetGate.Host

# 运行测试
dotnet test AionNetGate.Modern.sln

# 发布单文件
dotnet publish src/AionNetGate.Host -c Release -r win-x64 --self-contained -o publish
```

## 项目架构

### 传统项目架构 (AionNetGate)

这是一个 WinForms 网络网关应用程序：

#### 核心组件

1. **MainForm** (`MainForm.cs`) - 主应用程序窗口
   - 管理客户端连接显示
   - 处理服务器启动/停止
   - 远程管理功能（桌面、进程、文件浏览）

2. **MainService** (`Services\MainService.cs`) - 核心网络服务
   - 继承自 NetServer 处理 TCP 连接
   - 管理客户端连接生命周期
   - 维护 `Dictionary<int, LauncherInfo>` 连接表

3. **AionConnection** (`Netwok\AionConnection.cs`) - 客户端连接处理器
   - 处理传入数据包
   - 管理远程管理窗口
   - ping/pong 心跳机制

#### Network 架构

- **Packet System**: 基于 opcode 路由
  - Client packets: `Netwok\Client\CM_*`
  - Server packets: `Netwok\Server\SM_*`
  - Opcode: 0x00-0x09

#### 配置系统

- 设置存储: Windows Registry `HKEY_CURRENT_USER\software\AionRoy\AionNetGate`
- 配置类: `Configs\Config.cs`

### 现代化项目架构 (AionNetGate.Modern)

基于 .NET 9 的**单体应用**，采用分层架构设计：

```
AionNetGate.Modern.sln
├── src/
│   ├── AionNetGate.Core/           # 核心领域层
│   │   ├── Domain/Entities/        # 实体类 (Account, Session, HardwareFingerprint, IpBlacklist)
│   │   ├── Interfaces/             # 仓储接口 (IRepository, IAccountRepository, etc.)
│   │   ├── Configuration/          # 配置类 (ServerConfig, DatabaseConfig, SecurityConfig)
│   │   └── Results/                # Result 模式
│   │
│   ├── AionNetGate.Application/    # 应用服务层
│   │   └── Services/               # 业务服务 (AccountService, etc.)
│   │
│   ├── AionNetGate.Infrastructure/ # 基础设施层
│   │   ├── Data/                   # EF Core DbContext
│   │   ├── Repositories/           # 仓储实现
│   │   └── Security/               # 加密、密码哈希
│   │
│   ├── AionNetGate.Network/        # 网络通信层
│   │   ├── Protocols/              # 数据包协议
│   │   ├── Handlers/               # 消息处理器
│   │   └── Server/                 # TCP 服务器
│   │
│   └── AionNetGate.Host/           # 主机入口 (Worker Service)
│       ├── Program.cs              # 入口点
│       └── appsettings.json        # 配置文件
│
└── tests/
    ├── AionNetGate.UnitTests/
    └── AionNetGate.IntegrationTests/
```

### AI 模块架构 (AionNetGate.AI)

基于 **Microsoft Semantic Kernel** 的智能运营 Agent 框架：

```
src/AionNetGate.AI/
├── Configuration/
│   └── AIConfiguration.cs          # AI 配置 (Provider, API Key, Models)
├── Services/
│   ├── ILLMService.cs              # LLM 服务接口和类型定义
│   └── LLMService.cs               # Semantic Kernel 实现
├── Tools/
│   ├── ITool.cs                    # 工具接口定义
│   └── ToolRegistry.cs             # 工具注册和管理
├── Agents/
│   ├── IAgent.cs                   # Agent 接口和核心类型
│   ├── AgentBase.cs                # Agent 基类 (含工具调用)
│   └── GameOps/
│       ├── CustomerServiceAgent.cs # 客服 Agent - 问题解答、工单处理
│       ├── SecurityAgent.cs        # 安全 Agent - 外挂检测、账号封禁
│       ├── AnalyticsAgent.cs       # 分析 Agent - 数据统计、报表生成
│       ├── OperationsAgent.cs      # 运维 Agent - 服务器管理、维护
│       └── ContentAgent.cs         # 内容 Agent - 活动策划、公告编写
├── Workflows/
│   ├── IWorkflow.cs                # 工作流接口和结果类型
│   └── Orchestrator.cs             # 多 Agent 编排器
└── Extensions/
    └── ServiceCollectionExtensions.cs # DI 注册扩展
```

#### Agent 层级 (AgentTier)

| 层级 | 说明 | 模型选择 |
|------|------|---------|
| Critical | 关键任务 (封禁、大额发放) | AdvancedModel (gpt-4o) |
| Complex | 复杂分析 (数据报表) | DefaultModel (gpt-4o) |
| Support | 支持任务 (客服、内容) | DefaultModel |
| Simple | 简单任务 (查询) | FastModel (gpt-4o-mini) |

#### 工具调用流程

```
用户请求 → Orchestrator → 匹配 Agent/Workflow → LLM 推理 → Tool 调用 → 响应
                ↓
        [需要人工确认?] → PendingAction → 审批 → 执行
```

#### 敏感操作确认机制

以下操作自动触发人工确认 (`PendingAction`):
- 账号封禁/解封
- 奖励批量发放
- 服务器重启/维护
- 配置变更

#### 技术栈

| 组件 | 技术 |
|------|------|
| 运行时 | .NET 9 |
| 主机模型 | Worker Service |
| ORM | EF Core 9 (SQLite/MySQL/MSSQL) |
| 日志 | Serilog |
| 配置 | appsettings.json + 环境变量 |
| **AI 框架** | Microsoft Semantic Kernel 1.54.0 |
| **LLM 提供商** | OpenAI / Azure OpenAI |

#### 配置文件

主配置: `src/AionNetGate.Host/appsettings.json`

```json
{
  "Server": {
    "ListenPort": 9000,
    "MaxConnections": 10000
  },
  "Database": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=aiongate.db"
  },
  "Security": {
    "EnableIpBlacklist": true,
    "MaxLoginAttempts": 5
  },
  "AI": {
    "Enabled": true,
    "Provider": "OpenAI",
    "ApiKey": "sk-xxx",
    "Endpoint": "",
    "DefaultModel": "gpt-4o",
    "AdvancedModel": "gpt-4o",
    "FastModel": "gpt-4o-mini",
    "MaxTokens": 4096,
    "Temperature": 0.7,
    "TopP": 0.95,
    "Agents": {
      "EnableCustomerService": true,
      "EnableSecurity": true,
      "EnableAnalytics": true,
      "EnableOperations": true,
      "EnableContent": true,
      "MaxConcurrentAgents": 5,
      "AgentTimeoutSeconds": 300
    }
  }
}
```

#### AI 使用指南

**1. 启用 AI 功能**

在 `appsettings.json` 中设置:
```json
"AI": {
  "Enabled": true,
  "ApiKey": "your-openai-api-key"
}
```

**2. 使用 Azure OpenAI**

```json
"AI": {
  "Provider": "Azure",
  "Endpoint": "https://your-resource.openai.azure.com",
  "ApiKey": "your-azure-key",
  "DefaultModel": "your-deployment-name"
}
```

**3. 通过代码调用 Orchestrator**

```csharp
// 注入 IOrchestrator
public class MyService
{
    private readonly IOrchestrator _orchestrator;

    public MyService(IOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task HandleUserRequest(string input)
    {
        var result = await _orchestrator.ProcessAsync(
            input: "帮我查询账号 test123 的登录记录",
            userId: "admin001");

        if (result.Success)
        {
            Console.WriteLine(result.Response);
        }

        // 处理待确认操作
        foreach (var action in result.PendingActions)
        {
            // 显示给管理员确认
            var approved = await ShowConfirmDialog(action.Description);
            await _orchestrator.ConfirmActionAsync(action.ActionId, approved);
        }
    }
}
```

**4. 扩展自定义工具**

```csharp
public class MyCustomTool : ITool
{
    public string Name => "my_custom_tool";
    public string Description => "自定义工具描述";
    public string Category => "custom";

    public ToolDefinition GetDefinition() => new()
    {
        Name = Name,
        Description = Description,
        Parameters = new Dictionary<string, ToolParameter>
        {
            ["param1"] = new() { Type = "string", Description = "参数1" }
        }
    };

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, JsonElement> arguments,
        CancellationToken cancellationToken = default)
    {
        var param1 = arguments["param1"].GetString();
        // 执行逻辑...
        return ToolResult.Success("执行成功", new { result = "data" });
    }
}

// 注册工具
services.AddTransient<ITool, MyCustomTool>();
```

**5. 创建自定义 Agent**

```csharp
public class MyAgent : AgentBase
{
    public MyAgent(ILLMService llm, IToolRegistry tools, ILogger<MyAgent> logger)
        : base(llm, tools, logger) { }

    public override string Id => "my_agent";
    public override string Name => "自定义Agent";
    public override string Description => "处理特定业务";
    public override AgentType Type => AgentType.Operations;
    public override AgentTier Tier => AgentTier.Support;

    public override string SystemPrompt => """
        你是一个专业的助手...
        """;

    public override IReadOnlyList<string> AvailableTools => new[]
    {
        "my_custom_tool"
    };

    public override bool CanHandle(AgentRequest request)
    {
        return request.Input.Contains("关键词");
    }
}

// 注册 Agent
services.AddTransient<IAgent, MyAgent>();
```

## 启动器生成器

网关包含可视化启动器生成器 (`Launcher\DesignLauncher.cs`)：

### 生成流程

1. **UI 设计**: 拖拽调整按钮位置、设置背景图
2. **配置嵌入**: 服务器IP/端口、外挂检测列表
3. **动态编译**: CSharpCodeProvider 编译
4. **代码保护**: Reactor 混淆 + TMD 加壳

### 皮肤资源

```
Resources/Skins/BackInBlack/
├── skin.config        # 皮肤配置
├── background.png     # 背景图
├── button*.png        # 按钮图片
└── icon.ico           # 程序图标
```

### 外挂检测配置

```
EXENAME=进程名.exe      # 按进程名检测
EXEMD5=MD5哈希值        # 按文件MD5检测
EXECLASS=窗口类名       # 按窗口类名检测
```

## 通信协议

### 数据包格式

```
[包大小 4字节 int32] [Opcode 1字节] [数据负载 N字节]
加密: XOR ^ "煌" (Unicode 0x714C)
```

### Opcode 映射

| Opcode | 客户端→网关 | 网关→客户端 |
|--------|------------|------------|
| 0x00 | 连接请求 | 连接确认 |
| 0x01 | 账号操作 | 账号结果 |
| 0x02 | 上传桌面 | 请求桌面 |
| 0x03 | 上传进程 | 请求进程 |
| 0x04 | 电脑信息 | 请求信息 |
| 0x05 | Ping | Pong |
| 0x06 | 外挂信息 | 外挂配置 |
| 0x07 | 文件列表 | 请求文件 |
| 0x08 | 注册表 | 请求注册表 |
| 0x09 | 服务列表 | 请求服务 |

## 开发说明

### 传统项目注意事项

- 目标框架: .NET Framework 2.0
- 配置持久化: Windows 注册表
- 目录拼写: `Netwok` (保持原样)
- 依赖: AionCommons.dll

### 现代化项目注意事项

- 目标框架: .NET 9
- 单体应用，单进程部署
- 分层架构，便于测试和维护
- 支持多数据库 (SQLite/MySQL/MSSQL)

### AI 模块注意事项

- **依赖**: Microsoft.SemanticKernel 1.54.0
- **启用方式**: 配置 `AI.Enabled = true` 后自动注册
- **类型冲突**: 使用 `Services.ChatMessage`、`Services.ToolCall` 避免命名空间冲突
- **Agent 匹配**: Orchestrator 按 `AgentTier` 优先级选择 (Critical > Complex > Support > Simple)
- **人工确认**: 敏感操作返回 `PendingAction`，需调用 `ConfirmActionAsync` 执行

#### 内置 Agent 触发关键词

| Agent | 关键词 |
|-------|--------|
| CustomerService | 问题、帮助、查询、怎么、为什么、密码、登录 |
| Security | 外挂、封禁、异常、攻击、安全、可疑、举报 |
| Analytics | 统计、报表、分析、趋势、数据、DAU、留存 |
| Operations | 服务器、重启、维护、配置、性能、负载、监控 |
| Content | 活动、公告、奖励、发放、邮件、推送、策划 |

## 数据库初始化

**传统项目:**
```bash
# MySQL
mysql -u root -p < database/init_mysql.sql

# MSSQL
sqlcmd -S localhost -U sa -i database/init_mssql.sql
```

**现代化项目:**
```bash
# 使用 EF Core 迁移
dotnet ef database update --project src/AionNetGate.Infrastructure
```

## 快速开始

**传统项目:**
```bash
msbuild AionNetGate.sln /p:Configuration=Debug
start AionNetGate\bin\Debug\AionNetGate.exe
```

**现代化项目:**
```bash
dotnet run --project src/AionNetGate.Host
```

## 相关项目

- **AionLanucher**: 游戏启动器客户端
- **NetGateReg**: 注册工具

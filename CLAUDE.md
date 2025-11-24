# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 构建命令

这是一个使用 Visual Studio 构建的 .NET Framework 2.0 C# WinForms 应用程序。

**构建主项目（AionNetGate）：**
```bash
msbuild AionNetGate.sln /p:Configuration=Debug
msbuild AionNetGate.sln /p:Configuration=Release
```

**构建特定平台：**
```bash
msbuild AionNetGate.sln /p:Configuration=Debug /p:Platform="Any CPU"
msbuild AionNetGate.sln /p:Configuration=Release /p:Platform=x64
```

**构建子项目：**
```bash
# Launcher 项目
msbuild AionLanucher\AionLanucher.sln /p:Configuration=Release

# 注册工具
msbuild NetGateReg\NetGateReg.sln /p:Configuration=Release
```

**输出位置：**
- Debug 构建: `AionNetGate\bin\Debug\`
- Release 构建: `AionNetGate\bin\Release\`
- x64 构建: `AionNetGate\bin\x64\`
- Release 构建带代码保护: `AionNetGate\bin\Release\已加壳\`（通过 post-build 事件自动生成）

## 项目架构

这是一个网络网关应用程序，具有以下关键架构组件：

### 核心组件

1. **MainForm** (`MainForm.cs`) - 主应用程序窗口和入口点
   - 管理客户端连接在 ListView 中的显示
   - 处理服务器启动/停止操作
   - 提供远程管理功能（桌面查看、进程监控、文件浏览）
   - 通过注册表和 UI 控件管理配置

2. **MainService** (`Services\MainService.cs`) - 核心网络服务
   - 继承自 NetServer 处理 TCP 连接
   - 管理客户端连接生命周期
   - 路由网络事件（连接、接收数据、断开连接）
   - 维护包含 LauncherInfo 对象的连接表

3. **AionConnection** (`Netwok\AionConnection.cs`) - 单个客户端连接处理器
   - 处理来自客户端的传入数据包
   - 管理数据包队列和发送
   - 处理远程管理窗口（桌面、进程、浏览器窗体）
   - 通过 ping/pong 实现连接监控

### Network 架构

- **Packet System**: 基于 opcode 路由的 Client-Server packet 架构
  - Client packets: `Netwok\Client\` 中的 `CM_*` 类（从客户端发送到服务器）
  - Server packets: `Netwok\Server\` 中的 `SM_*` 类（从服务器发送到客户端）
  - Packet registry: `AionPackets.cs` 中的 `Initialize()` 方法将 opcodes 映射到 packet 类型
  - Opcode 范围: 0x00-0x09 涵盖所有当前功能（连接、账号、远程桌面、进程、电脑信息、ping/pong、外挂检测、文件浏览、注册表、服务）

- **Connection 管理**:
  - MainService 使用 `Dictionary<int, LauncherInfo>` 跟踪所有客户端连接
  - 每个 LauncherInfo 封装 AionConnection 实例和客户端元数据（账号ID、玩家ID、硬件ID等）
  - DefenseService 提供 IP 阻止和攻击防护
  - 自动 ping/pong 心跳机制维持连接活性

- **数据流**:
  1. 客户端连接 → MainService 创建 AionConnection
  2. AionConnection 接收原始数据 → 解析为 CM_* packets
  3. 根据 opcode 路由到对应的 packet handler
  4. Packet handler 处理业务逻辑，生成 SM_* response packets
  5. Response packets 通过 AionConnection 发送回客户端

### 关键功能

- **远程管理**: 查看客户端桌面、进程、文件系统、注册表和服务
- **Database 集成**: 支持 MySQL 和 MSSQL 用于用户/军团数据
- **邮件通知**: SMTP 配置用于警报
- **自动重启**: 可配置的服务重启间隔
- **安全**: IP 阻止、硬件指纹识别和攻击检测
- **Launcher 生成**: 创建带有嵌入式设置的自定义游戏启动器

### 配置系统

项目使用混合配置方法：

- **传统配置** (`Configs\Config.cs`):
  - 静态类管理所有配置值
  - 设置存储在 Windows Registry: `HKEY_CURRENT_USER\software\AionRoy\AionNetGate`
  - UI 控件更改时自动保存到注册表
  - 包括服务器配置（IP、端口）、数据库连接、邮件设置、安全选项等

- **新配置架构** (`Configs\` 目录):
  - `Core\ConfigurationManager.cs`: 模块化配置管理器
  - `Core\IConfigurationSection.cs`: 配置节接口
  - `Sections\ServerConfig.cs`: 服务器设置
  - `Sections\DatabaseConfig.cs`: 数据库设置
  - `Sections\LoggingConfig.cs`: 日志设置
  - `Sections\SecurityConfig.cs`: 安全设置
  - `LegacyConfigAdapter.cs`: 新旧配置系统之间的适配器

**注意**: 新配置系统已实现但可能尚未完全集成。当前代码主要使用传统的 `Config.cs` 静态类。

### 依赖项

- **AionCommons.dll**: 核心网络和实用程序库
- **MySql.Data.dll**: MySQL 数据库连接
- **BSE.Windows.Forms.dll**: 增强的 WinForms 控件
- .NET Framework 2.0 (为兼容性目标旧框架)

### 子项目

- **AionLanucher** (`AionLanucher\`):
  - 游戏启动器客户端，与 AionNetGate 服务器通信
  - 具有镜像 packet 系统（CM_* 和 SM_* 类，但角色相反）
  - 实现文件更新验证、外挂检测、端口映射等功能
  - 包含自定义 WinForms UI 组件 (`FormSkin\`)
  - Network 层使用 `NetClient` 连接到网关服务器

- **NetGateReg** (`NetGateReg\`):
  - 注册工具实用程序
  - 独立解决方案

- **protect** (`protect\`):
  - 代码混淆/保护工具
  - `Reactor.dat`: .NET Reactor 混淆器
  - `TMLicense64.dat`: Themida/WinLicense 64位保护工具
  - 通过 post-build 事件自动应用到 Release 构建

### 服务层

项目包含多个专门的服务类（`Services\` 目录）：

- **MainService.cs**: 核心网络监听和连接管理
- **AccountService.cs**: 账号验证和数据库查询
- **DefenseService.cs**: IP 阻止和攻击检测
- **DownFileServer.cs**: 文件下载服务
- **MailService.cs**: SMTP 邮件通知
- **DatabaseManagerService.cs**: 数据库连接管理
- **RemoteManagementService.cs**: 远程管理功能协调
- **SecurityEnhancementService.cs**: 安全增强功能

## 启动器生成器

网关包含一个可视化启动器生成器模块 (`Launcher\DesignLauncher.cs`)：

### 生成流程

1. **UI 设计**: 拖拽调整按钮位置、设置背景图、图标
2. **配置嵌入**: 服务器IP/端口、外挂检测列表、文件校验规则
3. **动态编译**: 使用 `CSharpCodeProvider` 编译源码
4. **代码保护**: Reactor 混淆 + TMD 加壳

### 皮肤系统

皮肤资源位于 `Resources\Skins\` 目录：

```
Resources/
├── Skins/
│   └── BackInBlack/           # 深色动漫风格主题
│       ├── skin.config        # 皮肤配置文件
│       ├── background.png     # 背景图
│       ├── button*.png        # 按钮图片 (normal/hover/pressed)
│       ├── progress_block.png # 进度条背景
│       ├── icon.ico           # 程序图标
│       └── refresh.png        # 刷新按钮图标
└── waigua_merged.txt          # 合并的外挂检测配置
```

### 外挂检测配置格式

```
EXENAME=进程名.exe      # 按进程名检测
EXEMD5=MD5哈希值        # 按文件MD5检测
EXECLASS=窗口类名       # 按窗口类名检测
```

## 通信协议

### 数据包格式

```
[包大小 4字节 int32] [Opcode 1字节] [数据负载 N字节]
加密方式: XOR ^ "煌" (Unicode 0x714C)
```

### Opcode 映射表

| Opcode | 客户端→网关 (CM_) | 网关→客户端 (SM_) |
|--------|------------------|------------------|
| 0x00 | 连接请求 | 连接确认+配置下发 |
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

- **目标框架**: .NET Framework 2.0（为了向后兼容性）
- **语言**: 中文注释和 UI 元素
- **代码保护**: Post-build 事件自动应用混淆（仅 Release 构建）
- **配置持久化**: 基于 Windows 注册表
- **网络模型**: 异步 socket 操作与回调处理
- **依赖管理**: 项目依赖于 `AionCommons.dll`，这是一个共享库，提供网络、日志和实用程序功能
- **目录结构注意**: `Netwok` 目录拼写不标准（应为 "Network"），但保持原样以避免破坏现有引用

## 相关项目

- **Aion Launcher back in black** (`登录器windows\`): 独立启动器项目，UI 素材已整合到本项目的皮肤系统
# AionGate 示例项目

本目录包含各种示例项目，展示如何集成和使用 AionGate 系统。

---

## 📁 示例列表

### 1. 简单C#启动器示例

**目录**: `simple-launcher/`

一个最简单的C#控制台启动器示例，展示基本的更新流程：

```csharp
// Program.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var apiBase = "https://your-gateway.com/api/update";
        var client = new HttpClient { BaseAddress = new Uri(apiBase) };

        // 1. 检查更新
        Console.WriteLine("检查更新中...");
        var checkRequest = new { client_version = "0.0.0.0" };
        var response = await client.PostAsJsonAsync("/check", checkRequest);
        var updateInfo = await response.Content.ReadFromJsonAsync<UpdateCheckResponse>();

        if (!updateInfo.needs_update)
        {
            Console.WriteLine("已是最新版本!");
            return;
        }

        // 2. 显示更新信息
        if (updateInfo.needs_full_client)
        {
            Console.WriteLine($"\n需要下载完整客户端: {updateInfo.latest_version}");
            Console.WriteLine("\n可用下载链接:");

            foreach (var link in updateInfo.full_package_links)
            {
                var recommend = link.is_recommended ? "⭐ " : "";
                Console.WriteLine($"\n{recommend}{link.package_name}");
                Console.WriteLine($"  类型: {link.type_name}");
                Console.WriteLine($"  大小: {link.file_size_text}");
                Console.WriteLine($"  链接: {link.url}");

                if (!string.IsNullOrEmpty(link.verification_code))
                    Console.WriteLine($"  提取码: {link.verification_code}");

                if (!string.IsNullOrEmpty(link.extraction_password))
                    Console.WriteLine($"  解压密码: {link.extraction_password}");

                if (!string.IsNullOrEmpty(link.description))
                    Console.WriteLine($"  说明: {link.description}");
            }

            Console.WriteLine("\n请从上述链接下载完整客户端后再次运行本启动器。");
        }
        else
        {
            Console.WriteLine($"\n发现新版本: {updateInfo.latest_version}");
            Console.WriteLine($"更新大小: {updateInfo.download_size_text}");
            Console.WriteLine($"文件数量: {updateInfo.file_count}");
            Console.WriteLine($"\n更新日志:\n{updateInfo.changelog}");

            Console.Write("\n是否立即更新? (Y/N): ");
            if (Console.ReadLine()?.ToUpper() == "Y")
            {
                // TODO: 实现增量更新下载
                Console.WriteLine("\n开始更新...");
            }
        }
    }
}

public record UpdateCheckResponse(
    bool needs_update,
    bool needs_full_client,
    string current_version,
    string latest_version,
    string update_type,
    bool is_forced,
    int file_count,
    long download_size,
    string download_size_text,
    int estimated_time,
    string changelog,
    List<FullPackageLink> full_package_links
);

public record FullPackageLink(
    long id,
    string version_code,
    string package_name,
    string type,
    string type_name,
    string url,
    string verification_code,
    string extraction_password,
    long file_size,
    string file_size_text,
    string description,
    int priority,
    bool is_recommended,
    int download_count
);
```

**运行方法**:
```bash
dotnet run
```

---

### 2. WPF启动器示例

**目录**: `wpf-launcher/`

完整的WPF启动器示例，包含UI界面和完整的更新流程：

**功能特性**:
- ✅ 美观的现代UI界面
- ✅ 实时进度显示
- ✅ 断点续传支持
- ✅ 并发下载（8线程）
- ✅ Hash校验
- ✅ 暂停/继续/取消
- ✅ 错误重试
- ✅ P2P统计

**运行方法**:
```bash
cd wpf-launcher
dotnet run
```

---

### 3. 管理工具脚本示例

**目录**: `admin-scripts/`

#### 3.1 生成版本清单

```bash
cd admin-scripts
.\generate-manifest.ps1 -GameDir "D:\GameServer\Aion" -Version "2.7.0.16"
```

#### 3.2 批量上传到CDN

```bash
.\upload-to-cdn.ps1 -Provider "AliOSS" -LocalDir "D:\GameServer\Aion\Data"
```

#### 3.3 添加网盘下载链接

```bash
.\add-full-package.ps1 `
  -Version "2.7.0.15" `
  -Name "Aion 2.7 完整客户端 (百度网盘)" `
  -Type "baidu" `
  -Url "https://pan.baidu.com/s/xxxxxx" `
  -Code "abc123" `
  -Password "aion2024" `
  -Size 15728640000 `
  -Priority 100
```

---

### 4. API测试示例

**目录**: `api-tests/`

使用 Postman / curl 测试API的示例：

```bash
# 检查更新
curl -X POST https://your-gateway.com/api/update/check \
  -H "Content-Type: application/json" \
  -d '{"client_version":"2.7.0.15","channel_code":"official"}'

# 获取版本清单
curl https://your-gateway.com/api/update/manifest/2.7.0.16?fromVersion=2.7.0.15

# 获取完整客户端链接
curl https://your-gateway.com/api/update/full-packages/2.7.0.15
```

**Postman Collection**: `api-tests/AionGate-Update-API.postman_collection.json`

---

### 5. Docker部署示例

**目录**: `docker-deployment/`

虽然AionGate主要为Windows设计，但Shop API可以Docker化部署：

```yaml
# docker-compose.yml
version: '3.8'

services:
  aiongate-shop:
    build: .
    ports:
      - "5000:5000"
    environment:
      - ConnectionStrings__AionDB=Server=mssql;Database=AionGameDB;...
      - CDN__Provider=AliOSS
      - CDN__AccessKey=...
    depends_on:
      - mssql
      - redis

  mssql:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!

  redis:
    image: redis:7-alpine
```

**运行方法**:
```bash
docker-compose up -d
```

---

### 6. 数据库管理示例

**目录**: `database-tools/`

#### 6.1 自动备份脚本

```powershell
# backup-database.ps1
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
sqlcmd -S localhost -Q "BACKUP DATABASE [AionGameDB] TO DISK = 'D:\Backups\AionGate_$timestamp.bak' WITH COMPRESSION"
```

#### 6.2 查询统计

```sql
-- stats-queries.sql

-- 查看今日更新统计
SELECT
    COUNT(*) AS total_updates,
    COUNT(CASE WHEN status = 1 THEN 1 END) AS success,
    COUNT(CASE WHEN status = 2 THEN 1 END) AS failed,
    AVG(download_speed) AS avg_speed,
    SUM(downloaded_size) / 1073741824.0 AS total_gb
FROM client_update_logs
WHERE CAST(started_at AS DATE) = CAST(GETDATE() AS DATE);

-- 查看网盘下载次数排行
SELECT TOP 10
    package_name,
    type_name,
    download_count,
    priority
FROM client_full_packages
WHERE is_active = 1
ORDER BY download_count DESC;
```

---

### 7. 性能测试示例

**目录**: `performance-tests/`

使用 JMeter / Locust 进行压力测试：

```python
# locustfile.py
from locust import HttpUser, task, between

class UpdateAPIUser(HttpUser):
    wait_time = between(1, 3)

    @task
    def check_update(self):
        self.client.post("/api/update/check", json={
            "client_version": "2.7.0.15",
            "channel_code": "official"
        })

    @task
    def get_manifest(self):
        self.client.get("/api/update/manifest/2.7.0.16?fromVersion=2.7.0.15")
```

**运行方法**:
```bash
locust -f locustfile.py --host=https://your-gateway.com
```

---

## 🚀 快速开始

1. **选择适合你的示例**：
   - 新手：从 `simple-launcher` 开始
   - WPF开发者：查看 `wpf-launcher`
   - 服务器管理员：使用 `admin-scripts`

2. **修改配置**：
   - 将示例中的 `your-gateway.com` 替换为你的实际域名
   - 修改数据库连接字符串

3. **运行示例**：
   ```bash
   cd examples/simple-launcher
   dotnet run
   ```

---

## 📚 相关文档

- [API文档](../docs/UPDATE_API.md)
- [启动器集成指南](../docs/LAUNCHER_INTEGRATION.md)
- [部署指南](../docs/DEPLOYMENT.md)
- [README](../README.md)

---

## 💡 贡献示例

欢迎提交你的示例项目！

1. Fork本仓库
2. 在 `examples/` 下创建你的示例目录
3. 添加README说明
4. 提交Pull Request

---

## ❓ 常见问题

**Q: 示例项目可以直接用于生产环境吗？**

A: 示例项目仅供学习和参考，生产环境需要：
- 添加完善的错误处理
- 实现日志记录
- 添加安全验证
- 进行充分的测试

**Q: 如何调试示例代码？**

A: 在Visual Studio或VS Code中打开示例项目，设置断点后按F5调试。

**Q: 示例支持哪些.NET版本？**

A: 所有示例基于 .NET 9.0，部分示例向下兼容 .NET 6.0+。

---

## 📧 技术支持

如有问题，请：
1. 查看示例代码中的注释
2. 阅读相关文档
3. 提交 [Issue](https://github.com/xiaohan1105/AionNetGate/issues)

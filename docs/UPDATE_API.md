# 热更新 API 文档

## 概述

AionGate 热更新系统提供完整的客户端更新解决方案，支持：
- ✅ 完整客户端网盘分发（降本增效）
- ✅ 增量更新（节省88%+带宽）
- ✅ CDN加速下载（多节点负载均衡）
- ✅ P2P分流（降低CDN成本20-40%）
- ✅ 实时进度跟踪
- ✅ 断点续传

## API 基础信息

- **Base URL**: `https://your-gateway.com/api/update`
- **Content-Type**: `application/json`
- **认证**: 部分接口需要JWT Token

---

## 1. 检查更新

检查是否有新版本可用，返回更新信息。

### 请求

```http
POST /api/update/check
Content-Type: application/json

{
  "client_version": "2.7.0.15",
  "channel_code": "official",
  "hardware_id": "ABC123DEF456"
}
```

### 请求参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| client_version | string | 是 | 当前客户端版本，首次安装填 "0.0.0.0" |
| channel_code | string | 否 | 渠道代码，用于渠道追踪 |
| hardware_id | string | 否 | 硬件ID，用于统计 |

### 响应

#### 场景A：需要完整客户端（新用户）

```json
{
  "needs_update": true,
  "needs_full_client": true,
  "current_version": "0.0.0.0",
  "latest_version": "2.7.0.15",
  "update_type": "full",
  "is_forced": true,
  "file_count": 0,
  "download_size": 0,
  "download_size_text": "0 B",
  "estimated_time": 0,
  "changelog": null,
  "full_package_links": [
    {
      "id": 1,
      "version_code": "2.7.0.15",
      "package_name": "Aion 2.7 完整客户端 (百度网盘)",
      "type": "baidu",
      "type_name": "百度网盘",
      "url": "https://pan.baidu.com/s/xxxxxx",
      "verification_code": "abc123",
      "extraction_password": "aion2024",
      "file_size": 15728640000,
      "file_size_text": "14.65 GB",
      "description": "推荐下载。解压后运行登录器即可。",
      "priority": 100,
      "is_recommended": true,
      "download_count": 1523
    },
    {
      "id": 2,
      "version_code": "2.7.0.15",
      "package_name": "Aion 2.7 完整客户端 (阿里云盘)",
      "type": "aliyun",
      "type_name": "阿里云盘",
      "url": "https://www.aliyundrive.com/s/xxxxxx",
      "verification_code": "xyz789",
      "extraction_password": "aion2024",
      "file_size": 15728640000,
      "file_size_text": "14.65 GB",
      "description": "下载速度快，推荐。",
      "priority": 95,
      "is_recommended": true,
      "download_count": 892
    }
  ]
}
```

#### 场景B：需要增量更新（老用户）

```json
{
  "needs_update": true,
  "needs_full_client": false,
  "current_version": "2.7.0.15",
  "latest_version": "2.7.0.16",
  "update_type": "incremental",
  "is_forced": false,
  "file_count": 184,
  "download_size": 1878507520,
  "download_size_text": "1.75 GB",
  "estimated_time": 300,
  "changelog": "# 2.7.0.16 更新内容\n\n- 新增: 全新副本「永恒之塔」\n- 优化: 提升战斗流畅度\n- 修复: 修复已知BUG",
  "full_package_links": null
}
```

#### 场景C：已是最新版本

```json
{
  "needs_update": false,
  "needs_full_client": false,
  "current_version": "2.7.0.16",
  "latest_version": "2.7.0.16",
  "update_type": "none",
  "is_forced": false,
  "file_count": 0,
  "download_size": 0,
  "download_size_text": "0 B",
  "estimated_time": 0,
  "changelog": null,
  "full_package_links": null
}
```

---

## 2. 获取版本清单

获取指定版本的文件清单，用于增量更新。

### 请求

```http
GET /api/update/manifest/2.7.0.16?fromVersion=2.7.0.15
```

### 请求参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| versionCode | string | 是 | 目标版本号（URL路径参数） |
| fromVersion | string | 否 | 当前版本号（Query参数），提供后返回增量清单 |

### 响应

```json
{
  "version_code": "2.7.0.16",
  "version_name": "永恒之塔更新",
  "file_count": 184,
  "total_size": 1878507520,
  "download_size": 1878507520,
  "is_forced": false,
  "manifest_hash": "a1b2c3d4e5f6...",
  "files": [
    {
      "file_path": "Data/Levels/level1.pak",
      "file_size": 1048576000,
      "file_hash": "sha256:a1b2c3...",
      "file_crc32": "12345678",
      "compressed_size": null,
      "compression_type": null,
      "cdn_url": "https://cdn.yourdomain.com/Data/Levels/level1.pak?OSSAccessKeyId=LTAI5...&Expires=1732588800&Signature=xxxxx",
      "is_critical": true,
      "download_priority": 100
    },
    {
      "file_path": "Data/Textures/texture1.dds",
      "file_size": 8388608,
      "file_hash": "sha256:d4e5f6...",
      "file_crc32": "87654321",
      "compressed_size": null,
      "compression_type": null,
      "cdn_url": "https://cdn.yourdomain.com/Data/Textures/texture1.dds?OSSAccessKeyId=LTAI5...&Expires=1732588800&Signature=yyyyy",
      "is_critical": false,
      "download_priority": 50
    }
  ]
}
```

**注意**：
- `cdn_url` 有1小时有效期，过期后需重新请求
- 按 `download_priority` 从高到低下载，核心文件优先
- 下载完成后必须校验 `file_hash` 和 `file_crc32`

---

## 3. 记录更新开始

记录客户端开始更新，用于统计和进度跟踪。

### 请求

```http
POST /api/update/start
Content-Type: application/json

{
  "account_id": 123456,
  "channel_code": "official",
  "from_version": "2.7.0.15",
  "to_version": "2.7.0.16",
  "total_files": 184,
  "total_size": 1878507520,
  "use_p2p": true
}
```

### 请求参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| account_id | number | 否 | 账号ID（已登录时提供） |
| channel_code | string | 否 | 渠道代码 |
| from_version | string | 否 | 当前版本 |
| to_version | string | 是 | 目标版本 |
| total_files | number | 是 | 总文件数 |
| total_size | number | 是 | 总大小（字节） |
| use_p2p | boolean | 是 | 是否启用P2P |

### 响应

```json
{
  "log_id": 789012
}
```

**保存 `log_id` 用于后续进度上报。**

---

## 4. 上报更新进度

实时上报更新进度，用于统计和监控。

### 请求

```http
POST /api/update/progress
Content-Type: application/json

{
  "log_id": 789012,
  "downloaded_files": 92,
  "downloaded_size": 939253760,
  "status": 0,
  "download_speed": 52.5,
  "p2p_ratio": 0.25,
  "error_message": null
}
```

### 请求参数

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| log_id | number | 是 | 更新日志ID（从 `/start` 接口获取） |
| downloaded_files | number | 是 | 已下载文件数 |
| downloaded_size | number | 是 | 已下载大小（字节） |
| status | number | 是 | 状态：0=下载中, 1=已完成, 2=失败, 3=取消 |
| download_speed | number | 否 | 下载速度（MB/s） |
| p2p_ratio | number | 否 | P2P分享率（0-1） |
| error_message | string | 否 | 错误消息（失败时提供） |

### 响应

```json
{
  "success": true
}
```

**建议每下载5个文件或每5秒上报一次进度。**

---

## 5. 获取完整客户端下载链接

直接获取完整客户端的网盘下载链接（不通过检查更新）。

### 请求

```http
GET /api/update/full-packages/2.7.0.15
```

### 响应

```json
[
  {
    "id": 1,
    "version_code": "2.7.0.15",
    "package_name": "Aion 2.7 完整客户端 (百度网盘)",
    "type": "baidu",
    "type_name": "百度网盘",
    "url": "https://pan.baidu.com/s/xxxxxx",
    "verification_code": "abc123",
    "extraction_password": "aion2024",
    "file_size": 15728640000,
    "file_size_text": "14.65 GB",
    "description": "推荐下载。解压后运行登录器即可。",
    "priority": 100,
    "is_recommended": true,
    "download_count": 1523
  }
]
```

---

## 6. 记录完整客户端下载

记录用户点击了某个网盘下载链接，用于统计。

### 请求

```http
POST /api/update/full-packages/1/download
```

### 响应

```json
{
  "success": true
}
```

---

## 7. 获取CDN节点列表

获取所有可用的CDN节点，客户端可自动选择最快节点。

### 请求

```http
GET /api/update/cdn-nodes
```

### 响应

```json
[
  {
    "id": 1,
    "node_name": "阿里云OSS-华东",
    "provider": "oss",
    "region": "cn-hangzhou",
    "endpoint": "oss-cn-hangzhou.aliyuncs.com",
    "cdn_domain": "cdn.yourdomain.com",
    "priority": 100,
    "bandwidth_limit": 1000
  },
  {
    "id": 2,
    "node_name": "腾讯云COS-华北",
    "provider": "cos",
    "region": "ap-beijing",
    "endpoint": "cos.ap-beijing.myqcloud.com",
    "cdn_domain": "cdn2.yourdomain.com",
    "priority": 90,
    "bandwidth_limit": 800
  }
]
```

---

## 完整更新流程示例

### 场景A：新用户首次安装

```typescript
// 1. 检查更新
const checkResult = await fetch('/api/update/check', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    client_version: '0.0.0.0',
    channel_code: 'official'
  })
});

const updateInfo = await checkResult.json();

if (updateInfo.needs_full_client) {
  // 2. 显示网盘下载链接给用户
  const links = updateInfo.full_package_links;

  // 按优先级排序，推荐的在前面
  links.sort((a, b) => b.priority - a.priority);

  // 展示下载页面
  showDownloadLinks(links);

  // 3. 用户点击链接时记录
  links.forEach(link => {
    link.onClick = async () => {
      await fetch(`/api/update/full-packages/${link.id}/download`, {
        method: 'POST'
      });

      // 打开网盘链接
      window.open(link.url);

      // 显示提取码和解压密码
      showDownloadInstructions({
        verificationCode: link.verification_code,
        extractionPassword: link.extraction_password,
        description: link.description
      });
    };
  });
}
```

### 场景B：老用户增量更新

```typescript
// 1. 检查更新
const checkResult = await fetch('/api/update/check', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    client_version: '2.7.0.15',
    channel_code: 'official'
  })
});

const updateInfo = await checkResult.json();

if (!updateInfo.needs_update) {
  console.log('已是最新版本');
  return;
}

// 2. 获取版本清单
const manifestResult = await fetch(
  `/api/update/manifest/${updateInfo.latest_version}?fromVersion=2.7.0.15`
);
const manifest = await manifestResult.json();

// 3. 记录更新开始
const startResult = await fetch('/api/update/start', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    from_version: '2.7.0.15',
    to_version: updateInfo.latest_version,
    total_files: manifest.file_count,
    total_size: manifest.total_size,
    use_p2p: true
  })
});

const { log_id } = await startResult.json();

// 4. 下载文件
let downloadedFiles = 0;
let downloadedSize = 0;
const startTime = Date.now();

for (const file of manifest.files) {
  try {
    // 下载文件
    await downloadFile(file.cdn_url, file.file_path);

    // 校验文件
    const localHash = await calculateFileHash(file.file_path);
    if (localHash !== file.file_hash) {
      throw new Error('文件校验失败');
    }

    downloadedFiles++;
    downloadedSize += file.file_size;

    // 每5个文件上报一次进度
    if (downloadedFiles % 5 === 0) {
      const elapsedSeconds = (Date.now() - startTime) / 1000;
      const speed = (downloadedSize / 1048576) / elapsedSeconds; // MB/s

      await fetch('/api/update/progress', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          log_id,
          downloaded_files: downloadedFiles,
          downloaded_size: downloadedSize,
          status: 0, // 下载中
          download_speed: speed
        })
      });
    }

  } catch (error) {
    // 上报失败
    await fetch('/api/update/progress', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        log_id,
        downloaded_files: downloadedFiles,
        downloaded_size: downloadedSize,
        status: 2, // 失败
        error_message: error.message
      })
    });
    throw error;
  }
}

// 5. 上报完成
await fetch('/api/update/progress', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    log_id,
    downloaded_files: downloadedFiles,
    downloaded_size: downloadedSize,
    status: 1 // 完成
  })
});

console.log('更新完成！');
```

---

## 错误处理

所有API接口在出错时返回统一格式：

```json
{
  "error": "错误类型",
  "message": "详细错误信息"
}
```

常见错误码：

- **400 Bad Request**: 请求参数错误
- **404 Not Found**: 版本不存在
- **500 Internal Server Error**: 服务器内部错误

---

## 最佳实践

1. **断点续传**：保存下载进度，失败后从断点继续
2. **并发下载**：同时下载8个文件，提升速度
3. **优先级下载**：按 `download_priority` 排序，核心文件优先
4. **Hash校验**：下载完成后必须校验 `file_hash`
5. **URL刷新**：CDN URL 1小时后过期，需重新请求清单
6. **进度上报**：每5秒或每5个文件上报一次
7. **错误重试**：失败后最多重试3次
8. **用户体验**：显示详细进度（文件数、速度、剩余时间）

---

## 技术支持

如有问题，请联系技术支持或查看源码：
- GitHub: https://github.com/xiaohan1105/AionNetGate
- 文档: README.md

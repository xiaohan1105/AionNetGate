-- ===========================================
-- AionGate 热更新系统 (CDN + 增量更新)
-- ===========================================
-- 解决客户端大文件更新带宽问题
-- 支持CDN加速、增量更新、断点续传、P2P分发

-- ===========================================
-- 完整客户端下载链接表（网盘分发）
-- ===========================================
CREATE TABLE client_full_packages (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    version_code VARCHAR(32) NOT NULL,
    package_name NVARCHAR(100) NOT NULL,  -- 如: Aion 2.7完整客户端
    file_size BIGINT NOT NULL,  -- 文件大小（字节）
    download_type VARCHAR(32) NOT NULL,  -- baidu|aliyun|thunder|115|mega|direct
    download_url NVARCHAR(500) NOT NULL,  -- 下载链接
    extraction_password NVARCHAR(50),  -- 解压密码
    verification_code NVARCHAR(10),  -- 提取码
    md5_hash VARCHAR(32),  -- MD5校验
    description NVARCHAR(MAX),  -- 说明（安装步骤等）
    download_count INT NOT NULL DEFAULT 0,  -- 下载次数
    is_active BIT NOT NULL DEFAULT 1,  -- 是否有效
    priority INT NOT NULL DEFAULT 50,  -- 优先级（推荐顺序）
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_client_packages_version ON client_full_packages(version_code, is_active);
CREATE INDEX idx_client_packages_priority ON client_full_packages(priority DESC);

-- 版本管理表（只管增量更新）
CREATE TABLE update_versions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    version_code VARCHAR(32) NOT NULL UNIQUE,  -- 版本号 如: 2.7.0.15
    version_name NVARCHAR(100) NOT NULL,  -- 版本名称
    version_type TINYINT NOT NULL DEFAULT 2,  -- 2=增量包（默认） 3=补丁包 4=热修复
    base_version VARCHAR(32),  -- 基于哪个版本
    description NVARCHAR(MAX),  -- 更新说明（详细更新内容）
    changelog NVARCHAR(MAX),  -- 更新日志（用户可见）
    file_count INT NOT NULL DEFAULT 0,  -- 文件数量
    total_size BIGINT NOT NULL DEFAULT 0,  -- 总大小（字节）
    download_size BIGINT NOT NULL DEFAULT 0,  -- 实际下载大小（压缩后）
    is_forced BIT NOT NULL DEFAULT 0,  -- 是否强制更新
    is_published BIT NOT NULL DEFAULT 0,  -- 是否已发布
    min_client_version VARCHAR(32),  -- 最低客户端版本要求
    cdn_provider VARCHAR(32) NOT NULL DEFAULT 'oss',  -- cdn|oss|s3|r2|local
    cdn_bucket VARCHAR(100),  -- CDN桶名
    cdn_region VARCHAR(50),  -- CDN区域
    manifest_url NVARCHAR(500),  -- 清单文件URL
    manifest_hash VARCHAR(64),  -- 清单文件Hash
    estimated_time INT,  -- 预计更新时间（秒）
    published_at DATETIME,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_update_versions_code ON update_versions(version_code);
CREATE INDEX idx_update_versions_published ON update_versions(is_published, created_at DESC);

-- 版本文件清单表
CREATE TABLE update_files (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    version_id BIGINT NOT NULL,
    file_path NVARCHAR(500) NOT NULL,  -- 相对路径 如: Data/Levels/level1.pak
    file_size BIGINT NOT NULL,  -- 文件大小
    file_hash VARCHAR(64) NOT NULL,  -- 文件Hash (SHA256)
    file_crc32 VARCHAR(8) NOT NULL,  -- CRC32 校验码
    compressed_size BIGINT,  -- 压缩后大小
    compression_type VARCHAR(20),  -- 压缩类型: gzip|brotli|none
    cdn_url NVARCHAR(500),  -- CDN完整URL (已签名)
    cdn_key NVARCHAR(500),  -- CDN对象Key
    url_expires_at DATETIME,  -- URL过期时间
    is_critical BIT NOT NULL DEFAULT 0,  -- 是否核心文件（必须下载）
    download_priority INT NOT NULL DEFAULT 50,  -- 下载优先级 0-100
    chunk_size INT,  -- 分块大小（大文件分块下载）
    chunk_count INT,  -- 分块数量
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_update_files_version FOREIGN KEY (version_id) REFERENCES update_versions(id) ON DELETE CASCADE
);

CREATE INDEX idx_update_files_version ON update_files(version_id);
CREATE INDEX idx_update_files_hash ON update_files(file_hash);
CREATE INDEX idx_update_files_priority ON update_files(download_priority DESC);

-- 文件差分表 (增量更新)
CREATE TABLE update_file_diffs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    from_version_id BIGINT NOT NULL,
    to_version_id BIGINT NOT NULL,
    file_path NVARCHAR(500) NOT NULL,
    diff_type TINYINT NOT NULL,  -- 1=新增 2=修改 3=删除
    old_hash VARCHAR(64),
    new_hash VARCHAR(64),
    diff_size BIGINT,  -- 差分包大小
    diff_url NVARCHAR(500),  -- 差分包URL
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_update_diffs_from FOREIGN KEY (from_version_id) REFERENCES update_versions(id),
    CONSTRAINT FK_update_diffs_to FOREIGN KEY (to_version_id) REFERENCES update_versions(id)
);

CREATE INDEX idx_update_diffs_versions ON update_file_diffs(from_version_id, to_version_id);

-- 客户端更新记录表
CREATE TABLE client_update_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    account_id BIGINT,
    channel_code VARCHAR(32),
    from_version VARCHAR(32),
    to_version VARCHAR(32) NOT NULL,
    update_type TINYINT NOT NULL,  -- 1=完整更新 2=增量更新
    total_files INT NOT NULL,
    downloaded_files INT NOT NULL DEFAULT 0,
    total_size BIGINT NOT NULL,
    downloaded_size BIGINT NOT NULL DEFAULT 0,
    status TINYINT NOT NULL DEFAULT 0,  -- 0=下载中 1=已完成 2=失败 3=取消
    error_message NVARCHAR(500),
    client_ip VARCHAR(45),
    download_speed FLOAT,  -- MB/s
    time_elapsed INT,  -- 耗时（秒）
    use_p2p BIT NOT NULL DEFAULT 0,  -- 是否使用P2P
    p2p_ratio DECIMAL(5,2),  -- P2P分享率
    started_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    completed_at DATETIME,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_client_update_account ON client_update_logs(account_id);
CREATE INDEX idx_client_update_status ON client_update_logs(status, started_at DESC);
CREATE INDEX idx_client_update_version ON client_update_logs(to_version);

-- CDN节点配置表
CREATE TABLE cdn_nodes (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    node_name NVARCHAR(100) NOT NULL,
    provider VARCHAR(32) NOT NULL,  -- oss|s3|r2|cloudflare|local
    region VARCHAR(50),
    endpoint NVARCHAR(500) NOT NULL,
    bucket_name VARCHAR(100),
    access_key NVARCHAR(200),  -- 加密存储
    secret_key NVARCHAR(200),  -- 加密存储
    cdn_domain NVARCHAR(200),  -- CDN加速域名
    is_enabled BIT NOT NULL DEFAULT 1,
    priority INT NOT NULL DEFAULT 50,  -- 优先级
    bandwidth_limit INT,  -- 带宽限制 (Mbps)
    storage_used BIGINT NOT NULL DEFAULT 0,  -- 已用存储（字节）
    storage_limit BIGINT,  -- 存储限制
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_cdn_nodes_enabled ON cdn_nodes(is_enabled, priority DESC);

-- P2P节点表 (客户端上传统计)
CREATE TABLE p2p_nodes (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    account_id BIGINT,
    node_id VARCHAR(64) NOT NULL UNIQUE,  -- 节点唯一标识
    version_code VARCHAR(32) NOT NULL,
    client_ip VARCHAR(45) NOT NULL,
    port INT NOT NULL,
    upload_speed FLOAT,  -- 上传速度 MB/s
    total_uploaded BIGINT NOT NULL DEFAULT 0,  -- 累计上传（字节）
    total_downloaded BIGINT NOT NULL DEFAULT 0,  -- 累计下载
    share_ratio DECIMAL(5,2),  -- 分享率
    is_active BIT NOT NULL DEFAULT 1,
    last_seen_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_p2p_nodes_version ON p2p_nodes(version_code, is_active);
CREATE INDEX idx_p2p_nodes_account ON p2p_nodes(account_id);

-- 更新统计表 (每日)
CREATE TABLE update_daily_stats (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    stat_date DATE NOT NULL,
    version_code VARCHAR(32) NOT NULL,
    update_count INT NOT NULL DEFAULT 0,  -- 更新次数
    success_count INT NOT NULL DEFAULT 0,  -- 成功次数
    failed_count INT NOT NULL DEFAULT 0,  -- 失败次数
    total_downloaded BIGINT NOT NULL DEFAULT 0,  -- 总下载量（字节）
    cdn_downloaded BIGINT NOT NULL DEFAULT 0,  -- CDN下载量
    p2p_downloaded BIGINT NOT NULL DEFAULT 0,  -- P2P下载量
    avg_speed FLOAT,  -- 平均速度 MB/s
    avg_time INT,  -- 平均耗时（秒）
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE UNIQUE INDEX idx_update_daily_stats_unique ON update_daily_stats(stat_date, version_code);

GO

-- ===========================================
-- 存储过程: 生成版本清单
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GenerateVersionManifest
    @VersionID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 返回版本基本信息
    SELECT
        v.version_code,
        v.version_name,
        v.version_type,
        v.base_version,
        v.description,
        v.file_count,
        v.total_size,
        v.download_size,
        v.is_forced,
        v.min_client_version,
        v.manifest_hash
    FROM update_versions v
    WHERE v.id = @VersionID AND v.is_published = 1;

    -- 返回文件清单
    SELECT
        f.file_path,
        f.file_size,
        f.file_hash,
        f.file_crc32,
        f.compressed_size,
        f.compression_type,
        f.is_critical,
        f.download_priority,
        f.chunk_size,
        f.chunk_count
    FROM update_files f
    WHERE f.version_id = @VersionID
    ORDER BY f.download_priority DESC, f.file_size ASC;
END
GO

-- ===========================================
-- 存储过程: 生成CDN签名URL
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GenerateCDNUrl
    @FileID BIGINT,
    @ExpiresInMinutes INT = 60
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CDNProvider VARCHAR(32);
    DECLARE @CDNDomain NVARCHAR(200);
    DECLARE @CDNKey NVARCHAR(500);
    DECLARE @ExpiresAt DATETIME;

    SET @ExpiresAt = DATEADD(MINUTE, @ExpiresInMinutes, GETUTCDATE());

    -- 获取文件和CDN信息
    SELECT TOP 1
        @CDNProvider = v.cdn_provider,
        @CDNDomain = c.cdn_domain,
        @CDNKey = f.cdn_key
    FROM update_files f
    INNER JOIN update_versions v ON f.version_id = v.id
    LEFT JOIN cdn_nodes c ON c.provider = v.cdn_provider AND c.is_enabled = 1
    WHERE f.id = @FileID
    ORDER BY c.priority DESC;

    -- 更新文件URL（实际签名需要在应用层完成）
    UPDATE update_files
    SET url_expires_at = @ExpiresAt
    WHERE id = @FileID;

    -- 返回URL信息
    SELECT
        @CDNProvider AS provider,
        @CDNDomain AS domain,
        @CDNKey AS cdn_key,
        @ExpiresAt AS expires_at;
END
GO

-- ===========================================
-- 存储过程: 计算增量更新
-- ===========================================
CREATE OR ALTER PROCEDURE sp_CalculateDiffUpdate
    @FromVersionID BIGINT,
    @ToVersionID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 清空旧的差分记录
    DELETE FROM update_file_diffs
    WHERE from_version_id = @FromVersionID AND to_version_id = @ToVersionID;

    -- 计算新增文件
    INSERT INTO update_file_diffs (from_version_id, to_version_id, file_path, diff_type, new_hash)
    SELECT
        @FromVersionID,
        @ToVersionID,
        f2.file_path,
        1, -- 新增
        f2.file_hash
    FROM update_files f2
    WHERE f2.version_id = @ToVersionID
        AND NOT EXISTS (
            SELECT 1 FROM update_files f1
            WHERE f1.version_id = @FromVersionID
                AND f1.file_path = f2.file_path
        );

    -- 计算修改文件
    INSERT INTO update_file_diffs (from_version_id, to_version_id, file_path, diff_type, old_hash, new_hash)
    SELECT
        @FromVersionID,
        @ToVersionID,
        f2.file_path,
        2, -- 修改
        f1.file_hash,
        f2.file_hash
    FROM update_files f2
    INNER JOIN update_files f1 ON f1.file_path = f2.file_path AND f1.version_id = @FromVersionID
    WHERE f2.version_id = @ToVersionID
        AND f1.file_hash <> f2.file_hash;

    -- 计算删除文件
    INSERT INTO update_file_diffs (from_version_id, to_version_id, file_path, diff_type, old_hash)
    SELECT
        @FromVersionID,
        @ToVersionID,
        f1.file_path,
        3, -- 删除
        f1.file_hash
    FROM update_files f1
    WHERE f1.version_id = @FromVersionID
        AND NOT EXISTS (
            SELECT 1 FROM update_files f2
            WHERE f2.version_id = @ToVersionID
                AND f2.file_path = f1.file_path
        );

    -- 返回差分统计
    SELECT
        COUNT(CASE WHEN diff_type = 1 THEN 1 END) AS new_files,
        COUNT(CASE WHEN diff_type = 2 THEN 1 END) AS modified_files,
        COUNT(CASE WHEN diff_type = 3 THEN 1 END) AS deleted_files,
        COUNT(*) AS total_diffs
    FROM update_file_diffs
    WHERE from_version_id = @FromVersionID AND to_version_id = @ToVersionID;
END
GO

-- ===========================================
-- 存储过程: 记录客户端更新开始
-- ===========================================
CREATE OR ALTER PROCEDURE sp_StartClientUpdate
    @AccountID BIGINT,
    @ChannelCode VARCHAR(32),
    @FromVersion VARCHAR(32),
    @ToVersion VARCHAR(32),
    @UpdateType TINYINT,
    @TotalFiles INT,
    @TotalSize BIGINT,
    @ClientIP VARCHAR(45),
    @UseP2P BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO client_update_logs (
        account_id, channel_code, from_version, to_version,
        update_type, total_files, total_size, status,
        client_ip, use_p2p, started_at
    )
    VALUES (
        @AccountID, @ChannelCode, @FromVersion, @ToVersion,
        @UpdateType, @TotalFiles, @TotalSize, 0,
        @ClientIP, @UseP2P, GETUTCDATE()
    );

    RETURN SCOPE_IDENTITY();
END
GO

-- ===========================================
-- 存储过程: 更新客户端更新进度
-- ===========================================
CREATE OR ALTER PROCEDURE sp_UpdateClientUpdateProgress
    @LogID BIGINT,
    @DownloadedFiles INT,
    @DownloadedSize BIGINT,
    @Status TINYINT,
    @DownloadSpeed FLOAT = NULL,
    @P2PRatio DECIMAL(5,2) = NULL,
    @ErrorMessage NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartedAt DATETIME;
    DECLARE @TimeElapsed INT;

    SELECT @StartedAt = started_at FROM client_update_logs WHERE id = @LogID;
    SET @TimeElapsed = DATEDIFF(SECOND, @StartedAt, GETUTCDATE());

    UPDATE client_update_logs
    SET
        downloaded_files = @DownloadedFiles,
        downloaded_size = @DownloadedSize,
        status = @Status,
        download_speed = ISNULL(@DownloadSpeed, download_speed),
        p2p_ratio = ISNULL(@P2PRatio, p2p_ratio),
        time_elapsed = @TimeElapsed,
        error_message = @ErrorMessage,
        completed_at = CASE WHEN @Status IN (1, 2, 3) THEN GETUTCDATE() ELSE completed_at END
    WHERE id = @LogID;
END
GO

-- ===========================================
-- 存储过程: 获取最新版本信息
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GetLatestVersion
    @ClientVersion VARCHAR(32) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        v.id,
        v.version_code,
        v.version_name,
        v.version_type,
        v.description,
        v.file_count,
        v.total_size,
        v.download_size,
        v.is_forced,
        v.manifest_url,
        v.manifest_hash,
        v.published_at
    FROM update_versions v
    WHERE v.is_published = 1
        AND (@ClientVersion IS NULL OR v.version_code > @ClientVersion)
    ORDER BY v.published_at DESC, v.id DESC;
END
GO

-- ===========================================
-- 存储过程: 查询更新统计
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GetUpdateStatistics
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE));

    IF @EndDate IS NULL
        SET @EndDate = CAST(GETUTCDATE() AS DATE);

    -- 总体统计
    SELECT
        COUNT(*) AS total_updates,
        COUNT(CASE WHEN status = 1 THEN 1 END) AS success_count,
        COUNT(CASE WHEN status = 2 THEN 1 END) AS failed_count,
        SUM(downloaded_size) AS total_downloaded,
        AVG(download_speed) AS avg_speed,
        AVG(time_elapsed) AS avg_time,
        SUM(CASE WHEN use_p2p = 1 THEN 1 ELSE 0 END) AS p2p_users,
        AVG(CASE WHEN use_p2p = 1 THEN p2p_ratio END) AS avg_p2p_ratio
    FROM client_update_logs
    WHERE CAST(started_at AS DATE) >= @StartDate
        AND CAST(started_at AS DATE) <= @EndDate;

    -- 每日统计
    SELECT
        CAST(started_at AS DATE) AS date,
        COUNT(*) AS update_count,
        COUNT(CASE WHEN status = 1 THEN 1 END) AS success_count,
        SUM(downloaded_size) AS total_downloaded,
        AVG(download_speed) AS avg_speed
    FROM client_update_logs
    WHERE CAST(started_at AS DATE) >= @StartDate
        AND CAST(started_at AS DATE) <= @EndDate
    GROUP BY CAST(started_at AS DATE)
    ORDER BY CAST(started_at AS DATE) DESC;
END
GO

-- ===========================================
-- 存储过程: 检查更新（返回更新信息和网盘链接）
-- ===========================================
CREATE OR ALTER PROCEDURE sp_CheckForUpdate
    @ClientVersion VARCHAR(32),
    @ChannelCode VARCHAR(32) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LatestVersionCode VARCHAR(32);
    DECLARE @LatestVersionID BIGINT;
    DECLARE @NeedsUpdate BIT = 0;
    DECLARE @NeedsFullClient BIT = 0;

    -- 获取最新版本
    SELECT TOP 1
        @LatestVersionCode = version_code,
        @LatestVersionID = id
    FROM update_versions
    WHERE is_published = 1
    ORDER BY published_at DESC, id DESC;

    -- 判断是否需要更新
    IF @LatestVersionCode > @ClientVersion
        SET @NeedsUpdate = 1;

    -- 判断是否需要完整客户端（版本相差太大或没有增量包）
    IF @ClientVersion IS NULL OR @ClientVersion = '' OR @ClientVersion = '0.0.0.0'
        SET @NeedsFullClient = 1;

    -- 返回更新检查结果
    SELECT
        @NeedsUpdate AS needs_update,
        @NeedsFullClient AS needs_full_client,
        @ClientVersion AS current_version,
        @LatestVersionCode AS latest_version,
        v.version_type,
        CASE v.version_type WHEN 2 THEN 'incremental' WHEN 3 THEN 'patch' WHEN 4 THEN 'hotfix' ELSE 'full' END AS update_type,
        v.is_forced,
        v.file_count,
        v.download_size,
        CASE
            WHEN v.download_size >= 1073741824 THEN CAST(v.download_size / 1073741824.0 AS DECIMAL(10,2)) + ' GB'
            WHEN v.download_size >= 1048576 THEN CAST(v.download_size / 1048576.0 AS DECIMAL(10,2)) + ' MB'
            WHEN v.download_size >= 1024 THEN CAST(v.download_size / 1024.0 AS DECIMAL(10,2)) + ' KB'
            ELSE CAST(v.download_size AS VARCHAR) + ' B'
        END AS download_size_text,
        v.estimated_time,
        v.changelog
    FROM update_versions v
    WHERE v.id = @LatestVersionID;

    -- 如果需要完整客户端，返回网盘下载链接
    IF @NeedsFullClient = 1
    BEGIN
        SELECT
            id,
            version_code,
            package_name,
            download_type,
            download_type AS type,
            CASE download_type
                WHEN 'baidu' THEN N'百度网盘'
                WHEN 'aliyun' THEN N'阿里云盘'
                WHEN 'thunder' THEN N'迅雷云盘'
                WHEN '115' THEN N'115网盘'
                WHEN 'mega' THEN N'MEGA网盘'
                WHEN 'direct' THEN N'直链下载'
                ELSE N'其他'
            END AS type_name,
            download_url AS url,
            verification_code,
            extraction_password,
            file_size,
            CASE
                WHEN file_size >= 1073741824 THEN CAST(file_size / 1073741824.0 AS DECIMAL(10,2)) + ' GB'
                WHEN file_size >= 1048576 THEN CAST(file_size / 1048576.0 AS DECIMAL(10,2)) + ' MB'
                ELSE CAST(file_size / 1024.0 AS DECIMAL(10,2)) + ' KB'
            END AS file_size_text,
            description,
            priority,
            CASE WHEN priority >= 90 THEN 1 ELSE 0 END AS is_recommended
        FROM client_full_packages
        WHERE version_code = @LatestVersionCode
            AND is_active = 1
        ORDER BY priority DESC, id DESC;
    END
END
GO

-- ===========================================
-- 存储过程: 添加/更新完整客户端下载链接
-- ===========================================
CREATE OR ALTER PROCEDURE sp_UpsertFullPackageLink
    @ID BIGINT = NULL,
    @VersionCode VARCHAR(32),
    @PackageName NVARCHAR(100),
    @FileSize BIGINT,
    @DownloadType VARCHAR(32),
    @DownloadUrl NVARCHAR(500),
    @VerificationCode NVARCHAR(10) = NULL,
    @ExtractionPassword NVARCHAR(50) = NULL,
    @MD5Hash VARCHAR(32) = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @Priority INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    IF @ID IS NOT NULL AND EXISTS (SELECT 1 FROM client_full_packages WHERE id = @ID)
    BEGIN
        -- 更新
        UPDATE client_full_packages
        SET
            version_code = @VersionCode,
            package_name = @PackageName,
            file_size = @FileSize,
            download_type = @DownloadType,
            download_url = @DownloadUrl,
            verification_code = @VerificationCode,
            extraction_password = @ExtractionPassword,
            md5_hash = @MD5Hash,
            description = @Description,
            priority = @Priority,
            updated_at = GETUTCDATE()
        WHERE id = @ID;

        SELECT @ID AS id;
    END
    ELSE
    BEGIN
        -- 新增
        INSERT INTO client_full_packages (
            version_code, package_name, file_size, download_type,
            download_url, verification_code, extraction_password,
            md5_hash, description, priority
        )
        VALUES (
            @VersionCode, @PackageName, @FileSize, @DownloadType,
            @DownloadUrl, @VerificationCode, @ExtractionPassword,
            @MD5Hash, @Description, @Priority
        );

        SELECT SCOPE_IDENTITY() AS id;
    END
END
GO

-- ===========================================
-- 存储过程: 删除完整客户端下载链接
-- ===========================================
CREATE OR ALTER PROCEDURE sp_DeleteFullPackageLink
    @ID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM client_full_packages WHERE id = @ID;

    SELECT @@ROWCOUNT AS affected_rows;
END
GO

-- ===========================================
-- 存储过程: 获取完整客户端下载链接列表
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GetFullPackageLinks
    @VersionCode VARCHAR(32) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.id,
        p.version_code,
        p.package_name,
        p.file_size,
        CASE
            WHEN p.file_size >= 1073741824 THEN CAST(p.file_size / 1073741824.0 AS DECIMAL(10,2)) + ' GB'
            WHEN p.file_size >= 1048576 THEN CAST(p.file_size / 1048576.0 AS DECIMAL(10,2)) + ' MB'
            ELSE CAST(p.file_size / 1024.0 AS DECIMAL(10,2)) + ' KB'
        END AS file_size_text,
        p.download_type,
        CASE p.download_type
            WHEN 'baidu' THEN N'百度网盘'
            WHEN 'aliyun' THEN N'阿里云盘'
            WHEN 'thunder' THEN N'迅雷云盘'
            WHEN '115' THEN N'115网盘'
            WHEN 'mega' THEN N'MEGA网盘'
            WHEN 'direct' THEN N'直链下载'
            ELSE N'其他'
        END AS type_name,
        p.download_url,
        p.verification_code,
        p.extraction_password,
        p.md5_hash,
        p.description,
        p.download_count,
        p.is_active,
        p.priority,
        CASE WHEN p.priority >= 90 THEN 1 ELSE 0 END AS is_recommended,
        p.created_at,
        p.updated_at
    FROM client_full_packages p
    WHERE (@VersionCode IS NULL OR p.version_code = @VersionCode)
    ORDER BY p.version_code DESC, p.priority DESC, p.created_at DESC;
END
GO

-- ===========================================
-- 存储过程: 记录下载次数
-- ===========================================
CREATE OR ALTER PROCEDURE sp_IncrementDownloadCount
    @ID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE client_full_packages
    SET download_count = download_count + 1,
        updated_at = GETUTCDATE()
    WHERE id = @ID;
END
GO

-- 插入示例CDN节点
INSERT INTO cdn_nodes (node_name, provider, region, endpoint, bucket_name, cdn_domain, is_enabled, priority)
VALUES
    (N'阿里云OSS-华东', 'oss', 'cn-hangzhou', 'oss-cn-hangzhou.aliyuncs.com', 'aion-updates', 'cdn.yourdomain.com', 1, 100),
    (N'腾讯云COS-华北', 'cos', 'ap-beijing', 'cos.ap-beijing.myqcloud.com', 'aion-updates', 'cdn2.yourdomain.com', 1, 90),
    (N'Cloudflare R2', 'r2', 'auto', 'r2.cloudflarestorage.com', 'aion-updates', 'r2.yourdomain.com', 1, 80);

-- 插入示例完整客户端下载链接
INSERT INTO client_full_packages (version_code, package_name, file_size, download_type, download_url, verification_code, extraction_password, priority, description)
VALUES
    ('2.7.0.15', N'Aion 2.7 完整客户端 (百度网盘)', 15728640000, 'baidu', 'https://pan.baidu.com/s/xxxxxx', 'abc123', 'aion2024', 100, N'推荐下载。解压后运行登录器即可。'),
    ('2.7.0.15', N'Aion 2.7 完整客户端 (阿里云盘)', 15728640000, 'aliyun', 'https://www.aliyundrive.com/s/xxxxxx', 'xyz789', 'aion2024', 95, N'下载速度快，推荐。'),
    ('2.7.0.15', N'Aion 2.7 完整客户端 (迅雷云盘)', 15728640000, 'thunder', 'https://pan.xunlei.com/s/xxxxxx', NULL, 'aion2024', 80, N'支持迅雷加速下载。');

PRINT '热更新系统初始化完成！';
GO

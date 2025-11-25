-- ===============================================
-- AionGate 升级脚本: 添加热更新系统
-- ===============================================
-- 用途: 为已有的AionGate系统添加热更新功能
-- 版本: 2.0
-- 日期: 2024-11-25
--
-- 使用方法:
-- 1. 备份数据库
-- 2. 在SSMS中打开此文件
-- 3. 执行脚本
-- 4. 重启AionGate服务
-- ===============================================

USE AionGameDB;
GO

PRINT '========================================';
PRINT '开始升级到热更新系统...';
PRINT '========================================';
PRINT '';

-- 检查表是否已存在
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'client_full_packages')
BEGIN
    PRINT '警告: 热更新系统表已存在，跳过创建。';
    PRINT '如需重新创建，请先手动删除相关表。';
    PRINT '';
END
ELSE
BEGIN
    PRINT '步骤 1/9: 创建完整客户端下载链接表...';

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

    PRINT '✓ 完整客户端下载链接表创建完成';
    PRINT '';

    PRINT '步骤 2/9: 创建版本管理表...';

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

    PRINT '✓ 版本管理表创建完成';
    PRINT '';

    PRINT '步骤 3/9: 创建版本文件清单表...';

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

    PRINT '✓ 版本文件清单表创建完成';
    PRINT '';

    PRINT '步骤 4/9: 创建文件差分表...';

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

    PRINT '✓ 文件差分表创建完成';
    PRINT '';

    PRINT '步骤 5/9: 创建客户端更新记录表...';

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

    PRINT '✓ 客户端更新记录表创建完成';
    PRINT '';

    PRINT '步骤 6/9: 创建CDN节点配置表...';

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

    PRINT '✓ CDN节点配置表创建完成';
    PRINT '';

    PRINT '步骤 7/9: 创建P2P节点表...';

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

    PRINT '✓ P2P节点表创建完成';
    PRINT '';

    PRINT '步骤 8/9: 创建更新统计表...';

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

    PRINT '✓ 更新统计表创建完成';
    PRINT '';
END
GO

-- ===========================================
-- 步骤 9/9: 创建存储过程
-- ===========================================
PRINT '步骤 9/9: 创建存储过程...';
PRINT '';

-- 检查更新
EXEC('
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

    -- 判断是否需要完整客户端
    IF @ClientVersion IS NULL OR @ClientVersion = '''' OR @ClientVersion = ''0.0.0.0''
        SET @NeedsFullClient = 1;

    -- 返回更新检查结果
    SELECT
        @NeedsUpdate AS needs_update,
        @NeedsFullClient AS needs_full_client,
        @ClientVersion AS current_version,
        @LatestVersionCode AS latest_version,
        v.version_type,
        CASE v.version_type WHEN 2 THEN ''incremental'' WHEN 3 THEN ''patch'' WHEN 4 THEN ''hotfix'' ELSE ''full'' END AS update_type,
        v.is_forced,
        v.file_count,
        v.download_size,
        CASE
            WHEN v.download_size >= 1073741824 THEN CAST(v.download_size / 1073741824.0 AS DECIMAL(10,2)) + '' GB''
            WHEN v.download_size >= 1048576 THEN CAST(v.download_size / 1048576.0 AS DECIMAL(10,2)) + '' MB''
            WHEN v.download_size >= 1024 THEN CAST(v.download_size / 1024.0 AS DECIMAL(10,2)) + '' KB''
            ELSE CAST(v.download_size AS VARCHAR) + '' B''
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
            download_type AS type,
            CASE download_type
                WHEN ''baidu'' THEN N''百度网盘''
                WHEN ''aliyun'' THEN N''阿里云盘''
                WHEN ''thunder'' THEN N''迅雷云盘''
                WHEN ''115'' THEN N''115网盘''
                WHEN ''mega'' THEN N''MEGA网盘''
                WHEN ''direct'' THEN N''直链下载''
                ELSE N''其他''
            END AS type_name,
            download_url AS url,
            verification_code,
            extraction_password,
            file_size,
            CASE
                WHEN file_size >= 1073741824 THEN CAST(file_size / 1073741824.0 AS DECIMAL(10,2)) + '' GB''
                WHEN file_size >= 1048576 THEN CAST(file_size / 1048576.0 AS DECIMAL(10,2)) + '' MB''
                ELSE CAST(file_size / 1024.0 AS DECIMAL(10,2)) + '' KB''
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
');

PRINT '✓ sp_CheckForUpdate 创建完成';

-- 其他存储过程
EXEC('
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
            WHEN p.file_size >= 1073741824 THEN CAST(p.file_size / 1073741824.0 AS DECIMAL(10,2)) + '' GB''
            WHEN p.file_size >= 1048576 THEN CAST(p.file_size / 1048576.0 AS DECIMAL(10,2)) + '' MB''
            ELSE CAST(p.file_size / 1024.0 AS DECIMAL(10,2)) + '' KB''
        END AS file_size_text,
        p.download_type,
        CASE p.download_type
            WHEN ''baidu'' THEN N''百度网盘''
            WHEN ''aliyun'' THEN N''阿里云盘''
            WHEN ''thunder'' THEN N''迅雷云盘''
            WHEN ''115'' THEN N''115网盘''
            WHEN ''mega'' THEN N''MEGA网盘''
            WHEN ''direct'' THEN N''直链下载''
            ELSE N''其他''
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
');

PRINT '✓ sp_GetFullPackageLinks 创建完成';

EXEC('
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
');

PRINT '✓ sp_IncrementDownloadCount 创建完成';
PRINT '';

-- ===========================================
-- 插入示例数据
-- ===========================================
PRINT '插入示例CDN节点...';

IF NOT EXISTS (SELECT 1 FROM cdn_nodes)
BEGIN
    INSERT INTO cdn_nodes (node_name, provider, region, endpoint, bucket_name, cdn_domain, is_enabled, priority)
    VALUES
        (N'阿里云OSS-华东', 'oss', 'cn-hangzhou', 'oss-cn-hangzhou.aliyuncs.com', 'aion-updates', 'cdn.yourdomain.com', 1, 100),
        (N'腾讯云COS-华北', 'cos', 'ap-beijing', 'cos.ap-beijing.myqcloud.com', 'aion-updates', 'cdn2.yourdomain.com', 1, 90),
        (N'Cloudflare R2', 'r2', 'auto', 'r2.cloudflarestorage.com', 'aion-updates', 'r2.yourdomain.com', 1, 80);

    PRINT '✓ 示例CDN节点已插入';
END
ELSE
BEGIN
    PRINT '⊗ CDN节点已存在，跳过插入';
END

PRINT '';
PRINT '========================================';
PRINT '升级完成！';
PRINT '';
PRINT '下一步操作:';
PRINT '1. 配置 appsettings.json 中的 CDN 设置';
PRINT '2. 使用管理界面添加完整客户端下载链接';
PRINT '3. 扫描游戏目录生成版本清单';
PRINT '4. 重启 AionGate 服务';
PRINT '';
PRINT '详细文档请查看:';
PRINT '- README.md';
PRINT '- docs/UPDATE_API.md';
PRINT '- docs/LAUNCHER_INTEGRATION.md';
PRINT '========================================';
GO

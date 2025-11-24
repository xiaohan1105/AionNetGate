-- ===========================================
-- AionGate 2.0 数据库初始化脚本
-- ===========================================
-- 支持: MSSQL Server 2019+
--
-- 注意: 此脚本会在游戏数据库中创建网关相关的表
-- 不会影响游戏原有的表结构

-- 创建账号表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[accounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[accounts] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [username] NVARCHAR(64) NOT NULL,
        [email] NVARCHAR(255) NULL,
        [password_hash] NVARCHAR(255) NOT NULL,
        [password_salt] NVARCHAR(64) NOT NULL,
        [status] TINYINT NOT NULL DEFAULT 1, -- 0=禁用, 1=正常, 2=锁定
        [role] TINYINT NOT NULL DEFAULT 0, -- 0=玩家, 1=GM, 2=管理员
        [login_attempts] INT NOT NULL DEFAULT 0,
        [locked_until] DATETIME2 NULL,
        [last_login_at] DATETIME2 NULL,
        [last_login_ip] NVARCHAR(45) NULL,
        [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_accounts] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [UK_accounts_username] UNIQUE ([username]),
        CONSTRAINT [UK_accounts_email] UNIQUE ([email])
    );

    CREATE NONCLUSTERED INDEX [IX_accounts_status] ON [dbo].[accounts] ([status]);
    CREATE NONCLUSTERED INDEX [IX_accounts_created_at] ON [dbo].[accounts] ([created_at]);
END
GO

-- 创建会话表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[sessions] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [account_id] BIGINT NOT NULL,
        [token_hash] NVARCHAR(64) NOT NULL,
        [refresh_token_hash] NVARCHAR(64) NULL,
        [client_ip] NVARCHAR(45) NOT NULL,
        [client_info] NVARCHAR(255) NULL,
        [expires_at] DATETIME2 NOT NULL,
        [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [revoked_at] DATETIME2 NULL,
        CONSTRAINT [PK_sessions] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_sessions_account] FOREIGN KEY ([account_id])
            REFERENCES [dbo].[accounts] ([id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_sessions_account_id] ON [dbo].[sessions] ([account_id]);
    CREATE NONCLUSTERED INDEX [IX_sessions_token_hash] ON [dbo].[sessions] ([token_hash]);
    CREATE NONCLUSTERED INDEX [IX_sessions_expires_at] ON [dbo].[sessions] ([expires_at]);
END
GO

-- 创建IP黑名单表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ip_blacklist]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ip_blacklist] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [ip_address] NVARCHAR(45) NOT NULL,
        [reason] NVARCHAR(255) NULL,
        [blocked_by] NVARCHAR(64) NOT NULL DEFAULT 'system',
        [blocked_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [expires_at] DATETIME2 NULL,
        [is_permanent] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ip_blacklist] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [UK_ip_blacklist_ip_address] UNIQUE ([ip_address])
    );

    CREATE NONCLUSTERED INDEX [IX_ip_blacklist_expires_at] ON [dbo].[ip_blacklist] ([expires_at]);
END
GO

-- 创建硬件指纹表 (防多开/封号)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[hardware_fingerprints]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[hardware_fingerprints] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [account_id] BIGINT NOT NULL,
        [fingerprint] NVARCHAR(64) NOT NULL,
        [mac_address] NVARCHAR(17) NULL,
        [cpu_id] NVARCHAR(64) NULL,
        [first_seen_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [last_seen_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [is_banned] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_hardware_fingerprints] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_fingerprints_account] FOREIGN KEY ([account_id])
            REFERENCES [dbo].[accounts] ([id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_fingerprints_account_id] ON [dbo].[hardware_fingerprints] ([account_id]);
    CREATE NONCLUSTERED INDEX [IX_fingerprints_fingerprint] ON [dbo].[hardware_fingerprints] ([fingerprint]);
END
GO

-- 创建外挂检测记录表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[cheat_detections]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[cheat_detections] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [account_id] BIGINT NULL,
        [session_id] BIGINT NULL,
        [client_ip] NVARCHAR(45) NOT NULL,
        [hardware_fingerprint] NVARCHAR(64) NULL,
        [detection_type] NVARCHAR(32) NOT NULL, -- process|md5|window_class
        [detected_name] NVARCHAR(255) NOT NULL,
        [action_taken] NVARCHAR(32) NOT NULL, -- warn|kick|ban
        [detected_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_cheat_detections] PRIMARY KEY CLUSTERED ([id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_cheat_detections_account_id] ON [dbo].[cheat_detections] ([account_id]);
    CREATE NONCLUSTERED INDEX [IX_cheat_detections_detected_at] ON [dbo].[cheat_detections] ([detected_at]);
    CREATE NONCLUSTERED INDEX [IX_cheat_detections_detection_type] ON [dbo].[cheat_detections] ([detection_type]);
END
GO

-- 创建审计日志表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[audit_logs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[audit_logs] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [account_id] BIGINT NULL,
        [action] NVARCHAR(64) NOT NULL,
        [resource_type] NVARCHAR(32) NULL,
        [resource_id] NVARCHAR(64) NULL,
        [old_value] NVARCHAR(MAX) NULL, -- JSON data
        [new_value] NVARCHAR(MAX) NULL, -- JSON data
        [client_ip] NVARCHAR(45) NULL,
        [user_agent] NVARCHAR(255) NULL,
        [created_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_audit_logs] PRIMARY KEY CLUSTERED ([id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_audit_logs_account_id] ON [dbo].[audit_logs] ([account_id]);
    CREATE NONCLUSTERED INDEX [IX_audit_logs_action] ON [dbo].[audit_logs] ([action]);
    CREATE NONCLUSTERED INDEX [IX_audit_logs_created_at] ON [dbo].[audit_logs] ([created_at]);
END
GO

-- 创建服务器配置表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[server_config]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[server_config] (
        [key] NVARCHAR(128) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        [description] NVARCHAR(255) NULL,
        [updated_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_server_config] PRIMARY KEY CLUSTERED ([key] ASC)
    );
END
GO

-- 插入默认配置
MERGE INTO [dbo].[server_config] AS target
USING (VALUES
    ('server.maintenance', 'false', N'维护模式开关'),
    ('server.max_online', '1000', N'最大在线人数'),
    ('server.announcement', '', N'服务器公告'),
    ('auth.registration_enabled', 'true', N'是否开放注册'),
    ('auth.email_verification', 'false', N'是否需要邮箱验证')
) AS source ([key], [value], [description])
ON target.[key] = source.[key]
WHEN MATCHED THEN
    UPDATE SET [updated_at] = GETDATE()
WHEN NOT MATCHED THEN
    INSERT ([key], [value], [description])
    VALUES (source.[key], source.[value], source.[description]);
GO

-- 创建性能指标表 (用于历史数据)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[metrics_history]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[metrics_history] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [metric_name] NVARCHAR(64) NOT NULL,
        [metric_value] FLOAT NOT NULL,
        [labels] NVARCHAR(MAX) NULL, -- JSON data
        [recorded_at] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_metrics_history] PRIMARY KEY CLUSTERED ([id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_metrics_history_name_time]
        ON [dbo].[metrics_history] ([metric_name], [recorded_at]);
END
GO

-- 创建更新 updated_at 字段的触发器
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[trg_accounts_updated_at]'))
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[trg_accounts_updated_at]
    ON [dbo].[accounts]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE [dbo].[accounts]
        SET [updated_at] = GETDATE()
        FROM [dbo].[accounts] a
        INNER JOIN inserted i ON a.[id] = i.[id];
    END
    ');
END
GO

-- 创建清理过期数据的存储过程
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_cleanup_expired_data]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[sp_cleanup_expired_data];
GO

CREATE PROCEDURE [dbo].[sp_cleanup_expired_data]
AS
BEGIN
    SET NOCOUNT ON;

    -- 清理过期会话 (7天前)
    DELETE FROM [dbo].[sessions]
    WHERE [expires_at] < DATEADD(DAY, -7, GETDATE());

    -- 清理过期IP封禁
    DELETE FROM [dbo].[ip_blacklist]
    WHERE [expires_at] IS NOT NULL
        AND [expires_at] < GETDATE()
        AND [is_permanent] = 0;

    -- 清理旧的指标数据 (30天前)
    DELETE FROM [dbo].[metrics_history]
    WHERE [recorded_at] < DATEADD(DAY, -30, GETDATE());

    -- 返回清理统计
    SELECT
        'Cleanup completed' AS [Status],
        GETDATE() AS [CompletedAt];
END
GO

-- 创建SQL Server Agent作业来定期清理（需要手动在SSMS中配置）
-- 或者可以在应用层使用定时任务调用 sp_cleanup_expired_data

-- 创建视图: 在线用户统计
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_online_stats]'))
    DROP VIEW [dbo].[v_online_stats];
GO

CREATE VIEW [dbo].[v_online_stats]
AS
SELECT
    COUNT(*) as total_sessions,
    COUNT(DISTINCT account_id) as unique_accounts,
    MIN(created_at) as oldest_session,
    MAX(created_at) as newest_session
FROM [dbo].[sessions]
WHERE [expires_at] > GETDATE() AND [revoked_at] IS NULL;
GO

-- 创建视图: 最近检测到的外挂
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_recent_cheats]'))
    DROP VIEW [dbo].[v_recent_cheats];
GO

CREATE VIEW [dbo].[v_recent_cheats]
AS
SELECT TOP 100
    cd.*,
    a.username
FROM [dbo].[cheat_detections] cd
LEFT JOIN [dbo].[accounts] a ON cd.account_id = a.id
WHERE cd.detected_at > DATEADD(HOUR, -24, GETDATE())
ORDER BY cd.detected_at DESC;
GO

-- 输出完成信息
PRINT 'AionGate database initialized successfully!';
GO

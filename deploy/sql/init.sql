-- ===========================================
-- AionGate 2.0 数据库初始化脚本
-- ===========================================
-- 支持: MySQL 8.0+

SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- 创建账号表
CREATE TABLE IF NOT EXISTS `accounts` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `username` VARCHAR(64) NOT NULL,
    `email` VARCHAR(255) DEFAULT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `password_salt` VARCHAR(64) NOT NULL,
    `status` TINYINT NOT NULL DEFAULT 1 COMMENT '0=禁用, 1=正常, 2=锁定',
    `role` TINYINT NOT NULL DEFAULT 0 COMMENT '0=玩家, 1=GM, 2=管理员',
    `login_attempts` INT NOT NULL DEFAULT 0,
    `locked_until` DATETIME DEFAULT NULL,
    `last_login_at` DATETIME DEFAULT NULL,
    `last_login_ip` VARCHAR(45) DEFAULT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_username` (`username`),
    UNIQUE KEY `uk_email` (`email`),
    KEY `idx_status` (`status`),
    KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建会话表
CREATE TABLE IF NOT EXISTS `sessions` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `account_id` BIGINT UNSIGNED NOT NULL,
    `token_hash` VARCHAR(64) NOT NULL,
    `refresh_token_hash` VARCHAR(64) DEFAULT NULL,
    `client_ip` VARCHAR(45) NOT NULL,
    `client_info` VARCHAR(255) DEFAULT NULL,
    `expires_at` DATETIME NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `revoked_at` DATETIME DEFAULT NULL,
    PRIMARY KEY (`id`),
    KEY `idx_account_id` (`account_id`),
    KEY `idx_token_hash` (`token_hash`),
    KEY `idx_expires_at` (`expires_at`),
    CONSTRAINT `fk_sessions_account` FOREIGN KEY (`account_id`)
        REFERENCES `accounts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建IP黑名单表
CREATE TABLE IF NOT EXISTS `ip_blacklist` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `ip_address` VARCHAR(45) NOT NULL,
    `reason` VARCHAR(255) DEFAULT NULL,
    `blocked_by` VARCHAR(64) DEFAULT 'system',
    `blocked_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `expires_at` DATETIME DEFAULT NULL,
    `is_permanent` BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_ip_address` (`ip_address`),
    KEY `idx_expires_at` (`expires_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建硬件指纹表 (防多开/封号)
CREATE TABLE IF NOT EXISTS `hardware_fingerprints` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `account_id` BIGINT UNSIGNED NOT NULL,
    `fingerprint` VARCHAR(64) NOT NULL,
    `mac_address` VARCHAR(17) DEFAULT NULL,
    `cpu_id` VARCHAR(64) DEFAULT NULL,
    `first_seen_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `last_seen_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `is_banned` BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (`id`),
    KEY `idx_account_id` (`account_id`),
    KEY `idx_fingerprint` (`fingerprint`),
    CONSTRAINT `fk_fingerprints_account` FOREIGN KEY (`account_id`)
        REFERENCES `accounts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建外挂检测记录表
CREATE TABLE IF NOT EXISTS `cheat_detections` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `account_id` BIGINT UNSIGNED DEFAULT NULL,
    `session_id` BIGINT UNSIGNED DEFAULT NULL,
    `client_ip` VARCHAR(45) NOT NULL,
    `hardware_fingerprint` VARCHAR(64) DEFAULT NULL,
    `detection_type` VARCHAR(32) NOT NULL COMMENT 'process|md5|window_class',
    `detected_name` VARCHAR(255) NOT NULL,
    `action_taken` VARCHAR(32) NOT NULL COMMENT 'warn|kick|ban',
    `detected_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_account_id` (`account_id`),
    KEY `idx_detected_at` (`detected_at`),
    KEY `idx_detection_type` (`detection_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建审计日志表
CREATE TABLE IF NOT EXISTS `audit_logs` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `account_id` BIGINT UNSIGNED DEFAULT NULL,
    `action` VARCHAR(64) NOT NULL,
    `resource_type` VARCHAR(32) DEFAULT NULL,
    `resource_id` VARCHAR(64) DEFAULT NULL,
    `old_value` JSON DEFAULT NULL,
    `new_value` JSON DEFAULT NULL,
    `client_ip` VARCHAR(45) DEFAULT NULL,
    `user_agent` VARCHAR(255) DEFAULT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_account_id` (`account_id`),
    KEY `idx_action` (`action`),
    KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建服务器配置表
CREATE TABLE IF NOT EXISTS `server_config` (
    `key` VARCHAR(128) NOT NULL,
    `value` TEXT NOT NULL,
    `description` VARCHAR(255) DEFAULT NULL,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 插入默认配置
INSERT INTO `server_config` (`key`, `value`, `description`) VALUES
('server.maintenance', 'false', '维护模式开关'),
('server.max_online', '1000', '最大在线人数'),
('server.announcement', '', '服务器公告'),
('auth.registration_enabled', 'true', '是否开放注册'),
('auth.email_verification', 'false', '是否需要邮箱验证')
ON DUPLICATE KEY UPDATE `updated_at` = CURRENT_TIMESTAMP;

-- 创建性能指标表 (用于历史数据)
CREATE TABLE IF NOT EXISTS `metrics_history` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `metric_name` VARCHAR(64) NOT NULL,
    `metric_value` DOUBLE NOT NULL,
    `labels` JSON DEFAULT NULL,
    `recorded_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    KEY `idx_metric_name_time` (`metric_name`, `recorded_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建清理过期数据的事件
DELIMITER //

CREATE EVENT IF NOT EXISTS `cleanup_expired_sessions`
ON SCHEDULE EVERY 1 HOUR
DO
BEGIN
    -- 清理过期会话
    DELETE FROM `sessions` WHERE `expires_at` < NOW() - INTERVAL 7 DAY;

    -- 清理过期IP封禁
    DELETE FROM `ip_blacklist` WHERE `expires_at` IS NOT NULL AND `expires_at` < NOW() AND `is_permanent` = FALSE;

    -- 清理旧的指标数据
    DELETE FROM `metrics_history` WHERE `recorded_at` < NOW() - INTERVAL 30 DAY;
END//

DELIMITER ;

-- 创建视图: 在线用户统计
CREATE OR REPLACE VIEW `v_online_stats` AS
SELECT
    COUNT(*) as total_sessions,
    COUNT(DISTINCT account_id) as unique_accounts,
    MIN(created_at) as oldest_session,
    MAX(created_at) as newest_session
FROM `sessions`
WHERE `expires_at` > NOW() AND `revoked_at` IS NULL;

-- 创建视图: 最近检测到的外挂
CREATE OR REPLACE VIEW `v_recent_cheats` AS
SELECT
    cd.*,
    a.username
FROM `cheat_detections` cd
LEFT JOIN `accounts` a ON cd.account_id = a.id
WHERE cd.detected_at > NOW() - INTERVAL 24 HOUR
ORDER BY cd.detected_at DESC
LIMIT 100;

-- 输出完成信息
SELECT 'AionGate database initialized successfully!' AS message;

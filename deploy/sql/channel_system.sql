-- ===========================================
-- AionGate 渠道分发和营销分成系统
-- ===========================================
-- 用于登录器渠道管理、推广追踪、分成结算

-- 渠道/代理商表
CREATE TABLE channels (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    channel_code VARCHAR(32) NOT NULL UNIQUE,  -- 渠道代码，嵌入登录器
    channel_name NVARCHAR(100) NOT NULL,
    contact_person NVARCHAR(50),
    contact_phone VARCHAR(20),
    contact_qq VARCHAR(20),
    contact_wechat VARCHAR(50),
    commission_rate DECIMAL(5,2) NOT NULL DEFAULT 0.00,  -- 分成比例 (0-100)
    settlement_type TINYINT NOT NULL DEFAULT 1,  -- 1=日结 2=周结 3=月结
    status TINYINT NOT NULL DEFAULT 1,  -- 0=禁用 1=正常 2=暂停
    total_users INT NOT NULL DEFAULT 0,  -- 累计注册用户
    active_users INT NOT NULL DEFAULT 0,  -- 活跃用户
    total_revenue DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- 累计充值
    total_commission DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- 累计分成
    unpaid_commission DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- 未结算分成
    remark NVARCHAR(500),
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_channels_code ON channels(channel_code);
CREATE INDEX idx_channels_status ON channels(status);

-- 用户渠道绑定表
CREATE TABLE user_channels (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    account_id BIGINT NOT NULL,
    channel_id BIGINT NOT NULL,
    channel_code VARCHAR(32) NOT NULL,
    register_ip VARCHAR(45),
    register_device_info NVARCHAR(200),
    first_login_at DATETIME,
    last_login_at DATETIME,
    login_count INT NOT NULL DEFAULT 0,
    total_recharge DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- 累计充值
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_user_channels_channel FOREIGN KEY (channel_id) REFERENCES channels(id)
);

CREATE UNIQUE INDEX idx_user_channels_account ON user_channels(account_id);
CREATE INDEX idx_user_channels_channel ON user_channels(channel_id);
CREATE INDEX idx_user_channels_created ON user_channels(created_at DESC);

-- 渠道充值记录表
CREATE TABLE channel_recharges (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    channel_id BIGINT NOT NULL,
    account_id BIGINT NOT NULL,
    order_no VARCHAR(64) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,  -- 充值金额
    commission_rate DECIMAL(5,2) NOT NULL,  -- 分成比例
    commission_amount DECIMAL(18,2) NOT NULL,  -- 分成金额
    status TINYINT NOT NULL DEFAULT 0,  -- 0=待结算 1=已结算
    settled_at DATETIME,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_channel_recharges_channel FOREIGN KEY (channel_id) REFERENCES channels(id)
);

CREATE INDEX idx_channel_recharges_channel ON channel_recharges(channel_id);
CREATE INDEX idx_channel_recharges_status ON channel_recharges(status);
CREATE INDEX idx_channel_recharges_created ON channel_recharges(created_at DESC);

-- 渠道结算记录表
CREATE TABLE channel_settlements (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    settlement_no VARCHAR(64) NOT NULL UNIQUE,
    channel_id BIGINT NOT NULL,
    settlement_period VARCHAR(20) NOT NULL,  -- 结算周期，如：2024-11, 2024-W47
    total_amount DECIMAL(18,2) NOT NULL,  -- 结算总金额
    recharge_count INT NOT NULL,  -- 充值笔数
    user_count INT NOT NULL,  -- 充值用户数
    status TINYINT NOT NULL DEFAULT 0,  -- 0=待审核 1=已审核 2=已支付
    payment_method NVARCHAR(50),  -- 支付方式：支付宝/微信/银行转账
    payment_account NVARCHAR(100),  -- 收款账号
    payment_voucher NVARCHAR(500),  -- 支付凭证URL
    audited_by NVARCHAR(50),
    audited_at DATETIME,
    paid_by NVARCHAR(50),
    paid_at DATETIME,
    remark NVARCHAR(500),
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_channel_settlements_channel FOREIGN KEY (channel_id) REFERENCES channels(id)
);

CREATE INDEX idx_channel_settlements_channel ON channel_settlements(channel_id);
CREATE INDEX idx_channel_settlements_status ON channel_settlements(status);
CREATE INDEX idx_channel_settlements_created ON channel_settlements(created_at DESC);

-- 渠道推广链接表
CREATE TABLE channel_links (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    channel_id BIGINT NOT NULL,
    link_code VARCHAR(32) NOT NULL UNIQUE,  -- 推广链接代码
    link_url NVARCHAR(500) NOT NULL,  -- 完整推广链接
    description NVARCHAR(200),
    click_count INT NOT NULL DEFAULT 0,  -- 点击次数
    download_count INT NOT NULL DEFAULT 0,  -- 下载次数
    register_count INT NOT NULL DEFAULT 0,  -- 注册次数
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_channel_links_channel FOREIGN KEY (channel_id) REFERENCES channels(id)
);

CREATE INDEX idx_channel_links_channel ON channel_links(channel_id);
CREATE INDEX idx_channel_links_code ON channel_links(link_code);

-- 渠道推广数据统计表 (每日)
CREATE TABLE channel_daily_stats (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    channel_id BIGINT NOT NULL,
    stat_date DATE NOT NULL,
    new_users INT NOT NULL DEFAULT 0,  -- 新增用户
    active_users INT NOT NULL DEFAULT 0,  -- 活跃用户
    recharge_users INT NOT NULL DEFAULT 0,  -- 充值用户
    recharge_count INT NOT NULL DEFAULT 0,  -- 充值次数
    recharge_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- 充值金额
    commission_amount DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- 分成金额
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_channel_daily_stats_channel FOREIGN KEY (channel_id) REFERENCES channels(id)
);

CREATE UNIQUE INDEX idx_channel_daily_stats_unique ON channel_daily_stats(channel_id, stat_date);
CREATE INDEX idx_channel_daily_stats_date ON channel_daily_stats(stat_date DESC);

GO

-- ===========================================
-- 存储过程: 用户注册时绑定渠道
-- ===========================================
CREATE OR ALTER PROCEDURE sp_BindUserToChannel
    @AccountID BIGINT,
    @ChannelCode VARCHAR(32),
    @RegisterIP VARCHAR(45),
    @DeviceInfo NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ChannelID BIGINT;

    -- 查找渠道
    SELECT @ChannelID = id FROM channels WHERE channel_code = @ChannelCode AND status = 1;

    IF @ChannelID IS NULL
    BEGIN
        RETURN -1; -- 渠道不存在或已禁用
    END

    -- 检查是否已绑定
    IF EXISTS (SELECT 1 FROM user_channels WHERE account_id = @AccountID)
    BEGIN
        RETURN -2; -- 用户已绑定其他渠道
    END

    BEGIN TRANSACTION;

    -- 绑定用户到渠道
    INSERT INTO user_channels (account_id, channel_id, channel_code, register_ip, register_device_info, created_at)
    VALUES (@AccountID, @ChannelID, @ChannelCode, @RegisterIP, @DeviceInfo, GETUTCDATE());

    -- 更新渠道统计
    UPDATE channels SET total_users = total_users + 1 WHERE id = @ChannelID;

    COMMIT TRANSACTION;

    RETURN 0;
END
GO

-- ===========================================
-- 存储过程: 用户充值时记录渠道分成
-- ===========================================
CREATE OR ALTER PROCEDURE sp_RecordChannelRecharge
    @AccountID BIGINT,
    @OrderNo VARCHAR(64),
    @Amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ChannelID BIGINT;
    DECLARE @CommissionRate DECIMAL(5,2);
    DECLARE @CommissionAmount DECIMAL(18,2);

    -- 查找用户绑定的渠道
    SELECT @ChannelID = channel_id FROM user_channels WHERE account_id = @AccountID;

    IF @ChannelID IS NULL
    BEGIN
        RETURN -1; -- 用户未绑定渠道
    END

    -- 获取渠道分成比例
    SELECT @CommissionRate = commission_rate FROM channels WHERE id = @ChannelID;

    -- 计算分成金额
    SET @CommissionAmount = @Amount * @CommissionRate / 100;

    BEGIN TRANSACTION;

    -- 记录充值分成
    INSERT INTO channel_recharges (channel_id, account_id, order_no, amount, commission_rate, commission_amount, status, created_at)
    VALUES (@ChannelID, @AccountID, @OrderNo, @Amount, @CommissionRate, @CommissionAmount, 0, GETUTCDATE());

    -- 更新渠道总收入和未结算分成
    UPDATE channels
    SET total_revenue = total_revenue + @Amount,
        total_commission = total_commission + @CommissionAmount,
        unpaid_commission = unpaid_commission + @CommissionAmount
    WHERE id = @ChannelID;

    -- 更新用户累计充值
    UPDATE user_channels
    SET total_recharge = total_recharge + @Amount
    WHERE account_id = @AccountID;

    COMMIT TRANSACTION;

    RETURN 0;
END
GO

-- ===========================================
-- 存储过程: 生成渠道结算单
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GenerateChannelSettlement
    @ChannelID BIGINT,
    @PeriodStart DATE,
    @PeriodEnd DATE,
    @SettlementPeriod VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SettlementNo VARCHAR(64);
    DECLARE @TotalAmount DECIMAL(18,2);
    DECLARE @RechargeCount INT;
    DECLARE @UserCount INT;

    -- 生成结算单号
    SET @SettlementNo = 'ST' + FORMAT(GETUTCDATE(), 'yyyyMMddHHmmss') + RIGHT('0000' + CAST(@ChannelID AS VARCHAR), 4);

    -- 统计该周期的充值数据
    SELECT
        @TotalAmount = ISNULL(SUM(commission_amount), 0),
        @RechargeCount = COUNT(*),
        @UserCount = COUNT(DISTINCT account_id)
    FROM channel_recharges
    WHERE channel_id = @ChannelID
        AND status = 0  -- 未结算
        AND created_at >= @PeriodStart
        AND created_at < DATEADD(DAY, 1, @PeriodEnd);

    IF @TotalAmount <= 0
    BEGIN
        RETURN -1; -- 无可结算金额
    END

    BEGIN TRANSACTION;

    -- 创建结算单
    INSERT INTO channel_settlements (settlement_no, channel_id, settlement_period, total_amount, recharge_count, user_count, status, created_at)
    VALUES (@SettlementNo, @ChannelID, @SettlementPeriod, @TotalAmount, @RechargeCount, @UserCount, 0, GETUTCDATE());

    -- 标记充值记录为已结算
    UPDATE channel_recharges
    SET status = 1, settled_at = GETUTCDATE()
    WHERE channel_id = @ChannelID
        AND status = 0
        AND created_at >= @PeriodStart
        AND created_at < DATEADD(DAY, 1, @PeriodEnd);

    -- 更新渠道未结算分成
    UPDATE channels
    SET unpaid_commission = unpaid_commission - @TotalAmount
    WHERE id = @ChannelID;

    COMMIT TRANSACTION;

    RETURN SCOPE_IDENTITY();
END
GO

-- ===========================================
-- 存储过程: 获取渠道统计数据
-- ===========================================
CREATE OR ALTER PROCEDURE sp_GetChannelStatistics
    @ChannelID BIGINT = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 如果没有指定日期，默认查询最近30天
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(DAY, -30, CAST(GETUTCDATE() AS DATE));

    IF @EndDate IS NULL
        SET @EndDate = CAST(GETUTCDATE() AS DATE);

    -- 如果指定了渠道，只查询该渠道
    IF @ChannelID IS NOT NULL
    BEGIN
        SELECT
            c.id,
            c.channel_code,
            c.channel_name,
            c.commission_rate,
            c.total_users,
            c.active_users,
            c.total_revenue,
            c.total_commission,
            c.unpaid_commission,
            COUNT(DISTINCT uc.account_id) AS period_active_users,
            ISNULL(SUM(cr.amount), 0) AS period_revenue,
            ISNULL(SUM(cr.commission_amount), 0) AS period_commission
        FROM channels c
        LEFT JOIN user_channels uc ON c.id = uc.channel_id
            AND uc.last_login_at >= @StartDate AND uc.last_login_at <= DATEADD(DAY, 1, @EndDate)
        LEFT JOIN channel_recharges cr ON c.id = cr.channel_id
            AND cr.created_at >= @StartDate AND cr.created_at < DATEADD(DAY, 1, @EndDate)
        WHERE c.id = @ChannelID
        GROUP BY c.id, c.channel_code, c.channel_name, c.commission_rate, c.total_users,
                 c.active_users, c.total_revenue, c.total_commission, c.unpaid_commission;
    END
    ELSE
    BEGIN
        -- 查询所有渠道
        SELECT
            c.id,
            c.channel_code,
            c.channel_name,
            c.commission_rate,
            c.total_users,
            c.active_users,
            c.total_revenue,
            c.total_commission,
            c.unpaid_commission,
            COUNT(DISTINCT uc.account_id) AS period_active_users,
            ISNULL(SUM(cr.amount), 0) AS period_revenue,
            ISNULL(SUM(cr.commission_amount), 0) AS period_commission
        FROM channels c
        LEFT JOIN user_channels uc ON c.id = uc.channel_id
            AND uc.last_login_at >= @StartDate AND uc.last_login_at <= DATEADD(DAY, 1, @EndDate)
        LEFT JOIN channel_recharges cr ON c.id = cr.channel_id
            AND cr.created_at >= @StartDate AND cr.created_at < DATEADD(DAY, 1, @EndDate)
        WHERE c.status = 1
        GROUP BY c.id, c.channel_code, c.channel_name, c.commission_rate, c.total_users,
                 c.active_users, c.total_revenue, c.total_commission, c.unpaid_commission
        ORDER BY c.total_revenue DESC;
    END
END
GO

-- ===========================================
-- 存储过程: 更新每日渠道统计 (定时任务调用)
-- ===========================================
CREATE OR ALTER PROCEDURE sp_UpdateChannelDailyStats
    @StatDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @StatDate IS NULL
        SET @StatDate = CAST(DATEADD(DAY, -1, GETUTCDATE()) AS DATE);  -- 昨天

    -- 删除已有的统计数据
    DELETE FROM channel_daily_stats WHERE stat_date = @StatDate;

    -- 生成每日统计
    INSERT INTO channel_daily_stats (channel_id, stat_date, new_users, active_users, recharge_users, recharge_count, recharge_amount, commission_amount)
    SELECT
        c.id AS channel_id,
        @StatDate AS stat_date,
        COUNT(DISTINCT CASE WHEN CAST(uc.created_at AS DATE) = @StatDate THEN uc.account_id END) AS new_users,
        COUNT(DISTINCT CASE WHEN CAST(uc.last_login_at AS DATE) = @StatDate THEN uc.account_id END) AS active_users,
        COUNT(DISTINCT cr.account_id) AS recharge_users,
        COUNT(cr.id) AS recharge_count,
        ISNULL(SUM(cr.amount), 0) AS recharge_amount,
        ISNULL(SUM(cr.commission_amount), 0) AS commission_amount
    FROM channels c
    LEFT JOIN user_channels uc ON c.id = uc.channel_id
    LEFT JOIN channel_recharges cr ON c.id = cr.channel_id AND CAST(cr.created_at AS DATE) = @StatDate
    WHERE c.status = 1
    GROUP BY c.id;
END
GO

-- 插入示例渠道
INSERT INTO channels (channel_code, channel_name, contact_person, contact_qq, commission_rate, settlement_type, status)
VALUES
    ('OFFICIAL', N'官方渠道', N'管理员', '10000', 0.00, 3, 1),
    ('PARTNER01', N'合作伙伴A', N'张三', '123456', 15.00, 2, 1),
    ('PARTNER02', N'合作伙伴B', N'李四', '234567', 20.00, 3, 1),
    ('AGENT01', N'代理商甲', N'王五', '345678', 25.00, 2, 1);

PRINT '渠道分发系统初始化完成！';
GO

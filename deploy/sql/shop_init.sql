-- ===========================================
-- AionGate 商城系统数据库初始化
-- ===========================================

-- 商城商品表
CREATE TABLE shop_items (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    image_url NVARCHAR(500),
    item_type TINYINT NOT NULL,  -- 1=装备 2=消耗品 3=材料 4=时装 5=坐骑 6=宠物 7=礼包 8=VIP
    game_item_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    price INT NOT NULL,
    original_price INT,
    is_active BIT NOT NULL DEFAULT 1,
    is_hot BIT NOT NULL DEFAULT 0,
    is_new BIT NOT NULL DEFAULT 0,
    sort_order INT NOT NULL DEFAULT 0,
    stock INT NOT NULL DEFAULT -1,  -- -1表示无限
    sold_count INT NOT NULL DEFAULT 0,
    limit_per_user INT NOT NULL DEFAULT 0,  -- 0表示不限购
    start_time DATETIME,
    end_time DATETIME,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_shop_items_type ON shop_items(item_type);
CREATE INDEX idx_shop_items_active ON shop_items(is_active);
CREATE INDEX idx_shop_items_sort ON shop_items(sort_order DESC);

-- 商城订单表
CREATE TABLE shop_orders (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    order_no VARCHAR(64) NOT NULL UNIQUE,
    account_id BIGINT NOT NULL,
    character_id INT,
    character_name NVARCHAR(100),
    item_id BIGINT NOT NULL,
    item_name NVARCHAR(100) NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    unit_price INT NOT NULL,
    total_price INT NOT NULL,
    status TINYINT NOT NULL DEFAULT 0,  -- 0=待支付 1=已支付 2=已发货 3=已完成 4=已取消 5=已退款
    payment_method TINYINT NOT NULL,  -- 1=点券 2=支付宝 3=微信 99=管理员赠送
    paid_at DATETIME,
    delivered_at DATETIME,
    completed_at DATETIME,
    client_ip VARCHAR(45),
    remark NVARCHAR(500),
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_shop_orders_account ON shop_orders(account_id);
CREATE INDEX idx_shop_orders_status ON shop_orders(status);
CREATE INDEX idx_shop_orders_created ON shop_orders(created_at DESC);

-- 公告表
CREATE TABLE announcements (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(200) NOT NULL,
    content NVARCHAR(MAX) NOT NULL,
    type TINYINT NOT NULL DEFAULT 1,  -- 1=系统 2=活动 3=维护 4=更新 5=新闻
    is_pinned BIT NOT NULL DEFAULT 0,
    is_published BIT NOT NULL DEFAULT 0,
    sort_order INT NOT NULL DEFAULT 0,
    view_count INT NOT NULL DEFAULT 0,
    published_at DATETIME,
    created_at DATETIME NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX idx_announcements_published ON announcements(is_published);
CREATE INDEX idx_announcements_pinned ON announcements(is_pinned DESC);
CREATE INDEX idx_announcements_date ON announcements(published_at DESC);

-- 插入示例商品
INSERT INTO shop_items (name, description, item_type, game_item_id, quantity, price, is_hot, is_new, sort_order)
VALUES
    (N'新手礼包', N'包含新手必备道具，助你快速成长', 7, 188900001, 1, 100, 1, 1, 100),
    (N'强化石礼包(大)', N'100个强化石，装备强化必备', 7, 166000195, 100, 500, 1, 0, 90),
    (N'高级时装礼包', N'永久时装，提升属性', 4, 125000001, 1, 2000, 1, 1, 80),
    (N'飞行坐骑', N'提升飞行速度50%', 5, 190100001, 1, 1500, 0, 1, 70),
    (N'复活石*10', N'原地满血复活', 2, 162000002, 10, 50, 0, 0, 60),
    (N'经验卷轴*5', N'经验获取+100%，持续1小时', 2, 169610000, 5, 80, 0, 0, 50);

-- 插入示例公告
INSERT INTO announcements (title, content, type, is_pinned, is_published, published_at)
VALUES
    (N'欢迎来到永恒之塔', N'<p>欢迎来到永恒之塔私服！</p><p>本服特色：</p><ul><li>经验倍率 10 倍</li><li>掉率倍率 5 倍</li><li>每日签到送好礼</li><li>GM 在线服务</li></ul>', 1, 1, 1, GETUTCDATE()),
    (N'商城上新啦！', N'<p>商城全新上架时装、坐骑等道具，欢迎选购！</p>', 2, 0, 1, GETUTCDATE()),
    (N'定期维护公告', N'<p>服务器将于每周三凌晨 2:00-4:00 进行例行维护，请提前下线。</p>', 3, 0, 1, GETUTCDATE());

GO

-- ===========================================
-- 存储过程: 发送邮件道具
-- ===========================================
CREATE OR ALTER PROCEDURE sp_SendMailWithItem
    @ReceiverID INT,
    @ItemID INT,
    @ItemCount BIGINT,
    @MailTitle NVARCHAR(100),
    @MailContent NVARCHAR(500),
    @SenderName NVARCHAR(50) = N'系统'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MailId INT;

    -- 插入邮件
    INSERT INTO aion.mail (sender_name, sender_id, receiver_id, title, message, unread, attached_item_id, attached_kinah_count, express, recieved_time)
    VALUES (@SenderName, 0, @ReceiverID, @MailTitle, @MailContent, 1, @ItemID, @ItemCount, 0, GETUTCDATE());

    SET @MailId = SCOPE_IDENTITY();

    -- 如果玩家在线，通知客户端
    -- 这里需要游戏服务器自己实现推送逻辑

    RETURN @MailId;
END
GO

-- ===========================================
-- 存储过程: 添加物品到背包
-- ===========================================
CREATE OR ALTER PROCEDURE sp_AddItemToInventory
    @PlayerID INT,
    @ItemID INT,
    @ItemCount BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- 查找背包空位
    DECLARE @SlotId INT;

    SELECT TOP 1 @SlotId = item_unique_id + 1
    FROM aion.inventory
    WHERE item_owner = @PlayerID
    ORDER BY item_unique_id DESC;

    IF @SlotId IS NULL
        SET @SlotId = 1;

    -- 插入物品
    INSERT INTO aion.inventory (item_unique_id, item_id, item_count, item_owner, is_equiped, slot)
    VALUES (@SlotId, @ItemID, @ItemCount, @PlayerID, 0, 126);

    RETURN @SlotId;
END
GO

-- ===========================================
-- 存储过程: 添加点券
-- ===========================================
CREATE OR ALTER PROCEDURE sp_AddAccountPoints
    @AccountID BIGINT,
    @Points INT,
    @Reason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aion.account_data
    SET toll = toll + @Points
    WHERE id = @AccountID;

    -- 记录日志
    INSERT INTO aion.account_points_log (account_id, points, balance_after, reason, created_at)
    VALUES (@AccountID, @Points, (SELECT toll FROM aion.account_data WHERE id = @AccountID), @Reason, GETUTCDATE());

    RETURN @@ROWCOUNT;
END
GO

-- ===========================================
-- 存储过程: 扣除点券
-- ===========================================
CREATE OR ALTER PROCEDURE sp_DeductAccountPoints
    @AccountID BIGINT,
    @Points INT,
    @Reason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentBalance INT;
    SELECT @CurrentBalance = toll FROM aion.account_data WHERE id = @AccountID;

    IF @CurrentBalance < @Points
    BEGIN
        RETURN -1; -- 余额不足
    END

    UPDATE aion.account_data
    SET toll = toll - @Points
    WHERE id = @AccountID;

    -- 记录日志
    INSERT INTO aion.account_points_log (account_id, points, balance_after, reason, created_at)
    VALUES (@AccountID, -@Points, (SELECT toll FROM aion.account_data WHERE id = @AccountID), @Reason, GETUTCDATE());

    RETURN @@ROWCOUNT;
END
GO

-- ===========================================
-- 点券日志表 (如果不存在)
-- ===========================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'account_points_log')
BEGIN
    CREATE TABLE aion.account_points_log (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        account_id BIGINT NOT NULL,
        points INT NOT NULL,  -- 正数=增加，负数=扣除
        balance_after INT NOT NULL,
        reason NVARCHAR(200),
        created_at DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX idx_points_log_account ON aion.account_points_log(account_id);
    CREATE INDEX idx_points_log_date ON aion.account_points_log(created_at DESC);
END
GO

PRINT '商城系统初始化完成！';

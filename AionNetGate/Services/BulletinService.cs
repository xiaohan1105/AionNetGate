using AionCommons.LogEngine;
using AionNetGate.Configs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AionNetGate.Services
{
    /// <summary>
    /// 留言板服务 - 提供公告和留言的数据库操作
    /// 支持 MySQL 和 MSSQL 双数据库
    /// </summary>
    class BulletinService
    {
        internal static BulletinService Instance = new BulletinService();

        private static readonly Logger Logger = LoggerFactory.getLogger();

        // 发言频率限制：每个账号每分钟最多发1条
        private Dictionary<string, DateTime> lastPostTime = new Dictionary<string, DateTime>();
        private object lockObj = new object();

        #region SQL语句定义

        // MySQL SQL语句
        private static readonly string MYSQL_CREATE_BULLETIN_TABLE = @"
            CREATE TABLE IF NOT EXISTS `bulletin_messages` (
                `id` INT PRIMARY KEY AUTO_INCREMENT,
                `account_name` VARCHAR(50) NOT NULL,
                `title` VARCHAR(100) NOT NULL,
                `content` TEXT NOT NULL,
                `type` TINYINT DEFAULT 0 COMMENT '0=普通留言, 1=投诉, 2=建议, 3=BUG反馈',
                `status` TINYINT DEFAULT 0 COMMENT '0=未处理, 1=已查看, 2=已回复, 3=已关闭',
                `priority` TINYINT DEFAULT 0 COMMENT '0=普通, 1=重要, 2=紧急',
                `is_public` TINYINT DEFAULT 1 COMMENT '是否公开',
                `admin_reply` TEXT,
                `reply_at` DATETIME,
                `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
                `updated_at` DATETIME,
                INDEX `idx_account` (`account_name`),
                INDEX `idx_status` (`status`),
                INDEX `idx_created` (`created_at`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='玩家留言表';";

        private static readonly string MYSQL_CREATE_ANNOUNCEMENT_TABLE = @"
            CREATE TABLE IF NOT EXISTS `system_announcements` (
                `id` INT PRIMARY KEY AUTO_INCREMENT,
                `title` VARCHAR(100) NOT NULL,
                `content` TEXT NOT NULL,
                `type` TINYINT DEFAULT 0 COMMENT '0=普通公告, 1=维护通知, 2=活动公告, 3=紧急通知',
                `priority` TINYINT DEFAULT 0 COMMENT '0=普通, 1=重要, 2=置顶',
                `is_active` TINYINT DEFAULT 1,
                `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
                `expires_at` DATETIME,
                `view_count` INT DEFAULT 0,
                INDEX `idx_active` (`is_active`),
                INDEX `idx_priority` (`priority`),
                INDEX `idx_created` (`created_at`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='系统公告表';";

        // MSSQL SQL语句
        private static readonly string MSSQL_CREATE_BULLETIN_TABLE = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='bulletin_messages' AND xtype='U')
            CREATE TABLE [dbo].[bulletin_messages] (
                [id] INT PRIMARY KEY IDENTITY(1,1),
                [account_name] VARCHAR(50) NOT NULL,
                [title] NVARCHAR(100) NOT NULL,
                [content] NVARCHAR(MAX) NOT NULL,
                [type] TINYINT DEFAULT 0,
                [status] TINYINT DEFAULT 0,
                [priority] TINYINT DEFAULT 0,
                [is_public] TINYINT DEFAULT 1,
                [admin_reply] NVARCHAR(MAX),
                [reply_at] DATETIME,
                [created_at] DATETIME DEFAULT GETDATE(),
                [updated_at] DATETIME
            );
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_bulletin_account' AND object_id = OBJECT_ID('bulletin_messages'))
                CREATE INDEX idx_bulletin_account ON bulletin_messages(account_name);
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_bulletin_status' AND object_id = OBJECT_ID('bulletin_messages'))
                CREATE INDEX idx_bulletin_status ON bulletin_messages(status);
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_bulletin_created' AND object_id = OBJECT_ID('bulletin_messages'))
                CREATE INDEX idx_bulletin_created ON bulletin_messages(created_at);";

        private static readonly string MSSQL_CREATE_ANNOUNCEMENT_TABLE = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='system_announcements' AND xtype='U')
            CREATE TABLE [dbo].[system_announcements] (
                [id] INT PRIMARY KEY IDENTITY(1,1),
                [title] NVARCHAR(100) NOT NULL,
                [content] NVARCHAR(MAX) NOT NULL,
                [type] TINYINT DEFAULT 0,
                [priority] TINYINT DEFAULT 0,
                [is_active] TINYINT DEFAULT 1,
                [created_at] DATETIME DEFAULT GETDATE(),
                [expires_at] DATETIME,
                [view_count] INT DEFAULT 0
            );
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_announcement_active' AND object_id = OBJECT_ID('system_announcements'))
                CREATE INDEX idx_announcement_active ON system_announcements(is_active);
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_announcement_priority' AND object_id = OBJECT_ID('system_announcements'))
                CREATE INDEX idx_announcement_priority ON system_announcements(priority);";

        #endregion

        #region 初始化

        internal BulletinService()
        {
            // 延迟初始化，等待配置加载完成
        }

        /// <summary>
        /// 初始化数据库表
        /// </summary>
        internal void InitializeTables()
        {
            try
            {
                if (Config.isMysql)
                {
                    InitializeMySQLTables();
                }
                else
                {
                    InitializeMSSQLTables();
                }
                Logger.info("留言板数据库表初始化完成");
            }
            catch (Exception ex)
            {
                Logger.error("留言板数据库表初始化失败: {0}", ex.Message);
            }
        }

        private void InitializeMySQLTables()
        {
            using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(MYSQL_CREATE_BULLETIN_TABLE, con))
                {
                    cmd.ExecuteNonQuery();
                }
                using (MySqlCommand cmd = new MySqlCommand(MYSQL_CREATE_ANNOUNCEMENT_TABLE, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InitializeMSSQLTables()
        {
            using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(MSSQL_CREATE_BULLETIN_TABLE, con))
                {
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand(MSSQL_CREATE_ANNOUNCEMENT_TABLE, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region 公告管理

        /// <summary>
        /// 获取有效的系统公告列表
        /// </summary>
        internal List<BulletinItem> GetActiveAnnouncements(int pageIndex, int pageSize)
        {
            return Config.isMysql ?
                GetActiveAnnouncementsMySQL(pageIndex, pageSize) :
                GetActiveAnnouncementsMSSQL(pageIndex, pageSize);
        }

        private List<BulletinItem> GetActiveAnnouncementsMySQL(int pageIndex, int pageSize)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, '', type, 0 as status, priority, created_at, 0 as has_reply
                          FROM system_announcements
                          WHERE is_active = 1 AND (expires_at IS NULL OR expires_at > NOW())
                          ORDER BY priority DESC, created_at DESC
                          LIMIT @offset, @limit";

            using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                AccountName = "系统公告",
                                Type = reader.GetByte("type"),
                                Status = 0,
                                Priority = reader.GetByte("priority"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                HasReply = false
                            });
                        }
                    }
                }
            }
            return list;
        }

        private List<BulletinItem> GetActiveAnnouncementsMSSQL(int pageIndex, int pageSize)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, type, priority, created_at
                          FROM system_announcements
                          WHERE is_active = 1 AND (expires_at IS NULL OR expires_at > GETDATE())
                          ORDER BY priority DESC, created_at DESC
                          OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                AccountName = "系统公告",
                                Type = reader.GetByte(2),
                                Status = 0,
                                Priority = reader.GetByte(3),
                                CreatedAt = reader.GetDateTime(4),
                                HasReply = false
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取公告总数
        /// </summary>
        internal int GetAnnouncementCount()
        {
            string sql = Config.isMysql ?
                "SELECT COUNT(*) FROM system_announcements WHERE is_active = 1 AND (expires_at IS NULL OR expires_at > NOW())" :
                "SELECT COUNT(*) FROM system_announcements WHERE is_active = 1 AND (expires_at IS NULL OR expires_at > GETDATE())";

            return ExecuteScalar(sql);
        }

        /// <summary>
        /// 添加系统公告（管理端使用）
        /// </summary>
        internal bool AddAnnouncement(string title, string content, byte type, byte priority, DateTime? expiresAt, out string message)
        {
            message = "公告发布成功";

            try
            {
                if (Config.isMysql)
                {
                    string sql = "INSERT INTO system_announcements (title, content, type, priority, expires_at) VALUES (@title, @content, @type, @priority, @expires_at)";
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@title", title);
                            cmd.Parameters.AddWithValue("@content", content);
                            cmd.Parameters.AddWithValue("@type", type);
                            cmd.Parameters.AddWithValue("@priority", priority);
                            cmd.Parameters.AddWithValue("@expires_at", expiresAt.HasValue ? (object)expiresAt.Value : DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    string sql = "INSERT INTO system_announcements (title, content, type, priority, expires_at) VALUES (@title, @content, @type, @priority, @expires_at)";
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@title", title);
                            cmd.Parameters.AddWithValue("@content", content);
                            cmd.Parameters.AddWithValue("@type", type);
                            cmd.Parameters.AddWithValue("@priority", priority);
                            cmd.Parameters.AddWithValue("@expires_at", expiresAt.HasValue ? (object)expiresAt.Value : DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Logger.info("发布了新公告: {0}", title);
                return true;
            }
            catch (Exception ex)
            {
                message = "公告发布失败: " + ex.Message;
                Logger.error(message);
                return false;
            }
        }

        #endregion

        #region 留言管理

        /// <summary>
        /// 检查是否可以发布留言（频率限制）
        /// </summary>
        internal bool CanPostMessage(string accountName)
        {
            lock (lockObj)
            {
                if (lastPostTime.ContainsKey(accountName))
                {
                    TimeSpan elapsed = DateTime.Now - lastPostTime[accountName];
                    if (elapsed.TotalSeconds < 60)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 发布新留言
        /// </summary>
        internal bool PostMessage(string accountName, string title, string content, byte type, out string message)
        {
            message = "留言发布成功，感谢您的反馈！";

            try
            {
                if (Config.isMysql)
                {
                    string sql = "INSERT INTO bulletin_messages (account_name, title, content, type) VALUES (@account, @title, @content, @type)";
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@account", accountName);
                            cmd.Parameters.AddWithValue("@title", title);
                            cmd.Parameters.AddWithValue("@content", content);
                            cmd.Parameters.AddWithValue("@type", type);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    string sql = "INSERT INTO bulletin_messages (account_name, title, content, type) VALUES (@account, @title, @content, @type)";
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@account", accountName);
                            cmd.Parameters.AddWithValue("@title", title);
                            cmd.Parameters.AddWithValue("@content", content);
                            cmd.Parameters.AddWithValue("@type", type);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 记录发言时间
                lock (lockObj)
                {
                    lastPostTime[accountName] = DateTime.Now;
                }

                Logger.info("玩家 [{0}] 发布了留言: {1}", accountName, title);
                return true;
            }
            catch (Exception ex)
            {
                message = "留言发布失败: " + ex.Message;
                Logger.error(message);
                return false;
            }
        }

        /// <summary>
        /// 获取公开留言列表
        /// </summary>
        internal List<BulletinItem> GetPublicMessages(int pageIndex, int pageSize)
        {
            return Config.isMysql ?
                GetPublicMessagesMySQL(pageIndex, pageSize) :
                GetPublicMessagesMSSQL(pageIndex, pageSize);
        }

        private List<BulletinItem> GetPublicMessagesMySQL(int pageIndex, int pageSize)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, account_name, type, status, priority, created_at,
                          CASE WHEN admin_reply IS NOT NULL AND admin_reply != '' THEN 1 ELSE 0 END as has_reply
                          FROM bulletin_messages
                          WHERE is_public = 1
                          ORDER BY created_at DESC
                          LIMIT @offset, @limit";

            using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                AccountName = MaskAccountName(reader.GetString("account_name")),
                                Type = reader.GetByte("type"),
                                Status = reader.GetByte("status"),
                                Priority = reader.GetByte("priority"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                HasReply = reader.GetInt32("has_reply") == 1
                            });
                        }
                    }
                }
            }
            return list;
        }

        private List<BulletinItem> GetPublicMessagesMSSQL(int pageIndex, int pageSize)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, account_name, type, status, priority, created_at,
                          CASE WHEN admin_reply IS NOT NULL AND admin_reply != '' THEN 1 ELSE 0 END as has_reply
                          FROM bulletin_messages
                          WHERE is_public = 1
                          ORDER BY created_at DESC
                          OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                AccountName = MaskAccountName(reader.GetString(2)),
                                Type = reader.GetByte(3),
                                Status = reader.GetByte(4),
                                Priority = reader.GetByte(5),
                                CreatedAt = reader.GetDateTime(6),
                                HasReply = reader.GetInt32(7) == 1
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取公开留言总数
        /// </summary>
        internal int GetPublicMessageCount()
        {
            string sql = "SELECT COUNT(*) FROM bulletin_messages WHERE is_public = 1";
            return ExecuteScalar(sql);
        }

        /// <summary>
        /// 获取我的留言列表
        /// </summary>
        internal List<BulletinItem> GetMyMessages(string accountName, int pageIndex, int pageSize)
        {
            return Config.isMysql ?
                GetMyMessagesMySQL(accountName, pageIndex, pageSize) :
                GetMyMessagesMSSQL(accountName, pageIndex, pageSize);
        }

        private List<BulletinItem> GetMyMessagesMySQL(string accountName, int pageIndex, int pageSize)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, account_name, type, status, priority, created_at,
                          CASE WHEN admin_reply IS NOT NULL AND admin_reply != '' THEN 1 ELSE 0 END as has_reply
                          FROM bulletin_messages
                          WHERE account_name = @account
                          ORDER BY created_at DESC
                          LIMIT @offset, @limit";

            using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@account", accountName);
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                AccountName = reader.GetString("account_name"),
                                Type = reader.GetByte("type"),
                                Status = reader.GetByte("status"),
                                Priority = reader.GetByte("priority"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                HasReply = reader.GetInt32("has_reply") == 1
                            });
                        }
                    }
                }
            }
            return list;
        }

        private List<BulletinItem> GetMyMessagesMSSQL(string accountName, int pageIndex, int pageSize)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, account_name, type, status, priority, created_at,
                          CASE WHEN admin_reply IS NOT NULL AND admin_reply != '' THEN 1 ELSE 0 END as has_reply
                          FROM bulletin_messages
                          WHERE account_name = @account
                          ORDER BY created_at DESC
                          OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@account", accountName);
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                AccountName = reader.GetString(2),
                                Type = reader.GetByte(3),
                                Status = reader.GetByte(4),
                                Priority = reader.GetByte(5),
                                CreatedAt = reader.GetDateTime(6),
                                HasReply = reader.GetInt32(7) == 1
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取我的留言总数
        /// </summary>
        internal int GetMyMessageCount(string accountName)
        {
            if (Config.isMysql)
            {
                string sql = "SELECT COUNT(*) FROM bulletin_messages WHERE account_name = @account";
                using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@account", accountName);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            else
            {
                string sql = "SELECT COUNT(*) FROM bulletin_messages WHERE account_name = @account";
                using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@account", accountName);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
        }

        /// <summary>
        /// 获取留言详情
        /// </summary>
        internal BulletinDetail GetMessageDetail(int messageId)
        {
            return Config.isMysql ?
                GetMessageDetailMySQL(messageId) :
                GetMessageDetailMSSQL(messageId);
        }

        private BulletinDetail GetMessageDetailMySQL(int messageId)
        {
            string sql = @"SELECT id, title, content, account_name, type, status, priority,
                          created_at, admin_reply, reply_at
                          FROM bulletin_messages
                          WHERE id = @id";

            using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", messageId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new BulletinDetail
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Content = reader.GetString("content"),
                                AccountName = MaskAccountName(reader.GetString("account_name")),
                                Type = reader.GetByte("type"),
                                Status = reader.GetByte("status"),
                                Priority = reader.GetByte("priority"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                AdminReply = reader.IsDBNull(reader.GetOrdinal("admin_reply")) ? null : reader.GetString("admin_reply"),
                                ReplyAt = reader.IsDBNull(reader.GetOrdinal("reply_at")) ? (DateTime?)null : reader.GetDateTime("reply_at")
                            };
                        }
                    }
                }
            }
            return null;
        }

        private BulletinDetail GetMessageDetailMSSQL(int messageId)
        {
            string sql = @"SELECT id, title, content, account_name, type, status, priority,
                          created_at, admin_reply, reply_at
                          FROM bulletin_messages
                          WHERE id = @id";

            using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", messageId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new BulletinDetail
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Content = reader.GetString(2),
                                AccountName = MaskAccountName(reader.GetString(3)),
                                Type = reader.GetByte(4),
                                Status = reader.GetByte(5),
                                Priority = reader.GetByte(6),
                                CreatedAt = reader.GetDateTime(7),
                                AdminReply = reader.IsDBNull(8) ? null : reader.GetString(8),
                                ReplyAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 管理员回复留言
        /// </summary>
        internal bool ReplyMessage(int messageId, string reply, out string message)
        {
            message = "回复成功";

            try
            {
                string sql = Config.isMysql ?
                    "UPDATE bulletin_messages SET admin_reply = @reply, reply_at = NOW(), status = 2, updated_at = NOW() WHERE id = @id" :
                    "UPDATE bulletin_messages SET admin_reply = @reply, reply_at = GETDATE(), status = 2, updated_at = GETDATE() WHERE id = @id";

                if (Config.isMysql)
                {
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@reply", reply);
                            cmd.Parameters.AddWithValue("@id", messageId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@reply", reply);
                            cmd.Parameters.AddWithValue("@id", messageId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Logger.info("管理员回复了留言 ID: {0}", messageId);
                return true;
            }
            catch (Exception ex)
            {
                message = "回复失败: " + ex.Message;
                Logger.error(message);
                return false;
            }
        }

        /// <summary>
        /// 获取所有留言（管理端使用）
        /// </summary>
        internal List<BulletinItem> GetAllMessages(int pageIndex, int pageSize, byte? statusFilter = null)
        {
            return Config.isMysql ?
                GetAllMessagesMySQL(pageIndex, pageSize, statusFilter) :
                GetAllMessagesMSSQL(pageIndex, pageSize, statusFilter);
        }

        private List<BulletinItem> GetAllMessagesMySQL(int pageIndex, int pageSize, byte? statusFilter)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, account_name, type, status, priority, created_at,
                          CASE WHEN admin_reply IS NOT NULL AND admin_reply != '' THEN 1 ELSE 0 END as has_reply
                          FROM bulletin_messages";

            if (statusFilter.HasValue)
            {
                sql += " WHERE status = @status";
            }

            sql += @" ORDER BY
                      CASE WHEN status = 0 THEN 0 ELSE 1 END,
                      priority DESC, created_at DESC
                      LIMIT @offset, @limit";

            using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    if (statusFilter.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@status", statusFilter.Value);
                    }
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                AccountName = reader.GetString("account_name"),
                                Type = reader.GetByte("type"),
                                Status = reader.GetByte("status"),
                                Priority = reader.GetByte("priority"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                HasReply = reader.GetInt32("has_reply") == 1
                            });
                        }
                    }
                }
            }
            return list;
        }

        private List<BulletinItem> GetAllMessagesMSSQL(int pageIndex, int pageSize, byte? statusFilter)
        {
            var list = new List<BulletinItem>();
            string sql = @"SELECT id, title, account_name, type, status, priority, created_at,
                          CASE WHEN admin_reply IS NOT NULL AND admin_reply != '' THEN 1 ELSE 0 END as has_reply
                          FROM bulletin_messages";

            if (statusFilter.HasValue)
            {
                sql += " WHERE status = @status";
            }

            sql += @" ORDER BY
                      CASE WHEN status = 0 THEN 0 ELSE 1 END,
                      priority DESC, created_at DESC
                      OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    if (statusFilter.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@status", statusFilter.Value);
                    }
                    cmd.Parameters.AddWithValue("@offset", pageIndex * pageSize);
                    cmd.Parameters.AddWithValue("@limit", pageSize);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BulletinItem
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                AccountName = reader.GetString(2),
                                Type = reader.GetByte(3),
                                Status = reader.GetByte(4),
                                Priority = reader.GetByte(5),
                                CreatedAt = reader.GetDateTime(6),
                                HasReply = reader.GetInt32(7) == 1
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取所有留言总数（管理端使用）
        /// </summary>
        internal int GetAllMessageCount(byte? statusFilter = null)
        {
            string sql = "SELECT COUNT(*) FROM bulletin_messages";
            if (statusFilter.HasValue)
            {
                sql += " WHERE status = " + statusFilter.Value;
            }
            return ExecuteScalar(sql);
        }

        /// <summary>
        /// 更新留言状态
        /// </summary>
        internal bool UpdateMessageStatus(int messageId, byte status, out string message)
        {
            message = "状态更新成功";

            try
            {
                string sql = Config.isMysql ?
                    "UPDATE bulletin_messages SET status = @status, updated_at = NOW() WHERE id = @id" :
                    "UPDATE bulletin_messages SET status = @status, updated_at = GETDATE() WHERE id = @id";

                if (Config.isMysql)
                {
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@id", messageId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@id", messageId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                message = "状态更新失败: " + ex.Message;
                Logger.error(message);
                return false;
            }
        }

        /// <summary>
        /// 删除留言（管理端使用）
        /// </summary>
        internal bool DeleteMessage(int messageId, out string message)
        {
            message = "删除成功";

            try
            {
                string sql = "DELETE FROM bulletin_messages WHERE id = @id";

                if (Config.isMysql)
                {
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@id", messageId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@id", messageId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Logger.info("删除了留言 ID: {0}", messageId);
                return true;
            }
            catch (Exception ex)
            {
                message = "删除失败: " + ex.Message;
                Logger.error(message);
                return false;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 账号名脱敏处理（保护用户隐私）
        /// </summary>
        private string MaskAccountName(string accountName)
        {
            if (string.IsNullOrEmpty(accountName) || accountName.Length <= 2)
            {
                return accountName;
            }

            int visibleStart = 1;
            int visibleEnd = 1;

            if (accountName.Length > 6)
            {
                visibleStart = 2;
                visibleEnd = 2;
            }

            string masked = accountName.Substring(0, visibleStart);
            masked += new string('*', accountName.Length - visibleStart - visibleEnd);
            masked += accountName.Substring(accountName.Length - visibleEnd);

            return masked;
        }

        /// <summary>
        /// 执行标量查询
        /// </summary>
        private int ExecuteScalar(string sql)
        {
            try
            {
                if (Config.isMysql)
                {
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }

    #region 数据模型

    /// <summary>
    /// 留言/公告列表项
    /// </summary>
    class BulletinItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AccountName { get; set; }
        public byte Type { get; set; }
        public byte Status { get; set; }
        public byte Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasReply { get; set; }
    }

    /// <summary>
    /// 留言详情
    /// </summary>
    class BulletinDetail
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AccountName { get; set; }
        public byte Type { get; set; }
        public byte Status { get; set; }
        public byte Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AdminReply { get; set; }
        public DateTime? ReplyAt { get; set; }
    }

    #endregion
}

using AionCommons.LogEngine;
using AionNetGate.Configs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AionNetGate.Services
{
    /// <summary>
    /// 高级数据库管理服务
    /// 提供备份、优化、监控等功能
    /// </summary>
    internal class DatabaseManagerService
    {
        internal static DatabaseManagerService Instance = new DatabaseManagerService();
        private static readonly Logger Logger = LoggerFactory.getLogger();

        private DatabaseManagerService() { }

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="databaseName">数据库名</param>
        /// <param name="backupPath">备份路径</param>
        /// <returns></returns>
        public bool BackupDatabase(string databaseName, string backupPath)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{databaseName}_backup_{timestamp}.bak";
                string fullPath = Path.Combine(backupPath, fileName);

                if (Config.isMysql)
                {
                    return BackupMySQL(databaseName, fullPath);
                }
                else
                {
                    return BackupMSSQL(databaseName, fullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.error("数据库备份失败：" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// MySQL数据库备份
        /// </summary>
        private bool BackupMySQL(string databaseName, string backupPath)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Config.GetMySQLConnectionString(databaseName)))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = connection;
                            connection.Open();
                            mb.ExportToFile(backupPath);
                            connection.Close();
                        }
                    }
                }
                Logger.info($"MySQL数据库 {databaseName} 备份成功：{backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.error($"MySQL数据库备份失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// MSSQL数据库备份
        /// </summary>
        private bool BackupMSSQL(string databaseName, string backupPath)
        {
            try
            {
                string sql = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH FORMAT, INIT";

                using (SqlConnection connection = new SqlConnection(Config.GetMSSQLConnectionString("master")))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 300; // 5分钟超时
                        cmd.ExecuteNonQuery();
                    }
                }
                Logger.info($"MSSQL数据库 {databaseName} 备份成功：{backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.error($"MSSQL数据库备份失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取数据库统计信息
        /// </summary>
        /// <returns></returns>
        public DatabaseStats GetDatabaseStats()
        {
            DatabaseStats stats = new DatabaseStats();

            try
            {
                if (Config.isMysql)
                {
                    stats = GetMySQLStats();
                }
                else
                {
                    stats = GetMSSQLStats();
                }
            }
            catch (Exception ex)
            {
                Logger.error("获取数据库统计信息失败：" + ex.Message);
            }

            return stats;
        }

        /// <summary>
        /// 获取MySQL统计信息
        /// </summary>
        private DatabaseStats GetMySQLStats()
        {
            DatabaseStats stats = new DatabaseStats();

            using (MySqlConnection connection = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
            {
                connection.Open();

                // 获取账号总数
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM account_data", connection))
                {
                    stats.TotalAccounts = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 获取在线账号数
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM account_data WHERE last_login_time > last_logout_time", connection))
                {
                    stats.OnlineAccounts = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 获取今日注册数
                using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM account_data WHERE DATE(created_at) = CURDATE()", connection))
                {
                    stats.TodayRegistrations = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return stats;
        }

        /// <summary>
        /// 获取MSSQL统计信息
        /// </summary>
        private DatabaseStats GetMSSQLStats()
        {
            DatabaseStats stats = new DatabaseStats();

            using (SqlConnection connection = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
            {
                connection.Open();

                if (Config.newaccountdatabase)
                {
                    // 新数据库结构
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[account_data]", connection))
                    {
                        stats.TotalAccounts = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[account_data] WHERE last_login_time > last_logout_time", connection))
                    {
                        stats.OnlineAccounts = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[account_data] WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)", connection))
                    {
                        stats.TodayRegistrations = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                else
                {
                    // 传统数据库结构
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[user_auth]", connection))
                    {
                        stats.TotalAccounts = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[user_data] WHERE last_login_time > last_logout_time", connection))
                    {
                        stats.OnlineAccounts = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[user_auth] WHERE CAST(lastat AS DATE) = CAST(GETDATE() AS DATE)", connection))
                    {
                        stats.TodayRegistrations = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }

            return stats;
        }

        /// <summary>
        /// 清理过期备份文件
        /// </summary>
        /// <param name="backupDirectory">备份目录</param>
        public void CleanOldBackups(string backupDirectory)
        {
            try
            {
                if (!Directory.Exists(backupDirectory))
                    return;

                DateTime cutoffDate = DateTime.Now.AddDays(-Config.backup_retention_days);
                string[] backupFiles = Directory.GetFiles(backupDirectory, "*_backup_*.bak");

                foreach (string file in backupFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        Logger.info($"删除过期备份文件：{file}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("清理过期备份失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 优化数据库
        /// </summary>
        public bool OptimizeDatabase()
        {
            try
            {
                if (Config.isMysql)
                {
                    return OptimizeMySQL();
                }
                else
                {
                    return OptimizeMSSQL();
                }
            }
            catch (Exception ex)
            {
                Logger.error("数据库优化失败：" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 优化MySQL数据库
        /// </summary>
        private bool OptimizeMySQL()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                {
                    connection.Open();

                    // 获取所有表
                    using (MySqlCommand cmd = new MySqlCommand("SHOW TABLES", connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> tables = new List<string>();
                            while (reader.Read())
                            {
                                tables.Add(reader.GetString(0));
                            }
                            reader.Close();

                            // 优化每个表
                            foreach (string table in tables)
                            {
                                using (MySqlCommand optimizeCmd = new MySqlCommand($"OPTIMIZE TABLE {table}", connection))
                                {
                                    optimizeCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                Logger.info("MySQL数据库优化完成");
                return true;
            }
            catch (Exception ex)
            {
                Logger.error($"MySQL数据库优化失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 优化MSSQL数据库
        /// </summary>
        private bool OptimizeMSSQL()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                {
                    connection.Open();

                    // 重新组织索引
                    string sql = @"
                        DECLARE @sql NVARCHAR(MAX) = ''
                        SELECT @sql = @sql + 'ALTER INDEX ALL ON ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' REORGANIZE;' + CHAR(13)
                        FROM sys.tables
                        EXEC sp_executesql @sql";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.ExecuteNonQuery();
                    }

                    // 更新统计信息
                    using (SqlCommand cmd = new SqlCommand("EXEC sp_updatestats", connection))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.ExecuteNonQuery();
                    }
                }
                Logger.info("MSSQL数据库优化完成");
                return true;
            }
            catch (Exception ex)
            {
                Logger.error($"MSSQL数据库优化失败：{ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// 数据库统计信息
    /// </summary>
    public class DatabaseStats
    {
        public int TotalAccounts { get; set; }
        public int OnlineAccounts { get; set; }
        public int TodayRegistrations { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
using AionNetGate.Configs.Core;
using System;

namespace AionNetGate.Configs.Sections
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DatabaseConfig : IConfigurationSection
    {
        public string SectionName => "Database";

        /// <summary>
        /// 数据库类型 (MySQL/MSSQL)
        /// </summary>
        public string DatabaseType { get; set; } = "MySQL";

        /// <summary>
        /// 数据库服务器地址
        /// </summary>
        public string Server { get; set; } = "localhost";

        /// <summary>
        /// 数据库端口
        /// </summary>
        public int Port { get; set; } = 3306;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = "root";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 登录服务器数据库名
        /// </summary>
        public string LoginDatabase { get; set; } = "eridian_ls";

        /// <summary>
        /// 游戏服务器数据库名
        /// </summary>
        public string GameDatabase { get; set; } = "eridian_gs";

        /// <summary>
        /// 字符编码
        /// </summary>
        public string Charset { get; set; } = "utf8mb4";

        /// <summary>
        /// 是否启用连接池
        /// </summary>
        public bool EnableConnectionPooling { get; set; } = true;

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// 最大连接池大小
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// 最小连接池大小
        /// </summary>
        public int MinPoolSize { get; set; } = 5;

        public bool IsValid()
        {
            if (StringHelper.IsNullOrWhiteSpace(Server))
                return false;

            if (Port < 1 || Port > 65535)
                return false;

            if (StringHelper.IsNullOrWhiteSpace(Username))
                return false;

            if (StringHelper.IsNullOrWhiteSpace(LoginDatabase) || StringHelper.IsNullOrWhiteSpace(GameDatabase))
                return false;

            if (!DatabaseType.Equals("MySQL", StringComparison.OrdinalIgnoreCase) &&
                !DatabaseType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
                return false;

            if (ConnectionTimeout < 1 || MaxPoolSize < 1 || MinPoolSize < 1)
                return false;

            if (MinPoolSize > MaxPoolSize)
                return false;

            return true;
        }

        public string GetValidationErrors()
        {
            var errors = new System.Text.StringBuilder();

            if (StringHelper.IsNullOrWhiteSpace(Server))
                errors.AppendLine("数据库服务器地址不能为空");

            if (Port < 1 || Port > 65535)
                errors.AppendLine("数据库端口号必须在1-65535范围内");

            if (StringHelper.IsNullOrWhiteSpace(Username))
                errors.AppendLine("数据库用户名不能为空");

            if (StringHelper.IsNullOrWhiteSpace(LoginDatabase))
                errors.AppendLine("登录服务器数据库名不能为空");

            if (StringHelper.IsNullOrWhiteSpace(GameDatabase))
                errors.AppendLine("游戏服务器数据库名不能为空");

            if (!DatabaseType.Equals("MySQL", StringComparison.OrdinalIgnoreCase) &&
                !DatabaseType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
                errors.AppendLine("数据库类型只支持MySQL或MSSQL");

            if (ConnectionTimeout < 1)
                errors.AppendLine("连接超时时间必须大于0秒");

            if (MaxPoolSize < 1)
                errors.AppendLine("最大连接池大小必须大于0");

            if (MinPoolSize < 1)
                errors.AppendLine("最小连接池大小必须大于0");

            if (MinPoolSize > MaxPoolSize)
                errors.AppendLine("最小连接池大小不能大于最大连接池大小");

            return errors.ToString();
        }

        public void ResetToDefaults()
        {
            DatabaseType = "MySQL";
            Server = "localhost";
            Port = 3306;
            Username = "root";
            Password = string.Empty;
            LoginDatabase = "eridian_ls";
            GameDatabase = "eridian_gs";
            Charset = "utf8mb4";
            EnableConnectionPooling = true;
            ConnectionTimeout = 30;
            MaxPoolSize = 100;
            MinPoolSize = 5;
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="databaseName">数据库名</param>
        /// <returns>连接字符串</returns>
        public string GetConnectionString(string databaseName)
        {
            if (DatabaseType.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
            {
                return "Server=" + Server + ";Port=" + Port + ";Database=" + databaseName + ";Uid=" + Username + ";Pwd=" + Password + ";Charset=" + Charset + ";Pooling=" + EnableConnectionPooling + ";Connection Timeout=" + ConnectionTimeout + ";Max Pool Size=" + MaxPoolSize + ";Min Pool Size=" + MinPoolSize + ";";
            }
            else if (DatabaseType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
            {
                return "Server=" + Server + "," + Port + ";Database=" + databaseName + ";User Id=" + Username + ";Password=" + Password + ";Pooling=" + EnableConnectionPooling + ";Connection Timeout=" + ConnectionTimeout + ";Max Pool Size=" + MaxPoolSize + ";Min Pool Size=" + MinPoolSize + ";";
            }

            throw new NotSupportedException("不支持的数据库类型: " + DatabaseType);
        }
    }
}
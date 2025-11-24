using AionNetGate.Configs.Core;
using AionCommons.LogEngine;
using System;

namespace AionNetGate.Configs
{
    /// <summary>
    /// 旧配置系统适配器，用于平滑迁移
    /// </summary>
    public static class LegacyConfigAdapter
    {
        private static readonly Logger log = LoggerFactory.getLogger();
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化配置适配器
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                log.info("正在初始化配置适配器...");

                // 加载新配置系统
                ConfigurationManager.Instance.LoadConfiguration();

                // 同步到旧配置系统
                SyncToLegacyConfig();

                _isInitialized = true;
                log.info("配置适配器初始化完成");

                // 输出配置摘要
                log.info(ConfigurationManager.Instance.GetConfigurationSummary());
            }
            catch (Exception ex)
            {
                log.error("配置适配器初始化失败: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void SaveConfiguration()
        {
            try
            {
                if (!_isInitialized)
                {
                    log.warn("配置适配器未初始化，正在初始化...");
                    Initialize();
                }

                // 从旧配置系统同步到新配置系统
                SyncFromLegacyConfig();

                // 保存新配置系统
                ConfigurationManager.Instance.SaveConfiguration();

                log.info("配置保存完成");
            }
            catch (Exception ex)
            {
                log.error("保存配置失败: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 从新配置系统同步到旧配置系统
        /// </summary>
        private static void SyncToLegacyConfig()
        {
            var config = ConfigurationManager.Instance;

            // 服务器配置
            Config.server_ip = config.Server.IPAddress;
            Config.server_port = config.Server.Port.ToString();
            Config.enable_two_ip = config.Server.EnableDualIP;
            Config.server_second_ip = config.Server.SecondaryIPAddress;

            // 数据库配置
            Config.mysql_url = config.Database.Server;
            Config.mysql_port = config.Database.Port.ToString();
            Config.mysql_user = config.Database.Username;
            Config.mysql_psw = config.Database.Password;
            Config.mysql_db_ls = config.Database.LoginDatabase;
            Config.mysql_db_gs = config.Database.GameDatabase;
            Config.mysql_code = config.Database.Charset;
            Config.isMysql = config.Database.DatabaseType.Equals("MySQL", StringComparison.OrdinalIgnoreCase);

            // 日志配置
            Config.enable_socket_log = config.Logging.EnableNetworkLogging;
            Config.enable_database_logging = config.Logging.EnableDatabaseLogging;

            // 安全配置
            Config.can_auto_ban_ip = config.Security.EnableAutoIPBan;
        }

        /// <summary>
        /// 从旧配置系统同步到新配置系统
        /// </summary>
        private static void SyncFromLegacyConfig()
        {
            var config = ConfigurationManager.Instance;

            // 服务器配置
            config.Server.IPAddress = Config.server_ip ?? "0.0.0.0";
            int port;
            if (int.TryParse(Config.server_port, out port))
                config.Server.Port = port;
            config.Server.EnableDualIP = Config.enable_two_ip;
            config.Server.SecondaryIPAddress = Config.server_second_ip ?? string.Empty;

            // 数据库配置
            config.Database.Server = Config.mysql_url ?? "localhost";
            int dbPort;
            if (int.TryParse(Config.mysql_port, out dbPort))
                config.Database.Port = dbPort;
            config.Database.Username = Config.mysql_user ?? "root";
            config.Database.Password = Config.mysql_psw ?? string.Empty;
            config.Database.LoginDatabase = Config.mysql_db_ls ?? "eridian_ls";
            config.Database.GameDatabase = Config.mysql_db_gs ?? "eridian_gs";
            config.Database.Charset = Config.mysql_code ?? "utf8mb4";
            config.Database.DatabaseType = Config.isMysql ? "MySQL" : "MSSQL";

            // 日志配置
            config.Logging.EnableNetworkLogging = Config.enable_socket_log;
            config.Logging.EnableDatabaseLogging = Config.enable_database_logging;

            // 安全配置
            config.Security.EnableAutoIPBan = Config.can_auto_ban_ip;
        }

        /// <summary>
        /// 获取配置管理器实例
        /// </summary>
        public static ConfigurationManager GetConfigurationManager()
        {
            if (!_isInitialized)
                Initialize();

            return ConfigurationManager.Instance;
        }

        /// <summary>
        /// 重置配置为默认值
        /// </summary>
        public static void ResetToDefaults()
        {
            try
            {
                log.info("正在重置配置为默认值...");

                ConfigurationManager.Instance.ResetAllToDefaults();
                SyncToLegacyConfig();
                SaveConfiguration();

                log.info("配置已重置为默认值");
            }
            catch (Exception ex)
            {
                log.error("重置配置失败: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        /// <returns>配置是否有效</returns>
        public static bool ValidateConfiguration()
        {
            try
            {
                if (!_isInitialized)
                    Initialize();

                return ConfigurationManager.Instance.ValidateAllSections();
            }
            catch (Exception ex)
            {
                log.error("验证配置失败: " + ex.Message);
                return false;
            }
        }
    }
}
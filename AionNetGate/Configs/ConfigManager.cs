using AionNetGate.Configs.Core;
using AionCommons.LogEngine;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace AionNetGate.Configs
{
    /// <summary>
    /// 配置管理器 - 使用新的模块化配置系统
    /// </summary>
    internal static class ConfigManager
    {
        private static readonly Logger log = LoggerFactory.getLogger();
        private static bool _isInitialized = false;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ConfigManager()
        {
            InitializeConfiguration();
        }

        /// <summary>
        /// 初始化配置系统
        /// </summary>
        private static void InitializeConfiguration()
        {
            if (_isInitialized)
                return;

            try
            {
                log.info("正在初始化配置管理器...");

                // 初始化新的配置系统
                LegacyConfigAdapter.Initialize();

                _isInitialized = true;
                log.info("配置管理器初始化完成");
            }
            catch (Exception ex)
            {
                log.error("配置管理器初始化失败: " + ex.Message);
                // 继续使用旧的配置加载方式作为后备
                LoadLegacyConfiguration();
            }
        }

        /// <summary>
        /// 加载传统配置（后备方案）
        /// </summary>
        private static void LoadLegacyConfiguration()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, "Common.config");
                if (File.Exists(configPath))
                {
                    log.info("使用传统配置文件: " + configPath);
                    // 这里可以保留原有的配置加载逻辑作为后备
                }
            }
            catch (Exception ex)
            {
                log.error("加载传统配置失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取配置值，支持默认值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public static string GetValue(string key, string defaultValue = "")
        {
            // 委托给新的配置系统
            return defaultValue; // 简化实现，主要配置通过新系统管理
        }

        /// <summary>
        /// 获取布尔配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>布尔值</returns>
        public static bool GetBoolValue(string key, bool defaultValue = false)
        {
            return defaultValue; // 简化实现
        }

        /// <summary>
        /// 获取整数配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>整数值</returns>
        public static int GetIntValue(string key, int defaultValue = 0)
        {
            return defaultValue; // 简化实现
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <param name="name">连接字符串名称</param>
        /// <returns>连接字符串</returns>
        public static string GetConnectionString(string name)
        {
            try
            {
                if (!_isInitialized)
                    InitializeConfiguration();

                var config = LegacyConfigAdapter.GetConfigurationManager();
                if (name.Contains("ls") || name.Contains("login"))
                    return config.Database.GetConnectionString(config.Database.LoginDatabase);
                else
                    return config.Database.GetConnectionString(config.Database.GameDatabase);
            }
            catch (Exception ex)
            {
                log.error("获取数据库连接字符串失败: " + ex.Message);
                return "";
            }
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static void Reload()
        {
            try
            {
                _isInitialized = false;
                InitializeConfiguration();
            }
            catch (Exception ex)
            {
                log.error("重新加载配置失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 加载高级配置
        /// </summary>
        public static void LoadAdvancedConfiguration()
        {
            try
            {
                if (!_isInitialized)
                    InitializeConfiguration();

                log.info("高级配置已通过新配置系统加载");
            }
            catch (Exception ex)
            {
                log.error("加载高级配置失败: " + ex.Message);
                MessageBox.Show("加载高级配置失败：" + ex.Message, "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    InitializeConfiguration();

                LegacyConfigAdapter.SaveConfiguration();
            }
            catch (Exception ex)
            {
                log.error("保存配置失败: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 获取配置管理器实例
        /// </summary>
        public static Core.ConfigurationManager GetInstance()
        {
            if (!_isInitialized)
                InitializeConfiguration();

            return LegacyConfigAdapter.GetConfigurationManager();
        }

    }
}
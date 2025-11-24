using AionNetGate.Configs.Sections;
using AionCommons.LogEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace AionNetGate.Configs.Core
{
    /// <summary>
    /// 统一配置管理器
    /// </summary>
    public sealed class ConfigurationManager
    {
        private static ConfigurationManager _instance;
        private static readonly object _lock = new object();

        private static readonly Logger log = LoggerFactory.getLogger();

        private readonly Dictionary<string, IConfigurationSection> _sections;
        private readonly string _configFilePath;

        private ConfigurationManager()
        {
            _sections = new Dictionary<string, IConfigurationSection>();
            _configFilePath = Path.Combine(Application.StartupPath, "gateway.config.xml");
            InitializeSections();
        }

        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ConfigurationManager();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 服务器配置
        /// </summary>
        public ServerConfig Server => GetSection<ServerConfig>();

        /// <summary>
        /// 数据库配置
        /// </summary>
        public DatabaseConfig Database => GetSection<DatabaseConfig>();

        /// <summary>
        /// 日志配置
        /// </summary>
        public LoggingConfig Logging => GetSection<LoggingConfig>();

        /// <summary>
        /// 安全配置
        /// </summary>
        public SecurityConfig Security => GetSection<SecurityConfig>();

        /// <summary>
        /// 初始化配置节
        /// </summary>
        private void InitializeSections()
        {
            RegisterSection(new ServerConfig());
            RegisterSection(new DatabaseConfig());
            RegisterSection(new LoggingConfig());
            RegisterSection(new SecurityConfig());
        }

        /// <summary>
        /// 注册配置节
        /// </summary>
        /// <param name="section">配置节</param>
        private void RegisterSection(IConfigurationSection section)
        {
            _sections[section.SectionName] = section;
        }

        /// <summary>
        /// 获取配置节
        /// </summary>
        /// <typeparam name="T">配置节类型</typeparam>
        /// <returns>配置节实例</returns>
        public T GetSection<T>() where T : class, IConfigurationSection
        {
            var sectionName = typeof(T).Name.Replace("Config", "");
            IConfigurationSection section;
            if (_sections.TryGetValue(sectionName, out section))
            {
                return section as T;
            }
            throw new InvalidOperationException("配置节 " + sectionName + " 未注册");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public void LoadConfiguration()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    log.info("配置文件不存在，创建默认配置文件");
                    CreateDefaultConfigFile();
                    return;
                }

                log.info("正在加载配置文件: " + _configFilePath);

                var doc = new XmlDocument();
                doc.Load(_configFilePath);

                foreach (var kvp in _sections)
                {
                    LoadSectionFromXml(doc, kvp.Value);
                }

                // 验证所有配置
                ValidateAllSections();

                log.info("配置文件加载完成");
            }
            catch (Exception ex)
            {
                log.error("加载配置文件失败: " + ex.Message);
                log.warn("使用默认配置");
                ResetAllToDefaults();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                log.info("正在保存配置文件: " + _configFilePath);

                var doc = new XmlDocument();
                var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.InsertBefore(declaration, doc.DocumentElement);

                var root = doc.CreateElement("GatewayConfiguration");
                doc.AppendChild(root);

                foreach (var kvp in _sections)
                {
                    SaveSectionToXml(doc, root, kvp.Value);
                }

                // 确保目录存在
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                doc.Save(_configFilePath);
                log.info("配置文件保存完成");
            }
            catch (Exception ex)
            {
                log.error("保存配置文件失败: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 验证所有配置节
        /// </summary>
        public bool ValidateAllSections()
        {
            var allValid = true;
            var errors = new StringBuilder();

            foreach (var kvp in _sections)
            {
                if (!kvp.Value.IsValid())
                {
                    allValid = false;
                    errors.AppendLine("[" + kvp.Key + "] " + kvp.Value.GetValidationErrors());
                }
            }

            if (!allValid)
            {
                log.error("配置验证失败:\\n" + errors.ToString());
            }

            return allValid;
        }

        /// <summary>
        /// 重置所有配置为默认值
        /// </summary>
        public void ResetAllToDefaults()
        {
            foreach (var kvp in _sections)
            {
                kvp.Value.ResetToDefaults();
            }
            log.info("所有配置已重置为默认值");
        }

        /// <summary>
        /// 创建默认配置文件
        /// </summary>
        private void CreateDefaultConfigFile()
        {
            ResetAllToDefaults();
            SaveConfiguration();
        }

        /// <summary>
        /// 从XML加载配置节
        /// </summary>
        /// <param name="doc">XML文档</param>
        /// <param name="section">配置节</param>
        private void LoadSectionFromXml(XmlDocument doc, IConfigurationSection section)
        {
            try
            {
                var sectionNode = doc.SelectSingleNode("//GatewayConfiguration/" + section.SectionName);
                if (sectionNode == null)
                {
                    log.warn("配置文件中未找到 " + section.SectionName + " 节，使用默认值");
                    return;
                }

                // 使用反射设置属性值
                var sectionType = section.GetType();
                var properties = sectionType.GetProperties();

                foreach (var property in properties)
                {
                    if (property.Name == "SectionName")
                        continue;

                    var propertyNode = sectionNode.SelectSingleNode(property.Name);
                    if (propertyNode != null)
                    {
                        SetPropertyFromXml(section, property, propertyNode.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                log.error("加载配置节 " + section.SectionName + " 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 保存配置节到XML
        /// </summary>
        /// <param name="doc">XML文档</param>
        /// <param name="root">根节点</param>
        /// <param name="section">配置节</param>
        private void SaveSectionToXml(XmlDocument doc, XmlElement root, IConfigurationSection section)
        {
            var sectionElement = doc.CreateElement(section.SectionName);
            root.AppendChild(sectionElement);

            var sectionType = section.GetType();
            var properties = sectionType.GetProperties();

            foreach (var property in properties)
            {
                if (property.Name == "SectionName")
                    continue;

                var value = property.GetValue(section, null);
                var valueString = ConvertToString(value);

                var propertyElement = doc.CreateElement(property.Name);
                propertyElement.InnerText = valueString;
                sectionElement.AppendChild(propertyElement);
            }
        }

        /// <summary>
        /// 从XML设置属性值
        /// </summary>
        /// <param name="section">配置节</param>
        /// <param name="property">属性信息</param>
        /// <param name="value">字符串值</param>
        private void SetPropertyFromXml(IConfigurationSection section, System.Reflection.PropertyInfo property, string value)
        {
            try
            {
                object convertedValue = ConvertFromString(value, property.PropertyType);
                property.SetValue(section, convertedValue, null);
            }
            catch (Exception ex)
            {
                log.warn("设置属性 " + property.Name + " 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 将对象转换为字符串
        /// </summary>
        /// <param name="value">对象值</param>
        /// <returns>字符串值</returns>
        private string ConvertToString(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is System.Collections.Generic.List<string> list)
            {
                return string.Join(";", list.ToArray());
            }

            return value.ToString();
        }

        /// <summary>
        /// 从字符串转换为指定类型
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private object ConvertFromString(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);
                return null;
            }

            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(bool))
                return bool.Parse(value);

            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(ushort))
                return ushort.Parse(value);

            if (targetType == typeof(System.Collections.Generic.List<string>))
            {
                var list = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(value))
                {
                    list.AddRange(value.Split(';'));
                }
                return list;
            }

            // 使用Convert.ChangeType作为后备
            return Convert.ChangeType(value, targetType);
        }

        /// <summary>
        /// 获取配置摘要信息
        /// </summary>
        /// <returns>配置摘要</returns>
        public string GetConfigurationSummary()
        {
            var summary = new StringBuilder();
            summary.AppendLine("=== 网关配置摘要 ===");
            summary.AppendLine("服务器: " + Server.IPAddress + ":" + Server.Port);
            summary.AppendLine("数据库: " + Database.DatabaseType + " - " + Database.Server + ":" + Database.Port);
            summary.AppendLine("安全模式: " + (Security.EnableEnhancedSecurity ? "启用" : "禁用"));
            summary.AppendLine("详细日志: " + (Logging.EnableVerboseLogging ? "启用" : "禁用"));
            summary.AppendLine("配置文件: " + _configFilePath);
            return summary.ToString();
        }
    }
}
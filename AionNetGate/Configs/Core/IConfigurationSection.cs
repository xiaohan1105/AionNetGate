using System;

namespace AionNetGate.Configs.Core
{
    /// <summary>
    /// 配置节接口
    /// </summary>
    public interface IConfigurationSection
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        string SectionName { get; }

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        /// <returns>验证结果</returns>
        bool IsValid();

        /// <summary>
        /// 获取验证错误信息
        /// </summary>
        /// <returns>错误信息</returns>
        string GetValidationErrors();

        /// <summary>
        /// 重置为默认值
        /// </summary>
        void ResetToDefaults();
    }
}
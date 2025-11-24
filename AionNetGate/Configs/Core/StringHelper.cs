using System;

namespace AionNetGate.Configs.Core
{
    /// <summary>
    /// .NET Framework 2.0 兼容性帮助类
    /// </summary>
    internal static class StringHelper
    {
        /// <summary>
        /// 检查字符串是否为null或空白字符
        /// (.NET Framework 2.0 兼容版本)
        /// </summary>
        /// <param name="value">要检查的字符串</param>
        /// <returns>如果字符串为null、空字符串或仅包含空白字符，则返回true</returns>
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;

            if (value.Length == 0)
                return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查字符串是否为null或空字符串
        /// </summary>
        /// <param name="value">要检查的字符串</param>
        /// <returns>如果字符串为null或空字符串，则返回true</returns>
        public static bool IsNullOrEmpty(string value)
        {
            return value == null || value.Length == 0;
        }
    }
}
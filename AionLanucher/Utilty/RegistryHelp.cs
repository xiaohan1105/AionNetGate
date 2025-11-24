using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher.Utilty
{
    /// <summary>
    /// 注册表操作类
    /// </summary>
    class RegistryHelp
    {
        /// <summary>
        /// 获得根节点注册表
        /// </summary>
        /// <param name="rootKeyType">根节点类型</param>
        /// <returns></returns>
        private static RegistryKey GetRootRegisterKey(RootKeyType rootKeyType)
        {
            if (rootKeyType == RootKeyType.HKEY_CLASSES_ROOT)
                return Registry.ClassesRoot;
            else if (rootKeyType == RootKeyType.HKEY_CURRENT_CONFIG)
                return Registry.CurrentConfig;
            else if (rootKeyType == RootKeyType.HKEY_CURRENT_USER)
                return Registry.CurrentUser;
            else if (rootKeyType == RootKeyType.HKEY_LOCAL_MACHINE)
                return Registry.LocalMachine;
            else
                throw new Exception("注册表类型未定义！");
        }

        /// <summary>
        /// 在指定注册表项下创建注册表子项
        /// </summary>
        /// <param name="fatherKey">父注册表项</param>
        /// <param name="keyPath">注册表路径</param>
        /// <returns></returns>
        private static RegistryKey CreateRegistryKey(RegistryKey fatherKey, string keyPath)
        {
            RegistryKey returnKey = fatherKey.CreateSubKey(keyPath);
            return returnKey;
        }

        /// <summary>
        /// 在指定注册表项下创建注册表子项
        /// </summary>
        /// <param name="fatherKey">父注册表项</param>
        /// <param name="keyPath">注册表路径</param>
        /// <param name="keyValueName">要添加的注册表项值名称</param>
        /// <param name="keyValue">要添加的注册表项值</param>
        /// <returns></returns>
        public static RegistryKey CreateRegistryKey(RegistryKey fatherKey, string keyPath, string keyValueName, string keyValue)
        {
            RegistryKey returnKey = CreateRegistryKey(fatherKey, keyPath);
            returnKey.SetValue(keyValueName, keyValue);
            return returnKey;
        }

        /// <summary>
        /// 在指定注册表项下创建注册表子项
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <returns></returns>
        public static RegistryKey CreateRegistryKey(RootKeyType rootKeyType, string keyPath)
        {
            RegistryKey rootKey = GetRootRegisterKey(rootKeyType);
            return CreateRegistryKey(rootKeyType, keyPath);
        }

        /// <summary>
        /// 在指定注册表项下创建注册表子项
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <param name="keyValueName">要添加的注册表项值名称</param>
        /// <param name="keyValue">要添加的注册表项值</param>
        /// <returns></returns>
        public static RegistryKey CreateRegistryKey(RootKeyType rootKeyType, string keyPath, string keyValueName, string keyValue)
        {
            RegistryKey returnKey = CreateRegistryKey(rootKeyType, keyPath);
            returnKey.SetValue(keyValueName, keyValue);
            return returnKey;
        }

        /// <summary>
        /// 删除注册表子项
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <returns></returns>
        public static bool DeleteRegistryKey(RootKeyType rootKeyType, string keyPath, string keyName)
        {
            RegistryKey rootKey = GetRootRegisterKey(rootKeyType);
            RegistryKey deleteKey = rootKey.OpenSubKey(keyPath);

            if (IsKeyHaveSubKey(deleteKey, keyName))
            {
                deleteKey.DeleteSubKey(keyName);
                return true;
            }
            return false;
        }



        /// <summary>
        /// 获取注册表项
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <returns></returns>
        public static RegistryKey GetRegistryKey(RootKeyType rootKeyType, string keyPath)
        {
            RegistryKey rootKey = GetRootRegisterKey(rootKeyType);
            return rootKey.OpenSubKey(keyPath);
        }



        /// <summary>
        /// 获取注册表项指定值
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <param name="keyName">要获得值的注册表值名称</param>
        /// <returns></returns>
        public static string GetRegistryValue(RootKeyType rootKeyType, string keyPath, string keyName)
        {
            RegistryKey key = GetRegistryKey(rootKeyType, keyPath);
            if (key == null)
                return null;
            if (IsKeyHaveValue(key, keyName))
            {
                return key.GetValue(keyName).ToString();
            }
            return null;
        }



        /// <summary>
        /// 设置注册表项值
        /// </summary>
        /// <param name="key">注册表项</param>
        /// <param name="keyValueName">值名称</param>
        /// <param name="keyValue">值</param>
        /// <returns></returns>
        public static bool SetRegistryValue(RegistryKey key, string keyValueName, string keyValue)
        {
            key.SetValue(keyValueName, keyValue);
            return true;
        }



        /// <summary>
        /// 设置注册表项值
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <param name="keyValueName">值名称</param>
        /// <param name="keyValue">值</param>
        /// <returns></returns>
        public static bool SetRegistryValue(RootKeyType rootKeyType, string keyPath, string keyValueName, string keyValue)
        {
            try
            {
                RegistryKey key = GetRegistryKey(rootKeyType, keyPath);

                if (key == null)
                {
                    key = CreateRegistryKey(rootKeyType, keyPath);
                }


                key.SetValue(keyValueName, keyValue);

                key.Close();
                return true;
            }
            catch(Exception e)
            {
                Debug.Fail(e.ToString());
            }
            return false;
        }



        /// <summary>
        /// 删除注册表项值
        /// </summary>
        /// <param name="key">注册表项</param>
        /// <param name="keyValueName">值名称</param>
        /// <param name="keyValue">值</param>
        /// <returns></returns>
        public static bool DeleteRegistryValue(RegistryKey key, string keyValueName)
        {
            if (IsKeyHaveValue(key, keyValueName))
            {
                key.DeleteValue(keyValueName);
                return true;
            }
            return false;
        }



        /// <summary>
        /// 删除注册表项值
        /// </summary>
        /// <param name="rootKeyType">注册表根节点项类型</param>
        /// <param name="keyPath">注册表路径</param>
        /// <param name="keyValueName">值名称</param>
        /// <returns></returns>
        public static bool DeleteRegistryValue(RootKeyType rootKeyType, string keyPath, string keyValueName)
        {
            RegistryKey key = GetRegistryKey(rootKeyType, keyPath);
            if (IsKeyHaveValue(key, keyValueName))
            {
                key.DeleteValue(keyValueName);
                return true;
            }
            return false;
        }



        /// <summary>
        /// 判断注册表项是否有指定的注册表值
        /// </summary>
        /// <param name="key">注册表项</param>
        /// <param name="valueName">注册表值</param>
        /// <returns></returns>
        private static bool IsKeyHaveValue(RegistryKey key, string valueName)
        {
            string[] keyNames = key.GetValueNames();
            foreach (string keyName in keyNames)
            {
                if (keyName.Trim() == valueName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断注册表项是否有指定的子项
        /// </summary>
        /// <param name="key">注册表项</param>
        /// <param name="keyName">子项名称</param>
        /// <returns></returns>
        private static bool IsKeyHaveSubKey(RegistryKey key, string keyName)
        {
            string[] subKeyNames = key.GetSubKeyNames();
            foreach (string subKeyName in subKeyNames)
            {
                if (keyName.Trim() == keyName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据注册表键路径获得注册表键名称
        /// </summary>
        /// <param name="keyPath">注册表键路径</param>
        /// <returns></returns>
        private static string GetKeyNameByPath(string keyPath)
        {
            int keyIndex = keyPath.LastIndexOf("/");
            return keyPath.Substring(keyIndex + 1);
        }




        /**/
        /// <summary>
        /// 写入注册表
        /// </summary>
        /// <param name="strName"></param>
        public static string GetRegEditData(string path, string strName)
        {
            try
            {
                RegistryKey hklm = Registry.CurrentUser;
                RegistryKey software = hklm.OpenSubKey(path, true);
                if (software == null)
                    return null;
                object o = software.GetValue(strName);
                if (o != null)
                    return o.ToString();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
            return null;
        }

        /**/
        /// <summary>
        /// 写入注册表
        /// </summary>
        /// <param name="strName"></param>
        public static void SetRegEditData(string path, string strName, string strValue)
        {
            try
            {
                RegistryKey user = Registry.CurrentUser;
                RegistryKey software = user.OpenSubKey(path, true);
                if(software==null)
                    software = user.CreateSubKey(path);
                software.SetValue(strName, strValue);
                software.Close();
                user.Close();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }

        }


        /**/
        /// <summary>
        /// 修改注册表项
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        public static bool DeleteRegEditData(string path, string strName)
        {
            try
            {
                RegistryKey hklm = Registry.CurrentUser;
                RegistryKey software = hklm.OpenSubKey(path, true);
                if (software != null && software.GetValueNames().Length > 0)
                {
                    foreach (string s in software.GetValueNames())
                    {
                        if (s.Equals(strName))
                        {
                            software.DeleteValue(strName);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
            return false;
        }
        /**/
        /// <summary>
        /// 判断指定注册表项是否存在
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static bool IsExist(string path, string key)
        {
            try
            {
                bool exit = false;
                string[] subkeyNames;
                RegistryKey hkml = Registry.CurrentUser;
                RegistryKey software = hkml.OpenSubKey(path, true);
                subkeyNames = software.GetValueNames();
                foreach (string keyName in subkeyNames)
                {
                    if (keyName == key)
                    {
                        exit = true;
                        return exit;
                    }
                }
                return exit;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                return false;
            }
        }

    }

    public enum RootKeyType
    {
        HKEY_CLASSES_ROOT = 0,
        HKEY_CURRENT_USER = 1,
        HKEY_LOCAL_MACHINE = 2,
        HKEY_USERS = 3,
        HKEY_CURRENT_CONFIG = 4
    }
}


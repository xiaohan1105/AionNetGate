using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AionLanucher.Utilty
{
    class Language
    {
        private static string lang = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
        /// <summary>
        /// en-US  zh-CN  zh-TW
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string getLang(string str)
        {
            if (lang == "zh-CN")
            {
                return str;
            }
            else if (lang == "zh-TW" || lang == "zh-HK") //zh-CN
            {
                return ToTraditional(str);
            }
            else //if (lang == "en-US")
            {
                switch (str)
                {
                    case "当前更新进度": return "CURRENT FILE";
                    case "总体更新进度": return "TOTAL FILES";
                    case "更新文件": return "Check for update";
                    case "在线状态": return "STATUS";
                    case "永恒登陆器": return "Aion Launcher";
                    case "启动游戏失败！": return "Failed to start the game!";
                    case "初始化失败，请重试！": return "Initialization failed, please try again!";
                    case "目录[ {0} ]不存在，请确认是否将登陆器放在客户端根目录下！": return "Directory [{0}] does not exist, Please confirm whether the launcher is in root directory of the aion client!";
                    case "双击图标可恢复登陆器界面！": return "Double-click the icon to show launcher's form!";

                    case "无法连接到更新服务器！": return "Unable to connect to the update server!";
                    case "正在检查远程更新文件,请稍后...": return "Checking remote update files, please wait...";
                    case "已检查到可更新的补丁文件{0}个": return "Checked {0} patch files can be updated.";
                    case "当前没有可用更新": return "No patch file need to update.";
                    case "已完成补丁更新": return "Patch update has been completed.";
                    case "正在更新": return "Updating";
                    case "速度": return "Speed";
                    case "更新文件数": return "Number of files";
                    case "提醒": return "NOTICE";
                    case "您的登陆器已过期，请下载最新版本！\n提醒\n确定\n错误": return "Your launcher has expired,Please download the latest!\nNOTICE\n确定\n错误";
                    case "您的登陆器已被GM禁止！\n提醒\n确定\n警告": return "Your launcher has been banned!\nNOTICE\n确定\n警告";
                    case "登陆器程序已运行，请不要重复启动！": return "The launcher is already running,please don't run it again.";
                    case "请将登陆器放于永恒之塔根目录启动！": return "Please put the launcher to the root directory of aion client.";
                    case "警告": return "WARNING";
                    case "在您的WinXP系统上无法启动游戏！": return "Initialization failed on your WinXP OS!";
                    case "已经拨了这个连接": return "This connection is already being dialed!";
                    case "Windows XP系统初始化失败\r\n请重启电脑后再开登陆器": return "Initialization failed on Windows XP\r\nRestart your pc and try again!";
                    case "连接被取消!": return "Connection is canceled";
                    case "连接超时!": return "Connection Timeout!";
                    case "WinXP初始化失败，请重启电脑。": return "Initialization failed on your WinXP OS, restart your pc and try again";
                    case "WinXP初始化成功!": return "Successful initialization on your WinXP OS!";
                    case "已停止初始化": return "Stopped initialized";
                }
            }
            return str;
        }

        /// <summary>
        /// 中文字符工具类
        /// </summary>
        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary>
        /// 将字符转换成简体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        private static string ToSimplified(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        /// <summary>
        /// 讲字符转换为繁体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        private static string ToTraditional(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, source, source.Length, target, source.Length);
            return target;
        }
    }
}

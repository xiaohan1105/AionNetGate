using AionLanucher.Configs;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher
{
    static class Program
    {

        /// <summary>
        /// 互斥法防止程序重复运行
        private static Mutex myMutex;
        private static bool requestInitialOwnership = true;
        private static bool mutexWasCreated;

        private const int WS_SHOWNORMAL = 1;
        public const Int32 AW_BLEND = 0x00080000;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("wc"))
            {
                //新代码
                myMutex = new Mutex(requestInitialOwnership, "AionLauncher", out mutexWasCreated);
                if (mutexWasCreated)
                {
                    IntPtr hwnd = WinAPI.FindWindow(null, Config.Name);
                    if (hwnd.ToInt32() > 0)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new WatchDog("AionLauncher", hwnd));
                        myMutex.WaitOne();
                        return;
                    }
                }
                else
                {
                    if (Config.CanDoubleStart)
                    {
                        //如果支持双开，则创建第2条互斥名称
                        myMutex = new Mutex(requestInitialOwnership, "AionLauncher2", out mutexWasCreated);
                        if (mutexWasCreated)
                        {
                            IntPtr hwnd = WinAPI.FindWindow(null, Config.Name + "2");
                            if (hwnd.ToInt32() > 0)
                            {
                                Application.EnableVisualStyles();
                                Application.SetCompatibleTextRenderingDefault(false);
                                Application.Run(new WatchDog("AionLauncher2", hwnd));
                                myMutex.WaitOne();
                            }
                        }
                        else
                        {
                            MessageBox.Show("登录器双开启动异常，请打开任务管理器结束掉所有登录器进程后重新打开", "帮助提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("登录器启动异常，请打开任务管理器结束掉登录器进程后重新打开", "帮助提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                return;
            }


            //新代码
            myMutex = new Mutex(requestInitialOwnership, Config.Name, out mutexWasCreated);
            if (mutexWasCreated)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(Config.Name));
                myMutex.WaitOne();
            }
            else
            {
                if (Config.CanDoubleStart)
                {
                    //如果支持双开，则创建第2条互斥名称
                    myMutex = new Mutex(requestInitialOwnership, Config.Name + "2", out mutexWasCreated);
                    if (mutexWasCreated)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm(Config.Name + "2"));
                        myMutex.WaitOne();
                    }
                    else //如果已经有第2个登录器了，那么提示
                    {
                        IntPtr hwnd = WinAPI.FindWindow(null, Config.Name);
                        if (hwnd.ToInt32() > 0)
                        {
                            WinAPI.AnimateWindow(hwnd, 3000, AW_BLEND);
                            WinAPI.ShowWindowAsync(hwnd, WS_SHOWNORMAL);
                            WinAPI.SetForegroundWindow(hwnd);
                        }

                        hwnd = WinAPI.FindWindow(null, Config.Name + "2");
                        if (hwnd.ToInt32() > 0)
                        {
                            WinAPI.AnimateWindow(hwnd, 3000, AW_BLEND);
                            WinAPI.ShowWindowAsync(hwnd, WS_SHOWNORMAL);
                            WinAPI.SetForegroundWindow(hwnd);
                        }

                        MessageBox.Show("您已经开启了2个登录器，无法再多开了！", "灰色枫叶警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    //如果不支持双开，则再启动第2个登录器时候把 第一个登录器显示出来
                    IntPtr hwnd = WinAPI.FindWindow(null, Config.Name);
                    if (hwnd.ToInt32() > 0)
                    {
                        WinAPI.AnimateWindow(hwnd, 3000, AW_BLEND);
                        WinAPI.ShowWindowAsync(hwnd, WS_SHOWNORMAL);
                        WinAPI.SetForegroundWindow(hwnd);
                    }
                    else
                    {
                        MessageBox.Show("启动登录器失败，请尝试重启电脑后打次打开！", "灰色枫叶警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}

using AionCommons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AionNetGate
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
        static void Main()
        {
            //新代码
            myMutex = new Mutex(requestInitialOwnership, "通用网关V5.5", out mutexWasCreated);
            if (mutexWasCreated)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm("通用网关V5.5"));
                myMutex.WaitOne();
            }
            else
            {
                IntPtr hwnd = NativeMethods.FindWindow(null, "通用网关V5.5");
                if (hwnd.ToInt32() > 0)
                {
                    try
                    {
                        int PID = 0;
                        NativeMethods.GetWindowThreadProcessId(hwnd, out PID);
                        Process Pr = Process.GetProcessById(PID);
                        if (Path.GetDirectoryName(Pr.MainModule.FileName).Equals(Application.StartupPath))
                        {
                            NativeMethods.AnimateWindow(hwnd, 3000, AW_BLEND);
                            NativeMethods.ShowWindowAsync(hwnd, WS_SHOWNORMAL);
                            NativeMethods.SetForegroundWindow(hwnd);
                        }
                        else
                        {
                            myMutex = new Mutex(requestInitialOwnership, "通用网关V5.5", out mutexWasCreated);
                            if (mutexWasCreated)
                            {
                                Application.EnableVisualStyles();
                                Application.SetCompatibleTextRenderingDefault(false);
                                Application.Run(new MainForm("通用网关V5.5"));
                                myMutex.WaitOne();
                            }
                            else
                            {
                                IntPtr hwnd2 = NativeMethods.FindWindow(null, "通用网关V5.5");
                                if (hwnd2.ToInt32() > 0)
                                {
                                    NativeMethods.AnimateWindow(hwnd2, 3000, AW_BLEND);
                                    NativeMethods.ShowWindowAsync(hwnd2, WS_SHOWNORMAL);
                                    NativeMethods.SetForegroundWindow(hwnd2);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "通用网关V5.5");
                    }

                }
                else
                {
                    MessageBox.Show("启动通用网关失败，请尝试重启电脑后打次打开！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

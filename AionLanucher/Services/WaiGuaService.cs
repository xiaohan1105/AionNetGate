using AionLanucher.Configs;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher.Services
{
    class WaiGuaService
    {
        /// <summary>
        /// 外挂名
        /// </summary>
        private List<string> waigua_name = new List<string>();
        /// <summary>
        /// 外挂MD5值
        /// </summary>
        private List<string> waigua_md5 = new List<string>();
        /// <summary>
        /// 外挂窗口类名
        /// </summary>
        private List<string> waigua_classname = new List<string>();

        private static string[] EXCEPT_MD5 = new string[] { 
            "807E12CE7417388B490AF62F0A2AD3AF", 
            "81C8746F49B854A300443475B8A32BAC", 
            "BF161A095864880A4C6D4F8E2C151F11",
            "1AE23D3DE4617F8D241CEEF38A1E4A8F"
        };

        private List<string> ExceptMD5;

        internal static WaiGuaService Instance = new WaiGuaService();

        internal WaiGuaService()
        {
            if (Config.CLIENT_WAIGUA == null || Config.CLIENT_WAIGUA.Length == 0)//外挂数检查
                return;
            foreach (string s in Config.CLIENT_WAIGUA)
            {
                if (!s.Contains("="))
                    continue;
                string[] ss = s.Split('=');
                if (ss[0].Equals("EXENAME"))
                {
                    waigua_name.Add(ss[1].ToLower());
                }
                else if (ss[0].Equals("EXEMD5"))
                {
                    waigua_md5.Add(ss[1].ToUpper());
                }
                else if (ss[0].Equals("EXECLASS"))
                {
                    waigua_classname.Add(ss[1]);
                }
            }
            if(!waigua_classname.Contains("Window"))
                waigua_classname.Add("Window");

            ExceptMD5 = new List<string>(EXCEPT_MD5);
        }
    

        private Thread CheckThread;
        private Thread CheckThread2;

        private bool running;
        internal void Start()
        {
            running = true;
            CheckThread = new Thread(CheckClassName);
            CheckThread.IsBackground = true;
            CheckThread.Start();

            CheckThread2 = new Thread(CheckWGbyName);
            CheckThread2.IsBackground = true;
            CheckThread2.Start();
        }

        internal void Stop()
        {
            running = true;
            if (CheckThread != null && CheckThread.IsAlive)
            {
                CheckThread.Abort();
            }
            if (CheckThread2 != null && CheckThread2.IsAlive)
            {
                CheckThread2.Abort();
            }
        }


        /// <summary>
        /// 启动游戏后定时检查外挂
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CheckClassName()
        {
            if (waigua_classname.Count == 0)
                return;
            while (running)
            {
                foreach (string classname in waigua_classname)
                {
                    IntPtr hwnd = WinAPI.FindWindow(classname, null);
                    if (hwnd.ToInt32() > 0)
                    {
                        int PID = 0;
                        WinAPI.GetWindowThreadProcessId(hwnd, out PID);
                        try
                        {
                            Process wg = Process.GetProcessById(PID);
                            string wgName = wg.ProcessName;
                            string wgTile = wg.MainWindowTitle;
                            try
                            {
                                wgName = wg.MainModule.FileName;
                                string exemd5 = AES.CretaeMD5(wgName);
                                if (ExceptMD5.Contains(exemd5))
                                    continue;
                                wgName = Path.GetFileName(wgName);
                                wg.Kill();
                            }
                            catch
                            {   //获取外挂程序的文件路径权限不足
                                if (wgTile.Contains("52pojie") || wgTile.Contains("Cheat"))
                                {
                                    wg.Kill();
                                    Thread.Sleep(1000);
                                }
                            }
                            WinAPI.SendMessage(hwnd, WinAPI.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                            MainForm.Instance.ClosAionGame();
                            MessageBox.Show(Language.getLang("发现疑是外挂") + wgName + Language.getLang("，踢出游戏！\r\n请不要使用第三方辅助或者外挂！\r\n发现3次以上使用将禁止登录！"), Config.Name,MessageBoxButtons.OK,MessageBoxIcon.Stop);
                        }
                        catch
                        {
                            WinAPI.SendMessage(hwnd, WinAPI.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        internal void CheckWGbyName()
        {
            if (waigua_name.Count == 0 && waigua_md5.Count == 0)
                return;
            while (running)
            {
                foreach (Process p in Process.GetProcesses())
                {
                    string pfilename = p.ProcessName;
                    try
                    {
                        pfilename = p.MainModule.FileName;
                        if (waigua_name.Contains(Path.GetFileName(pfilename).ToLower()) || waigua_md5.Contains(AES.CretaeMD5(pfilename)))
                        {
                            MainForm.Instance.ClosAionGame();
                            try
                            {
                                p.Kill();
                                MessageBox.Show(Language.getLang("发现外挂：") + Path.GetFileName(pfilename) + Language.getLang("，终止游戏！"));
                                Thread.Sleep(1000);
                                File.Delete(pfilename);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show(Language.getLang("外挂无法清除：") + Path.GetFileName(pfilename) + Language.getLang(" ,终止游戏！"));
                            }
                        }
                    }
                    catch
                    {
                        if (waigua_name.Contains(pfilename.ToLower()))
                        {
                            MainForm.Instance.ClosAionGame();
                            try
                            {
                                p.Kill();
                                MessageBox.Show(Language.getLang("发现外挂：") + pfilename + Language.getLang("，终止游戏！"));
                            }
                            catch (Exception)
                            {
                                MessageBox.Show(Language.getLang("外挂无法清除：") + pfilename + Language.getLang(" ,终止游戏！"));
                            }
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

    }
}

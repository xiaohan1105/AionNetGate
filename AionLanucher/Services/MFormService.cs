using AionLanucher.Configs;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher.Services
{
    class MFormService
    {
        internal static MFormService Instance = new MFormService();
        /// <summary>
        /// 检查到后关闭外挂
        /// </summary>
        internal static bool checkedAndClose = true;
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
        /// <summary>
        /// 特殊类外挂
        /// </summary>
        private Dictionary<string, string[]> classNames = new Dictionary<string, string[]>();
        /// <summary>
        /// 停止线程
        /// </summary>
        private bool wait = false;

        /// <summary>
        /// 初始化
        /// </summary>
        internal MFormService()
        {
            classNames.Add("SysTabControl32", new string[] { "Tab1" });
            classNames.Add("msctls_trackbar32", new string[] { "Slider1" });
            
            classNames.Add("_EL_Grid", new string[] { null });
            classNames.Add("_EL_Label", new string[] { null });
            classNames.Add("_EL_Timer", new string[] { "" });
            classNames.Add("Button", new string[] { "查找后锁空", "移动速度", "查找内存","试用", "解绑", "↑←→↓", "稳定功能", "连招加速", "技能无动作","攻击速度"});
            classNames.Add("LCLComboBox", new string[] { null });
            classNames.Add("Static", new string[] { "大红：", "大红：" , "攻速：", "移速：" });
            
        }
    
        
        internal void Start()
        {
            if (wait)
            {
                return;
            }
            else
            {
                if (waigua_md5.Count == 0 && waigua_classname.Count == 0)
                {
                    if (Config.CLIENT_WAIGUA != null && Config.CLIENT_WAIGUA.Length >0)//外挂数检查
                    {
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
                    }
                }
            }

            wait = true;
            ThreadPool.QueueUserWorkItem(new WaitCallback(CCN));
            //ThreadPool.QueueUserWorkItem(new WaitCallback(CWN));

        }

        internal void Stop()
        {
            wait = false;
        }


        /// <summary>
        /// 启动游戏后定时检查外挂
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CCN(object state)
        {
            if (waigua_classname.Count == 0)
                return;
            while (wait)
            {
                MainForm.checktime++;
                foreach (WindowInfo wi in  WinAPI.GetAllDesktopWindows())
                {
                    //遍历句柄
                    IntPtr hwnd = wi.hWnd;
                    //窗口类名
                    string cname = wi.szClassName;
                    //窗口名称
                    string wname = wi.szWindowName;


                    //检查到外挂名
                    string wgName;
                    //是否检查到外挂
                    bool isChecked = false;
                    //通过窗口类名查找进程
                    Process p = getProcessByHwnd(hwnd, wname, out wgName);
                    if (p != null)
                    {
                        try
                        {
                            string filePath = p.MainModule.FileName;
                            string md5 = Utilty.AES.CretaeMD5(filePath);
                            if (waigua_md5.Contains(md5))
                            {
                                isChecked = true;
                            }
                        }
                        catch
                        {

                        }
             
                    }

                    if (wname.Contains("52pojie") || wname.Contains("Cheat"))
                    {
                        isChecked = true;
                    }

                    if (waigua_classname.Contains(cname))
                    {
                        isChecked = true;
                    }
                    else
                    {
                        foreach (string c in classNames.Keys)
                        {
                            if (isChecked)
                                break;
                            foreach (string txt in classNames[c])
                            {
                                IntPtr buttonhwnd = WinAPI.FindWindowExX(hwnd, c, txt, true);
                                if (buttonhwnd != IntPtr.Zero)
                                {
                                    isChecked = true;
                                    break;
                                }
                            }
 
                        }

                    }

                    if (isChecked)
                    {
                        if (checkedAndClose)
                        {
                            try
                            {
                                if (p != null)
                                    p.Kill();
                                else
                                {
                                    WinAPI.PostMessage(hwnd, WinAPI.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                                    WinAPI.SendMessage(hwnd, WinAPI.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                                }
                            }
                            catch
                            {
                                WinAPI.PostMessage(hwnd, WinAPI.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                                WinAPI.SendMessage(hwnd, WinAPI.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                            }
                            finally
                            {
                                MainForm.Instance.ClosAionGameByWG(wgName);
                            }
                        }

                        isChecked = false;


                        //向网关发送检测到的信息
                        MainForm.Instance.sendPlayerUseWaigua(wgName, string.Format("程序名：{0} 窗口名：{1} 窗口类名：{2}", wgName, wname, cname));
                    }
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 根据句柄获取进程
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="windowName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Process getProcessByHwnd(IntPtr hwnd,string windowName, out string name)
        {
            try
            {
                int PID = 0;
                WinAPI.GetWindowThreadProcessId(hwnd, out PID);
                Process p =  Process.GetProcessById(PID);
                name = p.ProcessName;
                return p;
            }
            catch
            {
                
            }
            name = windowName;
            return null;
        }
    }
}

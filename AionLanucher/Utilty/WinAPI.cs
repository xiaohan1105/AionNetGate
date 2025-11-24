using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AionLanucher.Utilty
{
    class WinAPI
    {
        public struct ButtonInfo
        {
            public IntPtr Hwnd;
            public string Text;
            public int Width;
            public int Height;
        }

        public static List<WindowInfo> GetAllDesktopWindows()
        {
            List<WindowInfo> wndList = new List<WindowInfo>();
            EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                WindowInfo item = default(WindowInfo);
                StringBuilder stringBuilder = new StringBuilder(512);
                item.hWnd = hWnd;
                GetWindowTextW(hWnd, stringBuilder, stringBuilder.Capacity);
                item.szWindowName = stringBuilder.ToString();
                GetClassNameW(hWnd, stringBuilder, stringBuilder.Capacity);
                item.szClassName = stringBuilder.ToString();
                wndList.Add(item);
                return true;
            }, 0);
            return wndList;
        }
        /// <summary>
        /// 在父窗口里面通过搜出指定类名的控件
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="lpClassName"></param>
        /// <returns></returns>
        public static ButtonInfo[] GetAllButton(IntPtr hWndParent, string lpClassName)
        {
            List<ButtonInfo> wndList = new List<ButtonInfo>();
            EnumChildWindows(hWndParent, delegate(IntPtr hWnd, int lParam)
            {

                StringBuilder stringBuilder = new StringBuilder(512);
                GetClassNameW(hWnd, stringBuilder, stringBuilder.Capacity);
                if (!stringBuilder.ToString().Equals(lpClassName))
                    return true;

                ButtonInfo item = default(ButtonInfo);

                item.Hwnd = hWnd;
                GetWindowTextW(hWnd, stringBuilder, stringBuilder.Capacity);
                item.Text = stringBuilder.ToString();

                RECT r = new RECT();
                GetWindowRect(hWnd, ref r);
                item.Width = r.Right - r.Left;
                item.Height = r.Bottom - r.Top;
                wndList.Add(item);
                return true;
            }, 0);
            return wndList.ToArray();
        }

        public static WindowInfo[] GetAllChildControlsW(IntPtr phwnd)
        {
            List<WindowInfo> child = new List<WindowInfo>();
            EnumChildWindows(phwnd, delegate(IntPtr hWnd, int lParam)
            {
                WindowInfo item = default(WindowInfo);
                StringBuilder stringBuilder = new StringBuilder(512);
                item.hWnd = hWnd;
                GetWindowTextW(hWnd, stringBuilder, stringBuilder.Capacity);
                item.szWindowName = stringBuilder.ToString();
                GetClassNameW(hWnd, stringBuilder, stringBuilder.Capacity);
                item.szClassName = stringBuilder.ToString();
                child.Add(item);
                return true;
            }, 0);
            return child.ToArray();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; //最左坐标
            public int Top; //最上坐标
            public int Right; //最右坐标
            public int Bottom; //最下坐标
        }

        [DllImport("user32.dll")]
        public static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        public static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(CallBack lpEnumFunc, int lParam);

        public delegate bool CallBack(IntPtr hwnd, int lParam);
        /// <summary>
        /// 注意：回调函数的返回值将会影响到这个API函数的行为。
        /// 如果回调函数返回true，则枚举继续直到枚举完成；如果返回false，则将会中止枚举。
        /// 其中CallBack是这样的一个委托：public delegate bool CallBack(IntPtr hwnd, int lParam); 
        /// 如果 CallBack 返回的是true，则会继续枚举，否则就会终止枚举。
        /// </summary>
        /// <param name="hWndParent">父窗口句柄</param>
        /// <param name="lpEnumFunc">回调函数的地址</param>
        /// <param name="lParam">自定义的参数</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpEnumFunc, int lParam);



        //遍历找指定控件]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwnd">父窗口句柄</param>
        /// <param name="lpClassname">控件类名</param>
        /// <param name="lpszWindow">窗口标题</param>
        /// <param name="bChild">是否在子窗口查找</param>
        /// <returns></returns>
        public static IntPtr FindWindowExX(IntPtr hwnd, string lpClassname, string lpszWindow, bool bChild)
        {
            IntPtr iResult = IntPtr.Zero;
            // 首先在父窗体上查找控件
            iResult = FindWindowEx(hwnd, IntPtr.Zero, lpClassname, lpszWindow);
            // 如果找到直接返回控件句柄
            if (iResult != IntPtr.Zero)
                return iResult;

            // 如果设定了不在子窗体中查找
            if (!bChild)
                return iResult;

            // 枚举子窗体，查找控件句柄
            EnumChildWindows(hwnd, delegate(IntPtr h, int l)
            {
                IntPtr f1 = FindWindowEx(h, IntPtr.Zero, lpClassname, lpszWindow);
                if (f1 == IntPtr.Zero)
                    return true;
                else
                {
                    iResult = f1;
                    return false;
                }
            }, 0);
            // 返回查找结果
            return iResult;
        }




        [DllImport("User32.dll")]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 给指定窗口句柄指定显示动画
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="dwTime"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        internal static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        /// <summary>
        /// 设置指定句柄的窗口标题
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        internal static extern void SetWindowText(IntPtr hwnd, String s);
        /// <summary>
        /// 通过窗口句柄获取窗口标题
        /// </summary>
        /// <param name="IntPtr"></param>
        /// <param name="lpText"></param>
        /// <param name="nCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hwnd, StringBuilder lpText, int nCount);

        /// <summary>
        /// 通过窗口类名和标题获取 窗口句柄
        /// </summary>
        /// <param name="lpClassName">窗口类名</param>
        /// <param name="lpWindowName">窗口标题</param>
        /// <returns>窗口句柄</returns>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 通过主窗口句柄查找 子窗体句柄
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        /// <summary>
        /// 通过窗口句柄获取进程PID
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="ID">接收进程标识的32位值的地址。如果这个参数不为NULL，GetWindwThreadProcessld将进程标识拷贝到这个32位值中，否则不拷贝</param>
        /// <returns>返回值为创建窗口的线程标识</returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);



        //Windows消息值
        /// <summary>
        /// 关闭
        /// </summary>
        internal const uint WM_CLOSE = 0x10;
        /// <summary>
        /// 销毁
        /// </summary>
        internal const uint WM_DESTROY = 0x02;
        /// <summary>
        /// 退出
        /// </summary>
        internal const uint WM_QUIT = 0x12;



        /// <summary>
        /// 向指定窗口句柄 发送消息
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="Msg">消息</param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 向指定窗口句柄 发送消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 权限
        /// </summary>
        internal const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        internal const int PROCESS_VM_READ = 0x0010;
        internal const int PROCESS_VM_WRITE = 0x0020;

        private const int TH32CS_SNAPPROCESS=0x00000002;
        private const int TH32CS_SNAPMODULE = 0x00000008;


        [DllImport("KERNEL32.DLL")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, int processid);

        [DllImport("KERNEL32.DLL")]
        public static extern int CloseToolhelp32Snapshot(IntPtr handle);

        [DllImport("KERNEL32.DLL")]
        public static extern int Process32First(IntPtr handle, byte[] pe);



        [DllImport("Kernel32.dll", EntryPoint = "Module32First")]
        public static extern bool Module32First(IntPtr Handle, ref MODULEENTRY32 Me);

        [DllImport("Kernel32.dll", EntryPoint = "Module32Next")]
        public static extern bool Module32Next(IntPtr Handle, ref MODULEENTRY32 Me);


        /// <summary>
        /// 读取内存地址
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="lpBaseAddress">基址</param>
        /// <param name="lpBuffer"></param>
        /// <param name="nSize"></param>
        /// <param name="lpNumberOfBytesRead"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesRead);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwDesiredAccess">访问权限</param>
        /// <param name="bInheritHandle">继承标志</param>
        /// <param name="dwProcessId">进程ID</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, IntPtr dwProcessId);

        /// <summary>
        /// By: 史莱姆
        /// 获取模块地址
        /// </summary>
        /// <param name="PID">目标进程PID</param>
        /// <param name="ModuleName">需获取到的模块名</param>
        /// <returns>返回个int类型的吧.想怎么转换看你们自己了.</returns>
        internal static int GetModelAddress(int PID, string ModuleName)
        {
            PROCESSENTRY32 pr = new PROCESSENTRY32();
            MODULEENTRY32 mo = new MODULEENTRY32();
            IntPtr LM;
            if (ModuleName == "")
            {
                //如果模块空,直接88 返回-2 因为2..
                return -2;
            }
            pr.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
            LM = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, PID);
            if (LM.ToInt32() > 0)
            {
                mo.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));
                if (Module32First(LM, ref mo))
                {
                    do
                    {
                        if (mo.szModule == ModuleName)
                        {
                            //完成
                            CloseHandle(LM);
                            return mo.modBaseAddr.ToInt32();
                        }
                    }
                    while (Module32Next(LM, ref mo));
                }
                CloseHandle(LM);
            }
            //获取不到.或者遍历不到.都返回-1
            return -1;
        }


        public static int GetMemoryAddress(IntPtr PID, int[] BaseAddr)
        {
            int outint = 0;
            int Address = 0;
            byte[] Temp = new byte[4];
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, PID);
            for (int i = 0; i < BaseAddr.Length - 1; i++)
            {
                ReadProcessMemory(hProcess, (BaseAddr[i] + Address), Temp, 4, out outint);
                Address = BitConverter.ToInt32(Temp, 0);
            }
            CloseHandle(hProcess);
            return Address + BaseAddr[BaseAddr.Length - 1];
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [DllImport("kernel32.dll")] //声明API函数         
        public static extern int VirtualAllocEx(IntPtr hwnd, int lpaddress, int size, int type, int tect);

        [DllImport("kernel32.dll")]
        public static extern int WriteProcessMemory(IntPtr hwnd, int baseaddress, string buffer, int nsize, int filewriten);

        [DllImport("kernel32.dll")]
        public static extern int GetProcAddress(int hwnd, string lpname);

        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandleA(string name);

        [DllImport("kernel32.dll")]
        public static extern int CreateRemoteThread(IntPtr hwnd, int attrib, int size, int address, int par, int flags, int threadid);


        [DllImport("KERNEL32.DLL ")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);
        [DllImport("KERNEL32.DLL ")]
        public static extern int CloseHandle(IntPtr handle);




    }
}

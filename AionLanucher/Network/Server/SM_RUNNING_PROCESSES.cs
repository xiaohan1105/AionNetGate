using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AionLanucher.Network.Server
{
    /// <summary>
    /// 显示进程 - 灰色枫叶 - QQ93900604
    /// </summary>
    class SM_RUNNING_PROCESSES : AbstractServerPacket
    {
        private byte type;
        private int _pid;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="b">操作类型0刷新进程，1结束进程</param>
        /// <param name="pid">进程ID</param>
        public SM_RUNNING_PROCESSES(byte b, int pid)
        {
            this.type = b;
            this._pid = pid;
        }

        protected override void writeImpl()
        {
            writeC(type); //0刷新进程列表，1结束进程
            if (type == 0)
            {
                try
                {
                    Icon icon = myExtractIcon(Environment.SystemDirectory + "\\shell32.dll", 0x22);
                    Process[] processes = Process.GetProcesses();
                    writeH((short)processes.Length);
                    foreach (Process process in processes)
                    {
                        string str;
                        try
                        {
                            str = string.Format("{0}\t{1}\t{2} K\t{3}", process.MainModule.ModuleName, process.Id, process.PrivateMemorySize64 / 0x400L, process.MainModule.FileName);
                            icon = getIcon(process.MainModule.FileName, false);
                        }
                        catch (Exception)
                        {
                            str = string.Format("{0}\t{1}\t{2} K\t{3}", process.ProcessName, process.Id, process.PrivateMemorySize64 / 0x400L, "[System Process]");
                        }
                        try
                        {
                            byte[] bytes = getBytes(icon.ToBitmap());
                            if (bytes.Length > 0)
                            {
                                writeUH((ushort)bytes.Length);
                                writeB(bytes);
                            }
                            else
                            {
                                writeUH(0);
                            }
                            writeS(str);
                        }
                        catch (Exception e)
                        {
                            writeUH(0);
                            writeS(e.ToString());
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            else
            {
                string str2 = "[" + _pid + "]";
                string message = "进程结束成功!";
                try
                {
                    Process processById = Process.GetProcessById(_pid);
                    str2 = str2 + processById.ProcessName;
                    processById.Kill();
                    writeC(1);//指示进程结束成功
                }
                catch (Exception exception)
                {
                    message = exception.Message;
                    writeC(0);//指示进程结束失败
                }
                writeS(str2 + message);
            }


        }

        /// <summary>
        /// 图片转换成字节数组
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        private byte[] getBytes(Bitmap bit)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bit.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        #region WINAPI 图标导出
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;
            public string szDisplayName;
            public string szTypeName;
        }
        [DllImport("Shell32.dll")]
        private static extern int ExtractIcon(IntPtr h, string strx, int ii);
        [DllImport("shell32")]
        private static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);
        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        /// <summary>
        /// 从文件导出图标
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <param name="tf">大小图标</param>
        /// <returns></returns>
        private Icon getIcon(string FileName, bool tf)
        {
            Icon icon = null;
            try
            {
                SHFILEINFO psfi = new SHFILEINFO();
                int num = SHGetFileInfo(FileName, 0, ref psfi, 100, tf ? 0x4100 : 0x101);
                if (num > 0)
                    icon = Icon.FromHandle(psfi.hIcon);
            }
            catch (Exception) { }
            return icon;
        }
        /// <summary>
        /// 从文件中导出图标
        /// </summary>
        /// <param name="FileName">文件完整路径</param>
        /// <param name="iIndex">指定图标序号</param>
        /// <returns></returns>
        private Icon myExtractIcon(string FileName, int iIndex)
        {
            Icon icon = null;
            try
            {
                IntPtr handle = (IntPtr)ExtractIcon(IntPtr.Zero, FileName, iIndex);
                if (!handle.Equals(null))
                {
                    icon = Icon.FromHandle(handle);
                }
            }
            catch (Exception)
            {
                try
                {
                    Bitmap bit = new Bitmap(16, 16);
                    using (Graphics g = Graphics.FromImage(bit))
                    {
                        g.FillRectangle(Brushes.Black, 0, 0, bit.Width, bit.Height);
                        g.DrawString("E", SystemFonts.CaptionFont, Brushes.Red, 2, 2);
                        g.Flush();
                    }
                    IntPtr h = bit.GetHicon();
                    icon = Icon.FromHandle(h);
                    DeleteObject(h);// 释放IntPtr
                }
                catch
                {
                    icon = getIcon(new DirectoryInfo(Environment.SystemDirectory).Parent.FullName + "\\explorer.exe", false);
                }

            }
            return icon;
        }


        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        #endregion
    }
}

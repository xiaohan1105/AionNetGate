using AionNetGate.Configs;
using AionNetGate.Netwok;
using AionNetGate.Netwok.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate
{
    internal partial class DeskPictureForm : Form
    {
        private AionConnection con;
        private int decktop_hwnd;
        private Rectangle oldrect;
        private bool isFulled = false;

        private bool picture_auto = false;
        internal DeskPictureForm(ref AionConnection con)
        {
            InitializeComponent();
            this.con = con;
            this.Text = "玩家桌面[ " + con.getIP() + " ] 需要全屏可按F11";
        }

        #region WIN32
        public const int SW_SHOW = 5;
        public const int SW_HIDE = 0;
        public const int SPIF_UPDATEINIFILE = 0x1;
        public const int SPI_SETWORKAREA = 47;
        public const int SPI_GETWORKAREA = 48;

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern int SystemParametersInfo(int uAction, int uParam, ref Rectangle lpvParam, int fuWinIni);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);
        #endregion

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeskPictureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            picture_auto = false;
            // 修复：确保退出时恢复任务栏显示
            if (isFulled)
            {
                FullScreen(false);
            }
        }


        /// <summary>
        /// 图像模式菜单
        /// </summary>
        private ToolStripItemCollection imageModes;
        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeskPictureForm_Load(object sender, EventArgs e)
        {
            decktop_hwnd = FindWindow("Shell_TrayWnd", null);
            // 修复：检查任务栏窗口是否找到
            if (decktop_hwnd == 0)
            {
                MessageBox.Show("无法找到任务栏窗口，全屏功能可能无法正常工作", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            this.KeyPreview = true; // 允许窗体先收到键盘事件
            this.KeyUp += new KeyEventHandler(KeyEvent); // 指定键盘按下事件

            //给菜单加上事件响应
            foreach (object o in contextMenuStrip1.Items)
            {
                if (o is ToolStripMenuItem)
                {
                    ToolStripMenuItem t = (ToolStripMenuItem)o;
                    if (t.DropDownItems.Count > 0)
                    {
                        imageModes = t.DropDownItems;
                        foreach (ToolStripMenuItem ts in t.DropDownItems)
                        {
                            ts.Click += DeskPictureForm_Click;
                        }
                    }
                    else
                    {
                        t.Click += DeskPictureForm_Click;
                    }
                }
            }
            //发送截图请求
            con.SendPacket(new SM_PICTURE_INFO());
        }

        /// <summary>
        /// 键盘按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11) //F11
            {
                FullScreen(!isFulled);
            }
            else if (e.KeyCode == Keys.Escape)//“Esc” 按键退出全频
            {
                FullScreen(false);
            }
        }

        /// <summary>
        /// 菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeskPictureForm_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem ts = (ToolStripMenuItem)sender;
            if (imageModes.Contains(ts))
            {
                foreach (ToolStripMenuItem i in imageModes)
                    i.Checked = false;
            }

            switch (ts.Text)
            {
                case "自动":
                    ts.Checked = true;
                    pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                    break;
                case "拉伸":
                    ts.Checked = true;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;
                case "居中":
                    ts.Checked = true;
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                    break;
                case "缩放":
                    ts.Checked = true;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    break;
                case "正常":
                    ts.Checked = true;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                    break;
                case "手动刷新图像":
                    con.SendPacket(new SM_PICTURE_INFO());
                    break;
                case "自动刷新图像":
                    picture_auto = ts.Checked = !ts.Checked;
                    if (picture_auto)
                        con.SendPacket(new SM_PICTURE_INFO());
                    break;
                case "全屏显示（F11）":
                    FullScreen(true);
                    break;
            }
        }

        /// <summary>
        /// 全屏显示窗口
        /// </summary>
        /// <param name="full"></param>
        private void FullScreen(bool full)
        {
            if (decktop_hwnd <= 0)
                return;

            //隐藏或是显示任务栏
            ShowWindow(decktop_hwnd, full ? SW_HIDE : SW_SHOW);

            if (full)
            {
                //获取原始窗口大小 赋值给 oldrect ,以便下次还原
                SystemParametersInfo(SPI_GETWORKAREA, 0, ref oldrect, SPIF_UPDATEINIFILE);//get
                //设置窗口为屏幕大小
                Rectangle rectFull = Screen.PrimaryScreen.Bounds;
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref rectFull, SPIF_UPDATEINIFILE);//set

                isFulled = true;
            }
            else
            {
                //设置窗口为原始窗口大小
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref oldrect, SPIF_UPDATEINIFILE);
                isFulled = false;
            }

            FormBorderStyle = full ? FormBorderStyle.None : FormBorderStyle.Sizable;
            WindowState = full ? FormWindowState.Maximized : FormWindowState.Normal;//还原
        }


        internal void ShowImage(Image img)
        {
            if (this.IsDisposed || con == null || !con.isConnected)
                return;

            AionRoy.Invoke(pictureBox1, () =>
            {
                if (this.IsDisposed || pictureBox1.IsDisposed)
                    return;

                // 修复：释放旧图像资源，防止内存泄漏
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }
                pictureBox1.Image = img;
            });

            // 修复：确保自动刷新时有适当的延迟，避免频繁请求
            if (picture_auto && !this.IsDisposed && con != null && con.isConnected)
            {
                System.Threading.Timer autoRefreshTimer = null;
                autoRefreshTimer = new System.Threading.Timer(_ =>
                {
                    try
                    {
                        if (picture_auto && !this.IsDisposed && con != null && con.isConnected)
                        {
                            con.SendPacket(new SM_PICTURE_INFO());
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("自动刷新桌面图像失败: " + ex.Message);
                    }
                    finally
                    {
                        autoRefreshTimer?.Dispose();
                    }
                }, null, 1000, System.Threading.Timeout.Infinite); // 1秒延迟后发送下一次请求
            }
        }

    }
}

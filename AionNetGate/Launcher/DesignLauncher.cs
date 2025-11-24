using AionCommons.Unilty;
using AionCommons.WinForm;
using AionNetGate.Configs;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AionNetGate.Launcher
{
    public partial class DesignLauncher : Form
    {

        /// <summary>
        /// 反向生成文件
        /// </summary>
        private static bool outfile = false;
        /// <summary>
        /// 静态化本窗体
        /// </summary>
        public static DesignLauncher design;
        /// <summary>
        /// 自定义设置类
        /// </summary>
        public LauncherSetting ls;
        /// <summary>
        /// 生成登录器路径
        /// </summary>
        private string saveFile = "d:\\枫叶登录器.exe";
        /// <summary>
        /// 文件数组
        /// </summary>
        private static List<string> allfiles;//存放文件容器
        /// <summary>
        /// 是否已授权
        /// </summary>
        private bool isPromoted;
        /// <summary>
        /// 
        /// </summary>
        //private static List<DirectoryInfo> alldirectory;//存放文件夹容器


        public DesignLauncher()
        {
            InitializeComponent();
            DoubleBuffer.DoubleBufferedControl(panel_背景, true);
        }

        /// <summary>
        /// 遍历文件目录
        /// </summary>
        /// <param name="info"></param>
        private void ListFiles(FileSystemInfo info)
        {
            if (!info.Exists)
                return;
            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录 
            if (dir == null)
                return;

            //alldirectory.Add(dir); 现在不需要收集目录信息，所以屏蔽了

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (FileSystemInfo fsi in files)
            {
                FileInfo file = fsi as FileInfo;
                //是文件 
                if (file != null && file.Name.ToLower().EndsWith(".cs"))
                    allfiles.Add(file.FullName);
                //对于子目录，进行递归调用 
                else
                    ListFiles(fsi);
            }
        }

        #region 初始化窗体
        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignLauncher_Load(object sender, EventArgs e)
        {
            design = this;

            label_登录器名字.Text = Config.launcher_name;
            isPromoted = Config.isPromoted;

            PictureBox_Online.Image = ShowOnline(Color.GreenYellow);

            Lists(panel_背景.Controls);

            ls = new LauncherSetting();
            ls.登录器图标 = this.Icon;

            FlashLS();
        }


        /// <summary>
        /// 在线状态绘制
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>绘制制定原色的图像</returns>
        private Image ShowOnline(Color color)
        {
            Bitmap bmp = new Bitmap(12, 12);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(new SolidBrush(color), 0, 0, 12, 12);
            g.Dispose();
            return bmp;
        }


        /// <summary>
        /// 遍历控件
        /// 增加鼠标点击事件
        /// </summary>
        /// <param name="ctc"></param>
        private void Lists(Control.ControlCollection ctc)
        {
            foreach (Control ct in ctc)
            {
                if (ct is Panel
                    || ct is PictureBox
                    || ct is Label
                    || ct is ProgressBar
                    || ct is MButton)
                {

                    //ct.MouseClick += ChangeProset;
                    ct.MouseDown += new MouseEventHandler(this.cMouseDown);
                    ct.MouseMove += new MouseEventHandler(this.cMouseMove);
                    ct.MouseUp += new MouseEventHandler(this.cMouseUp);
                }
                //当窗体上的控件有子控件时，需要用递归的方法遍历，才能全部列出窗体上的控件
                if (ct.HasChildren)
                {
                    Lists(ct.Controls);
                }
            }
        }

        /// <summary>
        /// 给配置文件赋值
        /// </summary>
        public void FlashLS()
        {
            ls.登录器背景 = panel_背景.BackgroundImage;

            ls.关闭按钮大小 = mButton_关闭按钮.Size;
            ls.关闭按钮位置 = mButton_关闭按钮.Location;
            ls.关闭鼠标按下 = mButton_关闭按钮.DownImage;
            ls.关闭鼠标经过 = mButton_关闭按钮.MoveImage;
            ls.关闭鼠标离开 = mButton_关闭按钮.NormalImage;
            ls.关闭图片缩放 = mButton_关闭按钮.BackgroundImageLayout;

            ls.帐号管理按钮大小 = mButton_帐号管理.Size;
            ls.帐号管理按钮位置 = mButton_帐号管理.Location;
            ls.帐号按钮鼠标按下 = mButton_帐号管理.DownImage;
            ls.帐号按钮鼠标经过 = mButton_帐号管理.MoveImage;
            ls.帐号按钮鼠标离开 = mButton_帐号管理.NormalImage;
            ls.帐号管理图片缩放 = mButton_帐号管理.BackgroundImageLayout;

            ls.启动按钮大小 = mButton_启动游戏.Size;
            ls.启动按钮位置 = mButton_启动游戏.Location;
            ls.启动鼠标按下 = mButton_启动游戏.DownImage;
            ls.启动鼠标经过 = mButton_启动游戏.MoveImage;
            ls.启动鼠标离开 = mButton_启动游戏.NormalImage;
            ls.启动图片缩放 = mButton_启动游戏.BackgroundImageLayout;


            ls.状态灯位置 = PictureBox_Online.Location;
            ls.进度条大小 = skinProgressBar1.Size;
            ls.进度条位置 = skinProgressBar1.Location;
            ls.进度位置 = label_进度文字.Location;
            ls.名称位置 = label_登录器名字.Location;
            ls.区块背景 = panel区块.BackgroundImage;
            ls.区块大小 = panel区块.Size;
            ls.区块图片缩放 = panel区块.BackgroundImageLayout;
            ls.区块位置 = panel区块.Location;
            ls.速度位置 = label_速度.Location;
            ls.网页大小 = webBrowser1.Size;
            ls.网页位置 = webBrowser1.Location;
            ls.文本颜色 = label_登录器名字.ForeColor;
            ls.下载位置 = label_下载.Location;
            ls.信息位置 = label_信息.Location;
            ls.状态位置 = label_服务器状态.Location;

            propertyGrid1.SelectedObject = ls;
        }
        #endregion

        #region 拖动控件事件

        private bool IsMoving = false;
        private Point pCtrlLastCoordinate = new Point(0, 0);
        private Point pCursorLastCoordinate = new Point(0, 0);
        private Point pCursorOffset = new Point(0, 0);

        private void cMouseDown(object sender, MouseEventArgs e)
        {
            Control ctrl = (Control)sender;
            if (e.Button == MouseButtons.Left)
            {
                this.IsMoving = true;
                this.pCtrlLastCoordinate = ctrl.Location;
                this.pCursorLastCoordinate = Cursor.Position;
            }
        }

        private void cMouseMove(object sender, MouseEventArgs e)
        {
            Control ctrl = (Control)sender;
            if ((e.Button == MouseButtons.Left) && IsMoving)
            {
                Point point = Cursor.Position;
                pCursorOffset.X = point.X - pCursorLastCoordinate.X;
                pCursorOffset.Y = point.Y - pCursorLastCoordinate.Y;
                ctrl.Left = pCtrlLastCoordinate.X + pCursorOffset.X;
                ctrl.Top = pCtrlLastCoordinate.Y + pCursorOffset.Y;
            }
        }

        private void cMouseUp(object sender, MouseEventArgs e)
        {
            Control ctrl = (Control)sender;
            if (IsMoving && (pCursorOffset.X != 0 || pCursorOffset.Y != 0))
            {
                if (((pCtrlLastCoordinate.X + pCursorOffset.X) + ctrl.Width) > 0)
                {
                    ctrl.Left = pCtrlLastCoordinate.X + pCursorOffset.X;
                }
                else
                {
                    ctrl.Left = 0;
                }
                if (((pCtrlLastCoordinate.Y + pCursorOffset.Y) + ctrl.Height) > 0)
                {
                    ctrl.Top = pCtrlLastCoordinate.Y + pCursorOffset.Y;
                }
                else
                {
                    ctrl.Top = 0;
                }
                pCursorOffset = Point.Empty;
                FlashLS();
            }
        }
        #endregion

        #region 按钮事件
        /// <summary>
        /// 复选框显示网页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_显示网页_CheckedChanged(object sender, EventArgs e)
        {
            webBrowser1.Visible = checkBox_显示网页.Checked;
            if (webBrowser1.Visible)
            {
                webBrowser1.Navigate(Config.launcher_web_url);
                //propertyGrid1.SelectedObject = webBrowser1;
            }
        }

        /// <summary>
        /// 保存设计文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_保存设计_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "登录器设计文件(*.design)|*.design";
                saveFileDialog1.FileName = "枫叶自定义设计";

                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;

                string designSaveFile = saveFileDialog1.FileName;
                saveDesin(designSaveFile);
                MessageBox.Show("保存方案成功，下次你可以直接打开该方案", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        /// <summary>
        /// 打开设计文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button打开设计_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Filter = "登录器设计文件(*.design)|*.design";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.InitialDirectory = Application.StartupPath.ToString();
                openFileDialog1.RestoreDirectory = true;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
                loadDesign(openFileDialog1.FileName);
            }
        }
        /// <summary>
        /// 保存当前设计
        /// </summary>
        /// <param name="name"></param>
        private void saveDesin(string name)
        {
            File.Delete(name);
            using (ResourceWriter writer = new ResourceWriter(name))
            {
                writer.AddResource("创建快捷方式", ls.自动创建快捷方式);

                writer.AddResource("登录器背景", ls.登录器背景);
                writer.AddResource("登录器图标", ls.登录器图标);

                writer.AddResource("关闭按钮大小", ls.关闭按钮大小);
                writer.AddResource("关闭按钮位置", ls.关闭按钮位置);
                writer.AddResource("关闭鼠标按下", ls.关闭鼠标按下);
                writer.AddResource("关闭鼠标经过", ls.关闭鼠标经过);
                writer.AddResource("关闭鼠标离开", ls.关闭鼠标离开);
                writer.AddResource("关闭图片缩放", ls.关闭图片缩放);

                writer.AddResource("帐号按钮大小", ls.帐号管理按钮大小);
                writer.AddResource("帐号按钮位置", ls.帐号管理按钮位置);
                writer.AddResource("帐号鼠标按下", ls.帐号按钮鼠标按下);
                writer.AddResource("帐号鼠标经过", ls.帐号按钮鼠标经过);
                writer.AddResource("帐号鼠标离开", ls.帐号按钮鼠标离开);
                writer.AddResource("帐号图片缩放", ls.帐号管理图片缩放);
                writer.AddResource("帐号按钮名称", ls.帐号按钮文字名称);
                writer.AddResource("帐号名称位置", label_帐号管理.Location);

                writer.AddResource("进度条大小", ls.进度条大小);
                writer.AddResource("进度条位置", ls.进度条位置);
                writer.AddResource("进度位置", ls.进度位置);
                writer.AddResource("名称位置", ls.名称位置);

                writer.AddResource("启动按钮大小", ls.启动按钮大小);
                writer.AddResource("启动按钮位置", ls.启动按钮位置);
                writer.AddResource("启动鼠标按下", ls.启动鼠标按下);
                writer.AddResource("启动鼠标经过", ls.启动鼠标经过);
                writer.AddResource("启动鼠标离开", ls.启动鼠标离开);
                writer.AddResource("启动图片缩放", ls.启动图片缩放);

                writer.AddResource("区块背景", ls.区块背景);
                writer.AddResource("区块大小", ls.区块大小);
                writer.AddResource("区块图片缩放", ls.区块图片缩放);
                writer.AddResource("区块位置", ls.区块位置);
                writer.AddResource("速度位置", ls.速度位置);
                writer.AddResource("网页大小", ls.网页大小);

                writer.AddResource("网页位置", ls.网页位置);
                writer.AddResource("文本颜色", ls.文本颜色);
                writer.AddResource("下载位置", ls.下载位置);
                writer.AddResource("信息位置", ls.信息位置);
                writer.AddResource("状态灯位置", ls.状态灯位置);
                writer.AddResource("状态位置", ls.状态位置);

                writer.Generate();
                writer.Close();
                writer.Dispose();
            }
            ls.设计方案 = Path.GetFileNameWithoutExtension(name);
            propertyGrid1.SelectedObject = ls;
        }
        /// <summary>
        /// 加载设计
        /// </summary>
        /// <param name="name"></param>
        private void loadDesign(string name)
        {
            ResourceReader rr = new ResourceReader(name);
            //iterate through the reader, printing out the name-value pairs
            foreach (System.Collections.DictionaryEntry d in rr)
            {
                string key = d.Key.ToString();
                if (key == "登录器背景")
                    ls.登录器背景 = (Bitmap)d.Value;
                else if (key == "登录器图标")
                    ls.登录器图标 = (Icon)d.Value;

                else if (key == "关闭按钮大小")
                    ls.关闭按钮大小 = (Size)d.Value;
                else if (key == "关闭按钮位置")
                    ls.关闭按钮位置 = (Point)d.Value;
                else if (key == "关闭鼠标按下")
                    ls.关闭鼠标按下 = (Bitmap)d.Value;
                else if (key == "关闭鼠标经过")
                    ls.关闭鼠标经过 = (Bitmap)d.Value;
                else if (key == "关闭鼠标离开")
                    ls.关闭鼠标离开 = (Bitmap)d.Value;
                else if (key == "关闭图片缩放")
                    ls.关闭图片缩放 = (ImageLayout)d.Value;

                else if (key == "帐号按钮大小")
                    ls.帐号管理按钮大小 = (Size)d.Value;
                else if (key == "帐号按钮位置")
                    ls.帐号管理按钮位置 = (Point)d.Value;
                else if (key == "帐号鼠标按下")
                    ls.帐号按钮鼠标按下 = (Bitmap)d.Value;
                else if (key == "帐号鼠标经过")
                    ls.帐号按钮鼠标经过 = (Bitmap)d.Value;
                else if (key == "帐号鼠标离开")
                    ls.帐号按钮鼠标离开 = (Bitmap)d.Value;
                else if (key == "帐号图片缩放")
                    ls.帐号管理图片缩放 = (ImageLayout)d.Value;
                else if (key == "帐号按钮名称")
                    ls.帐号按钮文字名称 = (string)d.Value;
                else if (key == "帐号名称位置")
                    label_帐号管理.Location = (Point)d.Value;


                else if (key == "启动按钮大小")
                    ls.启动按钮大小 = (Size)d.Value;
                else if (key == "启动按钮位置")
                    ls.启动按钮位置 = (Point)d.Value;
                else if (key == "启动鼠标按下")
                    ls.启动鼠标按下 = (Bitmap)d.Value;
                else if (key == "启动鼠标经过")
                    ls.启动鼠标经过 = (Bitmap)d.Value;
                else if (key == "启动鼠标离开")
                    ls.启动鼠标离开 = (Bitmap)d.Value;
                else if (key == "启动图片缩放")
                    ls.启动图片缩放 = (ImageLayout)d.Value;

                else if (key == "进度条大小")
                    ls.进度条大小 = (Size)d.Value;
                else if (key == "进度条位置")
                    ls.进度条位置 = (Point)d.Value;
                else if (key == "进度位置")
                    ls.进度位置 = (Point)d.Value;
                else if (key == "名称位置")
                    ls.名称位置 = (Point)d.Value;

                else if (key == "区块背景")
                    ls.区块背景 = (Bitmap)d.Value;
                else if (key == "区块大小")
                    ls.区块大小 = (Size)d.Value;
                else if (key == "区块图片缩放")
                    ls.区块图片缩放 = (ImageLayout)d.Value;
                else if (key == "区块位置")
                    ls.区块位置 = (Point)d.Value;
                else if (key == "速度位置")
                    ls.速度位置 = (Point)d.Value;
                else if (key == "网页大小")
                    ls.网页大小 = (Size)d.Value;

                else if (key == "网页位置")
                    ls.网页位置 = (Point)d.Value;
                else if (key == "文本颜色")
                    ls.文本颜色 = (Color)d.Value;
                else if (key == "下载位置")
                    ls.下载位置 = (Point)d.Value;
                else if (key == "信息位置")
                    ls.信息位置 = (Point)d.Value;

                else if (key == "状态灯位置")
                    ls.状态灯位置 = (Point)d.Value;
                else if (key == "状态位置")
                    ls.状态位置 = (Point)d.Value;

                else if (key == "创建快捷方式")
                    ls.自动创建快捷方式 = (bool)d.Value;
            }
            //close the reader
            rr.Close();
            
            ls.设计方案 = Path.GetFileNameWithoutExtension(name);
            propertyGrid1.SelectedObject = ls;
        }

        #endregion

        #region 生成登录器按钮事件
        /// <summary>
        /// 生成登录器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinButton生成_Click(object sender, EventArgs e)
        {
            if (!isPromoted)
            {
                if (MessageBox.Show("你的网关为未授权的免费版本，无法生成带防挂系统和PC文件检查的登录器，并且登录器不支持双开，是否继续？", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "登录器名字(*.exe)|*.exe";
                saveFileDialog1.FileName = Config.launcher_name;

                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;

                saveFile = saveFileDialog1.FileName;
            }

            skinButton生成.Enabled = false;
            skinButton生成.Text = "正在生成,请稍等...";

            progressBar1.Visible = true;

            Thread t = new Thread(new ThreadStart(Release));
            t.IsBackground = true;
            t.Start();

        }

        /// <summary>
        /// 生成资源文件
        /// </summary>
        private void Release()
        {
            try
            {
                string resoucesFile = Environment.GetFolderPath(Environment.SpecialFolder.Templates) + "\\AionLanucher.Properties.Resources.resources";
                using (ResourceWriter writer = new ResourceWriter(resoucesFile))
                {
                    writer.AddResource("登录器", ls.登录器背景);
                    writer.AddResource("进度", ls.区块背景);
                    writer.AddResource("图标", ls.登录器图标);

                    writer.AddResource("服务器选择", AionNetGate.Properties.Resources.服务器选择);
                    writer.AddResource("账号管理背景", AionNetGate.Properties.Resources.账号管理背景);
                    writer.AddResource("绿色按钮常规", AionNetGate.Properties.Resources.绿色按钮常规);
                    writer.AddResource("绿色按钮经过", AionNetGate.Properties.Resources.绿色按钮经过);
                    writer.AddResource("绿色按钮按下", AionNetGate.Properties.Resources.绿色按钮按下);

                    writer.AddResource("关闭常规", ls.关闭鼠标离开);
                    writer.AddResource("关闭经过", ls.关闭鼠标经过);
                    writer.AddResource("关闭按下", ls.关闭鼠标按下);

                    writer.AddResource("启动常规", ls.启动鼠标离开);
                    writer.AddResource("启动按下", ls.启动鼠标按下);
                    writer.AddResource("启动经过", ls.启动鼠标经过);

                    writer.AddResource("账号常规", ls.帐号按钮鼠标离开);
                    writer.AddResource("账号经过", ls.帐号按钮鼠标经过);
                    writer.AddResource("账号按下", ls.帐号按钮鼠标按下);

                    writer.Generate();
                    writer.Close();
                    writer.Dispose();
                }

                //资源文件2号
                string resoucesFile1 = Environment.GetFolderPath(Environment.SpecialFolder.Templates) + "\\AionLanucher.MainForm.resources";

                using (ResourceWriter writer = new ResourceWriter(resoucesFile1))
                {
                    writer.AddResource("notifyIcon1.Icon", ls.登录器图标);
                    writer.AddResource("$this.Icon", ls.登录器图标);
                    writer.Generate();
                    writer.Close();
                    writer.Dispose();
                }

                string[] sources = new string[2] { resoucesFile, resoucesFile1 };

                string all = Decode(File.ReadAllText(Application.StartupPath + "\\data\\LData.dat"));


                string[] files = all.Split('★');
                List<string> Flist = new List<string>(files);

                foreach (string f in files)
                {
                    if (f.Contains("class Config"))
                    {
                        Flist.Remove(f);
                        Flist.Add(getConfigFile());
                        break;
                    }
                }
                foreach (string f in files)
                {
                    if (f.Contains("this.SkinSize = new System.Drawing.Size(1038, 704);"))
                    {
                        string newf = f.Replace("this.SkinSize = new System.Drawing.Size(1038, 704);", "this.SkinSize = new System.Drawing.Size(" + ls.登录器背景.Width + ", " + ls.登录器背景.Height + ");");
                        newf = newf.Replace("this.ClientSize = new System.Drawing.Size(1038, 704);", "this.ClientSize = new System.Drawing.Size(" + ls.登录器背景.Width + ", " + ls.登录器背景.Height + ");");
                        Flist.Remove(f);
                        Flist.Add(newf);
                        break;
                    }
                }


                Flist.CopyTo(files);
                Flist.Clear();
                Flist = null;

                string iconFile = Application.StartupPath + "\\Images\\icon.ico";
                if (ls.登录器图标 != null)
                {
                    FileStream ico = new FileStream(iconFile, FileMode.Create);
                    ls.登录器图标.Save(ico);
                    ico.Close();
                    ico.Dispose();
                }


                string tempname = Path.GetTempFileName();

                if (MakeExe(tempname, files, sources, iconFile))
                {
                    MessageBox.Show("生成[" + Config.launcher_name + ".exe" + "]成功", "恭喜", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AionRoy.Invoke(this, () =>
                    {
                        this.Close();
                    });
                }
                else
                {
                    AionRoy.Invoke(this, () =>
                    {
                        progressBar1.Visible = false;
                        skinButton生成.Text = "生成登录器";
                        skinButton生成.Enabled = true;
                    });
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("生成登录器遇到问题！" + ex.Message, "警告");
            }
        }

        #region 获取配置文件
        private string getConfigFile()
        {
            string file = null;
            {

                file = "" +
    "using System;" +
    "using System.Collections.Generic;" +
    "using System.Drawing;" +
    "using System.Text;" +
    "using System.Windows.Forms;" +
    "namespace AionLanucher.Configs" +
    "{" +
    "    class Config" +
    "    {" +
                    /// <summary>
                    /// 登录器名字
                    /// </summary>
    "        internal static string Name = \"" + Config.launcher_name + "\";\r\n" +
                    /// <summary>
                    /// 服务器IP地址
                    /// </summary>
    "        internal static string ServerIP = " + (Config.enable_two_ip ? "null" : ("\"" + Config.server_ip + "\"")) + ";\r\n" +
                    /// <summary>
                    /// 服务器IP地址
                    /// </summary>
    "        internal static string ServerIP_ONE = \"" + Config.server_ip + "\";\r\n" +
                    /// <summary>
                    /// 服务器IP地址
                    /// </summary>
    "        internal static string ServerIP_TWO = " +  (Config.enable_two_ip ? ("\""+ Config.server_second_ip +"\""): "null" )+ ";\r\n" +
                    /// <summary>
                    /// 服务器端口
                    /// </summary>
    "        internal static string ServerPort = \"" + Config.server_port + "\";\r\n" +
                    /// <summary>
                    /// LS登陆端口
                    /// </summary>
    "        internal static string LS_Port = \"" + Config.launcher_ls_port + "\";\r\n" +
                    /// <summary>
                    /// BIN32目录
                    /// </summary>
    "        internal static string bin32 = \"" + Config.launcher_bin32 + "\";\r\n" +
    /// <summary>
    /// BIN34目录
    /// </summary>
    "        internal static string bin64 = \"" + Config.launcher_bin64 + "\";\r\n" +

    /// <summary>
    /// 启动参数
    /// </summary>
    "        internal static string args = \"" + Config.launcher_args + "\";\r\n" +
                    /// <summary>
                    /// 登录器MD5
                    /// </summary>
     "       internal static string LauncherMD5 = \"\";\r\n" +
                    /// <summary>
                    /// 登录器更新地址
                    /// </summary>
    "        internal static string Launcher_Url = \"" + Config.launcher_update_url + "\";\r\n" +
                    /// <summary>
                    /// 补丁更新地址
                    /// </summary>
    "        internal static string Patch_Url = \"" + Config.launcher_patch_url + "\";\r\n" +
                    /// <summary>
                    /// 小主页地址
                    /// </summary>
    "        internal static string Web_Url = \"" + Config.launcher_web_url + "\";\r\n" +
                    /// <summary>
                    /// 外挂检查信息
                    /// </summary>
    "        internal static string[] CLIENT_WAIGUA = " + getSt(Config.launcher_waigua) + ";\r\n" +
                    /// <summary>
                    /// 客户端目录限制
                    /// </summary>
    "        internal static string[] CLIENT_FILES= " + getSt(Config.launcher_client_files) + ";\r\n" +
                    /// <summary>
                    /// 客户端文件MD5检查
                    /// </summary>
    "        internal static string[] CLIENT_FILES_MD5= " + getSt(Config.launcher_file_md5) + ";\r\n" +
                    /// <summary>
                    /// 是否可以双开
                    /// </summary>
    "        internal static bool CanDoubleStart = " + Config.launcher_double_start.ToString().ToLower() + ";\r\n" +
                    /// <summary>
                    /// 自动创建快捷方式
                    /// </summary>
    "        internal static bool AutoFastlink = " + ls.自动创建快捷方式.ToString().ToLower() + ";\r\n" +
                     /// <summary>
                    /// 启动时登陆
                    /// </summary>
    "        internal static bool not_login_at_start = true;\r\n" +
                    /// <summary>
                    /// 数据库类型
                    /// </summary>
    "        internal static bool UseMySQL = " + Config.isMysql.ToString().ToLower() + ";\r\n" +
                    /// <summary>
                    /// 新数据库结构
                    /// </summary>
    "        internal static bool NewDatabaseStructure = " + Config.newaccountdatabase.ToString().ToLower() + ";\r\n" +
                   /// <summary>
                   /// 账号按钮名称
                   /// </summary>
    "        internal static string AccountButtonName = \"" + ls.帐号按钮文字名称   +"\";\r\n" +
                    /// <summary>
                    /// 帐号管理按钮大小
                    /// </summary>
    "        internal static Size AccountButtonSize = new Size(" + ls.帐号管理按钮大小.Width + "," + ls.帐号管理按钮大小.Height + ");\r\n" +
                    /// <summary>
                    /// 帐号管理按钮位置
                    /// </summary>
    "        internal static Point AccountButtonLocation = new Point(" + ls.帐号管理按钮位置.X + "," + ls.帐号管理按钮位置.Y + ");\r\n" +
                    /// <summary>
                    /// 帐号管理按钮图像缩放
                    /// </summary>
    "        internal static ImageLayout AccountButtonLayout = ImageLayout." + ls.帐号管理图片缩放.ToString() + ";\r\n" +
                    /// <summary>
                    /// 登录器尺寸
                    /// </summary>
    "        internal static Size LauncherSize = new Size(" + ls.登录器背景.Width + "," + ls.登录器背景.Height + ");\r\n" +

                    /// <summary>
                    /// 文本颜色
                    /// </summary>
    "        internal static Color TextColor = Color.FromArgb(Convert.ToInt32(\"" + ls.文本颜色.ToArgb().ToString() + "\"));\r\n" +
                    /// <summary>
                    /// 状态灯位置
                    /// </summary>
    "        internal static Point StatLightLocation = new Point(" + ls.状态灯位置.X + "," + ls.状态灯位置.Y + ");\r\n" +
                    /// <summary>
                    /// 网页大小
                    /// </summary>
    "        internal static Size WebSize = new Size(" + ls.网页大小.Width + "," + ls.网页大小.Height + ");\r\n" +
                    /// <summary>
                    /// 网页位置
                    /// </summary>
    "        internal static Point WebLocation = new Point(" + ls.网页位置.X + "," + ls.网页位置.Y + ");\r\n" +

                     /// <summary>
                    /// 关闭按钮大小
                    /// </summary>
    "        internal static Size CloseButtonSize = new Size(" + ls.关闭按钮大小.Width + "," + ls.关闭按钮大小.Height + ");\r\n" +
                    /// <summary>
                    /// 关闭按钮位置
                    /// </summary>
    "        internal static Point CloseButtonLocation = new Point(" + ls.关闭按钮位置.X + "," + ls.关闭按钮位置.Y + ");\r\n" +
                    /// <summary>
                    /// 关闭按钮图像缩放
                    /// </summary>
    "        internal static ImageLayout CloseButtonLayout = ImageLayout." + ls.关闭图片缩放.ToString() + ";\r\n" +

            /// <summary>
                    /// 启动按钮大小
                    /// </summary>
    "        internal static Size StartButtonSize = new Size(" + ls.启动按钮大小.Width + "," + ls.启动按钮大小.Height + ");\r\n" +
                    /// <summary>
                    /// 启动按钮位置
                    /// </summary>
    "        internal static Point StartButtonLocation = new Point(" + ls.启动按钮位置.X + "," + ls.启动按钮位置.Y + ");\r\n" +
                    /// <summary>
                    /// 启动按钮图像缩放
                    /// </summary>
    "        internal static ImageLayout StartButtonLayout = ImageLayout." + ls.启动图片缩放.ToString() + ";\r\n" +

            /// <summary>
                    /// 进度条区块大小
                    /// </summary>
    "        internal static Size BackSize =  new Size(" + ls.区块大小.Width + "," + ls.区块大小.Height + ");\r\n" +
                    /// <summary>
                    /// 进度条区块位置
                    /// </summary>
    "        internal static Point BackLocation = new Point(" + ls.区块位置.X + "," + ls.区块位置.Y + ");\r\n" +
                    /// <summary>
                    /// 进度条区块图像缩放
                    /// </summary>
    "        internal static ImageLayout BackLayout = ImageLayout." + ls.区块图片缩放.ToString() + ";\r\n" +

            /// <summary>
                    /// 进度条大小
                    /// </summary>
    "        internal static Size ProcessBarSize = new Size(" + ls.进度条大小.Width + "," + ls.进度条大小.Height + ");\r\n" +
                    /// <summary>
                    /// 进度条位置
                    /// </summary>
    "        internal static Point ProcessBarLocation = new Point(" + ls.进度条位置.X + "," + ls.进度条位置.Y + ");\r\n" +

            /// <summary>
                    /// 文本“进度”位置
                    /// </summary>
    "        internal static Point TextJDLocation = new Point(" + ls.进度位置.X + "," + ls.进度位置.Y + ");\r\n" +
                    /// <summary>
                    /// 文本“登录器名”位置
                    /// </summary>
    "        internal static Point TextNameLocation = new Point(" + ls.名称位置.X + "," + ls.名称位置.Y + ");\r\n" +
                    /// <summary>
                    /// 文本“速度”位置
                    /// </summary>
    "        internal static Point TextSpeedLocation = new Point(" + ls.速度位置.X + "," + ls.速度位置.Y + ");\r\n" +
                    /// <summary>
                    /// 文本“下载”位置
                    /// </summary>
    "        internal static Point TextDownLocation = new Point(" + ls.下载位置.X + "," + ls.下载位置.Y + ");\r\n" +
                    /// <summary>
                    /// 文本“信息”位置
                    /// </summary>
    "        internal static Point TextInfoLocation = new Point(" + ls.信息位置.X + "," + ls.信息位置.Y + ");\r\n" +
                    /// <summary>
                    /// 文本“状态”位置
                    /// </summary>
    "        internal static Point TextStatLocation = new Point(" + ls.状态位置.X + "," + ls.状态位置.Y + ");\r\n" +
    "    }" +
    "}";
            }
            return file;
        }

        private string getSt(string[] str)
        {
            if (!isPromoted)
                return "null";
            if (str == null || str.Length == 0)
                return "null";

            string st = "new string[]{";
            int i = 0;
            foreach (string s in str)
            {
                string sl = s;
                if (s.Contains("\\"))
                    sl = s.Replace("\\", "\\\\");
                i++;
                if (i == str.Length)
                    st += "\"" + sl + "\"}";
                else
                    st += "\"" + sl + "\",";
            }
            return st;
        }
        #endregion

        #region 动态编译
        /// <summary>
        /// 动态编译
        /// </summary>
        /// <param name="exeName">生成EXE的文件名</param>
        /// <param name="fileLines">文件源代码数组</param>
        /// <returns>true成功，false失败</returns>
        private bool MakeExe(string exeName, string[] fileLines, string[] sourceFile, string ico_name)
        {
            if (!File.Exists(ico_name))
            {
                ico_name = Application.StartupPath + "\\images\\aion.ico";
            }
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();
            string[] sys = { 
            "System.DLL",
            "System.Data.DLL",
            "System.Drawing.DLL",
            "System.Windows.Forms.DLL",
            "System.ServiceProcess.DLL",
            "System.Management.DLL"};

            cp.ReferencedAssemblies.AddRange(sys);

            // Generate an executable instead of 
            // a class library.
            cp.GenerateExecutable = true;

            // Specify the assembly file name to generate.
            cp.OutputAssembly = exeName;

            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;

            cp.IncludeDebugInformation = false;

            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = false;

            cp.CompilerOptions = "/target:winexe /optimize /win32icon:\"" + ico_name + "\"";  //resource:" + sourceDir + "/Properties/Resources.resources  /win32manifest:./data/client/app.manifest


            if (provider.Supports(GeneratorSupport.Resources))
            {
                foreach (string s in sourceFile)
                    cp.EmbeddedResources.Add(s);
            }

            CompilerResults cr = provider.CompileAssemblyFromSource(cp, fileLines);

            string errorMessage;

            if (cr.Errors.Count > 0)
            {
                errorMessage = String.Format("无法将 {0} 代码生成 {1}\n", fileLines, cr.PathToAssembly);

                foreach (CompilerError ce in cr.Errors)
                {
                    errorMessage += String.Format(" {0}\n", ce.ToString());
                }
                MessageBox.Show("生成登录器失败，请联系网关开发者！" + errorMessage, "错误");


            }
            else
            {
                //存放用Reactor混淆后的临时文件
                // Random rnd = new Random();
                string tempname = Path.GetTempFileName();
                try
                {
                    Process myProcess = new Process();
                    myProcess.StartInfo.FileName = "cmd.exe";
                    myProcess.StartInfo.Arguments = "/c " + Application.StartupPath + "\\data\\Reactor.dat " + string.Format("-file \"{0}\" -project \"{1}\" -targetfile \"{2}\" -q", exeName, Application.StartupPath + "\\data\\aion.dat", tempname);
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    myProcess.Start();
                    myProcess.WaitForExit();
                    myProcess.Close();
                    myProcess.Dispose();
                    File.Delete(exeName);
                    File.Delete(tempname.Replace(".tmp", ".pdb"));
                    return ProtectExe(tempname);
                }
                catch (Exception)
                {
                    DeleteFiles(Path.GetDirectoryName(tempname));
                }

             //   File.Copy(exeName, saveFile, true);

            }
            return false;
        }

        /// <summary>
        /// TMD加壳保护
        /// </summary>
        /// <param name="exeName"></param>
        private bool ProtectExe(string exeName)
        {
            string tempname = string.Format("/protect \"{0}\" /inputfile \"{1}\" /outputfile \"{2}\"",
                Application.StartupPath + "\\data\\TMLicense.tmd",
                exeName,
                saveFile);
            try
            {
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = Application.StartupPath + "\\data\\TMLicense.dat";
                myProcess.StartInfo.Arguments =  tempname;
                myProcess.StartInfo.UseShellExecute = false;        //關閉Shell的使用
                myProcess.StartInfo.RedirectStandardInput = true;   //重定向標準輸入
                myProcess.StartInfo.RedirectStandardOutput = true;  //重定向標準輸出
                myProcess.StartInfo.RedirectStandardError = true;   //重定向錯誤輸出
                myProcess.StartInfo.CreateNoWindow = true;          //設置不顯示窗口
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.Start();
                myProcess.WaitForExit();

                if (myProcess.StandardOutput.ReadToEnd().Contains("Application successfully protected"))
                {
                    File.Delete(saveFile);
                    File.Copy(exeName, saveFile);
                    DeleteFiles(Path.GetDirectoryName(exeName));
                    return true;
                }
                else
                {
                    File.Delete(exeName);
                    File.Delete(exeName.Replace(".tmp", ".bak"));
                    File.Delete(exeName + ".log");
                    DeleteFiles(Path.GetDirectoryName(exeName));
                }
            }
            catch
            {
                DeleteFiles(Path.GetDirectoryName(exeName));
            }
            return false;
        }

        private void DeleteFiles(string path)
        {
            try
            {
                //删除这个目录下的所有文件
                if (Directory.GetFiles(path).Length > 0)
                {
                    foreach (string var in Directory.GetFiles(path))
                    {
                        try
                        {
                            File.Delete(var);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #endregion

        #region 生成登录器加密代码
        private void 生成加密登录器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "请选择登录器代码目录";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string foldPath = dialog.SelectedPath;


                    allfiles = new List<string>();
                    ListFiles(new DirectoryInfo(foldPath));//遍历目录下所有文件

                    StringBuilder sb = new StringBuilder();
                    foreach (string file in allfiles)
                    {
                        sb.Append(File.ReadAllText(file) + "★");
                    }

                    File.WriteAllText(Application.StartupPath + "\\data\\LData.dat", Encode(sb.ToString()));

                    MessageBox.Show(foldPath + "目录代码加密完成!", "成功提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion

        #region 加密和解密数据
        //密钥
        private byte[] arrDESKey = new byte[] { 62, 36, 93, 116, 178,224, 218, 132 };
        private byte[] arrDESIV = new byte[] { 95, 123, 246, 79, 36, 119, 167, 233 };

        /// <summary>
        /// 加密。
        /// </summary>
        /// <param name="m_Need_Encode_String"></param>
        /// <returns></returns>
        public string Encode(string m_Need_Encode_String)
        {
            if (m_Need_Encode_String == null)
            {
                throw new Exception("Error: \n源字符串为空！！");
            }
            try
            {
                DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
                MemoryStream objMemoryStream = new MemoryStream();
                CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateEncryptor(arrDESKey, arrDESIV), CryptoStreamMode.Write);
                StreamWriter objStreamWriter = new StreamWriter(objCryptoStream);
                objStreamWriter.Write(m_Need_Encode_String);
                objStreamWriter.Flush();
                objCryptoStream.FlushFinalBlock();
                objMemoryStream.Flush();
                return Convert.ToBase64String(objMemoryStream.GetBuffer(), 0, (int)objMemoryStream.Length);
            }
            catch (Exception)
            {

            }
            return "";
        }

        /// <summary>
        /// 解密。
        /// </summary>
        /// <param name="m_Need_Encode_String"></param>
        /// <returns></returns>
        public string Decode(string m_Need_Encode_String)
        {
            if (m_Need_Encode_String == null || m_Need_Encode_String == "")
            {
                return "";
            }
            try
            {
                DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
                byte[] arrInput = Convert.FromBase64String(m_Need_Encode_String);
                MemoryStream objMemoryStream = new MemoryStream(arrInput);
                CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateDecryptor(arrDESKey, arrDESIV), CryptoStreamMode.Read);
                StreamReader objStreamReader = new StreamReader(objCryptoStream);
                return objStreamReader.ReadToEnd();
            }
            catch (Exception)
            {

            }
            return "";
        }
        #endregion


        private void panel_背景_BackgroundImageChanged(object sender, EventArgs e)
        {
            Size oldsize = panel_背景.Size;
            int widthOffset = panel_背景.BackgroundImage.Size.Width - oldsize.Width;
            int heightOffset = panel_背景.BackgroundImage.Size.Height - oldsize.Height;

            panel_背景.Size = panel_背景.BackgroundImage.Size;

            skinSplitContainer1.Width += widthOffset;
            skinSplitContainer1.Height += heightOffset;
            skinSplitContainer1.Panel1MinSize += widthOffset;
            skinSplitContainer1.Panel2MinSize = 285;

            splitContainer2.Height += heightOffset;
            splitContainer2.Panel1MinSize += heightOffset;


            this.Width += widthOffset;
            this.Height += heightOffset;
            this.Update();
        }

        private void DesignLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            design = null;
            ls = null;
            allfiles = null;

        }

        private void 还原登录器文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!File.Exists(Application.StartupPath + "\\data\\LData.dat"))
            {
                MessageBox.Show("文件LData.dat不存在，请检查目录！");
                return;
            }
            string all = Decode(File.ReadAllText(Application.StartupPath + "\\data\\LData.dat"));

            string[] files = all.Split('★');
            foreach (string f in files)
            {
                if (outfile)
                {
                    if(f.Length < 1)
                    {
                        continue;
                    }
                    if (!f.Contains("namespace") && f.Contains("assembly"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + "\\launcherCodes\\AionLanucher\\Properties");
                        File.WriteAllText(Application.StartupPath + "\\launcherCodes\\AionLanucher\\Properties\\AssemblyInfo.cs", f);
                        continue;
                    }
                    else if(f.Contains("public struct WindowInfo"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + "\\launcherCodes\\AionLanucher\\Properties");
                        File.WriteAllText(Application.StartupPath + "\\launcherCodes\\AionLanucher\\Utilty\\WindowInfo.cs", f);
                        continue;
                    }



                    bool isDesin = false;
                    if(f.StartsWith("namespace "))
                    {
                        isDesin = true;
                    }


                    string[] outs = f.Split(new string[] { "namespace " }, StringSplitOptions.None);
                    string dir = outs[1].Substring(0, outs[1].IndexOf("\r"));

                    string saveDir = Application.StartupPath + "\\launcherCodes\\" + dir.Replace(".", "\\").Replace("{", "").Trim();

                    Directory.CreateDirectory(saveDir);

                    int index = outs[1].IndexOf("class ");
                    if (index == -1)
                    {
                        continue;
                    }

                    string news = outs[1].Substring(index + 6);
                    
                    news = news.Substring(0, news.IndexOf("\r\n"));
                    if (news.Contains(":"))
                    {
                        news = news.Substring(0, news.IndexOf(":")).Trim();
                    }

                    saveDir += "\\" + news + (isDesin? ".Designer." : "") + ".cs";
                    File.WriteAllText(saveDir, f);
                }
            }
            MessageBox.Show("登录器文件已经全部生成！");
        }
    }


    internal class WindowChange
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;                                                          //常量，隐藏
        private const int SW_SHOWNORMAL = 1;                                                    //常量，显示，标准状态
        private const int SW_SHOWMINIMIZED = 2;                                                 //常量，显示，最小化
        private const int SW_SHOWMAXIMIZED = 3;                                                 //常量，显示，最大化
        private const int SW_SHOWNOACTIVATE = 4;                                                //常量，显示，不激活
        private const int SW_RESTORE = 9;                                                       //常量，显示，回复原状
        private const int SW_SHOWDEFAULT = 10;                                                  //常量，显示，默认

        public static void ToChange(IntPtr p, bool isboolean)
        {
            if (isboolean)
            {
                ShowWindowAsync(p, SW_SHOWNORMAL);
            }
            else
            {
                ShowWindowAsync(p, SW_HIDE);
            }
        }
    }

}

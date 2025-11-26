using AionLanucher.Configs;
using AionLanucher.FormSkin;
using AionLanucher.Network;
using AionLanucher.Network.Server;
using AionLanucher.Services;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher
{
    public partial class MainForm : SkinMain
    {
        /// <summary>
        /// 静态化窗口
        /// </summary>
        internal static MainForm Instance;

        /// <summary>
        /// 主服务
        /// </summary>
        private MainService mainServer;

        /// <summary>
        /// 程序MD5自检
        /// </summary>
        private string LauncherMD5;

        /// <summary>
        /// 游戏根目录
        /// </summary>
        private string GameFoldPath;

        /// <summary>
        /// Aion游戏进程
        /// </summary>
        private Process AionProcess;//游戏进程

        /// <summary>
        /// 登录LS的端口转发器
        /// </summary>
        private PortMapService portmap;
        private PortMapService gsmap;

        /// <summary>
        /// 如果补丁里含有dbghelp.dll 这个WIN8补丁，那么单独赋值上去。如果登陆器检测到是WIN8系统，会自动更新它
        /// </summary>
        private AionFile dbghelp;

        /// <summary>
        /// 是否授权还是免费版本
        /// </summary>
        public static bool isPromote = false;

        /// <summary>
        /// 登陆转发加密
        /// </summary>
        public static string ls_port_password = "外";

        /// <summary>
        /// 下载端口
        /// </summary>
        public static ushort downPort = 444;

        /// <summary>
        /// 是否被禁止
        /// </summary>
        private static bool isbocked = false;

        /// <summary>
        /// 统计外挂循环检测次数
        /// </summary>
        public static long checktime = 0;


        //主要是判断是否被暂停了进程
        private long lastTime;

        //是否位64位系统，这用于启动客户端BIN64
        public static bool isX64 = false;


        /// <summary>
        /// 账号和角色地址内存偏移量，每个客户端偏移量不同
        /// </summary>
        private int accountIdAddress = 0xFE8DA0;
        private int playerIdAddress = 0xF9B664;
        //private int playerNameAddress = gameBase.ToInt32() + 0xF9BAA0;


        /// <summary>
        /// 构造函数
        /// </summary>
        public MainForm(string name)
        {
            InitializeComponent();
            Text = name;
            Instance = this;
            loadDesign();
            InitControl(button_Close);
            InitControl(button_StartGame);
            InitControl(button_Account);
            InitControl(panel_banner);
        }

        #region 窗体初始化
        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            notifyIcon1.Text = Config.Name;
            label_登录器名字.Text = Config.Name;

            if (Config.Web_Url != null && !Config.Web_Url.Equals(""))
            {
                webBrowser1.Visible = true;
                webBrowser1.Navigate(Config.Web_Url);
            }

            PictureBox_Online.Image = ShowOnline(Color.Red);

            //自动创建快捷方式
            if (Config.AutoFastlink)
                CreateVBS();
            try
            {
                //获取登录器MD5
                File.Copy(Application.ExecutablePath, Path.GetTempPath() + "\\TEMP102.tmp", true);
                LauncherMD5 = AES.CretaeMD5(Path.GetTempPath() + "\\TEMP102.tmp");
                File.Delete(Path.GetTempPath() + "\\TEMP102.tmp");
            }
            catch (Exception)
            {
                //获取登录器MD5
                File.Copy(Application.ExecutablePath, "C:\\TEMP102.tmp", true);
                LauncherMD5 = AES.CretaeMD5("C:\\TEMP102.tmp");
                try
                {
                    File.Delete("C:\\TEMP102.tmp");
                }
                catch
                {

                }
            }

            SelectLine();

            lastTime = currentTimeMillis();
            Thread t = new Thread(CheckStoped);
            t.IsBackground = true;
            t.Start();
        }

        private void CheckStoped()
        {
            while (true)
            {
                Thread.Sleep(10000);
                if (currentTimeMillis() - lastTime > 20000)
                {
                    //说明被暂停了进程
                    ClosAionGame();
                    OnDisconnectedServer(0);

                    mainServer.setStop = true;
                    isbocked = true;
                    break;
                }
                else
                {
                    lastTime = currentTimeMillis();
                }
            }
        }

        public long currentTimeMillis()
        {
            return (long)(DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
        }
        /// <summary>
        /// 服务器线路选择界面
        /// </summary>
        private void SelectLine()
        {
            //如果是单线这里不为空，所以现在是双线模式，首先要读取注册表信息。
            if (Config.ServerIP_TWO == null)
                Config.ServerIP = Config.ServerIP_ONE;

            if (Config.ServerIP == null)
            {
                Config.ServerIP = AionLanucher.Properties.Settings.Default.ServerIP;
                //如果读取注册表后还是空，那么弹出线路选择界面
                if (Config.ServerIP.Equals(""))
                {
                    ServerSelect ss = new ServerSelect();
                    if (ss.ShowDialog() != DialogResult.OK)
                        return;
                }
            }

            //开始连接到网关服务器
            mainServer = new MainService(Config.ServerIP, Config.ServerPort);
            mainServer.Start();
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

        #region 加载设计文件
        /// <summary>
        /// 加载配置
        /// </summary>
        private void loadDesign()
        {
            //尺寸
            if (Config.LauncherSize != Size.Empty)
            {
                MinimumSize = Config.LauncherSize;
                MaximumSize = Config.LauncherSize;
            }
            //状态灯
            if (Config.StatLightLocation != Point.Empty)
                PictureBox_Online.Location = Config.StatLightLocation;

            //小主页
            if (Config.WebSize != Size.Empty)
                webBrowser1.Size = Config.WebSize;
            if (Config.WebLocation != Point.Empty)
                webBrowser1.Location = Config.WebLocation;

            //关闭按钮
            if (Config.CloseButtonSize != Size.Empty)
                button_Close.Size = Config.CloseButtonSize;
            if (Config.CloseButtonLocation != Point.Empty)
                button_Close.Location = Config.CloseButtonLocation;
            if (Config.CloseButtonLayout != ImageLayout.Tile)
                button_Close.BackgroundImageLayout = Config.CloseButtonLayout;

            //启动游戏按钮
            if (Config.StartButtonSize != Size.Empty)
                button_StartGame.Size = Config.StartButtonSize;
            if (Config.StartButtonLocation != Point.Empty)
                button_StartGame.Location = Config.StartButtonLocation;
            if (Config.StartButtonLayout != ImageLayout.Tile)
                button_StartGame.BackgroundImageLayout = Config.StartButtonLayout;

            //帐号管理按钮
            if (Config.AccountButtonName != null)
                button_Account.Caption = Config.AccountButtonName;
            if (Config.AccountButtonSize != Size.Empty)
                button_Account.Size = Config.AccountButtonSize;
            if (Config.AccountButtonLocation != Point.Empty)
                button_Account.Location = Config.AccountButtonLocation;
            if (Config.AccountButtonLayout != ImageLayout.Tile)
                button_Account.BackgroundImageLayout = Config.AccountButtonLayout;

            //进度条区块
            if (Config.BackSize != Size.Empty)
                panel_banner.Size = Config.BackSize;
            if (Config.BackLocation != Point.Empty)
                panel_banner.Location = Config.BackLocation;
            if (Config.BackLayout != ImageLayout.Tile)
                panel_banner.BackgroundImageLayout = Config.BackLayout;

            //进度条
            if (Config.ProcessBarSize != Size.Empty)
                dualProgressBar1.Size = Config.ProcessBarSize;
            if (Config.ProcessBarLocation != Point.Empty)
                dualProgressBar1.Location = Config.ProcessBarLocation;

            //文字：进度 位置
            if (Config.TextJDLocation != Point.Empty)
                label1.Location = Config.TextJDLocation;
            //文件：文件名 位置
            if (Config.TextDownLocation != Point.Empty)
                Label_DownInfo.Location = Config.TextDownLocation;
            //文字：信息 位置
            if (Config.TextInfoLocation != Point.Empty)
                label_UpdateFileInfo.Location = Config.TextInfoLocation;
            //文字：登录器名 位置
            if (Config.TextNameLocation != Point.Empty)
                label_登录器名字.Location = Config.TextNameLocation;
            //文字：速度 位置
            if (Config.TextSpeedLocation != Point.Empty)
                label_Speed.Location = Config.TextSpeedLocation;
            //文字：服务器状态 位置
            if (Config.TextStatLocation != Point.Empty)
                label_ServerStat.Location = Config.TextStatLocation;

            if (Config.TextColor != Color.Empty)
            {
                Label_DownInfo.ForeColor = Config.TextColor;
                label_ServerStat.ForeColor = Config.TextColor;
                label_Speed.ForeColor = Config.TextColor;
                label_UpdateFileInfo.ForeColor = Config.TextColor;
                label_登录器名字.ForeColor = Config.TextColor;
                label1.ForeColor = Config.TextColor;
            }
        }
        #endregion

        #region 创建桌面快捷方式
        /// <summary>
        /// 创建快捷方式
        /// </summary>
        private void CreateVBS()
        {
            if (File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\" + Config.Name + ".lnk"))
                return;
            if (MessageBox.Show("是否需要为您创建[" + Config.Name + "]游戏的桌面快捷方式，以便您下次可以快速打开登陆器？\r\n\r\n注意：创建快捷方式可能会被360等安全软件拦截，请放心的选择允许！", Config.Name + "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Utilty.Shortcut.CreateDesktopShortcut(Application.ExecutablePath, Config.Name, null, Application.StartupPath);
            }
        }
        #endregion

        #endregion

        #region 按钮事件
        /// <summary>
        /// 关闭窗口按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Close_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(5000, Config.Name + "提醒", "双击图标可恢复登陆器界面！", ToolTipIcon.Info);
            notifyIcon1.BalloonTipTitle = Config.Name;
            WindowState = FormWindowState.Minimized;//隐藏
            AllHide();
        }
        /// <summary>
        /// 启动游戏按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_StartGame_Click(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            StartGame();
        }

        /// <summary>
        /// 账号管理按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Account_Click(object sender, EventArgs e)
        {
            Form f = new LoginForm(false, mainServer.getConnection());
            f.ShowDialog();
        }

        /// <summary>
        /// 状态栏小图标事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                AllShow();
                this.Focus();
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.notifyIcon1.ShowBalloonTip(5000, Config.Name + "提醒", "双击图标可恢复登陆器界面！", ToolTipIcon.Info);
                this.notifyIcon1.BalloonTipTitle = Config.Name;
                this.WindowState = FormWindowState.Minimized;//隐藏
                AllHide();
            }
        }
        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关于登录器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Config.Name + "\r\n钟丽通用游戏网关", Language.getLang("关于本登录器"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /// <summary>
        /// 关闭登录器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关闭登录器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClosAionGame();
            Environment.Exit(0);
        }

        /// <summary>
        /// 打开留言板窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 留言板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainServer == null || mainServer.getConnection() == null)
            {
                MessageBox.Show("请先连接到服务器", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取当前账号（如果已登录）
            string currentAccount = AionLanucher.Properties.Settings.Default.Account;
            if (string.IsNullOrEmpty(currentAccount))
            {
                currentAccount = "";
            }
            else if (currentAccount.Contains(":"))
            {
                string[] parts = currentAccount.Split(':');
                if (parts.Length > 1)
                {
                    currentAccount = parts[1].Trim();
                }
            }

            BulletinForm bulletinForm = new BulletinForm(mainServer.getConnection(), currentAccount);
            bulletinForm.ShowDialog();
        }
        #endregion

        #region 启动游戏线程和关闭客户端
        /// <summary>
        /// 启动游戏
        /// </summary>
        private void StartGame()
        {
            if (isbocked)
                return;
            if (button_StartGame.Enabled == false)
                return;

            if (GameFoldPath == null)
            {
                GameFoldPath = Application.StartupPath;
            }
            
            if (!File.Exists(GameFoldPath + "\\" + Config.bin32 + "\\aion.bin") && !File.Exists(GameFoldPath + "\\" + Config.bin64 + "\\aion.bin"))
            {
                if (MessageBox.Show(Language.getLang("检测到当前目录并非游戏根目录下！\r\n是否现在手动选择游戏目录？"), Config.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    dialog.Description = "请选择游戏的根目录";
                    if (dialog.ShowDialog() == DialogResult.OK)
                        GameFoldPath = dialog.SelectedPath;
                }
                else
                {
                    MessageBox.Show(Language.getLang("请退出登录器后复制本登录器到游戏目录再启动"), Config.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }


            AionLanucher.Properties.Settings.Default.Reset();
            AionLanucher.Properties.Settings.Default.GamePath = GameFoldPath;
            AionLanucher.Properties.Settings.Default.Save();


            if (!Config.not_login_at_start)
            {
                LoginForm f = new LoginForm(true, mainServer.getConnection());
                if (f.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            button_StartGame.Enabled = false;



            if (AionProcess != null && !AionProcess.HasExited)
            {
                MessageBox.Show(Language.getLang("客户端已经启动，请不要重复启动！"), Language.getLang("警告"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MFormService.Instance.Start();


            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadRunGame));

        }

        /// <summary>
        /// 线程启动游戏
        /// </summary>
        private void ThreadRunGame(object state)
        {
            CommonDownload();
            //WIN8
            //if ((Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2) || Environment.OSVersion.Version.Major > 6)
            //{
            //    if (dbghelp != null)
            //        DownloadFile(Config.Patch_Url, dbghelp);
            //}

            FileCheckService.Start();

            Random ran = new Random();
            int RandKey = ran.Next(19000, 35000);
            ushort locallsport = (ushort)RandKey;
            try
            {
                AionProcess = new Process();

                portmap = new PortMapService("127.0.0.1", locallsport.ToString(), Config.ServerIP, ushort.Parse(Config.LS_Port));
                portmap.Start();
                gsmap = new PortMapService("127.0.0.1", "7777", Config.ServerIP, 7777);
                gsmap.isCript = false;
                try
                {
                    gsmap.Start();
                }
                catch
                {

                }

                string Arguments = string.Format("-ip:127.0.0.1 -port:{0} {1} {2}", locallsport, Config.not_login_at_start ? "" : AionLanucher.Properties.Settings.Default.Account, Config.args);
                

                if (isX64 && File.Exists(GameFoldPath + "\\" + Config.bin64 + "\\aion.bin"))
                {
                    AionProcess.StartInfo.FileName = Path.Combine(GameFoldPath, Config.bin64 + "\\aion.bin");
                }
                else if(File.Exists(GameFoldPath + "\\" + Config.bin32 + "\\aion.bin"))
                {
                    AionProcess.StartInfo.FileName = Path.Combine(GameFoldPath, Config.bin32 + "\\aion.bin");
                }
                else
                {

                    return;
                }

             //   MessageBox.Show(AionProcess.StartInfo.FileName);

   
                AionProcess.StartInfo.Arguments = Arguments;
                AionProcess.StartInfo.UseShellExecute = false;
                AionProcess.StartInfo.CreateNoWindow = true;
                AionProcess.EnableRaisingEvents = true;
                AionProcess.Exited += AionProcess_Exited;
                AionProcess.Start();

            }
            catch (Exception error)
            {
                AionProcess = null;
                MessageBox.Show(Language.getLang("启动游戏失败：\r\n") + error.Message);
                return;
            }
            finally
            {
                RunWatchDog();
            }

            AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
            {
                notifyIcon1.ShowBalloonTip(2000, Config.Name + "提醒", "双击图标可恢复登陆器界面！", ToolTipIcon.Info);
                notifyIcon1.BalloonTipTitle = Config.Name;
                WindowState = FormWindowState.Minimized;//隐藏
                AllHide();
            }));


        }

        /// <summary>
        /// 游戏进程退出事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AionProcess_Exited(object sender, EventArgs e)
        {
            try
            {
                AionProcess.Exited -= AionProcess_Exited;
                AionProcess = null;

                if (portmap != null)
                    portmap.Stop();

                if (gsmap != null)
                    gsmap.Stop();

                if (watchdog != null && !watchdog.HasExited)
                {
                    watchdog.Kill();
                    watchdog = null;
                }


                MFormService.Instance.Stop();

                AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
                {
                    Thread.Sleep(2000);
                    AllShow();
                    this.Focus();
                    this.WindowState = FormWindowState.Normal;
                    button_StartGame.Enabled = true;
                }));
            }
            catch { }
        }


        /// <summary>
        /// 强制关闭客户端
        /// </summary>
        internal void ClosAionGame()
        {
            try
            {
                if (AionProcess != null && !AionProcess.HasExited)
                {
                    AionProcess.Kill();
                }

                AionProcess = null;

                Process[] process = Process.GetProcessesByName("aion.bin");
                foreach (Process p in process)
                {
                    p.Kill();
                }
                process = Process.GetProcessesByName("aion");
                foreach (Process p in process)
                {
                    p.Kill();
                }

                if (watchdog != null && !watchdog.HasExited)
                {
                    watchdog.Kill();
                    watchdog = null;
                }


                MFormService.Instance.Stop();
            }
            catch
            {

            }

        }

        private static Dictionary<string, string> waiguaInfos = new Dictionary<string, string>();

        /// <summary>
        /// 发送外挂信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        internal void sendPlayerUseWaigua(string name, string info)
        {
            if (waiguaInfos.ContainsKey(name))
            {
                return;
            }
            else
            {
                waiguaInfos.Add(name, info);

                string infos = "";
                int a = 0;
                foreach (string inf in waiguaInfos.Values)
                {
                    a++;
                    infos += "[" + a + "]" + inf + " | ";
                }

                SystemInfo si = new SystemInfo();
                if (mainServer != null && mainServer.getConnection() != null && mainServer.getConnection().ClientSocket != null && mainServer.getConnection().ClientSocket.Connected)
                    mainServer.getConnection().SendPacket(new SM_WAIGUA_INFO(accountId, playerId, si.getMNum(), si.GetMacAddress(), infos));
            }


        }


        private Thread getAccountThread;
        private volatile int accountId;  // 使用volatile保证线程可见性
        private volatile int playerId;
        private volatile bool stopAccountThread = false;  // 线程停止标志，替代Thread.Abort()
        private readonly object accountThreadLock = new object();  // 线程安全锁

        /// <summary>
        /// 第一次登陆时候获取账户ID并发送给网关，这个时候角色ID一般是获取不到的，因为玩家还没选择角色进入游戏世界
        /// </summary>
        internal void getAccountInfo()
        {
            int tempPlayerId;
            accountId = getAccountId(out tempPlayerId);
            playerId = tempPlayerId;

            if (accountId > 0)
            {
                SendAccountInfoToServer();
            }

            // 安全地停止旧线程
            StopAccountThread();

            // 创建新线程
            stopAccountThread = false;
            getAccountThread = new Thread(AccountThread);
            getAccountThread.IsBackground = true;
            getAccountThread.Start();
        }

        /// <summary>
        /// 安全停止账号线程
        /// </summary>
        private void StopAccountThread()
        {
            stopAccountThread = true;
            if (getAccountThread != null && getAccountThread.IsAlive)
            {
                // 等待线程自然结束，最多等待3秒
                if (!getAccountThread.Join(3000))
                {
                    // 如果线程仍未结束，记录警告但不强制中断
                    // Thread.Abort()已废弃，不再使用
                }
            }
        }

        /// <summary>
        /// 发送账号信息到服务器
        /// </summary>
        private void SendAccountInfoToServer()
        {
            try
            {
                // 缓存引用避免竞态条件
                var server = mainServer;
                if (server == null) return;

                var connection = server.getConnection();
                if (connection == null) return;

                var socket = connection.ClientSocket;
                if (socket == null || !socket.Connected) return;

                SystemInfo si = new SystemInfo();
                connection.SendPacket(new SM_WAIGUA_INFO(accountId, playerId, si.getMNum(), si.GetMacAddress(), ""));
            }
            catch (Exception)
            {
                // 忽略发送错误
            }
        }

        /// <summary>
        /// 线程执行 每1分钟一次
        /// </summary>
        internal void AccountThread()
        {
            while (!stopAccountThread)
            {
                try
                {
                    int pId;
                    int aId = getAccountId(out pId);

                    if (aId == 0 || stopAccountThread)
                        break;

                    if (aId > 0 && pId > 0 && playerId != pId)
                    {
                        accountId = aId;
                        playerId = pId;
                        SendAccountInfoToServer();

                        // 分段睡眠以便更快响应停止信号
                        for (int i = 0; i < 30 && !stopAccountThread; i++)
                        {
                            Thread.Sleep(10000);  // 5分钟 = 30 * 10秒
                        }
                    }

                    // 分段睡眠
                    for (int i = 0; i < 6 && !stopAccountThread; i++)
                    {
                        Thread.Sleep(10000);  // 1分钟 = 6 * 10秒
                    }
                }
                catch (Exception)
                {
                    // 忽略异常，继续循环
                    if (!stopAccountThread)
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
        }

        /// <summary>
        /// 获取账户ID - 修复内存地址累加错误
        /// </summary>
        private int getAccountId(out int pid)
        {
            int AccountId = 0;
            int PlayerId = 0;

            try
            {
                // 缓存进程引用避免竞态条件
                var process = AionProcess;
                if (process != null && !process.HasExited)
                {
                    IntPtr gameBase = IntPtr.Zero;
                    for (int i = 0; i < process.Modules.Count; i++)
                    {
                        string moduleName = process.Modules[i].ModuleName;
                        if (moduleName.ToLower().Equals("game.dll"))
                        {
                            gameBase = process.Modules[i].BaseAddress;
                            break;
                        }
                    }

                    if (gameBase != IntPtr.Zero)
                    {
                        // 修复：使用局部变量计算地址，不修改类成员变量
                        // 原代码每次调用都会累加，导致地址越来越大
                        int calcAccountAddr = accountIdAddress + gameBase.ToInt32();
                        int calcPlayerAddr = playerIdAddress + gameBase.ToInt32();

                        readMemoryData(process.Handle, calcAccountAddr, out AccountId);
                        readMemoryData(process.Handle, calcPlayerAddr, out PlayerId);
                    }
                }
            }
            catch (Exception)
            {
                // 进程可能已退出，忽略异常
            }

            pid = PlayerId;
            return AccountId;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="PID">进程句柄</param>
        /// <param name="baseAddress">内存地址</param>
        /// <param name="readData">读取的4字节数据</param>
        /// <returns></returns>
        private bool readMemoryData(IntPtr PID, int baseAddress, out int readData)
        {
            byte[] bs = new byte[4];
            int readSize;

            bool re = WinAPI.ReadProcessMemory(PID, baseAddress, bs, 4, out readSize);

            if (readSize > 0)
                readData = BitConverter.ToInt32(bs, 0);
            else
            {
                readData = -1;
            }
            return re;
        }




        /// <summary>
        /// 检查到外挂后关闭客户端
        /// </summary>
        /// <param name="msg"></param>
        internal void ClosAionGameByWG(string msg)
        {
            try
            {

                if (portmap != null)
                    portmap.Stop();
                if (gsmap != null)
                    gsmap.Stop();

                if (AionProcess != null && !AionProcess.HasExited)
                {
                    AionProcess.Kill();
                }
                AionProcess = null;

                Process[] process = Process.GetProcessesByName("aion.bin");
                foreach (Process p in process)
                {
                    p.Kill();
                }
                process = Process.GetProcessesByName("aion");
                foreach (Process p in process)
                {
                    p.Kill();
                }

                if (AionProcess != null && !AionProcess.HasExited)
                {
                    AionProcess.Kill();
                }
                AionProcess = null;

                if (watchdog != null && !watchdog.HasExited)
                {
                    watchdog.Kill();
                    watchdog = null;
                }

                //MFormService.Instance.Stop();

                MessageBox.Show(Language.getLang("发现疑是外挂") + msg + Language.getLang("，踢出游戏！\r\n请不要使用第三方辅助或者外挂！\r\n发现3次以上使用将禁止登录！"), Config.Name, MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }
            catch
            {

            }

        }

        #endregion

        #region 补丁更新和文件下载
        /// <summary>
        /// 单线程下载
        /// </summary>
        private void CommonDownload()
        {
            string updateFileUrl = Config.Patch_Url;//获取下载地址
            if (updateFileUrl == null || updateFileUrl == "")
                return;
            string UpdateFile = Path.GetFileName(updateFileUrl);//获取下载文件名

            WebRequest webReq = null;
            WebResponse webRes = null;
            try
            {
                webReq = WebRequest.Create(updateFileUrl);
                webRes = webReq.GetResponse();
            }
            catch (WebException)
            {
                MessageBox.Show(Language.getLang("无法连接到更新服务器！"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                label_UpdateFileInfo.Text = Language.getLang("正在检查远程更新文件,请稍后...");
            }));

            Stream srm = webRes.GetResponseStream();
            BinaryReader br = new BinaryReader(srm);

            int fileHead = br.ReadInt32();//获取文件头4字节里的数字
            byte[] TempBytes = br.ReadBytes(fileHead);//读取文件头信息，该文件所包含的所有文件信息
            string info = Encoding.UTF8.GetString(TempBytes);
            if (!info.Contains(";") || !info.Contains("|"))
            {
                MessageBox.Show(Language.getLang("读取更新文件失败，可能文件格式不正确！"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] FileInfo = info.Split(';');
            int firstLength = 4 + fileHead;
            //初始化更新文件数组数量
            List<AionFile> AFS = new List<AionFile>();

            for (int i = 0; i < FileInfo.Length; i++)
            {
                if (!FileInfo[i].Contains("|"))
                    continue;
                string[] infos = FileInfo[i].Split('|');

                AionFile af = new AionFile(infos[0], infos[1], int.Parse(infos[2]) + firstLength, int.Parse(infos[2]) + firstLength + int.Parse(infos[3]), (infos.Length == 5) ? int.Parse(infos[4]) : 0);

                if (File.Exists(GameFoldPath+ "\\" + af.fileName.Replace(".zip", "")))
                {
                    string md5 = AES.CretaeMD5(GameFoldPath + "\\" + af.fileName.Replace(".zip", ""));
                    if (md5.Equals(af.md5))
                        continue;
                }
                AFS.Add(af);
            }

            if (AFS != null && AFS.Count > 0)
            {
                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    Label_DownInfo.Text = Language.getLang("已检测到可更新文件") + AFS.Count + "个";
                    dualProgressBar1.MasterValue = 0;
                    dualProgressBar1.MasterMaximum = AFS.Count * 100;
                }));

                ServicePointManager.DefaultConnectionLimit = 30;//设置最大连接数|不然多线程下载慢
                foreach (AionFile af in AFS)
                {
                    DownloadFile(Config.Patch_Url, af);
                    AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                    {
                        dualProgressBar1.MasterValue += 100;
                        Label_DownInfo.Text = Language.getLang("更新文件数") + ":" + dualProgressBar1.MasterValue / 100 + " / " + AFS.Count;
                    }));
                }
            }
            else
            {
                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    label_UpdateFileInfo.Text = Language.getLang("当前没有可用更新");
                }));
            }
            // }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="URL">下载地址</param>
        /// <param name="af">AionFile</param>
        /// <param name="downinfo">显示信息文本</param>
        /// <param name="speedText"></param>
        private void DownloadFile(string URL, AionFile af)
        {
            try
            {
                HttpWebRequest Myrq = (HttpWebRequest)HttpWebRequest.Create(URL);
                Myrq.AddRange(af.start, af.end);//设置下载文件中文件数据位置
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;

                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    dualProgressBar1.Value = 0;
                    label_UpdateFileInfo.Text = Language.getLang("正在更新") + ":" + af.fileName;
                }));

                Stream st = myrp.GetResponseStream();//网络流

                string path = Path.GetDirectoryName(GameFoldPath + "\\" + af.fileName);
                Directory.CreateDirectory(path);
                Stream saveFile = new FileStream(GameFoldPath + "\\" + af.fileName + ".zip", FileMode.Create, FileAccess.Write);//下载保存文件

                DateTime starttime = DateTime.Now;
                long totalDownloadedByte = 0;
                byte[] by = new byte[102400];
                int osize = st.Read(by, 0, (int)by.Length);

                string sd = Language.getLang("速度");
                while (osize > 0)
                {
                    totalDownloadedByte += osize;

                    saveFile.Write(by, 0, osize);//写出文件流

                    osize = st.Read(by, 0, (int)by.Length);

                    double nowtime = (DateTime.Now - starttime).TotalSeconds;

                    AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                    {
                        double speeds = (totalDownloadedByte / nowtime);
                        if (speeds > (1024 * 1024))
                            label_Speed.Text = sd + ":" + (speeds / (1024 * 1024)).ToString("f2") + " M/s";
                        else if (speeds > 1024)
                            label_Speed.Text = sd + ":" + (speeds / 1024).ToString("f2") + " Kb/s";
                        else if (speeds > 0)
                            label_Speed.Text = sd + ":" + speeds.ToString("f2") + " b/s";
                        else
                            label_Speed.Text = sd + ":0 b/s";

                        dualProgressBar1.Value = (int)((float)totalDownloadedByte / (float)totalBytes * 100);//显示进度条  解压后大小
                    }));
                    Application.DoEvents();
                }
                if (saveFile != null)
                    saveFile.Close();

                //解压文件
                long finshedUpzip = 0;
                FileStream sf = new FileStream(GameFoldPath + "\\" + af.fileName + ".zip", FileMode.Open, FileAccess.Read);//下载保存文件
                FileStream unzipFile = new FileStream(GameFoldPath + "\\" + af.fileName, FileMode.Create, FileAccess.Write);//下载保存文件
                GZipStream zipStream = new GZipStream(sf, CompressionMode.Decompress, true);//解压ZIP流
                int upzip = 0;
                byte[] upbyte = new byte[1024000];
                while ((upzip = zipStream.Read(upbyte, 0, (int)upbyte.Length)) > 0)
                {
                    finshedUpzip += upzip;
                    unzipFile.Write(upbyte, 0, upzip);//写出文件流
                    AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                    {
                        label_UpdateFileInfo.Text = Language.getLang("正在解压") + ":" + af.fileName + Language.getLang("  已解压 ") + (((float)finshedUpzip / (float)af.or_length) * 100).ToString("f1") + " %";
                    }));
                    Application.DoEvents();
                }
                zipStream.Close();
                unzipFile.Close();
                sf.Close();
                File.Delete(GameFoldPath + "\\" + af.fileName + ".zip");
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message, Language.getLang("更新文件错误"));
            }
            finally
            {
                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    label_UpdateFileInfo.Text = Language.getLang("已完成更新");
                }));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <param name="filename">下载保存文件完整路径和名称</param>
        /// <param name="restart">是否自动更新启动（适合登陆器自动更新）</param>
        private void DownloadFile(string url, String filename, bool restart)
        {
            WebRequest webReq = null;
            WebResponse webRes = null;
            try
            {
                webReq = WebRequest.Create(url);
                webRes = webReq.GetResponse();

                long totalBytes = webRes.ContentLength;//下载文件大小

                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    label_UpdateFileInfo.Text = Language.getLang("正在更新：") + url.Split('/')[(url.Split('/').Length - 1)];
                    Label_DownInfo.Text = "0kb / " + totalBytes / 1024 + "kb";
                }));

                Stream st = webRes.GetResponseStream();
                string savefilename = (restart ? Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp") : filename);
                Stream so = new FileStream(savefilename, FileMode.Create);//下载保存的文件如果是登陆器EXE就加个后缀
                long totalDownloadedByte = 0;
                byte[] by = new byte[4096];
                int readDown = 0;
                while ((readDown = st.Read(by, 0, (int)by.Length)) > 0)
                {
                    //下载总字节数
                    totalDownloadedByte += readDown;
                    //进度条显示
                    AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                    {
                        dualProgressBar1.Value = (int)((double)totalDownloadedByte / (double)totalBytes * 100);
                        Label_DownInfo.Text = (int)(totalDownloadedByte / 1024) + "kb / " + (int)(totalBytes / 1024) + "kb";
                    }));
                    //把数据写入到文件
                    so.Write(by, 0, readDown);
                    Application.DoEvents();
                }
                so.Close();
                st.Close();

                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    dualProgressBar1.Value = dualProgressBar1.Maximum;
                    label_UpdateFileInfo.Text = Language.getLang("已完成下载");
                }));

                if (restart /* && MessageBox.Show("已完成下载，是否现在执行更新？", "灰色枫叶提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes */)
                {
                    File.WriteAllText(Application.StartupPath + "\\update.bat", string.Format(@"
                        @echo off
                        :selfkill
                        attrib -a -r -s -h ""{0}""
                        del ""{0}""
                        if exist ""{0}"" goto selfkill
                        ren ""{1}"" ""{0}""
                        ping 127.0.0.1 -n 2 >nul
                        start """" ""{0}""
                        del %0 ", Path.GetFileName(filename), Path.GetFileName(savefilename)), Encoding.GetEncoding("GB2312"));

                    // 启动自删除批处理文件
                    ProcessStartInfo info = new ProcessStartInfo(Application.StartupPath + "\\update.bat");
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(info);

                    ClosAionGame();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        #endregion

        #region 公开方法
        /// <summary>
        /// 准备连接到网关
        /// </summary>
        internal void BegainConnectServer()
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                button_StartGame.Enabled = false;
                button_Account.Enabled = false;
                PictureBox_Online.Image = ShowOnline(Color.Red);
                label_UpdateFileInfo.Text = "准备连接到服务器,请稍等...";
            }));
        }
        /// <summary>
        /// 连接网关失败
        /// </summary>
        internal void ConnectServerFail(int second)
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                label_UpdateFileInfo.Text = "连接服务器失败," + second + "秒后尝试重新连接...";
            }));
        }

        /// <summary>
        /// 已成功连接到网关
        /// </summary>
        internal void OnConnectedServer()
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                isbocked = false;
                if (!isPromote)
                {
                    label_登录器名字.Text = Config.Name + "（免费版）";
                }
                if (AionProcess == null)
                {
                    button_StartGame.Enabled = true;
                    button_Account.Enabled = true;
                    PictureBox_Online.Image = ShowOnline(Color.YellowGreen);
                    label_UpdateFileInfo.Text = "可以启动游戏";
                }
                else
                {
                    label_UpdateFileInfo.Text = Language.getLang("游戏已启动");
                }

                if (Config.LauncherMD5 != "" && !Config.LauncherMD5.Equals(LauncherMD5) && Config.LauncherMD5 != "请把登陆器拖到这里可生成MD5值")
                {
                    label_UpdateFileInfo.Text = Language.getLang("正在更新登录器,请稍等...");
                    //设置启动按钮不可用 
                    button_StartGame.Enabled = false;
                    button_Account.Enabled = false;
                    PictureBox_Online.Image = ShowOnline(Color.Red);
                    DownloadFile(Config.Launcher_Url, Application.ExecutablePath, true);
                }
            }));
        }
        /// <summary>
        /// 从已连接的网关上断开
        /// </summary>
        internal void OnDisconnectedServer(int second)
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                isbocked = true;
                button_StartGame.Enabled = false;
                button_Account.Enabled = false;
                PictureBox_Online.Image = ShowOnline(Color.Red);
                label_UpdateFileInfo.Text = "与服务器的连接被断开,"+second+"秒后尝试重新连接...";
            }));
        }


        internal void BlockServer()
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
            {
                isbocked = true;
                button_StartGame.Enabled = false;
                button_Account.Enabled = false;
                PictureBox_Online.Image = ShowOnline(Color.Red);
                label_UpdateFileInfo.Text = "准备连接到服务器,请稍等....";
            }));
        }

        #endregion

        #region 开门狗防止进程被结束
        private Process watchdog;
        private void RunWatchDog()
        {
            try
            {
                watchdog = new Process();
                watchdog.StartInfo.FileName = Application.ExecutablePath;
                watchdog.StartInfo.Arguments = "wc";
                watchdog.StartInfo.UseShellExecute = true;
                watchdog.StartInfo.CreateNoWindow = false;
                watchdog.EnableRaisingEvents = true;
                watchdog.Exited += watchdog_exit;
                watchdog.Start();
            }
            catch
            {

            }
        }

        private void watchdog_exit(object sender, EventArgs e)
        {
            try
            {
                AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
                {
                    关闭登录器ToolStripMenuItem.PerformClick();
                }));
                watchdog.Exited -= watchdog_exit;
            }
            catch
            {

            }
        }
        #endregion

        #region 清除线路选择
        private void 清除线路ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否要重置服务器线路选择？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                AionLanucher.Properties.Settings.Default.ServerIP = "";
                AionLanucher.Properties.Settings.Default.Save();
                MessageBox.Show("服务器线路选择已重置，请重启登录器！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion


        private bool is64BIN = true;
        private void 启动32位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            启动32位ToolStripMenuItem.Checked = !启动32位ToolStripMenuItem.Checked;
            启动64位ToolStripMenuItem.Checked = !启动32位ToolStripMenuItem.Checked;
            is64BIN = 启动64位ToolStripMenuItem.Checked;
        }

        private void 启动64位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            启动64位ToolStripMenuItem.Checked = !启动64位ToolStripMenuItem.Checked;
            启动32位ToolStripMenuItem.Checked = !启动64位ToolStripMenuItem.Checked;

            is64BIN = 启动64位ToolStripMenuItem.Checked;
        }
    }

    #region 通用委托
    class AionRoy
    {
        public delegate void Handler();
        /// <summary>
        /// 通用委托.NET2.0 方法： AionRoy.Invoke(this, new AionRoy.Handler(delegate(){    }));
        /// </summary>
        /// <param name="control">WINFORM：this</param>
        /// <param name="handler">委托</param>
        internal static void Invoke(System.Windows.Forms.Control control, Handler handler)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(handler);
            }
            else
            {
                handler();
            }
        }
    }
    #endregion
}

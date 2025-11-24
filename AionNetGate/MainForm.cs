using AionCommons.IPSanner;
using AionCommons.LogEngine;
using AionCommons.RegistryTool;
using AionCommons.Unilty;
using AionCommons.WinForm;
using AionNetGate.Configs;
using AionNetGate.Modles;
using AionNetGate.Netwok;
using AionNetGate.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AionNetGate.Logs;
using AionNetGate.Netwok.Server;

namespace AionNetGate
{
    public partial class MainForm : Form
    {
        private static readonly Logger log = LoggerFactory.getLogger();
        /// <summary>
        /// 注册表项
        /// </summary>
        private RegistryKey regKey;
        /// <summary>
        /// 静态化窗体
        /// </summary>
        public static MainForm Instance;
        /// <summary>
        /// 当前选中的用户连接
        /// </summary>
        private AionConnection selectedConnection;
        /// <summary>
        /// ListView数据
        /// </summary>
        private Dictionary<int, ListViewItem> clientListViewItems;
        /// <summary>
        /// 包含天，时，分，秒的数组
        /// </summary>
        private byte[] runtime = new byte[4] { 0, 0, 0, 0 };
        /// <summary>
        /// CPU使用率
        /// </summary>
        private PerformanceCounter pc;

        /// <summary>
        /// 统计军团线程
        /// </summary>
        private Thread leigonCountThreed;

        private TimeSpan prevCpuTime;

        private static LogHelper loginInfowriter ;
        private static LogHelper logwriter;

        private List<string> blockedHardInfos;
        private List<string> waiguaMaps;
        private bool isNotClose;
        private Process currutProcess;
        private long onlineTop;


        internal bool isRight = false;
        /// <summary>
        /// 构造方法
        /// </summary>
        public MainForm(string name)
        {
            InitializeComponent();
            Instance = this;
            Text = name;
            notifyIcon1.Text = name;

            waiguaMaps = new List<string>();
            blockedHardInfos = new List<string>();

            prevCpuTime = TimeSpan.Zero;
            onlineTop = 0L;
            isNotClose = true;

            currutProcess = Process.GetCurrentProcess();

            logwriter = new LogHelper(@"\logs\外挂记录.log");
            loginInfowriter = new LogHelper(@"\logs\登陆记录.log");
        }

        /// <summary>
        /// 窗体初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {



            pc = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            listView_online.ListViewItemSorter = new ListViewColumnSorter();
            listView_online.ColumnClick += new ColumnClickEventHandler(ListView_ColumnClick);

            DoubleBuffer.DoubleBufferedControl(tabControl2, true);
            DoubleBuffer.DoubleBufferedControl(listView_online, true);

            ReadConfigFromRegistry();
            SetValueToConfig();

            // 初始化新配置系统
            try
            {
                AionNetGate.Configs.LegacyConfigAdapter.Initialize();
                log.info("新配置系统初始化完成");
            }
            catch (Exception ex)
            {
                log.warn("配置系统初始化失败，使用默认配置: " + ex.Message);
            }


            RegForm.instance.readPromte();

            Thread t = new Thread(CheckUpdate);
            t.IsBackground = true;
            t.Start();

            RemoteH.RemoteMain.Start();

            if (checkBox_网关自动重启.Checked)
            {
                启动服务QToolStripMenuItem_Click(null, null);
            }

        }
        /// <summary>
        /// 从注册表中读取配置信息
        /// </summary>
        private void ReadConfigFromRegistry()
        {
            try
            {
                regKey = RegistryHelp.GetRegistryKey(@"software\AionRoy\AionNetGate");
                if (regKey == null)
                {
                    regKey = RegistryHelp.CreateRegistryKey(@"software\AionRoy\AionNetGate");
                }
                else
                {
                    if (regKey.GetValueNames().Length == 0)
                        return;
                    foreach (string key in regKey.GetValueNames())
                    {
                        object o = GetControlInstance(this, key);
                        if (o == null)
                            continue;

                        if (key.Contains("textBox"))
                        {
                            TextBox tb = (TextBox)o;
                            object v = regKey.GetValue(key,"");
                            if(v is string[])
                                tb.Lines = (string[])v;
                            else
                                tb.Text = (string)v;
                        }
                        else if (key.Contains("checkBox"))
                        {
                            CheckBox cb = (CheckBox)o;
                            cb.Checked = bool.Parse(regKey.GetValue(key,"False").ToString());
                        }
                        else if (key.Contains("radioButton"))
                        {
                            RadioButton cb = (RadioButton)o;
                            cb.Checked = bool.Parse(regKey.GetValue(key, "False").ToString());
                        }
                        else if (key.Contains("comboBox"))
                        {
                            ComboBox cb = (ComboBox)o;
                            cb.Text = regKey.GetValue(key, "False").ToString();
                        }
                        else if (key.Contains("waiguaMaps"))
                        {
                            object obj4 = this.regKey.GetValue(key, "");
                            waiguaMaps.Clear();
                            waiguaMaps.AddRange((string[])obj4);
                        }
                        else if (key.Contains("blockedHardInfos"))
                        {
                            object obj5 = this.regKey.GetValue(key, "");
                            blockedHardInfos.Clear();
                            blockedHardInfos.AddRange((string[])obj5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 根据指定容器和控件名字，获得控件
        /// </summary>
        /// <param name="obj">容器</param>
        /// <param name="strControlName">控件名字</param>
        /// <returns>控件</returns>
        private object GetControlInstance(object obj, string strControlName)
        {
            System.Collections.IEnumerator Controls = null;//所有控件
            Control c = null;//当前控件
            object cResult = null;//查找结果
            if (obj.GetType() == this.GetType())//窗体
            {
                Controls = this.Controls.GetEnumerator();
            }
            else//控件
            {
                Controls = ((Control)obj).Controls.GetEnumerator();
            }
            while (Controls.MoveNext())//遍历操作
            {
                c = (Control)Controls.Current;//当前控件
                if (c.HasChildren)//当前控件是个容器
                {
                    cResult = GetControlInstance(c, strControlName);//递归查找
                    if (cResult == null)//当前容器中没有，跳出，继续查找
                        continue;
                    else//找到控件，返回
                        return cResult;
                }
                else if (c.Name == strControlName)//不是容器，同时找到控件，返回
                {
                    return c;
                }
            }
            return null;//控件不存在
        }

        /// <summary>
        /// 将控件的设置赋值到Config
        /// </summary>
        private void SetValueToConfig()
        {
            Config.server_ip = textBox网关IP.Text;
            Config.enable_two_ip = checkBox开启双线双IP.Checked;
            if (Config.enable_two_ip)
                Config.server_second_ip = textBox网关IP2.Text;

            Config.server_port = textBox网关端口.Text;
            Config.can_auto_start_netgate = checkBox_网关自动重启.Checked;
            Config.netgate_autostart_time = ushort.Parse(textBox网关重启时间.Text);
            Config.can_auto_start_portmap = checkBox_转发器自动重启.Checked;
            Config.portmap_autostart_time = ushort.Parse(textBox转发重启时间.Text);
            Config.can_auto_ban_ip = checkBox_检查到攻击自动封IP.Checked;
            Config.enable_socket_log = checkBox开启通讯日志显示.Checked;

            Config.image_compress_rate = byte.Parse(textBox图像压缩率.Text);
            Config.image_width = ushort.Parse(textBox分块图片长度.Text);
            Config.image_height = ushort.Parse(textBox分块图片宽度.Text);

            Config.send_email = textBox发送邮箱.Text;
            Config.send_email_password = textBox邮箱密码.Text;
            Config.send_stmp_address = textBox邮箱STMP地址.Text;
            Config.send_stmp_port = textBox邮箱STMP端口.Text;

            Config.mysql_url     = textBox_mysqlurl.Text;
            Config.mysql_port    = textBox_mysqlport.Text;
            Config.mysql_user    = textBox_mysqluser.Text;
            Config.mysql_psw     = textBox_mysqlpassword.Text;
            Config.mysql_db_gs   = textBox_mysql_db_gs.Text;
            Config.mysql_db_ls   = textBox_mysql_db_ls.Text;
            Config.mysql_code    = textBox_mysql_code.Text;
            Config.isMysql = radioButton_MySQL.Checked; 


            Config.launcher_update_url   = textBox_updateUrl.Text;
            Config.launcher_md5          = textBox_launcherMD5.Text;
            Config.launcher_patch_url    = textBox_patch_url.Text;
            Config.launcher_ls_port      = textBox账号服务器LS端口.Text;
            Config.launcher_name         = textBox登录器名字.Text;
            Config.launcher_bin32        = textBox登录器BIN32.Text;
            Config.launcher_bin64 = textBox登录器BIN64.Text;

            Config.launcher_web_url      = textBox登录器WEB.Text;
            Config.launcher_args         = textBox登录器启动参数.Text;
            Config.launcher_client_files = textBox目录限制.Lines;
            Config.launcher_file_md5     = textBox文件MD5.Lines;
            Config.launcher_waigua       = textBox外挂进程.Lines;
            Config.launcher_double_start = checkBox登录器可双开.Checked;

            Config.leigonStartTime = comboBox_开始统计.Text;
            Config.leigonEndTime = comboBox_结束统计.Text;
            Config.leigonWaitTime = int.Parse(textBox_统计间隔.Text);
            Config.leigonCountAVG = int.Parse(textBox_平均统计几次.Text);

            Config.close_login_at_start = checkBox禁止提前登录.Checked;
            Config.port_password = textBox转发密码.Text;
            Config.down_port = ushort.Parse(textBox下载端口.Text);
            Config.disable_mmzh = checkBox_close_mmzh.Checked;

        }


        /// <summary>
        /// 记录外挂
        /// </summary>
        /// <param name="li"></param>
        /// <param name="mac"></param>
        /// <param name="info"></param>
        internal void recodeWG(LauncherInfo li, string mac, string info)
        {
            try
            {
                if (info.Length < 1)
                {
                    ListViewItem lv = li.listViewItem;
                    AionRoy.Invoke(listView_online, delegate {
                        lv.SubItems[8].Text = li.AccountId.ToString();
                        lv.SubItems[9].Text = li.PlayerId.ToString();
                        listView_online.Refresh();
                    });
                    loginInfowriter.WriteLine(string.Format("账户ID：{0} 角色ID：{1}  硬件识别码：{2} 网卡MAC：{3} 成功登陆了游戏!", li.AccountId, li.PlayerId, li.HardInfo, mac));
                }
                else
                {
                    logwriter.WriteLine(string.Format("账户ID：{0} 角色ID：{1}  硬件识别码：{2} 网卡MAC：{3}  {4}", li.AccountId, li.PlayerId, li.HardInfo, mac, info));
                    if (!waiguaMaps.Contains(li.HardInfo))
                    {
                        waiguaMaps.Add(li.HardInfo);
                        ListViewItem lv = li.listViewItem;
                        AionRoy.Invoke(this.listView_online, delegate {
                            lv.SubItems[7].Text = "外挂行为";
                            lv.SubItems[8].Text = li.AccountId.ToString();
                            lv.SubItems[9].Text = li.PlayerId.ToString();
                            listView_online.Refresh();
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                log.warn("保存外挂行为数据失败", System.Drawing.Color.Tomato, exception.ToString());
            }
        }

        internal void AddClientToList(LauncherInfo launcher)
        {
            // 确保在UI线程中执行
            if (InvokeRequired)
            {
                BeginInvoke(new Action<LauncherInfo>(AddClientToList), launcher);
                return;
            }

            try
            {
                object[] objArray1 = new object[] { launcher.Connection.getIP(), "[", launcher.Connection.GetHashCode(), "]" };
                ListViewItem lv = new ListViewItem(string.Concat(objArray1), 0)
                {
                    Tag = launcher.Connection
                };

                // 添加位置信息
                lv.SubItems.Add(launcher.Connection.getLoction());

                // 添加客户端信息（版本、计算机名、硬件ID等）
                if (launcher.Info != null)
                {
                    lv.SubItems.AddRange(launcher.Info);
                }
                else
                {
                    // 如果info为空，添加默认值
                    lv.SubItems.Add("未知版本");
                    lv.SubItems.Add("未知计算机");
                    lv.SubItems.Add("未知硬件ID");
                }

                // 添加登录状态（是否被封禁）
                lv.SubItems.Add(blockedHardInfos.Contains(launcher.HardInfo) ? "0" : "1");

                // 添加外挂检测状态
                lv.SubItems.Add(waiguaMaps.Contains(launcher.HardInfo) ? "外挂行为" : "正常");

                // 添加账号ID和角色ID
                lv.SubItems.Add(launcher.AccountId.ToString());
                lv.SubItems.Add(launcher.PlayerId.ToString());

                // 设置ListView项到LauncherInfo
                launcher.listViewItem = lv;

                // 发送后续配置包
                if (blockedHardInfos.Contains(launcher.HardInfo))
                {
                    launcher.Send(new SM_COMPUTER_INFO(2));
                }
                else
                {
                    // 发送外挂检测配置
                    launcher.Send(new SM_WAIGUA_INFO(textBox外挂进程.Lines, checkBox查到外挂关闭.Checked));
                    // 开始ping检测
                    launcher.Connection.CheckPingTime();
                }

                // 添加到ListView
                listView_online.Items.Add(lv);

                // 更新在线人数显示
                state_online.Text = listView_online.Items.Count.ToString();

                // 记录连接成功日志
                log.info("客户端[{0}]({1})连接成功，当前共有{2}人在线",
                    launcher.Connection.getIP(),
                    launcher.Connection.getLoction(),
                    listView_online.Items.Count,
                    Color.Green);
            }
            catch (Exception ex)
            {
                log.error("添加客户端到列表失败: {0}", ex.Message);
            }
        }


        #region 菜单事件
        private void 导入配置IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void 导出配置OToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 退出程序EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopServer();
            isNotClose = false;
            Application.Exit();
        }

        private void 启动服务QToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void 停止服务TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopServer();
        }


        #endregion

        /// <summary>
        /// 启动服务
        /// </summary>
        private void StartServer()
        {
            timer_checkOnline.Start();

            timerRestart.Interval = (int.Parse(this.textBox网关重启时间.Text) * 60) * 0x3e8;
            timerRestart.Start();


            runtime = new byte[4] { 0, 0, 0, 0 };
            启动服务QToolStripMenuItem.Enabled = false;
            停止服务TToolStripMenuItem.Enabled = true;
            richTextBox_log.Clear();
            listView_online.Items.Clear(); ;

            clientListViewItems = new Dictionary<int, ListViewItem>();

            LoggerFactory.Inistall(ref richTextBox_log);

            log.info("初始化日志服务成功.");

            MainService.Instance.Start();
        //    DownFileServer.Instance.Start();

            state_stat.Text = "已启动";
            state_stat.ForeColor = Color.Green;

            timer_main.Enabled = checkBox_网关自动重启.Checked;

        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private void StopServer()
        {
            timer_checkOnline.Stop();
            timerRestart.Stop();

            启动服务QToolStripMenuItem.Enabled = true;
            停止服务TToolStripMenuItem.Enabled = false;

            MainService.Instance.Stop();
            DownFileServer.Instance.Stop();

            log.info("==============================================",Color.OrangeRed);
            log.info("==================服务已停止==================", Color.OrangeRed);

            state_stat.Text = "已停止";
            state_stat.ForeColor = Color.Red;
            timer_main.Enabled = false;

            state_online.Text = "0";

            regKey.SetValue("waiguaMaps", waiguaMaps.ToArray());
        }


        /// <summary>
        /// 关闭时候最小化到托盘
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = isNotClose;

            if (isNotClose)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                this.notifyIcon1.ShowBalloonTip(5000, this.Text, "已切换到后台运行，如需恢复请双击图标！", ToolTipIcon.Info);
            }

            base.OnClosing(e);
        }
        /// <summary>
        /// 托盘小图标双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }


        #region 运行时间
        /// <summary>
        /// 定时器事件(1秒循环)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_main_Tick(object sender, EventArgs e)
        {
            state_run_time.Text = getTimeBybytes(runtime);
            long count = MainService.connectionTable.Count;
            if (count > this.onlineTop)
            {
                this.onlineTop = count;
            }
            state_online.Text = count.ToString();
            state_online_top.Text = onlineTop.ToString();

            state_memory.Text = (Process.GetCurrentProcess().PrivateMemorySize64 / 1024f / 1024f).ToString("f1") + " M";
            try
            {
                TimeSpan totalProcessorTime = currutProcess.TotalProcessorTime;
                TimeSpan span2 = totalProcessorTime - prevCpuTime;
                double num3 = ((span2.TotalMilliseconds / ((double)timer_main.Interval)) / ((double)Environment.ProcessorCount)) * 100.0;
                prevCpuTime = totalProcessorTime;
                state_cpu_useage.Text = num3.ToString("0.0") + "%";
            }
            catch
            {
            }

            if (runtime[2] == 59 && runtime[3]==59)
            {
                Thread t = new Thread(CheckUpdate);
                t.IsBackground = true;
                t.Start();
                GC.Collect();
            }
        }
        #endregion

        #region 检查更新

        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckUpdate()
        {
            // 更新功能已禁用 - 移除硬编码的外部URL依赖
            return;

            /*
            bool waiting = true;
            while (waiting)
            {
                // 注意：此处原有硬编码URL已被移除，如需启用更新功能，请配置本地更新服务器
                string updateFileUrl = Config.launcher_update_url; // 使用配置文件中的URL

                if (string.IsNullOrEmpty(updateFileUrl))
                {
                    Thread.Sleep(3600 * 1000);
                    continue;
                }

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
                    Thread.Sleep(3600 * 1000);
                    continue;
                }


                Stream srm = webRes.GetResponseStream();
                BinaryReader br = new BinaryReader(srm);

                int fileHead = br.ReadInt32();//获取文件头4字节里的数字
                byte[] TempBytes = br.ReadBytes(fileHead);//读取文件头信息，该文件所包含的所有文件信息
                string info = Encoding.UTF8.GetString(TempBytes);
                if (!info.Contains(";") || !info.Contains("|"))
                {
                    MessageBox.Show("读取更新文件失败，可能文件格式不正确！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Thread.Sleep(3600 * 1000);
                    continue;
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

                    AionFile af = new AionFile(infos[0], infos[1], int.Parse(infos[2]) + firstLength, int.Parse(infos[2]) + firstLength + int.Parse(infos[3]));
                    string filen = Application.StartupPath + "\\" + af.fileName;
                    if (File.Exists(filen))
                    {
                        string md5 = HashEncrypt.CretaeMD5(filen);
                        if (filen.EndsWith(".exe"))
                        {
                            try
                            {
                                File.Copy(filen, Environment.GetFolderPath(Environment.SpecialFolder.Templates) + "\\temp.tmp", true);
                                md5 = HashEncrypt.CretaeMD5(Environment.GetFolderPath(Environment.SpecialFolder.Templates) + "\\temp.tmp");
                                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Templates) + "\\temp.tmp");
                            }
                            catch
                            {

                            }
                        }

                        if (md5.Equals(af.md5))
                            continue;
                    }
                    AFS.Add(af);
                }

                if (AFS.Count > 0)
                {
                    waiting = false;
                    AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
                    {
                        Form f = new UpdateFile(AFS);
                        f.StartPosition = FormStartPosition.CenterParent;
                        f.ShowDialog();
                    }));
                }
                Thread.Sleep(3600 * 1000);
            }
            */
        }
        #endregion

        #region 时间处理

        /// <summary>
        /// 根据4位数组获取时间表示方式
        /// 需要每秒执行一次
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        private string getTimeBybytes(byte[] bs)
        {
            runtime[3]++;
            if (runtime[3] == 60)
            {
                runtime[3] = 0;
                runtime[2]++;
                if (runtime[2] == 60)
                {
                    runtime[2] = 0;
                    runtime[1]++;
                    if (runtime[1] == 24)
                    {
                        runtime[1] = 0;
                        runtime[0]++;
                    }
                }
            }
            return (runtime[0] == 0 ? "" : runtime[0] + "天") + runtime[1].ToString().PadLeft(2, '0') + ":" + runtime[2].ToString().PadLeft(2, '0') + ":" + runtime[3].ToString().PadLeft(2, '0');
        }

        #endregion

        #region LISTVIEW操作
        /// <summary>
        /// LISTVIEW 排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = sender as ListView;
            ListViewColumnSorter lvcs = lv.ListViewItemSorter as ListViewColumnSorter;
            // 检查点击的列是不是现在的排序列.
            if (e.Column == lvcs.SortColumn)
            {
                // 重新设置此列的排序方法.
                lvcs.Order = lvcs.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                //设置排序列，默认为正向排序
                lvcs.SortColumn = e.Column;
                lvcs.Order = SortOrder.Ascending;
            }
            // 用新的排序方法对ListView排序
            lv.Sort();
        }




        /// <summary>
        /// 从LISTVIEW删除
        /// </summary>
        /// <param name="hashcode"></param>
        internal void RemoveClientFromList(LauncherInfo li)
        {
            // 确保在UI线程中执行
            if (InvokeRequired)
            {
                BeginInvoke(new Action<LauncherInfo>(RemoveClientFromList), li);
                return;
            }

            try
            {
                if (li.listViewItem != null)
                {
                    listView_online.Items.Remove(li.listViewItem);
                    state_online.Text = listView_online.Items.Count.ToString();
                    li.listViewItem = null;
                }
            }
            catch (Exception ex)
            {
                log.error("移除客户端列表失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 选择LISTVIEW条目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_online_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_online.FocusedItem == null || listView_online.FocusedItem.SubItems.Count < 5)
                return;

            selectedConnection = (AionConnection)listView_online.FocusedItem.Tag;
        }

        #endregion

        #region 界面上的按钮 和 设置 相关

        /// <summary>
        /// 自动保存事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 修改自动保存(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox tb = sender as TextBox;
                if (tb.Lines.Length > 1)
                    regKey.SetValue(tb.Name, tb.Lines);
                else
                    regKey.SetValue(tb.Name, tb.Text);
            }
            else if (sender is CheckBox)
            {
                regKey.SetValue((sender as CheckBox).Name, (sender as CheckBox).Checked);
            }
            else if (sender is RadioButton)
            {
                regKey.SetValue((sender as RadioButton).Name, (sender as RadioButton).Checked);
            }
            else if (sender is ComboBox)
            {
                regKey.SetValue((sender as ComboBox).Name, (sender as ComboBox).Text);
            }
            SetValueToConfig();

        }

        /// <summary>
        /// 设置保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Button保存设置_Click(object sender, EventArgs e)
        {
            if (regKey == null)
            {
                MessageBox.Show("无法保存配置", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (string s in regKey.GetValueNames())
            {
                regKey.DeleteValue(s);
            }

            GetControls(this.Controls);

            SetValueToConfig();

            MessageBox.Show("保存设置参数成功！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 遍历控件并保存设置到注册表
        /// </summary>
        /// <param name="ctc"></param>
        private void GetControls(Control.ControlCollection ctc)
        {
            foreach (Control con in ctc)
            {
                if (!con.HasChildren)
                {
                    if (con is TextBox)
                    {
                        TextBox tb = con as TextBox;
                        if (tb.Lines.Length > 1)
                            regKey.SetValue(tb.Name, tb.Lines);
                        else
                            regKey.SetValue(tb.Name, tb.Text);
                    }
                    else if (con is CheckBox)
                    {
                        regKey.SetValue(con.Name, (con as CheckBox).Checked);
                    }
                    else if (con is RadioButton)
                    {
                        regKey.SetValue(con.Name, (con as RadioButton).Checked);
                    }
                    else if (con is ComboBox)
                    {
                        regKey.SetValue(con.Name, (con as ComboBox).Text);
                    }
                }
                else
                {
                    GetControls(con.Controls);
                }
            }
        }

        /// <summary>
        /// 生成客户端文件MD5列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinButton客户端文件MD5生成_Click(object sender, EventArgs e)
        {
            Form f = new Md5Form(ref textBox文件MD5);
            f.ShowDialog();
        }

        /// <summary>
        /// 数据库连接测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinButton_连接测试_Click(object sender, EventArgs e)
        {
            SetValueToConfig();
            if (Config.isMysql)
            {
                string msg;
                string connect = string.Format("Database={0};Data Source={1};User Id={2};Password={3};port={4};Charset={5}", Config.mysql_db_ls, Config.mysql_url, Config.mysql_user, Config.mysql_psw, Config.mysql_port, Config.mysql_code.ToLower());
                bool success = AccountService.Instance.ConnectionTest(connect, out msg);
                if (success)
                {
                    connect = string.Format("Database={0};Data Source={1};User Id={2};Password={3};port={4};Charset={5}", Config.mysql_db_gs, Config.mysql_url, Config.mysql_user, Config.mysql_psw, Config.mysql_port, Config.mysql_code.ToLower());
                    success = AccountService.Instance.ConnectionTest(connect, out msg);
                    MessageBox.Show(this, msg, "提醒", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, msg, "提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else //MSSQL 
            {
                string msg;
                string connect = string.Format("Server = {0},{1}; DataBase = {2}; uid = {3}; pwd = {4}", Config.mysql_url, Config.mysql_port, Config.mysql_db_ls, Config.mysql_user, Config.mysql_psw);
                bool success = AccountService.Instance.ConnectionTest(connect, out msg);
                if (success)
                {
                    connect = string.Format("Server = {0},{1}; DataBase = {2}; uid = {3}; pwd = {4}", Config.mysql_url, Config.mysql_port, Config.mysql_db_gs, Config.mysql_user, Config.mysql_psw);
                    success = AccountService.Instance.ConnectionTest(connect, out msg);
                    MessageBox.Show(this, msg, "提醒", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, msg, "提醒", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void skinButton清除MD5_Click(object sender, EventArgs e)
        {
            textBox_launcherMD5.Text = "请把登陆器拖到这里可生成MD5值";
        }

        private void skinButton补丁打包_Click(object sender, EventArgs e)
        {
            Form f = new PatchForm();
            f.ShowDialog();
        }

        private void skinButton_生成登录器_Click(object sender, EventArgs e)
        {
            Button保存设置_Click(sender, e);
            Form f = new AionNetGate.Launcher.DesignLauncher();
            f.ShowDialog();

        }

        private void checkBox开启双线双IP_CheckedChanged(object sender, EventArgs e)
        {
            textBox网关IP2.Enabled = checkBox开启双线双IP.Checked;
        }

        private void skinButton_恢复参数_Click(object sender, EventArgs e)
        {
            textBox登录器启动参数.Text = "-cc:5 -lang:chs -noauthgg -noweb -nb -gv  -megaphone -multithread -charnamemenu";
        }

        private void skinButton测试邮件发送_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button4.Text = "正在发送中...";
            Thread t = new Thread(new ThreadStart(() =>
            {
                string msg;
                bool b = MailService.SendMailTest(out msg);
                AionRoy.Invoke(this, () => {
                    button4.Enabled = true;
                    button4.Text = "测试邮件发送";
                    MessageBox.Show(this, msg, "提醒", MessageBoxButtons.OK, b ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                });
            }));
            t.IsBackground = true;
            t.Start();
        }

        #endregion

        #region 登录器拖放到控件生成MD5
        /// <summary>
        /// 生成登录器MD5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_launcherMD5_DragDrop(object sender, DragEventArgs e)
        {
            //拖放完成时，如果拖放的是文件则处理
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths[0].ToLower().EndsWith(".exe"))
                    textBox_launcherMD5.Text = HashEncrypt.CretaeMD5(paths[0]);
            }
        }
        /// <summary>
        /// 生成登录器MD5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_launcherMD5_DragEnter(object sender, DragEventArgs e)
        {
            //只允许文件拖放
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        #endregion

        #region 玩家列表右键菜单事件
        private void 查看客户端电脑信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedConnection != null && selectedConnection.ClientSocket.Connected)
            {
                if (selectedConnection.infoForm == null || selectedConnection.infoForm.IsDisposed)
                {
                    InfoForm f = new InfoForm(ref selectedConnection);
                    selectedConnection.setInfoForm(ref f);
                    f.Show();
                }
                else
                {
                    MessageBox.Show("请不要重复打开该客户的远程桌面窗口", "警告");
                }
            }
        }

        private void 查看客户端桌面图像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedConnection != null && selectedConnection.ClientSocket.Connected)
            {
                if (selectedConnection.deskForm == null || selectedConnection.deskForm.IsDisposed)
                {
                    DeskPictureForm f = new DeskPictureForm(ref selectedConnection);
                    selectedConnection.setDeskPictureForm(ref f);
                    f.Show();
                }
                else
                {
                    MessageBox.Show("请不要重复打开该客户的远程桌面窗口", "警告");
                }
            }
        }

        private void 查看客户端进程列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedConnection != null && selectedConnection.ClientSocket.Connected)
            {
                if (selectedConnection.processForm == null || selectedConnection.processForm.IsDisposed)
                {
                    ProcessForm ci = new ProcessForm(ref selectedConnection);
                    selectedConnection.setProcessForm(ref ci);
                    ci.Show();
                }
                else
                {
                    MessageBox.Show("请不要重复打开该客户的进程查看窗口", "警告");
                }
            }
        }

        private void 查看玩家电脑硬盘ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((selectedConnection != null) && selectedConnection.ClientSocket.Connected)
            {
                if ((selectedConnection.explorerForm == null) || selectedConnection.explorerForm.IsDisposed)
                {
                    ExplorerForm form = new ExplorerForm(ref this.selectedConnection);
                    this.selectedConnection.setExplorerForm(ref form);
                    form.Show();
                }
                else
                {
                    MessageBox.Show("请不要重复打开该客户的硬盘浏览窗口", "警告");
                }
            }
        }

        private void 查看客户端系统服务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedConnection != null && selectedConnection.ClientSocket.Connected)
            {
                if (selectedConnection.processForm == null || selectedConnection.processForm.IsDisposed)
                {
                    ServiceListForm ci = new ServiceListForm(ref selectedConnection);
                    selectedConnection.setServerListForm(ref ci);
                    ci.Show();
                }
                else
                {
                    MessageBox.Show("请不要重复打开该客户的系统服务查看窗口", "警告");
                }
            }
        }
        private void 查看客户端注册表项ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (selectedConnection != null && selectedConnection.ClientSocket.Connected)
            {
                if (selectedConnection.processForm == null || selectedConnection.processForm.IsDisposed)
                {
                    RegeditForm ci = new RegeditForm(ref selectedConnection);
                    selectedConnection.setRegeditForm(ref ci);
                    ci.Show();
                }
                else
                {
                    MessageBox.Show("请不要重复打开该客户的注册表查看窗口", "警告");
                }
            }
        }

        private void 禁止选中的玩家登陆ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((selectedConnection != null) && selectedConnection.ClientSocket.Connected) && (listView_online.FocusedItem != null))
            {
                listView_online.FocusedItem.SubItems[6].Text = "0";
                selectedConnection.SendPacket(new SM_COMPUTER_INFO(1));
                string text = listView_online.FocusedItem.SubItems[4].Text;
                if (!blockedHardInfos.Contains(text))
                {
                    blockedHardInfos.Add(text);
                    try
                    {
                        regKey.SetValue("blockedHardInfos", blockedHardInfos.ToArray());
                    }
                    catch (Exception)
                    {
                        log.warn("保存禁止登陆数据失败");
                    }
                }
            }
        }

        private void 允许选中的玩家登陆ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((selectedConnection != null) && selectedConnection.ClientSocket.Connected) && (this.listView_online.FocusedItem != null))
            {
                listView_online.FocusedItem.SubItems[6].Text = "1";
                selectedConnection.SendPacket(new SM_COMPUTER_INFO(3));
                string text = listView_online.FocusedItem.SubItems[4].Text;
                if (blockedHardInfos.Contains(text))
                {
                    blockedHardInfos.Remove(text);
                    regKey.SetValue("blockedHardInfos", blockedHardInfos.ToArray());
                }
            }
        }

        #endregion

        #region 军团在线人数统计
        /// <summary>
        /// 军团在线人数统计
        /// </summary>
        private void button_统计军团_Click(object sender, EventArgs e)
        {
            if (leigonCountThreed != null && leigonCountThreed.IsAlive)
            {
                // 使用安全的方式停止线程而不是Abort
                // TODO: 实现CancellationToken机制来安全停止线程
                try
                {
                    leigonCountThreed.Interrupt();
                }
                catch (Exception)
                {
                    // 忽略中断异常
                }
            }

            if (Config.isMysql)
            {
                MessageBox.Show("当前仅支持MSSQL数据库的军团统计！");
                return;
            }

            if (Config.leigonStartTime == "" || Config.leigonEndTime == "")
            {
                MessageBox.Show("军团统计里面起止时间需要先设置下！");
                return;
            }

            DateTime startTime = DateTime.Parse(Config.leigonStartTime);
            DateTime endTime = DateTime.Parse(Config.leigonEndTime);
            long startTicks = startTime.Ticks;
            long endTicks = endTime.Ticks;
            if (startTicks >= endTicks)
            {
                MessageBox.Show("结束时间必须在开始时间之后！");
                return;
            }

            if (textBox_平均统计几次.Text == "0" || textBox_平均统计几次.Text == "")
            {
                MessageBox.Show("平均统计次数必须大于零！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            button_停止军团统计.Enabled = true;
            button_开启军团统计.Enabled = false;

            leigonCountThreed = new Thread(new ThreadStart(() =>
            {
                Dictionary<string, int[]> nameOnline = new Dictionary<string, int[]>();//存放军团名 对应 在线人数
                long CountTimes = 0; //统计次数
                while (true)
                {
                    CountTimes++;

                    string sql = string.Format("SELECT " +
                 "guild.name AS '军团名称'," +
                 "(select user_id from user_data where char_id=master_id) as '军团长', " +
                 "(CASE guild.race WHEN 0 THEN N'天族' ELSE N'魔族' END) AS '种族'," +
                 "guild.level as '等级'," +
                 "count(user_data.char_id) as '总人数'," +
                 "(select count(char_id) from user_data where guild_id = guild.id and last_login_time=last_logout_time) as '在线'," +
                 "'{0}' as '统计时间' " +
                 "FROM guild INNER JOIN user_data ON user_data.guild_id = guild.id " +
                 "GROUP BY guild.ID,guild.name,guild.master_id,guild.race,guild.level " +
                 "ORDER BY N'在线' DESC;", DateTime.Now.ToLongTimeString());

                    long now = DateTime.Now.Ticks;

                    if (now >= startTicks && now <= endTicks)
                    {
                        DataSet dataSet = AccountService.Instance.ExecuteSelectCmmond(sql);

                        if (dataSet.Tables.Count > 0)
                        {


                            //说明这个军团已经在统计，然后判断统计多少次了
                            long rt = CountTimes % Config.leigonCountAVG;

                            string mn = "最近" + Config.leigonCountAVG + "次平均在线数";

                            DataTable dt = dataSet.Tables[0];
                            dt.Columns.Add(mn);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {

                                //已经存放上次统计结果的话
                                string leigonName = dt.Rows[i][0].ToString();

                                int[] avg;
                                if (nameOnline.TryGetValue(leigonName, out avg))
                                {
                                    //先存放本次查询出来都在线人数
                                    avg[rt] = int.Parse(dt.Rows[i][5].ToString());

                                    if (CountTimes < Config.leigonCountAVG)
                                    {
                                        //如果满足统计平均次数,第一次计算平均在线
                                        if (rt == 0)
                                        {
                                            int b = 0;
                                            foreach (int a in avg)
                                            {
                                                b += a;
                                            }
                                            dt.Rows[i][mn] = b / Config.leigonCountAVG;

                                        }
                                        else
                                        {
                                            dt.Rows[i][mn] = "暂未统计";
                                        }
                                    }
                                    else //如果已经超过统计次数的查询次数，那么每一次查询完后都要计算一次最近 N 次得平均统计
                                    {
                                        int b = 0;
                                        foreach (int a in avg)
                                        {
                                            b += a;
                                        }
                                        dt.Rows[i][mn] = b / Config.leigonCountAVG;
                                    }
                                }
                                else
                                {
                                    //说明这个军团还是第一次统计
                                    avg = new int[Config.leigonCountAVG];
                                    //先存放本次查询出来都在线人数
                                    avg[rt] = int.Parse(dt.Rows[i][5].ToString());
                                    nameOnline.Add(leigonName, avg);
                                    dt.Rows[i][mn] = "暂未统计";
                                }
                            }
                            
                            AionRoy.Invoke(this, () => {
                                dataGridView1.DataSource = dt;
                                dataGridView1.Columns[0].Width = 150;
                                dataGridView1.Columns[1].Width = 130;
                                dataGridView1.Columns[2].Width = 60;
                                dataGridView1.Columns[3].Width = 60;
                                dataGridView1.Columns[4].Width = 60;
                                dataGridView1.Columns[5].Width = 50;
                                dataGridView1.Columns[6].Width = 80;
                                dataGridView1.Columns[7].Width = 150;
                            });
                        }

                    }
                    Thread.Sleep(Config.leigonWaitTime * 60 * 1000);
     
                }
            }));


            leigonCountThreed.IsBackground = true;
            leigonCountThreed.Start();
        }
        
        private void button_停止军团统计_Click(object sender, EventArgs e)
        {
            if (leigonCountThreed != null && leigonCountThreed.IsAlive)
            {
                // 使用安全的方式停止线程而不是Abort
                try
                {
                    leigonCountThreed.Interrupt();
                }
                catch (Exception)
                {
                    // 忽略中断异常
                }
            }
            button_开启军团统计.Enabled = true;
            button_停止军团统计.Enabled = false;
        }




        #endregion

        /// <summary>
        /// 检查在线人数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_checkOnline_Tick(object sender, EventArgs e)
        {
            timer_checkOnline.Stop();
            List<AionConnection> list = new List<AionConnection>();
            int num = 0;
            while (true)
            {
                if (num >= listView_online.Items.Count)
                {
                    break;
                }
                try
                {
                    ListViewItem item = this.listView_online.Items[num];
                    if ((item != null) && (item.Tag != null))
                    {
                        AionConnection tag = (AionConnection)item.Tag;
                        long num2 = AionConnection.currentTimeMillis() - tag.getLastPing();
                        if ((tag.getLastPing() > 0) && (num2 > 70000))
                        {
                            list.Add(tag);
                        }
                        else
                        {
                            tag.CheckPingTime();
                        }
                    }
                }
                catch (Exception)
                {
                }
                num++;
            }
            try
            {
                if (list.Count > 0)
                {
                    foreach (AionConnection connection2 in list)
                    {
                        MainService.Instance.RemoveConnect(connection2);
                    }
                }
                else
                {
                    list = null;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                timer_checkOnline.Start();
            }
        }

        private void timerRestart_Tick(object sender, EventArgs e)
        {
            if (timer_main.Enabled)
            {
                停止服务TToolStripMenuItem_Click(null, null);
            }
            Application.Restart();
        }


        private void checkBox_NewDatabase_CheckedChanged(object sender, EventArgs e)
        {
            Config.newaccountdatabase = checkBox_NewDatabase.Checked;
        }
    }

    #region 匿名委托
    public class AionRoy
    {
        public delegate void Handler();

        /// <summary>
        /// 通用委托调用
        /// </summary>
        /// <param name="control">WINFORM：this</param>
        /// <param name="handler">委托</param>
        public static void Invoke(System.Windows.Forms.Control control, Handler handler)
        {
            try
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
            catch { }
        }
        public static void Invoke(ListView lvi, Handler handler)
        {
            try
            {
                if (lvi.InvokeRequired)
                {
                    lvi.Invoke(handler);
                }
                else
                {
                    handler();
                }
            }
            catch { }
        }

    }
    #endregion
}

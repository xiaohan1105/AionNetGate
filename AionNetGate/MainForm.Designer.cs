namespace AionNetGate
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件FToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.导入配置IToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.导出配置OToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出程序EToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.控制CToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.启动服务QToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.停止服务TToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于AToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.注册网关ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.注册软件RToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于作者AToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.state_stat = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.state_run_time = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.state_cpu_useage = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.state_memory = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel8 = new System.Windows.Forms.ToolStripStatusLabel();
            this.state_online = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel10 = new System.Windows.Forms.ToolStripStatusLabel();
            this.state_online_top = new System.Windows.Forms.ToolStripStatusLabel();
            this.richTextBox_log = new System.Windows.Forms.RichTextBox();
            this.listView_online = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.查看客户端电脑信息ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看客户端桌面图像ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看客户端进程列表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看客户端系统服务ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看客户端注册表项ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.查看玩家电脑硬盘ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.禁止选中的玩家登陆ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.允许选中的玩家登陆ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer_main = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.textBox账号服务器LS端口 = new System.Windows.Forms.TextBox();
            this.checkBox_启用静态传送 = new System.Windows.Forms.CheckBox();
            this.textBox邮箱密码 = new System.Windows.Forms.TextBox();
            this.textBox邮箱STMP端口 = new System.Windows.Forms.TextBox();
            this.textBox邮箱STMP地址 = new System.Windows.Forms.TextBox();
            this.textBox发送邮箱 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox转发密码 = new System.Windows.Forms.TextBox();
            this.checkBox禁止提前登录 = new System.Windows.Forms.CheckBox();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.panel9 = new BSE.Windows.Forms.Panel();
            this.checkBox_NewDatabase = new System.Windows.Forms.CheckBox();
            this.button3 = new System.Windows.Forms.Button();
            this.radioButton_MSSQL = new System.Windows.Forms.RadioButton();
            this.textBox_mysqluser = new System.Windows.Forms.TextBox();
            this.radioButton_MySQL = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_mysqlurl = new System.Windows.Forms.TextBox();
            this.textBox_mysqlport = new System.Windows.Forms.TextBox();
            this.textBox_mysql_db_gs = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_mysql_db_ls = new System.Windows.Forms.TextBox();
            this.textBox_mysql_code = new System.Windows.Forms.TextBox();
            this.textBox_mysqlpassword = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panel4 = new BSE.Windows.Forms.Panel();
            this.labelEx2 = new AionCommons.WinForm.LabelEx();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.textBox分块图片长度 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox图像压缩率 = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.textBox分块图片宽度 = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.panel3 = new BSE.Windows.Forms.Panel();
            this.checkBox_close_mmzh = new System.Windows.Forms.CheckBox();
            this.labelEx1 = new AionCommons.WinForm.LabelEx();
            this.button4 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.panel2 = new BSE.Windows.Forms.Panel();
            this.panel1 = new BSE.Windows.Forms.Panel();
            this.checkBox开启双线双IP = new System.Windows.Forms.CheckBox();
            this.Button保存设置 = new System.Windows.Forms.Button();
            this.checkBox_检查到攻击自动封IP = new System.Windows.Forms.CheckBox();
            this.label39 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_网关自动重启 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.textBox下载端口 = new System.Windows.Forms.TextBox();
            this.textBox网关端口 = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.textBox网关重启时间 = new System.Windows.Forms.TextBox();
            this.checkBox_转发器自动重启 = new System.Windows.Forms.CheckBox();
            this.textBox转发重启时间 = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox网关IP = new System.Windows.Forms.TextBox();
            this.textBox网关IP2 = new System.Windows.Forms.TextBox();
            this.checkBox开启通讯日志显示 = new System.Windows.Forms.CheckBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel6 = new BSE.Windows.Forms.Panel();
            this.button7 = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.textBox登录器名字 = new System.Windows.Forms.TextBox();
            this.label42 = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.checkBox登录器可双开 = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.textBox目录限制 = new System.Windows.Forms.TextBox();
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.textBox文件MD5 = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.checkBox查到外挂关闭 = new System.Windows.Forms.CheckBox();
            this.textBox外挂进程 = new System.Windows.Forms.TextBox();
            this.textBox登录器WEB = new System.Windows.Forms.TextBox();
            this.textBox登录器BIN64 = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox登录器BIN32 = new System.Windows.Forms.TextBox();
            this.textBox登录器启动参数 = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.panel5 = new BSE.Windows.Forms.Panel();
            this.button8 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.textBox_updateUrl = new System.Windows.Forms.TextBox();
            this.textBox_patch_url = new System.Windows.Forms.TextBox();
            this.label33 = new System.Windows.Forms.Label();
            this.textBox_launcherMD5 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel8 = new BSE.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel7 = new BSE.Windows.Forms.Panel();
            this.label38 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.textBox_平均统计几次 = new System.Windows.Forms.TextBox();
            this.button_停止军团统计 = new System.Windows.Forms.Button();
            this.comboBox_结束统计 = new System.Windows.Forms.ComboBox();
            this.comboBox_开始统计 = new System.Windows.Forms.ComboBox();
            this.button_开启军团统计 = new System.Windows.Forms.Button();
            this.label29 = new System.Windows.Forms.Label();
            this.textBox_统计间隔 = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.button_保存军团设置 = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer_checkOnline = new System.Windows.Forms.Timer(this.components);
            this.timerRestart = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.panel9.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.tabPage9.SuspendLayout();
            this.tabPage10.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件FToolStripMenuItem,
            this.控制CToolStripMenuItem,
            this.关于AToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(944, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件FToolStripMenuItem
            // 
            this.文件FToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.导入配置IToolStripMenuItem,
            this.导出配置OToolStripMenuItem,
            this.退出程序EToolStripMenuItem});
            this.文件FToolStripMenuItem.Name = "文件FToolStripMenuItem";
            this.文件FToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.文件FToolStripMenuItem.Text = "文件(&F)";
            // 
            // 导入配置IToolStripMenuItem
            // 
            this.导入配置IToolStripMenuItem.Name = "导入配置IToolStripMenuItem";
            this.导入配置IToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.导入配置IToolStripMenuItem.Text = "当前设置另存为(&S)";
            this.导入配置IToolStripMenuItem.Click += new System.EventHandler(this.导入配置IToolStripMenuItem_Click);
            // 
            // 导出配置OToolStripMenuItem
            // 
            this.导出配置OToolStripMenuItem.Name = "导出配置OToolStripMenuItem";
            this.导出配置OToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.导出配置OToolStripMenuItem.Text = "从文件导入配置(&I)";
            this.导出配置OToolStripMenuItem.Click += new System.EventHandler(this.导出配置OToolStripMenuItem_Click);
            // 
            // 退出程序EToolStripMenuItem
            // 
            this.退出程序EToolStripMenuItem.Name = "退出程序EToolStripMenuItem";
            this.退出程序EToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.退出程序EToolStripMenuItem.Text = "退出程序(&E)";
            this.退出程序EToolStripMenuItem.Click += new System.EventHandler(this.退出程序EToolStripMenuItem_Click);
            // 
            // 控制CToolStripMenuItem
            // 
            this.控制CToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.启动服务QToolStripMenuItem,
            this.停止服务TToolStripMenuItem});
            this.控制CToolStripMenuItem.Name = "控制CToolStripMenuItem";
            this.控制CToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.控制CToolStripMenuItem.Text = "控制(&C)";
            // 
            // 启动服务QToolStripMenuItem
            // 
            this.启动服务QToolStripMenuItem.Name = "启动服务QToolStripMenuItem";
            this.启动服务QToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.启动服务QToolStripMenuItem.Text = "启动服务(&Q)";
            this.启动服务QToolStripMenuItem.Click += new System.EventHandler(this.启动服务QToolStripMenuItem_Click);
            // 
            // 停止服务TToolStripMenuItem
            // 
            this.停止服务TToolStripMenuItem.Name = "停止服务TToolStripMenuItem";
            this.停止服务TToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.停止服务TToolStripMenuItem.Text = "停止服务(&T)";
            this.停止服务TToolStripMenuItem.Click += new System.EventHandler(this.停止服务TToolStripMenuItem_Click);
            // 
            // 关于AToolStripMenuItem
            // 
            this.关于AToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.注册网关ToolStripMenuItem,
            this.注册软件RToolStripMenuItem,
            this.关于作者AToolStripMenuItem});
            this.关于AToolStripMenuItem.Name = "关于AToolStripMenuItem";
            this.关于AToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.关于AToolStripMenuItem.Text = "关于(&A)";
            // 
            // 注册网关ToolStripMenuItem
            // 
            this.注册网关ToolStripMenuItem.Name = "注册网关ToolStripMenuItem";
            this.注册网关ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.注册网关ToolStripMenuItem.Text = "注册网关";
            // this.注册网关ToolStripMenuItem.Click += new System.EventHandler(this.注册网关ToolStripMenuItem_Click);
            // 
            // 注册软件RToolStripMenuItem
            // 
            this.注册软件RToolStripMenuItem.Name = "注册软件RToolStripMenuItem";
            this.注册软件RToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.注册软件RToolStripMenuItem.Text = "赞助我(&R)";
            // this.注册软件RToolStripMenuItem.Click += new System.EventHandler(this.注册软件RToolStripMenuItem_Click);
            // 
            // 关于作者AToolStripMenuItem
            // 
            this.关于作者AToolStripMenuItem.Name = "关于作者AToolStripMenuItem";
            this.关于作者AToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.关于作者AToolStripMenuItem.Text = "关于作者(&A)";
            // this.关于作者AToolStripMenuItem.Click += new System.EventHandler(this.关于作者AToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(95, 20);
            this.toolStripMenuItem1.Text = "当前版本：3.9";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.state_stat,
            this.toolStripStatusLabel2,
            this.state_run_time,
            this.toolStripStatusLabel4,
            this.state_cpu_useage,
            this.toolStripStatusLabel6,
            this.state_memory,
            this.toolStripStatusLabel8,
            this.state_online,
            this.toolStripStatusLabel10,
            this.state_online_top});
            this.statusStrip1.Location = new System.Drawing.Point(0, 510);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(944, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(41, 17);
            this.toolStripStatusLabel1.Text = "状态：";
            // 
            // state_stat
            // 
            this.state_stat.AutoSize = false;
            this.state_stat.Name = "state_stat";
            this.state_stat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.state_stat.Size = new System.Drawing.Size(50, 17);
            this.state_stat.Text = "未运行";
            this.state_stat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(77, 17);
            this.toolStripStatusLabel2.Text = "| 运行时间：";
            // 
            // state_run_time
            // 
            this.state_run_time.AutoSize = false;
            this.state_run_time.Name = "state_run_time";
            this.state_run_time.Size = new System.Drawing.Size(100, 17);
            this.state_run_time.Text = "0";
            this.state_run_time.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(83, 17);
            this.toolStripStatusLabel4.Text = "| CPU使用率：";
            // 
            // state_cpu_useage
            // 
            this.state_cpu_useage.AutoSize = false;
            this.state_cpu_useage.Name = "state_cpu_useage";
            this.state_cpu_useage.Size = new System.Drawing.Size(40, 17);
            this.state_cpu_useage.Text = "0%";
            this.state_cpu_useage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(77, 17);
            this.toolStripStatusLabel6.Text = "| 内存使用：";
            // 
            // state_memory
            // 
            this.state_memory.AutoSize = false;
            this.state_memory.Name = "state_memory";
            this.state_memory.Size = new System.Drawing.Size(60, 17);
            this.state_memory.Text = "0M";
            this.state_memory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel8
            // 
            this.toolStripStatusLabel8.Name = "toolStripStatusLabel8";
            this.toolStripStatusLabel8.Size = new System.Drawing.Size(77, 17);
            this.toolStripStatusLabel8.Text = "| 当前在线：";
            // 
            // state_online
            // 
            this.state_online.AutoSize = false;
            this.state_online.Name = "state_online";
            this.state_online.Size = new System.Drawing.Size(35, 17);
            this.state_online.Text = "0";
            this.state_online.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel10
            // 
            this.toolStripStatusLabel10.Name = "toolStripStatusLabel10";
            this.toolStripStatusLabel10.Size = new System.Drawing.Size(77, 17);
            this.toolStripStatusLabel10.Text = "| 最高在线：";
            // 
            // state_online_top
            // 
            this.state_online_top.Name = "state_online_top";
            this.state_online_top.Size = new System.Drawing.Size(212, 17);
            this.state_online_top.Spring = true;
            this.state_online_top.Text = "0";
            this.state_online_top.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // richTextBox_log
            // 
            this.richTextBox_log.BackColor = System.Drawing.Color.Black;
            this.richTextBox_log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_log.Location = new System.Drawing.Point(3, 3);
            this.richTextBox_log.Name = "richTextBox_log";
            this.richTextBox_log.Size = new System.Drawing.Size(930, 442);
            this.richTextBox_log.TabIndex = 0;
            this.richTextBox_log.Text = "";
            // 
            // listView_online
            // 
            this.listView_online.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listView_online.ContextMenuStrip = this.contextMenuStrip1;
            this.listView_online.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_online.FullRowSelect = true;
            this.listView_online.GridLines = true;
            this.listView_online.HideSelection = false;
            this.listView_online.Location = new System.Drawing.Point(3, 3);
            this.listView_online.MultiSelect = false;
            this.listView_online.Name = "listView_online";
            this.listView_online.ShowItemToolTips = true;
            this.listView_online.Size = new System.Drawing.Size(930, 442);
            this.listView_online.TabIndex = 0;
            this.listView_online.UseCompatibleStateImageBehavior = false;
            this.listView_online.View = System.Windows.Forms.View.Details;
            this.listView_online.SelectedIndexChanged += new System.EventHandler(this.listView_online_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "客户IP地址";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "归属地";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "操作系统";
            this.columnHeader3.Width = 80;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "计算机名";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "硬件标识";
            this.columnHeader5.Width = 100;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "网卡MAC地址";
            this.columnHeader6.Width = 130;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "允许登陆";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "可疑非法账号";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader8.Width = 100;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.查看客户端电脑信息ToolStripMenuItem,
            this.查看客户端桌面图像ToolStripMenuItem,
            this.查看客户端进程列表ToolStripMenuItem,
            this.查看客户端系统服务ToolStripMenuItem,
            this.查看客户端注册表项ToolStripMenuItem1,
            this.查看玩家电脑硬盘ToolStripMenuItem,
            this.toolStripSeparator1,
            this.禁止选中的玩家登陆ToolStripMenuItem,
            this.允许选中的玩家登陆ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(185, 186);
            // 
            // 查看客户端电脑信息ToolStripMenuItem
            // 
            this.查看客户端电脑信息ToolStripMenuItem.Name = "查看客户端电脑信息ToolStripMenuItem";
            this.查看客户端电脑信息ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.查看客户端电脑信息ToolStripMenuItem.Text = "查看客户端电脑信息";
            this.查看客户端电脑信息ToolStripMenuItem.Click += new System.EventHandler(this.查看客户端电脑信息ToolStripMenuItem_Click);
            // 
            // 查看客户端桌面图像ToolStripMenuItem
            // 
            this.查看客户端桌面图像ToolStripMenuItem.Name = "查看客户端桌面图像ToolStripMenuItem";
            this.查看客户端桌面图像ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.查看客户端桌面图像ToolStripMenuItem.Text = "查看客户端桌面图像";
            this.查看客户端桌面图像ToolStripMenuItem.Click += new System.EventHandler(this.查看客户端桌面图像ToolStripMenuItem_Click);
            // 
            // 查看客户端进程列表ToolStripMenuItem
            // 
            this.查看客户端进程列表ToolStripMenuItem.Name = "查看客户端进程列表ToolStripMenuItem";
            this.查看客户端进程列表ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.查看客户端进程列表ToolStripMenuItem.Text = "查看客户端进程列表";
            this.查看客户端进程列表ToolStripMenuItem.Click += new System.EventHandler(this.查看客户端进程列表ToolStripMenuItem_Click);
            // 
            // 查看客户端系统服务ToolStripMenuItem
            // 
            this.查看客户端系统服务ToolStripMenuItem.Name = "查看客户端系统服务ToolStripMenuItem";
            this.查看客户端系统服务ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.查看客户端系统服务ToolStripMenuItem.Text = "查看客户端系统服务";
            this.查看客户端系统服务ToolStripMenuItem.Click += new System.EventHandler(this.查看客户端系统服务ToolStripMenuItem_Click);
            // 
            // 查看客户端注册表项ToolStripMenuItem1
            // 
            this.查看客户端注册表项ToolStripMenuItem1.Name = "查看客户端注册表项ToolStripMenuItem1";
            this.查看客户端注册表项ToolStripMenuItem1.Size = new System.Drawing.Size(184, 22);
            this.查看客户端注册表项ToolStripMenuItem1.Text = "查看客户端注册表项";
            this.查看客户端注册表项ToolStripMenuItem1.Click += new System.EventHandler(this.查看客户端注册表项ToolStripMenuItem1_Click);
            // 
            // 查看玩家电脑硬盘ToolStripMenuItem
            // 
            this.查看玩家电脑硬盘ToolStripMenuItem.Name = "查看玩家电脑硬盘ToolStripMenuItem";
            this.查看玩家电脑硬盘ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.查看玩家电脑硬盘ToolStripMenuItem.Text = "查看客户端电脑硬盘";
            this.查看玩家电脑硬盘ToolStripMenuItem.Click += new System.EventHandler(this.查看玩家电脑硬盘ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // 禁止选中的玩家登陆ToolStripMenuItem
            // 
            this.禁止选中的玩家登陆ToolStripMenuItem.Name = "禁止选中的玩家登陆ToolStripMenuItem";
            this.禁止选中的玩家登陆ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.禁止选中的玩家登陆ToolStripMenuItem.Text = "禁止选中的玩家登陆";
            this.禁止选中的玩家登陆ToolStripMenuItem.Click += new System.EventHandler(this.禁止选中的玩家登陆ToolStripMenuItem_Click);
            // 
            // 允许选中的玩家登陆ToolStripMenuItem
            // 
            this.允许选中的玩家登陆ToolStripMenuItem.Name = "允许选中的玩家登陆ToolStripMenuItem";
            this.允许选中的玩家登陆ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.允许选中的玩家登陆ToolStripMenuItem.Text = "允许选中的玩家登陆";
            this.允许选中的玩家登陆ToolStripMenuItem.Click += new System.EventHandler(this.允许选中的玩家登陆ToolStripMenuItem_Click);
            // 
            // timer_main
            // 
            this.timer_main.Interval = 1000;
            this.timer_main.Tick += new System.EventHandler(this.timer_main_Tick);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 15000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "友情提示";
            // 
            // textBox账号服务器LS端口
            // 
            this.textBox账号服务器LS端口.Location = new System.Drawing.Point(560, 73);
            this.textBox账号服务器LS端口.Name = "textBox账号服务器LS端口";
            this.textBox账号服务器LS端口.Size = new System.Drawing.Size(46, 21);
            this.textBox账号服务器LS端口.TabIndex = 44;
            this.textBox账号服务器LS端口.Text = "2106";
            this.toolTip1.SetToolTip(this.textBox账号服务器LS端口, "该端口为账号服务器(LS)上设置的端口。\r\n如果你使用了转发器，那么该端口应该是\r\n转发器对外开放的端口。");
            this.textBox账号服务器LS端口.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // checkBox_启用静态传送
            // 
            this.checkBox_启用静态传送.AutoSize = true;
            this.checkBox_启用静态传送.Location = new System.Drawing.Point(695, 74);
            this.checkBox_启用静态传送.Name = "checkBox_启用静态传送";
            this.checkBox_启用静态传送.Size = new System.Drawing.Size(120, 16);
            this.checkBox_启用静态传送.TabIndex = 51;
            this.checkBox_启用静态传送.Text = "启用参数动态更新";
            this.toolTip1.SetToolTip(this.checkBox_启用静态传送, "启用静态参数动态更新到登录器，这样可能会影响某些玩家连接不上网关。而且会造成网关资源占用率较高！");
            this.checkBox_启用静态传送.UseVisualStyleBackColor = true;
            // 
            // textBox邮箱密码
            // 
            this.textBox邮箱密码.Location = new System.Drawing.Point(86, 58);
            this.textBox邮箱密码.Name = "textBox邮箱密码";
            this.textBox邮箱密码.PasswordChar = '*';
            this.textBox邮箱密码.Size = new System.Drawing.Size(172, 21);
            this.textBox邮箱密码.TabIndex = 2;
            this.toolTip1.SetToolTip(this.textBox邮箱密码, "请输入你用于发信的邮箱的密码");
            this.textBox邮箱密码.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox邮箱STMP端口
            // 
            this.textBox邮箱STMP端口.Location = new System.Drawing.Point(86, 112);
            this.textBox邮箱STMP端口.Name = "textBox邮箱STMP端口";
            this.textBox邮箱STMP端口.Size = new System.Drawing.Size(57, 21);
            this.textBox邮箱STMP端口.TabIndex = 2;
            this.textBox邮箱STMP端口.Text = "25";
            this.toolTip1.SetToolTip(this.textBox邮箱STMP端口, "一般smtp端口25");
            this.textBox邮箱STMP端口.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox邮箱STMP地址
            // 
            this.textBox邮箱STMP地址.Location = new System.Drawing.Point(86, 85);
            this.textBox邮箱STMP地址.Name = "textBox邮箱STMP地址";
            this.textBox邮箱STMP地址.Size = new System.Drawing.Size(172, 21);
            this.textBox邮箱STMP地址.TabIndex = 2;
            this.textBox邮箱STMP地址.Text = "smtp.163.com";
            this.toolTip1.SetToolTip(this.textBox邮箱STMP地址, "例如smtp.163.com ，smtp.126.com");
            this.textBox邮箱STMP地址.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox发送邮箱
            // 
            this.textBox发送邮箱.Location = new System.Drawing.Point(86, 31);
            this.textBox发送邮箱.Name = "textBox发送邮箱";
            this.textBox发送邮箱.Size = new System.Drawing.Size(172, 21);
            this.textBox发送邮箱.TabIndex = 2;
            this.toolTip1.SetToolTip(this.textBox发送邮箱, "请输入你自己的一个邮箱地址，建议使用163的邮箱");
            this.textBox发送邮箱.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label5.Location = new System.Drawing.Point(6, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "账号数据库：";
            this.toolTip1.SetToolTip(this.label5, "比如aionroy_ls，或者aionAccounts，主要用于存放玩家账号信息的表");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label10.Location = new System.Drawing.Point(6, 168);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 12);
            this.label10.TabIndex = 15;
            this.label10.Text = "角色数据库：";
            this.toolTip1.SetToolTip(this.label10, "主要用于存放玩家角色的相关数据，比如al_server_gs，或者AionWorlds");
            // 
            // textBox转发密码
            // 
            this.textBox转发密码.Location = new System.Drawing.Point(683, 72);
            this.textBox转发密码.Name = "textBox转发密码";
            this.textBox转发密码.Size = new System.Drawing.Size(39, 21);
            this.textBox转发密码.TabIndex = 44;
            this.textBox转发密码.Text = "密";
            this.toolTip1.SetToolTip(this.textBox转发密码, "仅支持单个中文字符");
            this.textBox转发密码.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // checkBox禁止提前登录
            // 
            this.checkBox禁止提前登录.AutoSize = true;
            this.checkBox禁止提前登录.Checked = true;
            this.checkBox禁止提前登录.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox禁止提前登录.Location = new System.Drawing.Point(740, 75);
            this.checkBox禁止提前登录.Name = "checkBox禁止提前登录";
            this.checkBox禁止提前登录.Size = new System.Drawing.Size(84, 16);
            this.checkBox禁止提前登录.TabIndex = 50;
            this.checkBox禁止提前登录.Text = "禁止预登陆";
            this.toolTip1.SetToolTip(this.checkBox禁止提前登录, "选中后登录器启动游戏时候不会先弹出让玩家输入账号密码都登录界面。");
            this.checkBox禁止提前登录.UseVisualStyleBackColor = true;
            this.checkBox禁止提前登录.Click += new System.EventHandler(this.修改自动保存);
            // 
            // tabControl2
            // 
            this.tabControl2.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Controls.Add(this.tabPage6);
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.ItemSize = new System.Drawing.Size(80, 30);
            this.tabControl2.Location = new System.Drawing.Point(0, 24);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(944, 486);
            this.tabControl2.TabIndex = 3;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.richTextBox_log);
            this.tabPage4.Location = new System.Drawing.Point(4, 34);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(936, 448);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "网关日志";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.listView_online);
            this.tabPage5.Location = new System.Drawing.Point(4, 34);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(936, 448);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "在线玩家";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.panel9);
            this.tabPage6.Controls.Add(this.panel4);
            this.tabPage6.Controls.Add(this.panel3);
            this.tabPage6.Controls.Add(this.panel2);
            this.tabPage6.Controls.Add(this.panel1);
            this.tabPage6.Location = new System.Drawing.Point(4, 34);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(936, 448);
            this.tabPage6.TabIndex = 2;
            this.tabPage6.Text = "网关设置";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // panel9
            // 
            this.panel9.AssociatedSplitter = null;
            this.panel9.BackColor = System.Drawing.Color.Transparent;
            this.panel9.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel9.CaptionHeight = 27;
            this.panel9.Controls.Add(this.checkBox_NewDatabase);
            this.panel9.Controls.Add(this.button3);
            this.panel9.Controls.Add(this.radioButton_MSSQL);
            this.panel9.Controls.Add(this.textBox_mysqluser);
            this.panel9.Controls.Add(this.radioButton_MySQL);
            this.panel9.Controls.Add(this.label6);
            this.panel9.Controls.Add(this.textBox_mysqlurl);
            this.panel9.Controls.Add(this.label5);
            this.panel9.Controls.Add(this.textBox_mysqlport);
            this.panel9.Controls.Add(this.textBox_mysql_db_gs);
            this.panel9.Controls.Add(this.label10);
            this.panel9.Controls.Add(this.label7);
            this.panel9.Controls.Add(this.textBox_mysql_db_ls);
            this.panel9.Controls.Add(this.textBox_mysql_code);
            this.panel9.Controls.Add(this.textBox_mysqlpassword);
            this.panel9.Controls.Add(this.label21);
            this.panel9.Controls.Add(this.label4);
            this.panel9.Controls.Add(this.label8);
            this.panel9.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel9.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel9.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel9.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel9.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel9.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel9.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel9.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel9.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel9.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel9.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel9.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel9.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel9.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel9.Image = null;
            this.panel9.Location = new System.Drawing.Point(691, 3);
            this.panel9.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(242, 288);
            this.panel9.TabIndex = 55;
            this.panel9.Text = "数据库连接";
            this.panel9.ToolTipTextCloseIcon = null;
            this.panel9.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel9.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // checkBox_NewDatabase
            // 
            this.checkBox_NewDatabase.AutoSize = true;
            this.checkBox_NewDatabase.ForeColor = System.Drawing.Color.Red;
            this.checkBox_NewDatabase.Location = new System.Drawing.Point(9, 256);
            this.checkBox_NewDatabase.Name = "checkBox_NewDatabase";
            this.checkBox_NewDatabase.Size = new System.Drawing.Size(120, 16);
            this.checkBox_NewDatabase.TabIndex = 29;
            this.checkBox_NewDatabase.Text = "变异格式(不要选)";
            this.checkBox_NewDatabase.UseVisualStyleBackColor = true;
            this.checkBox_NewDatabase.CheckedChanged += new System.EventHandler(this.checkBox_NewDatabase_CheckedChanged);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(137, 247);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(65, 33);
            this.button3.TabIndex = 26;
            this.button3.Text = "连接测试";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.skinButton_连接测试_Click);
            // 
            // radioButton_MSSQL
            // 
            this.radioButton_MSSQL.AutoSize = true;
            this.radioButton_MSSQL.Enabled = true;
            this.radioButton_MSSQL.Location = new System.Drawing.Point(149, 222);
            this.radioButton_MSSQL.Name = "radioButton_MSSQL";
            this.radioButton_MSSQL.Size = new System.Drawing.Size(53, 16);
            this.radioButton_MSSQL.TabIndex = 27;
            this.radioButton_MSSQL.Text = "MSSQL";
            this.radioButton_MSSQL.UseVisualStyleBackColor = true;
            this.radioButton_MSSQL.Visible = true;
            this.radioButton_MSSQL.CheckedChanged += new System.EventHandler(this.修改自动保存);
            // 
            // textBox_mysqluser
            // 
            this.textBox_mysqluser.Location = new System.Drawing.Point(89, 60);
            this.textBox_mysqluser.Name = "textBox_mysqluser";
            this.textBox_mysqluser.Size = new System.Drawing.Size(113, 21);
            this.textBox_mysqluser.TabIndex = 19;
            this.textBox_mysqluser.Text = "root";
            this.textBox_mysqluser.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // radioButton_MySQL
            // 
            this.radioButton_MySQL.AutoSize = true;
            this.radioButton_MySQL.Checked = true;
            this.radioButton_MySQL.Location = new System.Drawing.Point(89, 222);
            this.radioButton_MySQL.Name = "radioButton_MySQL";
            this.radioButton_MySQL.Size = new System.Drawing.Size(53, 16);
            this.radioButton_MySQL.TabIndex = 27;
            this.radioButton_MySQL.TabStop = true;
            this.radioButton_MySQL.Text = "MySQL";
            this.radioButton_MySQL.UseVisualStyleBackColor = true;
            this.radioButton_MySQL.CheckedChanged += new System.EventHandler(this.修改自动保存);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(18, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "用户名称：";
            // 
            // textBox_mysqlurl
            // 
            this.textBox_mysqlurl.Location = new System.Drawing.Point(89, 32);
            this.textBox_mysqlurl.Name = "textBox_mysqlurl";
            this.textBox_mysqlurl.Size = new System.Drawing.Size(113, 21);
            this.textBox_mysqlurl.TabIndex = 20;
            this.textBox_mysqlurl.Text = "127.0.0.1";
            this.textBox_mysqlurl.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox_mysqlport
            // 
            this.textBox_mysqlport.Location = new System.Drawing.Point(89, 87);
            this.textBox_mysqlport.Name = "textBox_mysqlport";
            this.textBox_mysqlport.Size = new System.Drawing.Size(113, 21);
            this.textBox_mysqlport.TabIndex = 17;
            this.textBox_mysqlport.Text = "3366";
            this.textBox_mysqlport.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox_mysql_db_gs
            // 
            this.textBox_mysql_db_gs.Location = new System.Drawing.Point(89, 168);
            this.textBox_mysql_db_gs.Name = "textBox_mysql_db_gs";
            this.textBox_mysql_db_gs.Size = new System.Drawing.Size(113, 21);
            this.textBox_mysql_db_gs.TabIndex = 21;
            this.textBox_mysql_db_gs.Text = "eridian_gs";
            this.textBox_mysql_db_gs.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(18, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "连接地址：";
            // 
            // textBox_mysql_db_ls
            // 
            this.textBox_mysql_db_ls.Location = new System.Drawing.Point(89, 140);
            this.textBox_mysql_db_ls.Name = "textBox_mysql_db_ls";
            this.textBox_mysql_db_ls.Size = new System.Drawing.Size(113, 21);
            this.textBox_mysql_db_ls.TabIndex = 22;
            this.textBox_mysql_db_ls.Text = "eridian_ls";
            this.textBox_mysql_db_ls.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox_mysql_code
            // 
            this.textBox_mysql_code.Location = new System.Drawing.Point(89, 195);
            this.textBox_mysql_code.Name = "textBox_mysql_code";
            this.textBox_mysql_code.Size = new System.Drawing.Size(65, 21);
            this.textBox_mysql_code.TabIndex = 21;
            this.textBox_mysql_code.Text = "GB2312";
            this.textBox_mysql_code.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox_mysqlpassword
            // 
            this.textBox_mysqlpassword.Location = new System.Drawing.Point(89, 115);
            this.textBox_mysqlpassword.Name = "textBox_mysqlpassword";
            this.textBox_mysqlpassword.Size = new System.Drawing.Size(113, 21);
            this.textBox_mysqlpassword.TabIndex = 18;
            this.textBox_mysqlpassword.Text = "aionroy";
            this.textBox_mysqlpassword.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label21.Location = new System.Drawing.Point(7, 198);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(65, 12);
            this.label21.TabIndex = 16;
            this.label21.Text = "连接编码：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(18, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "用户密码：";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label8.Location = new System.Drawing.Point(18, 90);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 11;
            this.label8.Text = "连接端口：";
            // 
            // panel4
            // 
            this.panel4.AssociatedSplitter = null;
            this.panel4.BackColor = System.Drawing.Color.Transparent;
            this.panel4.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel4.CaptionHeight = 27;
            this.panel4.Controls.Add(this.labelEx2);
            this.panel4.Controls.Add(this.label13);
            this.panel4.Controls.Add(this.label11);
            this.panel4.Controls.Add(this.label17);
            this.panel4.Controls.Add(this.textBox分块图片长度);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.textBox图像压缩率);
            this.panel4.Controls.Add(this.label35);
            this.panel4.Controls.Add(this.textBox分块图片宽度);
            this.panel4.Controls.Add(this.label20);
            this.panel4.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel4.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel4.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel4.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel4.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel4.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel4.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel4.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel4.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel4.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel4.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel4.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel4.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel4.Image = null;
            this.panel4.Location = new System.Drawing.Point(487, 3);
            this.panel4.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(204, 288);
            this.panel4.TabIndex = 5;
            this.panel4.Text = "远程桌面参数";
            this.panel4.ToolTipTextCloseIcon = null;
            this.panel4.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel4.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // labelEx2
            // 
            this.labelEx2.ForeColor = System.Drawing.Color.Red;
            this.labelEx2.LineDistance = 5;
            this.labelEx2.Location = new System.Drawing.Point(13, 110);
            this.labelEx2.Name = "labelEx2";
            this.labelEx2.Size = new System.Drawing.Size(191, 182);
            this.labelEx2.TabIndex = 8;
            this.labelEx2.Text = "为减少带宽使用率，屏幕图像将分块并压缩后传输过来。图像质量越高容量越大，带宽占用越多，刷新频率越慢。同时分块图像的分辨率越小分块数量越多，处理越慢。但在屏幕图像局" +
    "部有变化时能迅速更新。反之分块图像越大在整个屏幕画面更新时候性能会更好。";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(13, 57);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(101, 12);
            this.label13.TabIndex = 7;
            this.label13.Text = "分块图像的长度：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(13, 82);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(101, 12);
            this.label11.TabIndex = 6;
            this.label11.Text = "分块图像的宽带：";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(168, 57);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(29, 12);
            this.label17.TabIndex = 7;
            this.label17.Text = "像素";
            // 
            // textBox分块图片长度
            // 
            this.textBox分块图片长度.Location = new System.Drawing.Point(119, 54);
            this.textBox分块图片长度.Name = "textBox分块图片长度";
            this.textBox分块图片长度.Size = new System.Drawing.Size(43, 21);
            this.textBox分块图片长度.TabIndex = 3;
            this.textBox分块图片长度.Text = "100";
            this.textBox分块图片长度.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "桌面图像的质量：";
            // 
            // textBox图像压缩率
            // 
            this.textBox图像压缩率.Location = new System.Drawing.Point(119, 30);
            this.textBox图像压缩率.Name = "textBox图像压缩率";
            this.textBox图像压缩率.Size = new System.Drawing.Size(43, 21);
            this.textBox图像压缩率.TabIndex = 4;
            this.textBox图像压缩率.Text = "30";
            this.textBox图像压缩率.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(168, 34);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(11, 12);
            this.label35.TabIndex = 7;
            this.label35.Text = "%";
            // 
            // textBox分块图片宽度
            // 
            this.textBox分块图片宽度.Location = new System.Drawing.Point(119, 79);
            this.textBox分块图片宽度.Name = "textBox分块图片宽度";
            this.textBox分块图片宽度.Size = new System.Drawing.Size(43, 21);
            this.textBox分块图片宽度.TabIndex = 2;
            this.textBox分块图片宽度.Text = "100";
            this.textBox分块图片宽度.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(168, 82);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(29, 12);
            this.label20.TabIndex = 7;
            this.label20.Text = "像素";
            // 
            // panel3
            // 
            this.panel3.AssociatedSplitter = null;
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel3.CaptionHeight = 27;
            this.panel3.Controls.Add(this.checkBox_close_mmzh);
            this.panel3.Controls.Add(this.labelEx1);
            this.panel3.Controls.Add(this.button4);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Controls.Add(this.textBox邮箱密码);
            this.panel3.Controls.Add(this.label12);
            this.panel3.Controls.Add(this.label16);
            this.panel3.Controls.Add(this.textBox邮箱STMP端口);
            this.panel3.Controls.Add(this.textBox邮箱STMP地址);
            this.panel3.Controls.Add(this.textBox发送邮箱);
            this.panel3.Controls.Add(this.label15);
            this.panel3.Controls.Add(this.label18);
            this.panel3.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel3.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel3.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel3.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel3.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel3.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel3.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel3.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel3.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel3.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel3.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel3.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel3.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel3.Image = null;
            this.panel3.Location = new System.Drawing.Point(216, 3);
            this.panel3.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(271, 288);
            this.panel3.TabIndex = 4;
            this.panel3.Text = "密码找回邮箱参数";
            this.panel3.ToolTipTextCloseIcon = null;
            this.panel3.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel3.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // checkBox_close_mmzh
            // 
            this.checkBox_close_mmzh.AutoSize = true;
            this.checkBox_close_mmzh.Location = new System.Drawing.Point(53, 256);
            this.checkBox_close_mmzh.Name = "checkBox_close_mmzh";
            this.checkBox_close_mmzh.Size = new System.Drawing.Size(180, 16);
            this.checkBox_close_mmzh.TabIndex = 51;
            this.checkBox_close_mmzh.Text = "勾选此处禁止登录器密码找回";
            this.checkBox_close_mmzh.UseVisualStyleBackColor = true;
            // 
            // labelEx1
            // 
            this.labelEx1.ForeColor = System.Drawing.Color.Red;
            this.labelEx1.LineDistance = 5;
            this.labelEx1.Location = new System.Drawing.Point(15, 192);
            this.labelEx1.Name = "labelEx1";
            this.labelEx1.Size = new System.Drawing.Size(252, 61);
            this.labelEx1.TabIndex = 50;
            this.labelEx1.Text = "玩家在登录器上找回密码时，新密码将通过邮件发送到玩家的注册邮箱中这里主要是配置邮件发送的邮箱：";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(86, 147);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(172, 42);
            this.button4.TabIndex = 49;
            this.button4.Text = "测试邮件发送";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.skinButton测试邮件发送_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 34);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(0, 12);
            this.label9.TabIndex = 0;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(21, 34);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 12);
            this.label12.TabIndex = 1;
            this.label12.Text = "发送邮箱：";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(21, 88);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(65, 12);
            this.label16.TabIndex = 1;
            this.label16.Text = "SMTP地址：";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(21, 61);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(65, 12);
            this.label15.TabIndex = 1;
            this.label15.Text = "邮箱密码：";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(21, 115);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(65, 12);
            this.label18.TabIndex = 1;
            this.label18.Text = "SMTP端口：";
            // 
            // panel2
            // 
            this.panel2.AssociatedSplitter = null;
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel2.CaptionHeight = 27;
            this.panel2.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel2.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel2.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel2.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel2.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel2.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel2.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel2.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel2.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel2.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel2.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel2.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel2.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel2.Image = null;
            this.panel2.Location = new System.Drawing.Point(216, 291);
            this.panel2.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(717, 154);
            this.panel2.TabIndex = 54;
            this.panel2.Text = "其他备用";
            this.panel2.ToolTipTextCloseIcon = null;
            this.panel2.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel2.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // panel1
            // 
            this.panel1.AssociatedSplitter = null;
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel1.CaptionHeight = 27;
            this.panel1.Controls.Add(this.checkBox开启双线双IP);
            this.panel1.Controls.Add(this.Button保存设置);
            this.panel1.Controls.Add(this.checkBox_检查到攻击自动封IP);
            this.panel1.Controls.Add(this.label39);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.checkBox_网关自动重启);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label30);
            this.panel1.Controls.Add(this.textBox下载端口);
            this.panel1.Controls.Add(this.textBox网关端口);
            this.panel1.Controls.Add(this.label31);
            this.panel1.Controls.Add(this.textBox网关重启时间);
            this.panel1.Controls.Add(this.checkBox_转发器自动重启);
            this.panel1.Controls.Add(this.textBox转发重启时间);
            this.panel1.Controls.Add(this.label19);
            this.panel1.Controls.Add(this.textBox网关IP);
            this.panel1.Controls.Add(this.textBox网关IP2);
            this.panel1.Controls.Add(this.checkBox开启通讯日志显示);
            this.panel1.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel1.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel1.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel1.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel1.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel1.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel1.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel1.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel1.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel1.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel1.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel1.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel1.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel1.Image = null;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(213, 442);
            this.panel1.TabIndex = 4;
            this.panel1.Text = "网络参数";
            this.panel1.ToolTipTextCloseIcon = null;
            this.panel1.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel1.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // checkBox开启双线双IP
            // 
            this.checkBox开启双线双IP.AutoSize = true;
            this.checkBox开启双线双IP.Location = new System.Drawing.Point(82, 85);
            this.checkBox开启双线双IP.Name = "checkBox开启双线双IP";
            this.checkBox开启双线双IP.Size = new System.Drawing.Size(120, 16);
            this.checkBox开启双线双IP.TabIndex = 42;
            this.checkBox开启双线双IP.Text = "开启双线双IP支持";
            this.checkBox开启双线双IP.UseVisualStyleBackColor = true;
            this.checkBox开启双线双IP.CheckedChanged += new System.EventHandler(this.checkBox开启双线双IP_CheckedChanged);
            this.checkBox开启双线双IP.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // Button保存设置
            // 
            this.Button保存设置.Location = new System.Drawing.Point(28, 329);
            this.Button保存设置.Name = "Button保存设置";
            this.Button保存设置.Size = new System.Drawing.Size(148, 84);
            this.Button保存设置.TabIndex = 44;
            this.Button保存设置.Text = "保存所有设置";
            this.Button保存设置.UseVisualStyleBackColor = true;
            this.Button保存设置.Click += new System.EventHandler(this.Button保存设置_Click);
            // 
            // checkBox_检查到攻击自动封IP
            // 
            this.checkBox_检查到攻击自动封IP.AutoSize = true;
            this.checkBox_检查到攻击自动封IP.Location = new System.Drawing.Point(14, 237);
            this.checkBox_检查到攻击自动封IP.Name = "checkBox_检查到攻击自动封IP";
            this.checkBox_检查到攻击自动封IP.Size = new System.Drawing.Size(132, 16);
            this.checkBox_检查到攻击自动封IP.TabIndex = 24;
            this.checkBox_检查到攻击自动封IP.Text = "检测到攻击自动封IP";
            this.checkBox_检查到攻击自动封IP.UseVisualStyleBackColor = true;
            this.checkBox_检查到攻击自动封IP.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(13, 137);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(65, 12);
            this.label39.TabIndex = 0;
            this.label39.Text = "下载端口：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "网关端口：";
            // 
            // checkBox_网关自动重启
            // 
            this.checkBox_网关自动重启.AutoSize = true;
            this.checkBox_网关自动重启.Location = new System.Drawing.Point(14, 183);
            this.checkBox_网关自动重启.Name = "checkBox_网关自动重启";
            this.checkBox_网关自动重启.Size = new System.Drawing.Size(96, 16);
            this.checkBox_网关自动重启.TabIndex = 18;
            this.checkBox_网关自动重启.Text = "自动重启网关";
            this.checkBox_网关自动重启.UseVisualStyleBackColor = true;
            this.checkBox_网关自动重启.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "网关地址：";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(152, 184);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(29, 12);
            this.label30.TabIndex = 20;
            this.label30.Text = "分钟";
            // 
            // textBox下载端口
            // 
            this.textBox下载端口.Location = new System.Drawing.Point(83, 134);
            this.textBox下载端口.Name = "textBox下载端口";
            this.textBox下载端口.Size = new System.Drawing.Size(48, 21);
            this.textBox下载端口.TabIndex = 1;
            this.textBox下载端口.Text = "8989";
            this.textBox下载端口.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox网关端口
            // 
            this.textBox网关端口.Location = new System.Drawing.Point(83, 107);
            this.textBox网关端口.Name = "textBox网关端口";
            this.textBox网关端口.Size = new System.Drawing.Size(48, 21);
            this.textBox网关端口.TabIndex = 1;
            this.textBox网关端口.Text = "10001";
            this.textBox网关端口.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(152, 211);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(29, 12);
            this.label31.TabIndex = 21;
            this.label31.Text = "分钟";
            // 
            // textBox网关重启时间
            // 
            this.textBox网关重启时间.Location = new System.Drawing.Point(116, 180);
            this.textBox网关重启时间.Name = "textBox网关重启时间";
            this.textBox网关重启时间.Size = new System.Drawing.Size(30, 21);
            this.textBox网关重启时间.TabIndex = 1;
            this.textBox网关重启时间.Text = "60";
            this.textBox网关重启时间.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // checkBox_转发器自动重启
            // 
            this.checkBox_转发器自动重启.AutoSize = true;
            this.checkBox_转发器自动重启.Location = new System.Drawing.Point(14, 210);
            this.checkBox_转发器自动重启.Name = "checkBox_转发器自动重启";
            this.checkBox_转发器自动重启.Size = new System.Drawing.Size(96, 16);
            this.checkBox_转发器自动重启.TabIndex = 19;
            this.checkBox_转发器自动重启.Text = "自动重启转发";
            this.checkBox_转发器自动重启.UseVisualStyleBackColor = true;
            this.checkBox_转发器自动重启.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox转发重启时间
            // 
            this.textBox转发重启时间.Location = new System.Drawing.Point(116, 208);
            this.textBox转发重启时间.Name = "textBox转发重启时间";
            this.textBox转发重启时间.Size = new System.Drawing.Size(30, 21);
            this.textBox转发重启时间.TabIndex = 1;
            this.textBox转发重启时间.Text = "15";
            this.textBox转发重启时间.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label19.Location = new System.Drawing.Point(12, 63);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 12);
            this.label19.TabIndex = 41;
            this.label19.Text = "第二个IP：";
            // 
            // textBox网关IP
            // 
            this.textBox网关IP.Location = new System.Drawing.Point(83, 31);
            this.textBox网关IP.Name = "textBox网关IP";
            this.textBox网关IP.Size = new System.Drawing.Size(113, 21);
            this.textBox网关IP.TabIndex = 1;
            this.textBox网关IP.Text = "192.168.31.145";
            this.textBox网关IP.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox网关IP2
            // 
            this.textBox网关IP2.Enabled = false;
            this.textBox网关IP2.Location = new System.Drawing.Point(83, 59);
            this.textBox网关IP2.Name = "textBox网关IP2";
            this.textBox网关IP2.Size = new System.Drawing.Size(113, 21);
            this.textBox网关IP2.TabIndex = 1;
            this.textBox网关IP2.Text = "192.168.31.145";
            this.textBox网关IP2.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // checkBox开启通讯日志显示
            // 
            this.checkBox开启通讯日志显示.AutoSize = true;
            this.checkBox开启通讯日志显示.Location = new System.Drawing.Point(14, 262);
            this.checkBox开启通讯日志显示.Name = "checkBox开启通讯日志显示";
            this.checkBox开启通讯日志显示.Size = new System.Drawing.Size(132, 16);
            this.checkBox开启通讯日志显示.TabIndex = 43;
            this.checkBox开启通讯日志显示.Text = "开启通讯包日志显示";
            this.checkBox开启通讯日志显示.UseVisualStyleBackColor = true;
            this.checkBox开启通讯日志显示.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel6);
            this.tabPage1.Controls.Add(this.panel5);
            this.tabPage1.Location = new System.Drawing.Point(4, 34);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(936, 448);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "登录器配置和生成";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel6
            // 
            this.panel6.AssociatedSplitter = null;
            this.panel6.BackColor = System.Drawing.Color.Transparent;
            this.panel6.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel6.CaptionHeight = 27;
            this.panel6.Controls.Add(this.checkBox_启用静态传送);
            this.panel6.Controls.Add(this.button7);
            this.panel6.Controls.Add(this.label25);
            this.panel6.Controls.Add(this.textBox登录器名字);
            this.panel6.Controls.Add(this.label42);
            this.panel6.Controls.Add(this.label41);
            this.panel6.Controls.Add(this.label23);
            this.panel6.Controls.Add(this.checkBox登录器可双开);
            this.panel6.Controls.Add(this.tabControl1);
            this.panel6.Controls.Add(this.textBox登录器WEB);
            this.panel6.Controls.Add(this.textBox登录器BIN64);
            this.panel6.Controls.Add(this.button5);
            this.panel6.Controls.Add(this.textBox登录器BIN32);
            this.panel6.Controls.Add(this.textBox登录器启动参数);
            this.panel6.Controls.Add(this.label22);
            this.panel6.Controls.Add(this.label24);
            this.panel6.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel6.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel6.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel6.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel6.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel6.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel6.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel6.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel6.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel6.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel6.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel6.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel6.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel6.Image = null;
            this.panel6.Location = new System.Drawing.Point(0, 113);
            this.panel6.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(936, 335);
            this.panel6.TabIndex = 6;
            this.panel6.Text = "登录器静态参数配置（当生成登录器时会永久写入到登录器内部）";
            this.panel6.ToolTipTextCloseIcon = null;
            this.panel6.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel6.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(784, 259);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(132, 66);
            this.button7.TabIndex = 50;
            this.button7.Text = "生成登录器";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.skinButton_生成登录器_Click);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label25.Location = new System.Drawing.Point(527, 42);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(53, 12);
            this.label25.TabIndex = 35;
            this.label25.Text = "小主页：";
            // 
            // textBox登录器名字
            // 
            this.textBox登录器名字.Location = new System.Drawing.Point(83, 42);
            this.textBox登录器名字.Name = "textBox登录器名字";
            this.textBox登录器名字.Size = new System.Drawing.Size(99, 21);
            this.textBox登录器名字.TabIndex = 39;
            this.textBox登录器名字.Text = "钟丽永恒";
            this.textBox登录器名字.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label42.Location = new System.Drawing.Point(398, 45);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(41, 12);
            this.label42.TabIndex = 41;
            this.label42.Text = "(64位)";
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label41.Location = new System.Drawing.Point(278, 45);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(41, 12);
            this.label41.TabIndex = 41;
            this.label41.Text = "(32位)";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label23.Location = new System.Drawing.Point(207, 45);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(65, 12);
            this.label23.TabIndex = 41;
            this.label23.Text = "启动目录：";
            // 
            // checkBox登录器可双开
            // 
            this.checkBox登录器可双开.AutoSize = true;
            this.checkBox登录器可双开.Location = new System.Drawing.Point(784, 237);
            this.checkBox登录器可双开.Name = "checkBox登录器可双开";
            this.checkBox登录器可双开.Size = new System.Drawing.Size(132, 16);
            this.checkBox登录器可双开.TabIndex = 46;
            this.checkBox登录器可双开.Text = "开启登录器双开支持";
            this.checkBox登录器可双开.UseVisualStyleBackColor = true;
            this.checkBox登录器可双开.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabPage8);
            this.tabControl1.Controls.Add(this.tabPage9);
            this.tabControl1.Controls.Add(this.tabPage10);
            this.tabControl1.HotTrack = true;
            this.tabControl1.ItemSize = new System.Drawing.Size(120, 28);
            this.tabControl1.Location = new System.Drawing.Point(14, 99);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(764, 230);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 44;
            this.tabControl1.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // tabPage8
            // 
            this.tabPage8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage8.Controls.Add(this.textBox目录限制);
            this.tabPage8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage8.Location = new System.Drawing.Point(4, 32);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(756, 194);
            this.tabPage8.TabIndex = 0;
            this.tabPage8.Text = "客户端目录文件限制";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // textBox目录限制
            // 
            this.textBox目录限制.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox目录限制.Location = new System.Drawing.Point(3, 3);
            this.textBox目录限制.Multiline = true;
            this.textBox目录限制.Name = "textBox目录限制";
            this.textBox目录限制.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox目录限制.Size = new System.Drawing.Size(748, 186);
            this.textBox目录限制.TabIndex = 0;
            this.textBox目录限制.Text = resources.GetString("textBox目录限制.Text");
            this.textBox目录限制.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // tabPage9
            // 
            this.tabPage9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage9.Controls.Add(this.textBox文件MD5);
            this.tabPage9.Controls.Add(this.button6);
            this.tabPage9.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage9.Location = new System.Drawing.Point(4, 32);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage9.Size = new System.Drawing.Size(756, 194);
            this.tabPage9.TabIndex = 1;
            this.tabPage9.Text = "客户端文件MD5检查";
            this.tabPage9.UseVisualStyleBackColor = true;
            // 
            // textBox文件MD5
            // 
            this.textBox文件MD5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox文件MD5.Location = new System.Drawing.Point(3, 3);
            this.textBox文件MD5.MaxLength = 65536;
            this.textBox文件MD5.Multiline = true;
            this.textBox文件MD5.Name = "textBox文件MD5";
            this.textBox文件MD5.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox文件MD5.Size = new System.Drawing.Size(688, 186);
            this.textBox文件MD5.TabIndex = 1;
            // 
            // button6
            // 
            this.button6.Dock = System.Windows.Forms.DockStyle.Right;
            this.button6.Location = new System.Drawing.Point(691, 3);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(60, 186);
            this.button6.TabIndex = 49;
            this.button6.Text = "客户端文件MD5生成";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.skinButton客户端文件MD5生成_Click);
            // 
            // tabPage10
            // 
            this.tabPage10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage10.Controls.Add(this.checkBox查到外挂关闭);
            this.tabPage10.Controls.Add(this.textBox外挂进程);
            this.tabPage10.Location = new System.Drawing.Point(4, 32);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage10.Size = new System.Drawing.Size(756, 194);
            this.tabPage10.TabIndex = 2;
            this.tabPage10.Text = "外挂进程查杀";
            this.tabPage10.UseVisualStyleBackColor = true;
            // 
            // checkBox查到外挂关闭
            // 
            this.checkBox查到外挂关闭.AutoSize = true;
            this.checkBox查到外挂关闭.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox查到外挂关闭.Location = new System.Drawing.Point(687, 3);
            this.checkBox查到外挂关闭.Name = "checkBox查到外挂关闭";
            this.checkBox查到外挂关闭.Size = new System.Drawing.Size(64, 186);
            this.checkBox查到外挂关闭.TabIndex = 2;
            this.checkBox查到外挂关闭.Text = "检测到外挂关客户端";
            this.checkBox查到外挂关闭.UseVisualStyleBackColor = true;
            // 
            // textBox外挂进程
            // 
            this.textBox外挂进程.Dock = System.Windows.Forms.DockStyle.Left;
            this.textBox外挂进程.Location = new System.Drawing.Point(3, 3);
            this.textBox外挂进程.Multiline = true;
            this.textBox外挂进程.Name = "textBox外挂进程";
            this.textBox外挂进程.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox外挂进程.Size = new System.Drawing.Size(684, 186);
            this.textBox外挂进程.TabIndex = 1;
            this.textBox外挂进程.Text = "EXENAME=calc.exe\r\nEXEMD5=1234567890ABCDE\r\nEXECLASS=WTWindows";
            this.textBox外挂进程.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox登录器WEB
            // 
            this.textBox登录器WEB.Location = new System.Drawing.Point(598, 39);
            this.textBox登录器WEB.Name = "textBox登录器WEB";
            this.textBox登录器WEB.Size = new System.Drawing.Size(217, 21);
            this.textBox登录器WEB.TabIndex = 36;
            this.textBox登录器WEB.Text = "http://www.zooli.com";
            this.textBox登录器WEB.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox登录器BIN64
            // 
            this.textBox登录器BIN64.Location = new System.Drawing.Point(445, 42);
            this.textBox登录器BIN64.Name = "textBox登录器BIN64";
            this.textBox登录器BIN64.Size = new System.Drawing.Size(66, 21);
            this.textBox登录器BIN64.TabIndex = 38;
            this.textBox登录器BIN64.Text = "bin32";
            this.textBox登录器BIN64.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(529, 70);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(132, 23);
            this.button5.TabIndex = 48;
            this.button5.Text = "恢复默认启动参数";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.skinButton_恢复参数_Click);
            // 
            // textBox登录器BIN32
            // 
            this.textBox登录器BIN32.Location = new System.Drawing.Point(325, 42);
            this.textBox登录器BIN32.Name = "textBox登录器BIN32";
            this.textBox登录器BIN32.Size = new System.Drawing.Size(66, 21);
            this.textBox登录器BIN32.TabIndex = 38;
            this.textBox登录器BIN32.Text = "bin32";
            this.textBox登录器BIN32.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox登录器启动参数
            // 
            this.textBox登录器启动参数.Location = new System.Drawing.Point(83, 72);
            this.textBox登录器启动参数.Name = "textBox登录器启动参数";
            this.textBox登录器启动参数.Size = new System.Drawing.Size(428, 21);
            this.textBox登录器启动参数.TabIndex = 37;
            this.textBox登录器启动参数.Text = "-cc:5 -lang:chs -noweb -megaphone -multithread";
            this.textBox登录器启动参数.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label22.Location = new System.Drawing.Point(12, 75);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(65, 12);
            this.label22.TabIndex = 40;
            this.label22.Text = "启动参数：";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label24.Location = new System.Drawing.Point(12, 45);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(65, 12);
            this.label24.TabIndex = 42;
            this.label24.Text = "登录器名：";
            // 
            // panel5
            // 
            this.panel5.AssociatedSplitter = null;
            this.panel5.BackColor = System.Drawing.Color.Transparent;
            this.panel5.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel5.CaptionHeight = 27;
            this.panel5.Controls.Add(this.checkBox禁止提前登录);
            this.panel5.Controls.Add(this.button8);
            this.panel5.Controls.Add(this.button2);
            this.panel5.Controls.Add(this.label34);
            this.panel5.Controls.Add(this.label40);
            this.panel5.Controls.Add(this.label28);
            this.panel5.Controls.Add(this.textBox_updateUrl);
            this.panel5.Controls.Add(this.textBox转发密码);
            this.panel5.Controls.Add(this.textBox账号服务器LS端口);
            this.panel5.Controls.Add(this.textBox_patch_url);
            this.panel5.Controls.Add(this.label33);
            this.panel5.Controls.Add(this.textBox_launcherMD5);
            this.panel5.Controls.Add(this.button1);
            this.panel5.Controls.Add(this.label32);
            this.panel5.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel5.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel5.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel5.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel5.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel5.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel5.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel5.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel5.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel5.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel5.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel5.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel5.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel5.Image = null;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(936, 113);
            this.panel5.TabIndex = 5;
            this.panel5.Text = "登录器动态参数配置（当登录器连接网关时会读取这里的参数）";
            this.panel5.ToolTipTextCloseIcon = null;
            this.panel5.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel5.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(841, 68);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 31);
            this.button8.TabIndex = 49;
            this.button8.Text = "保存设置";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.Button保存设置_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(422, 72);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(45, 21);
            this.button2.TabIndex = 48;
            this.button2.Text = "打包";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.skinButton补丁打包_Click);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label34.Location = new System.Drawing.Point(16, 48);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(65, 12);
            this.label34.TabIndex = 37;
            this.label34.Text = "下载地址：";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label40.Location = new System.Drawing.Point(612, 77);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(65, 12);
            this.label40.TabIndex = 45;
            this.label40.Text = "通讯密匙：";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label28.Location = new System.Drawing.Point(489, 77);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(65, 12);
            this.label28.TabIndex = 45;
            this.label28.Text = "登录端口：";
            // 
            // textBox_updateUrl
            // 
            this.textBox_updateUrl.Location = new System.Drawing.Point(83, 44);
            this.textBox_updateUrl.Name = "textBox_updateUrl";
            this.textBox_updateUrl.Size = new System.Drawing.Size(384, 21);
            this.textBox_updateUrl.TabIndex = 39;
            this.textBox_updateUrl.Text = "http://www.zooli.com/钟丽永恒.exe";
            this.textBox_updateUrl.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // textBox_patch_url
            // 
            this.textBox_patch_url.Location = new System.Drawing.Point(83, 73);
            this.textBox_patch_url.Name = "textBox_patch_url";
            this.textBox_patch_url.Size = new System.Drawing.Size(333, 21);
            this.textBox_patch_url.TabIndex = 42;
            this.textBox_patch_url.Text = "http://www.zooli.com/update/update.dat";
            this.textBox_patch_url.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label33.Location = new System.Drawing.Point(483, 48);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(71, 12);
            this.label33.TabIndex = 36;
            this.label33.Text = "登录器MD5：";
            // 
            // textBox_launcherMD5
            // 
            this.textBox_launcherMD5.AllowDrop = true;
            this.textBox_launcherMD5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_launcherMD5.Location = new System.Drawing.Point(560, 46);
            this.textBox_launcherMD5.Name = "textBox_launcherMD5";
            this.textBox_launcherMD5.ReadOnly = true;
            this.textBox_launcherMD5.Size = new System.Drawing.Size(255, 21);
            this.textBox_launcherMD5.TabIndex = 46;
            this.textBox_launcherMD5.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox_launcherMD5_DragDrop);
            this.textBox_launcherMD5.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox_launcherMD5_DragEnter);
            this.textBox_launcherMD5.Leave += new System.EventHandler(this.修改自动保存);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(841, 41);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 22);
            this.button1.TabIndex = 47;
            this.button1.Text = "清除MD5";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.skinButton清除MD5_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label32.Location = new System.Drawing.Point(16, 77);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(65, 12);
            this.label32.TabIndex = 41;
            this.label32.Text = "补丁地址：";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel8);
            this.tabPage2.Controls.Add(this.panel7);
            this.tabPage2.Location = new System.Drawing.Point(4, 34);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(936, 448);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Text = "军团在线统计";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            this.panel8.AssociatedSplitter = null;
            this.panel8.BackColor = System.Drawing.Color.Transparent;
            this.panel8.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel8.CaptionHeight = 27;
            this.panel8.Controls.Add(this.dataGridView1);
            this.panel8.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel8.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel8.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel8.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel8.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel8.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel8.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel8.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel8.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel8.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel8.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel8.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel8.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel8.Image = null;
            this.panel8.Location = new System.Drawing.Point(3, 3);
            this.panel8.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(821, 442);
            this.panel8.TabIndex = 6;
            this.panel8.Text = "军团统计列表";
            this.panel8.ToolTipTextCloseIcon = null;
            this.panel8.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel8.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // dataGridView1
            // 
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeight = 30;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(1, 28);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 20;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridView1.Size = new System.Drawing.Size(819, 413);
            this.dataGridView1.TabIndex = 0;
            // 
            // panel7
            // 
            this.panel7.AssociatedSplitter = null;
            this.panel7.BackColor = System.Drawing.Color.Transparent;
            this.panel7.CaptionFont = new System.Drawing.Font("Microsoft YaHei UI", 11.75F, System.Drawing.FontStyle.Bold);
            this.panel7.CaptionHeight = 27;
            this.panel7.Controls.Add(this.label38);
            this.panel7.Controls.Add(this.label37);
            this.panel7.Controls.Add(this.label36);
            this.panel7.Controls.Add(this.textBox_平均统计几次);
            this.panel7.Controls.Add(this.button_停止军团统计);
            this.panel7.Controls.Add(this.comboBox_结束统计);
            this.panel7.Controls.Add(this.comboBox_开始统计);
            this.panel7.Controls.Add(this.button_开启军团统计);
            this.panel7.Controls.Add(this.label29);
            this.panel7.Controls.Add(this.textBox_统计间隔);
            this.panel7.Controls.Add(this.label27);
            this.panel7.Controls.Add(this.label26);
            this.panel7.Controls.Add(this.label14);
            this.panel7.Controls.Add(this.button_保存军团设置);
            this.panel7.CustomColors.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(184)))), ((int)(((byte)(184)))));
            this.panel7.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
            this.panel7.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
            this.panel7.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel7.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
            this.panel7.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panel7.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel7.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(215)))), ((int)(((byte)(243)))));
            this.panel7.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
            this.panel7.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
            this.panel7.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
            this.panel7.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panel7.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
            this.panel7.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel7.Image = null;
            this.panel7.Location = new System.Drawing.Point(824, 3);
            this.panel7.MinimumSize = new System.Drawing.Size(27, 27);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(109, 442);
            this.panel7.TabIndex = 6;
            this.panel7.Text = "统计设置";
            this.panel7.ToolTipTextCloseIcon = null;
            this.panel7.ToolTipTextExpandIconPanelCollapsed = null;
            this.panel7.ToolTipTextExpandIconPanelExpanded = null;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(12, 228);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(77, 12);
            this.label38.TabIndex = 13;
            this.label38.Text = "均在线人数。";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(50, 207);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(41, 12);
            this.label37.TabIndex = 13;
            this.label37.Text = "次的平";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(13, 189);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(77, 12);
            this.label36.TabIndex = 12;
            this.label36.Text = "自动计算最近";
            // 
            // textBox_平均统计几次
            // 
            this.textBox_平均统计几次.Location = new System.Drawing.Point(15, 204);
            this.textBox_平均统计几次.Name = "textBox_平均统计几次";
            this.textBox_平均统计几次.Size = new System.Drawing.Size(35, 21);
            this.textBox_平均统计几次.TabIndex = 11;
            this.textBox_平均统计几次.Text = "5";
            // 
            // button_停止军团统计
            // 
            this.button_停止军团统计.Enabled = false;
            this.button_停止军团统计.ForeColor = System.Drawing.Color.Red;
            this.button_停止军团统计.Location = new System.Drawing.Point(15, 366);
            this.button_停止军团统计.Name = "button_停止军团统计";
            this.button_停止军团统计.Size = new System.Drawing.Size(84, 43);
            this.button_停止军团统计.TabIndex = 10;
            this.button_停止军团统计.Text = "停止统计";
            this.button_停止军团统计.UseVisualStyleBackColor = true;
            this.button_停止军团统计.Click += new System.EventHandler(this.button_停止军团统计_Click);
            // 
            // comboBox_结束统计
            // 
            this.comboBox_结束统计.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_结束统计.FormatString = "t";
            this.comboBox_结束统计.FormattingEnabled = true;
            this.comboBox_结束统计.Items.AddRange(new object[] {
            "00:00",
            "01:00",
            "02:00",
            "03:00",
            "04:00",
            "05:00",
            "06:00",
            "07:00",
            "08:00",
            "09:00",
            "10:00",
            "11:00",
            "12:00",
            "13:00",
            "14:00",
            "15:00",
            "16:00",
            "17:00",
            "18:00",
            "19:00",
            "20:00",
            "21:00",
            "22:00",
            "23:00"});
            this.comboBox_结束统计.Location = new System.Drawing.Point(15, 98);
            this.comboBox_结束统计.Name = "comboBox_结束统计";
            this.comboBox_结束统计.Size = new System.Drawing.Size(84, 20);
            this.comboBox_结束统计.TabIndex = 9;
            // 
            // comboBox_开始统计
            // 
            this.comboBox_开始统计.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_开始统计.FormatString = "t";
            this.comboBox_开始统计.FormattingEnabled = true;
            this.comboBox_开始统计.Items.AddRange(new object[] {
            "00:00",
            "01:00",
            "02:00",
            "03:00",
            "04:00",
            "05:00",
            "06:00",
            "07:00",
            "08:00",
            "09:00",
            "10:00",
            "11:00",
            "12:00",
            "13:00",
            "14:00",
            "15:00",
            "16:00",
            "17:00",
            "18:00",
            "19:00",
            "20:00",
            "21:00",
            "22:00",
            "23:00"});
            this.comboBox_开始统计.Location = new System.Drawing.Point(15, 50);
            this.comboBox_开始统计.Name = "comboBox_开始统计";
            this.comboBox_开始统计.Size = new System.Drawing.Size(84, 20);
            this.comboBox_开始统计.TabIndex = 9;
            // 
            // button_开启军团统计
            // 
            this.button_开启军团统计.ForeColor = System.Drawing.Color.Green;
            this.button_开启军团统计.Location = new System.Drawing.Point(14, 318);
            this.button_开启军团统计.Name = "button_开启军团统计";
            this.button_开启军团统计.Size = new System.Drawing.Size(85, 42);
            this.button_开启军团统计.TabIndex = 8;
            this.button_开启军团统计.Text = "启用统计";
            this.button_开启军团统计.UseVisualStyleBackColor = true;
            this.button_开启军团统计.Click += new System.EventHandler(this.button_统计军团_Click);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(13, 162);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(89, 12);
            this.label29.TabIndex = 7;
            this.label29.Text = "分钟统计一次。";
            // 
            // textBox_统计间隔
            // 
            this.textBox_统计间隔.Location = new System.Drawing.Point(47, 132);
            this.textBox_统计间隔.Name = "textBox_统计间隔";
            this.textBox_统计间隔.Size = new System.Drawing.Size(52, 21);
            this.textBox_统计间隔.TabIndex = 6;
            this.textBox_统计间隔.Text = "10";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(12, 135);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(29, 12);
            this.label27.TabIndex = 5;
            this.label27.Text = "每隔";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(13, 83);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(89, 12);
            this.label26.TabIndex = 3;
            this.label26.Text = "结束统计时间：";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(13, 35);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(89, 12);
            this.label14.TabIndex = 1;
            this.label14.Text = "开始统计时间：";
            // 
            // button_保存军团设置
            // 
            this.button_保存军团设置.Location = new System.Drawing.Point(14, 249);
            this.button_保存军团设置.Name = "button_保存军团设置";
            this.button_保存军团设置.Size = new System.Drawing.Size(85, 37);
            this.button_保存军团设置.TabIndex = 0;
            this.button_保存军团设置.Text = "保持设置";
            this.button_保存军团设置.UseVisualStyleBackColor = true;
            this.button_保存军团设置.Click += new System.EventHandler(this.Button保存设置_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 34);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(936, 448);
            this.tabPage3.TabIndex = 5;
            this.tabPage3.Text = "推广系统";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // timer_checkOnline
            // 
            this.timer_checkOnline.Interval = 30000;
            this.timer_checkOnline.Tick += new System.EventHandler(this.timer_checkOnline_Tick);
            // 
            // timerRestart
            // 
            this.timerRestart.Tick += new System.EventHandler(this.timerRestart_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 532);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "钟丽网关V5.5";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage8.ResumeLayout(false);
            this.tabPage8.PerformLayout();
            this.tabPage9.ResumeLayout(false);
            this.tabPage9.PerformLayout();
            this.tabPage10.ResumeLayout(false);
            this.tabPage10.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件FToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 导入配置IToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 导出配置OToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出程序EToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 控制CToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 启动服务QToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 停止服务TToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关于AToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 注册软件RToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关于作者AToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.RichTextBox richTextBox_log;
        private System.Windows.Forms.ListView listView_online;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Timer timer_main;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel state_stat;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel state_run_time;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel state_cpu_useage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.ToolStripStatusLabel state_memory;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel8;
        private System.Windows.Forms.ToolStripStatusLabel state_online;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel10;
        private System.Windows.Forms.ToolStripStatusLabel state_online_top;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 查看客户端电脑信息ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看客户端桌面图像ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看客户端进程列表ToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBox发送邮箱;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBox邮箱密码;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBox邮箱STMP地址;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox textBox邮箱STMP端口;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_mysqlport;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.CheckBox checkBox开启通讯日志显示;
        private System.Windows.Forms.TextBox textBox_mysqlpassword;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_mysqluser;
        private System.Windows.Forms.TextBox textBox_launcherMD5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_mysqlurl;
        private System.Windows.Forms.TextBox textBox分块图片宽度;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox分块图片长度;
        private System.Windows.Forms.TextBox textBox_mysql_db_ls;
        private System.Windows.Forms.TextBox textBox_mysql_code;
        private System.Windows.Forms.TextBox textBox_mysql_db_gs;
        private System.Windows.Forms.CheckBox checkBox开启双线双IP;
        private System.Windows.Forms.TextBox textBox图像压缩率;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBox账号服务器LS端口;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.CheckBox checkBox_检查到攻击自动封IP;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.CheckBox checkBox登录器可双开;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox checkBox_网关自动重启;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.CheckBox checkBox_转发器自动重启;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.TextBox textBox目录限制;
        private System.Windows.Forms.TabPage tabPage9;
        private System.Windows.Forms.TextBox textBox文件MD5;
        private System.Windows.Forms.TabPage tabPage10;
        private System.Windows.Forms.TextBox textBox外挂进程;
        private System.Windows.Forms.TextBox textBox网关IP2;
        private System.Windows.Forms.TextBox textBox网关IP;
        private System.Windows.Forms.TextBox textBox转发重启时间;
        private System.Windows.Forms.TextBox textBox登录器启动参数;
        private System.Windows.Forms.TextBox textBox网关重启时间;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox textBox网关端口;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox登录器BIN32;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox textBox登录器名字;
        private System.Windows.Forms.TextBox textBox_patch_url;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox textBox_updateUrl;
        private System.Windows.Forms.TextBox textBox登录器WEB;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button Button保存设置;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TabPage tabPage1;
        private AionCommons.WinForm.LabelEx labelEx1;
        private AionCommons.WinForm.LabelEx labelEx2;

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button button8;
        private BSE.Windows.Forms.Panel panel1;
        private BSE.Windows.Forms.Panel panel2;
        private BSE.Windows.Forms.Panel panel3;
        private BSE.Windows.Forms.Panel panel4;
        private BSE.Windows.Forms.Panel panel5;
        private BSE.Windows.Forms.Panel panel6;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.RadioButton radioButton_MSSQL;
        private System.Windows.Forms.RadioButton radioButton_MySQL;
        private System.Windows.Forms.CheckBox checkBox_启用静态传送;
        private System.Windows.Forms.TabPage tabPage2;
        private BSE.Windows.Forms.Panel panel8;
        private BSE.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Button button_保存军团设置;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button button_开启军团统计;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox textBox_统计间隔;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox comboBox_结束统计;
        private System.Windows.Forms.ComboBox comboBox_开始统计;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button_停止军团统计;
        private System.Windows.Forms.TextBox textBox_平均统计几次;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private BSE.Windows.Forms.Panel panel9;
        private System.Windows.Forms.ToolStripMenuItem 查看玩家电脑硬盘ToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox_close_mmzh;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.TextBox textBox下载端口;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.TextBox textBox转发密码;
        private System.Windows.Forms.CheckBox checkBox查到外挂关闭;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 禁止选中的玩家登陆ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 允许选中的玩家登陆ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看客户端系统服务ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看客户端注册表项ToolStripMenuItem1;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.TextBox textBox登录器BIN64;
        private System.Windows.Forms.CheckBox checkBox禁止提前登录;
        private System.Windows.Forms.Timer timer_checkOnline;
        private System.Windows.Forms.Timer timerRestart;
        private System.Windows.Forms.ToolStripMenuItem 注册网关ToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox_NewDatabase;
    }
}
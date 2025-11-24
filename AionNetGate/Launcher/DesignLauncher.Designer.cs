namespace AionNetGate.Launcher
{
    partial class DesignLauncher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DesignLauncher));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.生成加密登录器ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.还原登录器文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.skinSplitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel_背景 = new System.Windows.Forms.Panel();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.mButton_帐号管理 = new AionNetGate.Launcher.MButton();
            this.label_帐号管理 = new System.Windows.Forms.Label();
            this.mButton_关闭按钮 = new AionNetGate.Launcher.MButton();
            this.mButton_启动游戏 = new AionNetGate.Launcher.MButton();
            this.label_登录器名字 = new System.Windows.Forms.Label();
            this.PictureBox_Online = new System.Windows.Forms.PictureBox();
            this.panel区块 = new System.Windows.Forms.Panel();
            this.skinProgressBar1 = new System.Windows.Forms.ProgressBar();
            this.label_速度 = new System.Windows.Forms.Label();
            this.label_下载 = new System.Windows.Forms.Label();
            this.label_信息 = new System.Windows.Forms.Label();
            this.label_进度文字 = new System.Windows.Forms.Label();
            this.label_服务器状态 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.skinButton生成 = new System.Windows.Forms.Button();
            this.skinButton保存设计 = new System.Windows.Forms.Button();
            this.skinButton打开设计 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.checkBox_显示网页 = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.skinSplitContainer1.Panel1.SuspendLayout();
            this.skinSplitContainer1.Panel2.SuspendLayout();
            this.skinSplitContainer1.SuspendLayout();
            this.panel_背景.SuspendLayout();
            this.mButton_帐号管理.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_Online)).BeginInit();
            this.panel区块.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.生成加密登录器ToolStripMenuItem,
            this.还原登录器文件ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 70);
            // 
            // 生成加密登录器ToolStripMenuItem
            // 
            this.生成加密登录器ToolStripMenuItem.Name = "生成加密登录器ToolStripMenuItem";
            this.生成加密登录器ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.生成加密登录器ToolStripMenuItem.Text = "生成加密登录器";
            this.生成加密登录器ToolStripMenuItem.Click += new System.EventHandler(this.生成加密登录器ToolStripMenuItem_Click);
            // 
            // 还原登录器文件ToolStripMenuItem
            // 
            this.还原登录器文件ToolStripMenuItem.Enabled = false;
            this.还原登录器文件ToolStripMenuItem.Name = "还原登录器文件ToolStripMenuItem";
            this.还原登录器文件ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.还原登录器文件ToolStripMenuItem.Text = "还原登录器文件";
            this.还原登录器文件ToolStripMenuItem.Click += new System.EventHandler(this.还原登录器文件ToolStripMenuItem_Click);
            // 
            // skinSplitContainer1
            // 
            this.skinSplitContainer1.Cursor = System.Windows.Forms.Cursors.Default;
            this.skinSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skinSplitContainer1.Location = new System.Drawing.Point(0, 0);
            this.skinSplitContainer1.Name = "skinSplitContainer1";
            // 
            // skinSplitContainer1.Panel1
            // 
            this.skinSplitContainer1.Panel1.Controls.Add(this.panel_背景);
            this.skinSplitContainer1.Panel1MinSize = 800;
            // 
            // skinSplitContainer1.Panel2
            // 
            this.skinSplitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.skinSplitContainer1.Size = new System.Drawing.Size(1086, 609);
            this.skinSplitContainer1.SplitterDistance = 800;
            this.skinSplitContainer1.SplitterWidth = 5;
            this.skinSplitContainer1.TabIndex = 4;
            // 
            // panel_背景
            // 
            this.panel_背景.BackColor = System.Drawing.Color.Transparent;
            this.panel_背景.BackgroundImage = global::AionNetGate.Properties.Resources.背景;
            this.panel_背景.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_背景.Controls.Add(this.webBrowser1);
            this.panel_背景.Controls.Add(this.mButton_帐号管理);
            this.panel_背景.Controls.Add(this.mButton_关闭按钮);
            this.panel_背景.Controls.Add(this.mButton_启动游戏);
            this.panel_背景.Controls.Add(this.label_登录器名字);
            this.panel_背景.Controls.Add(this.PictureBox_Online);
            this.panel_背景.Controls.Add(this.panel区块);
            this.panel_背景.Controls.Add(this.label_服务器状态);
            this.panel_背景.Location = new System.Drawing.Point(3, 4);
            this.panel_背景.MinimumSize = new System.Drawing.Size(800, 600);
            this.panel_背景.Name = "panel_背景";
            this.panel_背景.Size = new System.Drawing.Size(800, 600);
            this.panel_背景.TabIndex = 1;
            this.panel_背景.BackgroundImageChanged += new System.EventHandler(this.panel_背景_BackgroundImageChanged);
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(13, 38);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(775, 414);
            this.webBrowser1.TabIndex = 39;
            this.webBrowser1.Visible = false;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
            // 
            // mButton_帐号管理
            // 
            this.mButton_帐号管理.BackColor = System.Drawing.Color.Transparent;
            this.mButton_帐号管理.BackgroundImage = global::AionNetGate.Properties.Resources.修改密码1;
            this.mButton_帐号管理.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.mButton_帐号管理.Controls.Add(this.label_帐号管理);
            this.mButton_帐号管理.DownImage = global::AionNetGate.Properties.Resources.修改密码3;
            this.mButton_帐号管理.Location = new System.Drawing.Point(646, 474);
            this.mButton_帐号管理.MoveImage = global::AionNetGate.Properties.Resources.修改密码2;
            this.mButton_帐号管理.Name = "mButton_帐号管理";
            this.mButton_帐号管理.NormalImage = global::AionNetGate.Properties.Resources.修改密码1;
            this.mButton_帐号管理.Size = new System.Drawing.Size(83, 33);
            this.mButton_帐号管理.TabIndex = 47;
            // 
            // label_帐号管理
            // 
            this.label_帐号管理.AutoSize = true;
            this.label_帐号管理.Location = new System.Drawing.Point(16, 9);
            this.label_帐号管理.Name = "label_帐号管理";
            this.label_帐号管理.Size = new System.Drawing.Size(53, 12);
            this.label_帐号管理.TabIndex = 0;
            this.label_帐号管理.Text = "帐号管理";
            // 
            // mButton_关闭按钮
            // 
            this.mButton_关闭按钮.BackColor = System.Drawing.Color.Transparent;
            this.mButton_关闭按钮.BackgroundImage = global::AionNetGate.Properties.Resources.关闭常规;
            this.mButton_关闭按钮.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.mButton_关闭按钮.DownImage = global::AionNetGate.Properties.Resources.关闭按下;
            this.mButton_关闭按钮.Location = new System.Drawing.Point(741, 3);
            this.mButton_关闭按钮.MoveImage = global::AionNetGate.Properties.Resources.关闭经过;
            this.mButton_关闭按钮.Name = "mButton_关闭按钮";
            this.mButton_关闭按钮.NormalImage = global::AionNetGate.Properties.Resources.关闭常规;
            this.mButton_关闭按钮.Size = new System.Drawing.Size(35, 28);
            this.mButton_关闭按钮.TabIndex = 46;
            // 
            // mButton_启动游戏
            // 
            this.mButton_启动游戏.BackColor = System.Drawing.Color.Transparent;
            this.mButton_启动游戏.BackgroundImage = global::AionNetGate.Properties.Resources.启动常规;
            this.mButton_启动游戏.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.mButton_启动游戏.DownImage = global::AionNetGate.Properties.Resources.启动按下;
            this.mButton_启动游戏.Location = new System.Drawing.Point(597, 509);
            this.mButton_启动游戏.MoveImage = global::AionNetGate.Properties.Resources.启动经过;
            this.mButton_启动游戏.Name = "mButton_启动游戏";
            this.mButton_启动游戏.NormalImage = global::AionNetGate.Properties.Resources.启动常规;
            this.mButton_启动游戏.Size = new System.Drawing.Size(179, 55);
            this.mButton_启动游戏.TabIndex = 45;
            // 
            // label_登录器名字
            // 
            this.label_登录器名字.AutoSize = true;
            this.label_登录器名字.BackColor = System.Drawing.Color.Transparent;
            this.label_登录器名字.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_登录器名字.ForeColor = System.Drawing.Color.LightGray;
            this.label_登录器名字.Location = new System.Drawing.Point(16, 10);
            this.label_登录器名字.Name = "label_登录器名字";
            this.label_登录器名字.Size = new System.Drawing.Size(40, 16);
            this.label_登录器名字.TabIndex = 35;
            this.label_登录器名字.Text = "名称";
            // 
            // PictureBox_Online
            // 
            this.PictureBox_Online.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox_Online.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PictureBox_Online.Location = new System.Drawing.Point(701, 571);
            this.PictureBox_Online.Name = "PictureBox_Online";
            this.PictureBox_Online.Size = new System.Drawing.Size(18, 18);
            this.PictureBox_Online.TabIndex = 38;
            this.PictureBox_Online.TabStop = false;
            // 
            // panel区块
            // 
            this.panel区块.BackColor = System.Drawing.Color.Transparent;
            this.panel区块.BackgroundImage = global::AionNetGate.Properties.Resources.进度;
            this.panel区块.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel区块.Controls.Add(this.skinProgressBar1);
            this.panel区块.Controls.Add(this.label_速度);
            this.panel区块.Controls.Add(this.label_下载);
            this.panel区块.Controls.Add(this.label_信息);
            this.panel区块.Controls.Add(this.label_进度文字);
            this.panel区块.Location = new System.Drawing.Point(21, 505);
            this.panel区块.Name = "panel区块";
            this.panel区块.Size = new System.Drawing.Size(568, 74);
            this.panel区块.TabIndex = 36;
            // 
            // skinProgressBar1
            // 
            this.skinProgressBar1.ForeColor = System.Drawing.Color.DarkOliveGreen;
            this.skinProgressBar1.Location = new System.Drawing.Point(76, 30);
            this.skinProgressBar1.Name = "skinProgressBar1";
            this.skinProgressBar1.Size = new System.Drawing.Size(449, 10);
            this.skinProgressBar1.TabIndex = 35;
            this.skinProgressBar1.Value = 100;
            // 
            // label_速度
            // 
            this.label_速度.AutoSize = true;
            this.label_速度.BackColor = System.Drawing.Color.Transparent;
            this.label_速度.Font = new System.Drawing.Font("宋体", 9F);
            this.label_速度.ForeColor = System.Drawing.Color.Silver;
            this.label_速度.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_速度.Location = new System.Drawing.Point(436, 47);
            this.label_速度.Name = "label_速度";
            this.label_速度.Size = new System.Drawing.Size(119, 12);
            this.label_速度.TabIndex = 34;
            this.label_速度.Text = "速度：468 kb/s     ";
            // 
            // label_下载
            // 
            this.label_下载.AutoSize = true;
            this.label_下载.BackColor = System.Drawing.Color.Transparent;
            this.label_下载.Font = new System.Drawing.Font("宋体", 9F);
            this.label_下载.ForeColor = System.Drawing.Color.Silver;
            this.label_下载.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_下载.Location = new System.Drawing.Point(74, 46);
            this.label_下载.Name = "label_下载";
            this.label_下载.Size = new System.Drawing.Size(65, 12);
            this.label_下载.TabIndex = 33;
            this.label_下载.Text = "下载：1/15";
            // 
            // label_信息
            // 
            this.label_信息.AutoSize = true;
            this.label_信息.BackColor = System.Drawing.Color.Transparent;
            this.label_信息.Font = new System.Drawing.Font("宋体", 9F);
            this.label_信息.ForeColor = System.Drawing.Color.Silver;
            this.label_信息.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_信息.Location = new System.Drawing.Point(74, 12);
            this.label_信息.Name = "label_信息";
            this.label_信息.Size = new System.Drawing.Size(53, 12);
            this.label_信息.TabIndex = 33;
            this.label_信息.Text = "信息显示";
            // 
            // label_进度文字
            // 
            this.label_进度文字.AutoSize = true;
            this.label_进度文字.BackColor = System.Drawing.Color.Transparent;
            this.label_进度文字.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label_进度文字.ForeColor = System.Drawing.Color.Silver;
            this.label_进度文字.Location = new System.Drawing.Point(29, 29);
            this.label_进度文字.Name = "label_进度文字";
            this.label_进度文字.Size = new System.Drawing.Size(35, 14);
            this.label_进度文字.TabIndex = 4;
            this.label_进度文字.Text = "进度";
            // 
            // label_服务器状态
            // 
            this.label_服务器状态.AutoSize = true;
            this.label_服务器状态.BackColor = System.Drawing.Color.Transparent;
            this.label_服务器状态.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_服务器状态.ForeColor = System.Drawing.Color.Silver;
            this.label_服务器状态.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_服务器状态.Location = new System.Drawing.Point(633, 571);
            this.label_服务器状态.Name = "label_服务器状态";
            this.label_服务器状态.Size = new System.Drawing.Size(65, 12);
            this.label_服务器状态.TabIndex = 37;
            this.label_服务器状态.Text = "服务器状态";
            // 
            // splitContainer2
            // 
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.propertyGrid1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.ContextMenuStrip = this.contextMenuStrip1;
            this.splitContainer2.Panel2.Controls.Add(this.skinButton生成);
            this.splitContainer2.Panel2.Controls.Add(this.skinButton保存设计);
            this.splitContainer2.Panel2.Controls.Add(this.skinButton打开设计);
            this.splitContainer2.Panel2.Controls.Add(this.progressBar1);
            this.splitContainer2.Panel2.Controls.Add(this.checkBox_显示网页);
            this.splitContainer2.Size = new System.Drawing.Size(277, 601);
            this.splitContainer2.SplitterDistance = 516;
            this.splitContainer2.TabIndex = 3;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(277, 516);
            this.propertyGrid1.TabIndex = 0;
            // 
            // skinButton生成
            // 
            this.skinButton生成.BackColor = System.Drawing.Color.Transparent;
            this.skinButton生成.Location = new System.Drawing.Point(137, 5);
            this.skinButton生成.Name = "skinButton生成";
            this.skinButton生成.Size = new System.Drawing.Size(126, 48);
            this.skinButton生成.TabIndex = 6;
            this.skinButton生成.Text = "生成登录器";
            this.skinButton生成.UseVisualStyleBackColor = false;
            this.skinButton生成.Click += new System.EventHandler(this.skinButton生成_Click);
            // 
            // skinButton保存设计
            // 
            this.skinButton保存设计.BackColor = System.Drawing.Color.Transparent;
            this.skinButton保存设计.Location = new System.Drawing.Point(17, 30);
            this.skinButton保存设计.Name = "skinButton保存设计";
            this.skinButton保存设计.Size = new System.Drawing.Size(114, 23);
            this.skinButton保存设计.TabIndex = 5;
            this.skinButton保存设计.Text = "保存设计文件";
            this.skinButton保存设计.UseVisualStyleBackColor = false;
            this.skinButton保存设计.Click += new System.EventHandler(this.button_保存设计_Click);
            // 
            // skinButton打开设计
            // 
            this.skinButton打开设计.BackColor = System.Drawing.Color.Transparent;
            this.skinButton打开设计.Location = new System.Drawing.Point(17, 5);
            this.skinButton打开设计.Name = "skinButton打开设计";
            this.skinButton打开设计.Size = new System.Drawing.Size(114, 23);
            this.skinButton打开设计.TabIndex = 5;
            this.skinButton打开设计.Text = "打开设计文件";
            this.skinButton打开设计.UseVisualStyleBackColor = false;
            this.skinButton打开设计.Click += new System.EventHandler(this.button打开设计_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(137, 61);
            this.progressBar1.MarqueeAnimationSpeed = 50;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(126, 10);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // checkBox_显示网页
            // 
            this.checkBox_显示网页.AutoSize = true;
            this.checkBox_显示网页.Location = new System.Drawing.Point(32, 59);
            this.checkBox_显示网页.Name = "checkBox_显示网页";
            this.checkBox_显示网页.Size = new System.Drawing.Size(84, 16);
            this.checkBox_显示网页.TabIndex = 1;
            this.checkBox_显示网页.Text = "预览小主页";
            this.checkBox_显示网页.UseVisualStyleBackColor = true;
            this.checkBox_显示网页.Click += new System.EventHandler(this.checkBox_显示网页_CheckedChanged);
            // 
            // DesignLauncher
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1086, 609);
            this.Controls.Add(this.skinSplitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1065, 640);
            this.Name = "DesignLauncher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "登录器界面设计 - 可按住鼠标左键拖动界面上的控件位置（如果您的网关已授权，请先启动网关服务后再来生成登录器）";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DesignLauncher_FormClosing);
            this.Load += new System.EventHandler(this.DesignLauncher_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.skinSplitContainer1.Panel1.ResumeLayout(false);
            this.skinSplitContainer1.Panel2.ResumeLayout(false);
            this.skinSplitContainer1.ResumeLayout(false);
            this.panel_背景.ResumeLayout(false);
            this.panel_背景.PerformLayout();
            this.mButton_帐号管理.ResumeLayout(false);
            this.mButton_帐号管理.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_Online)).EndInit();
            this.panel区块.ResumeLayout(false);
            this.panel区块.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 生成加密登录器ToolStripMenuItem;
        internal System.Windows.Forms.Panel panel_背景;
        internal MButton mButton_帐号管理;
        internal MButton mButton_关闭按钮;
        internal MButton mButton_启动游戏;
        internal System.Windows.Forms.WebBrowser webBrowser1;
        internal System.Windows.Forms.Label label_登录器名字;
        internal System.Windows.Forms.PictureBox PictureBox_Online;
        internal System.Windows.Forms.Panel panel区块;
        internal System.Windows.Forms.Label label_速度;
        internal System.Windows.Forms.Label label_下载;
        internal System.Windows.Forms.Label label_信息;
        internal System.Windows.Forms.Label label_进度文字;
        internal System.Windows.Forms.Label label_服务器状态;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox checkBox_显示网页;
        private System.Windows.Forms.Button skinButton生成;
        private System.Windows.Forms.Button skinButton保存设计;
        private System.Windows.Forms.Button skinButton打开设计;
        internal System.Windows.Forms.SplitContainer skinSplitContainer1;
        internal System.Windows.Forms.ProgressBar skinProgressBar1;
        internal System.Windows.Forms.Label label_帐号管理;
        private System.Windows.Forms.ToolStripMenuItem 还原登录器文件ToolStripMenuItem;
    }
}
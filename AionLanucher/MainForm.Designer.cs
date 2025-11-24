namespace AionLanucher
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip_状态栏图标菜单 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.关于登录器ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清除线路ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.启动32位ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.启动64位ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.关闭登录器ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.label_登录器名字 = new System.Windows.Forms.Label();
            this.PictureBox_Online = new System.Windows.Forms.PictureBox();
            this.label_ServerStat = new System.Windows.Forms.Label();
            this.button_Account = new AionLanucher.FormSkin.ButtonEx();
            this.button_StartGame = new AionLanucher.FormSkin.ButtonEx();
            this.button_Close = new AionLanucher.FormSkin.ButtonEx();
            this.panel_banner = new System.Windows.Forms.Panel();
            this.label_UpdateFileInfo = new System.Windows.Forms.Label();
            this.Label_DownInfo = new System.Windows.Forms.Label();
            this.label_Speed = new System.Windows.Forms.Label();
            this.dualProgressBar1 = new AionLanucher.FormSkin.DualProgressBar();
            this.进度条背景显示 = new AionLanucher.FormSkin.PlainBackgroundPainter();
            this.进度条边框显示 = new AionLanucher.FormSkin.PlainBorderPainter();
            this.次要进度条显示 = new AionLanucher.FormSkin.PlainProgressPainter();
            this.次要进度条显示效果 = new AionLanucher.FormSkin.RoundGlossPainter();
            this.主要进度条显示 = new AionLanucher.FormSkin.PlainProgressPainter();
            this.主要进度条显示效果 = new AionLanucher.FormSkin.GradientGlossPainter();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip_状态栏图标菜单.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_Online)).BeginInit();
            this.panel_banner.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip_状态栏图标菜单;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "永恒登陆器";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip_状态栏图标菜单
            // 
            this.contextMenuStrip_状态栏图标菜单.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.关于登录器ToolStripMenuItem,
            this.清除线路ToolStripMenuItem,
            this.toolStripSeparator2,
            this.启动32位ToolStripMenuItem,
            this.启动64位ToolStripMenuItem,
            this.toolStripSeparator1,
            this.关闭登录器ToolStripMenuItem});
            this.contextMenuStrip_状态栏图标菜单.Name = "contextMenuStrip_状态栏图标菜单";
            this.contextMenuStrip_状态栏图标菜单.Size = new System.Drawing.Size(127, 126);
            // 
            // 关于登录器ToolStripMenuItem
            // 
            this.关于登录器ToolStripMenuItem.Name = "关于登录器ToolStripMenuItem";
            this.关于登录器ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.关于登录器ToolStripMenuItem.Text = "关于程序";
            this.关于登录器ToolStripMenuItem.Click += new System.EventHandler(this.关于登录器ToolStripMenuItem_Click);
            // 
            // 清除线路ToolStripMenuItem
            // 
            this.清除线路ToolStripMenuItem.Name = "清除线路ToolStripMenuItem";
            this.清除线路ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.清除线路ToolStripMenuItem.Text = "清除线路";
            this.清除线路ToolStripMenuItem.Click += new System.EventHandler(this.清除线路ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(123, 6);
            // 
            // 启动32位ToolStripMenuItem
            // 
            this.启动32位ToolStripMenuItem.Name = "启动32位ToolStripMenuItem";
            this.启动32位ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.启动32位ToolStripMenuItem.Text = "启动32位";
            this.启动32位ToolStripMenuItem.Click += new System.EventHandler(this.启动32位ToolStripMenuItem_Click);
            // 
            // 启动64位ToolStripMenuItem
            // 
            this.启动64位ToolStripMenuItem.Checked = true;
            this.启动64位ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.启动64位ToolStripMenuItem.Name = "启动64位ToolStripMenuItem";
            this.启动64位ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.启动64位ToolStripMenuItem.Text = "启动64位";
            this.启动64位ToolStripMenuItem.Click += new System.EventHandler(this.启动64位ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(123, 6);
            // 
            // 关闭登录器ToolStripMenuItem
            // 
            this.关闭登录器ToolStripMenuItem.Name = "关闭登录器ToolStripMenuItem";
            this.关闭登录器ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.关闭登录器ToolStripMenuItem.Text = "退出程序";
            this.关闭登录器ToolStripMenuItem.Click += new System.EventHandler(this.关闭登录器ToolStripMenuItem_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(110, 172);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(582, 396);
            this.webBrowser1.TabIndex = 38;
            this.webBrowser1.Visible = false;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
            // 
            // label_登录器名字
            // 
            this.label_登录器名字.AutoSize = true;
            this.label_登录器名字.BackColor = System.Drawing.Color.Transparent;
            this.label_登录器名字.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_登录器名字.ForeColor = System.Drawing.Color.LightGray;
            this.label_登录器名字.Location = new System.Drawing.Point(90, 130);
            this.label_登录器名字.Name = "label_登录器名字";
            this.label_登录器名字.Size = new System.Drawing.Size(56, 16);
            this.label_登录器名字.TabIndex = 37;
            this.label_登录器名字.Text = "登录器";
            // 
            // PictureBox_Online
            // 
            this.PictureBox_Online.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox_Online.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PictureBox_Online.Location = new System.Drawing.Point(857, 666);
            this.PictureBox_Online.Name = "PictureBox_Online";
            this.PictureBox_Online.Size = new System.Drawing.Size(18, 18);
            this.PictureBox_Online.TabIndex = 36;
            this.PictureBox_Online.TabStop = false;
            // 
            // label_ServerStat
            // 
            this.label_ServerStat.AutoSize = true;
            this.label_ServerStat.BackColor = System.Drawing.Color.Transparent;
            this.label_ServerStat.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_ServerStat.ForeColor = System.Drawing.Color.Silver;
            this.label_ServerStat.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_ServerStat.Location = new System.Drawing.Point(786, 669);
            this.label_ServerStat.Name = "label_ServerStat";
            this.label_ServerStat.Size = new System.Drawing.Size(65, 12);
            this.label_ServerStat.TabIndex = 35;
            this.label_ServerStat.Text = "服务器状态";
            // 
            // button_Account
            // 
            this.button_Account.ADialogResult = System.Windows.Forms.DialogResult.None;
            this.button_Account.BackColor = System.Drawing.Color.Transparent;
            this.button_Account.BackgroundImage = global::AionLanucher.Properties.Resources.账号常规;
            this.button_Account.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Account.Caption = "账号管理";
            this.button_Account.CaptionColor = System.Drawing.SystemColors.ControlText;
            this.button_Account.CaptionFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_Account.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_Account.DownImage = global::AionLanucher.Properties.Resources.账号按下;
            this.button_Account.Enabled = false;
            this.button_Account.Location = new System.Drawing.Point(792, 436);
            this.button_Account.MoveImage = global::AionLanucher.Properties.Resources.账号经过;
            this.button_Account.Name = "button_Account";
            this.button_Account.NormalImage = global::AionLanucher.Properties.Resources.账号常规;
            this.button_Account.Size = new System.Drawing.Size(75, 38);
            this.button_Account.TabIndex = 3;
            this.button_Account.Click += new System.EventHandler(this.button_Account_Click);
            // 
            // button_StartGame
            // 
            this.button_StartGame.ADialogResult = System.Windows.Forms.DialogResult.None;
            this.button_StartGame.BackColor = System.Drawing.Color.Transparent;
            this.button_StartGame.BackgroundImage = global::AionLanucher.Properties.Resources.启动常规;
            this.button_StartGame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_StartGame.Caption = "";
            this.button_StartGame.CaptionColor = System.Drawing.SystemColors.ControlText;
            this.button_StartGame.CaptionFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_StartGame.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_StartGame.DownImage = global::AionLanucher.Properties.Resources.启动常规;
            this.button_StartGame.Enabled = false;
            this.button_StartGame.Location = new System.Drawing.Point(726, 469);
            this.button_StartGame.MoveImage = global::AionLanucher.Properties.Resources.启动经过;
            this.button_StartGame.Name = "button_StartGame";
            this.button_StartGame.NormalImage = global::AionLanucher.Properties.Resources.启动常规;
            this.button_StartGame.Size = new System.Drawing.Size(215, 195);
            this.button_StartGame.TabIndex = 0;
            this.button_StartGame.Click += new System.EventHandler(this.button_StartGame_Click);
            // 
            // button_Close
            // 
            this.button_Close.ADialogResult = System.Windows.Forms.DialogResult.None;
            this.button_Close.BackColor = System.Drawing.Color.Transparent;
            this.button_Close.BackgroundImage = global::AionLanucher.Properties.Resources.关闭常规;
            this.button_Close.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button_Close.Caption = "";
            this.button_Close.CaptionColor = System.Drawing.SystemColors.ControlText;
            this.button_Close.CaptionFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_Close.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_Close.DownImage = global::AionLanucher.Properties.Resources.关闭按下;
            this.button_Close.Location = new System.Drawing.Point(926, 124);
            this.button_Close.MoveImage = global::AionLanucher.Properties.Resources.关闭经过;
            this.button_Close.Name = "button_Close";
            this.button_Close.NormalImage = global::AionLanucher.Properties.Resources.关闭常规;
            this.button_Close.Size = new System.Drawing.Size(32, 30);
            this.button_Close.TabIndex = 1;
            this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
            // 
            // panel_banner
            // 
            this.panel_banner.BackColor = System.Drawing.Color.Transparent;
            this.panel_banner.BackgroundImage = global::AionLanucher.Properties.Resources.进度;
            this.panel_banner.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panel_banner.Controls.Add(this.label_UpdateFileInfo);
            this.panel_banner.Controls.Add(this.Label_DownInfo);
            this.panel_banner.Controls.Add(this.label_Speed);
            this.panel_banner.Controls.Add(this.dualProgressBar1);
            this.panel_banner.Controls.Add(this.label1);
            this.panel_banner.Location = new System.Drawing.Point(110, 583);
            this.panel_banner.Name = "panel_banner";
            this.panel_banner.Size = new System.Drawing.Size(573, 81);
            this.panel_banner.TabIndex = 2;
            // 
            // label_UpdateFileInfo
            // 
            this.label_UpdateFileInfo.AutoSize = true;
            this.label_UpdateFileInfo.BackColor = System.Drawing.Color.Transparent;
            this.label_UpdateFileInfo.Font = new System.Drawing.Font("宋体", 9F);
            this.label_UpdateFileInfo.ForeColor = System.Drawing.Color.White;
            this.label_UpdateFileInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_UpdateFileInfo.Location = new System.Drawing.Point(74, 16);
            this.label_UpdateFileInfo.Name = "label_UpdateFileInfo";
            this.label_UpdateFileInfo.Size = new System.Drawing.Size(101, 12);
            this.label_UpdateFileInfo.TabIndex = 37;
            this.label_UpdateFileInfo.Text = "登录器尚未初始化";
            // 
            // Label_DownInfo
            // 
            this.Label_DownInfo.AutoSize = true;
            this.Label_DownInfo.BackColor = System.Drawing.Color.Transparent;
            this.Label_DownInfo.Font = new System.Drawing.Font("宋体", 9F);
            this.Label_DownInfo.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.Label_DownInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Label_DownInfo.Location = new System.Drawing.Point(74, 48);
            this.Label_DownInfo.Name = "Label_DownInfo";
            this.Label_DownInfo.Size = new System.Drawing.Size(71, 12);
            this.Label_DownInfo.TabIndex = 36;
            this.Label_DownInfo.Text = "           ";
            // 
            // label_Speed
            // 
            this.label_Speed.AutoSize = true;
            this.label_Speed.BackColor = System.Drawing.Color.Transparent;
            this.label_Speed.Font = new System.Drawing.Font("宋体", 9F);
            this.label_Speed.ForeColor = System.Drawing.Color.Gold;
            this.label_Speed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_Speed.Location = new System.Drawing.Point(434, 47);
            this.label_Speed.Name = "label_Speed";
            this.label_Speed.Size = new System.Drawing.Size(95, 12);
            this.label_Speed.TabIndex = 38;
            this.label_Speed.Text = "               ";
            // 
            // dualProgressBar1
            // 
            this.dualProgressBar1.BackgroundPainter = this.进度条背景显示;
            this.dualProgressBar1.BorderPainter = this.进度条边框显示;
            this.dualProgressBar1.Location = new System.Drawing.Point(74, 32);
            this.dualProgressBar1.MarqueePercentage = 25;
            this.dualProgressBar1.MarqueeSpeed = 30;
            this.dualProgressBar1.MarqueeStep = 1;
            this.dualProgressBar1.MasterMaximum = 100;
            this.dualProgressBar1.MasterPainter = this.次要进度条显示;
            this.dualProgressBar1.MasterProgressPadding = 0;
            this.dualProgressBar1.MasterValue = 100;
            this.dualProgressBar1.Maximum = 100;
            this.dualProgressBar1.Minimum = 0;
            this.dualProgressBar1.Name = "dualProgressBar1";
            this.dualProgressBar1.PaintMasterFirst = true;
            this.dualProgressBar1.ProgressPadding = 0;
            this.dualProgressBar1.ProgressPainter = this.主要进度条显示;
            this.dualProgressBar1.ProgressType = AionLanucher.FormSkin.ProgressType.Smooth;
            this.dualProgressBar1.ShowPercentage = true;
            this.dualProgressBar1.Size = new System.Drawing.Size(455, 12);
            this.dualProgressBar1.TabIndex = 1;
            this.dualProgressBar1.Text = "100%";
            this.dualProgressBar1.Value = 100;
            // 
            // 进度条背景显示
            // 
            this.进度条背景显示.Color = System.Drawing.Color.White;
            this.进度条背景显示.GlossPainter = null;
            // 
            // 进度条边框显示
            // 
            this.进度条边框显示.Color = System.Drawing.Color.OliveDrab;
            this.进度条边框显示.RoundedCorners = false;
            this.进度条边框显示.Style = AionLanucher.FormSkin.PlainBorderPainter.PlainBorderStyle.Flat;
            // 
            // 次要进度条显示
            // 
            this.次要进度条显示.Color = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(229)))), ((int)(((byte)(124)))));
            this.次要进度条显示.GlossPainter = this.次要进度条显示效果;
            this.次要进度条显示.LeadingEdge = System.Drawing.Color.GreenYellow;
            this.次要进度条显示.ProgressBorderPainter = null;
            // 
            // 次要进度条显示效果
            // 
            this.次要进度条显示效果.AlphaHigh = 255;
            this.次要进度条显示效果.AlphaLow = 0;
            this.次要进度条显示效果.Color = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(191)))), ((int)(((byte)(104)))));
            this.次要进度条显示效果.Style = AionLanucher.FormSkin.GlossStyle.Both;
            this.次要进度条显示效果.Successor = null;
            this.次要进度条显示效果.TaperHeight = 3;
            // 
            // 主要进度条显示
            // 
            this.主要进度条显示.Color = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(127)))), ((int)(((byte)(0)))));
            this.主要进度条显示.GlossPainter = this.主要进度条显示效果;
            this.主要进度条显示.LeadingEdge = System.Drawing.Color.DarkGreen;
            this.主要进度条显示.ProgressBorderPainter = null;
            // 
            // 主要进度条显示效果
            // 
            this.主要进度条显示效果.AlphaHigh = 240;
            this.主要进度条显示效果.AlphaLow = 0;
            this.主要进度条显示效果.Angle = 90F;
            this.主要进度条显示效果.Color = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(233)))), ((int)(((byte)(143)))));
            this.主要进度条显示效果.PercentageCovered = 90;
            this.主要进度条显示效果.Style = AionLanucher.FormSkin.GlossStyle.Top;
            this.主要进度条显示效果.Successor = null;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label1.Location = new System.Drawing.Point(33, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "进度";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1038, 704);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.label_登录器名字);
            this.Controls.Add(this.PictureBox_Online);
            this.Controls.Add(this.label_ServerStat);
            this.Controls.Add(this.button_Account);
            this.Controls.Add(this.button_StartGame);
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.panel_banner);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.SkinBack = global::AionLanucher.Properties.Resources.登录器;
            this.SkinSize = new System.Drawing.Size(1038, 704);
            this.SkinWhetherTank = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "登录器";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.contextMenuStrip_状态栏图标菜单.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox_Online)).EndInit();
            this.panel_banner.ResumeLayout(false);
            this.panel_banner.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FormSkin.ButtonEx button_StartGame;
        private FormSkin.ButtonEx button_Close;
        private System.Windows.Forms.Panel panel_banner;
        private System.Windows.Forms.Label label1;
        private FormSkin.ButtonEx button_Account;
        private FormSkin.DualProgressBar dualProgressBar1;
        private FormSkin.PlainBackgroundPainter 进度条背景显示;
        private FormSkin.PlainBorderPainter 进度条边框显示;
        private FormSkin.PlainProgressPainter 主要进度条显示;
        private FormSkin.GradientGlossPainter 主要进度条显示效果;
        private FormSkin.PlainProgressPainter 次要进度条显示;
        private FormSkin.RoundGlossPainter 次要进度条显示效果;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_状态栏图标菜单;
        private System.Windows.Forms.ToolStripMenuItem 关于登录器ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关闭登录器ToolStripMenuItem;
        private System.Windows.Forms.Label label_UpdateFileInfo;
        private System.Windows.Forms.Label Label_DownInfo;
        private System.Windows.Forms.Label label_Speed;
        private System.Windows.Forms.Label label_ServerStat;
        private System.Windows.Forms.PictureBox PictureBox_Online;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Label label_登录器名字;
        private System.Windows.Forms.ToolStripMenuItem 清除线路ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 启动32位ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 启动64位ToolStripMenuItem;
    }
}


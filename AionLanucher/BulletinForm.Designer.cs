namespace AionLanucher
{
    partial class BulletinForm
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageAnnouncement = new System.Windows.Forms.TabPage();
            this.tabPageMessages = new System.Windows.Forms.TabPage();
            this.tabPagePost = new System.Windows.Forms.TabPage();
            this.tabPageMyMessages = new System.Windows.Forms.TabPage();
            this.listViewItems = new System.Windows.Forms.ListView();
            this.panelPage = new System.Windows.Forms.Panel();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.panelPost = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblType = new System.Windows.Forms.Label();
            this.comboMsgType = new System.Windows.Forms.ComboBox();
            this.lblContent = new System.Windows.Forms.Label();
            this.txtContent = new System.Windows.Forms.TextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lblTip = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabPageAnnouncement.SuspendLayout();
            this.tabPageMessages.SuspendLayout();
            this.tabPagePost.SuspendLayout();
            this.tabPageMyMessages.SuspendLayout();
            this.panelPage.SuspendLayout();
            this.panelPost.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabPageAnnouncement);
            this.tabControl.Controls.Add(this.tabPageMessages);
            this.tabControl.Controls.Add(this.tabPagePost);
            this.tabControl.Controls.Add(this.tabPageMyMessages);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(684, 511);
            this.tabControl.TabIndex = 0;
            //
            // tabPageAnnouncement
            //
            this.tabPageAnnouncement.Controls.Add(this.listViewItems);
            this.tabPageAnnouncement.Controls.Add(this.panelPage);
            this.tabPageAnnouncement.Location = new System.Drawing.Point(4, 22);
            this.tabPageAnnouncement.Name = "tabPageAnnouncement";
            this.tabPageAnnouncement.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAnnouncement.Size = new System.Drawing.Size(676, 485);
            this.tabPageAnnouncement.TabIndex = 0;
            this.tabPageAnnouncement.Text = "系统公告";
            this.tabPageAnnouncement.UseVisualStyleBackColor = true;
            //
            // tabPageMessages
            //
            this.tabPageMessages.Location = new System.Drawing.Point(4, 22);
            this.tabPageMessages.Name = "tabPageMessages";
            this.tabPageMessages.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMessages.Size = new System.Drawing.Size(676, 485);
            this.tabPageMessages.TabIndex = 1;
            this.tabPageMessages.Text = "留言列表";
            this.tabPageMessages.UseVisualStyleBackColor = true;
            //
            // tabPagePost
            //
            this.tabPagePost.Controls.Add(this.panelPost);
            this.tabPagePost.Location = new System.Drawing.Point(4, 22);
            this.tabPagePost.Name = "tabPagePost";
            this.tabPagePost.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePost.Size = new System.Drawing.Size(676, 485);
            this.tabPagePost.TabIndex = 2;
            this.tabPagePost.Text = "我要留言";
            this.tabPagePost.UseVisualStyleBackColor = true;
            //
            // tabPageMyMessages
            //
            this.tabPageMyMessages.Location = new System.Drawing.Point(4, 22);
            this.tabPageMyMessages.Name = "tabPageMyMessages";
            this.tabPageMyMessages.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMyMessages.Size = new System.Drawing.Size(676, 485);
            this.tabPageMyMessages.TabIndex = 3;
            this.tabPageMyMessages.Text = "我的留言";
            this.tabPageMyMessages.UseVisualStyleBackColor = true;
            //
            // listViewItems
            //
            this.listViewItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewItems.FullRowSelect = true;
            this.listViewItems.GridLines = true;
            this.listViewItems.Location = new System.Drawing.Point(3, 3);
            this.listViewItems.Name = "listViewItems";
            this.listViewItems.Size = new System.Drawing.Size(670, 439);
            this.listViewItems.TabIndex = 0;
            this.listViewItems.UseCompatibleStateImageBehavior = false;
            this.listViewItems.View = System.Windows.Forms.View.Details;
            //
            // panelPage
            //
            this.panelPage.Controls.Add(this.lblPageInfo);
            this.panelPage.Controls.Add(this.btnRefresh);
            this.panelPage.Controls.Add(this.btnNextPage);
            this.panelPage.Controls.Add(this.btnPrevPage);
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelPage.Location = new System.Drawing.Point(3, 442);
            this.panelPage.Name = "panelPage";
            this.panelPage.Size = new System.Drawing.Size(670, 40);
            this.panelPage.TabIndex = 1;
            //
            // btnPrevPage
            //
            this.btnPrevPage.Location = new System.Drawing.Point(10, 8);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(75, 25);
            this.btnPrevPage.TabIndex = 0;
            this.btnPrevPage.Text = "上一页";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            //
            // btnNextPage
            //
            this.btnNextPage.Location = new System.Drawing.Point(91, 8);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 25);
            this.btnNextPage.TabIndex = 1;
            this.btnNextPage.Text = "下一页";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(580, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // lblPageInfo
            //
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(280, 13);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(125, 12);
            this.lblPageInfo.TabIndex = 3;
            this.lblPageInfo.Text = "第 1/1 页 (共 0 条)";
            //
            // panelPost
            //
            this.panelPost.Controls.Add(this.lblTip);
            this.panelPost.Controls.Add(this.btnSubmit);
            this.panelPost.Controls.Add(this.txtContent);
            this.panelPost.Controls.Add(this.lblContent);
            this.panelPost.Controls.Add(this.comboMsgType);
            this.panelPost.Controls.Add(this.lblType);
            this.panelPost.Controls.Add(this.txtTitle);
            this.panelPost.Controls.Add(this.lblTitle);
            this.panelPost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPost.Location = new System.Drawing.Point(3, 3);
            this.panelPost.Name = "panelPost";
            this.panelPost.Padding = new System.Windows.Forms.Padding(20);
            this.panelPost.Size = new System.Drawing.Size(670, 479);
            this.panelPost.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(30, 30);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(41, 12);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "标题:";
            //
            // txtTitle
            //
            this.txtTitle.Location = new System.Drawing.Point(80, 27);
            this.txtTitle.MaxLength = 50;
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(400, 21);
            this.txtTitle.TabIndex = 1;
            //
            // lblType
            //
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(500, 30);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(41, 12);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "类型:";
            //
            // comboMsgType
            //
            this.comboMsgType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMsgType.FormattingEnabled = true;
            this.comboMsgType.Items.AddRange(new object[] {
            "普通留言",
            "投诉",
            "建议",
            "BUG反馈"});
            this.comboMsgType.Location = new System.Drawing.Point(545, 27);
            this.comboMsgType.Name = "comboMsgType";
            this.comboMsgType.Size = new System.Drawing.Size(100, 20);
            this.comboMsgType.TabIndex = 3;
            //
            // lblContent
            //
            this.lblContent.AutoSize = true;
            this.lblContent.Location = new System.Drawing.Point(30, 70);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(41, 12);
            this.lblContent.TabIndex = 4;
            this.lblContent.Text = "内容:";
            //
            // txtContent
            //
            this.txtContent.Location = new System.Drawing.Point(80, 67);
            this.txtContent.MaxLength = 2000;
            this.txtContent.Multiline = true;
            this.txtContent.Name = "txtContent";
            this.txtContent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtContent.Size = new System.Drawing.Size(565, 300);
            this.txtContent.TabIndex = 5;
            //
            // btnSubmit
            //
            this.btnSubmit.Font = new System.Drawing.Font("SimSun", 10F, System.Drawing.FontStyle.Bold);
            this.btnSubmit.Location = new System.Drawing.Point(270, 390);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(120, 35);
            this.btnSubmit.TabIndex = 6;
            this.btnSubmit.Text = "提交留言";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            //
            // lblTip
            //
            this.lblTip.ForeColor = System.Drawing.Color.Gray;
            this.lblTip.Location = new System.Drawing.Point(80, 435);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(565, 35);
            this.lblTip.TabIndex = 7;
            this.lblTip.Text = "温馨提示: 请文明留言，每分钟最多发布1条留言。您的意见和建议是我们前进的动力！";
            //
            // BulletinForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 511);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BulletinForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "留言板 - 玩家服务中心";
            this.tabControl.ResumeLayout(false);
            this.tabPageAnnouncement.ResumeLayout(false);
            this.tabPageMessages.ResumeLayout(false);
            this.tabPagePost.ResumeLayout(false);
            this.tabPageMyMessages.ResumeLayout(false);
            this.panelPage.ResumeLayout(false);
            this.panelPage.PerformLayout();
            this.panelPost.ResumeLayout(false);
            this.panelPost.PerformLayout();
            this.SuspendLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageAnnouncement;
        private System.Windows.Forms.TabPage tabPageMessages;
        private System.Windows.Forms.TabPage tabPagePost;
        private System.Windows.Forms.TabPage tabPageMyMessages;
        private System.Windows.Forms.ListView listViewItems;
        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Panel panelPost;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox comboMsgType;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Label lblTip;
    }
}

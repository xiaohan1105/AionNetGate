using System.Windows.Forms;
namespace AionNetGate
{
    partial class DeskPictureForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeskPictureForm));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.图像显示模式ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.缩放ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.拉伸ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.居中ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.缩放ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.适应ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.图像画面刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.自动刷新图像ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.全屏模式F11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(989, 609);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.图像显示模式ToolStripMenuItem,
            this.toolStripSeparator1,
            this.图像画面刷新ToolStripMenuItem,
            this.自动刷新图像ToolStripMenuItem,
            this.toolStripSeparator2,
            this.全屏模式F11ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(169, 104);
            // 
            // 图像显示模式ToolStripMenuItem
            // 
            this.图像显示模式ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.缩放ToolStripMenuItem,
            this.拉伸ToolStripMenuItem,
            this.居中ToolStripMenuItem,
            this.缩放ToolStripMenuItem1,
            this.适应ToolStripMenuItem});
            this.图像显示模式ToolStripMenuItem.Name = "图像显示模式ToolStripMenuItem";
            this.图像显示模式ToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.图像显示模式ToolStripMenuItem.Text = "图像显示模式";
            // 
            // 缩放ToolStripMenuItem
            // 
            this.缩放ToolStripMenuItem.Name = "缩放ToolStripMenuItem";
            this.缩放ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.缩放ToolStripMenuItem.Text = "自动";
            // 
            // 拉伸ToolStripMenuItem
            // 
            this.拉伸ToolStripMenuItem.Name = "拉伸ToolStripMenuItem";
            this.拉伸ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.拉伸ToolStripMenuItem.Text = "拉伸";
            // 
            // 居中ToolStripMenuItem
            // 
            this.居中ToolStripMenuItem.Name = "居中ToolStripMenuItem";
            this.居中ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.居中ToolStripMenuItem.Text = "居中";
            // 
            // 缩放ToolStripMenuItem1
            // 
            this.缩放ToolStripMenuItem1.Checked = true;
            this.缩放ToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.缩放ToolStripMenuItem1.Name = "缩放ToolStripMenuItem1";
            this.缩放ToolStripMenuItem1.Size = new System.Drawing.Size(100, 22);
            this.缩放ToolStripMenuItem1.Text = "缩放";
            // 
            // 适应ToolStripMenuItem
            // 
            this.适应ToolStripMenuItem.Name = "适应ToolStripMenuItem";
            this.适应ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.适应ToolStripMenuItem.Text = "正常";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // 图像画面刷新ToolStripMenuItem
            // 
            this.图像画面刷新ToolStripMenuItem.Name = "图像画面刷新ToolStripMenuItem";
            this.图像画面刷新ToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.图像画面刷新ToolStripMenuItem.Text = "手动刷新图像";
            // 
            // 自动刷新图像ToolStripMenuItem
            // 
            this.自动刷新图像ToolStripMenuItem.Name = "自动刷新图像ToolStripMenuItem";
            this.自动刷新图像ToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.自动刷新图像ToolStripMenuItem.Text = "自动刷新图像";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(165, 6);
            // 
            // 全屏模式F11ToolStripMenuItem
            // 
            this.全屏模式F11ToolStripMenuItem.Name = "全屏模式F11ToolStripMenuItem";
            this.全屏模式F11ToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.全屏模式F11ToolStripMenuItem.Text = "全屏显示（F11）";
            // 
            // DeskPictureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(989, 609);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DeskPictureForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "玩家桌面";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeskPictureForm_FormClosing);
            this.Load += new System.EventHandler(this.DeskPictureForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        private PictureBox pictureBox1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem 图像显示模式ToolStripMenuItem;
        private ToolStripMenuItem 缩放ToolStripMenuItem;
        private ToolStripMenuItem 拉伸ToolStripMenuItem;
        private ToolStripMenuItem 居中ToolStripMenuItem;
        private ToolStripMenuItem 缩放ToolStripMenuItem1;
        private ToolStripMenuItem 适应ToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem 图像画面刷新ToolStripMenuItem;
        private ToolStripMenuItem 自动刷新图像ToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem 全屏模式F11ToolStripMenuItem;
    }
}
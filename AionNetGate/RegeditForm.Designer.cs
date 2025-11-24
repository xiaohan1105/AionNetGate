namespace AionNetGate
{
    partial class RegeditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegeditForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新建项ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新建项ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.字符串值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.二进制值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dWORD32位值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.qWORD64位值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.多字符串值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.可扩充字符串值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(807, 569);
            this.splitContainer1.SplitterDistance = 226;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(224, 567);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(574, 567);
            this.listView1.SmallImageList = this.imageList2;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemActivate += new System.EventHandler(this.listView1_ItemActivate);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "名称";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "类型";
            this.columnHeader2.Width = 130;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "数据";
            this.columnHeader3.Width = 200;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建项ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(119, 26);
            // 
            // 新建项ToolStripMenuItem
            // 
            this.新建项ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建项ToolStripMenuItem1,
            this.toolStripSeparator1,
            this.字符串值ToolStripMenuItem,
            this.二进制值ToolStripMenuItem,
            this.dWORD32位值ToolStripMenuItem,
            this.qWORD64位值ToolStripMenuItem,
            this.多字符串值ToolStripMenuItem,
            this.可扩充字符串值ToolStripMenuItem});
            this.新建项ToolStripMenuItem.Name = "新建项ToolStripMenuItem";
            this.新建项ToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.新建项ToolStripMenuItem.Text = "新建(&N)";
            // 
            // 新建项ToolStripMenuItem1
            // 
            this.新建项ToolStripMenuItem1.Name = "新建项ToolStripMenuItem1";
            this.新建项ToolStripMenuItem1.Size = new System.Drawing.Size(171, 22);
            this.新建项ToolStripMenuItem1.Text = "新建项";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // 字符串值ToolStripMenuItem
            // 
            this.字符串值ToolStripMenuItem.Name = "字符串值ToolStripMenuItem";
            this.字符串值ToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.字符串值ToolStripMenuItem.Text = "字符串值";
            // 
            // 二进制值ToolStripMenuItem
            // 
            this.二进制值ToolStripMenuItem.Name = "二进制值ToolStripMenuItem";
            this.二进制值ToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.二进制值ToolStripMenuItem.Text = "二进制值";
            // 
            // dWORD32位值ToolStripMenuItem
            // 
            this.dWORD32位值ToolStripMenuItem.Name = "dWORD32位值ToolStripMenuItem";
            this.dWORD32位值ToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.dWORD32位值ToolStripMenuItem.Text = "DWORD(32位)值";
            // 
            // qWORD64位值ToolStripMenuItem
            // 
            this.qWORD64位值ToolStripMenuItem.Name = "qWORD64位值ToolStripMenuItem";
            this.qWORD64位值ToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.qWORD64位值ToolStripMenuItem.Text = "QWORD(64位)值";
            // 
            // 多字符串值ToolStripMenuItem
            // 
            this.多字符串值ToolStripMenuItem.Name = "多字符串值ToolStripMenuItem";
            this.多字符串值ToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.多字符串值ToolStripMenuItem.Text = "多字符串值";
            // 
            // 可扩充字符串值ToolStripMenuItem
            // 
            this.可扩充字符串值ToolStripMenuItem.Name = "可扩充字符串值ToolStripMenuItem";
            this.可扩充字符串值ToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.可扩充字符串值ToolStripMenuItem.Text = "可扩充字符串值";
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "S.png");
            this.imageList2.Images.SetKeyName(1, "DW.png");
            // 
            // RegeditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 569);
            this.Controls.Add(this.splitContainer1);
            this.Name = "RegeditForm";
            this.Text = "RegeditForm";
            this.Load += new System.EventHandler(this.RegeditForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 新建项ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新建项ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 字符串值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 二进制值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dWORD32位值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem qWORD64位值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 多字符串值ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 可扩充字符串值ToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList2;
    }
}
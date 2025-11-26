using AionLanucher.Network;
using AionLanucher.Network.Client;
using AionLanucher.Network.Server;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher
{
    /// <summary>
    /// 留言板窗体 - 支持查看公告、留言列表、发布留言等功能
    /// </summary>
    public partial class BulletinForm : Form
    {
        internal static BulletinForm Instance;

        private AionConnection con;
        private string currentAccount;

        // 分页参数
        private int currentPageIndex = 0;
        private int pageSize = 10;
        private int totalCount = 0;

        // 当前显示模式：0=公告, 1=留言列表, 4=我的留言
        private byte currentMode = 0;

        public BulletinForm(AionConnection connection, string accountName)
        {
            InitializeComponent();
            Instance = this;
            con = connection;
            currentAccount = accountName;

            InitializeUI();
        }

        #region 初始化

        private void InitializeUI()
        {
            // 设置窗体属性
            this.Text = "留言板 - 玩家服务中心";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 初始化列表视图
            InitializeListView();

            // 初始化事件
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            listViewItems.DoubleClick += ListViewItems_DoubleClick;

            // 加载系统公告
            LoadAnnouncements();
        }

        private void InitializeListView()
        {
            listViewItems.View = View.Details;
            listViewItems.FullRowSelect = true;
            listViewItems.GridLines = true;

            listViewItems.Columns.Clear();
            listViewItems.Columns.Add("ID", 50, HorizontalAlignment.Center);
            listViewItems.Columns.Add("标题", 280, HorizontalAlignment.Left);
            listViewItems.Columns.Add("发布者", 100, HorizontalAlignment.Center);
            listViewItems.Columns.Add("类型", 60, HorizontalAlignment.Center);
            listViewItems.Columns.Add("状态", 60, HorizontalAlignment.Center);
            listViewItems.Columns.Add("时间", 120, HorizontalAlignment.Center);
        }

        #endregion

        #region 事件处理

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPageIndex = 0;

            switch (tabControl.SelectedIndex)
            {
                case 0: // 系统公告
                    currentMode = 0;
                    LoadAnnouncements();
                    break;

                case 1: // 留言列表
                    currentMode = 1;
                    LoadPublicMessages();
                    break;

                case 2: // 我要留言
                    // 不需要加载数据
                    break;

                case 3: // 我的留言
                    currentMode = 4;
                    LoadMyMessages();
                    break;
            }
        }

        private void ListViewItems_DoubleClick(object sender, EventArgs e)
        {
            if (listViewItems.SelectedItems.Count > 0)
            {
                int id = int.Parse(listViewItems.SelectedItems[0].Text);
                LoadMessageDetail(id);
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                RefreshCurrentList();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            int maxPage = (totalCount + pageSize - 1) / pageSize;
            if (currentPageIndex < maxPage - 1)
            {
                currentPageIndex++;
                RefreshCurrentList();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshCurrentList();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            SubmitMessage();
        }

        #endregion

        #region 数据加载

        private void RefreshCurrentList()
        {
            switch (currentMode)
            {
                case 0:
                    LoadAnnouncements();
                    break;
                case 1:
                    LoadPublicMessages();
                    break;
                case 4:
                    LoadMyMessages();
                    break;
            }
        }

        private void LoadAnnouncements()
        {
            SetLoading(true);
            con.SendPacket(new SM_BULLETIN_REQUEST(0, currentAccount, currentPageIndex, pageSize));
        }

        private void LoadPublicMessages()
        {
            SetLoading(true);
            con.SendPacket(new SM_BULLETIN_REQUEST(1, currentAccount, currentPageIndex, pageSize));
        }

        private void LoadMyMessages()
        {
            if (string.IsNullOrEmpty(currentAccount))
            {
                ShowMessage("请先登录后再查看留言", true);
                return;
            }

            SetLoading(true);
            con.SendPacket(new SM_BULLETIN_REQUEST(4, currentAccount, currentPageIndex, pageSize));
        }

        private void LoadMessageDetail(int messageId)
        {
            con.SendPacket(new SM_BULLETIN_REQUEST(currentAccount, messageId));
        }

        private void SubmitMessage()
        {
            if (string.IsNullOrEmpty(currentAccount))
            {
                ShowMessage("请先登录后再发布留言", true);
                return;
            }

            string title = txtTitle.Text.Trim();
            string content = txtContent.Text.Trim();
            byte msgType = (byte)comboMsgType.SelectedIndex;

            if (string.IsNullOrEmpty(title) || title.Length < 2)
            {
                ShowMessage("请输入标题（至少2个字符）", true);
                txtTitle.Focus();
                return;
            }

            if (string.IsNullOrEmpty(content) || content.Length < 5)
            {
                ShowMessage("请输入内容（至少5个字符）", true);
                txtContent.Focus();
                return;
            }

            btnSubmit.Enabled = false;
            btnSubmit.Text = "提交中...";

            con.SendPacket(new SM_BULLETIN_REQUEST(currentAccount, title, content, msgType));
        }

        #endregion

        #region 服务器响应处理

        /// <summary>
        /// 处理列表响应
        /// </summary>
        internal void OnListResponse(byte type, bool success, string message,
            List<BulletinItemData> items, int total, int pageIdx, int pageSz)
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
            {
                SetLoading(false);

                if (!success)
                {
                    ShowMessage(message, true);
                    return;
                }

                totalCount = total;
                currentPageIndex = pageIdx;
                pageSize = pageSz;

                UpdateListView(items, type);
                UpdatePageInfo();
            }));
        }

        /// <summary>
        /// 处理发布留言响应
        /// </summary>
        internal void OnPostResponse(bool success, string message)
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
            {
                btnSubmit.Enabled = true;
                btnSubmit.Text = "提交留言";

                ShowMessage(message, !success);

                if (success)
                {
                    // 清空输入
                    txtTitle.Text = "";
                    txtContent.Text = "";
                    comboMsgType.SelectedIndex = 0;

                    // 切换到我的留言页面
                    tabControl.SelectedIndex = 3;
                }
            }));
        }

        /// <summary>
        /// 处理留言详情响应
        /// </summary>
        internal void OnDetailResponse(BulletinDetailData detail, string message)
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
            {
                if (detail == null)
                {
                    ShowMessage(message, true);
                    return;
                }

                ShowDetailDialog(detail);
            }));
        }

        #endregion

        #region UI更新

        private void UpdateListView(List<BulletinItemData> items, byte listType)
        {
            listViewItems.Items.Clear();

            if (items == null || items.Count == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                ListViewItem lvi = new ListViewItem(item.Id.ToString());
                lvi.SubItems.Add(item.PriorityText + item.Title);
                lvi.SubItems.Add(item.AccountName);
                lvi.SubItems.Add(listType == 0 ? "公告" : item.TypeText);
                lvi.SubItems.Add(item.HasReply ? "已回复" : item.StatusText);
                lvi.SubItems.Add(item.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                lvi.Tag = item;

                // 根据优先级设置颜色
                if (item.Priority == 2)
                {
                    lvi.ForeColor = Color.Red;
                }
                else if (item.Priority == 1)
                {
                    lvi.ForeColor = Color.Orange;
                }

                // 已回复的留言用绿色
                if (item.HasReply)
                {
                    lvi.BackColor = Color.FromArgb(230, 255, 230);
                }

                listViewItems.Items.Add(lvi);
            }
        }

        private void UpdatePageInfo()
        {
            int maxPage = (totalCount + pageSize - 1) / pageSize;
            if (maxPage == 0) maxPage = 1;

            lblPageInfo.Text = string.Format("第 {0}/{1} 页 (共 {2} 条)",
                currentPageIndex + 1, maxPage, totalCount);

            btnPrevPage.Enabled = currentPageIndex > 0;
            btnNextPage.Enabled = currentPageIndex < maxPage - 1;
        }

        private void SetLoading(bool loading)
        {
            listViewItems.Enabled = !loading;
            btnRefresh.Enabled = !loading;

            if (loading)
            {
                listViewItems.Items.Clear();
                ListViewItem lvi = new ListViewItem("加载中...");
                listViewItems.Items.Add(lvi);
            }
        }

        private void ShowDetailDialog(BulletinDetailData detail)
        {
            using (Form detailForm = new Form())
            {
                detailForm.Text = detail.Title;
                detailForm.Size = new Size(500, 450);
                detailForm.StartPosition = FormStartPosition.CenterParent;
                detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                detailForm.MaximizeBox = false;
                detailForm.MinimizeBox = false;

                Panel panel = new Panel();
                panel.Dock = DockStyle.Fill;
                panel.AutoScroll = true;
                panel.Padding = new Padding(15);

                // 标题
                Label lblTitle = new Label();
                lblTitle.Text = detail.Title;
                lblTitle.Font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
                lblTitle.Location = new Point(15, 10);
                lblTitle.AutoSize = true;
                panel.Controls.Add(lblTitle);

                // 发布者和时间
                Label lblInfo = new Label();
                lblInfo.Text = string.Format("发布者: {0}  |  时间: {1}",
                    detail.AccountName, detail.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                lblInfo.ForeColor = Color.Gray;
                lblInfo.Location = new Point(15, 45);
                lblInfo.AutoSize = true;
                panel.Controls.Add(lblInfo);

                // 内容
                Label lblContent = new Label();
                lblContent.Text = "内容:";
                lblContent.Location = new Point(15, 75);
                lblContent.AutoSize = true;
                panel.Controls.Add(lblContent);

                TextBox txtContentView = new TextBox();
                txtContentView.Multiline = true;
                txtContentView.ReadOnly = true;
                txtContentView.ScrollBars = ScrollBars.Vertical;
                txtContentView.Text = detail.Content;
                txtContentView.Location = new Point(15, 95);
                txtContentView.Size = new Size(450, 120);
                panel.Controls.Add(txtContentView);

                // 管理员回复
                if (!string.IsNullOrEmpty(detail.AdminReply))
                {
                    Label lblReplyTitle = new Label();
                    lblReplyTitle.Text = "管理员回复:";
                    lblReplyTitle.ForeColor = Color.Green;
                    lblReplyTitle.Font = new Font(lblReplyTitle.Font, FontStyle.Bold);
                    lblReplyTitle.Location = new Point(15, 225);
                    lblReplyTitle.AutoSize = true;
                    panel.Controls.Add(lblReplyTitle);

                    Label lblReplyTime = new Label();
                    lblReplyTime.Text = detail.ReplyTime.HasValue ?
                        "回复时间: " + detail.ReplyTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                    lblReplyTime.ForeColor = Color.Gray;
                    lblReplyTime.Location = new Point(15, 245);
                    lblReplyTime.AutoSize = true;
                    panel.Controls.Add(lblReplyTime);

                    TextBox txtReplyView = new TextBox();
                    txtReplyView.Multiline = true;
                    txtReplyView.ReadOnly = true;
                    txtReplyView.ScrollBars = ScrollBars.Vertical;
                    txtReplyView.Text = detail.AdminReply;
                    txtReplyView.BackColor = Color.FromArgb(230, 255, 230);
                    txtReplyView.Location = new Point(15, 265);
                    txtReplyView.Size = new Size(450, 100);
                    panel.Controls.Add(txtReplyView);
                }
                else
                {
                    Label lblNoReply = new Label();
                    lblNoReply.Text = "暂无管理员回复";
                    lblNoReply.ForeColor = Color.Gray;
                    lblNoReply.Location = new Point(15, 225);
                    lblNoReply.AutoSize = true;
                    panel.Controls.Add(lblNoReply);
                }

                // 关闭按钮
                Button btnClose = new Button();
                btnClose.Text = "关闭";
                btnClose.Size = new Size(80, 30);
                btnClose.Location = new Point(385, 375);
                btnClose.Click += (s, e) => detailForm.Close();
                panel.Controls.Add(btnClose);

                detailForm.Controls.Add(panel);
                detailForm.ShowDialog(this);
            }
        }

        private void ShowMessage(string msg, bool isWarning)
        {
            MessageBox.Show(msg, isWarning ? "警告" : "提示",
                MessageBoxButtons.OK, isWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        #endregion

        #region 窗体关闭

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Instance = null;
            base.OnFormClosing(e);
        }

        #endregion
    }
}

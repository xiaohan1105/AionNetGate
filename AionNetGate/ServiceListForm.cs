using AionNetGate.Netwok;
using AionNetGate.Netwok.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class ServiceListForm : Form
    {
        private AionConnection con;
        public ServiceListForm(ref AionConnection _con)
        {
            InitializeComponent();
            con = _con;
        }

        private void ServiceListForm_Load(object sender, EventArgs e)
        {
            // 修复：使用异步方式发送服务请求，避免阻塞UI
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (con != null && con.isConnected)
                    {
                        con.SendPacket(new SM_SERVICES_LIST(0, ""));
                    }
                    else
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("连接已断开，无法获取服务信息", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("发送服务请求失败: " + ex.Message);
                    this.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("发送服务请求失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        public void AddServicesToListView(string[] info)
        {
            // 修复：确保在UI线程中执行
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string[]>(AddServicesToListView), info);
                return;
            }

            // 修复：检查窗体是否已释放
            if (this.IsDisposed || listView1.IsDisposed)
                return;

            try
            {
                if (info != null && info.Length > 0)
                {
                    listView1.BeginUpdate();
                    List<ListViewItem> lists = new List<ListViewItem>();

                    foreach (string s in info)
                    {
                        try
                        {
                            string[] str = s.Split('\t');
                            // 修复：验证数据格式
                            if (str.Length >= 6) // 确保有足够的字段
                            {
                                ListViewItem lvi = new ListViewItem(str, 0);
                                lists.Add(lvi);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("服务信息格式错误: " + s);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("处理服务项失败: " + ex.Message);
                            continue;
                        }
                    }

                    listView1.Items.Clear();

                    // 修复：分批添加服务项，避免UI冻结
                    const int batchSize = 50;
                    for (int i = 0; i < lists.Count; i += batchSize)
                    {
                        int count = Math.Min(batchSize, lists.Count - i);
                        ListViewItem[] batch = new ListViewItem[count];
                        lists.CopyTo(i, batch, 0, count);
                        listView1.Items.AddRange(batch);

                        if (i + batchSize < lists.Count)
                        {
                            Application.DoEvents(); // 允许UI响应
                        }
                    }

                    listView1.EndUpdate();
                    lists.Clear();
                }
                else
                {
                    listView1.Items.Clear();
                    MessageBox.Show("未获取到服务信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("更新服务列表失败: " + ex.Message);
                MessageBox.Show("更新服务列表时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                listView1.EndUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">操作类型，0显示列表，1启动，2停止，3设置</param>
        /// <param name="msg"></param>
        public void setState(int i, string msg)
        {
            // 修复：确保在UI线程中执行
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int, string>(setState), i, msg);
                return;
            }

            // 修复：检查窗体是否已释放
            if (this.IsDisposed || listView1.IsDisposed)
                return;

            try
            {
                if (msg.Contains("成功"))
                {
                    string[] ss = msg.Split(new char[] { '[', ']' });
                    if (ss.Length >= 2)
                    {
                        string serviceName = ss[1].Trim();

                        ListViewItem l = null;
                        foreach (ListViewItem lvi in listView1.Items)
                        {
                            // 修复：安全地检查SubItems
                            if (lvi.SubItems.Count > 5 && lvi.SubItems[5].Text == serviceName)
                            {
                                l = lvi;
                                break;
                            }
                        }

                        if (l != null)
                        {
                            switch (i)
                            {
                                case 1:
                                    if (l.SubItems.Count > 2)
                                        l.SubItems[2].Text = "正在运行";
                                    break;
                                case 2:
                                    if (l.SubItems.Count > 2)
                                        l.SubItems[2].Text = "已停止";
                                    break;
                                case 3:
                                    if (l.SubItems.Count > 3 && ss.Length > 3)
                                        l.SubItems[3].Text = ss[3];
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("设置服务状态失败: " + ex.Message);
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (listView1.FocusedItem == null)
                return;

            // 修复：验证连接状态
            if (con == null || !con.isConnected)
            {
                MessageBox.Show("连接已断开，无法操作服务", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ToolStripItem tsi = (ToolStripItem)e.ClickedItem;
            string text = tsi.Text;

            // 修复：使用异步方式处理服务操作，避免阻塞UI
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    switch (text)
                    {
                        case "刷新服务":
                            con.SendPacket(new SM_SERVICES_LIST(0, ""));
                            break;
                        case "启动服务":
                            // 修复：安全地获取服务名
                            if (listView1.FocusedItem.SubItems.Count > 5)
                            {
                                con.SendPacket(new SM_SERVICES_LIST(1, listView1.FocusedItem.SubItems[5].Text));
                            }
                            break;
                        case "停止服务":
                            if (listView1.FocusedItem.SubItems.Count > 5)
                            {
                                con.SendPacket(new SM_SERVICES_LIST(2, listView1.FocusedItem.SubItems[5].Text));
                            }
                            break;
                        case "设置自动":
                            if (listView1.FocusedItem.SubItems.Count > 5)
                            {
                                con.SendPacket(new SM_SERVICES_LIST(3, listView1.FocusedItem.SubItems[5].Text + "\t2"));
                            }
                            break;
                        case "设置手动":
                            if (listView1.FocusedItem.SubItems.Count > 5)
                            {
                                con.SendPacket(new SM_SERVICES_LIST(3, listView1.FocusedItem.SubItems[5].Text + "\t3"));
                            }
                            break;
                        case "禁用服务":
                            if (listView1.FocusedItem.SubItems.Count > 5)
                            {
                                con.SendPacket(new SM_SERVICES_LIST(3, listView1.FocusedItem.SubItems[5].Text + "\t4"));
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("服务操作失败: " + ex.Message);
                    this.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("服务操作失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }
    }
}

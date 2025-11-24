using AionCommons.Unilty;
using AionCommons.WinForm;
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
    internal partial class ProcessForm : Form
    {
        private AionConnection _con;

        internal ProcessForm(ref AionConnection con)
        {
            InitializeComponent();
            _con = con;
            Text = "[" + con.getIP() + "]的电脑进程";

        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = sender as System.Windows.Forms.ListView;
            // 检查点击的列是不是现在的排序列.
            if (e.Column == (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn)
            {
                // 重新设置此列的排序方法.
                if ((lv.ListViewItemSorter as ListViewColumnSorter).Order == SortOrder.Ascending)
                {
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = SortOrder.Descending;
                }
                else
                {
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = SortOrder.Ascending;
                }
            }
            else
            {
                // 设置排序列，默认为正向排序
                (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn = e.Column;
                (lv.ListViewItemSorter as ListViewColumnSorter).Order = SortOrder.Ascending;
            }
            // 用新的排序方法对ListView排序
            ((System.Windows.Forms.ListView)sender).Sort();
        }

        private void ProcessForm_Load(object sender, EventArgs e)
        {
            DoubleBuffer.DoubleBufferedControl(listView1, true);
            listView1.ListViewItemSorter = new ListViewColumnSorter();
            listView1.ColumnClick += new ColumnClickEventHandler(ListView_ColumnClick);

            lable_state.Text = "正在读取远程进程,请稍等...";

            // 修复：使用异步方式发送进程请求，避免阻塞UI
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (_con != null && _con.isConnected)
                    {
                        _con.SendPacket(new SM_RUNNING_PROCESSES());
                    }
                    else
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            lable_state.Text = "连接已断开，无法获取进程信息";
                        }));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("发送进程请求失败: " + ex.Message);
                    this.BeginInvoke(new Action(() =>
                    {
                        lable_state.Text = "发送进程请求失败: " + ex.Message;
                    }));
                }
            });
        }

        private void 刷新进程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lable_state.Text = "正在读取远程进程,请稍等...";

            // 修复：使用异步方式刷新进程，避免阻塞UI
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    if (_con != null && _con.isConnected)
                    {
                        _con.SendPacket(new SM_RUNNING_PROCESSES());
                    }
                    else
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            lable_state.Text = "连接已断开，无法刷新进程信息";
                        }));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("刷新进程失败: " + ex.Message);
                    this.BeginInvoke(new Action(() =>
                    {
                        lable_state.Text = "刷新进程失败: " + ex.Message;
                    }));
                }
            });
        }

        private void 结束进程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
                _con.SendPacket(new SM_RUNNING_PROCESSES(int.Parse(listView1.FocusedItem.Tag.ToString())));
        }


        public void ShowProcessList(string[] info, Image[] imgs)
        {
            // 修复：确保在UI线程中执行
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string[], Image[]>(ShowProcessList), info, imgs);
                return;
            }

            // 修复：检查窗体是否已释放
            if (this.IsDisposed || listView1.IsDisposed)
                return;

            try
            {
                // 修复：安全地清理和设置图像列表
                if (imageList1.Images.Count > 0)
                {
                    imageList1.Images.Clear();
                }

                if (imgs != null && imgs.Length > 0)
                {
                    imageList1.Images.AddRange(imgs);
                }

                listView1.BeginUpdate();
                listView1.Items.Clear();

                if (info != null && info.Length > 0)
                {
                    lable_state.Text = "正在加载 " + info.Length + " 个进程信息...";

                    // 修复：分批处理大量进程，避免UI冻结
                    const int batchSize = 50;
                    for (int batchStart = 0; batchStart < info.Length; batchStart += batchSize)
                    {
                        int batchEnd = Math.Min(batchStart + batchSize, info.Length);

                        for (int i = batchStart; i < batchEnd; i++)
                        {
                            try
                            {
                                string[] ts = info[i].Split('\t');

                                // 修复：验证数据格式
                                if (ts.Length < 4)
                                {
                                    System.Diagnostics.Debug.WriteLine("进程信息格式错误: " + info[i]);
                                    continue;
                                }

                                // 修复：安全地获取图像索引
                                int imageIndex = Math.Min(i, imageList1.Images.Count - 1);
                                if (imageIndex < 0) imageIndex = 0;

                                ListViewItem lvi = new ListViewItem(ts, imageIndex);
                                lvi.Tag = ts[1]; // PID
                                listView1.Items.Add(lvi);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("处理进程项失败: " + ex.Message);
                                continue;
                            }
                        }

                        // 修复：分批处理时允许UI响应
                        if (batchEnd < info.Length)
                        {
                            Application.DoEvents();
                            System.Threading.Thread.Sleep(10); // 短暂延迟，让UI保持响应
                        }
                    }

                    lable_state.Text = "已读取 " + listView1.Items.Count + " 个进程信息";
                }
                else
                {
                    lable_state.Text = "未获取到进程信息";
                }

                listView1.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("显示进程列表失败: " + ex.Message);
                lable_state.Text = "读取进程信息时发生错误: " + ex.Message;
                listView1.EndUpdate();
            }
        }

        public void RemoveFromListByPid(string pid)
        {
            AionRoy.Invoke(this, () =>
            {
                // 修复：循环条件错误，应该遍历所有项
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    ListViewItem lvi = listView1.Items[i];

                    // 修复：安全地检查Tag和SubItems
                    if (lvi != null && lvi.Tag != null && lvi.Tag.ToString().Equals(pid))
                    {
                        // 修复：安全地获取进程信息
                        string processName = lvi.SubItems.Count > 0 ? lvi.SubItems[0].Text : "未知";
                        string processPid = lvi.SubItems.Count > 1 ? lvi.SubItems[1].Text : pid;

                        listView1.Items.Remove(lvi);
                        lable_state.Text = "进程[" + processName + "]-PID[" + processPid + "]已成功结束掉";
                        break;
                    }
                }
            });
        }
    }
}

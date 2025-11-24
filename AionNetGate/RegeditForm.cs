using AionCommons.Unilty;
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
    public partial class RegeditForm : Form
    {
        private AionConnection con;

        public RegeditForm(ref AionConnection selectedConnection)
        {
            InitializeComponent();
            // TODO: Complete member initialization
            this.con = selectedConnection;

            DoubleBufferListView.DoubleBufferedListView(listView1, true);
        }

        private void RegeditForm_Load(object sender, EventArgs e)
        {
            imageList1.Images.Add(IconMake.myExtractIcon(15));
            imageList1.Images.Add(IconMake.myExtractIcon(3));
            imageList1.Images.Add(IconMake.myExtractIcon(4));

            treeView1.Nodes.Add(con.computerName);
            treeView1.Nodes[0].Nodes.Add("HKEY_CLASSES_ROOT", "HKEY_CLASSES_ROOT", 1, 2);
            treeView1.Nodes[0].Nodes.Add("HKEY_CURRENT_USER", "HKEY_CURRENT_USER", 1, 2);
            treeView1.Nodes[0].Nodes.Add("HKEY_LOCAL_MACHINE", "HKEY_LOCAL_MACHINE", 1, 2);
            treeView1.Nodes[0].Nodes.Add("HKEY_USERS", "HKEY_USERS", 1, 2);
            treeView1.Nodes[0].Nodes.Add("HKEY_CURRENT_CONFIG", "HKEY_CURRENT_CONFIG", 1, 2);
            treeView1.Nodes[0].Expand();

            this.Icon = IconMake.myExtractIcon(46);

            this.Text = "[" + con.GetHashCode() + "@" + con.getIP() + "]注册表编辑器";
        }

        internal void AddTreeList(TreeNode[] ts, ListViewItem[] values)
        {
            AionRoy.Invoke(treeView1, () =>
            {
                selectNode.Nodes.AddRange(ts);
                selectNode.Tag = values;

                listView1.Items.Clear();
                listView1.Items.AddRange(values);
            });
            ts = null;
            selectNode = null;
        }

        private TreeNode selectNode;

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.Tag != null)
                {
                    ListViewItem[] lv = (ListViewItem[])e.Node.Tag;
                    listView1.Items.Clear();
                    listView1.Items.AddRange(lv);
                }
                return;
            }

            if (e.Node.Parent != null && selectNode == null)
            {
                string s = e.Node.FullPath.Replace(con.computerName + "\\", "");

                selectNode = e.Node;

                con.SendPacket(new SM_REGEDIT_LIST(0, s));
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
            {
                Regedit reg = new Regedit();
                if (listView1.FocusedItem.SubItems[1].Text.Contains("SZ"))
                {
                    reg.Text = "编辑字符串";
                    reg.groupBox1.Hide();
                    reg.textBox2.Size = reg.textBox1.Size;
                    reg.Height -= 60;
                    reg.button1.Location = new Point(reg.button1.Location.X, reg.button1.Location.Y - 60);
                    reg.button2.Location = new Point(reg.button2.Location.X, reg.button2.Location.Y - 60);
                    reg.textBox2.Text = listView1.FocusedItem.SubItems[2].Text;
                }
                else if (listView1.FocusedItem.SubItems[1].Text.Contains("DWORD"))
                {
                    reg.Text = "编辑DWORD(32位)值";
                    reg.textBox2.Text = Convert.ToUInt32(listView1.FocusedItem.SubItems[2].Text.Split(' ')[0], 16).ToString("X");
                }
                else if (listView1.FocusedItem.SubItems[1].Text.Contains("QWORD"))
                {
                    reg.Text = "编辑DWORD(64位)值";
                    reg.textBox2.Text = Convert.ToUInt64(listView1.FocusedItem.SubItems[2].Text.Split(' ')[0], 16).ToString("X");
                }
                else if (listView1.FocusedItem.SubItems[1].Text.Contains("BINARY"))
                {
                    reg.Text = "编辑二进制数值";
                    reg.textBox2.Text = listView1.FocusedItem.SubItems[2].Text.Split(' ')[0];
                }

                reg.textBox1.Text = listView1.FocusedItem.SubItems[0].Text;

                if (reg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    listView1.FocusedItem.SubItems[2].Text = reg.textBox2.Text;
                    if (listView1.FocusedItem.SubItems[1].Text.Contains("WORD"))
                    {
                        if (reg.radioButton1.Checked)
                        {
                            if (reg.Text.Contains("DWORD"))
                                listView1.FocusedItem.SubItems[2].Text = Convert.ToUInt32(reg.textBox2.Text, 16).ToString("X8") + " (" + Convert.ToUInt32(reg.textBox2.Text, 16) + ")";
                            else if (Text.Contains("QWORD"))
                                listView1.FocusedItem.SubItems[2].Text = Convert.ToUInt64(reg.textBox2.Text, 16).ToString("X16") + " (" + Convert.ToUInt32(reg.textBox2.Text, 16) + ")";
                        }
                        else
                        {
                            if (reg.Text.Contains("DWORD"))
                                listView1.FocusedItem.SubItems[2].Text = Convert.ToUInt32(reg.textBox2.Text, 10).ToString("X8") + " (" + Convert.ToUInt32(reg.textBox2.Text, 10) + ")";
                            else if (Text.Contains("QWORD"))
                                listView1.FocusedItem.SubItems[2].Text = Convert.ToUInt64(reg.textBox2.Text, 10).ToString("X16") + " (" + Convert.ToUInt32(reg.textBox2.Text, 10) + ")";

                        }

                    }

                }
            }
        }
    }
}

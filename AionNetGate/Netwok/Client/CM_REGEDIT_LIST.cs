using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Netwok.Client
{
    internal class CM_REGEDIT_LIST : AbstractClientPacket
    {
        private TreeNode[] treeNodes;
        private ListViewItem[] lists;

        protected override void readImpl()
        {
            try
            {
                byte type = readC();
                if (type == 0)
                {
                    int size = readUH();
                    treeNodes = new TreeNode[size];
                    for (int i = 0; i < size; i++)
                    {
                        treeNodes[i] = new TreeNode(readS(), 1, 2);
                    }

                    int a = readUH();
                    lists = new ListViewItem[a];
                    for (int i = 0; i < a; i++)
                    {
                        string[] ss = readS().Split('\t');
                        lists[i] = new ListViewItem(ss, ss[1].Contains("SZ") ? 0 : 1);
                    }
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        protected override void runImpl()
        {
            ((AionConnection)getConnection()).regeditForm.AddTreeList(treeNodes, lists);
        }
    }
}

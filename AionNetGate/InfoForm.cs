using AionNetGate.Netwok;
using AionNetGate.Netwok.Client;
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
    internal partial class InfoForm : Form
    {
        private AionConnection con;
        internal InfoForm(ref AionConnection ac)
        {
            InitializeComponent();
            con = ac;
            Text = "[" + con.getIP() + "]的电脑信息";
        }

        private void InfoForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = "正在获取远程玩家的电脑信息...";
            con.SendPacket(new SM_COMPUTER_INFO(0));
        }

        internal void ShowInfo(ClientInfo ci)
        {
            AionRoy.Invoke(textBox1, () => {
                textBox1.Lines  = ci.toString();
            });
        }

        private void InfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            textBox1.Text = null;
        }
    }
}

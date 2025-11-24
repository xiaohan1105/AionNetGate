using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class Regedit : Form
    {
        public Regedit()
        {
            InitializeComponent();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                if (Text.Contains("DWORD"))
                    textBox2.Text = "" + Convert.ToUInt32(textBox2.Text, 16);
                else if (Text.Contains("QWORD"))
                    textBox2.Text = "" + Convert.ToUInt64(textBox2.Text, 16);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (Text.Contains("DWORD"))
                    textBox2.Text = Convert.ToUInt32(textBox2.Text, 10).ToString("X");
                else if (Text.Contains("QWORD"))
                    textBox2.Text = Convert.ToUInt64(textBox2.Text, 10).ToString("X");
            }
        }
    }
}

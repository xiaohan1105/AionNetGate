using CCWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class MainForm : Skin_Mac
    {
        public MainForm()
        {
            InitializeComponent();
            DoubleBuffer.DoubleBufferedControl(skinTabControl1,true);
        }

        private void TextBox_网关密匙_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox_launcherMD5_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void textBox_launcherMD5_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void button_清除MD5_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }


    public static class DoubleBuffer
    {
        /// <summary>  
        /// 双缓冲，解决闪烁问题  
        /// </summary>  
        /// <param name="lv"></param>  
        /// <param name="flag"></param>  
        public static void DoubleBufferedControl(this Control lv, bool flag)
        {
            Type lvType = lv.GetType();
            System.Reflection.PropertyInfo pi = lvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(lv, flag, null);
        }

    }
}

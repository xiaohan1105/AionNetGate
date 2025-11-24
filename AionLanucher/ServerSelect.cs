using AionLanucher.FormSkin;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher
{
    public partial class ServerSelect : SkinMain
    {
        public ServerSelect()
        {
            InitializeComponent();
            InitControl(radioButton1);
            InitControl(radioButton2);
            InitControl(buttonEx3);
        }

        private void ServerSelect_Load(object sender, EventArgs e)
        {
            buttonEx3.Enabled = false;
            label电信延时.Text = "正在检测延时";
            label网通延时.Text = "正在检测延时";

            Thread t = new Thread(ThreadDo);
            t.IsBackground = true;
            t.Start();
        }

        private void ThreadDo()
        {
            string delay = PingIP(Configs.Config.ServerIP_ONE);
            string delay2 = PingIP(Configs.Config.ServerIP_TWO);
            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                label电信延时.Text = delay;
                label网通延时.Text = delay2;
                buttonEx3.Enabled = true;
            }));
        }
        /// <summary>
        /// 确认选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEx3_Click(object sender, EventArgs e)
        {
            this.DialogResult = ((ButtonEx)sender).ADialogResult;
            Configs.Config.ServerIP = radioButton1.Checked ? Configs.Config.ServerIP_ONE : Configs.Config.ServerIP_TWO;

            //记住线路选中，那么保存到注册表
            if (checkBox1.Checked)
            {
                AionLanucher.Properties.Settings.Default.ServerIP = Configs.Config.ServerIP;
                AionLanucher.Properties.Settings.Default.Save();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton2.Checked = !radioButton1.Checked;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = !radioButton2.Checked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string PingIP(string ip)
        {
            string info = "延时检测失败";
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";           //設定程序名
                p.StartInfo.Arguments = "/c ping " + ip + " -n 2";    //設定程式執行參數
                p.StartInfo.UseShellExecute = false;        //關閉Shell的使用
                p.StartInfo.RedirectStandardInput = true;   //重定向標準輸入
                p.StartInfo.RedirectStandardOutput = true;  //重定向標準輸出
                p.StartInfo.RedirectStandardError = true;   //重定向錯誤輸出
                p.StartInfo.CreateNoWindow = true;          //設置不顯示窗口
                p.Start();//啟動
                p.WaitForExit();
                string str = p.StandardOutput.ReadToEnd();
                int i = str.LastIndexOf('=') + 1;
                string last = str.Substring(i, str.Length - i);
                if (last.Contains("ms"))
                    info = "延时 " + last.Trim().Replace("ms", " 毫秒");
                str = null;
                last = null;

                p.Close();
                p.Dispose();
            }
            catch (Exception)
            {
            }
            return info;
        }

    }
}

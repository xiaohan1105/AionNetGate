using AionCommons.Unilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class Md5Form : Form
    {
        public Md5Form(ref TextBox textbox)
        {
            InitializeComponent();
            this.textbox = textbox;
        }
        private TextBox textbox;

        private void skinButton2_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_path.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            skinButton1.Enabled = false;
            skinButton1.Text = "请稍等...";
            //Application.DoEvents();
            allfiles = new List<FileInfo>();//存放文件容器
            alldirectory = new List<DirectoryInfo>();//存放文件夹容器
            ListFiles(new DirectoryInfo(@textBox_path.Text));//遍历目录下所有文件
            alldirectory.Clear();
            textbox.Clear();
            Thread t = new Thread(Produce);
            t.IsBackground = true;
            t.Start();
        }

        private void Produce()
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (FileInfo fsi in allfiles)
            {
                string fullname = fsi.FullName.ToLower();
                if ((!fullname.Contains("objects\\pc")
                    && !fullname.Contains("data\\china")
                    && !fullname.Contains("data\\items")
                    && !fullname.Contains("data\\skills")
                    && !fullname.Contains("data\\npcs"))
                    || fullname.Contains("mesh_textures")
                    )
                    continue;
                else
                    files.Add(fsi);
            }
            allfiles.Clear();

            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                progressBar1.Maximum = files.Count;
                progressBar1.Value = 0;
                textbox.Text = "";
                skinButton1.Text = "正在生成...";
            }));

            foreach (FileInfo fsi in files)
            {
                string fullname = fsi.FullName.ToLower();
                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    string line = fullname.Replace(textBox_path.Text.ToLower() + "\\", "") + "|" + HashEncrypt.CretaeMD5(fullname) + Environment.NewLine;

                    textbox.AppendText(line);
                    progressBar1.Value += 1;
                    Label2.Text = ((double)progressBar1.Value / (double)progressBar1.Maximum * 100).ToString("f2") + "%";
                    //  Application.DoEvents();

                }));
            }
            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                skinButton1.Text = "已完成";
                Thread.Sleep(1000);
                this.Close();
            }));
        }

        private static List<FileInfo> allfiles;//存放文件容器
        private static List<DirectoryInfo> alldirectory;//存放文件夹容器
        /// <summary>
        /// 遍历文件目录
        /// </summary>
        /// <param name="info"></param>
        public static void ListFiles(FileSystemInfo info)
        {
            if (!info.Exists)
                return;
            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录 
            if (dir == null)
                return;
            alldirectory.Add(dir);
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (FileSystemInfo fsi in files)
            {
                FileInfo file = fsi as FileInfo;
                //是文件 
                if (file != null)
                    allfiles.Add(file);
                //对于子目录，进行递归调用 
                else
                    ListFiles(fsi);
            }
        }



    }
}

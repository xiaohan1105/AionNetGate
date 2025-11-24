
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class UpdateFile : Form
    {
        internal UpdateFile(List<AionFile> upfiles)
        {
            InitializeComponent();
            this.upfiles = upfiles;
        }

        private List<AionFile> upfiles;


        private void UpdateFile_Load(object sender, EventArgs e)
        {
            int allsize = 0;
            int i = 1;
            richTextBox1.AppendText("可更新的文件数量" + upfiles.Count + "个\r\n-------------------------------------------\r\n");
            foreach (AionFile a in upfiles)
            {
                allsize += a.length;
                richTextBox1.AppendText("[" + i + "]" + a.fileName + " 文件大小：" + getSize(a.length) + "\r\n");
                i++;

            }
            progressBar2.Maximum = allsize;


            Thread s = new Thread(down);
            s.IsBackground = true;
            s.Start();
        }

        private string getSize(int size)
        {
            if (size > (1024 * 1024))
                return ((float)size / (float)1024 / (float)1024).ToString("f2") + " Mb";
            else if (size > 1024)
                return ((float)size / (float)1024).ToString("f2") + " Kb";
            else
                return size + " B";
        }

        private bool isFinished = false;
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            if (!isFinished)
            {
                if (MessageBox.Show("请等待更新完成！", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //Application.Exit();
                }
            }
            else
            {
                Application.Exit();
            }
        }


        private void down()
        {
            int i = 0;
            foreach (AionFile af in upfiles)
            {
                i++;
                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    label3.Text = "更新文件数:" + i + " / " + upfiles.Count;
                }));
                DownloadFile("http://115.239.227.75:88/AionNewGate/update.zip", af, progressBar1, label1, label2);
            }

            AionRoy.Invoke(this, new AionRoy.Handler(delegate()
            {
                progressBar1.Value = progressBar1.Maximum;
                progressBar2.Value = progressBar2.Maximum;
            }));

            if (File.Exists(Application.StartupPath + "\\通用网关.exe.tmp"))
            {
                File.WriteAllText(Application.StartupPath + "\\update.bat", string.Format(@"
                        @echo off
                        :selfkill
                        attrib -a -r -s -h ""{0}""
                        del ""{0}""
                        if exist ""{0}"" goto selfkill
                        move ""{1}"" ""{0}""
                        ping 127.0.0.1 -n 1 >nul
                        start """" ""{0}""
                        del %0 ", Path.GetFileName(Application.ExecutablePath), Application.StartupPath + "\\通用网关.exe.tmp"), Encoding.GetEncoding("GB2312"));

                // 启动自删除批处理文件
                ProcessStartInfo info = new ProcessStartInfo(Application.StartupPath + "\\update.bat");
                info.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(info);

                // 强制关闭当前进程
                Environment.Exit(0);
            }
            isFinished = true;
            this.Close();
        }

        private bool killProcess(string name)
        {
            bool iskilled = false;
            Process[] ps = Process.GetProcessesByName(name);
            foreach (Process p in ps)
            {
                try
                {
                    p.Kill();
                    iskilled = true;
                }
                catch
                {
                    iskilled = false;
                }
            }
            return iskilled;

        }

        /// <summary>        
        /// c#,.net 下载文件        
        /// </summary>        
        /// <param name="URL">下载文件地址</param>       
        /// 
        /// <param name="Filename">下载后的存放地址</param>        
        /// <param name="Prog">用于显示的进度条</param>        
        /// 
        private void DownloadFile(string URL, AionFile af, ProgressBar prog, Label downinfo, Label speedText)
        {
            Stream saveFile = null;
            try
            {
                HttpWebRequest Myrq = (HttpWebRequest)HttpWebRequest.Create(URL);
                Myrq.AddRange(af.start, af.end - 1);//设置下载文件中文件数据位置
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;

                Stream st = myrp.GetResponseStream();//网络流

                string path = Path.GetDirectoryName(Application.StartupPath + "\\" + af.fileName);
                Directory.CreateDirectory(path);

                if (af.fileName.EndsWith("登录转发.exe"))
                {
                    if (File.Exists(af.fileName))
                    {
                        try
                        {
                            if (killProcess("登录转发.exe"))
                            {
                                Thread.Sleep(2000);
                                File.Delete(Application.StartupPath + "\\" + af.fileName);
                            }
                        }
                        catch
                        {


                        }
                    }
                }

                saveFile = new FileStream(Application.StartupPath + "\\" + (af.fileName.EndsWith("通用网关.exe") ? af.fileName + ".tmp" : af.fileName), FileMode.Create);//下载保存文件


                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    prog.Maximum = (int)totalBytes;//设置进度条范围 = 文件尺寸(这里应该被压缩之前的文件大小)
                    prog.Value = 0;
                }));

                DateTime starttime = DateTime.Now;
                long totalDownloadedByte = 0;

                byte[] by = new byte[1024];
                int osize = 0;

                while ((osize = st.Read(by, 0, by.Length)) > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;

                    saveFile.Write(by, 0, osize);//写出文件流

                    AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                    {
                        prog.Value = (int)totalDownloadedByte;//显示进度条  解压后大小
                        progressBar2.Value += osize;
                    }));

                    string nowtime = (DateTime.Now - starttime).ToString();
                    string[] strs = nowtime.Split(':');

                    double t = Convert.ToDouble(strs[2]) + Convert.ToDouble(strs[1]) * 60 + Convert.ToDouble(strs[0]) * 3600;

                    AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                    {
                        downinfo.Text = "正在更新" + ":" + af.fileName;
                        double speeds = (totalDownloadedByte / 1024 / t);
                        if (speeds >= 0)
                            speedText.Text = "速度" + ":" + (totalDownloadedByte / 1024 / t).ToString("f2") + "Kb/s";
                        else
                            speedText.Text = "速度" + ":0 kb/s";
                    }));

                    Application.DoEvents();//必须加注这句代码，否则label1将因为循环执行太快而来不及显示信息
                }

                st.Close();
                myrp.Close();
                Myrq.Abort();
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e.ToString(), "更新文件错误");
            }
            finally
            {
                if (saveFile != null)
                    saveFile.Close();

                AionRoy.Invoke(this, new AionRoy.Handler(delegate()
                {
                    downinfo.Text = "已完成更新";
                }));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }

    class AionFile
    {
        private string filename;
        private string Md5;
        private int End;
        private int Start;
        private int Or_length;

        private string localMD5;

        public AionFile(string fileName, string md5, int start, int end)
        {
            this.filename = fileName;
            this.Md5 = md5;
            this.Start = start;
            this.End = end;
        }
        public AionFile(string fileName, string md5, int start, int end, int or_length)
        {
            this.filename = fileName;
            this.Md5 = md5;
            this.Start = start;
            this.End = end;
            this.Or_length = or_length;
        }
        /// <summary>
        /// 文件相对名字,如：data\china\items.pak
        /// </summary>
        public string fileName
        {
            get { return filename; }
            set { filename = value; }
        }
        /// <summary>
        /// 文件MD5值
        /// </summary>
        public string md5
        {
            get { return Md5; }
            set { Md5 = value; }
        }
        public string LocalMD5
        {
            get { return localMD5; }
            set { localMD5 = value; }
        }
        /// <summary>
        /// 文件位于下载文件中的起始位置
        /// </summary>
        public Int32 end
        {
            get { return End; }
            set { End = value; }
        }
        /// <summary>
        /// 文件位于下载文件中的结束位置
        /// </summary>
        public Int32 start
        {
            get { return Start; }
            set { Start = value; }
        }
        /// <summary>
        /// ZIP压缩前文件原长度
        /// </summary>
        public Int32 or_length
        {
            get { return Or_length; }
            set { Or_length = value; }
        }
        //文件长度
        public Int32 length
        {
            get { return End - (Start - 1); }
        }


        public string toString()
        {
            return filename + "|" + Md5 + "|" + Start.ToString() + "|" + End.ToString() + "\r\n";
        }
    }
}

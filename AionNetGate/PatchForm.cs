using AionCommons.Config;
using AionCommons.Unilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class PatchForm : Form
    {
        public PatchForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 选择补丁目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.SelectedPath = @"d:\客户端补丁"; //　设置打开目录选择对话框时默认的目录 
            folderBrowser.ShowNewFolderButton = false; //是否显示新建文件夹按钮 
            folderBrowser.Description = "请选择客户端补丁的目录";//描述弹出框功能 

            folderBrowser.ShowDialog();　// 打开目录选择对话框 
            textBox1.Text = folderBrowser.SelectedPath;　// 返回用户选择的目录地址
        }
        /// <summary>
        /// 选择生成目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.SelectedPath = @"d:\AionLight2013\AionWeb\update"; //　设置打开目录选择对话框时默认的目录 
            folderBrowser.ShowNewFolderButton = true; //是否显示新建文件夹按钮 
            folderBrowser.Description = "请选择补丁生成到的目录。建议选择网站目录下某个空文件夹内";//描述弹出框功能 

            folderBrowser.ShowDialog();　// 打开目录选择对话框 
            textBox2.Text = folderBrowser.SelectedPath;　// 返回用户选择的目录地址
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

        //获取所有文件名
        private string getMD5(string dir)
        {
            StringBuilder md5 = new StringBuilder();
            foreach (string file in Directory.GetFiles(dir))
            {
                md5.AppendLine(file + "|" + HashEncrypt.CretaeMD5(file));
            }
            return md5.ToString();
        }

        private const long BUFFER_SIZE = 20480;



        /// <summary>
        /// 窗体初始化加载配置文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PatchProduceForm_Load(object sender, EventArgs e)
        {
            allfiles = new List<FileInfo>();//存放文件容器
            alldirectory = new List<DirectoryInfo>();//存放文件夹容器

            INIFile myini = new INIFile(Application.StartupPath + "\\config\\makepatch.ini");
            textBox1.Text = myini.Read("PATH_CONFIG", "sourcepath");
            textBox2.Text = myini.Read("PATH_CONFIG", "targetpath");
        }



        #region 删除这个目录下的所有子目录和文件
        /// <summary>
        /// 删除这个目录下的所有文件及文件夹
        /// </summary>
        /// <param name="strPath">指定目录</param>
        private void DeleteDirectoryAndFiles(string strPath)
        {
            //删除这个目录下的所有子目录
            if (Directory.GetDirectories(strPath).Length > 0)
            {
                foreach (string var in Directory.GetDirectories(strPath))
                {
                    //DeleteDirectory(var);
                    Directory.Delete(var, true);
                    //DeleteDirectory(var);
                }
            }
            //删除这个目录下的所有文件
            if (Directory.GetFiles(strPath).Length > 0)
            {
                foreach (string var in Directory.GetFiles(strPath))
                {
                    File.Delete(var);
                }
            }
        }
        #endregion

        private void skinButton1_Click(object sender, EventArgs e)
        {
            allfiles = new List<FileInfo>();//存放文件容器
            alldirectory = new List<DirectoryInfo>();//存放文件夹容器
            Thread t = new Thread(new ThreadStart(Produce));
            t.IsBackground = true;
            t.Start();

            INIFile myini = new INIFile(Application.StartupPath + "\\config\\makepatch.ini");
            myini.Write("PATH_CONFIG", "sourcepath", textBox1.Text);
            myini.Write("PATH_CONFIG", "targetpath", textBox2.Text);
        }

        private void Produce()
        {

            if (!Directory.Exists(@textBox1.Text))
            {
                MessageBox.Show("补丁目录" + @textBox1.Text + "不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Directory.CreateDirectory(textBox2.Text);//创建新的目录

            if (MessageBox.Show("更新补丁将会清空" + @textBox2.Text + "目录，继续？", "友情提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            try
            {
                //删除补丁目录
                //删除这个目录下的所有子目录
                if (Directory.GetDirectories(@textBox2.Text).Length > 0)
                {
                    foreach (string var in Directory.GetDirectories(@textBox2.Text))
                    {
                        //DeleteDirectory(var);
                        Directory.Delete(var, true);
                        //DeleteDirectory(var);
                    }
                }
                //删除这个目录下的所有文件
                if (Directory.GetFiles(@textBox2.Text).Length > 0)
                {
                    foreach (string var in Directory.GetFiles(@textBox2.Text))
                    {
                        File.Delete(var);
                    }
                }
                if (File.Exists(@textBox2.Text + "\\update.dat"))
                {
                    MessageBox.Show("无法删除老的补丁文件" + (@textBox2.Text + "\\update.dat") + "\n可能该文件正在被玩家登录器下载中\n您可以尝试停止网站服务后再生成补丁\n然后等生成完补丁后再启动网站服务", "错误");
                    return;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("清理补丁目录失败，建议选择其他目录生成：" + e.Message, "错误");
                return;
            }

            


            ListFiles(new DirectoryInfo(@textBox1.Text));//遍历目录下所有文件


            AionRoy.Invoke(this, new AionRoy.Handler(delegate() { skinButton1.Enabled = false; progressBar1.Maximum = allfiles.Count; progressBar1.Value = 0; skinButton1.Text = "正在打包文件..."; }));

            //以合并后的文件名称和打开方式来创建、初始化FileStream文件流
            FileStream TempUpdate = new FileStream(textBox2.Text + "\\update.temp", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            string info = "";
            /*循环合并小文件，并生成合并文件 */
            int whichPoint = 0;

            foreach (FileInfo fsi in allfiles)
            {
                string toFullPath = fsi.FullName.Replace(@textBox1.Text, @textBox2.Text) + ".zip";
                string path = fsi.DirectoryName.Replace(@textBox1.Text, @textBox2.Text); ;
                //压缩文件

                Directory.CreateDirectory(path);//创建新的目录

                FileStream toCompress = new FileStream(toFullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                GZipStream zipStream = new GZipStream(toCompress, CompressionMode.Compress, true);

                //读取被压缩文件
                FileStream ToBeCompress = new FileStream(fsi.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
                long FileLength = ToBeCompress.Length;//给登陆器下载文件时设置进度条最大容量的

                long bufferSize = ToBeCompress.Length < 2048000 ? ToBeCompress.Length : 2048000;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                while ((bytesRead = ToBeCompress.Read(buffer, 0, buffer.Length)) != 0)
                {
                    zipStream.Write(buffer, 0, bytesRead);
                }
                zipStream.Close();//关闭压缩流
                ToBeCompress.Close();//关闭文件流
                toCompress.Close();//关闭文件流


                //获取文件信息，并合并文件
                FileStream TempStream = new FileStream(toFullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                info += fsi.FullName.Replace(@textBox1.Text + "\\", "") + "|" + HashEncrypt.CretaeMD5(fsi.FullName) + "|" + whichPoint.ToString() + "|" + TempStream.Length + "|" + FileLength + ";";
                whichPoint += (int)TempStream.Length;
                //合并文件
                bufferSize = TempStream.Length < 2048000 ? TempStream.Length : 2048000;
                buffer = new byte[bufferSize];
                bytesRead = 0;
                while ((bytesRead = TempStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    TempUpdate.Write(buffer, 0, bytesRead);
                }
                TempStream.Close();
                //删除ZIP文件
                File.Delete(toFullPath);

                AionRoy.Invoke(this, new AionRoy.Handler(delegate() { progressBar1.Value += 1; }));
                Application.DoEvents();
            }
            whichPoint = 0;
            //关闭文件流
            TempUpdate.Close();

            string[] dirs = Directory.GetDirectories(textBox2.Text);
            foreach (string s in dirs)
            {
                Directory.Delete(s, true);
            }


            //最终合并的文件
            FileStream Add = new FileStream(textBox2.Text + "\\update.dat", FileMode.OpenOrCreate, FileAccess.Write);
            //以合并后的文件名称和打开方式来创建、初始化FileStream文件流
            BinaryWriter AddW = new BinaryWriter(Add);

            AddW.Write((int)Encoding.UTF8.GetByteCount(info));//开头写入 4 个字符 的 info 长度信息
            AddW.Write(Encoding.UTF8.GetBytes(info));

            //读取缓存的文件
            FileStream Temp = new FileStream(textBox2.Text + "\\update.temp", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            AionRoy.Invoke(this, new AionRoy.Handler(delegate() { progressBar1.Value = 0; progressBar1.Maximum = (int)Temp.Length; skinButton1.Text = "正在合并文件..."; }));

            long readSize = Temp.Length < 20480 ? Temp.Length : 20480;
            byte[] readbuffer = new byte[readSize];
            int Reads = 0;
            while ((Reads = Temp.Read(readbuffer, 0, readbuffer.Length)) != 0)
            {
                AddW.Write(readbuffer, 0, Reads);
                AionRoy.Invoke(this, new AionRoy.Handler(delegate() { progressBar1.Value += Reads; }));
                Application.DoEvents();
            }

            Temp.Close();
            AddW.Close();
            Add.Close();

            File.Delete(textBox2.Text + "\\update.temp");
            // File.WriteAllText(textBox2.Text + "\\log.txt", info);

            AionRoy.Invoke(this, new AionRoy.Handler(delegate() { skinButton1.Enabled = true; progressBar1.Value = progressBar1.Maximum; skinButton1.Text = "生成补丁包"; }));

            MessageBox.Show("生成成功！");

            System.Diagnostics.Process.Start("explorer", "/select," + textBox2.Text + "\\update.dat");

            AionRoy.Invoke(this, new AionRoy.Handler(delegate() { this.Close(); }));
        }

        #region 不能用的代码
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourcepath">文件源目录</param>
        /// <param name="copytopath">压缩后文件目录</param>
        public void CompresseFiles(string sourcepath, string copytopath)
        {
            copytopath += "\\";
            //获取被压缩文件目录下的子文件和子目录
            FileSystemInfo[] fsinfos = new DirectoryInfo(sourcepath).GetFileSystemInfos();

            Queue<FileSystemInfo> Folders = new Queue<FileSystemInfo>(fsinfos);

            copytopath = (copytopath.LastIndexOf(Path.DirectorySeparatorChar) == copytopath.Length - 1) ? copytopath : copytopath + Path.DirectorySeparatorChar + Path.GetFileName(sourcepath);
            //   MessageBox.Show(copytopath);

            //创建压缩存放目录
            Directory.CreateDirectory(copytopath);

            StringBuilder md5 = new StringBuilder();
            while (Folders.Count > 0)
            {
                FileSystemInfo atom = Folders.Dequeue();
                FileInfo sourcefile = atom as FileInfo;
                if (sourcefile == null)//表示 为目录
                {
                    DirectoryInfo directory = atom as DirectoryInfo;
                    Directory.CreateDirectory(directory.FullName.Replace(sourcepath, copytopath));
                    foreach (FileSystemInfo nextatom in directory.GetFileSystemInfos())
                        Folders.Enqueue(nextatom);
                }
                else
                {
                    string sourcefilename = sourcefile.FullName;
                    string zipfilename = sourcefile.FullName.Replace(sourcepath, copytopath) + ".zip";

                    string md5name = sourcefilename.Replace(sourcepath + "\\", "");
                    md5.AppendLine(md5name + "|" + HashEncrypt.CretaeMD5(sourcefilename));

                    FileStream sourceStream = null;
                    FileStream destinationStream = null;
                    GZipStream compressedStream = null;
                    try
                    {
                        // Read the bytes from the source file into a byte array
                        sourceStream = new FileStream(sourcefilename, FileMode.Open, FileAccess.Read, FileShare.Read);
                        // Open the FileStream to write to
                        destinationStream = new FileStream(zipfilename, FileMode.OpenOrCreate, FileAccess.Write);
                        // Create a compression stream pointing to the destiantion stream
                        compressedStream = new GZipStream(destinationStream, CompressionMode.Compress, true);

                        long bufferSize = sourceStream.Length < BUFFER_SIZE ? sourceStream.Length : BUFFER_SIZE;
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead = 0;
                        long bytesWritten = 0;
                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            compressedStream.Write(buffer, 0, bytesRead);
                            bytesWritten += bufferSize;
                        }
                    }
                    catch (ApplicationException)
                    {
                        continue;
                    }
                    finally
                    {
                        // Make sure we allways close all streams
                        if (sourceStream != null)
                            sourceStream.Close();
                        if (compressedStream != null)
                            compressedStream.Close();
                        if (destinationStream != null)
                            destinationStream.Close();
                    }
                    progressBar1.Value += 1;//进度条

                }
            }
            File.WriteAllText(@copytopath + "\\aionfiles.txt", md5.ToString());
        }

        /// <summary>
        /// 生成补丁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@textBox1.Text))
            {
                MessageBox.Show("目录" + @textBox1.Text + "不存在！", "错误");
                return;
            }

            ListFiles(new DirectoryInfo(@textBox1.Text));//遍历目录下所有文件

            progressBar1.Maximum = allfiles.Count;

            Directory.CreateDirectory(@textBox2.Text);//创建新的目录

            if (File.Exists(@textBox2.Text + "\\update.dat"))
            {
                if (MessageBox.Show("更新补丁将会清空" + @textBox2.Text + "目录，继续？", "友情提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                //删除补丁目录
                if (Directory.Exists(@textBox2.Text))
                {
                    Directory.Delete(@textBox2.Text, true);
                    Directory.CreateDirectory(@textBox2.Text);
                }
            }

            FileStream AddStream = new FileStream(@textBox2.Text + "\\update.tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            GZipStream fileStream = new GZipStream(AddStream, CompressionMode.Compress, true);
            string info = "";
            /*循环合并小文件，并生成合并文件 */
            long whichPoint = 0;
            long zipfile = 0;
            foreach (FileInfo fsi in allfiles)
            {
                long bytesWritten = 0;
                FileStream TempStream = new FileStream(fsi.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                //if (fsi.FullName.ToLower().EndsWith(".pak"))
                //{
                long bufferSize = TempStream.Length < BUFFER_SIZE ? TempStream.Length : BUFFER_SIZE;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                while ((bytesRead = TempStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    //  bytesWritten += bytesRead;
                }
                bytesWritten = AddStream.Length - zipfile;
                zipfile = AddStream.Length;
                info += fsi.FullName.Replace(@textBox1.Text + "\\", "") + "|" + HashEncrypt.CretaeMD5(fsi.FullName) + "|" + whichPoint.ToString() + "|" + bytesWritten + ";";
                //}
                //else
                //{
                //    byte[] b = CompressFile(TempStream);
                //    AddStream.Write(b, 0, b.Length);

                //    info += fsi.FullName.Replace(@textBox1.Text + "\\", "") + "|" + MD5.CretaeMD5(fsi.FullName) + "|" + whichPoint.ToString() + "|" + b.Length + "|" + TempStream.Length + ";";
                //}

                //信息：  data\china\items\items.pak|434e314c32422424241dd2233232|0|21432131;

                whichPoint += bytesWritten;
                TempStream.Close();

                progressBar1.Value += 1;
            }
            fileStream.Close();
            AddStream.Close();
            whichPoint = 0;

            FileStream TempFile = new FileStream(textBox2.Text + "\\update.tmp", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            FileStream updateFile = new FileStream(textBox2.Text + "\\update.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            BinaryReader br = new BinaryReader(TempFile);
            BinaryWriter bw = new BinaryWriter(updateFile);

            byte[] infos = Encoding.UTF8.GetBytes(info);

            bw.Write((int)infos.Length);//开头写入 4 个字符 的 info 长度信息
            bw.Write(infos);
            bw.Write(br.ReadBytes((int)TempFile.Length));


            bw.Close();
            br.Close();

            File.Delete(@textBox2.Text + "\\update.tmp");

            //关闭FileStream文件流
            richTextBox1.AppendText(info);
            MessageBox.Show("生成成功！");
            //  this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FileStream updateFile = new FileStream(@textBox2.Text + "\\update.dat", FileMode.Open, FileAccess.Read, FileShare.None);
            BinaryReader br = new BinaryReader(updateFile);

            Int32 infolength = br.ReadInt32();

            byte[] infoBytes = br.ReadBytes(infolength);

            string info = Encoding.UTF8.GetString(infoBytes);

            richTextBox2.AppendText(info);

            string[] FileInfo = info.Split(';');


            foreach (string fileinfo in FileInfo)
            {
                if (!fileinfo.Contains("|"))
                    continue;
                string[] ss = fileinfo.Split('|');
                //信息：  data\china\items\items.pak|434e314c32422424241dd2233232|0|21432131;
                int start = Int32.Parse(ss[2]);
                br.BaseStream.Seek(start + infolength + 4, SeekOrigin.Begin);

                string filename = @"D:\A\" + ss[0];
                if (File.Exists(filename))
                    File.Delete(filename);
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                FileStream fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                BinaryWriter b = new BinaryWriter(fs);
                byte[] read = new byte[long.Parse(ss[3])];
                br.Read(read, 0, read.Length);
                b.Write(read);
                b.Close();
                fs.Close();

            }
            br.Close();
            updateFile.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@textBox1.Text))
            {
                MessageBox.Show("目录" + @textBox1.Text + "不存在！", "错误");
                return;
            }

            ListFiles(new DirectoryInfo(@textBox1.Text));
            progressBar1.Maximum = allfiles.Count;

            Directory.CreateDirectory(@textBox2.Text);

            if (File.Exists(@textBox2.Text + "\\update.dat"))
            {
                if (MessageBox.Show("更新补丁将会清空" + @textBox2.Text + "目录，继续？", "友情提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
                File.Delete(@textBox2.Text + "\\update.dat");
            }

            FileStream AddStream = new FileStream(textBox2.Text + "\\update.dat", FileMode.OpenOrCreate, FileAccess.Write);
            //以合并后的文件名称和打开方式来创建、初始化FileStream文件流
            BinaryWriter AddWriter = new BinaryWriter(AddStream);

            string info = "";
            /*循环合并小文件，并生成合并文件 */
            int whichPoint = 0;
            foreach (FileInfo fsi in allfiles)
            {
                FileStream TempStream = new FileStream(fsi.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                info += fsi.FullName.Replace(@textBox1.Text + "\\", "") + "|" + HashEncrypt.CretaeMD5(fsi.FullName) + "|" + whichPoint.ToString() + "|" + TempStream.Length + ";";
                whichPoint += (int)TempStream.Length;
                TempStream.Close();
            }
            whichPoint = 0;
            AddWriter.Write((int)Encoding.UTF8.GetByteCount(info));//开头写入 4 个字符 的 info 长度信息
            AddWriter.Write(Encoding.UTF8.GetBytes(info));

            foreach (FileInfo fsi in allfiles)
            {
                FileStream TempStream = new FileStream(fsi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                //以小文件所对应的文件名称和打开模式来初始化FileStream文件流，起读取分割作用
                BinaryReader TempReader = new BinaryReader(TempStream);
                //用FileStream文件流来初始化BinaryReader文件阅读器，也起读取分割文件作用
                AddWriter.Write(TempReader.ReadBytes((int)TempStream.Length));
                //读取分割文件中的数据，并生成合并后文件
                TempReader.Close();
                //关闭BinaryReader文件阅读器
                TempStream.Close();
                //关闭FileStream文件流
                progressBar1.Value += 1;
                //显示合并进程
            }
            AddWriter.Close();
            //关闭BinaryWriter文件书写器
            AddStream.Close();
            //关闭FileStream文件流

            MessageBox.Show("生成成功！");
        }
        #endregion

        private void PatchProduceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            INIFile myini = new INIFile(Application.StartupPath + "\\config\\makepatch.ini");
            myini.Write("PATH_CONFIG", "sourcepath", textBox1.Text);
            myini.Write("PATH_CONFIG", "targetpath", textBox2.Text);
        }


    }
}

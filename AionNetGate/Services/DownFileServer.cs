using AionCommons.LogEngine;
using AionNetGate.Configs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Services
{
    public class DownFileServer
    {
        private static Logger log = LoggerFactory.getLogger();
        /// <summary>
        /// 静态化类
        /// </summary>
        public static DownFileServer Instance = new DownFileServer();

        /// <summary>
        /// 存放需要下载的东西
        /// </summary>
        private List<ListView> waitForDownloadOrUpload;

        /// <summary>
        /// 是否已启动
        /// </summary>
        private bool _isRun = false;

        /// <summary>
        /// 服务端监听总Socket
        /// </summary>
        private Socket _svrSock;

        public DownFileServer()
        {
            waitForDownloadOrUpload = new List<ListView>();
        }
        /// <summary>
        /// 添加下载列表
        /// </summary>
        /// <param name="lvi"></param>
        public void AddTask(ref ListView lvi)
        {
            if (!waitForDownloadOrUpload.Contains(lvi))
                waitForDownloadOrUpload.Add(lvi);
        }

        public void RemoveTask(ref ListView lvi)
        {
            if (waitForDownloadOrUpload.Contains(lvi))
                waitForDownloadOrUpload.Remove(lvi);
        }



        private ListView getListViewByCode(int code, out ListViewItem lvi)
        {
            ListView listview = null;
            ListViewItem listviewitem = null;
            foreach (ListView lv in waitForDownloadOrUpload)
            {
                AionRoy.Invoke(lv, () =>
                {
                    foreach (ListViewItem l in lv.Items)
                    {
                        if (l.GetHashCode() == code)
                        {
                            listview = lv;
                            listviewitem = l;
                            break;
                        }
                    }
                });
            }
            lvi = listviewitem;
            return listview;
        }

        public void Start()
        {
            if (_isRun)
                return;
            try
            {
                //初始化socket 
                _svrSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _svrSock.Bind(new IPEndPoint(IPAddress.Parse(Config.server_ip), Config.down_port));
                //开始监听 
                _svrSock.Listen(2);

                _isRun = true;

                _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

                log.info("文件服务器已启动，监听端口：" + Config.down_port);

            }
            catch (Exception e)
            {
                log.error(e.Message);
            }
        }

        public void Stop()
        {
            if (!_isRun)
                return;

            _isRun = false;

            try
            {
                if (_svrSock != null)
                {
                    _svrSock.Close();
                    _svrSock = null;
                }
            }
            catch (Exception ex)
            {
                log.error("关闭文件服务器Socket时发生错误: " + ex.Message);
            }
        }

        /// <summary>
        /// 客户端连接处理函数
        /// </summary>
        /// <param name="iar">欲建立服务器连接的Socket对象</param>
        private void AcceptConn(IAsyncResult iar)
        {
            //如果服务器停止了服务,就不能再接收新的客户端
            if (!_isRun)
            {
                return;
            }
            try
            {
                //继续接受客户端
                if (_svrSock != null)
                    _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
            }
            catch (ObjectDisposedException)
            {
                // Socket已被关闭，忽略此异常
                return;
            }
            catch (Exception e)
            {
                log.error("继续接受客户端连接时发生错误: " + e.Message);
                return;
            }

            //接受一个客户端的连接请求
            Socket oldserver = (Socket)iar.AsyncState;
            Socket newSocket = null;
            try
            {
                newSocket = oldserver.EndAccept(iar);
                if (newSocket != null && newSocket.Connected)
                {
                    log.info("文件服务已收到一个请求[{0}]", newSocket.RemoteEndPoint.ToString());
                    using (NetworkStream ns = new NetworkStream(newSocket, false)) // false表示不拥有Socket
                    {
                        if (ns.CanRead)
                        {
                            int type = ns.ReadByte();
                            //读取首部4字节获取 hashCode值 以 获得ListviewItem
                            byte[] bs = new byte[4];
                            int read = ns.Read(bs, 0, 4);
                            int hashcode = BitConverter.ToInt32(bs, 0);
                            ListViewItem lvi;
                            ListView lv = getListViewByCode(hashcode, out lvi);


                            if (type == 0) //文件下载
                            {
                                DownFile(ns, lv, lvi);
                            }
                            else if (type == 1)
                            {
                                DownFolder(ns, lv, lvi);
                            }
                            else if (type == 2)
                            {
                                UploadFile(ns, lv, lvi);
                            }
                            else if (type == 3)
                            {
                                UploadFolder(ns, lv, lvi);
                            }
                        }
                        // ns.Close() 由using自动调用
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket已被关闭，忽略
            }
            catch (Exception e)
            {
                log.error("处理文件请求时发生错误: " + e.Message);
            }
            finally
            {
                // 确保Socket被正确关闭
                if (newSocket != null)
                {
                    try
                    {
                        if (newSocket.Connected)
                            newSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                    try
                    {
                        newSocket.Close();
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="ns"></param>
        private void UploadFile(NetworkStream ns, ListView lv, ListViewItem lvi)
        {
            DownFileInfo df = (DownFileInfo)lvi.Tag;
            AionRoy.Invoke(lv, () =>
            {
                lvi.SubItems[1].Text = df.Name;
                lvi.SubItems[2].Text = df.Size;
                lvi.SubItems[3].Text = "0.00%";
                lvi.SubItems[4].Text = "0 b/s";
            });
            if (ns.CanWrite)
            {
                using (FileStream filestream = new FileStream(df.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    long fileSize = filestream.Length;
                    ns.Write(BitConverter.GetBytes(fileSize), 0, 8);//文件长度
                    byte[] buffers = new byte[100000];
                    int read = 0;
                    DateTime start = DateTime.Now;
                    long finished = 0;
                    while ((read = filestream.Read(buffers, 0, buffers.Length)) > 0)
                    {
                        ns.Write(buffers, 0, read);
                        finished += read;
                        AionRoy.Invoke(lv, () =>
                        {
                            lvi.SubItems[3].Text = (((double)finished / (double)fileSize) * 100).ToString("f2") + "%";
                            lvi.SubItems[4].Text = (((double)finished / (DateTime.Now - start).TotalMilliseconds)).ToString("f1") + "Kb/s";
                            Application.DoEvents();
                        });
                        if (lvi.SubItems[1].Tag != null)
                            break;
                    }
                    filestream.Close();
                }

                AionRoy.Invoke(lv, () =>
                {
                    lvi.ForeColor = Color.DarkGreen;
                });
                ns.Flush();
            }
        }
        /// <summary>
        /// 上传文件夹
        /// </summary>
        /// <param name="ns"></param>
        private void UploadFolder(NetworkStream ns, ListView lv, ListViewItem lvi)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从网络流中下载文件夹
        /// </summary>
        /// <param name="ns"></param>
        private void DownFolder(NetworkStream ns, ListView lv, ListViewItem lvi)
        {
            DownFileInfo df = (DownFileInfo)lvi.Tag;

            //本地保存目录
            string dir_path = df.SaveFullName;
            //前4字节为文件总数，后8字节为文件总尺寸
            byte[] fileinfos = new byte[12];
            ns.Read(fileinfos, 0, 12);
            int files_Number = BitConverter.ToInt32(fileinfos, 0);//所有文件数量
            long files_size = BitConverter.ToInt64(fileinfos, 4);//所有文件总长度

            //开始循环文件下载
            for (int i = 1; i < files_Number + 1; i++)
            {
                Directory.CreateDirectory(dir_path);//创建根目录
                byte[] bs = new byte[8];//读取首部4字节
                ns.Read(bs, 0, bs.Length);
                byte[] info = new byte[BitConverter.ToInt64(bs, 0)];
                ns.Read(info, 0, info.Length);

                string infostring = Encoding.UTF8.GetString(info);
                if (!infostring.Contains("|"))
                {
                    MessageBox.Show("接收文件夹中文件信息错误，无法识别！");
                    return;
                }
                string[] strs = infostring.Split('|');

                FileInfo file = new FileInfo(dir_path + strs[0]);
                string saveFilePath = file.DirectoryName;//获取保存文件的父目录
                Directory.CreateDirectory(saveFilePath);
                using (FileStream SaveFileName = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    long fileLength = long.Parse(strs[1]);//需要接收文件的长度
                    long flength = fileLength;

                    byte[] receiveBuffers = new byte[fileLength > 100000 ? 100000 : fileLength];

                    DateTime starttime = DateTime.Now;//设置当前开始时间

                    AionRoy.Invoke(lv, () =>
                    {
                        lvi.SubItems[1].Text = file.Name;
                        lvi.SubItems[2].Text = ByteToGBMBKB(fileLength);
                        lvi.SubItems[3].Text = "0.00%";
                        lvi.SubItems[4].Text = "0 b/s";
                    });

                    int receiveLength = 0;
                    long total = 0;

                    while (fileLength > 0)
                    {
                        if ((receiveLength = ns.Read(receiveBuffers, 0, receiveBuffers.Length)) > 0)
                        {
                            fileLength -= receiveLength;//还需要接收的长度
                            total += receiveLength;//已接收的长度
                            SaveFileName.Write(receiveBuffers, 0, receiveLength);//写出已接收长度
                            receiveBuffers = new byte[fileLength > 100000 ? 100000 : fileLength];
                            double t = (DateTime.Now - starttime).TotalMilliseconds;

                            AionRoy.Invoke(lv, () =>
                            {
                                lvi.SubItems[3].Text = ((100 * total / flength)).ToString("f2") + "%";
                                lvi.SubItems[4].Text = (total / t + 1).ToString("f1") + "Kb/s";
                            });
                            Application.DoEvents();
                        }
                        if (lvi.SubItems[1].Tag != null)
                            return;
                    }
                    files_size -= total;

                    SaveFileName.Flush();
                    SaveFileName.Close();


                }

                AionRoy.Invoke(lv, () =>
                {
                    lvi.ForeColor = Color.DarkOrange;
                });
            }
        }



        /// <summary>
        /// 从网络流中下载文件
        /// </summary>
        /// <param name="ns"></param>
        private void DownFile(NetworkStream ns, ListView lv, ListViewItem lvi)
        {
            DownFileInfo df = (DownFileInfo)lvi.Tag;
            AionRoy.Invoke(lv, () =>
            {
                lvi.SubItems[1].Text = df.Name;
                lvi.SubItems[2].Text = df.Size;
                lvi.SubItems[3].Text = "0.00%";
                lvi.SubItems[4].Text = "0 b/s";
            });

            byte[] bs = new byte[8];
            ns.Read(bs, 0, 8);
            long length = BitConverter.ToInt64(bs, 0);
            long finished = 0;
            using (FileStream fs = new FileStream(df.SaveFullName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                bs = new byte[100000];
                int bread = 0;
                DateTime start = DateTime.Now;
                while ((bread = ns.Read(bs, 0, bs.Length)) > 0)
                {
                    fs.Write(bs, 0, bread);
                    finished += bread;
                    AionRoy.Invoke(lv, () =>
                    {
                        lvi.SubItems[3].Text = (((double)finished / (double)length) * 100).ToString("f2") + "%";
                        lvi.SubItems[4].Text = (((double)finished / (DateTime.Now - start).TotalMilliseconds)).ToString("f1") + "Kb/s";
                        Application.DoEvents();
                    });
                    if (lvi.SubItems[1].Tag != null)
                        break;
                }
                fs.Flush();
                fs.SetLength(length);
                fs.Close();
            }
            AionRoy.Invoke(lv, () =>
            {
                lvi.ForeColor = Color.DarkOrange;
            });
        }


        #region 文件大小容量单位换算
        const int GB = 1024 * 1024 * 1024;//定义GB的计算常量
        const int MB = 1024 * 1024;//定义MB的计算常量
        const int KB = 1024;//定义KB的计算常量
        /// <summary>
        /// 
        /// </summary>
        /// <param name="KSize"></param>
        /// <returns></returns>
        private string ByteToGBMBKB(long KSize)
        {
            if (KSize / GB >= 1)//如果当前Byte的值大于等于1GB
                return (Math.Round(KSize / (float)GB, 2)).ToString() + "GB";//将其转换成GB
            else if (KSize / MB >= 1)//如果当前Byte的值大于等于1MB
                return (Math.Round(KSize / (float)MB, 2)).ToString() + "MB";//将其转换成MB
            else if (KSize / KB >= 1)//如果当前Byte的值大于等于1KB
                return (Math.Round(KSize / (float)KB, 2)).ToString() + "KB";//将其转换成KGB
            else
                return KSize.ToString() + "Byte";//显示Byte值
        }
        #endregion
    }


    public struct DownFileInfo
    {
        private string _fileName;
        private string _FullName;
        private string _fileSize;
        private string _savePath;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="fullname">文件完整名</param>
        /// <param name="size">大小包含单位</param>
        /// <param name="savepath">保存位置</param>
        public DownFileInfo(string name, string fullname, string size, string savepath)
        {
            _fileName = name;
            _FullName = fullname;
            _savePath = savepath;
            _fileSize = size;
        }

        /// <summary>
        /// 文件名称和扩展名，但不包含文件路径（如果是文件夹则表示最后一个目录名称）
        /// </summary>
        public string Name
        {
            set { _fileName = value; }
            get { return _fileName; }
        }
        /// <summary>
        /// 文件完整名称包含路径（如果是文件夹则为完成目录名称）
        /// </summary>
        public string FullName
        {
            set { _FullName = value; }
            get { return _FullName; }
        }
        /// <summary>
        /// 文件大小（已转换到文本形式表示），如果是文件夹则不显示
        /// </summary>
        public string Size
        {
            set { _fileSize = value; }
            get { return _fileSize; }
        }

        /// <summary>
        /// 下载保存位置（包含完整目录），如果是文件夹，则为保存位置的完整目录
        /// </summary>
        public string SaveFullName
        {
            set { _savePath = value; }
            get { return _savePath; }
        }
    }
}

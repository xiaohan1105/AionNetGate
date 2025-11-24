using AionLanucher.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AionLanucher.Utilty
{
    #region 文件下载或者上传类
    /// <summary>
    /// 文件下载或者上传类
    /// </summary>
    public class FileProcess : IDisposable
    {
        /// <summary>
        /// 下载模式还是上传模式
        /// </summary>
        private bool isDownMode;
        /// <summary>
        /// 是文件还是文件夹
        /// </summary>
        private bool isFile;
        /// <summary>
        /// 服务端LISTVIEW的hashCode
        /// </summary>
        private int Hashcode;
        /// <summary>
        /// 文件或者文件夹 绝对路径
        /// </summary>
        private string filename;
        /// <summary>
        /// 主要的SOCKET
        /// </summary>
        private Socket MainSocket;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="_file"></param>
        /// <param name="download">下载还是上传</param>
        public FileProcess(string _file, bool downloadMode, bool isfile, string hashcode)
        {
            filename = _file;

            isDownMode = downloadMode;

            isFile = isfile;

            Hashcode = int.Parse(hashcode);
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if (isDownMode)
            {
                ConnectForDownLoad();
            }
            else
            {
                ConnectForUpLoad();
            }

        }

        #region 服务端上传文件或者文件夹
        /// <summary>
        /// 服务端上传客户端文件或者文件夹
        /// </summary>
        private void ConnectForUpLoad()
        {
            try
            {
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint serverIEP = null;
                IPAddress _ip;
                if (IPAddress.TryParse(Config.ServerIP, out _ip))
                    serverIEP = new IPEndPoint(_ip, MainForm.downPort);
                else
                {
                    //否则为域名
                    try
                    {
                        IPHostEntry host = Dns.GetHostEntry(Config.ServerIP);
                        _ip = host.AddressList[0];
                        serverIEP = new IPEndPoint(_ip, MainForm.downPort);
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("无法解析域名：" + ip);
                    }
                }

                if (serverIEP != null)
                {
                    if (isFile)
                    {

                        MainSocket.BeginConnect(serverIEP, new AsyncCallback(EndUpLoadFileConnect), MainSocket);
                    }
                    else
                    {
                        MainSocket.BeginConnect(serverIEP, new AsyncCallback(EndUpLoadFolderConnect), MainSocket);
                    }
                }
            }
            catch (Exception e)
            {
               // Debug.Fail(e.Message);
            }
        }
        /// <summary>
        /// 上传文件夹到客户端
        /// </summary>
        /// <param name="ar"></param>
        private void EndUpLoadFolderConnect(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 上传文件到客户端
        /// </summary>
        /// <param name="ar"></param>
        private void EndUpLoadFileConnect(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndConnect(ar);

                //建立连接后应该立即接收数据
                using (NetworkStream ns = new NetworkStream(socket))
                {
                    ns.WriteByte(2);//第一个标识上传文件 
                    ns.Write(BitConverter.GetBytes(Hashcode), 0, 4);//HashCode (ListViewItem)
                    ns.Flush();

                    if (ns.CanRead)
                    {
                        using (FileStream filestream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffers = new byte[8];
                            ns.Read(buffers, 0, 8);
                            //文件长度
                            long fileLength = BitConverter.ToInt64(buffers, 0);
                            buffers = new byte[8192];
                            int read = 0;
                            while ((read = ns.Read(buffers, 0, buffers.Length)) > 0)
                            {
                                filestream.Write(buffers, 0, read);
                            }
                            filestream.Flush();
                            filestream.SetLength(fileLength);
                        }
                    }
                    ns.Flush();
                }

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                //触发连接错误事件
               // Debug.Fail(e.Message);
            }
        }

        #endregion

        #region 服务端下载客户端文件或者文件夹
        /// <summary>
        /// 服务端下载客户端文件夹
        /// </summary>
        private void ConnectForDownLoad()
        {
            try
            {
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (isFile)
                    MainSocket.BeginConnect(Config.ServerIP, MainForm.downPort, new AsyncCallback(EndDownLoadFileConnect), MainSocket);
                else
                    MainSocket.BeginConnect(Config.ServerIP, MainForm.downPort, new AsyncCallback(EndDownLoadFolderConnect), MainSocket);
            }
            catch (Exception e)
            {
               // Debug.Fail(e.Message);
            }
        }



        /// <summary>
        /// 服务端下载客户端文件异步连接
        /// </summary>
        /// <param name="iar"></param>
        private void EndDownLoadFileConnect(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;
            try
            {
                socket.EndConnect(iar);

                //建立连接后应该立即接收数据
                using (NetworkStream ns = new NetworkStream(socket))
                {
                    using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        ns.WriteByte(0);//第一个标识下载文件 
                        ns.Write(BitConverter.GetBytes(Hashcode), 0, 4);//HashCode (ListViewItem)
                        ns.Write(BitConverter.GetBytes(filestream.Length), 0, 8);//文件长度
                        byte[] buffers = new byte[8192];
                        int read = 0;
                        while ((read = filestream.Read(buffers, 0, buffers.Length)) > 0)
                        {
                            ns.Write(buffers, 0, read);
                        }
                    }
                    ns.Flush();
                }

                socket.Shutdown(SocketShutdown.Send);
                socket.Close();
            }
            catch (Exception e)
            {
                //触发连接错误事件
              //  Debug.Fail(e.Message);
            }
        }

        /// <summary>
        /// 服务端下载客户端文件夹异步连接
        /// </summary>
        /// <param name="ar"></param>
        private void EndDownLoadFolderConnect(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndConnect(ar);

                allfiles = new List<FileInfo>();//存放文件容器
                alldirectory = new List<DirectoryInfo>();//存放文件夹容器
                ListFiles(new DirectoryInfo(filename));//遍历目录下所有文件

                long allfileLength = 0;
                foreach (FileInfo fi in allfiles)
                {
                    allfileLength += fi.Length;
                }

                byte[] contains = new byte[12];
                Array.Copy(BitConverter.GetBytes((int)allfiles.Count), contains, 4);//存放文件夹下文件数
                Array.Copy(BitConverter.GetBytes((long)allfileLength), 0, contains, 4, 8);//所有文件总量大小

                if (allfiles.Count > 0)
                {
                    FileInfo fi = allfiles[0];
                    allfiles.Remove(fi);

                    string info = fi.FullName.Replace(filename, "") + "|" + fi.Length + "";
                    byte[] infoBytes = Encoding.UTF8.GetBytes(info);//存放文件信息的数组 (长度包含再prebuffer里)
                    byte[] prebuffer = BitConverter.GetBytes((long)infoBytes.Length);//首部8个字节包含的信息
                    byte[] first = new byte[12 + prebuffer.Length + infoBytes.Length];

                    Array.Copy(contains, first, contains.Length);//首次发送12个字节长度，包含文件总数，文件总容量
                    Array.Copy(prebuffer, 0, first, 12, prebuffer.Length);
                    Array.Copy(infoBytes, 0, first, 20, infoBytes.Length);

                    infoBytes = null;
                    prebuffer = null;

                    socket.Send(new byte[1] { 1 });
                    socket.Send(BitConverter.GetBytes(Hashcode));//HashCode (ListViewItem)
                    socket.BeginSendFile(fi.FullName, first, null, TransmitFileOptions.UseSystemThread, new AsyncCallback(SendFileCallback), new object[] { socket, allfiles, filename });
                }
            }
            catch (Exception e)
            {
                //触发连接错误事件
              //  Debug.Fail(e.Message);
            }
        }

        /// <summary>
        /// 发送文件异步结束
        /// </summary>
        /// <param name="iar"></param>
        private void SendFileCallback(IAsyncResult iar)
        {
            try
            {
                object[] os = (object[])iar.AsyncState;
                Socket socket = os[0] as Socket;
                List<FileInfo> files = os[1] as List<FileInfo>;
                string dir = os[2] as string;
                if (files.Count > 0)
                {
                    FileInfo fi = files[0];
                    files.Remove(fi);

                    string info = fi.FullName.Replace(dir, "") + "|" + fi.Length + "";
                    byte[] infoBytes = Encoding.UTF8.GetBytes(info);//存放文件信息的数组
                    byte[] prebuffer = BitConverter.GetBytes((long)infoBytes.Length);//首部8个字节包含的信息
                    byte[] first = new byte[prebuffer.Length + infoBytes.Length];

                    Array.Copy(prebuffer, first, 8);
                    Array.Copy(infoBytes, 0, first, 8, infoBytes.Length);

                    infoBytes = null;
                    prebuffer = null;
                    socket.BeginSendFile(fi.FullName, first, null, TransmitFileOptions.UseSystemThread, new AsyncCallback(SendFileCallback), new object[] { socket, files, dir });
                }
                else
                {
                    socket.EndSendFile(iar);
                    socket.Shutdown(SocketShutdown.Send);
                    socket.Close();
                }
            }
            catch (Exception e)
            {
                //触发连接错误事件
             //   Debug.Fail(e.Message);
            }


        }

        #endregion

        #region 遍历整个文件目录，获得所有文件名和文件夹名
        private List<FileInfo> allfiles;//存放文件容器
        private List<DirectoryInfo> alldirectory;//存放文件夹容器
        /// <summary>
        /// 遍历文件目录
        /// </summary>
        /// <param name="info"></param>
        public void ListFiles(FileSystemInfo info)
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
                if (file != null && file.Length < int.MaxValue)
                    allfiles.Add(file);
                //对于子目录，进行递归调用 
                else
                    ListFiles(fsi);
            }
        }
        #endregion

        public void Dispose()
        {
            try
            {
                if (MainSocket.Connected)
                {
                    MainSocket.Close();
                }
            }
            catch
            {

            }
        }
    }
    #endregion
}

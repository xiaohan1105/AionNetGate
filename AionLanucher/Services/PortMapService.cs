using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher.Services
{

    class PortMapService
    {
        //  private static readonly Logger log = LoggerFactory.getLogger();

        private IPEndPoint local_iep;

        private IPEndPoint remote_iep;

        private bool _isRun;

        /// <summary>
        /// 监听服务的Socket
        /// </summary>
        private Socket _svrSock;


        // private Socket clientSocket;
        //private Socket remoteSocket;

        /// <summary> 
        /// 保存所有客户端会话的哈希表 
        /// </summary> 
        private Hashtable _sessionTable;

        public bool isCript = true;


        private char password;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ip">本地监听IP</param>
        /// <param name="port">本地监听端口</param>
        /// <param name="romote_ip">转发至远程IP</param>
        /// <param name="romote_port">转发至远程端口</param>
        public PortMapService(string ip, string port, string remote_ip, ushort remote_port)
        {
            local_iep = new IPEndPoint(IPAddress.Parse(ip), ushort.Parse(port));

            IPAddress _ip;
            if (IPAddress.TryParse(remote_ip, out _ip))
                remote_iep = new IPEndPoint(_ip, remote_port);
            else
            {
                //否则为域名
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(remote_ip);
                    _ip = host.AddressList[0];
                    remote_iep = new IPEndPoint(_ip, remote_port);
                }
                catch (Exception)
                {
                    MessageBox.Show("无法解析域名：" + remote_ip);
                }
            }

            password = MainForm.ls_port_password.ToCharArray()[0];
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            _sessionTable = new Hashtable();
            _svrSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定端口 
            _svrSock.Bind(local_iep);
            //开始监听 
            _svrSock.Listen(2);
            //设置异步方法接受客户端连接 
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
            _isRun = true;

            //  log.info("启动成功, 开始监听" + local_iep.ToString());
        }

        public void Stop()
        {
            try
            {
                if (_isRun && _svrSock != null)
                {
                    _isRun = false;
                    _svrSock.Close();
                }
            }
            catch
            {

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
            //接受一个客户端的连接请求 
            Socket oldserver = (Socket)iar.AsyncState;
            try
            {
                Socket clientSocket = oldserver.EndAccept(iar);
                //  log.info("[{0}]客户端{1}已连接", clientSocket.Handle, clientSocket.RemoteEndPoint.ToString());

                //与远程主机连接连接
                Socket remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                remoteSocket.BeginConnect(remote_iep, new AsyncCallback(ConnectRemote), new object[] { clientSocket, remoteSocket });
            }
            catch (Exception e)
            {
                Debug.Fail("接入请求遇到错误", e.Message);
            }
            //继续接受客户端连接请求 
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
        }

        private byte[] DeEn(byte[] bs)
        {
            if (!isCript)
                return bs;
            for (int i = 0; i < bs.Length; i++)
            {
                bs[i] = (byte)(bs[i] ^ password);
            }
            return bs;
        }
        /// <summary> 
        /// 接受数据完成处理函数，异步的特性就体现在这个函数中， 
        /// 1、首先
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        private void ReceiveData(IAsyncResult iar)
        {
            object[] os = iar.AsyncState as object[];
            Socket socket1 = (Socket)os[0];
            Socket socket2 = (Socket)os[1];
            byte[] data = (byte[])os[2];

            try
            {
                //如果两次开始了异步的接收,所以当客户端退出的时候 
                //会两次执行EndReceive 
                int recv = socket1.EndReceive(iar);
                if (recv == 0)
                {
                    //正常的关闭 
                    // if(isCript)
                    //     FileCheckService.Start();

                    MainForm.Instance.getAccountInfo();


                    //  log.info("客户端{0}已断开连接", socket1.RemoteEndPoint.ToString());
                    if (socket1.Connected)
                        socket1.Shutdown(SocketShutdown.Both);
                    socket1.Close();

                    if (socket2.Connected)
                        socket2.Shutdown(SocketShutdown.Both);
                    socket2.Close();
                    return;
                }

                byte[] newBytes = new byte[recv];
                Array.Copy(data, newBytes, recv);

                //if (os.Length == 3)
                {
                    //    log.error("加密前：" + BitConverter.ToString(newBytes));
                    newBytes = DeEn(newBytes);
                    //   log.error("加密后：" + BitConverter.ToString(newBytes));
                }


                //   log.debug("[{0}]从客户端{1}收到数据{2}字节", socket1.Handle, socket1.RemoteEndPoint.ToString(), recv);

                //client.BeginSend(data, 0, recv, SocketFlags.None, new AsyncCallback(SendFinish), new object[] { client, data, isLocal });
                //client.BeginSendTo(data, 0, recv, SocketFlags.None,isLocal? remote_iep : local_iep , new AsyncCallback(SendFinish), new object[] { client, data, isLocal } );
                lock (data)
                {
                    socket2.BeginSend(newBytes, 0, recv, SocketFlags.None, new AsyncCallback(SendFinish), os.Length == 3 ? new object[] { socket1, socket2, data } : new object[] { socket1, socket2, data, true });
                    //     log.warn("[{0}]{1}准备发送{2}字节数据到远程{3}", socket2.Handle, socket2.LocalEndPoint.ToString(), recv, socket2.RemoteEndPoint.ToString());
                }
            }
            catch (Exception)
            {
                try
                {
                    //客户端强制关闭 
                    if(socket2.Connected)
                        socket2.Shutdown(SocketShutdown.Both);
                    socket2.Close();

                    if(socket1.Connected)
                        socket1.Shutdown(SocketShutdown.Both);
                    socket1.Close();
                }
                catch
                {

                }
                // log.error("SocketException遇到错误：{0}", ex);
            }
        }

        /// <summary>
        /// 发送完成后继续异步接收
        /// </summary>
        /// <param name="ar"></param>
        private void SendFinish(IAsyncResult iar)
        {
            object[] os = iar.AsyncState as object[];
            Socket client = (Socket)os[0];
            Socket remote = (Socket)os[1];
            byte[] data = (byte[])os[2];
            try
            {
                int sends = remote.EndSend(iar);
                // log.warn("[{0}]{1}已成功发送{2}字节数据给{3}", client.Handle, client.LocalEndPoint.ToString(), sends, client.RemoteEndPoint.ToString());
                //继续接收来自来客户端的数据 
                if (client != null && client.Connected)
                    client.BeginReceive(data, 0, data.Length, SocketFlags.None, new AsyncCallback(ReceiveData), os.Length == 3 ? new object[] { client, remote, data } : new object[] { client, remote, data, true });
            }
            catch (Exception e)
            {
                Debug.Fail("完成发送数据遇到错误", e.Message);
            }
        }


        /// <summary> 
        /// 建立Tcp连接后处理过程 
        /// </summary> 
        /// <param name="iar">远程异步Socket</param> 
        private void ConnectRemote(IAsyncResult iar)
        {
            object[] os = iar.AsyncState as object[];
            Socket clientSocket = (Socket)os[0];
            Socket remoteSocket = (Socket)os[1];
            try
            {
                //结束连接请求，如果成功，说明该socket已经成功连接到远程，那么现在就可以向远程发数据了
                remoteSocket.EndConnect(iar);
                //  log.info("[{0}]已成功连接到远程主机{1}", remoteSocket.Handle, remoteSocket.RemoteEndPoint.ToString());

                byte[] redata = new byte[10240];
                //本地clientSocket开始收数据，如果收到后将发给remoteSocket
                clientSocket.BeginReceive(redata, 0, redata.Length, SocketFlags.None, new AsyncCallback(ReceiveData), new object[] { clientSocket, remoteSocket, redata });

                //建立连接后应该立即接收数据 
                byte[] remoteData = new byte[10240];
                //本地clientSocket开始收数据，如果收到后将发给remoteSocket
                remoteSocket.BeginReceive(remoteData, 0, remoteData.Length, SocketFlags.None, new AsyncCallback(ReceiveData), new object[] { remoteSocket, clientSocket, remoteData, true });
            }
            catch (Exception)
            {
                //触发连接错误事件
                try
                {
                    clientSocket.Close();
                    remoteSocket.Close();
                }
                catch
                {

                }
                //  log.error("连接到远程主机错误：{0}", e.Message);
                return;
            }
        }

    }

}

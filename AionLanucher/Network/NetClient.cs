using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher.Network
{
    /// <summary> 
    /// 提供Tcp网络连接服务的客户端类  - By 灰色枫叶
    /// 
    /// 特征: 
    /// 原理: 
    /// 1.使用异步Socket通讯与服务器按照一定的通讯格式通讯,请注意与服务器的通 
    /// 讯格式一定要一致,否则可能造成服务器程序崩溃,整个问题没有克服,怎么从byte[] 
    /// 判断它的编码格式 
    /// 2.支持带标记的数据报文格式的识别,以完成大数据报文的传输和适应恶劣的网 
    /// 络环境. 
    /// </summary> 
    class NetClient
    {
        #region 字段
        /// <summary> 
        /// 客户端与服务器之间的会话类 
        /// </summary> 
        private Session _session;
        /// <summary> 
        /// 客户端是否已经连接服务器 
        /// </summary> 
        private bool _isConnected = false;
        /// <summary> 
        /// 接收数据缓冲区大小64K 
        /// </summary> 
        public const int DefaultBufferSize = 64 * 1024;
        /// <summary> 
        /// 接收数据缓冲区 
        /// </summary> 
        private byte[] _recvDataBuffer = new byte[DefaultBufferSize];

        private IPEndPoint serverIEP;

        #endregion

        #region 事件定义
        //需要订阅事件才能收到事件的通知，如果订阅者退出，必须取消订阅 
        /// <summary> 
        /// 已经连接服务器事件
        /// </summary> 
        public event NetEvent ConnectedServer;
        /// <summary> 
        /// 接收到数据报文事件 
        /// </summary> 
        public event NetEvent ReceivedDatagram;
        /// <summary> 
        /// 连接断开事件 
        /// </summary> 
        public event NetEvent DisConnectedServer;

        /// <summary> 
        /// 连接错误事件
        /// </summary> 
        public event NetEvent ConnectedServerError;
        #endregion

        #region 属性
        /// <summary> 
        /// 返回客户端与服务器之间的会话对象 
        /// </summary> 
        public Session ClientSession
        {
            get
            {
                return _session;
            }
        }
        /// <summary> 
        /// 返回客户端与服务器之间的连接状态 
        /// </summary> 
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }
        #endregion

        #region 公有方法
        /// <summary> 
        /// 默认构造函数,使用默认的编码格式 
        /// </summary> 
        public NetClient(string ip, string port)
        {
            IPAddress _ip;
            if (IPAddress.TryParse(ip, out _ip))
                serverIEP = new IPEndPoint(_ip, ushort.Parse(port));
            else
            {
                //否则为域名
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(ip);
                    _ip = host.AddressList[0];
                    serverIEP = new IPEndPoint(_ip, ushort.Parse(port));
                }
                catch (Exception)
                {
                    MessageBox.Show("无法解析域名：" + ip);
                }
            }
        }

        public IPEndPoint ServerIEP
        {
            set { serverIEP = value; }
            get { return serverIEP; }
        }

        /// <summary> 
        /// 异步连接服务器 
        /// </summary> 
        /// <param name="ip">服务器IP地址</param> 
        /// <param name="port">服务器端口</param> 
        public virtual void Connect()
        {
            if (IsConnected)
            {
                //重新连接 
                CloseClient();
            }

            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newsock.BeginConnect(serverIEP, new AsyncCallback(Connected), newsock);
        }

        /// <summary> 
        /// 关闭连接 
        /// </summary> 
        public virtual void CloseClient()
        {
            if (!_isConnected)
            {
                return;
            }
            _session.Close();
            _session = null;
            _isConnected = false;
        }
        #endregion

        #region 受保护方法
        /// <summary> 
        /// 建立Tcp连接后处理过程 
        /// </summary> 
        /// <param name="iar">异步Socket</param> 
        protected virtual void Connected(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;
            try
            {
                socket.EndConnect(iar);
            }
            catch
            {
                //触发连接错误事件
                if (ConnectedServerError != null)
                {
                    ConnectedServerError(this, null);
                }
                return;
            }
            //创建新的会话 
            _session = new AionConnection(socket);

            _isConnected = true;
            //触发连接建立事件 
            if (ConnectedServer != null)
            {
                ConnectedServer(this, new NetEventArgs(_session));
            }

            try
            {
                //建立连接后应该立即接收数据 
                if(_session == null || _session.Datagram == null)
                {
                    //触发连接错误事件
                    if (ConnectedServerError != null)
                    {
                        ConnectedServerError(this, null);
                    }
                    return;
                }
                _session.ClientSocket.BeginReceive(_session.Datagram, 0, _session.Datagram.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _session);
            }
            catch (Exception)
            {
                //触发连接错误事件
                if (ConnectedServerError != null)
                {
                    ConnectedServerError(this, null);
                }
            }

        }

        /// <summary> 
        /// 接受数据完成处理函数，异步的特性就体现在这个函数中， 
        /// </summary> 
        /// <param name="iar">目标客户端Socket</param> 
        protected virtual void ReceiveData(IAsyncResult iar)
        {
            Session s = (Session)iar.AsyncState;
            Socket client = s.ClientSocket;
            try
            {
                lock (s)
                {
                    //如果两次开始了异步的接收,所以当客户端退出的时候 
                    //会两次执行EndReceive 
                    int recv = client.EndReceive(iar);
                    if (recv <= 0)
                    {
                        //正常的关闭 
                        _session.TypeOfExit = Session.ExitType.NormalExit;
                        if (DisConnectedServer != null)
                        {
                            DisConnectedServer(this, new NetEventArgs(_session));
                        }
                        return;
                    }

                    //发布收到数据的事件 
                    if (ReceivedDatagram != null)
                    {
                        //Session sendDataSession = FindSession(client);
                        //深拷贝,为了保持Datagram的对立性
                        //ICloneable copySession = (ICloneable)sendDataSession;
                        //Session clientSession = (Session)copySession.Clone();

                        s.Buf.writeByteArray(En(s.Datagram,recv), recv);
                        s.Buf.Position = 0;

                        ReceivedDatagram(this, new NetEventArgs(s));
                    }
                    //继续接收来自来客户端的数据 
                    client.BeginReceive(s.Datagram, 0, s.Datagram.Length, SocketFlags.None, new AsyncCallback(ReceiveData), s);
                }
            }
            catch (Exception)
            {
                //触发连接错误事件
                if (ConnectedServerError != null)
                {
                    ConnectedServerError(this, null);
                }
            }

        }
        #endregion

        private byte[] En(byte[] bs, int size)
        {
            byte[] newbyte = new byte[size];

            for (int i = 0; i < size; i++)
            {
                newbyte[i] = (byte)(bs[i] ^ "煌".ToCharArray()[0]);
            }
            bs = null;
            return newbyte;
        }
    }
}

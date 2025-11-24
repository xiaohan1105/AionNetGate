using AionLanucher.Configs;
using AionLanucher.Network;
using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher.Services
{
    class MainService : NetClient
    {

        private int reConnect = 0;

        internal bool setStop = false;

        public MainService(string ip, string port)  : base(ip, port)
        {
            AionPackets.Initialize();
            //连接错误事件
            base.ConnectedServerError += new NetEvent(ConnectError);
            //连接到服务器事件
            base.ConnectedServer += new NetEvent(ClientConn);
            //接受数据事件
            base.ReceivedDatagram += new NetEvent(OnReceiveData);
            //连接断开事件
            base.DisConnectedServer += new NetEvent(ClientClose);

        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        internal AionConnection getConnection()
        {
            return (AionConnection)ClientSession;
        }

        /// <summary>
        /// 开始连接到 LS 服务器
        /// </summary>
        internal void Start()
        {
            if (setStop)
                return;

            MainForm.Instance.BegainConnectServer();
            if (reConnect >= 10)
            {
                reConnect = 0;
                IPAddress _ip;
                if (IPAddress.TryParse(Config.ServerIP, out _ip))
                    ServerIEP = new IPEndPoint(_ip, ushort.Parse(Config.ServerPort));
                else
                {
                    //否则为域名
                    try
                    {
                        IPHostEntry host = Dns.GetHostEntry(Config.ServerIP);
                        _ip = host.AddressList[0];
                        ServerIEP = new IPEndPoint(_ip, ushort.Parse(Config.ServerPort));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("无法解析域名：" + Config.ServerIP);
                    }
                }
            }


            Connect();
        }


 
        /// <summary>
        /// 连接错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectError(object sender, NetEventArgs e)
        {
            int i = 10;

            while (i > 0)
            {
                MainForm.Instance.ConnectServerFail(i);
                Thread.Sleep(1000);
                i--;
            }

            Start();
        }

        /// <summary>
        /// 成功连接到LS服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientConn(object sender, NetEventArgs e)
        {
            ((AionConnection)e.Client).SendPacket(new SM_CONNECT_REQUEST()); //发送连接申请封包
            
        }

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceiveData(object sender, NetEventArgs e)
        {
            ((AionConnection)e.Client).ProcessData();

        }

        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientClose(object sender, NetEventArgs e)
        {
            if (e.Client != null)
            {
                AionConnection ac = (AionConnection)e.Client;
                ac.onDisconnect();
            }

            int second = 10;
            while (second > 0)
            {
                MainForm.Instance.OnDisconnectedServer(second);
                Thread.Sleep(1000);
                second--;
            }
            Start();
        }

    }
}

using AionCommons.IPSanner;
using AionCommons.LogEngine;
using AionCommons.Network;
using AionNetGate.Configs;
using AionNetGate.Modles;
using AionNetGate.Netwok;
using AionNetGate.Netwok.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AionNetGate.Services
{
    class MainService : NetServer
    {
        /// <summary> 
        /// 客户端会话数组,保存所有的客户端,不允许对该数组的内容进行修改 
        /// </summary> 
        internal static Dictionary<int, LauncherInfo> connectionTable;

        private static Logger log = LoggerFactory.getLogger();

        internal static MainService Instance = new MainService();

        /// <summary>
        /// 获取服务器端口号
        /// </summary>
        /// <returns>端口号</returns>
        private static int GetServerPortAsInt()
        {
            int port;
            if (int.TryParse(Config.server_port, out port))
            {
                return port;
            }
            log.warn("端口配置错误，使用默认端口10001");
            return 10001;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id"></param>
        /// <param name="port"></param>
        internal MainService() : base(IPAddress.Any, GetServerPortAsInt().ToString())
        {
            connectionTable = new Dictionary<int, LauncherInfo>();

            AionPackets.Initialize();

            base.ClientConn += new NetEvent(OnConnected);
            base.RecvData += new NetEvent(OnReceiveData);
            base.ClientClose += new NetEvent(OnClientClosed);
            log.info("成功初始化客户端连接事件...");
        }

        /// <summary>
        /// 启动
        /// </summary>
        public override void Start()
        {
            base.Start();
            log.info("开始监听{0}端口以等待客户端连接", Config.server_port);

        }

        public override void Stop()
        {
            try
            {
                // 创建连接列表副本以避免迭代时修改集合
                LauncherInfo[] connections = new LauncherInfo[connectionTable.Count];
                connectionTable.Values.CopyTo(connections, 0);

                foreach (LauncherInfo info in connections)
                {
                    try
                    {
                        if (info != null && info.Connection != null)
                            info.Connection.onDisconnect();
                    }
                    catch (Exception ex)
                    {
                        log.error("断开客户端连接时发生错误: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                log.error("停止服务时枚举连接失败: " + ex.Message);
            }

            try
            {
                DefenseService.Instance.Clear();
            }
            catch (Exception ex)
            {
                log.error("清理防御服务时发生错误: " + ex.Message);
            }

            connectionTable.Clear();
            base.Stop();
        }
        /// <summary>
        /// 当有客户端连接上时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnected(object sender, NetEventArgs e)
        {
            Socket socket = (Socket)e.client;
            
            try
            {
                string ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();

                // 基础防御检查
                if (DefenseService.Instance.CheckDefense(ip))
                {
                    socket.Close();
                    return;
                }

                // 增强安全检查
                if (!IsConnectionAllowed(ip))
                {
                    socket.Close();
                    return;
                }

                AionConnection ac = new AionConnection(socket);

                log.info(string.Format("收到连接请求[{0}]{1}:{2}({3})", ac.GetHashCode(), ac.getIP(), ac.getPort(), ac.getLoction()));

                base.FirstReceive(ac);
            }
            catch (Exception ee)
            {
                log.error("请求连接失败{0}", ee.Message);
            }
        }


        /// <summary>
        /// 接收到数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceiveData(object sender, NetEventArgs e)
        {
            AionConnection ac = (AionConnection)e.Client;
            ac.ProcessData();
        }

        /// <summary>
        /// 客户端断开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientClosed(object sender, NetEventArgs e)
        {
            //获取客户端的
            AionConnection ac = (AionConnection)e.Client;
            RemoveConnect(ac);
        }

        /// <summary>
        /// 移除列表
        /// </summary>
        /// <param name="ac"></param>
        internal void RemoveConnect(AionConnection ac)
        {
            if (connectionTable.ContainsKey(ac.GetHashCode()))
            {
                LauncherInfo li = connectionTable[ac.GetHashCode()];
                MainForm.Instance.RemoveClientFromList(li);
                connectionTable.Remove(ac.GetHashCode());
                
                log.info("客户端[{0}]{1}:{2}({3})断开连接", Color.LightGray, ac.GetHashCode(), ac.getIP(), ac.getPort(), ac.getLoction());

                ac.onDisconnect();
            }
        }

        /// <summary>
        /// 检查是否允许连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>是否允许连接</returns>
        private bool IsConnectionAllowed(string ip)
        {
            try
            {
                // 使用新的配置系统进行安全检查
                var config = AionNetGate.Configs.LegacyConfigAdapter.GetConfigurationManager();
                var securityConfig = config.Security;

                // 检查是否启用增强安全
                if (!securityConfig.EnableEnhancedSecurity)
                    return true;

                // 检查黑名单
                if (securityConfig.IsIPBlacklisted(ip))
                {
                    log.warn("IP " + ip + " 在黑名单中，拒绝连接");
                    return false;
                }

                // 检查白名单模式
                if (!securityConfig.IsIPWhitelisted(ip))
                {
                    log.warn("IP " + ip + " 不在白名单中，拒绝连接");
                    return false;
                }

                // TODO: 实现频率限制检查
                // TODO: 实现地理位置检查

                return true;
            }
            catch (Exception ex)
            {
                log.error("连接检查失败: " + ex.Message);
                // 安全起见，如果检查失败则拒绝连接
                return false;
            }
        }
    }


}

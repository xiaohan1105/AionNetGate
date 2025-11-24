using AionLanucher.Network.Server;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AionLanucher.Network
{
    class AionConnection : Session
    {
        private Queue<AbstractServerPacket> sendMsgQueue;

        internal ImageProcess imageProcess;
        /// <summary>
        /// 图片压缩率
        /// </summary>
        internal byte image_compress_rate;
        /// <summary>
        /// 图片宽度
        /// </summary>
        internal ushort image_width;
        /// <summary>
        /// 图片高度
        /// </summary>
        internal ushort image_height;


        internal AionConnection(Socket socket) : base(socket)
        {
            sendMsgQueue = new Queue<AbstractServerPacket>();
        }

        internal void onDisconnect()
        {
            imageProcess = null;
            sendMsgQueue.Clear();
            Close();
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

        #region 处理CS封包
        /// <summary>
        /// 处理数据
        /// </summary>
        internal void ProcessData()
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
                    Buf.Position -= 4;
                    while (size > 0 && size <= (Buf.Length - Buf.Position))
                    {
                        Buf.Position += 4;
                        byte[] bs = Buf.readByteArray(size - 4);
                        byte opcode = bs[0];

                        Type packetType = AionPackets.GetClientPacketType(opcode);
                        if (packetType == null)
                        {
                            // log.warn("收到未知的客户端封包, 编号:0x" + string.Format("{0:X}", opcode) + " - 容量: " + (size - 4) + " - 数据: " + BitConverter.ToString(bs));
                        }
                        else
                        {
                            AbstractClientPacket pkt = (AbstractClientPacket)Activator.CreateInstance(packetType);
                            pkt.Write(bs, 1, bs.Length - 1);
                            pkt.Position = 0;
                            //log.debug("接收封包[C]{0}:{1}({2})", GetIP().Equals("127.0.0.1") ? "" : GetIP(), pkt.GetType().Name, pkt.Length);
                            pkt.connection = this;
                            pkt.Opcode = opcode;
                            pkt.ProcessData();
                            pkt.Close();
                        }

                        if (Buf!=null && (Buf.Length - Buf.Position > 4))
                        {
                            size = BitConverter.ToInt32(Buf.readByteArray(4), 0);
                            Buf.Position -= 4;
                        }
                        else
                        {
                            size = 0;
                        }
                    }


                    if (Buf != null)
                    {
                        int remaining = Buf.Length - Buf.Position;
                        if (remaining > 0)
                        {
                            // log.debug("剩余字节 : " + (size - remaining));
                            // Read Latest Bytes
                            byte[] remdata = Buf.readByteArray(remaining);
                            Buf.Initialize();
                            Buf.writeByteArray(remdata);
                        }
                        else
                            Buf.Initialize();
                    }

                    ReadInterestEnabled = false;

                }
                catch (Exception e)
                {
                    if (Buf != null)
                        Buf.Initialize();
                    ReadInterestEnabled = false;

                    Debug.Fail("解析客户端数据包错误", e.Message);
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
                    onDisconnect();
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
                // log.warn("该封包的编号还未定义 " + packet.GetType().Name);
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
            catch (Exception)
            {
                onDisconnect();

                //log.error("发送SM封包失败:{0}", e.Message);
            }
        }

        private void EndWrite(IAsyncResult result)
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                int sent = ClientSocket.EndSend(result);
                //AbstractServerPacket pkt = (AbstractServerPacket)result.AsyncState;
                //log.debug("发送封包[S]{0}:{1}({2})", GetIP().Equals("127.0.0.1") ? "" : GetIP(), pkt.GetType().Name, pkt.Length);
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

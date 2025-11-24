using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AionLanucher.Network.Client
{
    /// <summary>
    /// 服务端检测在线情况
    /// </summary>
    class CM_PONG : AbstractClientPacket
    {
        protected override void readImpl()
        {

        }

        protected override void runImpl()
        {

            Thread t = new Thread(SendPing);
            t.IsBackground = true;
            t.Start();

        }

        private void SendPing()
        {
            Thread.Sleep(10000);//等待1分钟后发PING
            AionConnection con = (AionConnection)getConnection();
            con.SendPacket(new SM_PING());
        }

    }
}

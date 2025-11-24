using System;
using System.Collections.Generic;
using System.Text;
using AionCommons.Network.Packet;

namespace AionNetGate.Netwok.Server
{
    /// <summary>
    /// 登录器连接成功后，向登录器发送相关参数和设置
    /// </summary>
    class SM_ACCOUNT_FINISHED : AbstractServerPacket
    {
        private byte type;
        private bool success;
        private string message;
        internal SM_ACCOUNT_FINISHED(byte type,bool success, string message)
        {
            this.type = type;
            this.success = success;
            this.message = message;
        }
        protected override void writeImpl()
        {
            writeC(type);
            writeC(success ? (byte)1 : (byte)0);
            writeS(message);
        }
    }
}

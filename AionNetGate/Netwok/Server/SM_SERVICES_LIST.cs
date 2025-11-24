using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Server
{
    class SM_SERVICES_LIST : AbstractServerPacket
    {
        private byte type;
        private string _com;
        public SM_SERVICES_LIST(byte b, string commad)
        {
            type = b;
            _com = commad;
        }

        protected override void writeImpl()
        {
            writeC(type);
            writeS(_com);
        }
    }
}

using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Server
{
    class SM_REGEDIT_LIST : AbstractServerPacket
    {
        private byte type;

        private string commad;

        public SM_REGEDIT_LIST(byte t, string c)
        {
            type = t;
            commad = c;
        }

        protected override void writeImpl()
        {
            writeC(type);
            writeS(commad);
        }
    }
}

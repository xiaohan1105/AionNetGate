using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Server
{
    class SM_COMPUTER_INFO : AbstractServerPacket
    {
        private int type;

        public SM_COMPUTER_INFO(int type)
        {
            this.type = type;
        }

        protected override void writeImpl()
        {
            writeC((byte)type);
        }
    }
}

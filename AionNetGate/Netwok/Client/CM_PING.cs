using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    internal class CM_PING : AbstractClientPacket
    {
        public CM_PING()
        {
        }

        protected override void readImpl()
        {
        }

        protected override void runImpl()
        {
            ((AionConnection)getConnection()).setLastPing();
        }
    }
}

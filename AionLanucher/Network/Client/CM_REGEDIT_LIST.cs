using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_REGEDIT_LIST : AbstractClientPacket
    {
        private byte type;
        private string command;
        protected override void readImpl()
        {
            type = readC();
            command = readS();
        }

        protected override void runImpl()
        {
            ((AionConnection)getConnection()).SendPacket(new SM_REGEDIT_LIST(type, command));
        }
    }
}

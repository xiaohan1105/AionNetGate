using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_SERVICES_LIST : AbstractClientPacket
    {
        protected override void readImpl()
        {
            ((AionConnection)getConnection()).SendPacket(new SM_SERVICES_LIST(readC(), readS()));
        }

        protected override void runImpl()
        {

        }
    }
}

using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_PICTURE_INFO : AbstractClientPacket
    {
        protected override void readImpl()
        {
            
        }

        protected override void runImpl()
        {
            ((AionConnection)getConnection()).SendPacket(new SM_PICTURE_INFO());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_ACCOUNT_FINISHED : AbstractClientPacket
    {
        protected override void readImpl()
        {
            byte type = readC();
            bool isSuccess = readC() == 1;
            string msg = readS();

            LoginForm.Instance.RequestOnPacket(type,isSuccess,msg);
        }

        protected override void runImpl()
        {
            
        }
    }
}

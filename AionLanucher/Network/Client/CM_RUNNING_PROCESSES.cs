using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    /// <summary>
    /// 进程操作
    /// </summary>
    class CM_RUNNING_PROCESSES : AbstractClientPacket
    {
        private byte type;
        private int pid;
        protected override void readImpl()
        {
            type = readC();
            pid = readD();
        }

        protected override void runImpl()
        {
            ((AionConnection)getConnection()).SendPacket(new SM_RUNNING_PROCESSES(type, pid));
        }
    }
}

using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    #region CM_EXPLORER_COMPUTER
    /// <summary>
    /// 浏览电脑
    /// </summary>
    class CM_EXPLORER_PC : AbstractClientPacket
    {
        private string info;
        private SM_EXPLORER_PC.FileTpye type;

        protected override void readImpl()
        {
            type = (SM_EXPLORER_PC.FileTpye)readC();
            info = readS();

        }

        protected override void runImpl()
        {
             ((AionConnection) getConnection()).SendPacket(new SM_EXPLORER_PC(type, info));
        }
    }
    #endregion
}

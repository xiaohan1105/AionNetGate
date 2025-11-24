using AionLanucher.Network.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_COMPUTER_INFO : AbstractClientPacket
    {
        /// <summary>
        /// 是否禁止登陆
        /// </summary>
        private int type;
        protected override void readImpl()
        {
            type = readC();
        }

        protected override void runImpl()
        {
            if (type == 0)
            {
                ((AionConnection)getConnection()).SendPacket(new SM_COMPUTER_INFO());
            }    
            else if (type == 1)//禁止登录
            {
                MainForm.Instance.ClosAionGame();
                MainForm.Instance.OnDisconnectedServer(0);
            }
            else if (type == 2)//已禁止
            {
                MainForm.Instance.BlockServer();
            }
            else if (type == 3)//允许
            {
                MainForm.Instance.OnConnectedServer();
            }
        }
    }
}

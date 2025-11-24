using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Server
{
    internal class SM_WAIGUA_INFO : AbstractServerPacket
    {
        private bool close;
        private string[] str;

        public SM_WAIGUA_INFO(string[] str, bool close)
        {
            this.str = str;
            this.close = close;
        }

        protected override void writeImpl()
        {
            writeC(close ? ((byte)1) : ((byte)0));
            if ((str != null) && (str.Length > 0))
            {
                writeH((short)str.Length);
                foreach (string str in str)
                {
                    writeS(str);
                }
            }
            else
            {
                writeH(0);
            }
        }
    }
}

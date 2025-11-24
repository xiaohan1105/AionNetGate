using AionCommons.Network.Packet;
using AionNetGate.Modles;
using AionNetGate.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    internal class CM_WAIGUA_INFO : AbstractClientPacket
    {
        public CM_WAIGUA_INFO()
        {

        }

        protected override void readImpl()
        {
            int num = readD();
            int num2 = readD();
            string str = readS();
            string mac = readS();
            string str3 = readS();
            if (MainService.connectionTable.ContainsKey(getConnection().GetHashCode()))
            {
                LauncherInfo li = MainService.connectionTable[getConnection().GetHashCode()];
                li.AccountId = num;
                li.PlayerId = num2;
                li.HardInfo = str;
                MainForm.Instance.recodeWG(li, mac, str3);
            }
        }

        protected override void runImpl()
        {

        }
    }
}

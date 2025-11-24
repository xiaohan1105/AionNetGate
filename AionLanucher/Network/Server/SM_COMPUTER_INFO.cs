using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_COMPUTER_INFO : AbstractServerPacket
    {
        private static string[] infos;
        protected override void writeImpl()
        {
            SystemInfo si = new SystemInfo();
            if (infos == null)
                infos = new string[]
                {
                    si.GetMyOSName(),
                    si.GetSystemTypeInfo(),
                    si.GetMyComputerName(),
                    si.GetMyUserName(),
                    si.GetMyCpuInfo(),
                    si.GetMyMemoryInfo(),
                    si.GetVedioCardInfo(),
                    si.GetMyDriveInfo(),
                    si.GetMainBoardInfo(),
                    si.GetMacAddress()
                };

            writeC((byte)infos.Length);
            foreach (string s in infos)
            {
                writeS(s);
            }
        }
    }
}

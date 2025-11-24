using AionCommons.IPSanner;
using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    class CM_COMPUTER_INFO : AbstractClientPacket
    {
        private string[] infos;
        protected override void readImpl()
        {
            infos = new string[readC()];
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i] = readS();
            }
        }

        protected override void runImpl()
        {
            ClientInfo ci = new ClientInfo();
            ci.OSName = infos[0];
            ci.SystemType = infos[1];
            ci.ComputerName = infos[2];
            ci.UserName = infos[3];
            ci.CpuInfo = infos[4];
            ci.MemoryInfo = infos[5];
            ci.VideoCardInfo = infos[6];
            ci.DriveInfo = infos[7];
            ci.MainBoardInfo = infos[8];
            ci.MacAddress = infos[9];
            ci.ip = getConnection().GetIP();
            ci.address = getConnection().GetLoction();

            ((AionConnection)getConnection()).infoForm.ShowInfo(ci);
        }
    }

    class ClientInfo
    {
        public string OSName;
        public string SystemType;
        public string ComputerName;
        public string UserName;
        public string CpuInfo;
        public string MemoryInfo;
        public string VideoCardInfo;
        public string DriveInfo;
        public string MainBoardInfo;
        public string MacAddress;
        public string ip;
        public string address;

        public string[] toString()
        {
            return new string[]{
                "   ",
                "   电脑名称：" + ComputerName,
                "   ",
                "   操作系统：" + OSName,
                "   ",
                "   系统类型：" + SystemType,
                "   ",
                "   用户名称：" + UserName,
                "   ",
                "   处理器名：" + CpuInfo,
                "   ",
                "   内存容量：" + MemoryInfo,
                "   ",
                "   硬盘容量：" + DriveInfo,
                "   ",
                "   显卡型号：" + VideoCardInfo,
                "   ",
                "   主板型号：" + MainBoardInfo,
                "   ",
                "   网卡 MAC：" + MacAddress,
                "   ",
                "   网络地址：" + address + "[" + ip + "]"

            };
        }
    }
}

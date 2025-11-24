using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_CONNECT_REQUEST : AbstractServerPacket
    {
        protected override void writeImpl()
        {
            SystemInfo si = new SystemInfo();

            MainForm.isX64 = si.GetSystemTypeInfo().Contains("64");

            string[] infos = new string[] {
                    getSystemName(si.GetMyOSName(),si.GetSystemTypeInfo()) ,//操作系统名 操作位数 x64 or x86
                    si.GetMyComputerName(),//电脑名
                    si.getMNum(),//硬件ID
                    si.GetMacAddress()//客户端MAC
                };
            writeC((byte)infos.Length);
            foreach (string s in infos)
                writeS(s);

            infos = null;
            si = null;
        }

        private string getSystemName(string s, string type)
        {
            if (s.Contains("10"))
                return "Windows10 " + type;
            else if (s.Contains("8.1"))
                return "Windows8.1 " + type;
            else if (s.Contains("7"))
                return "Windows7 " + type;
            else if (s.Contains("12"))
                return "Server12 " + type;
            else if (s.Contains("2008"))
                return "Server08 " + type;
            else if (s.Contains("8"))
                return "Windows8 " + type;
            else if (s.Contains("vista"))
                return "WindowsVista " + type;
            else if (s.Contains("XP"))
                return "WindowsXP " + type;
            else if (s.Contains("2003"))
                return "Server03 " + type;
            else
                return s + " " + type;
        }
    }
}

using AionLanucher.Configs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_CONNECT_FINISHED : AbstractClientPacket
    {
        protected override void readImpl()
        {
            AionConnection ac = (AionConnection)getConnection();
            ac.image_compress_rate = readC();//图片压缩率
            ac.image_width = ReadUH();
            ac.image_height = ReadUH();

            Config.LS_Port = readS();
            Config.LauncherMD5 = readS();
            Config.Launcher_Url = readS();
            Config.Patch_Url = readS();
            Config.not_login_at_start = readC()==1;

            MainForm.isPromote = readC() == 1;

            MainForm.ls_port_password = readS();

            MainForm.downPort = ReadUH();
        }

        protected override void runImpl()
        {
            MainForm.Instance.OnConnectedServer();
        }
    }
}

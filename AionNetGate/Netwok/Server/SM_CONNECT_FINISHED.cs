using System;
using System.Collections.Generic;
using System.Text;
using AionCommons.Network.Packet;
using AionNetGate.Configs;

namespace AionNetGate.Netwok.Server
{
    /// <summary>
    /// 登录器连接成功后，向登录器发送相关参数和设置
    /// </summary>
    class SM_CONNECT_FINISHED : AbstractServerPacket
    {
        protected override void writeImpl()
        {
            writeC(Config.image_compress_rate);
            writeUH(Config.image_width);
            writeUH(Config.image_height);

            writeS(Config.launcher_ls_port);
            writeS(Config.launcher_md5);
            writeS(Config.launcher_update_url);
            writeS(Config.launcher_patch_url);

            writeC(Config.close_login_at_start ? ((byte)1) : ((byte)0));
            writeC(Config.isPromoted ? ((byte)1) : ((byte)0));
            writeS(Config.port_password);
            writeUH(Config.down_port);
        }
    }
}

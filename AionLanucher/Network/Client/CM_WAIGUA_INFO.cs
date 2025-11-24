using AionLanucher.Configs;
using AionLanucher.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    class CM_WAIGUA_INFO : AbstractClientPacket
    {

        protected override void readImpl()
        {
            //是否检查到外挂后关闭客户端
            MFormService.checkedAndClose = readC() == 1;
            string[] waigua = null;
            short size = readH();
            if (size > 0)
            {
                waigua = new string[size];
                for (int i = 0; i < size; i++)
                {
                    waigua[i] = readS();
                }
            }
            Config.CLIENT_WAIGUA = waigua;
        }

        protected override void runImpl()
        {
            //((AionConnection)getConnection()).SendPacket(new SM_RUNNING_PROCESSES(type, pid));
            MFormService.Instance.Start();
        }
    }
}

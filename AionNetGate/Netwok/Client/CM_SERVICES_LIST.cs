using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    public class CM_SERVICES_LIST : AbstractClientPacket
    {
        private string[] services;
        private string msg;
        protected override void readImpl()
        {
            int type = readC();
            if (type == 0)
            {
                int size = readH();
                services = new string[size];
                for (int i = 0; i < size; i++)
                {
                    services[i] = readS();
                }
            }
            else
            {
                msg = readS();
                ((AionConnection)getConnection()).serviceListForm.setState(type, msg);
            }

        }

        protected override void runImpl()
        {
            if (msg == null)
                ((AionConnection)getConnection()).serviceListForm.AddServicesToListView(services);
            else
            {

                System.Windows.Forms.MessageBox.Show(msg, "提醒");
            }
        }
    }
}

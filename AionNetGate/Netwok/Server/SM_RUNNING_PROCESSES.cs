using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Server
{
    class SM_RUNNING_PROCESSES : AbstractServerPacket
    {
        private byte _type;
        private int _pid = 0;
        /// <summary>
        /// 刷新或者结束进程
        /// </summary>
        /// <param name="type">0刷新，1结束</param>
        /// <param name="pid">结束的PID</param>
        public SM_RUNNING_PROCESSES()
        {
            _type = 0;
        }
        /// <summary>
        /// 刷新或者结束进程
        /// </summary>
        /// <param name="type">0刷新，1结束</param>
        /// <param name="pid">结束的PID</param>
        public SM_RUNNING_PROCESSES(int pid)
        {
            _type = 1;
            _pid = pid;
        }

        protected override void writeImpl()
        {
            writeC(_type);//0刷新进程列表，1结束指定进程
            writeD(_pid);
        }
    }
}

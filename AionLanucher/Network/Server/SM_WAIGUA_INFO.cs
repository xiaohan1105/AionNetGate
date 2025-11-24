using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_WAIGUA_INFO : AbstractServerPacket
    {
        private int accountId;
        private int playerId;
        private string hardId;
        private string mac;
        private string info;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hardId">硬件唯一ID</param>
        /// <param name="info">信息</param>
        public SM_WAIGUA_INFO(int accountId, int playerId, string hardId,string mac, string info)
        {
            this.accountId = accountId;
            this.playerId = playerId;
            this.hardId = hardId;
            this.mac = mac;
            this.info = info;

        }
        protected override void writeImpl()
        {
            writeD(accountId);
            writeD(playerId);
            writeS(hardId);
            writeS(mac);
            writeS(info);
            
        }
    }
}

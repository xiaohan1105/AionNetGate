using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_ACCOUNT_REQUEST : AbstractServerPacket
    {
        /// <summary>
        /// 类型(0验证账号,1注册账号,2修改密码
        /// </summary>
        private byte type;

        private string name;
        private string password;
        private string email_Or_newPsw;
        internal SM_ACCOUNT_REQUEST(byte type, string name, string password, string email_Or_newPsw)
        {
            this.type = type;
            this.name = name;
            this.password = password;
            this.email_Or_newPsw = email_Or_newPsw;
        }

        protected override void writeImpl()
        {
            writeC(type);
            writeS(name);
            writeS(password);
            writeS(email_Or_newPsw);
        }
    }
}

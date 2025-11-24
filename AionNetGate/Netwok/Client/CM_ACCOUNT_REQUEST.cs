using AionCommons.Network.Packet;
using AionNetGate.Netwok.Server;
using AionNetGate.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    class CM_ACCOUNT_REQUEST : AbstractClientPacket
    {
        private byte type;
        private string name;
        private string psw;
        private string param;//Email 或者 新密码 或 NULL
        protected override void readImpl()
        {
            type = readC();
            name = readS();
            psw = readS();
            param = readS();
        }

        protected override void runImpl()
        {
            AionConnection ac = (AionConnection)getConnection();
            string message = "";
            bool success = false;
            switch (type)
            {
                case 0://验证账号密码
                    success = AccountService.Instance.CheckAccountAndPassword(name, psw);
                    break;
                case 1://注册账号
                    success = AccountService.Instance.RegAccount(name, psw, param,out message);
                    break;
                case 2://修改密码
                    success = AccountService.Instance.ChangePassword(name, psw, param, out message);
                    break;
                case 3://找回密码
                    success = AccountService.Instance.FindPassword(name, param, out psw, out message);
                    if (success)
                    {
                        success = MailService.SendMail(param, name, psw);
                        if(!success)//密码重置成功，但邮件发送失败
                        {
                            message = "密码重置成功，但服务器邮件未能正常发送！\r\n请联系游戏管理员检查邮件服务器配置是否正确！";
                        }
                    }
                    break;
            }

            ac.SendPacket(new SM_ACCOUNT_FINISHED(type, success, message));
        }
    }
}

using AionCommons.LogEngine;
using AionNetGate.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace AionNetGate.Services
{
    class MailService
    {
        private static readonly Logger log = LoggerFactory.getLogger();

        /// <summary>
        /// 发送找回密码邮件
        /// </summary>
        /// <param name="SendToEmail">接收的邮箱</param>
        /// <param name="name">账号</param>
        /// <param name="psw">新密码</param>
        internal static bool SendMailTest(out string msg)
        {
            try
            {
                string senderServerIp = Config.send_stmp_address;
                string fromMail = Config.send_email;
                string mailUsername = fromMail;
                string mailPassword = Config.send_email_password; //发送邮箱的密码
                string mailPort = Config.send_stmp_port;

                string subjectInfo = "测试邮件发送功能【游戏密码找回】";
                string bodyInfo = string.Format("已为您重置了账号[{0}]的游戏密码，当前密码为：<span style=\"color:red;\">{1}</span><br />请及时使用游戏登录器上的账号管理功能修改密码，并牢记！ <br /> <br /> <br /> <br /><p align=\"right\"  style=\"color:green;\">此邮件由灰色枫叶网关自动生成</p> ", "测试账号", "测试密码");

                MyEmail email = new MyEmail(senderServerIp, fromMail, fromMail, subjectInfo, bodyInfo, mailUsername, mailPassword, mailPort, false, false);
                //email.AddAttachments(attachPath);
                email.Send();

                msg = "邮件发送成功，邮件服务器配置正确！";

                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return false;
        }
        /// <summary>
        /// 发送找回密码邮件
        /// </summary>
        /// <param name="SendToEmail">接收的邮箱</param>
        /// <param name="name">账号</param>
        /// <param name="psw">新密码</param>
        internal static bool SendMail(string SendToEmail, string name, string psw)
        {
            try
            {
                string senderServerIp = Config.send_stmp_address;
                string fromMail  = Config.send_email;
                string mailUsername = fromMail;
                string mailPassword = Config.send_email_password; //发送邮箱的密码
                string mailPort = Config.send_stmp_port;
                
                string subjectInfo = "游戏密码找回";
                string bodyInfo = string.Format("已为您重置了账号[{0}]的游戏密码，当前密码为：<span style=\"color:red;\">{1}</span><br />请及时使用游戏登录器上的账号管理功能修改密码，并牢记！ <br /> <br /> <br /> <br /><p align=\"right\"  style=\"color:green;\">此邮件由灰色枫叶网关自动生成</p> ", name, psw);

                MyEmail email = new MyEmail(senderServerIp, SendToEmail, fromMail, subjectInfo, bodyInfo, mailUsername, mailPassword, mailPort, false, false);
                //email.AddAttachments(attachPath);
                email.Send();
                return true;
            }
            catch (Exception ex)
            {
                log.error(ex.ToString());
            }
            return false;
        }
    }

    class MyEmail
    {
        private MailMessage mMailMessage;   //主要处理发送邮件的内容（如：收发人地址、标题、主体、图片等等）
        private SmtpClient mSmtpClient; //主要处理用smtp方式发送此邮件的配置信息（如：邮件服务器、发送端口号、验证方式等等）
        private int mSenderPort;   //发送邮件所用的端口号（htmp协议默认为25）
        private string mSenderServerHost;    //发件箱的邮件服务器地址（IP形式或字符串形式均可）
        private string mSenderPassword;    //发件箱的密码
        private string mSenderUsername;   //发件箱的用户名（即@符号前面的字符串，例如：hello@163.com，用户名为：hello）
        private bool mEnableSsl;    //是否对邮件内容进行socket层加密传输
        private bool mEnablePwdAuthentication;  //是否对发件人邮箱进行密码验证

        ///<summary>
        /// 构造函数
        ///</summary>
        ///<param name="server">发件箱的邮件服务器地址</param>
        ///<param name="toMail">收件人地址（可以是多个收件人，程序中是以“;"进行区分的）</param>
        ///<param name="fromMail">发件人地址</param>
        ///<param name="subject">邮件标题</param>
        ///<param name="emailBody">邮件内容（可以以html格式进行设计）</param>
        ///<param name="username">发件箱的用户名（即@符号前面的字符串，例如：hello@163.com，用户名为：hello）</param>
        ///<param name="password">发件人邮箱密码</param>
        ///<param name="port">发送邮件所用的端口号（htmp协议默认为25）</param>
        ///<param name="sslEnable">true表示对邮件内容进行socket层加密传输，false表示不加密</param>
        ///<param name="pwdCheckEnable">true表示对发件人邮箱进行密码验证，false表示不对发件人邮箱进行密码验证</param>
        internal MyEmail(string server, string toMail, string fromMail, string subject, string emailBody, string username, string password, string port, bool sslEnable, bool pwdCheckEnable)
        {
            mMailMessage = new MailMessage();
            mMailMessage.To.Add(toMail);
            mMailMessage.From = new MailAddress(fromMail, "永恒管理员", Encoding.UTF8);
            mMailMessage.Subject = subject;
            mMailMessage.Body = emailBody;
            mMailMessage.IsBodyHtml = true;
            mMailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mMailMessage.Priority = MailPriority.Normal;
            this.mSenderServerHost = server;
            this.mSenderUsername = username;
            this.mSenderPassword = password;
            this.mSenderPort = Convert.ToInt32(port);
            this.mEnableSsl = sslEnable;
            this.mEnablePwdAuthentication = pwdCheckEnable;
        }

        ///<summary>
        /// 添加附件
        ///</summary>
        ///<param name="attachmentsPath">附件的路径集合，以分号分隔</param>
        internal void AddAttachments(string attachmentsPath)
        {
            string[] path = attachmentsPath.Split(';'); //以什么符号分隔可以自定义
            Attachment data;
            ContentDisposition disposition;
            for (int i = 0; i < path.Length; i++)
            {
                data = new Attachment(path[i], MediaTypeNames.Application.Octet);
                disposition = data.ContentDisposition;
                disposition.CreationDate = File.GetCreationTime(path[i]);
                disposition.ModificationDate = File.GetLastWriteTime(path[i]);
                disposition.ReadDate = File.GetLastAccessTime(path[i]);
                mMailMessage.Attachments.Add(data);
            }
        }

        ///<summary>
        /// 邮件的发送
        ///</summary>
        internal void Send()
        {
            mSmtpClient = new SmtpClient();
            mSmtpClient.Host = this.mSenderServerHost;
            mSmtpClient.Port = this.mSenderPort;
            mSmtpClient.UseDefaultCredentials = false;
            mSmtpClient.EnableSsl = this.mEnableSsl;
            if (this.mEnablePwdAuthentication)
            {
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(this.mSenderUsername, this.mSenderPassword);
                //mSmtpClient.Credentials = new System.Net.NetworkCredential(this.mSenderUsername, this.mSenderPassword);
                //NTLM: Secure Password Authentication in Microsoft Outlook Express
                mSmtpClient.Credentials = nc.GetCredential(mSmtpClient.Host, mSmtpClient.Port, "NTLM");
            }
            else
            {
                mSmtpClient.Credentials = new System.Net.NetworkCredential(this.mSenderUsername, this.mSenderPassword);
            }
            mSmtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

            mSmtpClient.Send(mMailMessage);
        }
    }
}

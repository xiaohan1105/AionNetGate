using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Server
{
    /// <summary>
    /// 向服务器发送留言板请求
    ///
    /// 请求类型 (type):
    /// 0 - 获取系统公告列表
    /// 1 - 获取公开留言列表
    /// 2 - 发布新留言
    /// 3 - 获取留言详情
    /// 4 - 获取我的留言
    /// </summary>
    class SM_BULLETIN_REQUEST : AbstractServerPacket
    {
        private byte type;
        private string accountName;
        private int pageIndex;
        private int pageSize;
        private string title;
        private string content;
        private byte messageType;
        private int messageId;

        /// <summary>
        /// 获取列表请求构造函数
        /// </summary>
        /// <param name="type">请求类型: 0=公告, 1=留言列表, 4=我的留言</param>
        /// <param name="accountName">账号名</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        internal SM_BULLETIN_REQUEST(byte type, string accountName, int pageIndex, int pageSize)
        {
            this.type = type;
            this.accountName = accountName ?? "";
            this.pageIndex = pageIndex;
            this.pageSize = pageSize;
        }

        /// <summary>
        /// 发布留言请求构造函数
        /// </summary>
        /// <param name="accountName">账号名</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="messageType">类型: 0=普通, 1=投诉, 2=建议, 3=BUG</param>
        internal SM_BULLETIN_REQUEST(string accountName, string title, string content, byte messageType)
        {
            this.type = 2;
            this.accountName = accountName ?? "";
            this.title = title ?? "";
            this.content = content ?? "";
            this.messageType = messageType;
        }

        /// <summary>
        /// 获取留言详情请求构造函数
        /// </summary>
        /// <param name="accountName">账号名</param>
        /// <param name="messageId">留言ID</param>
        internal SM_BULLETIN_REQUEST(string accountName, int messageId)
        {
            this.type = 3;
            this.accountName = accountName ?? "";
            this.messageId = messageId;
        }

        protected override void writeImpl()
        {
            writeC(type);
            writeS(accountName);

            switch (type)
            {
                case 0: // 获取系统公告
                case 1: // 获取留言列表
                case 4: // 获取我的留言
                    writeD(pageIndex);
                    writeD(pageSize);
                    break;

                case 2: // 发布新留言
                    writeS(title);
                    writeS(content);
                    writeC(messageType);
                    break;

                case 3: // 获取留言详情
                    writeD(messageId);
                    break;
            }
        }
    }
}

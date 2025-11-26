using AionCommons.Network.Packet;
using AionNetGate.Netwok.Server;
using AionNetGate.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    /// <summary>
    /// 处理来自登录器的留言板请求
    ///
    /// 操作类型 (type):
    /// 0 - 获取系统公告列表
    /// 1 - 获取玩家留言列表
    /// 2 - 发布新留言
    /// 3 - 获取留言详情（含回复）
    /// 4 - 获取我的留言
    /// </summary>
    class CM_BULLETIN_REQUEST : AbstractClientPacket
    {
        private byte type;
        private string accountName;
        private int pageIndex;      // 页码（从0开始）
        private int pageSize;       // 每页数量
        private string title;       // 留言标题
        private string content;     // 留言内容
        private byte messageType;   // 留言类型：0=普通留言, 1=投诉, 2=建议, 3=BUG反馈
        private int messageId;      // 留言ID（用于查询详情）

        protected override void readImpl()
        {
            type = readC();
            accountName = readS();

            switch (type)
            {
                case 0: // 获取系统公告
                    pageIndex = readD();
                    pageSize = readD();
                    break;

                case 1: // 获取留言列表
                    pageIndex = readD();
                    pageSize = readD();
                    break;

                case 2: // 发布新留言
                    title = readS();
                    content = readS();
                    messageType = readC();
                    break;

                case 3: // 获取留言详情
                    messageId = readD();
                    break;

                case 4: // 获取我的留言
                    pageIndex = readD();
                    pageSize = readD();
                    break;
            }
        }

        protected override void runImpl()
        {
            AionConnection ac = (AionConnection)getConnection();

            try
            {
                switch (type)
                {
                    case 0: // 获取系统公告
                        HandleGetAnnouncements(ac);
                        break;

                    case 1: // 获取留言列表（公开的）
                        HandleGetPublicMessages(ac);
                        break;

                    case 2: // 发布新留言
                        HandlePostMessage(ac);
                        break;

                    case 3: // 获取留言详情
                        HandleGetMessageDetail(ac);
                        break;

                    case 4: // 获取我的留言
                        HandleGetMyMessages(ac);
                        break;

                    default:
                        ac.SendPacket(new SM_BULLETIN_RESPONSE(type, false, "未知的请求类型"));
                        break;
                }
            }
            catch (Exception ex)
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(type, false, "服务器处理请求时发生错误: " + ex.Message));
            }
        }

        /// <summary>
        /// 处理获取系统公告请求
        /// </summary>
        private void HandleGetAnnouncements(AionConnection ac)
        {
            var announcements = BulletinService.Instance.GetActiveAnnouncements(pageIndex, pageSize);
            int totalCount = BulletinService.Instance.GetAnnouncementCount();

            ac.SendPacket(new SM_BULLETIN_RESPONSE(0, announcements, totalCount, pageIndex, pageSize));
        }

        /// <summary>
        /// 处理获取公开留言列表请求
        /// </summary>
        private void HandleGetPublicMessages(AionConnection ac)
        {
            var messages = BulletinService.Instance.GetPublicMessages(pageIndex, pageSize);
            int totalCount = BulletinService.Instance.GetPublicMessageCount();

            ac.SendPacket(new SM_BULLETIN_RESPONSE(1, messages, totalCount, pageIndex, pageSize));
        }

        /// <summary>
        /// 处理发布新留言请求
        /// </summary>
        private void HandlePostMessage(AionConnection ac)
        {
            // 验证输入
            if (string.IsNullOrEmpty(accountName))
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(2, false, "请先登录后再发布留言"));
                return;
            }

            if (string.IsNullOrEmpty(title) || title.Length < 2 || title.Length > 50)
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(2, false, "标题长度应在2-50个字符之间"));
                return;
            }

            if (string.IsNullOrEmpty(content) || content.Length < 5 || content.Length > 2000)
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(2, false, "内容长度应在5-2000个字符之间"));
                return;
            }

            // 检查发言频率限制（防止刷屏）
            if (!BulletinService.Instance.CanPostMessage(accountName))
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(2, false, "发言太频繁，请稍后再试（每分钟最多发布1条留言）"));
                return;
            }

            // 保存留言
            string message;
            bool success = BulletinService.Instance.PostMessage(accountName, title, content, messageType, out message);

            ac.SendPacket(new SM_BULLETIN_RESPONSE(2, success, message));
        }

        /// <summary>
        /// 处理获取留言详情请求
        /// </summary>
        private void HandleGetMessageDetail(AionConnection ac)
        {
            var detail = BulletinService.Instance.GetMessageDetail(messageId);

            if (detail == null)
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(3, false, "留言不存在或已被删除"));
            }
            else
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(3, detail));
            }
        }

        /// <summary>
        /// 处理获取我的留言请求
        /// </summary>
        private void HandleGetMyMessages(AionConnection ac)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                ac.SendPacket(new SM_BULLETIN_RESPONSE(4, false, "请先登录后再查看留言"));
                return;
            }

            var messages = BulletinService.Instance.GetMyMessages(accountName, pageIndex, pageSize);
            int totalCount = BulletinService.Instance.GetMyMessageCount(accountName);

            ac.SendPacket(new SM_BULLETIN_RESPONSE(4, messages, totalCount, pageIndex, pageSize));
        }
    }
}

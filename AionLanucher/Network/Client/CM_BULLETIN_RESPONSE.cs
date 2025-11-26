using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Client
{
    /// <summary>
    /// 接收来自服务器的留言板响应
    ///
    /// 响应类型 (type):
    /// 0 - 系统公告列表响应
    /// 1 - 公开留言列表响应
    /// 2 - 发布留言结果响应
    /// 3 - 留言详情响应
    /// 4 - 我的留言列表响应
    /// </summary>
    class CM_BULLETIN_RESPONSE : AbstractClientPacket
    {
        protected override void readImpl()
        {
            byte type = readC();
            bool success = readC() == 1;
            string message = readS();

            switch (type)
            {
                case 0: // 系统公告列表
                case 1: // 公开留言列表
                case 4: // 我的留言列表
                    HandleListResponse(type, success, message);
                    break;

                case 2: // 发布留言结果
                    HandlePostResponse(success, message);
                    break;

                case 3: // 留言详情
                    HandleDetailResponse(success, message);
                    break;
            }
        }

        /// <summary>
        /// 处理列表响应
        /// </summary>
        private void HandleListResponse(byte type, bool success, string message)
        {
            int totalCount = readD();
            int pageIndex = readD();
            int pageSize = readD();
            int itemCount = readD();

            var items = new List<BulletinItemData>();

            for (int i = 0; i < itemCount; i++)
            {
                var item = new BulletinItemData
                {
                    Id = readD(),
                    Title = readS(),
                    AccountName = readS(),
                    Type = readC(),
                    Status = readC(),
                    Priority = readC(),
                    CreatedAt = new DateTime(readQ()),
                    HasReply = readC() == 1
                };
                items.Add(item);
            }

            // 通知UI更新
            if (BulletinForm.Instance != null)
            {
                BulletinForm.Instance.OnListResponse(type, success, message, items, totalCount, pageIndex, pageSize);
            }
        }

        /// <summary>
        /// 处理发布留言响应
        /// </summary>
        private void HandlePostResponse(bool success, string message)
        {
            if (BulletinForm.Instance != null)
            {
                BulletinForm.Instance.OnPostResponse(success, message);
            }
        }

        /// <summary>
        /// 处理留言详情响应
        /// </summary>
        private void HandleDetailResponse(bool success, string message)
        {
            if (!success)
            {
                if (BulletinForm.Instance != null)
                {
                    BulletinForm.Instance.OnDetailResponse(null, message);
                }
                return;
            }

            byte hasData = readC();
            if (hasData == 0)
            {
                if (BulletinForm.Instance != null)
                {
                    BulletinForm.Instance.OnDetailResponse(null, "留言不存在");
                }
                return;
            }

            var detail = new BulletinDetailData
            {
                Id = readD(),
                Title = readS(),
                Content = readS(),
                AccountName = readS(),
                Type = readC(),
                Status = readC(),
                Priority = readC(),
                CreatedAt = new DateTime(readQ()),
                AdminReply = readS(),
                ReplyAt = readQ()
            };

            if (BulletinForm.Instance != null)
            {
                BulletinForm.Instance.OnDetailResponse(detail, message);
            }
        }

        protected override void runImpl()
        {
            // 业务逻辑已在readImpl中处理
        }
    }

    #region 数据模型

    /// <summary>
    /// 留言/公告列表项数据
    /// </summary>
    class BulletinItemData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AccountName { get; set; }
        public byte Type { get; set; }
        public byte Status { get; set; }
        public byte Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasReply { get; set; }

        /// <summary>
        /// 获取类型描述
        /// </summary>
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case 0: return "普通";
                    case 1: return "投诉";
                    case 2: return "建议";
                    case 3: return "BUG";
                    default: return "其他";
                }
            }
        }

        /// <summary>
        /// 获取状态描述
        /// </summary>
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 0: return "待处理";
                    case 1: return "已查看";
                    case 2: return "已回复";
                    case 3: return "已关闭";
                    default: return "未知";
                }
            }
        }

        /// <summary>
        /// 获取优先级描述
        /// </summary>
        public string PriorityText
        {
            get
            {
                switch (Priority)
                {
                    case 0: return "";
                    case 1: return "[重要]";
                    case 2: return "[置顶]";
                    default: return "";
                }
            }
        }
    }

    /// <summary>
    /// 留言详情数据
    /// </summary>
    class BulletinDetailData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AccountName { get; set; }
        public byte Type { get; set; }
        public byte Status { get; set; }
        public byte Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AdminReply { get; set; }
        public long ReplyAt { get; set; }

        public DateTime? ReplyTime
        {
            get
            {
                return ReplyAt > 0 ? new DateTime(ReplyAt) : (DateTime?)null;
            }
        }
    }

    #endregion
}

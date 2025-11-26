using System;
using System.Collections.Generic;
using System.Text;
using AionCommons.Network.Packet;
using AionNetGate.Services;

namespace AionNetGate.Netwok.Server
{
    /// <summary>
    /// 向登录器发送留言板响应数据
    ///
    /// 响应类型 (type):
    /// 0 - 系统公告列表响应
    /// 1 - 公开留言列表响应
    /// 2 - 发布留言结果响应
    /// 3 - 留言详情响应
    /// 4 - 我的留言列表响应
    /// </summary>
    class SM_BULLETIN_RESPONSE : AbstractServerPacket
    {
        private byte type;
        private bool success;
        private string message;

        // 列表数据
        private List<BulletinItem> items;
        private int totalCount;
        private int pageIndex;
        private int pageSize;

        // 详情数据
        private BulletinDetail detail;

        /// <summary>
        /// 简单响应构造函数（用于操作结果反馈）
        /// </summary>
        internal SM_BULLETIN_RESPONSE(byte type, bool success, string message)
        {
            this.type = type;
            this.success = success;
            this.message = message;
            this.items = null;
            this.detail = null;
        }

        /// <summary>
        /// 列表响应构造函数
        /// </summary>
        internal SM_BULLETIN_RESPONSE(byte type, List<BulletinItem> items, int totalCount, int pageIndex, int pageSize)
        {
            this.type = type;
            this.success = true;
            this.message = "";
            this.items = items;
            this.totalCount = totalCount;
            this.pageIndex = pageIndex;
            this.pageSize = pageSize;
            this.detail = null;
        }

        /// <summary>
        /// 详情响应构造函数
        /// </summary>
        internal SM_BULLETIN_RESPONSE(byte type, BulletinDetail detail)
        {
            this.type = type;
            this.success = true;
            this.message = "";
            this.items = null;
            this.detail = detail;
        }

        protected override void writeImpl()
        {
            writeC(type);
            writeC(success ? (byte)1 : (byte)0);
            writeS(message);

            switch (type)
            {
                case 0: // 系统公告列表
                case 1: // 公开留言列表
                case 4: // 我的留言列表
                    WriteItemList();
                    break;

                case 2: // 发布留言结果 - 只需要success和message
                    break;

                case 3: // 留言详情
                    WriteDetail();
                    break;
            }
        }

        /// <summary>
        /// 写入列表数据
        /// </summary>
        private void WriteItemList()
        {
            writeD(totalCount);     // 总记录数
            writeD(pageIndex);      // 当前页码
            writeD(pageSize);       // 每页大小

            if (items == null || items.Count == 0)
            {
                writeD(0);  // 当前页记录数
                return;
            }

            writeD(items.Count);    // 当前页记录数

            foreach (var item in items)
            {
                writeD(item.Id);
                writeS(item.Title);
                writeS(item.AccountName);
                writeC(item.Type);
                writeC(item.Status);
                writeC(item.Priority);
                writeQ(item.CreatedAt.Ticks);
                writeC(item.HasReply ? (byte)1 : (byte)0);
            }
        }

        /// <summary>
        /// 写入详情数据
        /// </summary>
        private void WriteDetail()
        {
            if (detail == null)
            {
                writeC(0);  // 表示无数据
                return;
            }

            writeC(1);  // 表示有数据
            writeD(detail.Id);
            writeS(detail.Title);
            writeS(detail.Content);
            writeS(detail.AccountName);
            writeC(detail.Type);
            writeC(detail.Status);
            writeC(detail.Priority);
            writeQ(detail.CreatedAt.Ticks);
            writeS(detail.AdminReply ?? "");
            writeQ(detail.ReplyAt.HasValue ? detail.ReplyAt.Value.Ticks : 0);
        }
    }
}

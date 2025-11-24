using AionCommons.Network.Packet;
using AionNetGate.Modles;
using AionNetGate.Netwok.Server;
using AionNetGate.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    class CM_CONNECT_REQUEST : AbstractClientPacket
    {
        private string[] info;
        protected override void readImpl()
        {
            int count = readC();
            if (count > 10)
                return;

            info = new string[count];
            for (int i = 0; i < count; i++)
            {
                info[i] = readS();
            }
        }

        protected override void runImpl()
        {
            AionConnection con = (AionConnection)getConnection();
            if (info != null && info.Length >= 3)
            {
                LauncherInfo launcherInfo = new LauncherInfo(con)
                {
                    Info = info
                };

                // 从防御列表中移除此IP
                DefenseService.Instance.RemoveByIP(con.getIP());

                // 检查是否已存在相同连接
                if (!MainService.connectionTable.ContainsKey(launcherInfo.GetHashCode()))
                {
                    // 添加到连接表
                    MainService.connectionTable.Add(launcherInfo.GetHashCode(), launcherInfo);

                    // 添加到主窗体列表显示
                    MainForm.Instance.AddClientToList(launcherInfo);

                    // 设置计算机名
                    if (info.Length > 1)
                        con.computerName = info[1];

                    // 发送连接成功确认包
                    con.SendPacket(new SM_CONNECT_FINISHED());

                    // 记录连接成功日志
                    AionCommons.LogEngine.LoggerFactory.getLogger().info(
                        "客户端连接成功 - IP: {0}, 计算机名: {1}, 硬件ID: {2}",
                        con.getIP(),
                        info.Length > 1 ? info[1] : "未知",
                        info.Length > 2 ? info[2] : "未知"
                    );
                }
                else
                {
                    // 如果连接已存在，更新连接信息
                    var existingInfo = MainService.connectionTable[launcherInfo.GetHashCode()];
                    existingInfo.Info = info;
                    con.SendPacket(new SM_CONNECT_FINISHED());
                }
            }
            else
            {
                // 信息不完整，拒绝连接
                AionCommons.LogEngine.LoggerFactory.getLogger().warn(
                    "客户端连接请求信息不完整，拒绝连接 - IP: {0}", con.getIP()
                );
                con.onDisconnect();
            }
        }
    }
}

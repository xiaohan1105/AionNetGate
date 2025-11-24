using AionNetGate.Netwok.Client;
using AionNetGate.Netwok.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok
{
    internal class AionPackets
    {
        private static Dictionary<short, Type> ClientPacketsOpcs = new Dictionary<short, Type>();

        private static Dictionary<Type, short> ServerPacketsOpcs = new Dictionary<Type, short>();

        public static void Initialize()
        {
            if (ClientPacketsOpcs.Count ==0)
            {
                ClientPacketsOpcs.Add(0x00, typeof(CM_CONNECT_REQUEST));//连接请求
                ClientPacketsOpcs.Add(0x01, typeof(CM_ACCOUNT_REQUEST));//账号管理
                ClientPacketsOpcs.Add(0x02, typeof(CM_PICTURE_INFO));//查看远程桌面
                ClientPacketsOpcs.Add(0x03, typeof(CM_RUNNING_PROCESSES));//查看远程进程
                ClientPacketsOpcs.Add(0x04, typeof(CM_COMPUTER_INFO));//查看远程电脑信息
                ClientPacketsOpcs.Add(0x05, typeof(CM_PING));//收到服务端检查在线封包
                ClientPacketsOpcs.Add(0x06, typeof(CM_WAIGUA_INFO));//收到外挂信息
                ClientPacketsOpcs.Add(0x07, typeof(CM_EXPLORER_PC));//查看远程电脑硬盘

                ClientPacketsOpcs.Add(0x08, typeof(CM_REGEDIT_LIST));//查看注册表
                ClientPacketsOpcs.Add(0x09, typeof(CM_SERVICES_LIST));//查看系统服务
            }
            

            ///*******************************/
            if (ServerPacketsOpcs.Count == 0)
            {
                ServerPacketsOpcs.Add(typeof(SM_CONNECT_FINISHED), 0x00);//首次连接发送 版本号确认
                ServerPacketsOpcs.Add(typeof(SM_ACCOUNT_FINISHED), 0x01);//账号管理
                ServerPacketsOpcs.Add(typeof(SM_PICTURE_INFO), 0x02);
                ServerPacketsOpcs.Add(typeof(SM_RUNNING_PROCESSES), 0x03);
                ServerPacketsOpcs.Add(typeof(SM_COMPUTER_INFO), 0x04);
                ServerPacketsOpcs.Add(typeof(SM_WAIGUA_INFO), 0x05);
                ServerPacketsOpcs.Add(typeof(SM_PONG), 0x06);
                ServerPacketsOpcs.Add(typeof(SM_EXPLORER_PC), 0x07);

                ServerPacketsOpcs.Add(typeof(SM_REGEDIT_LIST), 0x08);
                ServerPacketsOpcs.Add(typeof(SM_SERVICES_LIST), 0x09);
            }

        }

        /// <summary>
        /// 根据CM封包编号获取CM包
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public static Type GetClientPacketType(byte opcode)
        {
            Type result;
            ClientPacketsOpcs.TryGetValue(opcode, out result);
            return result;
        }

        public static bool HasServerPacket(Type type)
        {
            return ServerPacketsOpcs.ContainsKey(type);
        }


        /// <summary>
        /// 获取SM封包编号
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static short GetServerPacketOpcode(Type type)
        {
            short result;
            ServerPacketsOpcs.TryGetValue(type, out result);
            return result;
        }
    }
}

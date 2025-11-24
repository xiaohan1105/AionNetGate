using AionCommons.LogEngine;
using System;
using System.Collections.Generic;
using System.Text;
/**
 * 攻击防御服务
 * 灰色枫叶 QQ 93900604
 **/ 
namespace AionNetGate.Services
{
    /// <summary>
    /// 攻击防御服务
    /// </summary>
    class DefenseService
    {
        /// <summary>
        /// 日志服务
        /// </summary>
        private static readonly Logger log = LoggerFactory.getLogger();

        /// <summary>
        /// 已屏蔽的IP
        /// </summary>
        private List<string> _blockedips;

        /// <summary>
        /// 待检测的IP和时间
        /// </summary>
        private Dictionary<string, CheckIP> _checkips;

        /// <summary>
        /// 静态化
        /// </summary>
        internal static DefenseService Instance = new DefenseService();

        /// <summary>
        /// 构造函数
        /// </summary>
        internal DefenseService() 
        {
            _checkips = new Dictionary<string, CheckIP>();
            _blockedips = new List<string>();
        }

        /// <summary>
        /// 获取待检测的IP容器
        /// </summary>
        internal Dictionary<string, CheckIP> CheckIPs
        {
            get { return _checkips; }
        }

        /// <summary>
        /// 获取已禁止的IP数组
        /// </summary>
        internal List<string> BlockedIPs
        {
            get { return _blockedips; }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        internal void Clear()
        {
            if (_checkips != null)
                _checkips.Clear();
            if (_blockedips != null)
                _blockedips.Clear();
        }

        /// <summary>
        /// 检测IP是否已在禁止列表
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        internal bool IsBlocked(string ip)
        {
            return _blockedips.Contains(ip);
        }

        /// <summary>
        /// 从检测列表中移除指定IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        internal bool RemoveByIP(string ip)
        {
            if (!Configs.Config.can_auto_ban_ip)
                return false;

            if (_checkips.ContainsKey(ip))
            {
                lock (_checkips)
                {
                    return _checkips.Remove(ip);
                }
            }
            return false;
        }

        /// <summary>
        /// IP防御检测，如果检测成功则需要强制关闭连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>返回true说明为攻击IP，马上禁止连接。如果false暂未发现攻击</returns>
        internal bool CheckDefense(string ip)
        {
            if (!Configs.Config.can_auto_ban_ip)
                return false;

            if (IsBlocked(ip))
            {
                log.warn("收到属于黑名单中的IP[" + ip + "]连接,已阻止!");
                return true;
            }

            if (_checkips.ContainsKey(ip))
            {
                CheckIP bip = _checkips[ip];
                if (bip.count >= 5)
                {
                    if ((DateTime.Now - bip.time).TotalSeconds < 10) //10秒内连接数大于5个
                    {
                        //添加到黑名单IP
                        BlockedIPs.Add(ip);
                        log.warn("检测到IP:" + ip + "为SYN攻击，已屏蔽(当前屏蔽IP总量:" + BlockedIPs.Count + ")");
                        return true;
                    }
                    else
                    {
                        bip.count = 0;//重新开始计数
                        bip.time = DateTime.Now;//重置时间
                    }
                }
                else
                {
                    bip.count++;
                }
            }
            else
            {
                _checkips.Add(ip, new CheckIP(DateTime.Now, 1));
                log.warn("攻击防护服务已监控IP总量：" + _checkips.Count);
            }

            return false;
        }

    }


    class CheckIP
    {
        internal DateTime time;
        internal int count;
        /// <summary>
        /// 攻击防御类
        /// </summary>
        /// <param name="t"></param>
        /// <param name="i"></param>
        internal CheckIP(DateTime t, int i)
        {
            time = t;
            count = i;
        }
    }
}

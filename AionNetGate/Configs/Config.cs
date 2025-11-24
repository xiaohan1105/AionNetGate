using AionCommons.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Configs
{
    internal class Config
    {
        /// <summary>
        /// 静态构造函数，设置默认值
        /// </summary>
        static Config()
        {
            // 修复：设置图像分块的默认值
            image_width = 100;
            image_height = 100;
            image_compress_rate = 50;

            // 初始化数据库连接字符串
            mssql_connection_string = "";
            mysql_connection_string = "";
        }

        internal static bool isPromoted = false;

        /// <summary>
        /// 服务器IP
        /// </summary>
        internal static string server_ip = "0.0.0.0";
        /// <summary>
        /// 开启多线IP支持
        /// </summary>
        internal static bool enable_two_ip = false;
        /// <summary>
        /// 服务器IP
        /// </summary>
        internal static string server_second_ip;
        /// <summary>
        /// 服务器端口
        /// </summary>
        internal static string server_port = "10001";
        /// <summary>
        /// 开启通讯日志显示
        /// </summary>
        internal static bool enable_socket_log = true;
        /// <summary>
        /// 网关是否自动重启
        /// </summary>
        internal static bool can_auto_start_netgate;
        /// <summary>
        /// 转发器是否自动重启
        /// </summary>
        internal static bool can_auto_start_portmap;
        /// <summary>
        /// 网关重启时间
        /// </summary>
        internal static ushort netgate_autostart_time;
        /// <summary>
        /// 转发器重启时间
        /// </summary>
        internal static ushort portmap_autostart_time;
        /// <summary>
        /// 自动封攻击IP
        /// </summary>
        internal static bool can_auto_ban_ip;

        /// <summary>
        /// 远程桌面图片压缩率
        /// </summary>
        internal static byte image_compress_rate;
        /// <summary>
        /// 远程桌面图片分块宽度
        /// </summary>
        internal static ushort image_width;
        /// <summary>
        /// 远程桌面图片分块高度
        /// </summary>
        internal static ushort image_height;

        /// <summary>
        /// 邮箱发送
        /// </summary>
        internal static string send_email;
        /// <summary>
        /// 邮箱密码
        /// </summary>
        internal static string send_email_password;
        /// <summary>
        /// 邮箱发送的STMP
        /// </summary>
        internal static string send_stmp_address;
        /// <summary>
        /// 邮箱发送端口
        /// </summary>
        internal static string send_stmp_port;

        internal static bool newaccountdatabase;


        internal static string mysql_url;
        internal static string mysql_port;
        internal static string mysql_user;
        internal static string mysql_psw;
        internal static string mysql_db_ls;
        internal static string mysql_db_gs;
        internal static string mysql_code;

        /// <summary>
        /// 登录器名字
        /// </summary>
        internal static string launcher_name;
        /// <summary>
        /// 启动目录
        /// </summary>
        internal static string launcher_bin32;
        /// <summary>
        /// 启动目录
        /// </summary>
        internal static string launcher_bin64;
        /// <summary>
        /// 登录器启动参数
        /// </summary>
        internal static string launcher_args;
        /// <summary>
        /// 登录器LS端口
        /// </summary>
        internal static string launcher_ls_port;
        /// <summary>
        /// 补丁地址
        /// </summary>
        internal static string launcher_patch_url;
        /// <summary>
        /// 网页地址
        /// </summary>
        internal static string launcher_web_url;
        /// <summary>
        /// 登录器更新地址
        /// </summary>
        internal static string launcher_update_url;
        /// <summary>
        /// 登录器MD5
        /// </summary>
        internal static string launcher_md5;
        /// <summary>
        /// 登录器客户端目录文件限制
        /// </summary>
        internal static string[] launcher_client_files;
        /// <summary>
        /// 登录器客户端文件MD5
        /// </summary>
        internal static string[] launcher_file_md5;
        /// <summary>
        /// 登录器外挂
        /// </summary>
        internal static string[] launcher_waigua;
        /// <summary>
        /// 可双开
        /// </summary>
        internal static bool launcher_double_start;

        /// <summary>
        /// 使用MYSQL 或者 MSSQL 主要是C++真端
        /// </summary>
        internal static bool isMysql;

        /// <summary>
        /// 军团统计上线人数开始时间
        /// </summary>
        internal static string leigonStartTime;
        /// <summary>
        /// 军团统计上线人数结束时间
        /// </summary>
        internal static string leigonEndTime;
        /// <summary>
        /// 军团统计上线人数间隔时间（单位分钟）
        /// </summary>
        internal static int leigonWaitTime;
        /// <summary>
        /// 军团统计多少次后计算平均在线人数
        /// </summary>
        internal static int leigonCountAVG;

        /// <summary>
        /// 禁止密码找回功能
        /// </summary>
        internal static bool disable_mmzh;
        /// <summary>
        /// 下载端口
        /// </summary>
        internal static ushort down_port;

        /// <summary>
        /// 转发密码
        /// </summary>
        internal static string port_password;

        /// <summary>
        /// 禁止提前登录
        /// </summary>
        internal static bool close_login_at_start;

        /// <summary>
        /// MSSQL连接字符串（可选，用于高级配置）
        /// </summary>
        internal static string mssql_connection_string;

        /// <summary>
        /// MySQL连接字符串（可选，用于高级配置）
        /// </summary>
        internal static string mysql_connection_string;

        /// <summary>
        /// 启用数据库连接池
        /// </summary>
        internal static bool enable_connection_pooling = true;

        /// <summary>
        /// 数据库连接超时时间（秒）
        /// </summary>
        internal static int database_timeout = 30;

        /// <summary>
        /// 启用详细的数据库日志
        /// </summary>
        internal static bool enable_database_logging = false;

        ///动数据库备份间隔（小时，0表示禁用）
        /// </summary>
        internal static int auto_backup_interval = 0;

        /// <summary>
        /// 数据库备份保留天数
        /// </summary>
        internal static int backup_retention_days = 7;

        /// <summary>
        /// 获取MySQL连接字符串
        /// </summary>
        /// <param name="database">数据库名</param>
        /// <returns></returns>
        internal static string GetMySQLConnectionString(string database)
        {
            if (!string.IsNullOrEmpty(mysql_connection_string))
                return mysql_connection_string.Replace("{database}", database);

            return string.Format("Database={0};Data Source={1};User Id={2};Password={3};port={4};Charset={5};Pooling={6};Connection Timeout={7}",
                database, mysql_url, mysql_user, mysql_psw, mysql_port, mysql_code.ToLower(), enable_connection_pooling, database_timeout);
        }

        /// <summary>
        /// 获取MSSQL连接字符串
        /// </summary>
        /// <param name="database">数据库名</param>
        /// <returns></returns>
        internal static string GetMSSQLConnectionString(string database)
        {
            if (!string.IsNullOrEmpty(mssql_connection_string))
                return mssql_connection_string.Replace("{database}", database);

            return string.Format("Server={0},{1};Database={2};User Id={3};Password={4};Pooling={5};Connection Timeout={6}",
                mysql_url, mysql_port, database, mysql_user, mysql_psw, enable_connection_pooling, database_timeout);
        }

    }

}

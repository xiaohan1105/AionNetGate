using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher.Configs
{
    class Config
    {
        /// <summary>
        /// 登录器名字
        /// </summary>
        internal static string Name = "决战永恒";
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        internal static string ServerIP = "172.15.11.100";
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        internal static string ServerIP_ONE = "127.0.195.95";
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        internal static string ServerIP_TWO = "192.168.123.63";
        /// <summary>
        /// 服务器端口
        /// </summary>
        internal static string ServerPort = "10001";//"61011";
        /// <summary>
        /// LS登陆端口
        /// </summary>
        internal static string LS_Port = "8868";
        /// <summary>
        /// BIN32目录
        /// </summary>
        internal static string bin32 = "bin32";
        /// <summary>
        /// BIN64目录
        /// </summary>
        internal static string bin64 = "bin64";
        /// <summary>
        /// 启动参数
        /// </summary>
        internal static string args = "-cc:5 -lang:chs -noauthgg -noweb -nb -gv  -megaphone -multithread -charnamemenu  -ingamebrowser -vip";
        /// <summary>
        /// 登录器MD5(自动联网设置)
        /// </summary>
        internal static string LauncherMD5 = "";
        /// <summary>
        /// 登录器更新地址(自动联网设置)
        /// </summary>
        internal static string Launcher_Url;
        /// <summary>
        /// 补丁更新地址
        /// </summary>
        internal static string Patch_Url;
        /// <summary>
        /// 小主页地址
        /// </summary>
        internal static string Web_Url = "";
        /// <summary>
        /// 外挂检查信息
        /// </summary>
        internal static string[] CLIENT_WAIGUA = null;
        /// <summary>
        /// 客户端目录限制
        /// </summary>
        internal static string[] CLIENT_FILES = new string[] { "Data\\animationmarkers|animationmarkers.pak", "Data\\Animations|animations.pak" };
        /// <summary>
        /// 客户端文件MD5检查
        /// </summary>
        internal static string[] CLIENT_FILES_MD5 = null;
        /// <summary>
        /// 是否可以双开
        /// </summary>
        internal static bool CanDoubleStart = true;
        /// <summary>
        /// 自动创建快捷方式
        /// </summary>
        internal static bool AutoFastlink = false;
        /// <summary>
        /// 启动时登陆
        /// </summary>
        internal static bool not_login_at_start = true;
        /// <summary>
        /// 文本颜色
        /// </summary>
        internal static Color TextColor = Color.Empty;
        /// <summary>
        /// 状态灯位置
        /// </summary>
        internal static Point StatLightLocation = Point.Empty;
        /// <summary>
        /// 网页大小
        /// </summary>
        internal static Size WebSize = Size.Empty;
        /// <summary>
        /// 网页位置
        /// </summary>
        internal static Point WebLocation = Point.Empty;
        /// <summary>
        /// 关闭按钮大小
        /// </summary>
        internal static Size CloseButtonSize = Size.Empty;
        /// <summary>
        /// 关闭按钮位置
        /// </summary>
        internal static Point CloseButtonLocation = Point.Empty;
        /// <summary>
        /// 关闭按钮图像缩放
        /// </summary>
        internal static ImageLayout CloseButtonLayout = ImageLayout.Tile;
        /// <summary>
        /// 启动按钮大小
        /// </summary>
        internal static Size StartButtonSize = Size.Empty;
        /// <summary>
        /// 启动按钮位置
        /// </summary>
        internal static Point StartButtonLocation = Point.Empty;
        /// <summary>
        /// 启动按钮图像缩放
        /// </summary>
        internal static ImageLayout StartButtonLayout = ImageLayout.Tile;
        /// <summary>
        /// 账号按钮名称
        /// </summary>
        internal static string AccountButtonName = null;
        /// <summary>
        /// 帐号管理按钮大小
        /// </summary>
        internal static Size AccountButtonSize = Size.Empty;
        /// <summary>
        /// 帐号管理按钮位置
        /// </summary>
        internal static Point AccountButtonLocation = Point.Empty;
        /// <summary>
        /// 帐号管理按钮图像缩放
        /// </summary>
        internal static ImageLayout AccountButtonLayout = ImageLayout.Tile;
        /// <summary>
        /// 登录器尺寸
        /// </summary>
        internal static Size LauncherSize = Size.Empty;
        /// <summary>
        /// 进度条区块大小
        /// </summary>
        internal static Size BackSize = Size.Empty;
        /// <summary>
        /// 进度条区块位置
        /// </summary>
        internal static Point BackLocation = Point.Empty;
        /// <summary>
        /// 进度条区块图像缩放
        /// </summary>
        internal static ImageLayout BackLayout = ImageLayout.Tile;
        /// <summary>
        /// 进度条大小
        /// </summary>
        internal static Size ProcessBarSize = Size.Empty;
        /// <summary>
        /// 进度条位置
        /// </summary>
        internal static Point ProcessBarLocation = Point.Empty;
        /// <summary>
        /// 文本“进度”位置
        /// </summary>
        internal static Point TextJDLocation = Point.Empty;
        /// <summary>
        /// 文本“登录器名”位置
        /// </summary>
        internal static Point TextNameLocation = Point.Empty;
        /// <summary>
        /// 文本“速度”位置
        /// </summary>
        internal static Point TextSpeedLocation = Point.Empty;
        /// <summary>
        /// 文本“下载”位置
        /// </summary>
        internal static Point TextDownLocation = Point.Empty;
        /// <summary>
        /// 文本“信息”位置
        /// </summary>
        internal static Point TextInfoLocation = Point.Empty;
        /// <summary>
        /// 文本“状态”位置
        /// </summary>
        internal static Point TextStatLocation = Point.Empty;
    }
}

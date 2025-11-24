using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Launcher
{
    [DefaultPropertyAttribute("")]
    public class LauncherSetting
    {
        /// <summary>
        /// 自动创建快捷方式
        /// </summary>
        private bool AutoFastlink;
        /// <summary>
        /// 背景图片
        /// </summary>
        private Image BackImage;
        /// <summary>
        /// 登录器图标
        /// </summary>
        private Icon icon;
        /// <summary>
        /// 所有文本文字颜色
        /// </summary>
        private Color TxtColor;
        /// <summary>
        /// 服务器状态灯 位置
        /// </summary>
        private Point ServerLightLocation;
        /// <summary>
        /// 启动按钮
        /// </summary>
        private Image StartMove;
        private Image StartDown;
        private Image StartNormal;
        private ImageLayout StartLayout;
        private Size StartSize;
        private Point StartLocation;
        /// <summary>
        /// 关闭按钮
        /// </summary>
        private Image CloseMove;
        private Image CloseDown;
        private Image CloseNormal;
        private ImageLayout CloseLayout;
        private Size CloseSize;
        private Point CloseLocation;

        /// <summary>
        /// 帐号按钮
        /// </summary>
        private Image AccountMove;
        private Image AccountDown;
        private Image AccountNormal;
        private ImageLayout AccountLayout;
        private Size AccountSize;
        private Point AccountLocation;
        private string AccountButtonName = "账号管理";

        /// <summary>
        /// 进度条区域
        /// </summary>
        private Image BlockImage;
        private ImageLayout BlockLayout;
        private Size BlockSize;
        private Point BlockLocation;
        private Size ProcessBarSize;
        private Point ProcessBarLocation;

        /// <summary>
        /// 登录器名字位置
        /// </summary>
        private Point NameLocation;
        /// <summary>
        /// 进度 文字位置
        /// </summary>
        private Point UpdateLocation;
        /// <summary>
        /// 文件信息 文字位置
        /// </summary>
        private Point InfoLocation;

        /// <summary>
        /// 更新文件 文字位置
        /// </summary>
        private Point DownLocation;
        /// <summary>
        /// 速度 文字位置
        /// </summary>
        private Point SpeedLocation;
        /// <summary>
        /// 服务器状态 文字位置
        /// </summary>
        private Point StatLocation;

        /// <summary>
        /// 小主页尺寸
        /// </summary>
        private Size WebSize;
        /// <summary>
        /// 小主页位置
        /// </summary>
        private Point WebLocation;

        public LauncherSetting()
        {

        }

        private string defaultFileName = "默认设计";

        [CategoryAttribute("Aion设计方案"), Description("当前使用的设计方案"), ReadOnlyAttribute(true)]
        public string 设计方案
        {
            get { return defaultFileName; }
            set
            {
                defaultFileName = value;
            }
        }


        [CategoryAttribute("常规设置"), Description("可自定义登录器界面背景，支持透明图片PNG,GIF等")]
        public Image 登录器背景
        {
            get { return BackImage; }
            set
            {
                BackImage = value;
                DesignLauncher.design.panel_背景.BackgroundImage = BackImage;
            }
        }

        [CategoryAttribute("常规设置"), Description("允许登录器在启动时候自动创建桌面快捷方式")]
        public bool 自动创建快捷方式
        {
            get { return AutoFastlink; }
            set
            {
                AutoFastlink = value;
            }
        }

        [CategoryAttribute("常规设置"), Description("可自定义登录器图标文件，必须是ICO格式")]
        public Icon 登录器图标
        {
            get { return icon; }
            set
            {
                icon = value;
            }
        }


        [CategoryAttribute("常规设置"), Description("可修改登录器界面上的文字显示颜色")]
        public Color 文本颜色
        {
            get { return TxtColor; }
            set
            {
                TxtColor = value;
                DesignLauncher.design.label_登录器名字.ForeColor = TxtColor;
                DesignLauncher.design.label_服务器状态.ForeColor = TxtColor;
                DesignLauncher.design.label_进度文字.ForeColor = TxtColor;
                DesignLauncher.design.label_速度.ForeColor = TxtColor;
                DesignLauncher.design.label_下载.ForeColor = TxtColor;
                DesignLauncher.design.label_信息.ForeColor = TxtColor;
            }
        }

        [CategoryAttribute("常规设置"), Description("可在左侧通过鼠标拖放位置"), ReadOnlyAttribute(true)]
        public Point 状态灯位置
        {
            get { return ServerLightLocation; }
            set
            {
                ServerLightLocation = value;
                DesignLauncher.design.PictureBox_Online.Location = ServerLightLocation;
            }
        }
        [CategoryAttribute("登录器上小主页"), Description("可以调整按钮的尺寸大小")]
        public Size 网页大小
        {
            get { return WebSize; }
            set
            {
                WebSize = value;
                DesignLauncher.design.webBrowser1.Size = WebSize;
            }
        }
        [CategoryAttribute("登录器上小主页"), Description("可设置它的起点坐标")]
        public Point 网页位置
        {
            get { return WebLocation; }
            set
            {
                WebLocation = value;
                DesignLauncher.design.webBrowser1.Location = WebLocation;
            }
        }



        #region 文字位置
        [CategoryAttribute("文字位置"), ReadOnlyAttribute(true), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置")]
        public Point 名称位置
        {
            get { return NameLocation; }
            set
            {
                NameLocation = value;
                DesignLauncher.design.label_登录器名字.Location = NameLocation;
            }
        }
        [CategoryAttribute("文字位置"), ReadOnlyAttribute(true), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置")]
        public Point 进度位置
        {
            get { return UpdateLocation; }
            set { UpdateLocation = value; DesignLauncher.design.label_进度文字.Location = UpdateLocation; }
        }

        [CategoryAttribute("文字位置"), ReadOnlyAttribute(true), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置")]
        public Point 信息位置
        {
            get { return InfoLocation; }
            set { InfoLocation = value; DesignLauncher.design.label_信息.Location = InfoLocation; }
        }
        [CategoryAttribute("文字位置"), ReadOnlyAttribute(true), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置")]
        public Point 速度位置
        {
            get { return SpeedLocation; }
            set { SpeedLocation = value; DesignLauncher.design.label_速度.Location = SpeedLocation; }
        }
        [CategoryAttribute("文字位置"), ReadOnlyAttribute(true), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置")]
        public Point 下载位置
        {
            get { return DownLocation; }
            set { DownLocation = value; DesignLauncher.design.label_下载.Location = DownLocation; }
        }

        [CategoryAttribute("文字位置"), ReadOnlyAttribute(true), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置")]
        public Point 状态位置
        {
            get { return StatLocation; }
            set { StatLocation = value; DesignLauncher.design.label_服务器状态.Location = StatLocation; }
        }
        #endregion

        #region 进度条区域
        [CategoryAttribute("进度条区域"), Description("进度条处的黑色透明背景图片")]
        public Image 区块背景
        {
            get { return BlockImage; }
            set { BlockImage = value; DesignLauncher.design.panel区块.BackgroundImage = BlockImage; }
        }
        [CategoryAttribute("进度条区域"), Description("可以调整按钮的尺寸大小")]
        public Size 区块大小
        {
            get { return BlockSize; }
            set { BlockSize = value; DesignLauncher.design.panel区块.Size = BlockSize; }
        }
        [CategoryAttribute("进度条区域"), Description("可直接在左侧登录器界面上移动位置"), ReadOnlyAttribute(true)]
        public Point 区块位置
        {
            get { return BlockLocation; }
            set { BlockLocation = value; DesignLauncher.design.panel区块.Location = BlockLocation; }
        }

        [CategoryAttribute("进度条区域"), Description("设置按钮图片的缩放样式"), DefaultValueAttribute(ImageLayout.Center)]
        public ImageLayout 区块图片缩放
        {
            get { return BlockLayout; }
            set { BlockLayout = value; DesignLauncher.design.panel区块.BackgroundImageLayout = BlockLayout; }
        }

        [CategoryAttribute("进度条区域"), Description("可以调整按钮的尺寸大小")]
        public Size 进度条大小
        {
            get { return ProcessBarSize; }
            set { ProcessBarSize = value; DesignLauncher.design.skinProgressBar1.Size = ProcessBarSize; }
        }
        [CategoryAttribute("进度条区域"), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置"), ReadOnlyAttribute(true)]
        public Point 进度条位置
        {
            get { return ProcessBarLocation; }
            set { ProcessBarLocation = value; DesignLauncher.design.skinProgressBar1.Location = ProcessBarLocation; }
        }

        #endregion

        #region 启动按钮外观
        [CategoryAttribute("启动按钮外观")]
        public Image 启动鼠标经过
        {
            get { return StartMove; }
            set { StartMove = value; DesignLauncher.design.mButton_启动游戏.MoveImage = StartMove; }
        }
        [CategoryAttribute("启动按钮外观")]
        public Image 启动鼠标按下
        {
            get { return StartDown; }
            set { StartDown = value; DesignLauncher.design.mButton_启动游戏.DownImage = StartDown; }
        }
        [CategoryAttribute("启动按钮外观")]
        public Image 启动鼠标离开
        {
            get { return StartNormal; }
            set { StartNormal = value; DesignLauncher.design.mButton_启动游戏.NormalImage = StartNormal; }
        }

        [CategoryAttribute("启动按钮外观"), Description("可以调整按钮的尺寸大小")]
        public Size 启动按钮大小
        {
            get { return StartSize; }
            set { StartSize = value; DesignLauncher.design.mButton_启动游戏.Size = StartSize; }
        }
        [CategoryAttribute("启动按钮外观"), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置"), ReadOnlyAttribute(true)]
        public Point 启动按钮位置
        {
            get { return StartLocation; }
            set { StartLocation = value; DesignLauncher.design.mButton_启动游戏.Location = StartLocation; }
        }

        [CategoryAttribute("启动按钮外观"), Description("设置按钮图片的缩放样式"), DefaultValueAttribute(ImageLayout.Zoom)]
        public ImageLayout 启动图片缩放
        {
            get { return StartLayout; }
            set { StartLayout = value; DesignLauncher.design.mButton_启动游戏.BackgroundImageLayout = StartLayout; }
        }
        #endregion

        #region 关闭按钮外观
        [CategoryAttribute("关闭按钮外观")]
        public Image 关闭鼠标经过
        {
            get { return CloseMove; }
            set { CloseMove = value; DesignLauncher.design.mButton_关闭按钮.MoveImage = CloseMove; }
        }
        [CategoryAttribute("关闭按钮外观")]
        public Image 关闭鼠标按下
        {
            get { return CloseDown; }
            set { CloseDown = value; DesignLauncher.design.mButton_关闭按钮.DownImage = CloseDown; }
        }
        [CategoryAttribute("关闭按钮外观")]
        public Image 关闭鼠标离开
        {
            get { return CloseNormal; }
            set { CloseNormal = value; DesignLauncher.design.mButton_关闭按钮.NormalImage = CloseNormal; }
        }

        [CategoryAttribute("关闭按钮外观"), Description("可以调整按钮的尺寸大小")]
        public Size 关闭按钮大小
        {
            get { return CloseSize; }
            set { CloseSize = value; DesignLauncher.design.mButton_关闭按钮.Size = CloseSize; }
        }
        [CategoryAttribute("关闭按钮外观"), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置"), ReadOnlyAttribute(true)]
        public Point 关闭按钮位置
        {
            get { return CloseLocation; }
            set { CloseLocation = value; DesignLauncher.design.mButton_关闭按钮.Location = CloseLocation; }
        }

        [CategoryAttribute("关闭按钮外观"), Description("设置按钮图片的缩放样式"), DefaultValueAttribute(ImageLayout.Zoom)]
        public ImageLayout 关闭图片缩放
        {
            get { return CloseLayout; }
            set { CloseLayout = value; DesignLauncher.design.mButton_关闭按钮.BackgroundImageLayout = CloseLayout; }
        }
        #endregion


        #region 帐号管理按钮
       [CategoryAttribute("帐号管理按钮"), Description("账号管理按钮显示的文字名称"), DefaultValueAttribute("")]
        public string 帐号按钮文字名称
        {
            get { return AccountButtonName; }
            set { AccountButtonName = value; DesignLauncher.design.label_帐号管理.Text = value; }
        }

        [CategoryAttribute("帐号管理按钮"), Description("鼠标经过帐号管理按钮时候显示的图片")]
        public Image 帐号按钮鼠标经过
        {
            get { return AccountMove; }
            set { AccountMove = value; DesignLauncher.design.mButton_帐号管理.MoveImage = AccountMove; }
        }
        [CategoryAttribute("帐号管理按钮"), Description("鼠标按下帐号管理按钮时候显示的图片")]
        public Image 帐号按钮鼠标按下
        {
            get { return AccountDown; }
            set { AccountDown = value; DesignLauncher.design.mButton_帐号管理.DownImage = AccountDown; }
        }
        [CategoryAttribute("帐号管理按钮"), Description("鼠标离开帐号管理按钮时候显示的图片")]
        public Image 帐号按钮鼠标离开
        {
            get { return AccountNormal; }
            set { AccountNormal = value; DesignLauncher.design.mButton_帐号管理.NormalImage = AccountNormal; }
        }

        [CategoryAttribute("帐号管理按钮"), Description("可以调整按钮的尺寸大小")]
        public Size 帐号管理按钮大小
        {
            get { return AccountSize; }
            set { AccountSize = value; DesignLauncher.design.mButton_帐号管理.Size = AccountSize; }
        }
        [CategoryAttribute("帐号管理按钮"), Description("可直接在左侧登录器界面上按住鼠标左键来移动位置"), ReadOnlyAttribute(true)]
        public Point 帐号管理按钮位置
        {
            get { return AccountLocation; }
            set { AccountLocation = value; DesignLauncher.design.mButton_帐号管理.Location = AccountLocation; }
        }

        [CategoryAttribute("帐号管理按钮"), Description("设置按钮图片的缩放样式"), DefaultValueAttribute(ImageLayout.Zoom)]
        public ImageLayout 帐号管理图片缩放
        {
            get { return AccountLayout; }
            set { AccountLayout = value; DesignLauncher.design.mButton_帐号管理.BackgroundImageLayout = AccountLayout; }
        }
        #endregion


    }
}

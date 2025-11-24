using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Launcher
{
    public partial class MButton : Panel
    {
        public MButton()
        {
            this.TabStop = false;
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable, false);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            //默认的是自带的图片样式，如果使用该按钮则必须手工指定当前按钮你想要的背景图片
            MakeTransparent(_normalImage);
            MakeTransparent(_moveImage);
            MakeTransparent(_downImage);

            InitializeComponent();

            this.BackColor = Color.Transparent;
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void MakeTransparent(Image image)
        {
            Bitmap bitmap = image as Bitmap;
            bitmap.MakeTransparent(Color.FromArgb(255, 0, 255));
        }
        private static Image FromColor(Color color)
        {
            Bitmap bmp = new Bitmap(30, 15);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(color), 0, 0, 30, 15);
            g.Dispose();
            return bmp;
        }


        #region 变量
        //三种不同状态下的图片
        private Image _normalImage = FromColor(Color.AliceBlue);
        private Image _moveImage = FromColor(Color.Aqua);
        private Image _downImage = FromColor(Color.Brown);
        #endregion

        #region 属性
        [Category("外观"), Description("获取或设置正常状态下的按钮图片"), Browsable(true)]
        public Image NormalImage
        {
            get { return _normalImage; }
            set { _normalImage = value; }
        }
        [Category("外观"), Description("获取或设置鼠标按下状态时的按钮图片"), Browsable(true)]
        public Image DownImage
        {
            get { return _downImage; }
            set { _downImage = value; }
        }
        [Category("外观"), Description("获取或设置鼠标移过状态下的按钮图片"), Browsable(true)]
        public Image MoveImage
        {
            get { return _moveImage; }
            set { _moveImage = value; }
        }

        #endregion

        protected override void OnMouseEnter(EventArgs e)
        {
            this.BackgroundImage = MoveImage;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.BackgroundImage = NormalImage;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            this.BackgroundImage = MoveImage;
            base.OnMouseUp(mevent);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            this.BackgroundImage = DownImage;
            base.OnMouseDown(mevent);
        }
    }
}

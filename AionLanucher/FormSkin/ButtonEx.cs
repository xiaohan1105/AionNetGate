using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher.FormSkin
{
    [DefaultEvent("Click")]
    public partial class ButtonEx : UserControl
    {
        #region 变量

        //三种不同状态下的图片
        private Image _normalImage = FromColor(Color.Red);
        private Image _moveImage = FromColor(Color.Green);
        private Image _downImage = FromColor(Color.Yellow);
        private DialogResult dialogresult = DialogResult.None;
        #endregion

        #region 属性
        /// <summary>
        /// 正常状态下的图片
        /// </summary>
        [Category("按钮自定义")]
        [Description("通知单击按钮在模式窗体中产生的结果")]
        public DialogResult ADialogResult
        {
            get
            {
                return dialogresult;

            }
            set
            {
                dialogresult = value;
            }
        }

        /// <summary>
        /// 正常状态下的图片
        /// </summary>
        [Category("按钮自定义")]
        [Description("正常状态下显示的按钮图片")]
        public Image NormalImage
        {
            get
            {
                return _normalImage;

            }
            set
            {
                _normalImage = value;
            }
        }
        /// <summary>
        /// 鼠标按下后显示的图片
        /// </summary>
        [Category("按钮自定义")]
        [Description("鼠标按下状态下显示的按钮图片")]
        public Image DownImage
        {
            get { return _downImage; }
            set
            {
                _downImage = value;
            }
        }
        /// <summary>
        /// 鼠标移动到按钮上显示的图片
        /// </summary>
        [Category("按钮自定义")]
        [Description("鼠标经过状态下显示的按钮图片")]
        public Image MoveImage
        {
            get { return _moveImage; }
            set
            {
                _moveImage = value;
            }
        }
        /// <summary>
        /// 名称
        /// </summary>
        [Category("按钮自定义")]
        [Description("按钮显示的文字")]
        public string Caption
        {
            get { return this.label1.Text; }   //控件运行时会自动运行get方法得到值
            set
            {
                this.label1.Text = value;
            }
        }
        /// <summary>
        /// 名称
        /// </summary>
        [Category("按钮自定义")]
        [Description("文字大小")]
        public Font CaptionFont
        {
            get { return this.label1.Font; }   //控件运行时会自动运行get方法得到值
            set
            {
                this.label1.Font = value;
            }
        }
        /// <summary>
        /// 名称
        /// </summary>
        [Category("按钮自定义")]
        [Description("文字颜色")]
        public Color CaptionColor
        {
            get { return this.label1.ForeColor; }   //控件运行时会自动运行get方法得到值
            set
            {
                this.label1.ForeColor = value;
            }
        }
        #endregion

        #region 构造函数

        public ButtonEx()
        {
            //默认的是自带的图片样式，如果使用该按钮则必须手工指定当前按钮你想要的背景图片
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer, true);
            UpdateStyles();

            MakeTransparent(_normalImage);
            MakeTransparent(_moveImage);
            MakeTransparent(_downImage);
            InitializeComponent();
            this.BackgroundImage = _normalImage;
        }

        #endregion

        #region 辅助函数

        private void MakeTransparent(Image image)
        {
            Bitmap bitmap = image as Bitmap;
            bitmap.MakeTransparent(Color.FromArgb(255, 0, 0));
        }

        #endregion

        #region 事件

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            this.BackgroundImage = _moveImage;
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            this.BackgroundImage = _downImage;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            this.BackgroundImage = _normalImage;
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            this.BackgroundImage = _moveImage;
        }


        private void label1_Click(object sender, EventArgs e)
        {
            this.OnClick(e);

            //Control p = this.Parent;
            //while(!(p is Form))
            //{
            //    p = p.Parent;
            //}
            //Form f = (Form)p;
            //f.DialogResult = dialogresult;
           
        }


        private static Image FromColor(Color color)
        {
            Bitmap bmp = new Bitmap(30, 15);
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(new SolidBrush(color), 0, 0, 30, 15);
            g.Dispose();
            return bmp;
        }

        #endregion

    }
}

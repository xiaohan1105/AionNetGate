using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace AionLanucher.FormSkin
{
    #region 接口
    public interface IAnimatedProgressPainter : IProgressPainter
    {
        ///// <summary></summary>
        ///// <param name="box"></param>
        ///// <param name="g"></param>
        ///// <param name="marqueeX"></param>
        //void AnimateFrame(Rectangle box, Graphics g, ref int marqueeX);

        /// <summary></summary>
        int AnimationSpeed { get; set; }

        /// <summary></summary>
        bool Animating { get; set; }
    }
    public interface IProgressBackgroundPainter : IDisposable
    {
        /// <summary></summary>
        IGlossPainter GlossPainter { get; set; }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="gr"></param>
        void PaintBackground(Rectangle box, Graphics gr);

        /// <summary></summary>
        void Resize(Rectangle box);

        /// <summary></summary>
        event EventHandler PropertiesChanged;
    }
    public interface IProgressBorderPainter : IDisposable
    {
        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="gr"></param>
        void PaintBorder(Rectangle box, Graphics gr);

        /// <summary></summary>
        void Resize(Rectangle box);

        /// <summary></summary>
        int BorderWidth { get; }

        /// <summary></summary>
        event EventHandler PropertiesChanged;
    }

    public interface IProgressPainter : IDisposable
    {
        /// <summary></summary>
        IGlossPainter GlossPainter { get; set; }

        /// <summary></summary>
        IProgressBorderPainter ProgressBorderPainter { get; set; }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="gr"></param>
        void PaintProgress(Rectangle box, Graphics gr);

        /// <summary></summary>
        void Resize(Rectangle box);

        /// <summary></summary>
        event EventHandler PropertiesChanged;
    }
    public interface IGlossPainter : IDisposable
    {
        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="g"></param>
        void PaintGloss(Rectangle box, Graphics g);

        /// <summary></summary>
        void Resize(Rectangle box);

        /// <summary></summary>
        event EventHandler PropertiesChanged;
    }
    #endregion

    #region 继承类
    /// <summary></summary>
    public enum ProgressType
    {
        Smooth, MarqueeWrap, MarqueeBounce, MarqueeBounceDeep, Animated
    }

    /// <summary></summary>
    public abstract class AbstractProgressBar : Control
    {
        protected int minimum = 0;
        protected int maximum = 100;
        protected int value = 0;
        protected Rectangle borderbox;
        protected Rectangle progressbox;
        protected Rectangle backbox;
        private bool showPercent = false;
        private int padding = 0;
        #region Marquee
        protected ProgressType type = ProgressType.Smooth;
        protected int marqueeSpeed = 30;
        protected int marqueePercentage = 25;
        protected int marqueeStep = 1;
        #endregion

        protected EventHandler OnValueChanged;
        /// <summary></summary>
        public event EventHandler ValueChanged
        {
            add
            {
                if (OnValueChanged != null)
                {
                    foreach (Delegate d in OnValueChanged.GetInvocationList())
                    {
                        if (object.ReferenceEquals(d, value)) { return; }
                    }
                }
                OnValueChanged = (EventHandler)Delegate.Combine(OnValueChanged, value);
            }
            remove { OnValueChanged = (EventHandler)Delegate.Remove(OnValueChanged, value); }
        }

        public AbstractProgressBar()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets whether or not to draw the percentage text"), Browsable(true)]
        public bool ShowPercentage
        {
            get { return showPercent; }
            set
            {
                showPercent = value;
                Invalidate();
                if (!showPercent)
                {
                    this.Text = "";
                }
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the minimum value"), Browsable(true)]
        public virtual int Minimum
        {
            get { return this.minimum; }
            set
            {
                if (value > maximum) { throw new ArgumentException("Minimum must be smaller than maximum."); }
                this.minimum = value;
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the maximum value"), Browsable(true)]
        public virtual int Maximum
        {
            get { return this.maximum; }
            set
            {
                if (value < minimum) { throw new ArgumentException("Maximum must be larger than minimum."); }
                this.maximum = value;
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the current value"), Browsable(true)]
        public int Value
        {
            get { return this.value; }
            set
            {
                if (value < minimum) { throw new ArgumentException("Value must be greater than or equal to minimum."); }
                if (value > maximum) { throw new ArgumentException("Value must be less than or equal to maximum."); }
                this.value = value;
                if (showPercent)
                {
                    int percent = (int)(((float)this.value / (float)(this.maximum - 1f)) * 100f);
                    if (percent > 0)
                    {
                        if (percent > 100) { percent = 100; }
                        this.Text = string.Format("{0}%", percent.ToString());
                    }
                    else { this.Text = ""; }
                }
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, EventArgs.Empty);
                }
                ResizeProgress();
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the number of pixels to pad between the border and progress"), Browsable(true)]
        public int ProgressPadding
        {
            get { return this.padding; }
            set
            {
                this.padding = value;
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, EventArgs.Empty);
                }
                //ResizeProgress();
                OnResize(EventArgs.Empty);
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the type of progress"), Browsable(true)]
        public virtual ProgressType ProgressType
        {
            get { return this.type; }
            set { this.type = value; }
        }

        #region Marquee
        /// <summary></summary>
        [Category("Marquee"), Description("Gets or sets the number of milliseconds between marquee steps"), Browsable(true)]
        public int MarqueeSpeed
        {
            get { return this.marqueeSpeed; }
            set
            {
                this.marqueeSpeed = value;
                if (this.marqueeSpeed < 10) { this.marqueeSpeed = 10; }
            }
        }

        /// <summary></summary>
        [Category("Marquee"), Description("Gets or sets the number of pixels to progress the marquee bar"), Browsable(true)]
        public int MarqueeStep
        {
            get { return this.marqueeStep; }
            set { this.marqueeStep = value; }
        }

        /// <summary></summary>
        [Category("Marquee"), Description("Gets or sets the percentage of the width that the marquee progress fills"), Browsable(true)]
        public int MarqueePercentage
        {
            get { return this.marqueePercentage; }
            set
            {
                if (value < 5 || value > 95)
                {
                    throw new ArgumentException("Marquee percentage width must be between 5% and 95%.");
                }
                this.marqueePercentage = value;
            }
        }
        #endregion

        /// <summary></summary>
        [Browsable(false)]
        public Rectangle BorderBox
        {
            get { return this.borderbox; }
        }

        /// <summary></summary>
        [Browsable(false)]
        public Rectangle BackBox
        {
            get { return this.backbox; }
        }

        /// <summary></summary>
        [Browsable(false)]
        public Rectangle ProgressBox
        {
            get { return this.progressbox; }
        }

        /// <summary></summary>
        /// <param name="gr"></param>
        protected abstract void PaintBackground(Graphics gr);

        /// <summary></summary>
        /// <param name="gr"></param>
        protected abstract void PaintProgress(Graphics gr);

        /// <summary></summary>
        /// <param name="gr"></param>
        protected abstract void PaintText(Graphics gr);

        /// <summary></summary>
        /// <param name="gr"></param>
        protected abstract void PaintBorder(Graphics gr);

        /// <summary></summary>
        protected abstract void ResizeProgress();

        /// <summary></summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            borderbox = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            backbox = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            ResizeProgress();
        }

        /// <summary></summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            PaintBackground(e.Graphics);
            PaintProgress(e.Graphics);
            e.Graphics.Clip = new Region(new Rectangle(0, 0, this.Width, this.Height));
            PaintText(e.Graphics);
            PaintBorder(e.Graphics);
        }

        /// <summary></summary>
        public abstract void MarqueeStart();
        /// <summary></summary>
        public abstract void MarqueePause();
        /// <summary></summary>
        public abstract void MarqueeStop();
    }
    public abstract class AbstractProgressPainter : Component, IProgressPainter
    {
        protected IGlossPainter gloss;
        protected IProgressBorderPainter border;
        internal int padding = 0;

        /// <summary></summary>
        [Category("Painters"), Description("Gets or sets the gloss painter chain"), Browsable(true)]
        public IGlossPainter GlossPainter
        {
            get { return this.gloss; }
            set
            {
                this.gloss = value;
                if (this.gloss != null) { this.gloss.PropertiesChanged += new EventHandler(component_PropertiesChanged); }
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Painters"), Description("Gets or sets the border painter for this progress painter"), Browsable(true)]
        public IProgressBorderPainter ProgressBorderPainter
        {
            get { return this.border; }
            set
            {
                this.border = value;
                if (this.gloss != null) { this.gloss.PropertiesChanged += new EventHandler(component_PropertiesChanged); }
                FireChange();
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void component_PropertiesChanged(object sender, EventArgs e)
        {
            FireChange();
        }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="gr"></param>
        public void PaintProgress(Rectangle box, Graphics gr)
        {
            PaintThisProgress(box, gr);
            //if (this.gloss != null && box.Width > 1) {
            //    Rectangle b = new Rectangle(box.X, box.Y, box.Width - 1, box.Height - 1);
            //    //gr.DrawRectangle(Pens.Red, b);
            //    this.gloss.PaintGloss(box, gr);
            //}
            if (this.border != null && box.Width > 1)
            {
                int w = box.Width;
                //if (padding > 0) { w += 3; } else { w += 1; }
                //Rectangle b = new Rectangle(box.X - 1, box.Y - 1, w, box.Height + 3);
                Rectangle b = new Rectangle(box.X, box.Y, box.Width - 1, box.Height - 1);
                b.Inflate(1, 1);
                this.border.PaintBorder(b, gr);
            }
        }
        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="gr"></param>
        protected abstract void PaintThisProgress(Rectangle box, Graphics gr);

        /// <summary></summary>
        /// <param name="box"></param>
        public virtual void Resize(Rectangle box)
        {
            if (gloss != null) { gloss.Resize(box); }
            if (border != null) { border.Resize(box); }
            ResizeThis(box);
        }
        /// <summary></summary>
        /// <param name="box"></param>
        protected virtual void ResizeThis(Rectangle box) { }

        private EventHandler onPropertiesChanged;
        /// <summary></summary>
        public event EventHandler PropertiesChanged
        {
            add
            {
                if (onPropertiesChanged != null)
                {
                    foreach (Delegate d in onPropertiesChanged.GetInvocationList())
                    {
                        if (object.ReferenceEquals(d, value)) { return; }
                    }
                }
                onPropertiesChanged = (EventHandler)Delegate.Combine(onPropertiesChanged, value);
            }
            remove { onPropertiesChanged = (EventHandler)Delegate.Remove(onPropertiesChanged, value); }
        }

        /// <summary></summary>
        protected void FireChange()
        {
            if (onPropertiesChanged != null) { onPropertiesChanged(this, EventArgs.Empty); }
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeThis(disposing);
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void DisposeThis(bool disposing)
        {
        }
    }
    #endregion

    #region DualProgressBar "Icons.DualProgressBar.ico"  双进度条显示
    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(DualProgressBar), "Icons.DualProgressBar.ico")]
    public class DualProgressBar : ProgressBarEx
    {
        private int masterval = 0;
        private int mastermax = 100;
        private IProgressPainter masterpainter;
        private bool masterBottom = false;
        private Rectangle masterbox;
        private int padding = 0;

        protected EventHandler OnMasterValueChanged;
        /// <summary></summary>
        public event EventHandler MasterValueChanged
        {
            add
            {
                if (OnMasterValueChanged != null)
                {
                    foreach (Delegate d in OnMasterValueChanged.GetInvocationList())
                    {
                        if (object.ReferenceEquals(d, value)) { return; }
                    }
                }
                OnMasterValueChanged = (EventHandler)Delegate.Combine(OnMasterValueChanged, value);
            }
            remove { OnMasterValueChanged = (EventHandler)Delegate.Remove(OnMasterValueChanged, value); }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the maximum value"), Browsable(true)]
        public override int Maximum
        {
            get { return base.maximum; }
            set
            {
                base.Maximum = value;
                mastermax = value;
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the value of the master progress"), Browsable(true)]
        public int MasterValue
        {
            get { return this.masterval; }
            set
            {
                this.masterval = value;
                if (OnMasterValueChanged != null)
                {
                    OnMasterValueChanged(this, EventArgs.Empty);
                }
                ResizeMasterProgress();
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the maximum value for the master progress"), Browsable(true)]
        public int MasterMaximum
        {
            get { return mastermax; }
            set
            {
                this.mastermax = value;
                ResizeMasterProgress();
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the padding for the master progress"), Browsable(true)]
        public int MasterProgressPadding
        {
            get { return this.padding; }
            set
            {
                this.padding = value;
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, EventArgs.Empty);
                }
                ResizeMasterProgress();
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Painters"), Description("Paints this progress bar's master progress"), Browsable(true)]
        public IProgressPainter MasterPainter
        {
            get { return this.masterpainter; }
            set
            {
                this.masterpainter = value;
                if (this.masterpainter is AbstractProgressPainter)
                {
                    ((AbstractProgressPainter)this.masterpainter).padding = base.ProgressPadding;
                }
                this.masterpainter.PropertiesChanged += new EventHandler(component_PropertiesChanged);
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Determines whether or not the master progress is painted under the main progress"), Browsable(true)]
        public bool PaintMasterFirst
        {
            get { return this.masterBottom; }
            set
            {
                this.masterBottom = value;
                this.Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeProgress();
            ResizeMasterProgress();
            if (this.backgroundpainter != null) { this.backgroundpainter.Resize(borderbox); }
            if (masterBottom && this.masterpainter != null) { this.masterpainter.Resize(masterbox); }
            if (this.progresspainter != null) { this.progresspainter.Resize(borderbox); }
            if (!masterBottom && this.masterpainter != null) { this.masterpainter.Resize(masterbox); }
            if (this.borderpainter != null) { this.borderpainter.Resize(borderbox); }
        }

        private void ResizeMasterProgress()
        {
            Rectangle newprog = base.borderbox;
            newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);
            base.backbox = newprog;

            int val = masterval; if (val > 0) { val++; }
            int progWidth = mastermax > 0 ? (backbox.Width * val / mastermax) : 1;
            if (value >= mastermax && mastermax > 0)
            {
                progWidth = backbox.Width;
            } /*else if (value > 0) {
				progWidth++;
			}*/
            //newprog = new Rectangle(backbox.X + base.ProgressPadding, backbox.Y + base.ProgressPadding, progWidth - (base.ProgressPadding * 2), backbox.Height - (base.ProgressPadding * 2));
            //newprog = new Rectangle(backbox.X, backbox.Y, progWidth, backbox.Height);
            newprog = new Rectangle(backbox.X + this.padding, backbox.Y + this.padding, progWidth - (this.padding * 2), backbox.Height - (this.padding * 2));
            masterbox = newprog;
        }

        ///// <summary></summary>
        //protected override void MarqueeStart() {
        //}
        ///// <summary></summary>
        //protected override void MarqueePause() {
        //}
        ///// <summary></summary>
        //protected override void MarqueeStop() {
        //}

        /// <summary></summary>
        /// <param name="gr"></param>
        protected override void PaintProgress(Graphics g)
        {
            if (this.progresspainter != null)
            {
                if (masterBottom && this.masterpainter != null)
                {
                    this.masterpainter.PaintProgress(masterbox, g);
                }
                this.progresspainter.PaintProgress(progressbox, g);
                if (!masterBottom && this.masterpainter != null)
                {
                    this.masterpainter.PaintProgress(masterbox, g);
                }
            }
        }
    }
    #endregion

    #region PlainBackgroundPainter  "Icons.PlainBackgroundPainter.ico"
    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(PlainBackgroundPainter), "Icons.PlainBackgroundPainter.ico")]
    public class PlainBackgroundPainter : Component, IProgressBackgroundPainter, IDisposable
    {
        private Color color;
        private Brush brush;
        private IGlossPainter gloss;

        private EventHandler onPropertiesChanged;
        /// <summary></summary>
        public event EventHandler PropertiesChanged
        {
            add
            {
                if (onPropertiesChanged != null)
                {
                    foreach (Delegate d in onPropertiesChanged.GetInvocationList())
                    {
                        if (object.ReferenceEquals(d, value)) { return; }
                    }
                }
                onPropertiesChanged = (EventHandler)Delegate.Combine(onPropertiesChanged, value);
            }
            remove { onPropertiesChanged = (EventHandler)Delegate.Remove(onPropertiesChanged, value); }
        }

        private void FireChange()
        {
            if (onPropertiesChanged != null) { onPropertiesChanged(this, EventArgs.Empty); }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void component_PropertiesChanged(object sender, EventArgs e)
        {
            FireChange();
        }

        /// <summary></summary>
        public PlainBackgroundPainter()
        {
            this.Color = Color.FromArgb(240, 240, 240);
        }

        /// <summary></summary>
        /// <param name="color"></param>
        public PlainBackgroundPainter(Color color)
        {
            this.Color = color;
        }

        /// <summary></summary>
        /// <summary></summary>
        [Category("Painters"), Description("Gets or sets the chain of gloss painters"), Browsable(true)]
        public IGlossPainter GlossPainter
        {
            get { return this.gloss; }
            set
            {
                this.gloss = value;
                if (this.gloss != null) { this.gloss.PropertiesChanged += new EventHandler(component_PropertiesChanged); }
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the background color"), Browsable(true)]
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                brush = new SolidBrush(color);
                FireChange();
            }
        }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="g"></param>
        public void PaintBackground(Rectangle box, Graphics g)
        {
            g.FillRectangle(brush, box);

            if (gloss != null)
            {
                gloss.PaintGloss(box, g);
            }
        }

        /// <summary></summary>
        public void Resize(Rectangle box)
        {
        }

        /// <summary></summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            brush.Dispose();
        }
    }
    #endregion

    #region PlainProgressPainter "Icons.PlainProgressPainter.ico"
    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(PlainProgressPainter), "Icons.PlainProgressPainter.ico")]
    public class PlainProgressPainter : AbstractProgressPainter, IProgressPainter, IDisposable
    {
        private Color color;
        private Brush brush;
        private Color edge = Color.Transparent;

        /// <summary></summary>
        public PlainProgressPainter()
        {
            this.Color = Color.FromArgb(151, 151, 234);
        }

        /// <summary></summary>
        /// <param name="color"></param>
        public PlainProgressPainter(Color color)
        {
            this.Color = color;
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the color to draw the leading edge of the progress with"), Browsable(true)]
        public Color LeadingEdge
        {
            get { return this.edge; }
            set { this.edge = value; FireChange(); }
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the base progress color"), Browsable(true)]
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                brush = new SolidBrush(color);
                FireChange();
            }
        }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="g"></param>
        protected override void PaintThisProgress(Rectangle box, Graphics g)
        {
            if (box.Width <= 1)
            {
                return;
            }

            g.FillRectangle(brush, box);
            if (gloss != null)
            {
                gloss.PaintGloss(box, g);
            }
            if (!edge.Equals(Color.Transparent))
            {
                g.DrawLine(new Pen(new SolidBrush(edge), 1f), box.Right, box.Y, box.Right, box.Bottom - 1);
            }
        }

        /// <summary></summary>
        protected override void DisposeThis(bool disposing)
        {
            brush.Dispose();
        }
    }

    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(PlainBorderPainter), "Icons.PlainBorderPainter.ico")]
    public class PlainBorderPainter : Component, IProgressBorderPainter, IDisposable
    {
        private Color color;
        private Pen pent;
        private Pen penb;
        private Pen cleart;
        private Pen clearb;
        private bool rounded = false;
        private PlainBorderStyle style = PlainBorderStyle.Flat;

        private EventHandler onPropertiesChanged;
        /// <summary></summary>
        public event EventHandler PropertiesChanged
        {
            add
            {
                if (onPropertiesChanged != null)
                {
                    foreach (Delegate d in onPropertiesChanged.GetInvocationList())
                    {
                        if (object.ReferenceEquals(d, value)) { return; }
                    }
                }
                onPropertiesChanged = (EventHandler)Delegate.Combine(onPropertiesChanged, value);
            }
            remove { onPropertiesChanged = (EventHandler)Delegate.Remove(onPropertiesChanged, value); }
        }

        private void FireChange()
        {
            if (onPropertiesChanged != null) { onPropertiesChanged(this, EventArgs.Empty); }
        }

        /// <summary></summary>
        public PlainBorderPainter()
        {
            this.Color = Color.Black;
            //this.clear = new Pen(new SolidBrush(SystemColors.Control));
        }

        /// <summary></summary>
        /// <param name="color"></param>
        public PlainBorderPainter(Color color)
        {
            this.Color = color;
            //this.clear = new Pen(new SolidBrush(SystemColors.Control));
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the border color"), Browsable(true)]
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                if (style == PlainBorderStyle.Flat)
                {
                    pent = new Pen(new SolidBrush(color), 1f);
                    penb = pent;
                    this.cleart = new Pen(new SolidBrush(Color.FromArgb(64, color.R, color.G, color.B)));
                    this.clearb = cleart;
                    FireChange();
                }
            }
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Determines wether or not to make the border a flat sqaure"), Browsable(true)]
        public bool RoundedCorners
        {
            get { return rounded; }
            set { rounded = value; FireChange(); }
        }

        [Category("Appearance"), Description("Gets or sets the border style"), Browsable(true)]
        public PlainBorderStyle Style
        {
            get { return style; }
            set
            {
                style = value;
                switch (style)
                {
                    case PlainBorderStyle.Flat:
                        pent = new Pen(new SolidBrush(color), 1f);
                        penb = pent;
                        this.cleart = new Pen(new SolidBrush(Color.FromArgb(64, color.R, color.G, color.B)));
                        this.clearb = cleart;
                        break;
                    case PlainBorderStyle.Raised:
                        pent = new Pen(new SolidBrush(SystemColors.ControlLightLight), 1f);
                        penb = new Pen(new SolidBrush(SystemColors.ControlDark), 1f);
                        this.cleart = new Pen(new SolidBrush(Color.FromArgb(64, SystemColors.ControlLightLight.R, SystemColors.ControlLightLight.G, SystemColors.ControlLightLight.B)));
                        this.clearb = new Pen(new SolidBrush(Color.FromArgb(64, SystemColors.ControlDark.R, SystemColors.ControlDark.G, SystemColors.ControlDark.B)));
                        break;
                    case PlainBorderStyle.Sunken:
                        pent = new Pen(new SolidBrush(SystemColors.ControlDark), 1f);
                        penb = new Pen(new SolidBrush(SystemColors.ControlLightLight), 1f);
                        this.cleart = new Pen(new SolidBrush(Color.FromArgb(64, SystemColors.ControlDark.R, SystemColors.ControlDark.G, SystemColors.ControlDark.B)));
                        this.clearb = new Pen(new SolidBrush(Color.FromArgb(64, SystemColors.ControlLightLight.R, SystemColors.ControlLightLight.G, SystemColors.ControlLightLight.B)));
                        break;
                }
                FireChange();
            }
        }

        /// <summary></summary>
        [Browsable(false)]
        public int BorderWidth
        {
            get { return 1; }
        }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="g"></param>
        public void PaintBorder(Rectangle box, Graphics g)
        {
            //try {
            //    box.Width -= 1;
            //    box.Height -= 1;
            //} catch {}
            if (rounded)
            {
                //// draws the left and right side (because they're shorter) to cover the corner pixels.
                //g.DrawLine(clear, box.X, 0, box.X, box.Height);
                //g.DrawLine(clear, box.Width, 0, box.Width, box.Height);

                g.DrawLine(cleart, box.X, box.Y, box.Right - 1, box.Y); // top
                g.DrawLine(cleart, box.X, box.Y, box.X, box.Bottom - 1); // left
                g.DrawLine(clearb, box.X, box.Bottom, box.Right, box.Bottom); // bottom
                g.DrawLine(clearb, box.Right, box.Y, box.Right, box.Bottom); // right

                //g.DrawRectangle(clear, box);
                g.DrawLine(pent, box.X + 1, box.Y, box.Right - 1, box.Y); // top
                g.DrawLine(penb, box.X + 1, box.Bottom, box.Right - 1, box.Bottom); // bottom
                g.DrawLine(pent, box.X, box.Y + 1, box.X, box.Bottom - 1); // left
                g.DrawLine(penb, box.Right, box.Y + 1, box.Right, box.Bottom - 1); // right
            }
            else
            {
                //g.DrawRectangle(pen, box);
                g.DrawLine(pent, box.X, box.Y, box.Right, box.Y); // top
                g.DrawLine(pent, box.X, box.Y, box.X, box.Bottom); // left
                g.DrawLine(penb, box.X, box.Bottom, box.Right, box.Bottom); // bottom
                g.DrawLine(penb, box.Right, box.Y, box.Right, box.Bottom); // right
            }
        }

        /// <summary></summary>
        public void Resize(Rectangle box)
        {
        }

        /// <summary></summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            pent.Dispose();
            penb.Dispose();
            cleart.Dispose();
            clearb.Dispose();
        }

        /// <summary></summary>
        public enum PlainBorderStyle
        {
            Flat, Sunken, Raised
        }
    }
    #endregion

    #region ProgressBarEx "Icons.ProgressBarEx.ico"
    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(ProgressBarEx), "Icons.ProgressBarEx.ico")]
    public class ProgressBarEx : AbstractProgressBar
    {
        protected IProgressBackgroundPainter backgroundpainter;
        protected IProgressPainter progresspainter;
        protected IProgressBorderPainter borderpainter;

        public ProgressBarEx()
        {
            backgroundpainter = new PlainBackgroundPainter();
            progresspainter = new PlainProgressPainter(Color.Gold);
            borderpainter = new PlainBorderPainter();
        }

        /// <summary></summary>
        [Category("Painters"), Description("Paints this progress bar's background"), Browsable(true)]
        public IProgressBackgroundPainter BackgroundPainter
        {
            get { return this.backgroundpainter; }
            set
            {
                this.backgroundpainter = value;
                this.backgroundpainter.PropertiesChanged += new EventHandler(component_PropertiesChanged);
                this.Invalidate();
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void component_PropertiesChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        /// <summary></summary>
        [Category("Painters"), Description("Paints this progress bar's progress"), Browsable(true)]
        public IProgressPainter ProgressPainter
        {
            get { return this.progresspainter; }
            set
            {
                if (!(value is IAnimatedProgressPainter) && base.ProgressType == ProgressType.Animated)
                {
                    base.ProgressType = ProgressType.Smooth;
                }
                this.progresspainter = value;
                if (this.progresspainter is AbstractProgressPainter)
                {
                    ((AbstractProgressPainter)this.progresspainter).padding = base.ProgressPadding;
                }
                this.progresspainter.PropertiesChanged += new EventHandler(component_PropertiesChanged);
                this.Invalidate();
            }
        }

        /// <summary></summary>
        [Category("Progress"), Description("Gets or sets the type of progress"), Browsable(true)]
        public override ProgressType ProgressType
        {
            get { return base.type; }
            set
            {
                if (value == ProgressType.Animated && !(progresspainter is IAnimatedProgressPainter))
                {
                    throw new ArgumentException("Animated is not available with the current Progress Painter");
                }
                this.type = value;
            }
        }

        /// <summary></summary>
        [Category("Painters"), Description("Paints this progress bar's border"), Browsable(true)]
        public IProgressBorderPainter BorderPainter
        {
            get { return this.borderpainter; }
            set
            {
                this.borderpainter = value;
                this.borderpainter.PropertiesChanged += new EventHandler(component_PropertiesChanged);
                ResizeProgress();
                this.Invalidate();
            }
        }

        protected override void ResizeProgress()
        {
            if (base.ProgressType != ProgressType.Smooth) { return; }
            Rectangle newprog = base.borderbox;
            //newprog.Inflate(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);
            base.backbox = newprog;

            int val = value; if (val > 0) { val++; }
            int progWidth = maximum > 0 ? (backbox.Width * val / maximum) : 1;
            if (value >= maximum && maximum > 0)
            {
                progWidth = backbox.Width;
            } /*else if (value > 0) {
				progWidth++;
			}*/
            newprog.Inflate(-base.ProgressPadding, -base.ProgressPadding);
            newprog.Width = progWidth - (base.ProgressPadding * 2);
            //newprog.Offset(base.ProgressPadding, base.ProgressPadding);
            //newprog = new Rectangle(backbox.X + base.ProgressPadding, backbox.Y + base.ProgressPadding, progWidth - (base.ProgressPadding * 2), backbox.Height - (base.ProgressPadding * 2));
            base.progressbox = newprog;
        }

        #region Animation
        public void StartAnimation()
        {
            if (running) { return; }
            IAnimatedProgressPainter iapp = this.progresspainter as IAnimatedProgressPainter;
            if (iapp == null) { return; }
            iapp.Animating = true;
            running = true;
            timerMethod = new EventHandler(DoAnimation);
            timer.Interval = iapp.AnimationSpeed;
            timer.Tick += timerMethod;
            timer.Enabled = true;
        }
        public void StopAnimation()
        {
            timer.Enabled = false;
            timer.Tick -= timerMethod;
            running = false;
            IAnimatedProgressPainter iapp = this.progresspainter as IAnimatedProgressPainter;
            if (iapp == null) { return; }
            iapp.Animating = false;
        }
        private void DoAnimation(object sender, EventArgs e)
        {
            IAnimatedProgressPainter iapp = this.progresspainter as IAnimatedProgressPainter;
            if (iapp == null) { return; }

            //Rectangle newprog = base.borderbox;
            //newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            //newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);
            ////int progWidth = (int)(((float)marqueePercentage * (float)backbox.Width) / 100f);
            //int progWidth = (int)(((float)marqueePercentage * (float)backbox.Width) / 100f);
            //newprog.Inflate(-base.ProgressPadding, -base.ProgressPadding);
            //newprog.Width = progWidth - (base.ProgressPadding * 2);

            //base.progressbox = newprog;

            ////iapp.AnimateFrame(newprog, g, ref marqueeX);

            this.Invalidate();
            this.Refresh();
        }
        #endregion

        #region Marquee
        private bool running = false;
        private Timer timer = new Timer();
        private EventHandler timerMethod;
        /// <summary></summary>
        public override void MarqueeStart()
        {
            if (running) { return; }
            running = true;
            switch (base.ProgressType)
            {
                case ProgressType.MarqueeWrap: timerMethod = new EventHandler(DoMarqueeWrap); break;
                case ProgressType.MarqueeBounce: timerMethod = new EventHandler(DoMarqueeBounce); break;
                case ProgressType.MarqueeBounceDeep: timerMethod = new EventHandler(DoMarqueeDeep); break;
            }
            timer.Interval = base.marqueeSpeed;
            timer.Tick += timerMethod;
            timer.Enabled = true;
        }

        private int marqueeX = 0;
        private void DoMarqueeWrap(object sender, EventArgs e)
        {
            Rectangle newprog = base.borderbox;
            newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);

            int progWidth = (int)(((float)marqueePercentage * (float)backbox.Width) / 100f);

            marqueeX += marqueeStep;
            if (marqueeX > backbox.Width)
            {
                marqueeX = 0 - progWidth;
            }

            newprog.Inflate(-base.ProgressPadding, -base.ProgressPadding);
            newprog.Width = progWidth - (base.ProgressPadding * 2);
            newprog.X += marqueeX;

            int leftBoundry = backbox.X + this.borderpainter.BorderWidth + base.ProgressPadding;
            int rightBoundry = backbox.X + backbox.Width - (this.borderpainter.BorderWidth + base.ProgressPadding);
            if (marqueeX <= leftBoundry)
            {
                newprog.Width -= leftBoundry - marqueeX;
                newprog.X = leftBoundry;
            }
            else if (marqueeX + newprog.Width >= rightBoundry)
            {
                newprog.Width -= (marqueeX + newprog.Width + base.ProgressPadding) - rightBoundry;
            }

            base.progressbox = newprog;

            this.Invalidate();
            this.Refresh();
        }
        private bool marqueeForward = true;
        private void DoMarqueeBounce(object sender, EventArgs e)
        {
            Rectangle newprog = base.borderbox;
            newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);

            int progWidth = (int)(((float)marqueePercentage * (float)backbox.Width) / 100f);

            if (marqueeForward)
            {
                marqueeX += marqueeStep;
            }
            else
            {
                marqueeX -= marqueeStep;
            }

            newprog.Inflate(-base.ProgressPadding, -base.ProgressPadding);
            newprog.Width = progWidth - (base.ProgressPadding * 2);
            newprog.X += marqueeX;

            int leftBoundry = backbox.X + this.borderpainter.BorderWidth + base.ProgressPadding;
            int rightBoundry = backbox.X + backbox.Width - (this.borderpainter.BorderWidth + base.ProgressPadding);
            if (marqueeX + progWidth >= rightBoundry)
            {
                marqueeForward = false;
            }
            else if (marqueeX <= leftBoundry)
            {
                marqueeForward = true;
            }

            base.progressbox = newprog;

            this.Invalidate();
            this.Refresh();
        }
        private void DoMarqueeDeep(object sender, EventArgs e)
        {
            Rectangle newprog = base.borderbox;
            newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);

            int progWidth = (int)(((float)marqueePercentage * (float)backbox.Width) / 100f);

            if (marqueeForward)
            {
                marqueeX += marqueeStep;
            }
            else
            {
                marqueeX -= marqueeStep;
            }
            if (marqueeX > backbox.Width)
            {
                marqueeForward = false;
            }
            else if (marqueeX < backbox.X - progWidth)
            {
                marqueeForward = true;
            }

            newprog.Inflate(-base.ProgressPadding, -base.ProgressPadding);
            newprog.Width = progWidth - (base.ProgressPadding * 2);
            newprog.X += marqueeX;

            int leftBoundry = backbox.X + this.borderpainter.BorderWidth + base.ProgressPadding;
            int rightBoundry = backbox.X + backbox.Width - (this.borderpainter.BorderWidth + base.ProgressPadding);
            if (marqueeX <= leftBoundry)
            {
                newprog.Width -= leftBoundry - marqueeX;
                newprog.X = leftBoundry;
            }
            else if (marqueeX + newprog.Width >= rightBoundry)
            {
                newprog.Width -= (marqueeX + newprog.Width + base.ProgressPadding) - rightBoundry;
            }

            base.progressbox = newprog;

            this.Invalidate();
            this.Refresh();
        }

        /// <summary></summary>
        public override void MarqueePause()
        {
            running = false;
            timer.Enabled = false;
            timer.Tick -= timerMethod;
        }
        /// <summary></summary>
        public override void MarqueeStop()
        {
            Rectangle newprog = base.borderbox;
            newprog.Offset(this.borderpainter.BorderWidth, this.borderpainter.BorderWidth);
            newprog.Size = new Size(newprog.Size.Width - this.borderpainter.BorderWidth, newprog.Size.Height - this.borderpainter.BorderWidth);

            newprog.Inflate(-base.ProgressPadding, -base.ProgressPadding);
            newprog.Width = 1;
            base.progressbox = newprog;

            running = false;
            timer.Enabled = false;
            timer.Tick -= timerMethod;

            marqueeX = 0;
            this.Invalidate();
        }
        #endregion

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (running) { running = false; }
        }

        /// <summary></summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeProgress();
            if (this.backgroundpainter != null) { this.backgroundpainter.Resize(borderbox); }
            if (this.progresspainter != null) { this.progresspainter.Resize(borderbox); }
            if (this.borderpainter != null) { this.borderpainter.Resize(borderbox); }
        }

        /// <summary></summary>
        /// <param name="gr"></param>
        protected override void PaintBackground(Graphics g)
        {
            if (this.backgroundpainter != null)
            {
                this.backgroundpainter.PaintBackground(backbox, g);
            }
        }

        /// <summary></summary>
        /// <param name="gr"></param>
        protected override void PaintProgress(Graphics g)
        {
            if (this.progresspainter != null)
            {
                this.progresspainter.PaintProgress(progressbox, g);
            }
        }

        /// <summary></summary>
        /// <param name="gr"></param>
        protected override void PaintText(Graphics g)
        {
            if (base.ProgressType != ProgressType.Smooth) { return; }
            Brush b = new SolidBrush(ForeColor);
            SizeF sf = g.MeasureString(Text, Font, Convert.ToInt32(Width), StringFormat.GenericDefault);
            float m = sf.Width;
            float x = (Width / 2) - (m / 2);
            float w = (Width / 2) + (m / 2);
            float h = (float)borderbox.Height - (2f * (float)this.borderpainter.BorderWidth);
            float y = (float)this.borderpainter.BorderWidth + ((h - sf.Height) / 2f);
            g.DrawString(Text, Font, b, RectangleF.FromLTRB(x, y, w, Height - 1), StringFormat.GenericDefault);
        }

        /// <summary></summary>
        /// <param name="gr"></param>
        protected override void PaintBorder(Graphics g)
        {
            if (this.borderpainter != null)
            {
                this.borderpainter.PaintBorder(borderbox, g);
            }
        }
    }
    #endregion

    /// <summary>Extending this class allows you to chain multiple IGlossPainters together.</summary>
    public abstract class ChainedGlossPainter : Component, IGlossPainter, IDisposable
    {
        private IGlossPainter successor = null;

        /// <summary></summary>
        [Category("Painters"), Description("Gets or sets the next gloss in the chain"), Browsable(true)]
        public IGlossPainter Successor
        {
            get { return successor; }
            set
            {
                IGlossPainter nextPainter = value;
                while (nextPainter != null && nextPainter is ChainedGlossPainter)
                {
                    if (object.ReferenceEquals(this, nextPainter))
                    {
                        throw new ArgumentException("Gloss cannot eventually be it's own successor, an infinite loop will result");
                    }
                    nextPainter = ((ChainedGlossPainter)nextPainter).Successor;
                }

                successor = value;
                if (successor != null)
                {
                    successor.PropertiesChanged += new EventHandler(successor_PropertiesChanged);
                }
                FireChange();
            }
        }
        private void successor_PropertiesChanged(object sender, EventArgs e)
        {
            FireChange();
        }

        private EventHandler onPropertiesChanged;
        /// <summary></summary>
        public event EventHandler PropertiesChanged
        {
            add
            {
                if (onPropertiesChanged != null)
                {
                    foreach (Delegate d in onPropertiesChanged.GetInvocationList())
                    {
                        if (object.ReferenceEquals(d, value)) { return; }
                    }
                }
                onPropertiesChanged = (EventHandler)Delegate.Combine(onPropertiesChanged, value);
            }
            remove { onPropertiesChanged = (EventHandler)Delegate.Remove(onPropertiesChanged, value); }
        }

        /// <summary></summary>
        protected void FireChange()
        {
            if (onPropertiesChanged != null) { onPropertiesChanged(this, EventArgs.Empty); }
        }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="g"></param>
        public void PaintGloss(Rectangle box, Graphics g)
        {
            if (box.Width < 1) { return; }
            PaintThisGloss(box, g);
            if (successor != null) { successor.PaintGloss(box, g); }
        }

        /// <summary></summary>
        /// <param name="box"></param>
        /// <param name="g"></param>
        protected abstract void PaintThisGloss(Rectangle box, Graphics g);

        /// <summary></summary>
        /// <param name="box"></param>
        public void Resize(Rectangle box)
        {
            ResizeThis(box);
            if (successor != null) { successor.Resize(box); }
        }

        protected abstract void ResizeThis(Rectangle box);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (successor != null) { successor.Dispose(); }
        }
    }

    /// <summary></summary>
    public enum GlossStyle
    {
        Top, Bottom, Both
    }

    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(RoundGlossPainter), "Icons.RoundGlossPainter.ico")]
    public class RoundGlossPainter : ChainedGlossPainter
    {
        private GlossStyle style = GlossStyle.Both;
        private int highAlpha = 240;
        private int lowAlpha = 0;
        private int fadewidth = 4;
        private Brush highBrush;
        private Brush lowBrush;
        private Rectangle box;
        private Color color = Color.White;
        private Color topColor;
        private Color botColor;

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the style for this progress gloss"), Browsable(true)]
        public GlossStyle Style
        {
            get { return this.style; }
            set
            {
                this.style = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Blending"), Description("Gets or sets the high alpha value"), Browsable(true)]
        public int AlphaHigh
        {
            get { return this.highAlpha; }
            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentException("Alpha values must be between 0 and 255.");
                }
                this.highAlpha = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Blending"), Description("Gets or sets the low alpha value"), Browsable(true)]
        public int AlphaLow
        {
            get { return this.lowAlpha; }
            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentException("Alpha values must be between 0 and 255.");
                }
                this.lowAlpha = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Blending"), Description("Gets or sets the number of pixels to blend over"), Browsable(true)]
        public int TaperHeight
        {
            get { return this.fadewidth; }
            set
            {
                this.fadewidth = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets color to gloss"), Browsable(true)]
        public Color Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                this.topColor = Color.FromArgb(highAlpha, this.color.R, this.color.G, this.color.B);
                this.botColor = Color.FromArgb(lowAlpha, this.color.R, this.color.G, this.color.B);
                box = new Rectangle(0, 0, 1, 1);
                FireChange();
            }
        }

        protected override void PaintThisGloss(Rectangle box, Graphics g)
        {
            if (!this.box.Equals(box))
            {
                this.box = box;
                ResetBrushes(box);
            }

            //int midpoint = (int)((float)box.Height / 2f);
            //int toppoint = midpoint > (fadewidth + 2) ? midpoint - (fadewidth / 2) : midpoint;
            //int botpoint = midpoint > (fadewidth + 2) ? midpoint + (fadewidth / 2) : midpoint;

            //Rectangle topBox = new Rectangle(box.X, box.Y, box.Width - 1, box.Y + fadewidth);
            //Rectangle botBox = new Rectangle(box.X, box.Bottom - fadewidth - 2, box.Width - 1, fadewidth + 1);
            Rectangle topBox = new Rectangle(box.X, box.Y, box.Width, fadewidth);
            Rectangle botBox = new Rectangle(box.X, box.Bottom - fadewidth, box.Width, fadewidth);

            //if (midpoint - fadewidth > 2) { midpoint -= fadewidth; }

            switch (style)
            {
                case GlossStyle.Bottom:
                    g.FillRectangle(lowBrush, botBox);
                    break;
                case GlossStyle.Top:
                    g.FillRectangle(highBrush, topBox);
                    break;
                case GlossStyle.Both:
                    g.FillRectangle(highBrush, topBox);
                    g.FillRectangle(lowBrush, botBox);
                    //g.DrawRectangle(Pens.Red, topBox);
                    //g.DrawRectangle(Pens.Blue, botBox);
                    break;
            }
            //g.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(64, 255, 0, 0))), topBox);
            //g.FillRectangle(new SolidBrush(Color.FromArgb(32, 255, 0, 0)), topBox);
            //g.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(64, 0, 0, 255))), botBox);
            //g.FillRectangle(new SolidBrush(Color.FromArgb(32, 0, 0, 255)), botBox);
        }

        protected override void ResizeThis(Rectangle box)
        {
            if (!this.box.Equals(box))
            {
                this.box = box;
                ResetBrushes(box);
            }
        }

        private void ResetBrushes(Rectangle box)
        {
            //int midpoint = (int)((float)box.Height / 2f);
            //int toppoint = midpoint - (fadewidth / 2);
            //if (toppoint < box.Y + 2) { toppoint = box.Y + 2; }
            //int botpoint = midpoint + (fadewidth / 2);
            //if (botpoint > box.Height - 2) { botpoint = box.Height - 2; }

            //Point top = new Point(box.X, box.Y);
            //Point topmid = new Point(box.X, box.Y + fadewidth + 1);
            //Point botmid = new Point(box.X, box.Height - fadewidth - 1);
            //Point bot = new Point(box.X, box.Bottom);

            Rectangle topBox = new Rectangle(box.X, box.Y, box.Width, fadewidth);
            Rectangle botBox = new Rectangle(box.X, box.Bottom - fadewidth, box.Width, fadewidth);
            Point top = new Point(box.X, topBox.Top);
            Point topmid = new Point(box.X, topBox.Bottom);
            Point botmid = new Point(box.X, botBox.Top - 1);
            Point bot = new Point(box.X, botBox.Bottom);

            Color high = topColor;
            Color low = botColor;
            switch (style)
            {
                case GlossStyle.Top:
                    highBrush = new LinearGradientBrush(top, topmid, high, low);
                    break;
                case GlossStyle.Bottom:
                    lowBrush = new LinearGradientBrush(botmid, bot, low, high);
                    break;
                case GlossStyle.Both:
                    highBrush = new LinearGradientBrush(top, topmid, high, low);
                    lowBrush = new LinearGradientBrush(botmid, bot, low, high);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (highBrush != null) { highBrush.Dispose(); }
            if (lowBrush != null) { lowBrush.Dispose(); }
        }
    }

    /// <summary></summary>
    [ToolboxBitmapAttribute(typeof(GradientGlossPainter), "Icons.GradientGlossPainter.ico")]
    public class GradientGlossPainter : ChainedGlossPainter
    {
        private GlossStyle style = GlossStyle.Bottom;
        private int percent = 50;
        private Color color = Color.White;
        private int highAlpha = 240;
        private int lowAlpha = 0;
        private float angle = 90f;
        private Brush brush;

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the style for this progress gloss"), Browsable(true)]
        public GlossStyle Style
        {
            get { return this.style; }
            set
            {
                this.style = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets the percentage of surface this gloss should cover"), Browsable(true)]
        public int PercentageCovered
        {
            get { return this.percent; }
            set
            {
                this.percent = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Appearance"), Description("Gets or sets color to gloss"), Browsable(true)]
        public Color Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Blending"), Description("Gets or sets the high alpha value"), Browsable(true)]
        public int AlphaHigh
        {
            get { return this.highAlpha; }
            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentException("Alpha values must be between 0 and 255.");
                }
                this.highAlpha = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Blending"), Description("Gets or sets the low alpha value"), Browsable(true)]
        public int AlphaLow
        {
            get { return this.lowAlpha; }
            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentException("Alpha values must be between 0 and 255.");
                }
                this.lowAlpha = value;
                FireChange();
            }
        }

        /// <summary></summary>
        [Category("Blending"), Description("Gets or sets angle for the gradient"), Browsable(true)]
        public float Angle
        {
            get { return this.angle; }
            set
            {
                this.angle = value;
                FireChange();
            }
        }

        protected override void PaintThisGloss(Rectangle box, Graphics g)
        {
            int y = (int)(((float)box.Height * (float)percent) / 100f);
            if (box.Y + y > box.Height) { y = box.Height; }

            Rectangle cover = box;
            switch (style)
            {
                case GlossStyle.Bottom:
                    int start = box.Height + box.Y - y;
                    cover = new Rectangle(box.X, start - 1, box.Width /*- 1*/, box.Bottom - start);
                    break;
                case GlossStyle.Top:
                    cover = new Rectangle(box.X, box.Y - 1, box.Width /*- 1*/, y + 2);
                    break;
                case GlossStyle.Both:
                    cover = box;
                    break;
            }

            Color hcolor = Color.FromArgb(highAlpha, color.R, color.G, color.B);
            Color lcolor = Color.FromArgb(lowAlpha, color.R, color.G, color.B);
            brush = new LinearGradientBrush(cover, hcolor, lcolor, angle, true);
            g.FillRectangle(brush, cover);
            //g.DrawRectangle(Pens.Red, cover);
        }

        protected override void ResizeThis(Rectangle box)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (brush != null) { brush.Dispose(); }
        }
    }
}

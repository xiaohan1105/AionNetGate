namespace AionLanucher
{
    partial class ServerSelect
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerSelect));
            this.label网通延时 = new System.Windows.Forms.Label();
            this.label电信延时 = new System.Windows.Forms.Label();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonEx3 = new AionLanucher.FormSkin.ButtonEx();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label网通延时
            // 
            this.label网通延时.AutoSize = true;
            this.label网通延时.BackColor = System.Drawing.Color.Transparent;
            this.label网通延时.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label网通延时.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label网通延时.Location = new System.Drawing.Point(169, 90);
            this.label网通延时.Name = "label网通延时";
            this.label网通延时.Size = new System.Drawing.Size(49, 14);
            this.label网通延时.TabIndex = 2;
            this.label网通延时.Text = "0 毫秒";
            // 
            // label电信延时
            // 
            this.label电信延时.AutoSize = true;
            this.label电信延时.BackColor = System.Drawing.Color.Transparent;
            this.label电信延时.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label电信延时.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label电信延时.Location = new System.Drawing.Point(169, 62);
            this.label电信延时.Name = "label电信延时";
            this.label电信延时.Size = new System.Drawing.Size(49, 14);
            this.label电信延时.TabIndex = 2;
            this.label电信延时.Text = "0 毫秒";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.BackColor = System.Drawing.Color.Transparent;
            this.radioButton2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.radioButton2.Location = new System.Drawing.Point(82, 88);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(81, 18);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "网通线路";
            this.radioButton2.UseVisualStyleBackColor = false;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.BackColor = System.Drawing.Color.Transparent;
            this.radioButton1.Checked = true;
            this.radioButton1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton1.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.radioButton1.Location = new System.Drawing.Point(82, 60);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(81, 18);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "电信线路";
            this.radioButton1.UseVisualStyleBackColor = false;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label4.Location = new System.Drawing.Point(79, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(136, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "请选择服务器线路";
            // 
            // buttonEx3
            // 
            this.buttonEx3.ADialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonEx3.BackColor = System.Drawing.Color.Transparent;
            this.buttonEx3.BackgroundImage = global::AionLanucher.Properties.Resources.绿色按钮常规;
            this.buttonEx3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonEx3.Caption = "确认选择";
            this.buttonEx3.CaptionColor = System.Drawing.SystemColors.ControlText;
            this.buttonEx3.CaptionFont = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonEx3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonEx3.DownImage = global::AionLanucher.Properties.Resources.绿色按钮按下;
            this.buttonEx3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonEx3.Location = new System.Drawing.Point(95, 142);
            this.buttonEx3.MoveImage = global::AionLanucher.Properties.Resources.绿色按钮经过;
            this.buttonEx3.Name = "buttonEx3";
            this.buttonEx3.NormalImage = global::AionLanucher.Properties.Resources.绿色按钮常规;
            this.buttonEx3.Size = new System.Drawing.Size(115, 42);
            this.buttonEx3.TabIndex = 13;
            this.buttonEx3.Click += new System.EventHandler(this.buttonEx3_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.Transparent;
            this.checkBox1.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.checkBox1.Location = new System.Drawing.Point(82, 117);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(156, 16);
            this.checkBox1.TabIndex = 14;
            this.checkBox1.Text = "记住线路，下次不再询问";
            this.checkBox1.UseVisualStyleBackColor = false;
            // 
            // ServerSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::AionLanucher.Properties.Resources.服务器选择;
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.buttonEx3);
            this.Controls.Add(this.label网通延时);
            this.Controls.Add(this.label电信延时);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.label4);
            this.Name = "ServerSelect";
            this.SkinBack = global::AionLanucher.Properties.Resources.服务器选择;
            this.SkinOpacity = 240;
            this.SkinSize = new System.Drawing.Size(300, 200);
            this.SkinWhetherTank = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "服务器选择";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ServerSelect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Label label电信延时;
        private System.Windows.Forms.Label label网通延时;
        private FormSkin.ButtonEx buttonEx3;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}
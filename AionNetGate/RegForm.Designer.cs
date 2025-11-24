namespace AionNetGate
{
    partial class RegForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegForm));
            this.skinPanel1 = new System.Windows.Forms.Panel();
            this.skinButton = new System.Windows.Forms.Button();
            this.textBox注册码 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox机器码 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.skinPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // skinPanel1
            // 
            this.skinPanel1.BackColor = System.Drawing.Color.Transparent;
            this.skinPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skinPanel1.Controls.Add(this.skinButton);
            this.skinPanel1.Controls.Add(this.textBox注册码);
            this.skinPanel1.Controls.Add(this.label2);
            this.skinPanel1.Controls.Add(this.textBox机器码);
            this.skinPanel1.Controls.Add(this.label1);
     
            this.skinPanel1.Location = new System.Drawing.Point(12, 12);
          
            this.skinPanel1.Name = "skinPanel1";
    
            this.skinPanel1.Size = new System.Drawing.Size(287, 179);
            this.skinPanel1.TabIndex = 0;
            // 
            // skinButton
            // 
            this.skinButton.Location = new System.Drawing.Point(78, 128);
            this.skinButton.Name = "skinButton";
            this.skinButton.Size = new System.Drawing.Size(168, 34);
            this.skinButton.TabIndex = 1;
            this.skinButton.Text = "注册";
            this.skinButton.UseVisualStyleBackColor = true;
            this.skinButton.Click += new System.EventHandler(this.skinButton1_Click);
            // 
            // textBox注册码
            // 
            this.textBox注册码.Location = new System.Drawing.Point(78, 49);
            this.textBox注册码.Multiline = true;
            this.textBox注册码.Name = "textBox注册码";
            this.textBox注册码.Size = new System.Drawing.Size(168, 73);
            this.textBox注册码.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "注册码：";
            // 
            // textBox机器码
            // 
            this.textBox机器码.Location = new System.Drawing.Point(78, 22);
            this.textBox机器码.Name = "textBox机器码";
            this.textBox机器码.ReadOnly = true;
            this.textBox机器码.Size = new System.Drawing.Size(168, 21);
            this.textBox机器码.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "机器码：";
            // 
            // RegForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 204);
            this.Controls.Add(this.skinPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(324, 240);
            this.Name = "RegForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "授权注册";
            this.Load += new System.EventHandler(this.RegForm_Load);
            this.skinPanel1.ResumeLayout(false);
            this.skinPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel skinPanel1;
        private System.Windows.Forms.TextBox textBox注册码;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox机器码;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button skinButton;
    }
}
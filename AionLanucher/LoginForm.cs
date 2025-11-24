using AionLanucher.Network;
using AionLanucher.Network.Server;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AionLanucher
{
    partial class LoginForm : SkinMain
    {
        private bool _isStartGame;

        internal static LoginForm Instance;

        private AionConnection con;

        private Dictionary<string, string> userPasswors = new Dictionary<string, string>();
        /// <summary>
        /// 验证码
        /// </summary>
        private ValidCode validCode;

        private Size skinSize = new Size(400, 388);

        private Point loc = new Point(33, 90);
        public LoginForm(bool startGame, AionConnection connection)
        {
            InitializeComponent();
            Size = skinSize;
            SkinSize = skinSize;

            Instance = this;
            con = connection;
            _isStartGame = startGame;
            InitControl(button_Close);
        }
        private void LoginForm_Load(object sender, EventArgs e)
        {


            //Bitmap back = KiCut((Bitmap)BackgroundImage, panel注册账号);

            //panel密码修改.BackgroundImage = back;
            //panel注册账号.BackgroundImage = back;
            //panel游戏登录.BackgroundImage = back;
            //panel密码找回.BackgroundImage = back;

            validCode = new ValidCode(4, ValidCode.CodeType.Numbers);
            if (!_isStartGame)
                ShowRegAccountPanel(null, null);
            else
                ShowLoginGamePanel(null, null);
        }


        /// <summary>
        /// 显示登陆游戏界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowLoginGamePanel(object sender, EventArgs e)
        {
            string account = AionLanucher.Properties.Settings.Default.Account;
            if (account.Contains(":"))
            {
                string[] ss = account.Split(':', ' ');
                comboBox账号.Text = ss[1];
                textBox游戏登录_密码.Text = ss[3];
                checkBox记住密码.Checked = true;
            }

            panel密码找回.Visible = false;
            panel密码修改.Visible = false;
            panel注册账号.Visible = false;
            panel游戏登录.Visible = true;
            panel游戏登录.Location = loc;
            pictureBox游戏登录.Image = Image.FromStream(validCode.CreateCheckCodeImage());
        }
        /// <summary>
        /// 显示修改密码界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowChangePswPanel(object sender, EventArgs e)
        {
            panel密码找回.Visible = false;
            panel密码修改.Visible = true;
            panel注册账号.Visible = false;
            panel游戏登录.Visible = false;
            panel密码修改.Location = loc;
            pictureBox密码修改.Image = Image.FromStream(validCode.CreateCheckCodeImage());
        }
        /// <summary>
        /// 显示注册账号界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRegAccountPanel(object sender, EventArgs e)
        {
            panel密码找回.Visible = false;
            panel密码修改.Visible = false;
            panel注册账号.Visible = true;
            panel游戏登录.Visible = false;
            panel注册账号.Location = loc;
            pictureBox注册账号.Image = Image.FromStream(validCode.CreateCheckCodeImage());
        }
        /// <summary>
        /// 显示密码找回界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFindPswPanel(object sender, EventArgs e)
        {
            panel密码找回.Visible = true;
            panel密码修改.Visible = false;
            panel注册账号.Visible = false;
            panel游戏登录.Visible = false;
            panel密码找回.Location = loc;
            pictureBox密码找回.Image = Image.FromStream(validCode.CreateCheckCodeImage());
        }

        private void pictureBox游戏登录_Click(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = Image.FromStream(validCode.CreateCheckCodeImage());
        }

        /// <summary>
        /// 登录游戏按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEx登录游戏_Click(object sender, EventArgs e)
        {
            Regex PwdRegex = new Regex(@"^[\x20-\xFF]{4,16}$");
            if (!PwdRegex.IsMatch(comboBox账号.Text))
            {
                showMSG("账号只能是4-16位的数字与字母！", true);
                comboBox账号.Focus();
                return;
            }

            if (textBox游戏登录_密码.Text.Length < 3 && textBox游戏登录_密码.Text.Length > 20)
            {
                showMSG("密码长度3 - 20位长度", true);
                textBox游戏登录_密码.Focus();
                return;
            }

            if (!textBox游戏登录_验证码.Text.Equals(validCode.CheckCode, StringComparison.CurrentCultureIgnoreCase))
            {
                showMSG("请输入正确的验证码！", true);
                textBox游戏登录_验证码.Focus();
                return;
            }

            buttonEx登录游戏.Caption = "正在验证...";
            buttonEx登录游戏.Enabled = false;
            pictureBox游戏登录.Image = Image.FromStream(validCode.CreateCheckCodeImage());
            con.SendPacket(new SM_ACCOUNT_REQUEST(0, comboBox账号.Text, textBox游戏登录_密码.Text, ""));



        }

        /// <summary>
        /// 注册账号按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEx注册账号_Click(object sender, EventArgs e)
        {
            Regex PwdRegex = new Regex(@"^[\x20-\xFF]{4,16}$");
            if (!PwdRegex.IsMatch(textBox注册账号_账号.Text))
            {
                showMSG("账号只能是4-16位的数字与字母！", true);
                textBox注册账号_账号.Focus();
                return;
            }
            if (textBox注册账号_密码.Text.Length < 3 && textBox注册账号_密码.Text.Length > 20)
            {
                showMSG("密码长度3 - 20位长度", true);
                textBox注册账号_密码.Focus();
                return;
            }


            if (!checkemail(textBox注册账号_邮箱.Text))
            {
                showMSG("邮箱只允许英文字母、数字、下划线、英文句号、以及中划线组成\r\n例如：aa_bb-001@qq.com", true);
                textBox注册账号_邮箱.Focus();
            }
            else if (!textBox账号注册_验证码.Text.Equals(validCode.CheckCode, StringComparison.CurrentCultureIgnoreCase))
            {
                showMSG("请输入正确的验证码！", true);
                textBox账号注册_验证码.Focus();
            }
            else
            {
                buttonEx注册账号.Caption = "正在注册...";
                buttonEx注册账号.Enabled = false;
                pictureBox注册账号.Image = Image.FromStream(validCode.CreateCheckCodeImage());
                con.SendPacket(new SM_ACCOUNT_REQUEST(1, textBox注册账号_账号.Text, textBox注册账号_密码.Text, textBox注册账号_邮箱.Text));
            }
        }

        /// <summary>
        /// 修改密码按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEx修改密码_Click(object sender, EventArgs e)
        {
            Regex PwdRegex = new Regex(@"^[\x20-\xFF]{4,16}$");
            if (!PwdRegex.IsMatch(textBox密码修改_账号.Text))
            {
                showMSG("账号只能是4-16位的数字与字母！", true);
                textBox密码修改_账号.Focus();
                return;
            }
            if (textBox密码修改_新密码.Text.Length < 3 && textBox密码修改_新密码.Text.Length > 20)
            {
                showMSG("密码长度3 - 20位长度", true);
                textBox密码修改_新密码.Focus();
                return;
            }



            if (textBox密码修改_原密码.Text.Length < 3)
            {
                showMSG("原始密码长度不能少于5位", true);
                textBox密码修改_原密码.Focus();
            }
            else if (!textBox密码修改_验证码.Text.Equals(validCode.CheckCode, StringComparison.CurrentCultureIgnoreCase))
            {
                showMSG("请输入正确的验证码！", true);
                textBox密码修改_验证码.Focus();
            }
            else
            {
                buttonEx修改密码.Caption = "正在修改...";
                buttonEx修改密码.Enabled = false;
                pictureBox密码修改.Image = Image.FromStream(validCode.CreateCheckCodeImage());
                con.SendPacket(new SM_ACCOUNT_REQUEST(2, textBox密码修改_账号.Text, textBox密码修改_原密码.Text, textBox密码修改_新密码.Text));
            }
        }

        /// <summary>
        /// 密码找回按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEx密码找回_Click(object sender, EventArgs e)
        {
            Regex PwdRegex = new Regex(@"^[\x20-\xFF]{4,16}$");
            if (!PwdRegex.IsMatch(textBox密码找回_账号.Text))
            {
                showMSG("账号只能是4-16位的数字与字母！", true);
                textBox密码找回_账号.Focus();
                return;
            }

            if (!checkemail(textBox密码找回_邮箱.Text))
            {
                showMSG("邮箱只允许英文字母、数字、下划线、英文句号、以及中划线组成\r\n例如：aa_bb-001@qq.com", true);
                textBox密码找回_邮箱.Focus();
            }
            else if (!textBox密码找回_验证码.Text.Equals(validCode.CheckCode, StringComparison.CurrentCultureIgnoreCase))
            {
                showMSG("请输入正确的验证码！", true);
                textBox密码找回_验证码.Focus();
            }
            else
            {
                buttonEx密码找回.Caption = "正在找回...";
                buttonEx密码找回.Enabled = false;
                pictureBox密码找回.Image = Bitmap.FromStream(validCode.CreateCheckCodeImage());
                con.SendPacket(new SM_ACCOUNT_REQUEST(3, textBox密码找回_账号.Text, "", textBox密码找回_邮箱.Text));
            }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="s">信息</param>
        /// <param name="warning">true为警告按钮,false为提醒按钮</param>
        private void showMSG(string s, bool warning)
        {
            MessageBox.Show(s, warning ? "警告" : "提醒", MessageBoxButtons.OK, warning ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        /// <summary>
        /// 检查邮件地址格式函数
        /// </summary>
        /// <param name="emailadd"></param>
        /// <returns></returns>
        private bool checkemail(string emailadd)
        {
            Regex PwdRegex = new Regex(@"^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$");
            return PwdRegex.IsMatch(emailadd);
        }


        /// <summary>
        /// 回调方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isSuccess"></param>
        /// <param name="msg"></param>
        internal void RequestOnPacket(byte type, bool isSuccess, string msg)
        {
            AionRoy.Invoke(this, new AionRoy.Handler(delegate ()
            {
                if (type == 0)
                {
                    if (!isSuccess)
                    {
                        showMSG("账号或者密码错误,请重新输入", !isSuccess);
                        buttonEx登录游戏.Enabled = true;
                        buttonEx登录游戏.Caption = "登录游戏";
                        return;
                    }
                   
                }
                else if (type == 1) //注册
                {
                    buttonEx注册账号.Enabled = true;
                    buttonEx注册账号.Caption = "注册账号";
                    showMSG(msg, !isSuccess);
                }
                else if (type == 2)//修改密码
                {
                    buttonEx修改密码.Enabled = true;
                    buttonEx修改密码.Caption = "修改密码";
                    showMSG(msg, !isSuccess);
                }
                else//找回密码
                {
                    buttonEx密码找回.Enabled = true;
                    buttonEx密码找回.Caption = "找回密码";
                    showMSG(msg, !isSuccess);
                }

                if (isSuccess)
                {
                    //如果是启动游戏界面, 玩家切换到注册账号的话,那么等注册账户完成后,不关闭窗口,而是切换到 登录界面
                    if (_isStartGame && type != 0)
                    {
                        //非登录验证账号密码状态
                        ShowLoginGamePanel(null, null);
                        return;
                    }
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
            }));
        }

        private void LoginForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) //F11
            {
                if (panel密码找回.Visible)
                {
                    buttonEx密码找回_Click(sender, null);
                }
                else if (panel密码修改.Visible)
                {
                    buttonEx修改密码_Click(sender, null);
                }
                else if (panel注册账号.Visible)
                {
                    buttonEx注册账号_Click(sender, null);
                }
                else if (panel游戏登录.Visible)
                {
                    buttonEx登录游戏_Click(sender, null);
                }
            }
        }

        private void button_Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox账号_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox游戏登录_密码.Text = userPasswors[comboBox账号.Text];
        }

        private void checkBox记住密码_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox记住密码.CheckState != CheckState.Checked)
            {
                switch (MessageBox.Show("您是否要取消记住当前账号和密码?\r\n按[是]只取消保存当前账号\r\n按[否]将清空所有列表记录", "提醒", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        break;
                    case DialogResult.No:
                        userPasswors.Clear();
                        comboBox账号.Items.Clear();
                        break;
                    default:
                        checkBox记住密码.Checked = true;
                        break;

                }
            }
        }
    }
}

using AionCommons.Unilty;
using AionNetGate.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate
{
    public partial class RegForm : Form
    {
        internal static RegForm instance = new RegForm();
        public RegForm()
        {
            InitializeComponent();
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            if (textBox注册码.Text == "" || textBox注册码.Text.Length < 10)
            {
                MessageBox.Show("请输入正确的注册码!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (textBox机器码.Text.Equals(RegHelp.KeyDecode(textBox注册码.Text.Replace("-", "+").Replace(".", "="))))
            {
                AionNetGate.Properties.Settings.Default.RegNumber = textBox注册码.Text;
                AionNetGate.Properties.Settings.Default.Save();

                MessageBox.Show("软件注册成功!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Configs.Config.isPromoted = true;
                MainForm.Instance.isRight = true;
                Close();
                Dispose();
            }
            else
            {
                MessageBox.Show("软件注册失败!", "失败",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void RegForm_Load(object sender, EventArgs e)
        {
            SystemInfo si = new SystemInfo();
            textBox机器码.Text = si.getMNum();

            if (Config.isPromoted)
            {
                textBox注册码.Text = "当前网关已注册成功，无需再次注册！";
                textBox注册码.ReadOnly = true;
                skinButton.Enabled = false;
            }

        }

        internal void readPromte()
        {
            SystemInfo si = new SystemInfo();
            if (si.getMNum().Equals(RegHelp.KeyDecode(AionNetGate.Properties.Settings.Default.RegNumber.Replace("-", "+").Replace(".", "="))))
            {
                Configs.Config.isPromoted = true;
                MainForm.Instance.isRight = true;

            }
            
        }
    }


    internal class RegHelp
    {
        //密钥
        private static byte[] arrDESKey = new byte[] { 122, 122, 193, 156, 178, 124, 218, 132 };

        private static byte[] arrDESIV = new byte[] { 185, 123, 246, 179, 146, 199, 167, 213 };
        private static byte[] En(byte[] bs, int size)
        {
            byte[] newbyte = new byte[size];

            for (int i = 0; i < size; i++)
            {
                newbyte[i] = (byte)(bs[i] ^ "晓".ToCharArray()[0]);
            }
            return newbyte;
        }


        /// <summary>
        /// 硬解码加密 生成注册码
        /// </summary>
        /// <param name="m_Need_Encode_String"></param>
        /// <returns></returns>
        internal static string KeyEncode(string m_Need_Encode_String)
        {
            if (m_Need_Encode_String == null)
            {
                throw new Exception("Error: \n源字符串为空！！");
            }
            try
            {
                using (MemoryStream objMemoryStream = new MemoryStream())
                {
                    DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();

                    using (CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateEncryptor(arrDESKey, arrDESIV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter objStreamWriter = new StreamWriter(objCryptoStream))
                        {
                            objStreamWriter.Write(m_Need_Encode_String);
                            objStreamWriter.Flush();
                            objCryptoStream.FlushFinalBlock();
                            objMemoryStream.Flush();
                            return Convert.ToBase64String(En(objMemoryStream.GetBuffer(), (int)objMemoryStream.Length));
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return "";
        }




        /// <summary>
        /// 解密。
        /// </summary>
        /// <param name="m_Need_Encode_String"></param>
        /// <returns></returns>
        internal static string KeyDecode(string m_Need_Encode_String)
        {
            if (m_Need_Encode_String == null || m_Need_Encode_String == "")
            {
                return "";
            }
            try
            {
                byte[] arrInput = Convert.FromBase64String(m_Need_Encode_String);
                arrInput = En(arrInput, arrInput.Length);

                using (MemoryStream objMemoryStream = new MemoryStream(arrInput))
                {
                    DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
                    using (CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateDecryptor(arrDESKey, arrDESIV), CryptoStreamMode.Read))
                    {
                        using (StreamReader objStreamReader = new StreamReader(objCryptoStream))
                        {
                            return objStreamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
               // Debug.Fail(e.ToString());
            }
            return "";
        }
    }
}

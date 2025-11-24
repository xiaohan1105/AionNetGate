using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using CCWin;

namespace NetGateReg
{
    public partial class Form1 : CCSkinMain
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            if(skinTextBox1.Text.Length > 10)
                skinTextBox2.Text = RegHelp.KeyEncode(skinTextBox1.Text);
            
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

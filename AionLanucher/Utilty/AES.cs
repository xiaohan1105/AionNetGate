using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AionLanucher.Utilty
{
    #region AES加密和解密类
    class AES
    {
        /// <summary>
        /// 异或方式加密
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        internal static byte[] En(byte[] bs)
        {
            byte[] newbyte = new byte[bs.Length];

            for (int i = 0; i < bs.Length; i++)
            {
                newbyte[i] = (byte)(bs[i] ^ "煌".ToCharArray()[0]);
            }
            return newbyte;
        }
        internal static byte[] En(byte[] bs, int size)
        {
            byte[] newbyte = new byte[size];

            for (int i = 0; i < size; i++)
            {
                newbyte[i] = (byte)(bs[i] ^ "煌".ToCharArray()[0]);
            }
            return newbyte;
        }
        /// <summary>
        /// 使用缺省密钥字符串解密byte[]
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        internal static byte[] Decrypt(byte[] encrypted)
        {
            byte[] key = System.Text.Encoding.UTF8.GetBytes(AES.Encode(pancher).Substring(0, 9));
            return Decrypt(encrypted, key);
        }
        /// <summary>
        /// 使用缺省密钥字符串加密
        /// </summary>
        /// <param name="original">原始数据</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        internal static byte[] Encrypt(byte[] original)
        {
            byte[] key = System.Text.Encoding.UTF8.GetBytes(AES.Encode(pancher).Substring(0, 9));
            return Encrypt(original, key);
        }


        /// <summary>
        /// 生成MD5摘要
        /// </summary>
        /// <param name="original">数据源</param>
        /// <returns>摘要</returns>
        internal static byte[] MakeMD5(byte[] original)
        {
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            byte[] keyhash = hashmd5.ComputeHash(original);
            hashmd5 = null;
            return keyhash;
        }

        /// <summary>
        /// 使用给定密钥加密
        /// </summary>
        /// <param name="original">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        internal static byte[] Encrypt(byte[] original, byte[] key)
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.Key = MakeMD5(key);
            des.Mode = CipherMode.ECB;

            return des.CreateEncryptor().TransformFinalBlock(original, 0, original.Length);
        }

        /// <summary>
        /// 使用给定密钥解密数据
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        internal static byte[] Decrypt(byte[] encrypted, byte[] key)
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.Key = MakeMD5(key);
            des.Mode = CipherMode.ECB;

            return des.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
        }

        private static string pancher = "METICSOFT";

        //密钥
        private static byte[] arrDESKey = new byte[] { 42, 16, 93, 156, 78, 4, 218, 32 };
        private static byte[] arrDESIV = new byte[] { 55, 103, 246, 79, 36, 99, 167, 3 };

        /// <summary>
        /// 加密。
        /// </summary>
        /// <param name="m_Need_Encode_String"></param>
        /// <returns></returns>
        internal static string Encode(string m_Need_Encode_String)
        {
            if (m_Need_Encode_String == null)
            {
                throw new Exception("Error: 源字符串为空！！");
            }

            DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
            using (MemoryStream objMemoryStream = new MemoryStream())
            {
                using (CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateEncryptor(arrDESKey, arrDESIV), CryptoStreamMode.Write))
                {
                    using (StreamWriter objStreamWriter = new StreamWriter(objCryptoStream))
                    {
                        objStreamWriter.Write(m_Need_Encode_String);
                        objStreamWriter.Flush();
                        objCryptoStream.FlushFinalBlock();
                        objMemoryStream.Flush();
                        m_Need_Encode_String = Convert.ToBase64String(objMemoryStream.GetBuffer(), 0, (int)objMemoryStream.Length);
                    }
                }
            }
            return m_Need_Encode_String;
        }

        /// <summary>
        /// 解密。
        /// </summary>
        /// <param name="m_Need_Encode_String"></param>
        /// <returns></returns>
        internal static string Decode(string m_Need_Encode_String)
        {
            if (m_Need_Encode_String == null)
            {
                throw new Exception("Error: 源字符串为空！！");
            }
            DESCryptoServiceProvider objDES = new DESCryptoServiceProvider();
            string DeString = "数据包解密失败";
            try
            {
                byte[] arrInput = Convert.FromBase64String(m_Need_Encode_String);
                using (MemoryStream objMemoryStream = new MemoryStream(arrInput))
                {
                    using (CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateDecryptor(arrDESKey, arrDESIV), CryptoStreamMode.Read))
                    {
                        using (StreamReader objStreamReader = new StreamReader(objCryptoStream))
                        {
                            DeString = objStreamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception de)
            {
                return de.ToString() + "待解密字符串：" + m_Need_Encode_String;
            }
            return DeString;
        }

        //获取文件的MD5码
        internal static string CretaeMD5(string fileName)
        {
            string hashStr = string.Empty;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] hash = md5.ComputeHash(fs);
                hashStr = ByteArrayToHexString(hash);
                fs.Close();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return hashStr;
        }
        //获取流的MD5码
        internal static string CretaeMD5(Stream stream)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(stream);
            return ByteArrayToHexString(hash);
        }
        //获取byte数组中指定部分的MD5码
        internal static string CretaeMD5(byte[] buffer, int offset, int count)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(buffer, offset, count);
            return ByteArrayToHexString(hash);
        }
        private static string ByteArrayToHexString(byte[] values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte value in values)
            {
                sb.AppendFormat("{0:X2}", value);
            }
            return sb.ToString();
        }


        /// <summary>
        /// 截取字符长度，以位长截取，并指定截取位长.不足指字位长,则不截取.原样返回.
        /// </summary>
        /// <param name="SourceString">被截取源字符串</param>
        /// <param name="CutLeng">指定要截取位长长度</param>
        /// <returns>返回截取后字符串</returns>
        internal static byte[] CutStringByByBytes(byte[] SourceString, int CutLeng)
        {
            int Bytes_Count = 0;
            byte[] CutStr_Bytes1 = new byte[CutLeng];
            byte[] SourceStr_Bytes = SourceString;
            Bytes_Count = SourceStr_Bytes.Length;
            if (Bytes_Count > CutLeng)
            {
                Array.Copy(SourceStr_Bytes, 0, CutStr_Bytes1, 0, CutLeng);
                return CutStr_Bytes1;
            }
            return SourceString;
        }
    }
    #endregion
}

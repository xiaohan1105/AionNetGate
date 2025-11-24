using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_REGEDIT_LIST : AbstractServerPacket
    {
        private byte type;
        private string command;

        public SM_REGEDIT_LIST(byte type, string command)
        {
            // TODO: Complete member initialization
            this.type = type;
            this.command = command;
        }
        protected override void writeImpl()
        {
            writeC(type);
            switch (type)
            {
                case 0:
                    GetRegKey();
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
            }
        }


        /// <summary>
        /// 取得注册表节点信息
        /// </summary>
        /// <param name="MyPath"></param>
        /// <returns></returns>
        private void GetRegKey()
        {
            RegistryKey MyReg = Registry.LocalMachine;
            string root = command;
            if (command.Contains("\\"))
                root = command.Split('\\')[0];
            switch (root)
            {
                case "HKEY_CLASSES_ROOT":
                    MyReg = Registry.ClassesRoot;
                    break;
                case "HKEY_CURRENT_USER":
                    MyReg = Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    MyReg = Registry.LocalMachine;
                    break;
                case "HKEY_USERS":
                    MyReg = Registry.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    MyReg = Registry.CurrentConfig;
                    break;
                default:
                    return;
            }

            if (command.IndexOf("\\") > 0)
            {
                command = command.Substring(command.IndexOf("\\") + 1);
                try
                {
                    MyReg = MyReg.OpenSubKey(command, true);
                }
                catch
                {

                }
            }
            try
            {
                string[] st = MyReg.GetSubKeyNames();
                writeUH((ushort)st.Length);
                foreach (string a in MyReg.GetSubKeyNames())
                {
                    writeS(a);
                }
                int length = 0;
                string[] Values = MyReg.GetValueNames();
                length = Values.Length;
                string MyRegValueName;
                object MyRegValueType;
                object MyRegValueData;

                writeUH((ushort)length);
                for (int i = 0; i < length; ++i)
                {
                    MyRegValueName = Values[i];
                    MyRegValueData = MyReg.GetValue(MyRegValueName);
                    MyRegValueType = MyRegValueData.GetType();
                    string value;
                    string type = getTypeD(MyRegValueType.ToString(), MyRegValueData, out value);

                    writeS(MyRegValueName + "\t" + type + "\t" + value);
                }



            }
            catch
            {

            }
        }

        private string getTypeD(string o, object v, out string va)
        {
            if (o.Contains("String[]"))
            {
                va = "";
                string[] st = (string[])v;
                foreach (string s in st)
                {
                    va += s + "\n";
                }
                return "REG_MULTI_SZ";
            }
            if (o.Contains("String"))
            {
                va = (string)v;
                return "REG_SZ";
            }
            else if (o.Contains("Int32"))
            {
                va = "0x" + ((Int32)v).ToString("X8") + " (" + ((Int32)v) + ")";
                return "REG_DWORD";
            }
            else if (o.Contains("Int64"))
            {
                va = "0x" + ((Int64)v).ToString("X16") + " (" + ((Int64)v) + ")";
                return "REG_QWORD";
            }
            else if (o.Contains("Byte[]"))
            {
                byte[] bs = (byte[])v;

                va = ToHexString(bs, true);
                return "REG_BINARY";
            }
            else
            {
                va = v.ToString();
                return o;
            }
        }

        public string ToHexString(byte[] data, bool withSpaces)
        {
            string result = "";
            foreach (byte b in data)
            {
                result += string.Format("{0:X2}", b);
                if (withSpaces) result += " ";
            }
            return result;
        }
    }
}

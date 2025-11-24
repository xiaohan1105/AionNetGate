using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace AionLanucher.Utilty
{
    class SystemInfo
    {
        /// <summary>
        /// 系统名称
        /// </summary>
        /// <returns></returns>
        internal string GetMyOSName()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_OperatingSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["Caption"].ToString();
                }
                mc = null; moc = null;
                return st;
            }
            catch
            {

            }
            return "系统未知";
        }
        /// <summary>
        /// 获取当前计算机名称
        /// </summary>
        /// <returns></returns>
        internal string GetMyComputerName()
        {
            return Environment.MachineName;
        }
        /// <summary>
        /// 获取当前用户名称
        /// </summary>
        /// <returns></returns>
        internal string GetMyUserName()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// 获取驱动器的存储空间大小
        /// </summary>
        /// <returns></returns>
        internal string GetMyDriveInfo()
        {
            try
            {
                string[] MyDrive = Environment.GetLogicalDrives();
                long s0 = 0, s1 = 0;
                foreach (string MyDriveLetter in MyDrive)
                {
                    try
                    {
                        DriveInfo MyDriveInfo = new DriveInfo(MyDriveLetter);
                        if (MyDriveInfo.DriveType == DriveType.CDRom || MyDriveInfo.DriveType == DriveType.Removable)
                            continue;
                        s0 += MyDriveInfo.TotalSize;
                        s1 += MyDriveInfo.TotalFreeSpace;
                    }
                    catch { }
                }
                return (s1 / 1000000000).ToString() + "G（空余容量）/" + (s0 / 1000000000).ToString() + "G（总容量）";
            }
            catch
            {

            }
            return "未知硬盘容量";
        }
        /// <summary>
        /// 获取当前计算机的内存信息
        /// </summary>
        /// <returns></returns>
        internal string GetMyMemoryInfo()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = (long.Parse(mo["TotalPhysicalMemory"].ToString()) / 1024 / 1024).ToString() + " M";
                }
                mc = null; moc = null;
                return st;
            }
            catch
            {

            }
            return "未知内存容量";
        }
        /// <summary>
        /// 系统类型（64还是32）
        /// </summary>
        /// <returns></returns>
        internal string GetSystemTypeInfo()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["SystemType"].ToString();
                }
                mc = null; moc = null;
                return st.Substring(0, 3);
            }
            catch
            {
                
            }
            return "未知类型";
        }

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        internal string GetMyCpuInfo()
        {
            try
            {
                RegistryKey reg = Registry.LocalMachine;
                reg = reg.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
                return reg.GetValue("ProcessorNameString").ToString();
            }
            catch
            {

            }
            return "未知CPU信息";
        }

        /// <summary>
        /// 显卡信息
        /// </summary>
        /// <returns></returns>
        internal string GetVedioCardInfo()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_VideoController");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["Caption"].ToString();
                }
                mc = null; moc = null;
                return st;
            }
            catch
            {

            }
            return "未知显卡信息";
        }
        /// <summary>
        /// 主板信息
        /// </summary>
        /// <returns></returns>
        internal string GetMainBoardInfo()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_BaseBoard");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["Manufacturer"].ToString().Trim() + " " + mo["Product"].ToString().Trim() + " " + mo["SerialNumber"].ToString();
                }
                mc = null; moc = null;
                return st;
            }
            catch
            {

            }
            return "未知主板信息";
        }


        /// <summary>
        /// 获取网卡硬件地址  
        /// </summary>
        /// <returns></returns>
        internal string GetMacAddress()
        {
            try
            {
                //获取网卡硬件地址   
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                    }
                }
                mc = null; moc = null;
                return mac;
            }
            catch
            {
                
            }
            return "未知MAC地址";
        }

        #region 获取GUID值
        /// <summary>
        /// 获取GUID值
        /// </summary>
        internal string NewGUID
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }
        #endregion

        /// <summary>
        /// 取得设备硬盘的卷标号
        /// </summary>
        /// <returns></returns>
        internal string GetDiskVolumeSerialNumber()
        {
            long number = 0;
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
                disk.Get();
                string c = disk.GetPropertyValue("VolumeSerialNumber").ToString();
                number += Convert.ToInt64(c, 16);

                disk = new ManagementObject("win32_logicaldisk.deviceid=\"d:\"");
                disk.Get();
                string d = disk.GetPropertyValue("VolumeSerialNumber").ToString();
                number += Convert.ToInt64(d, 16);

                mc = null; disk = null;
            }
            catch { }



            return number.ToString("X");
        }

        #region 获取硬盘序列号
        [DllImport("kernel32.dll")]
        private static extern int GetVolumeInformation(
        string lpRootPathName,
        string lpVolumeNameBuffer,
        int nVolumeNameSize,
        ref int lpVolumeSerialNumber,
        int lpMaximumComponentLength,
        int lpFileSystemFlags,
        string lpFileSystemNameBuffer,
        int nFileSystemNameSize
        );
        /// <summary>
        /// 获取硬盘序列号
        /// </summary>
        /// <param name="drvID">硬盘盘符[c|d|e|....]</param>
        /// <returns></returns>
        internal string GetDiskVolume(string drvID)
        {
            const int MAX_FILENAME_LEN = 256;
            int retVal = 0;
            int lpMaximumComponentLength = 0;
            int lpFileSystemFlags = 0;
            string lpVolumeNameBuffer = null;
            string lpFileSystemNameBuffer = null;
            int i = GetVolumeInformation(
            drvID + @":\",
            lpVolumeNameBuffer,
            MAX_FILENAME_LEN,
            ref retVal,
            lpMaximumComponentLength,
            lpFileSystemFlags,
            lpFileSystemNameBuffer,
            MAX_FILENAME_LEN
            );
            return retVal.ToString("x8");
        }
        #endregion

        /// <summary>
        /// 获得CPU的序列号
        /// </summary>
        /// <returns></returns>
        internal string getCpu()
        {
            try
            {
                string strCpu = null;
                ManagementClass myCpu = new ManagementClass("win32_Processor");
                ManagementObjectCollection myCpuConnection = myCpu.GetInstances();
                foreach (ManagementObject myObject in myCpuConnection)
                {
                    strCpu = myObject.Properties["Processorid"].Value.ToString();
                    break;
                }
                myCpu = null; myCpuConnection = null;
                return strCpu;
            }
            catch
            {

            }
            return "dddddddddd";
        }

        /// <summary>
        /// 生成机器码
        /// </summary>
        /// <returns></returns>
        internal string getMNum()
        {
            try
            {
                string c = GetDiskVolume("c");
                string d = GetDiskVolume("d");

                string cpuid = getCpu();
                string c1 = cpuid.Substring(0, 8);
                string c2 = cpuid.Substring(8, 8);

                string mac = GetMacAddress();
                if (mac.Contains(":"))
                    mac = mac.Replace(":", "");


                long ra = Convert.ToInt64(c1, 16)
                    + Convert.ToInt64(c2, 16)
                     + Convert.ToInt64(c, 16)
                      + Convert.ToInt64(d, 16)
                    + Convert.ToInt64(mac, 16);

                return ra.ToString("X10");
            }
            catch
            {
                string s = NewGUID;
                if (s.Contains("-"))
                    s = s.Replace("-", "");
                return "GUID" + s.Substring(0, 10).ToUpper();
            }
        }
    }
}

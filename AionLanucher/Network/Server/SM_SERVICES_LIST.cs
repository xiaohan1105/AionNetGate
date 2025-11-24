using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_SERVICES_LIST : AbstractServerPacket
    {
        private byte type;
        private string _info;
        public SM_SERVICES_LIST(byte b, string info)
        {
            type = b;
            _info = info;
        }
        protected override void writeImpl()
        {
            writeC(type);
            switch (type)
            {
                case 0:
                    ShowServicesList();
                    break;
                case 1:
                    StartService();
                    break;
                case 2:
                    StopService();
                    break;
                case 3:
                    setStartMode();
                    break;
            }
        }

        private void ShowServicesList()
        {
            List<string> lists = new List<string>();

            ServiceController[] ArraySrvCtrl = ServiceController.GetServices();
            foreach (ServiceController tempSC in ArraySrvCtrl)
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + tempSC.ServiceName);
                string description = "";
                string runMode = "";
                string filePath = "";
                string state = "";
                try
                {
                    int i = (int)reg.GetValue("Start");
                    runMode = getRunMode(i);
                    filePath = reg.GetValue("ImagePath").ToString();
                    description = reg.GetValue("Description").ToString();
                }
                catch
                {
                }
                switch (tempSC.Status)
                {
                    case ServiceControllerStatus.Running:
                        state = "正在运行";
                        break;
                    case ServiceControllerStatus.Stopped:
                        state = "已停止";
                        break;
                    case ServiceControllerStatus.Paused:
                        state = "已暂停";
                        break;
                }

                lists.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", tempSC.DisplayName, description, state, runMode, filePath, tempSC.ServiceName));
            }

            writeH((short)lists.Count);
            foreach (string s in lists)
            {
                writeS(s);
            }
            lists.Clear();
            lists = null;

        }
        private string getRunMode(int i)
        {
            string runMode;
            if (i == 2)
                runMode = "自动";
            else if (i == 3)
                runMode = "手动";
            else if (i == 4)
                runMode = "禁用";
            else
                runMode = "未知";
            return runMode;
        }

        private void StartService()
        {
            ServiceController tempSC = new ServiceController(_info);
            try
            {
                tempSC.Start();
                writeS("[ " + _info + " ]已成功启动");
            }
            catch (Exception e)
            {
                writeS("[ " + _info + " ]启动失败：" + e.Message);
            }
        }
        private void StopService()
        {
            ServiceController tempSC = new ServiceController(_info);
            try
            {
                tempSC.Stop();
                writeS("[ " + _info + " ]已成功停止");
            }
            catch (Exception e)
            {
                writeS("[ " + _info + " ]停止失败：" + e.Message);
            }

        }

        private void setStartMode()
        {
            string[] s = _info.Split('\t');
            ServiceController tempSC = new ServiceController(s[0]);
            try
            {
                int i = int.Parse(s[1]);
                string mode = getRunMode(i);
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + tempSC.ServiceName, true);
                reg.SetValue("Start", i);
                writeS("[ " + s[0] + " ]已成功更改启动模式为[" + mode + "]");
            }
            catch (Exception e)
            {
                writeS("[ " + s[0] + " ]更改启动模式失败：" + e.Message);
            }
        }
    }
}

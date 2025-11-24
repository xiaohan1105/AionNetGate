using AionLanucher.Configs;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AionLanucher.Services
{
    class FileCheckService
    {

        private static List<DirWatchService> dirWatchs = new List<DirWatchService>();

        internal static void Stop()
        {
            foreach (DirWatchService dws in dirWatchs)
            {
                dws.Stop();
            }
            dirWatchs.Clear();
        }

        internal static void Start()
        {
            foreach (DirWatchService dws in dirWatchs)
            {
                dws.Stop();
            }
            dirWatchs.Clear();


            ThreadPool.QueueUserWorkItem(new WaitCallback(checkFiles));
            ThreadPool.QueueUserWorkItem(new WaitCallback(chechFileMD5));
        }

        /// <summary>
        /// 检查客户端目录
        /// </summary>
        /// <returns></returns>
        internal static void checkFiles(object state)
        {
            if (Config.CLIENT_FILES == null || Config.CLIENT_FILES.Length == 0)
                return;

            try
            {
                foreach (string d in Config.CLIENT_FILES)
                {
                    string dir = d.ToLower();
                    if (dir.Contains("|"))
                    {
                        string[] ss = dir.Split('|');
                        string filePath = AionLanucher.Properties.Settings.Default.GamePath + "\\" + ss[0];
                        if (!Directory.Exists(filePath))
                            continue;

                        //获得网关上的目录限制文件名，存放与LIST中
                        List<string> keepfile = new List<string>();
                        if (ss[1].Contains(";"))
                            keepfile.AddRange(ss[1].Split(';'));
                        else
                            keepfile.Add(ss[1]);

                        string[] clientfiles = Directory.GetFiles(filePath, "*.*");//获取客户端对应目录下所有文件名
                        foreach (string f in clientfiles)
                        {
                            string name = Path.GetFileName(f).ToLower();
                            if (!keepfile.Contains(name))
                            {
                                try
                                {
                                    File.Delete(f);//发现多余文件，立刻删除
                                }
                                catch (Exception)
                                {
                                    MainForm.Instance.ClosAionGame();
                                    MessageBox.Show(Language.getLang("无法清除多余文件：") + f, "文件检查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }

                        DirWatchService dw = new DirWatchService(filePath);
                        dw.Start();
                        dirWatchs.Add(dw);

                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "文件检查", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// 检测客户端指定文件MD5值
        /// </summary>
        internal static void chechFileMD5(object state)
        {
            if (Config.CLIENT_FILES_MD5 == null || Config.CLIENT_FILES_MD5.Length == 0)
                return;

            foreach (string f in Config.CLIENT_FILES_MD5)
            {
                if (!f.Contains("|"))
                    continue;
                string[] fs = f.Split('|');
                string cf = AionLanucher.Properties.Settings.Default.GamePath + "\\" + fs[0];
                if (File.Exists(cf))//先判断文件是否存在，然后对比MD5是否一致
                {
                    string md5 = AES.CretaeMD5(cf);
                    if (!md5.Equals(fs[1].ToUpper()))
                    {
                        MainForm.Instance.ClosAionGame();
                        MessageBox.Show(Language.getLang("检查到") + fs[0] + Language.getLang("文件服务端MD5[") + fs[1] + Language.getLang("]与客户端MD5[") + md5 + Language.getLang("]不对，终止游戏！"), Language.getLang("警告"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    }
                }
                else
                {
                    MainForm.Instance.ClosAionGame();
                    MessageBox.Show(Language.getLang("检查到") + cf + Language.getLang("文件不存在，终止游戏！\r\n请恢复该文件后再启动游戏！"), Language.getLang("警告"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }
            }
        }

    }
}

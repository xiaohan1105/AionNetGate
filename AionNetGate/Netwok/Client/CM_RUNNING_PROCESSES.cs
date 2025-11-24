using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Netwok.Client
{
    class CM_RUNNING_PROCESSES : AbstractClientPacket
    {
        private string[] processInfo;
        private Image[] imgs;
        protected override void readImpl()
        {
            AionConnection ac = (AionConnection)getConnection();
            if (readC() == 0)
            {
                int size = readH();

                // 修复：检查进程数量的合理性
                if (size < 0 || size > 1000)
                {
                    System.Diagnostics.Debug.WriteLine("无效的进程数量: " + size);
                    return;
                }

                processInfo = new string[size];
                imgs = new Image[size];
                for (int i = 0; i < size; i++)
                {
                    int le = readUH();
                    if (le > 0 && le < 100000) // 修复：检查图标数据大小
                    {
                        byte[] bs = readB(le);
                        Image img = byteArrayToImage(bs);
                        imgs[i] = img ?? new Bitmap(16, 16); // 修复：使用默认图标如果转换失败
                    }
                    else
                    {
                        imgs[i] = new Bitmap(16, 16);
                    }
                    processInfo[i] = readS();
                }

                // 修复：检查processForm是否可用
                if (ac.processForm != null && !ac.processForm.IsDisposed)
                {
                    try
                    {
                        ac.processForm.ShowProcessList(processInfo, imgs);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("显示进程列表失败: " + ex.Message);
                    }
                }
            }
            else
            {
                if (readC() == 1) //结束进程成功 ，从list里面移除
                {
                    string message = readS();

                    // 修复：安全地解析PID
                    try
                    {
                        string[] parts = message.Split(new char[] { '[', ']' });
                        if (parts.Length > 1)
                        {
                            string pid = parts[1];

                            // 修复：检查processForm是否可用
                            if (ac.processForm != null && !ac.processForm.IsDisposed)
                            {
                                ac.processForm.RemoveFromListByPid(pid);
                            }

                            MessageBox.Show(message, "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("解析进程结束消息失败: " + ex.Message);
                        MessageBox.Show(message, "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    string errorMessage = readS();
                    MessageBox.Show(errorMessage, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        protected override void runImpl()
        {

        }


        /// <summary>
        /// byte[]转换成Image
        /// </summary>
        /// <param name="byteArrayIn">二进制图片流</param>
        /// <returns>Image</returns>
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    // 修复：验证流的有效性
                    if (ms.Length == 0)
                        return null;

                    Image returnImage = Image.FromStream(ms);
                    return returnImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("进程图标转换失败: " + ex.Message);
                return null;
            }
        }
    }
}

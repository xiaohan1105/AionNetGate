using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AionNetGate.Netwok.Client
{
    public class CM_EXPLORER_PC : AbstractClientPacket
    {
        public CM_EXPLORER_PC()
        {
        }

        private void CopyFileOrDir()
        {
            MessageBox.Show(base.readS(), "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void DeleteFileOrDir()
        {
            MessageBox.Show(base.readS(), "提醒", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        protected override void readImpl()
        {
            switch (readC())
            {
                case 0:
                    ShowDrives();
                    break;

                case 1:
                    ShowFileOrDir();
                    break;

                case 2:
                    CopyFileOrDir();
                    break;

                case 3:
                    DeleteFileOrDir();
                    break;
            }
        }

        protected override void runImpl()
        {
        }

        private void ShowDrives()
        {
            int num = base.readH();

            // 修复：检查驱动器数量的合理性
            if (num < 0 || num > 26) // 最多26个驱动器字母
            {
                System.Diagnostics.Debug.WriteLine("无效的驱动器数量: " + num);
                return;
            }

            string[] drives = new string[num];
            for (int i = 0; i < num; i++)
            {
                drives[i] = readS();
            }

            // 修复：检查explorerForm是否可用
            AionConnection ac = (AionConnection)getConnection();
            if (ac.explorerForm != null && !ac.explorerForm.IsDisposed)
            {
                try
                {
                    ac.explorerForm.showDirAndFiles(drives, null, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("显示驱动器失败: " + ex.Message);
                }
            }
        }

        private void ShowFileOrDir()
        {
            int num = base.readUH();

            // 修复：检查目录数量的合理性
            if (num < 0 || num > 10000)
            {
                System.Diagnostics.Debug.WriteLine("无效的目录数量: " + num);
                return;
            }

            string[] dirs = null;
            if (num > 0)
            {
                dirs = new string[num];
                for (int i = 0; i < num; i++)
                {
                    dirs[i] = readS();
                }
            }

            int num3 = base.readUH();

            // 修复：检查文件数量的合理性
            if (num3 < 0 || num3 > 10000)
            {
                System.Diagnostics.Debug.WriteLine("无效的文件数量: " + num3);
                return;
            }

            string[] files = null;
            if (num3 > 0)
            {
                files = new string[num3];
                for (int j = 0; j < num3; j++)
                {
                    files[j] = readS();
                }
            }

            // 修复：检查explorerForm是否可用
            AionConnection ac = (AionConnection)getConnection();
            if (ac.explorerForm != null && !ac.explorerForm.IsDisposed)
            {
                try
                {
                    ac.explorerForm.showDirAndFiles(null, dirs, files);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("显示文件和目录失败: " + ex.Message);
                }
            }
        }
    }
}

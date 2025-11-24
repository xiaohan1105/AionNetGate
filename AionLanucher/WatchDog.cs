using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher
{
    public partial class WatchDog : Form
    {
        public WatchDog(string name, IntPtr hwnd)
        {
            InitializeComponent();
            this.Text = name;


            int pid;
            WinAPI.GetWindowThreadProcessId(hwnd, out pid);
            if (pid > 0)
            {
                Process launcher = Process.GetProcessById(pid);
                launcher.EnableRaisingEvents = true;
                launcher.Exited += launcher_Exited;
            }

            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        private void launcher_Exited(object sender, EventArgs e)
        {
            try
            {
                Process[] process = Process.GetProcessesByName("aion.bin");
                foreach (Process p in process)
                {
                    p.Kill();
                }
            }
            catch
            {

            }
            Environment.Exit(0);
        }

        private void WatchDog_SizeChanged(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }


    }
}

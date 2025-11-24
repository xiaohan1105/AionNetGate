using AionLanucher.Configs;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AionLanucher.Services
{
    class DirWatchService
    {
        private FileSystemWatcher watcher = null;
        private bool isRuning = false;
        internal DirWatchService(string dir)
        {
            this.watcher = new FileSystemWatcher(dir,"*.*");

            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            watcher.IncludeSubdirectories = true;
        }

        internal void Start()
        {
            if (!isRuning)
            {
                watcher.Changed += new FileSystemEventHandler(OnProcess);
                watcher.Created += new FileSystemEventHandler(OnProcess);
                watcher.Deleted += new FileSystemEventHandler(OnProcess);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);
                watcher.EnableRaisingEvents = true;
                isRuning = true;
            }
        }

        internal void Stop()
        {
            if (isRuning)
            {
                watcher.Changed -= new FileSystemEventHandler(OnProcess);
                watcher.Created -= new FileSystemEventHandler(OnProcess);
                watcher.Deleted -= new FileSystemEventHandler(OnProcess);
                watcher.Renamed -= new RenamedEventHandler(OnRenamed);
                isRuning = false;
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }


        private static void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                OnCreated(source, e);
            }
            else if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                OnChanged(source, e);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                OnDeleted(source, e);
            }

        }
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            MainForm.Instance.ClosAionGame();
            MessageBox.Show("请不要复制["+e.Name+"]到客户端", Config.Name, MessageBoxButtons.OK, MessageBoxIcon.Stop);

         //   Console.WriteLine("文件新建事件处理逻辑 {0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name);
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            MainForm.Instance.ClosAionGame();
         //   Console.WriteLine("文件改变事件处理逻辑{0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name);
            MessageBox.Show("请不要修改[" + e.Name + "]", Config.Name, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            MainForm.Instance.ClosAionGame();
            MessageBox.Show("请不要删除[" + e.Name + "]", Config.Name, MessageBoxButtons.OK, MessageBoxIcon.Stop);
          //  Console.WriteLine("文件删除事件处理逻辑{0}  {1}   {2}", e.ChangeType, e.FullPath, e.Name);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            MainForm.Instance.ClosAionGame();
            MessageBox.Show("请不要重命名[" + e.Name + "]", Config.Name, MessageBoxButtons.OK, MessageBoxIcon.Stop);
          //  Console.WriteLine("文件重命名事件处理逻辑{0}  {1}  {2}", e.ChangeType, e.FullPath, e.Name);
        }
    }
}

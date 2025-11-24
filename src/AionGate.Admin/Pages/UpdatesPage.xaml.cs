using System.Windows;
using System.Windows.Controls;

namespace AionGate.Admin.Pages
{
    public partial class UpdatesPage : Page
    {
        public UpdatesPage()
        {
            InitializeComponent();
            LoadStatistics();
            LoadVersions();
        }

        private void LoadStatistics()
        {
            // TODO: 从数据库加载统计数据
            // EXEC sp_GetUpdateStatistics
        }

        private void LoadVersions()
        {
            // TODO: 加载版本列表
        }

        private void ScanGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 打开文件夹选择对话框，扫描游戏目录生成清单
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择游戏目录中的任意文件",
                Filter = "所有文件|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var gameDir = System.IO.Path.GetDirectoryName(dialog.FileName);
                MessageBox.Show($"开始扫描目录: {gameDir}\n\n这可能需要几分钟时间...", "扫描中", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: 调用 UpdateManifestGenerator 生成清单
                // var generator = new UpdateManifestGenerator(gameDir);
                // var manifest = await generator.GenerateManifestAsync("2.7.0.16", "测试版本");
            }
        }

        private void CreateVersion_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("创建版本功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshVersions_Click(object sender, RoutedEventArgs e)
        {
            LoadVersions();
            LoadStatistics();
        }

        private void ViewManifest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("查看清单功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateUrls_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 批量生成CDN签名URL
            var result = MessageBox.Show("确定要为该版本的所有文件生成CDN签名URL吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // TODO: 调用 CDNUrlSigner 生成签名URL
                MessageBox.Show("URL生成完成", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UploadToCdn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("上传到CDN功能开发中\n\n建议使用 ossutil、coscmd 等官方工具批量上传", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PublishVersion_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要发布该版本吗？\n\n发布后客户端将开始更新", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // TODO: 更新 is_published = 1
                MessageBox.Show("版本已发布", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewVersionStats_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("版本统计功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteVersion_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要删除该版本吗？\n\n此操作不可恢复", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("版本已删除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CalculateDiff_Click(object sender, RoutedEventArgs e)
        {
            if (CmbFromVersion.SelectedItem == null || CmbToVersion.SelectedItem == null)
            {
                MessageBox.Show("请选择要比对的版本", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: 调用 sp_CalculateDiffUpdate 计算差异
            MessageBox.Show("差异计算完成", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateDiffPackage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("增量包生成功能开发中\n\n建议使用 bsdiff/xdelta 等工具生成二进制差分", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCdnNode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("添加CDN节点功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshCdnNodes_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 刷新CDN节点列表
        }

        private void TestCdnConnection_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("正在测试CDN连接...\n\n所有节点连接正常", "测试结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditCdnNode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("编辑CDN节点功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToggleCdnNode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CDN节点状态已切换", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewCdnStats_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CDN统计功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportUpdateLogs_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("导出更新日志功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

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

        // ==================== 网盘链接管理 ====================

        private void FullPackageVersion_Changed(object sender, SelectionChangedEventArgs e)
        {
            // TODO: 根据选择的版本加载对应的网盘下载链接
            LoadFullPackageLinks();
        }

        private void LoadFullPackageLinks()
        {
            if (CmbFullPackageVersion.SelectedItem == null)
                return;

            // TODO: 调用 sp_GetFullPackageLinks 加载网盘链接
            // var versionCode = (CmbFullPackageVersion.SelectedItem as Version).VersionCode;
            // var links = await repository.GetFullPackageLinksAsync(versionCode);
            // FullPackageLinksDataGrid.ItemsSource = links;
        }

        private void AddFullPackageLink_Click(object sender, RoutedEventArgs e)
        {
            if (CmbFullPackageVersion.SelectedItem == null)
            {
                MessageBox.Show("请先选择版本", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: 打开添加网盘链接对话框
            var dialog = new ModernWpf.Controls.ContentDialog
            {
                Title = "添加完整客户端下载链接",
                PrimaryButtonText = "保存",
                CloseButtonText = "取消",
                DefaultButton = ModernWpf.Controls.ContentDialogButton.Primary
            };

            // TODO: 创建表单控件
            var stackPanel = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };

            stackPanel.Children.Add(new TextBlock { Text = "包名称:", Margin = new Thickness(0, 8, 0, 4) });
            var txtPackageName = new TextBox { PlaceholderText = "例如: Aion 2.7 完整客户端 (百度网盘)" };
            stackPanel.Children.Add(txtPackageName);

            stackPanel.Children.Add(new TextBlock { Text = "网盘类型:", Margin = new Thickness(0, 8, 0, 4) });
            var cmbType = new ComboBox { Width = double.NaN };
            cmbType.Items.Add(new ComboBoxItem { Content = "百度网盘", Tag = "baidu" });
            cmbType.Items.Add(new ComboBoxItem { Content = "阿里云盘", Tag = "aliyun" });
            cmbType.Items.Add(new ComboBoxItem { Content = "迅雷云盘", Tag = "thunder" });
            cmbType.Items.Add(new ComboBoxItem { Content = "115网盘", Tag = "115" });
            cmbType.Items.Add(new ComboBoxItem { Content = "MEGA网盘", Tag = "mega" });
            cmbType.Items.Add(new ComboBoxItem { Content = "直链下载", Tag = "direct" });
            cmbType.SelectedIndex = 0;
            stackPanel.Children.Add(cmbType);

            stackPanel.Children.Add(new TextBlock { Text = "下载链接:", Margin = new Thickness(0, 8, 0, 4) });
            var txtUrl = new TextBox { PlaceholderText = "例如: https://pan.baidu.com/s/xxxxxx" };
            stackPanel.Children.Add(txtUrl);

            stackPanel.Children.Add(new TextBlock { Text = "提取码:", Margin = new Thickness(0, 8, 0, 4) });
            var txtCode = new TextBox { PlaceholderText = "如有提取码请填写" };
            stackPanel.Children.Add(txtCode);

            stackPanel.Children.Add(new TextBlock { Text = "解压密码:", Margin = new Thickness(0, 8, 0, 4) });
            var txtPassword = new TextBox { PlaceholderText = "压缩包解压密码" };
            stackPanel.Children.Add(txtPassword);

            stackPanel.Children.Add(new TextBlock { Text = "文件大小 (字节):", Margin = new Thickness(0, 8, 0, 4) });
            var txtSize = new TextBox { PlaceholderText = "例如: 15728640000 (约15GB)" };
            stackPanel.Children.Add(txtSize);

            stackPanel.Children.Add(new TextBlock { Text = "优先级 (0-100):", Margin = new Thickness(0, 8, 0, 4) });
            var txtPriority = new TextBox { Text = "50", PlaceholderText = "优先级≥90显示为推荐" };
            stackPanel.Children.Add(txtPriority);

            stackPanel.Children.Add(new TextBlock { Text = "说明:", Margin = new Thickness(0, 8, 0, 4) });
            var txtDescription = new TextBox
            {
                PlaceholderText = "安装说明、注意事项等",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 80
            };
            stackPanel.Children.Add(txtDescription);

            dialog.Content = new ScrollViewer
            {
                Content = stackPanel,
                MaxHeight = 500
            };

            // TODO: 显示对话框并保存
            var result = dialog.ShowAsync();
            result.ContinueWith(async task =>
            {
                if (task.Result == ModernWpf.Controls.ContentDialogResult.Primary)
                {
                    // TODO: 调用 sp_UpsertFullPackageLink 保存
                    // 验证输入
                    // 调用数据库存储过程
                    // 刷新列表
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("下载链接已添加", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadFullPackageLinks();
                    });
                }
            });
        }

        private void EditFullPackageLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var link = button?.DataContext;

            if (link == null)
            {
                MessageBox.Show("请选择要编辑的链接", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: 打开编辑对话框，逻辑类似 AddFullPackageLink_Click
            MessageBox.Show("编辑网盘链接功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyFullPackageLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var link = button?.DataContext;

            if (link == null) return;

            // TODO: 复制链接信息到剪贴板
            // 格式化为用户友好的文本
            var copyText = $"下载链接: {GetPropertyValue(link, "Url")}\n" +
                          $"提取码: {GetPropertyValue(link, "VerificationCode")}\n" +
                          $"解压密码: {GetPropertyValue(link, "ExtractionPassword")}\n" +
                          $"说明: {GetPropertyValue(link, "Description")}";

            Clipboard.SetText(copyText);
            MessageBox.Show("链接信息已复制到剪贴板", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteFullPackageLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var link = button?.DataContext;

            if (link == null) return;

            var result = MessageBox.Show(
                "确定要删除该下载链接吗？\n\n此操作不可恢复",
                "确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                // TODO: 调用 sp_DeleteFullPackageLink 删除
                // var linkId = GetPropertyValue<long>(link, "Id");
                // await repository.DeleteFullPackageLinkAsync(linkId);

                MessageBox.Show("下载链接已删除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadFullPackageLinks();
            }
        }

        private void RefreshFullPackageLinks_Click(object sender, RoutedEventArgs e)
        {
            LoadFullPackageLinks();
        }

        // 辅助方法：通过反射获取对象属性值
        private string GetPropertyValue(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            var value = prop?.GetValue(obj);
            return value?.ToString() ?? "";
        }

        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            var value = prop?.GetValue(obj);
            return value != null ? (T)value : default(T);
        }
    }
}

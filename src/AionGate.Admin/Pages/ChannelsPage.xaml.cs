using System.Windows;
using System.Windows.Controls;

namespace AionGate.Admin.Pages
{
    public partial class ChannelsPage : Page
    {
        public ChannelsPage()
        {
            InitializeComponent();
            LoadChannels();
            LoadStatistics();
        }

        private async void LoadChannels()
        {
            // TODO: 从数据库加载渠道列表
            // EXEC sp_GetChannelStatistics
        }

        private async void LoadStatistics()
        {
            // TODO: 加载统计数据
            TxtTotalChannels.Text = "0";
            TxtTotalUsers.Text = "0";
            TxtTotalRevenue.Text = "¥0.00";
            TxtUnpaidCommission.Text = "¥0.00";
        }

        private void AddChannel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 打开添加渠道对话框
            MessageBox.Show("添加渠道功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshChannels_Click(object sender, RoutedEventArgs e)
        {
            LoadChannels();
            LoadStatistics();
        }

        private void ViewChannelReport_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 显示渠道统计报表
            MessageBox.Show("统计报表功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditChannel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 编辑渠道信息
            MessageBox.Show("编辑渠道功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewChannelStats_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 查看渠道详细统计
            MessageBox.Show("渠道统计功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SettleChannel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 生成渠道结算单
            var result = MessageBox.Show("确定要为该渠道生成结算单吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // EXEC sp_GenerateChannelSettlement
                MessageBox.Show("结算单已生成", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ManageLinks_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 管理推广链接
            MessageBox.Show("推广链接管理功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchSettlements_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 查询结算记录
        }

        private void BatchSettle_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 批量结算
            var result = MessageBox.Show("确定要批量生成结算单吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("批量结算完成", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AuditSettlement_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 审核结算单
            var result = MessageBox.Show("确定审核通过该结算单吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("审核完成", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PaySettlement_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 标记结算单为已支付
            var result = MessageBox.Show("确定已完成支付吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("已标记为已支付", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewSettlementDetail_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 查看结算单详情
            MessageBox.Show("结算详情功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportSettlement_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 导出结算单为Excel
            MessageBox.Show("导出功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateLink_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 生成推广链接
            if (CmbLinkChannel.SelectedItem == null)
            {
                MessageBox.Show("请先选择渠道", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show("推广链接已生成", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshLinks_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 刷新推广链接列表
        }

        private void CopyLink_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 复制推广链接到剪贴板
            Clipboard.SetText("https://example.com/download?ref=XXXXX");
            MessageBox.Show("链接已复制到剪贴板", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateQRCode_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 生成推广链接二维码
            MessageBox.Show("二维码生成功能开发中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

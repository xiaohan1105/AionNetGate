using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using AionGate.Admin.Pages;

namespace AionGate.Admin;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 默认选中仪表板
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(ModernWpf.Controls.NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            NavigateToPage(tag);
        }
    }

    private void NavigateToPage(string? pageTag)
    {
        Type? pageType = pageTag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Players" => typeof(PlayersPage),
            "Shop" => typeof(ShopManagementPage),
            "Announcements" => typeof(AnnouncementsPage),
            "Channels" => typeof(ChannelsPage),
            "Updates" => typeof(UpdatesPage),
            "Statistics" => typeof(StatisticsPage),
            "AntiCheat" => typeof(AntiCheatPage),
            "Tools" => typeof(ToolsPage),
            _ => null
        };

        if (pageType != null)
        {
            ContentFrame.Navigate(Activator.CreateInstance(pageType));
        }
    }

    private async void RefreshAll_Click(object sender, RoutedEventArgs e)
    {
        // 刷新当前页面
        if (ContentFrame.Content is DashboardPage dashboard)
        {
            await dashboard.RefreshAsync();
        }
    }

    private async void SendAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "发送全服公告",
            PrimaryButtonText = "发送",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var textBox = new TextBox
        {
            PlaceholderText = "输入公告内容...",
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            Height = 150,
            Margin = new Thickness(0, 12, 0, 0)
        };

        dialog.Content = textBox;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            // TODO: 发送公告
            MessageBox.Show($"公告已发送: {textBox.Text}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("设置功能开发中...", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

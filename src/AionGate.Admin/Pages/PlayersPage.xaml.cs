using System.Windows;
using System.Windows.Controls;

namespace AionGate.Admin.Pages;

public partial class PlayersPage : Page
{
    public PlayersPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadPlayersAsync();
    }

    private async Task LoadPlayersAsync()
    {
        // TODO: 加载玩家数据
        await Task.Delay(100);
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("搜索功能开发中", "提示");
    }

    private void BatchSendItems_Click(object sender, RoutedEventArgs e) { }
    private void BatchSendPoints_Click(object sender, RoutedEventArgs e) { }
    private void BatchSendMail_Click(object sender, RoutedEventArgs e) { }
    private void ViewDetails_Click(object sender, RoutedEventArgs e) { }
    private void KickPlayer_Click(object sender, RoutedEventArgs e) { }
    private void BanPlayer_Click(object sender, RoutedEventArgs e) { }
    private void SendItem_Click(object sender, RoutedEventArgs e) { }
    private void SendPoints_Click(object sender, RoutedEventArgs e) { }
}

using AionNetGate.Admin.WPF.ViewModels;
using AionNetGate.Admin.WPF.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace AionNetGate.Admin.WPF.Views;

/// <summary>
/// 主窗口 - 使用 WPF UI FluentWindow
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow(
        MainWindowViewModel viewModel,
        ISnackbarService snackbarService,
        ClientsPage clientsPage,
        LauncherConfigPage launcherConfigPage,
        ServerConfigPage serverConfigPage,
        DatabaseConfigPage databaseConfigPage,
        EmailConfigPage emailConfigPage,
        LegionStatsPage legionStatsPage)
    {
        InitializeComponent();
        DataContext = viewModel;

        // 设置 Snackbar 服务的呈现器
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        // 设置各标签页内容 (使用 Frame.Navigate 来承载 Page)
        ClientsPageHost.Navigate(clientsPage);
        LauncherConfigPageHost.Navigate(launcherConfigPage);
        ServerConfigPageHost.Navigate(serverConfigPage);
        DatabaseConfigPageHost.Navigate(databaseConfigPage);
        EmailConfigPageHost.Navigate(emailConfigPage);
        LegionStatsPageHost.Navigate(legionStatsPage);
    }
}

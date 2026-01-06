using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views.Pages;

/// <summary>
/// 服务器配置页面
/// </summary>
public partial class ServerConfigPage : Page
{
    public ServerConfigPage(ServerConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

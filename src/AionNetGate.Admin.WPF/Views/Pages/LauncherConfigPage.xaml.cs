using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views.Pages;

/// <summary>
/// 登录器配置页面
/// </summary>
public partial class LauncherConfigPage : Page
{
    public LauncherConfigPage(LauncherConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

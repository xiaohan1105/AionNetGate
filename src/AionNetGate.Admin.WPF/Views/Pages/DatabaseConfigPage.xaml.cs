using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views.Pages;

/// <summary>
/// 数据库配置页面
/// </summary>
public partial class DatabaseConfigPage : Page
{
    public DatabaseConfigPage(DatabaseConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

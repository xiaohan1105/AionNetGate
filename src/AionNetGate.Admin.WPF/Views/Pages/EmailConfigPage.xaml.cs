using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views.Pages;

/// <summary>
/// 邮件通知配置页面
/// </summary>
public partial class EmailConfigPage : Page
{
    public EmailConfigPage(EmailConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

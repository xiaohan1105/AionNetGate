using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views.Pages;

/// <summary>
/// 客户端管理页面
/// </summary>
public partial class ClientsPage : Page
{
    public ClientsPage(ClientsPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // 页面加载时加载客户端列表
        Loaded += async (s, e) => await viewModel.LoadClientsCommand.ExecuteAsync(null);
    }
}

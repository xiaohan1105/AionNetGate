using System.Windows;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views;

/// <summary>
/// 远程桌面窗口
/// </summary>
public partial class RemoteDesktopWindow : Window
{
    private readonly RemoteDesktopViewModel _viewModel;

    public RemoteDesktopWindow(RemoteDesktopViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public void Initialize(string connectionId, string clientInfo)
    {
        _viewModel.Initialize(connectionId, clientInfo);
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.Cleanup();
        base.OnClosed(e);
    }
}

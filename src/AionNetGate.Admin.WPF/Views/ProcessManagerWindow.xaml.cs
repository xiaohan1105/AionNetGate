using System.Windows;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views;

/// <summary>
/// 进程管理窗口
/// </summary>
public partial class ProcessManagerWindow : Window
{
    private readonly ProcessManagerViewModel _viewModel;

    public ProcessManagerWindow(ProcessManagerViewModel viewModel)
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

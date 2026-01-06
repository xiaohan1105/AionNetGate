using System.Windows;
using System.Windows.Input;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views;

/// <summary>
/// 文件浏览器窗口
/// </summary>
public partial class FileBrowserWindow : Window
{
    private readonly FileBrowserViewModel _viewModel;

    public FileBrowserWindow(FileBrowserViewModel viewModel)
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

    private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // 双击导航到目录或打开文件
        if (_viewModel.NavigateCommand.CanExecute(null))
        {
            await _viewModel.NavigateCommand.ExecuteAsync(null);
        }
    }
}

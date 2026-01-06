using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views;

/// <summary>
/// LauncherGeneratorView.xaml 的交互逻辑
/// </summary>
public partial class LauncherGeneratorView : UserControl
{
    public LauncherGeneratorView(LauncherGeneratorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

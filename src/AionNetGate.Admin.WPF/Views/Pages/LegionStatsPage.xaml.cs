using System.Windows.Controls;
using AionNetGate.Admin.WPF.ViewModels;

namespace AionNetGate.Admin.WPF.Views.Pages;

/// <summary>
/// 军团统计页面
/// </summary>
public partial class LegionStatsPage : Page
{
    public LegionStatsPage(LegionStatsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

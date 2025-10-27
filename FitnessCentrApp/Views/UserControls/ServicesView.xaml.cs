using FitnessCentrApp.ViewModels;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls;

/// <summary>
/// Логика взаимодействия для ServicesView.xaml
/// </summary>
public partial class ServicesView : UserControl
{
    private ServicesViewModel viewModel = new();
    public ServicesView()
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}

using FitnessCentrApp.ViewModels;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls;

/// <summary>
/// Логика взаимодействия для ClientsView.xaml
/// </summary>
public partial class ClientsView : UserControl
{
    private ClientsViewModel viewModel = new();
    public ClientsView()
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}

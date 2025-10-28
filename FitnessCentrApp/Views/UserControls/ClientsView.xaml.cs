using FitnessCentrApp.ViewModels;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls;

/// <summary>
/// Логика взаимодействия для ClientsView.xaml
/// </summary>
public partial class ClientsView : UserControl
{
    public ClientsView()
    {
        InitializeComponent();

        DataContext = new ClientsViewModel();
    }
}

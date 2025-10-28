using FitnessCentrApp.ViewModels;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls;

/// <summary>
/// Логика взаимодействия для ServicesView.xaml
/// </summary>
public partial class ServicesView : UserControl
{
    public ServicesView()
    {
        InitializeComponent();

        DataContext = new ServicesViewModel();
    }
}

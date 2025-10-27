using FitnessCentrApp.Views;
using FitnessCentrApp.Views.UserControls;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitnessCentrApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private BranchesView branchesView = new();
    private TrainersView trainersView = new();
    private ServicesView servicesView = new();
    private ClientsView clientsView = new();
    public MainWindow()
    {
        InitializeComponent();

        TrainersTab.Content = trainersView;
        BranchesTab.Content = branchesView;
        ServicesTab.Content = servicesView;
        ClientsTab.Content = clientsView;
    }
}
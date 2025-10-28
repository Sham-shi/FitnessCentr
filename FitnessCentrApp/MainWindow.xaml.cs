using FitnessCentrApp.Views.UserControls;
using System.Windows;

namespace FitnessCentrApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //private BranchesView branchesView = new();
    //private TrainersView trainersView = new();
    //private ServicesView servicesView = new();
    //private ClientsView clientsView = new();
    public MainWindow()
    {
        InitializeComponent();

        //TrainersTab.Content = trainersView;
        //BranchesTab.Content = branchesView;
        //ServicesTab.Content = servicesView;
        //ClientsTab.Content = clientsView;
    }
}
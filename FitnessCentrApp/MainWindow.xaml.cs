using FitnessCentrApp.Views.UserControls;
using System.Windows;

namespace FitnessCentrApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
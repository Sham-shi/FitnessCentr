using FitnessCentrApp.ViewModels;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls;

public partial class TrainersView : UserControl
{
    private TrainersViewModel viewModel = new();
    public TrainersView()
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}

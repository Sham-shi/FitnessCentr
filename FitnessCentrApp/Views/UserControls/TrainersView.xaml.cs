using FitnessCentrApp.ViewModels;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls;

public partial class TrainersView : UserControl
{
    public TrainersView()
    {
        InitializeComponent();

        DataContext = new TrainersViewModel();
    }
}

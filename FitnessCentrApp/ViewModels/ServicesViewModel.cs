using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;

namespace FitnessCentrApp.ViewModels;

public class ServicesViewModel : BaseCrudViewModel<Service>
{
    public ObservableCollection<Service> Services => Items;

    public Service? SelectedService
    {
        get => SelectedItem;
        set { SelectedItem = value; }
    }
}

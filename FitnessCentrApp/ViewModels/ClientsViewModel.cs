using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;

namespace FitnessCentrApp.ViewModels;

public class ClientsViewModel : BaseCrudViewModel<Client>
{
    public ObservableCollection<Client> Clients => Items;

    public Client? SelectedClient
    {
        get => SelectedItem;
        set { SelectedItem = value; }
    }
}

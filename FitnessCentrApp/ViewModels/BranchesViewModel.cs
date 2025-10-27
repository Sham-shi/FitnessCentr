using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;

namespace FitnessCentrApp.ViewModels;

public class BranchesViewModel : BaseCrudViewModel<Branch>
{
    public ObservableCollection<Branch> Branches => Items;

    public Branch? SelectedBranch
    {
        get => SelectedItem;
        set => SelectedItem = value;
    }
}

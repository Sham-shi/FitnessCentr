using DbFirst.Models;
using DbFirst.Services;
using System.Collections.ObjectModel;

namespace FitnessCentrApp.ViewModels;

public class BranchesViewModel : BaseViewModel
{
    private readonly Repository<Branch> _repo = new();
    private Branch? _selectedBranch;
    private Branch _newBranch = new();

    public ObservableCollection<Branch> Branches { get; set; }

    public Branch? SelectedBranch
    {
        get => _selectedBranch;
        set { _selectedBranch = value; OnPropertyChanged(); }
    }

    public Branch NewBranch
    {
        get => _newBranch;
        set { _newBranch = value; OnPropertyChanged(); }
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public BranchesViewModel()
    {
        Branches = new ObservableCollection<Branch>(_repo.GetAll());

        AddCommand = new RelayCommand(_ => AddBranch());
        UpdateCommand = new RelayCommand(_ => UpdateBranch(), _ => SelectedBranch != null);
        DeleteCommand = new RelayCommand(_ => DeleteBranch(), _ => SelectedBranch != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
    }

    private void AddBranch()
    {
        _repo.Add(NewBranch);
        Refresh();
        NewBranch = new Branch();
    }

    private void UpdateBranch()
    {
        if (SelectedBranch != null)
            _repo.Update(SelectedBranch);
        Refresh();
    }

    private void DeleteBranch()
    {
        if (SelectedBranch != null)
            _repo.Delete(SelectedBranch);
        Refresh();
    }

    private void Refresh()
    {
        Branches.Clear();
        foreach (var b in _repo.GetAll())
            Branches.Add(b);
    }
}

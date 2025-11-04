using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.ViewModels.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels;

public class BranchesViewModel : BaseCrudViewModel<Branch>
{
    public ObservableCollection<Branch> Branches => Items;

    public Branch? SelectedBranch
    {
        get => SelectedItem;
        set => SelectedItem = value;
    }

    public override bool CheckFilling()
    {
        if (EditableItem is not Branch branch)
            return true;

        return string.IsNullOrWhiteSpace(SelectedBranch.BranchName) ||
                string.IsNullOrWhiteSpace(SelectedBranch.Address) ||
                string.IsNullOrWhiteSpace(SelectedBranch.Phone) ||
                string.IsNullOrWhiteSpace(SelectedBranch.Email);
    }

    protected override async Task SaveSelectedItemAsync()
    {
        if (SelectedBranch == null)
            return;

        // Проверяем обязательные поля
        if (CheckFilling())
        {
            await MessageBoxService.ShowErrorAsync("Ошибка", "Все поля обязательны для заполнения.");
            return;
        }

        await base.SaveSelectedItemAsync();
    }
}

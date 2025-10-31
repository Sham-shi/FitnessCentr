using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
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

    protected override void SaveSelectedItem()
    {
        if (SelectedBranch == null)
            return;

        // Проверяем обязательные поля
        if (CheckFilling())
        {
            MessageBox.Show("Все поля обязательны для заполнения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        base.SaveSelectedItem();
    }
}

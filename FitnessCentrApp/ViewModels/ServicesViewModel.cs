using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels;

public class ServicesViewModel : BaseCrudViewModel<Service>
{
    public ObservableCollection<Service> Services => Items;

    public Service? SelectedService
    {
        get => SelectedItem;
        set { SelectedItem = value; }
    }

    protected override void CreateNewItem()
    {
        base.CreateNewItem();

        if (EditableItem is Service service)
        {
            service.MaxParticipants = 1;
        }
    }

    public override bool CheckFilling()
    {
        if (EditableItem is not Service service)
            return true;

        return string.IsNullOrWhiteSpace(SelectedService.ServiceName) ||
                string.IsNullOrWhiteSpace(SelectedService.ServiceType);
    }

    protected override void SaveSelectedItem()
    {
        if (SelectedService == null)
            return;

        // Проверяем обязательные поля
        if (CheckFilling())
        {
            MessageBox.Show("Поля Название и Тип услуги обязательны для заполнения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        base.SaveSelectedItem();
    }
}

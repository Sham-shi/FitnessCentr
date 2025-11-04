using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.ViewModels.Services;
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

    // Список допустимых типов услуг для валидации
    private readonly string[] _validServiceTypes = { "Групповое", "Индивидуальное" };

    protected override void CreateNewItem()
    {
        base.CreateNewItem();

        if (EditableItem is Service service)
        {
            service.MaxParticipants = 1;
            service.ServiceType = _validServiceTypes[0];
        }
    }

    public override bool CheckFilling()
    {
        if (EditableItem is not Service service)
            return true;

        return string.IsNullOrWhiteSpace(SelectedService.ServiceName) ||
                string.IsNullOrWhiteSpace(SelectedService.ServiceType) ||
                !_validServiceTypes.Contains(SelectedService.ServiceType);
    }

    protected override async Task SaveSelectedItemAsync()
    {
        if (SelectedService == null)
            return;

        // Проверяем обязательные поля
        if (CheckFilling())
        {
            await MessageBoxService.ShowErrorAsync("Ошибка", "Поля Название и Тип услуги обязательны для заполнения.");
            return;
        }

        await base.SaveSelectedItemAsync();
    }
}

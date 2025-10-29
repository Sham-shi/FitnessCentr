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

    /// <summary>
    /// Создать новую услугу (пока не добавляется в БД)
    /// </summary>
    protected override void CreateNewItem()
    {
        var service = new Service()
        {
            ServiceName = "",
            ServiceType = "",
            DurationMinutes = 0,
            MaxParticipants = 0,
            BasePrice = 0,
            Description = ""
        };

        Items.Add(service);
        SelectedService = service;
    }

    public override bool CheckFilling()
    {
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

    protected override void UpdateItem()
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

        base.UpdateItem();
    }
}

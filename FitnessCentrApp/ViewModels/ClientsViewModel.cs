using DbFirst.Models;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels;

public class ClientsViewModel : BaseCrudViewModel<Client>
{
    public ObservableCollection<Client> Clients => Items;

    public Client? SelectedClient
    {
        get => SelectedItem;
        set { SelectedItem = value; }
    }

    protected override void CreateNewItem()
    {
        // 1. Вызываем базовый метод для создания пустого объекта
        base.CreateNewItem();

        // 2. Применяем специфичные значения для нового клиента
        if (EditableItem is Client client)
        {
            client.BirthDate = DateOnly.FromDateTime(DateTime.Today);
            client.RegistrationDate = DateOnly.FromDateTime(DateTime.Today);
        }
    }

    public override bool CheckFilling()
    {
        if (EditableItem is not Client client)
            return true;

        return string.IsNullOrWhiteSpace(SelectedClient.FullName) ||
                string.IsNullOrWhiteSpace(SelectedClient.Phone);
    }

    protected override void SaveSelectedItem()
    {
        if (SelectedClient == null)
            return;

        // Проверяем обязательные поля
        if (CheckFilling())
        {
            MessageBox.Show("Поля ФИО и Телефон обязательны для заполнения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        base.SaveSelectedItem();
    }
}

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
        var client = new Client()
        {
            FullName = "",
            Phone = "",
            Email = "",
            BirthDate = DateOnly.FromDateTime(DateTime.Now),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Now)
        };

        Items.Add(client);
        SelectedClient = client;
    }

    public override bool CheckFilling()
    {
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

    //protected override void UpdateItem()
    //{
    //    if (SelectedClient == null)
    //        return;

    //    // Проверяем обязательные поля
    //    if (CheckFilling())
    //    {
    //        MessageBox.Show("Поля ФИО и Телефон обязательны для заполнения.",
    //                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
    //        return;
    //    }

    //    base.UpdateItem();
    //}
}

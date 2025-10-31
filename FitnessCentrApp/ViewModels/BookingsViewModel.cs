using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels;

public class BookingsViewModel : BaseCrudViewModel<Booking>
{
    public ObservableCollection<Booking> Bookings => Items;

    public ObservableCollection<Client> Clients { get; private set; }
    public ObservableCollection<Service> Services { get; private set; }
    public ObservableCollection<Trainer> Trainers { get; private set; }

    public Booking? SelectedBooking
    {
        get => SelectedItem;
        set => SelectedItem = value;
    }

    public BookingsViewModel()
    {
        Clients = new ObservableCollection<Client>(new Repository<Client>().GetAll());
        Services = new ObservableCollection<Service>(new Repository<Service>().GetAll());
        Trainers = new ObservableCollection<Trainer>(new Repository<Trainer>().GetAll());
    }

    protected override void CreateNewItem()
    {
        var booking = new Booking()
        {
            ClientID = Clients.FirstOrDefault()?.ClientID ?? 1,
            ServiceID = Services.FirstOrDefault()?.ServiceID ?? 1,
            TrainerID = Trainers.FirstOrDefault()?.TrainerID ?? 1,
            BookingDateTime = DateTime.Now,
            SessionsCount = 0,
            TotalPrice = 0,
            Status = "Запланировано",
            Notes = "",
            CreatedDate = DateTime.Now
        };

        Items.Add(booking);
        SelectedBooking = booking;
    }

    public override bool CheckFilling()
    {
        return string.IsNullOrWhiteSpace(SelectedBooking.Status);
    }

    protected override void SaveSelectedItem()
    {
        if (SelectedBooking == null)
            return;

        // Проверяем обязательные поля
        if (CheckFilling())
        {
            MessageBox.Show("Поля Статус и Кол-во занятий обязательны для заполнения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        base.SaveSelectedItem();
    }

    //protected override void UpdateItem()
    //{
    //    if (SelectedBooking == null)
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

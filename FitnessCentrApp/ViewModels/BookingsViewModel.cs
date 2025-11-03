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
        Clients = new ObservableCollection<Client>(DatabaseService.GetAll<Client>());
        Services = new ObservableCollection<Service>(DatabaseService.GetAll<Service>());
        Trainers = new ObservableCollection<Trainer>(DatabaseService.GetAll<Trainer>());
    }

    private readonly string[] _validStatuses = { "Запланировано", "Перенесено", "Завершено", "Отменено" };

    protected override void CreateNewItem()
    {
        base.CreateNewItem();

        if (EditableItem is Booking booking)
        {
            booking.ClientID = Clients.FirstOrDefault()?.ClientID ?? 1;
            booking.ServiceID = Services.FirstOrDefault()?.ServiceID ?? 1;
            booking.TrainerID = Trainers.FirstOrDefault()?.TrainerID ?? 1;
            booking.Status = _validStatuses[0];
            booking.SessionsCount = 1;
        }
    }

    public override bool CheckFilling()
    {
        if (EditableItem is not Booking booking)
            return true;

        return !_validStatuses.Contains(SelectedBooking.Status) ||
               booking.SessionsCount <= 0;
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

        RecalculateTotalPrice(EditableItem);

        base.SaveSelectedItem();
    }

    public void RecalculateTotalPrice(Booking booking)
    {
        if (booking == null) return;

        var service = Services.FirstOrDefault(s => s.ServiceID == booking.ServiceID);
        booking.TotalPrice = (service?.BasePrice ?? 0m) * booking.SessionsCount;
    }
}

using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using System.Collections.ObjectModel;

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
}

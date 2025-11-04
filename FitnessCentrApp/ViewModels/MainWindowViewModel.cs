using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using System.Windows.Input;

namespace FitnessCentrApp.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private bool _isSqLiteCheked = true;
    public bool IsSqLiteCheked
    {
        get => _isSqLiteCheked;
        set
        {
            _isSqLiteCheked = value;
            OnPropertyChanged();
        }
    }

    private bool _isMssqlCheked = false;
    public bool IsMssqlCheked
    {
        get => _isMssqlCheked;
        set
        {
            _isMssqlCheked= value;
            OnPropertyChanged();
        }
    }

    // Свойство для получения текущей активной базы данных
    public string CurrentDbType => DatabaseService.CurrentDb;

    // Команда для переключения на MSSQL
    public ICommand UseMssqlCommand { get; }

    // Команда для переключения на SQLite
    public ICommand UseSqliteCommand { get; }

    public MainWindowViewModel()
    {
        UseMssqlCommand = new RelayCommand(
            _ => UseDatabase("MSSQL"),
            _ => CurrentDbType != "MSSQL" // Можно переключиться, если текущий не MSSQL
        );

        UseSqliteCommand = new RelayCommand(
            _ => UseDatabase("SQLite"),
            _ => CurrentDbType != "SQLite" // Можно переключиться, если текущий не SQLite
        );
    }

    private void UseDatabase(string dbType)
    {
        try
        {
            // 1. Попытка смены БД
            DatabaseService.UseDatabase(dbType);

            if (dbType.Equals("SQLite"))
            {
                IsSqLiteCheked= true;
                IsMssqlCheked = false;
            }
            else
            {
                IsSqLiteCheked= false;
                IsMssqlCheked = true;
            }

            // 2. Инициализация новой БД (создаст, если еще нет)
            DatabaseService.InitializeDatabase();
            CommandManager.InvalidateRequerySuggested();

            // 3. Уведомление об изменении (нужно для обновления галочек/состояния в UI)
            // PropertyChanged должен быть вызван для CurrentDbType, чтобы UI обновился
            OnPropertyChanged(nameof(CurrentDbType));
        }
        catch (Exception ex)
        {
            // Обработка ошибки переключения или инициализации
            System.Windows.MessageBox.Show($"Ошибка при переключении на {dbType}: {ex.Message}");
        }
    }
}

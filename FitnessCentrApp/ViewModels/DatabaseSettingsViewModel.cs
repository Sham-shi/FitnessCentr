using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using System.Windows.Input;

namespace FitnessCentrApp.ViewModels;

public class DatabaseSettingsViewModel : BaseViewModel
{
    // Свойство для получения текущей активной базы данных
    public string CurrentDbType => DatabaseService.CurrentDb;

    // Команда для переключения на MSSQL
    public ICommand UseMssqlCommand { get; }

    // Команда для переключения на SQLite
    public ICommand UseSqliteCommand { get; }

    public DatabaseSettingsViewModel()
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

            // 2. Инициализация новой БД (создаст, если еще нет)
            DatabaseService.InitializeDatabase();
            CommandManager.InvalidateRequerySuggested();
            // 3. Уведомление об изменении (нужно для обновления галочек/состояния в UI)
            // PropertyChanged должен быть вызван для CurrentDbType, чтобы UI обновился
            OnPropertyChanged(nameof(CurrentDbType));


            //Если вам нужна перезагрузка всего приложения после смены,
            //то после DatabaseService.InitializeDatabase() вызовите:
            //System.Windows.MessageBox.Show(
            //$"База данных успешно изменена на {dbType}. Приложение будет перезапущено для применения изменений.",
            //"Смена базы данных",
            //System.Windows.MessageBoxButton.OK,
            //System.Windows.MessageBoxImage.Information);

            //// 1. Получаем полный путь к исполняемому файлу (.exe)
            //string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //// 2. Запускаем новый экземпляр приложения
            //System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(executablePath));

            //// 3. Завершаем текущий экземпляр
            //System.Windows.Application.Current.Shutdown();

        }
        catch (Exception ex)
        {
            // Обработка ошибки переключения или инициализации
            System.Windows.MessageBox.Show($"Ошибка при переключении на {dbType}: {ex.Message}");
        }
    }
}

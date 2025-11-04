using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.ViewModels.Base.Interfaces;
using FitnessCentrApp.ViewModels.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FitnessCentrApp.ViewModels
{
    public class TrainersViewModel : BaseCrudViewModel<Trainer>
    {
        public ObservableCollection<Trainer> Trainers => Items;

        public ObservableCollection<Branch> Branches { get; private set; }

        public Trainer? SelectedTrainer
        {
            get => SelectedItem;
            set
            {
                SelectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPhoto));
            }
        }

        public BitmapImage? SelectedPhoto => LoadPhoto((EditableItem as Trainer)?.PhotoPath ?? SelectedTrainer?.PhotoPath);

        public AsyncRelayCommand SelectPhotoCommand { get; }

        public TrainersViewModel()
        {
            Branches = new ObservableCollection<Branch>(DatabaseService.GetAll<Branch>());
            SelectPhotoCommand = new AsyncRelayCommand(async _ => await SelectPhoto());
        }

        protected override void CreateNewItem()
        {
            base.CreateNewItem();

            if (EditableItem is Trainer trainer)
            {
                trainer.BranchID = Branches.FirstOrDefault()?.BranchID ?? 1;
                trainer.PhotoPath = "/Photos/Images/default_user.png";
            }

            OnPropertyChanged(nameof(SelectedTrainer));
        }

        protected override async Task SaveSelectedItemAsync()
        {
            if (SelectedTrainer == null)
                return;

            // Проверяем обязательные поля
            if (CheckFilling())
            {
                await MessageBoxService.ShowErrorAsync("Ошибка", "Для заполнения обязательны все поля, кроме Зарплата и Фото.");
                return;
            }

            await base.SaveSelectedItemAsync();
        }

        public override bool CheckFilling()
        {
            // Сначала безопасная проверка: если нет выбранного/редактируемого тренера - это ошибка
            if (EditableItem is not Trainer trainer)
                return true;

            return (string.IsNullOrWhiteSpace(SelectedTrainer.FullName) ||
                    string.IsNullOrWhiteSpace(SelectedTrainer.Phone) ||
                    string.IsNullOrWhiteSpace(SelectedTrainer.Email) ||
                    string.IsNullOrWhiteSpace(SelectedTrainer.Specialization) ||
                    string.IsNullOrWhiteSpace(SelectedTrainer.Education) ||
                    string.IsNullOrWhiteSpace(SelectedTrainer.WorkExperience) ||
                    string.IsNullOrWhiteSpace(SelectedTrainer.SportsAchievements));
        }

        protected override void Refresh()
        {
            base.Refresh();
            OnPropertyChanged(nameof(SelectedPhoto));
        }

        private async Task SelectPhoto()
        {
            if (EditableItem is not Trainer trainer)
            {
                await MessageBoxService.ShowInfoAsync("Информация", "Выберите тренера для добавления фотографии.");
                return;
            }

            var dlg = new OpenFileDialog
            {
                Title = "Выберите фото тренера",
                Filter = "Файлы изображений|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dlg.ShowDialog() == true)
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var folder = Path.Combine(appDirectory, "Photos");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // --- 1. Извлекаем части имени файла ---
                var fullPath = dlg.FileName;
                var originalFileName = Path.GetFileNameWithoutExtension(fullPath); // "моя_фотка"
                var fileExtension = Path.GetExtension(fullPath);                   // ".jpg"

                // --- 2. Формируем имя с инкрементальным суффиксом (1), (2)... ---
                string baseName = originalFileName;
                int counter = 0;
                string uniqueFileName = string.Empty;
                string destPath = string.Empty;

                do
                {
                    // Формируем имя: "Имя.ext" или "Имя (1).ext", "Имя (2).ext", и т.д.
                    if (counter == 0)
                        uniqueFileName = $"{baseName}{fileExtension}";
                    else
                        uniqueFileName = $"{baseName} ({counter}){fileExtension}";

                    destPath = Path.Combine(folder, uniqueFileName);
                    counter++;

                } while (File.Exists(destPath)); // Продолжаем, пока файл по пути destPath существует

                // --- 3. Копируем файл и обновляем PhotoPath ---
                try
                {
                    // КЛЮЧЕВОЙ ШАГ: КОПИРУЕМ ФАЙЛ С ДИСКА ПОЛЬЗОВАТЕЛЯ В НАШУ ПАПКУ
                    // Поскольку имя файла уникально, перезаписи не произойдет.
                    File.Copy(dlg.FileName, destPath, true);
                    // Это путь, относительно корня приложения или сервера.
                    trainer.PhotoPath = $"/Photos/{uniqueFileName}";

                    OnPropertyChanged(nameof(SelectedPhoto));
                    OnPropertyChanged(nameof(EditableItem));
                }
                catch (Exception ex)
                {
                    await MessageBoxService.ShowErrorAsync("Ошибка", "Ошибка копирования файла: {ex.Message}");
                }
            }
        }

        private BitmapImage? LoadPhoto(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.TrimStart('\\', '/');
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(appDirectory, path);

            if (!File.Exists(fullPath))
                return null;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(fullPath))
            {
                // Возвращаем заглушку, если путь пуст или файл не найден.
                return new BitmapImage(new Uri("pack://application:,,,/FitnessCentrApp;component/Photos/Images/default_user.png"));
            }

            try
            {
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // Освобождает файл после загрузки
                    image.StreamSource = stream; // Загрузка из потока
                    image.EndInit();
                    image.Freeze(); // Рекомендуется для BitmapImage в MVVM
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}

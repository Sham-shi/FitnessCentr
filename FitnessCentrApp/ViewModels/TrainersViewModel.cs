using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
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

        public RelayCommand SelectPhotoCommand { get; }

        public TrainersViewModel()
        {
            Branches = new ObservableCollection<Branch>(DatabaseService.GetAll<Branch>());
            SelectPhotoCommand = new RelayCommand(_ => SelectPhoto());
        }

        protected override void CreateNewItem()
        {
            base.CreateNewItem();

            if (EditableItem is Trainer trainer)
            {
                trainer.BranchID = Branches.FirstOrDefault()?.BranchID ?? 1;
            }
        }

        protected override void SaveSelectedItem()
        {
            if (SelectedTrainer == null)
                return;

            // Проверяем обязательные поля
            if (CheckFilling())
            {
                MessageBox.Show("Для заполнения обязательны все поля, кроме Зарплата и Фото.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            base.SaveSelectedItem();
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

        private void SelectPhoto()
        {
            if (EditableItem is not Trainer trainer)
            {
                MessageBox.Show("Выберите тренера для добавления фотографии.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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

                // --- 2. Создаем уникальный суффикс (короткий таймстамп) ---
                // Формат: "_yyyyMMddHHmmss" или "_ddMMyy_hhmmss"
                var uniqueSuffix = DateTime.Now.ToString("_yyyyMMddHHmmss");

                // --- 3. Формируем новое уникальное имя файла ---
                // Пример: "моя_фотка_20251101123638.jpg"
                var uniqueFileName = $"{originalFileName}{uniqueSuffix}{fileExtension}";

                // --- 4. Создаем путь назначения и копируем файл ---
                var destPath = Path.Combine(folder, uniqueFileName);

                try
                {
                    // КЛЮЧЕВОЙ ШАГ: КОПИРУЕМ ФАЙЛ С ДИСКА ПОЛЬЗОВАТЕЛЯ В НАШУ ПАПКУ
                    // Поскольку имя файла уникально, перезаписи не произойдет.
                    File.Copy(dlg.FileName, destPath, true);
                    // Это путь, относительно корня приложения или сервера.
                    trainer.PhotoPath = $"/Photos/{uniqueFileName}";
                    OnPropertyChanged(nameof(SelectedPhoto));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка копирования файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        protected override void Refresh()
        {
            base.Refresh();
            OnPropertyChanged(nameof(SelectedPhoto));
        }
    }
}

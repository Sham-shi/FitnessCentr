using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FitnessCentrApp.ViewModels
{
    public class TrainersViewModel : BaseCrudViewModel<Trainer>
    {
        private readonly Repository<Branch> _branchesRepo = new();

        public ObservableCollection<Trainer> Trainers => Items;

        public ObservableCollection<Branch> Branches { get; set; }

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

        public BitmapImage? SelectedPhoto => LoadPhoto(SelectedTrainer?.PhotoPath);

        public RelayCommand SelectPhotoCommand { get; }

        public TrainersViewModel()
        {
            Branches = new ObservableCollection<Branch>(_branchesRepo.GetAll());
            SelectPhotoCommand = new RelayCommand(_ => SelectPhoto());
        }

        /// <summary>
        /// Создать нового тренера (пока не добавляется в БД)
        /// </summary>
        protected override void CreateNewItem()
        {
            var trainer = new Trainer
            {
                FullName = "",
                Phone = "",
                Email = "",
                Education = "",
                WorkExperience = "",
                SportsAchievements = "",
                Specialization = "",
                Salary = 0,
                BranchID = Branches.FirstOrDefault()?.BranchID ?? 1,
                PhotoPath = ""
            };

            Items.Add(trainer);
            SelectedTrainer = trainer;
        }

        /// <summary>
        /// Сохранить выбранного тренера в БД
        /// </summary>
        protected override void SaveSelectedItem()
        {
            if (SelectedTrainer == null)
                return;

            // Проверяем обязательные поля
            if (string.IsNullOrWhiteSpace(SelectedTrainer.FullName) ||
                string.IsNullOrWhiteSpace(SelectedTrainer.Phone) ||
                string.IsNullOrWhiteSpace(SelectedTrainer.Email))
            {
                MessageBox.Show("Поля ФИО, Телефон и Email обязательны для заполнения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _repo.Add(SelectedTrainer);
                Refresh();


                IsReadOnly = true; // снова делаем только для чтения
                EditableItem = null; // снимаем режим редактирования

                MessageBox.Show("Тренер успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectPhoto()
        {
            if (SelectedTrainer == null)
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
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "Photos");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Path.GetFileName(dlg.FileName);
                var destPath = Path.Combine(folder, fileName);
                File.Copy(dlg.FileName, destPath, true);

                SelectedTrainer.PhotoPath = $"/Photos/{fileName}";
                OnPropertyChanged(nameof(SelectedPhoto));
            }
        }

        private BitmapImage? LoadPhoto(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.TrimStart('\\', '/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);

            if (!File.Exists(fullPath))
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(fullPath);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
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

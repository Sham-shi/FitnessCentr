using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.ViewModels.Base.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FitnessCentrApp.ViewModels
{
    public class TrainersViewModel : BaseCrudViewModel<Trainer>
    {
        private readonly Repository<Trainer> _trainerRepo = new();
        private readonly Repository<Branch> _branchesRepo = new();

        public ObservableCollection<Branch> Branches { get; }

        public TrainerViewModel? SelectedTrainer
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
            Refresh();
        }

        protected override void Refresh()
        {
            Items.Clear();
            foreach (var trainer in _trainerRepo.GetAll())
                Items.Add(new TrainerViewModel(trainer));

            IsReadOnly = true;
            EditableItem = null;
            OnPropertyChanged(nameof(SelectedPhoto));
        }

        protected override void CreateNewItem()
        {
            var vm = new TrainerViewModel();
            Items.Add(vm);
            SelectedTrainer = vm;
            IsReadOnly = false;
            EditableItem = vm;
        }

        protected override void SaveSelectedItem()
        {
            if (SelectedTrainer == null)
                return;

            SelectedTrainer.ValidateAll();
            if (SelectedTrainer.HasErrors)
            {
                MessageBox.Show("Исправьте ошибки перед сохранением.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _trainerRepo.Add(SelectedTrainer.Model);
                Refresh();
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
                MessageBox.Show("Выберите тренера для добавления фотографии.");
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
    }
}

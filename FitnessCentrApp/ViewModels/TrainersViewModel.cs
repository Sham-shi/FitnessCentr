using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FitnessCentrApp.ViewModels
{
    public class TrainersViewModel : BaseCrudViewModel<Trainer>
    {
        private readonly Repository<Branch> _branchesRepo = new();

        // Свойства с валидацией
        private string _fullName;
        private string _phone;
        private string _email;
        private decimal _salary;

        [Required(ErrorMessage = "ФИО обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "ФИО должно быть от 2 до 100 символов")]
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
                ValidateProperty(value);
            }
        }

        [Required(ErrorMessage = "Телефон обязателен для заполнения")]
        [CustomValidation(typeof(TrainersViewModel), nameof(ValidatePhone))]
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
                ValidateProperty(value);
            }
        }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                ValidateProperty(value);
            }
        }

        [Range(0, 1000000, ErrorMessage = "Зарплата должна быть от 0 до 1 000 000")]
        public decimal Salary
        {
            get => _salary;
            set
            {
                _salary = value;
                OnPropertyChanged();
                ValidateProperty(value);
            }
        }

        public ObservableCollection<Trainer> Trainers => Items;
        public ObservableCollection<Branch> Branches { get; private set; }

        public Trainer? SelectedTrainer
        {
            get => SelectedItem;
            set
            {
                SelectedItem = value;
                UpdateValidationProperties();
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

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedItem))
                {
                    UpdateValidationProperties();
                }
            };
        }

        // Кастомная валидация телефона
        public static ValidationResult ValidatePhone(object value, ValidationContext context)
        {
            var phone = value as string;

            if (string.IsNullOrEmpty(phone))
                return ValidationResult.Success; // Required атрибут уже проверит это

            // Очищаем телефон от всех нецифровых символов, кроме +
            var cleanPhone = CleanPhoneNumber(phone);

            // Проверяем минимальную длину (например, 10 цифр без кода страны)
            if (cleanPhone.Length < 10)
                return new ValidationResult("Телефон должен содержать не менее 10 цифр");

            // Проверяем формат российского телефона
            if (!IsValidRussianPhone(cleanPhone))
                return new ValidationResult("Неверный формат телефона.");

            return ValidationResult.Success;
        }

        // Очистка номера телефона
        private static string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            // Оставляем только цифры
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        // Проверка российского номера телефона
        private static bool IsValidRussianPhone(string cleanPhone)
        {
            // Российские номера обычно начинаются с +7, 7, 8 или без кода
            if (cleanPhone.StartsWith("+7"))
                cleanPhone = cleanPhone.Substring(2); // Убираем +7
            else if (cleanPhone.StartsWith("7"))
                cleanPhone = cleanPhone.Substring(1); // Убираем 7
            else if (cleanPhone.StartsWith("8"))
                cleanPhone = cleanPhone.Substring(1); // Убираем 8

            // После очистки должно остаться 10 цифр
            if (cleanPhone.Length != 10)
                return false;

            // Проверяем, что все символы - цифры
            return cleanPhone.All(char.IsDigit);
        }

        private void UpdateValidationProperties()
        {
            if (SelectedTrainer != null)
            {
                FullName = SelectedTrainer.FullName;
                Phone = SelectedTrainer.Phone;
                Email = SelectedTrainer.Email;
                Salary = SelectedTrainer.Salary;
            }
            else
            {
                FullName = string.Empty;
                Phone = string.Empty;
                Email = string.Empty;
                Salary = 0;
            }
        }

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
            UpdateValidationProperties();
        }

        protected override void SaveSelectedItem()
        {
            if (SelectedTrainer == null) return;

            // Очищаем телефон перед сохранением
            if (!string.IsNullOrEmpty(Phone))
            {
                SelectedTrainer.Phone = CleanPhoneNumber(Phone);
            }

            SelectedTrainer.FullName = FullName;
            SelectedTrainer.Email = Email;
            SelectedTrainer.Salary = Salary;

            if (HasErrors)
            {
                MessageBox.Show("Исправьте ошибки валидации перед сохранением.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _repo.Add(SelectedTrainer);
                Refresh();
                IsReadOnly = true;
                EditableItem = null;
                MessageBox.Show("Тренер успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void UpdateItem()
        {
            if (SelectedTrainer == null) return;

            // Очищаем телефон перед обновлением
            if (!string.IsNullOrEmpty(Phone))
            {
                SelectedTrainer.Phone = CleanPhoneNumber(Phone);
            }

            SelectedTrainer.FullName = FullName;
            SelectedTrainer.Email = Email;
            SelectedTrainer.Salary = Salary;

            base.UpdateItem();
        }

        // Остальные методы остаются без изменений...
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
            if (string.IsNullOrWhiteSpace(path)) return null;

            path = path.TrimStart('\\', '/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);

            if (!File.Exists(fullPath)) return null;

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

        protected override void CustomValidation(object value, string propertyName)
        {
            if (propertyName == nameof(Email) && !string.IsNullOrEmpty(Email))
            {
                var existingTrainer = Items.FirstOrDefault(t =>
                    t.Email == Email && t != SelectedTrainer);
                if (existingTrainer != null)
                {
                    AddError(propertyName, "Тренер с таким email уже существует");
                }
            }

            if (propertyName == nameof(Phone) && !string.IsNullOrEmpty(Phone))
            {
                // Проверка уникальности
                var cleanPhone = CleanPhoneNumber(Phone);
                var existingTrainer = Items.FirstOrDefault(t =>
                    CleanPhoneNumber(t.Phone) == cleanPhone && t != SelectedTrainer);
                if (existingTrainer != null)
                {
                    AddError(propertyName, "Тренер с таким телефоном уже существует");
                }

                // Автоформатирование (опционально)
                if (string.IsNullOrEmpty(this[propertyName])) // Если нет ошибок валидации
                {
                    var formattedPhone = FormatPhoneNumber(Phone);
                    if (formattedPhone != Phone)
                    {
                        // Обновляем значение с форматированием
                        Phone = formattedPhone;
                    }
                }
            }
        }

        // Метод для форматирования телефона
        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            var cleanPhone = CleanPhoneNumber(phone);

            if (cleanPhone.Length == 10)
            {
                return $"+7 ({cleanPhone.Substring(0, 3)}) {cleanPhone.Substring(3, 3)}-{cleanPhone.Substring(6, 2)}-{cleanPhone.Substring(8, 2)}";
            }
            else if (cleanPhone.Length == 11 && cleanPhone.StartsWith("7"))
            {
                cleanPhone = cleanPhone.Substring(1);
                return $"+7 ({cleanPhone.Substring(0, 3)}) {cleanPhone.Substring(3, 3)}-{cleanPhone.Substring(6, 2)}-{cleanPhone.Substring(8, 2)}";
            }

            return phone; // Возвращаем как есть, если не можем отформатировать
        }
    }
}
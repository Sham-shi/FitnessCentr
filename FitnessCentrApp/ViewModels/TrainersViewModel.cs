using DbFirst.Models;
using DbFirst.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FitnessCentrApp.ViewModels;

public class TrainersViewModel : BaseViewModel
{
    private readonly Repository<Trainer> _trainersRepo = new();
    private readonly Repository<Branch> _branchesRepo = new();

    private Trainer? _selectedTrainer;
    private Trainer _newTrainer = new();

    public ObservableCollection<Trainer> Trainers { get; set; }
    public ObservableCollection<Branch> Branches { get; set; }

    public Trainer? SelectedTrainer
    {
        get => _selectedTrainer;
        set { _selectedTrainer = value; OnPropertyChanged(); }
    }

    public Trainer NewTrainer
    {
        get => _newTrainer;
        set { _newTrainer = value; OnPropertyChanged(); }
    }

    // Отображение фото в интерфейсе
    public BitmapImage? SelectedPhoto => LoadPhoto(SelectedTrainer?.PhotoPath);
    public BitmapImage? NewPhoto => LoadPhoto(NewTrainer?.PhotoPath);

    public RelayCommand AddCommand { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand SelectPhotoCommand { get; }

    public TrainersViewModel()
    {
        Trainers = new ObservableCollection<Trainer>(_trainersRepo.GetAll());
        Branches = new ObservableCollection<Branch>(_branchesRepo.GetAll());

        AddCommand = new RelayCommand(_ => AddTrainer());
        UpdateCommand = new RelayCommand(_ => UpdateTrainer(), _ => SelectedTrainer != null);
        DeleteCommand = new RelayCommand(_ => DeleteTrainer(), _ => SelectedTrainer != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        SelectPhotoCommand = new RelayCommand(_ => SelectPhoto());
    }

    private void SelectPhoto()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выберите фото тренера",
            Filter = "Файлы изображений|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dlg.ShowDialog() == true)
        {
            // Копируем файл в локальную папку проекта (например, "Photos")
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "Photos");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Path.GetFileName(dlg.FileName);
            var destPath = Path.Combine(folder, fileName);

            File.Copy(dlg.FileName, destPath, true);

            // сохраняем относительный путь для БД
            var relativePath = $"/Photos/{fileName}";

            // Присваиваем путь текущему тренеру
            if (SelectedTrainer != null)
            {
                SelectedTrainer.PhotoPath = relativePath;
                OnPropertyChanged(nameof(SelectedPhoto));
            }
            else
            {
                NewTrainer.PhotoPath = destPath;
                OnPropertyChanged(nameof(NewPhoto));
            }
        }
    }

    private BitmapImage? LoadPhoto(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        // Если путь начинается со слеша — уберём его
        path = path.TrimStart('\\', '/');

        // Формируем абсолютный путь (из папки, где запущено приложение)
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

    private void AddTrainer()
    {
        if (NewTrainer.BranchID == 0 || string.IsNullOrWhiteSpace(NewTrainer.FullName))
            return;

        _trainersRepo.Add(NewTrainer);
        Refresh();
        NewTrainer = new Trainer();
    }

    private void UpdateTrainer()
    {
        if (SelectedTrainer != null)
            _trainersRepo.Update(SelectedTrainer);
        Refresh();
    }

    private void DeleteTrainer()
    {
        if (SelectedTrainer != null)
            _trainersRepo.Delete(SelectedTrainer);
        Refresh();
    }

    private void Refresh()
    {
        Trainers.Clear();
        foreach (var t in _trainersRepo.GetAll())
            Trainers.Add(t);

        OnPropertyChanged(nameof(SelectedPhoto));
    }
}

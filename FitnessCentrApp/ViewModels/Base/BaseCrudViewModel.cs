using DbFirst.Models;
using DbFirst.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels.Base;

public abstract class BaseCrudViewModel<T> : BaseViewModel where T : class, new()
{
    protected readonly Repository<T> _repo = new();

    private T? _selectedItem;

    public ObservableCollection<T> Items { get; set; }

    public T? SelectedItem
    {
        get => _selectedItem;
        set { _selectedItem = value; OnPropertyChanged(); }
    }

    private bool _isReadOnly = true;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            _isReadOnly = value;
            OnPropertyChanged();
        }
    }

    // Команды
    public RelayCommand EditCommand { get; }
    public RelayCommand CreateCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    protected BaseCrudViewModel()
    {
        Items = new ObservableCollection<T>(_repo.GetAll());

        EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
        CreateCommand = new RelayCommand(_ => CreateNewItem());
        SaveCommand = new RelayCommand(_ => SaveSelectedItem(), _ => SelectedItem != null);
        UpdateCommand = new RelayCommand(_ => UpdateItem(), _ => SelectedItem != null);
        DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
    }

    protected virtual void CreateNewItem()
    {
        var item = new T();
        Items.Add(item);
        SelectedItem = item;
        IsReadOnly = false;
    }

    protected virtual void SaveSelectedItem()
    {
        if (SelectedItem == null)
            return;

        try
        {
            _repo.Add(SelectedItem);
            Refresh();
            IsReadOnly = true; // снова делаем только для чтения

            MessageBox.Show("Изменения сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected virtual void UpdateItem()
    {
        if (SelectedItem == null)
            return;

        try
        {
            _repo.Update(SelectedItem);
            Refresh();
            IsReadOnly = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected virtual void EditItem()
    {
        if (SelectedItem == null)
        {
            MessageBox.Show("Сначала выберите элемент для редактирования.");
            return;
        }

        IsReadOnly = false; // разрешаем редактирование
    }

    protected virtual void DeleteItem()
    {
        if (SelectedItem == null)
            return;

        if (MessageBox.Show("Вы уверены, что хотите удалить запись?",
            "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                _repo.Delete(SelectedItem);
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    protected virtual void Refresh()
    {
        Items.Clear();
        foreach (var item in _repo.GetAll())
            Items.Add(item);
    }
}

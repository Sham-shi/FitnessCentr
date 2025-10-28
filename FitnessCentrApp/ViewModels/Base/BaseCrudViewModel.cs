using DbFirst.Models;
using DbFirst.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels.Base;

public interface IEditableViewModel
{
    event Action<object> BeginEditRequested;
}

public abstract class BaseCrudViewModel<T> : BaseViewModel, IEditableViewModel where T : class, new()
{
    public event Action<object>? BeginEditRequested;

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

    private T? _editableItem;
    public T? EditableItem
    {
        get => _editableItem;
        set
        {
            _editableItem = value;
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
            EditableItem = null; // снимаем режим редактирования

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
            EditableItem = null; // снимаем режим редактирования
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

        EditableItem = SelectedItem; // запоминаем, что можно редактировать только этот объект
        IsReadOnly = false; // разрешаем редактирование

        // сигнал для View, что нужно начать редактирование
        BeginEditRequested?.Invoke(SelectedItem);
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

        IsReadOnly = true;
        EditableItem = null;
    }
}

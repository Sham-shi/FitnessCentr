using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace FitnessCentrApp.ViewModels.Base;

public abstract class BaseCrudViewModel<T> : BaseViewModel, IEditableViewModel, ICheckableViewModel where T : class, new()
{
    protected readonly Repository<T> _repo = new();

    public ObservableCollection<T> Items { get; set; }

    private T? _selectedItem;
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

    //private bool _isUpdateEnabled = false;
    //public bool IsUpdateEnabled
    //{
    //    get => _isUpdateEnabled;
    //    set
    //    {
    //        _isUpdateEnabled = value;
    //        OnPropertyChanged();
    //    }
    //}

    private bool _isSaveEnabled = false;
    public bool IsSaveEnabled
    {
        get => _isSaveEnabled;
        set
        {
            _isSaveEnabled = value;
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

    // Реализация интерфейса
    object? IEditableViewModel.EditableItem => EditableItem;
    public event Action<object>? BeginEditRequested;

    // Команды
    public RelayCommand EditCommand { get; }
    public RelayCommand CreateCommand { get; }
    public RelayCommand SaveCommand { get; }
    //public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    protected BaseCrudViewModel()
    {
        Items = new ObservableCollection<T>(_repo.GetAll());

        EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
        CreateCommand = new RelayCommand(_ => CreateNewItem());
        SaveCommand = new RelayCommand(_ => SaveSelectedItem(), _ => SelectedItem != null);
        //UpdateCommand = new RelayCommand(_ => UpdateItem(), _ => SelectedItem != null);
        DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
    }

    protected virtual void CreateNewItem()
    {
        // логика у каждого своя
    }

    protected virtual void SaveSelectedItem()
    {
        if (EditableItem == null)
            return;

        try
        {
            _repo.Save(EditableItem);
            Refresh();

            IsReadOnly = true; // снова делаем только для чтения
            EditableItem = null; // снимаем режим редактирования
            IsSaveEnabled = false;

            MessageBox.Show("Изменения сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    //protected virtual void UpdateItem()
    //{
    //    if (EditableItem == null)
    //        return;

    //    try
    //    {
    //        _repo.Update(EditableItem);
    //        Refresh();

    //        IsReadOnly = true;
    //        EditableItem = null; // снимаем режим редактирования
    //        IsUpdateEnabled = false;

    //        MessageBox.Show("Изменения сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}

    protected virtual void EditItem()
    {
        if (SelectedItem == null)
        {
            MessageBox.Show("Сначала выберите элемент для редактирования.");
            return;
        }

        EditableItem = SelectedItem; // запоминаем, что можно редактировать только этот объект
        IsReadOnly = false; // разрешаем редактирование
        IsSaveEnabled = true;

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
        IsSaveEnabled = false;
    }

    public virtual bool CheckFilling()
    {
        throw new NotImplementedException();
    }
}

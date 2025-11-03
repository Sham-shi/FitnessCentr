using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace FitnessCentrApp.ViewModels.Base;

public abstract class BaseCrudViewModel<T> : BaseViewModel, IEditableViewModel, ICheckableViewModel where T : class, new()
{
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

    private bool _isEditEnabled = true;
    public bool IsSEditEnabled
    {
        get => _isEditEnabled;
        set
        {
            _isEditEnabled = value;
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
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    protected BaseCrudViewModel()
    {
        Items = new ObservableCollection<T>();

        EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
        CreateCommand = new RelayCommand(_ => CreateNewItem());
        SaveCommand = new RelayCommand(_ => SaveSelectedItem(), _ => EditableItem != null);
        DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);
        RefreshCommand = new RelayCommand(_ => Refresh());

        Refresh();
    }

    protected virtual void CreateNewItem()
    {
        // 1. Создаем новый экземпляр T
        SelectedItem = new T();
        EditableItem = SelectedItem;
        IsReadOnly = false;
        IsSaveEnabled = true;
        IsSEditEnabled = false;

        // 2. Добавляем элемент в коллекцию (начинается асинхронная отрисовка)
        Items.Add(SelectedItem);

        // 3. Откладываем сигнал для View, чтобы он выполнился после завершения отрисовки DataGrid.
        Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background, // Низкий приоритет, ждем завершения основного рендеринга
            new Action(() => BeginEditRequested?.Invoke(SelectedItem))
        );
    }

    protected virtual void SaveSelectedItem()
    {
        if (EditableItem is not T entity || entity.Equals(default(T)))
            return;

        try
        {
            using var ctx = DatabaseService.CreateContext();

            // Update() выполняет проверку ID и устанавливает
            // State = Added, если ID = 0, или State = Modified, если ID > 0.
            // Это заменяет всю логику Reflection и ручной установки State.
            ctx.Set<T>().Update(entity);

            ctx.SaveChanges();

            Refresh();

            MessageBox.Show("Изменения сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        IsSaveEnabled = true;
        IsSEditEnabled = false;

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
                using var ctx = DatabaseService.CreateContext();
                ctx.Set<T>().Remove(SelectedItem);
                ctx.SaveChanges();

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
        using var ctx = DatabaseService.CreateContext();
        // Используем ToList() для выполнения запроса, а AsNoTracking() для скорости чтения
        var newItems = ctx.Set<T>().AsNoTracking().ToList();

        Items.Clear();
        foreach (var item in newItems)
            Items.Add(item);

        IsReadOnly = true;
        EditableItem = null;
        IsSaveEnabled = false;
        IsSEditEnabled = true;
    }

    public virtual bool CheckFilling()
    {
        throw new NotImplementedException();
    }
}

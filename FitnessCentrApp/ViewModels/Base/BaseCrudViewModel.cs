using DbFirst.Services;
using FitnessCentrApp.ViewModels.Base.Interfaces;
using FitnessCentrApp.ViewModels.Services;
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
    public AsyncRelayCommand EditCommand { get; }
    public RelayCommand CreateCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    protected BaseCrudViewModel()
    {
        Items = new ObservableCollection<T>();

        EditCommand = new AsyncRelayCommand(async _ => await EditItemAsync(), _ => SelectedItem != null);
        CreateCommand = new RelayCommand(_ => CreateNewItem());
        SaveCommand = new AsyncRelayCommand(async _ => await SaveSelectedItemAsync(), _ => EditableItem != null);
        DeleteCommand = new AsyncRelayCommand(async _ => await DeleteItemAsync(), _ => SelectedItem != null);
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

        OnPropertyChanged(nameof(SelectedItem));

        // 3. Откладываем сигнал для View, чтобы он выполнился после завершения отрисовки DataGrid.
        Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background, // Низкий приоритет, ждем завершения основного рендеринга
            new Action(() => BeginEditRequested?.Invoke(SelectedItem))
        );
    }

    protected virtual async Task SaveSelectedItemAsync()
    {
        if (EditableItem is not T entity || entity.Equals(default(T)))
            return;

        try
        {
            using var ctx = DatabaseService.CreateContext();

            ctx.Set<T>().Update(entity);

            ctx.SaveChanges();

            Refresh();

            await MessageBoxService.ShowInfoAsync("Сохранено", "Изменения успешно сохранены!");
        }
        catch (Exception ex)
        {
            await MessageBoxService.ShowErrorAsync("Ошибка при сохранении", ex.Message);

            Refresh();
        }
    }

    protected virtual async Task EditItemAsync()
    {
        if (SelectedItem == null)
        {
            await MessageBoxService.ShowWarningAsync("Редактирование", "Сначала выберите элемент для редактирования.");
            return;
        }

        EditableItem = SelectedItem; // запоминаем, что можно редактировать только этот объект
        IsReadOnly = false; // разрешаем редактирование
        IsSaveEnabled = true;
        IsSEditEnabled = false;

        // сигнал для View, что нужно начать редактирование
        BeginEditRequested?.Invoke(SelectedItem);
    }

    protected virtual async Task DeleteItemAsync()
    {
        if (SelectedItem == null)
            return;
        bool confirm = await MessageBoxService.ShowConfirmAsync(
            "Подтверждение удаления",
            "Вы уверены, что хотите удалить выбранную запись?"
        );

        if(!confirm) return;

        try
        {
            using var ctx = DatabaseService.CreateContext();
            ctx.Set<T>().Remove(SelectedItem);
            await ctx.SaveChangesAsync();

            Refresh();

            await MessageBoxService.ShowInfoAsync("Удалено", "Запись успешно удалена.");
        }
        catch (Exception ex)
        {
            await MessageBoxService.ShowErrorAsync("Ошибка при удалении", ex.Message);

            Refresh();
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

using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.Views.Helpers;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessCentrApp.Views.UserControls.Base;

public partial class CrudView : UserControl
{
    public CrudView()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (DataContext is IEditableViewModel vm)
            {
                vm.BeginEditRequested += OnBeginEditRequested;

                // Подписываемся на изменения IsReadOnly (через PropertyChanged)
                if (vm is INotifyPropertyChanged notifier)
                {
                    notifier.PropertyChanged += (s2, e2) =>
                    {
                        if (e2.PropertyName == "IsReadOnly")
                        {
                            AdjustAllColumnWidths();
                        }
                    };
                }
            }

            // Подключаем обработчик кликов по строкам
            DataGridAuto.LoadingRow += DataGridAuto_LoadingRow;
        };
    }

    private void AdjustAllColumnWidths()
    {
        if (DataGridAuto == null)
            return;

        // Пересчитываем ширину содержимого
        foreach (var column in DataGridAuto.Columns)
        {
            column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
        }

        // Дожидаемся перерисовки, чтобы получить актуальные размеры
        Dispatcher.InvokeAsync(() =>
        {
            foreach (var column in DataGridAuto.Columns)
            {
                // Получаем ширину по содержимому
                var cellWidth = DataGridDisplayConfig.GetActualColumnContentWidth(DataGridAuto, column);

                // Получаем ширину заголовка
                var headerWidth = DataGridDisplayConfig.GetHeaderActualWidth(DataGridAuto, column);

                // Берём максимальную из двух
                var finalWidth = Math.Max(cellWidth, headerWidth);

                // Устанавливаем фиксированную ширину (чтобы не прыгала)
                column.Width = new DataGridLength(finalWidth);
            }
        }, System.Windows.Threading.DispatcherPriority.Background);
    }

    private void OnBeginEditRequested(object item)
    {
        var row = (DataGridRow)DataGridAuto.ItemContainerGenerator.ContainerFromItem(item);
        if (row == null)
        {
            // Если строки ещё не созданы (UI не успел отрисовать), подождём немного
            DataGridAuto.Dispatcher.InvokeAsync(() => OnBeginEditRequested(item));
            return;
        }

        // Находим первую редактируемую колонку (не IsReadOnly)
        // Если первая колонка не редактируемая (например, ID), пропускаем её
        //var editableColumn = DataGridAuto.Columns.ElementAtOrDefault(1); // вторая колонка (индекс 1)
        var editableColumn = DataGridAuto.Columns
            .SkipWhile(c =>
                c.IsReadOnly ||
                c.Header?.ToString()?.Contains("ID", StringComparison.OrdinalIgnoreCase) == true)
            .FirstOrDefault();

        if (editableColumn == null)
            return;

        // Устанавливаем текущую ячейку
        DataGridAuto.CurrentCell = new DataGridCellInfo(item, editableColumn);

        // Начинаем редактирование
        DataGridAuto.BeginEdit();
    }

    private void DataGridAuto_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (DataContext is not BaseCrudViewModel<object> vm)
            return;

        var item = e.Row.Item;

        // Если редактирование запрещено вообще
        if (vm.IsReadOnly)
        {
            e.Cancel = true;
            return;
        }

        // Разрешаем редактировать только тот элемент, который был выбран при нажатии "Редактировать"
        if (!Equals(item, vm.EditableItem))
        {
            e.Cancel = true;
        }
    }

    private void DataGridAuto_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        // Подключаем событие к каждой строке
        e.Row.PreviewMouseLeftButtonDown -= DataGridRow_PreviewMouseLeftButtonDown;
        e.Row.PreviewMouseLeftButtonDown += DataGridRow_PreviewMouseLeftButtonDown;
    }

    private void DataGridRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not IEditableViewModel vm)
            return;

        if (vm.EditableItem == null)
            return;

        if (sender is not DataGridRow row)
            return;

        var clickedItem = row.Item;

        // если клик по другой строке во время редактирования — блокируем
        if (!Equals(clickedItem, vm.EditableItem))
        {
            e.Handled = true;
        }
    }

    private void DataGridAuto_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyDescriptor is not PropertyDescriptor property)
            return;

        var type = e.PropertyType;
        if (DataGridDisplayConfig.ShouldSkipColumn(type))
        {
            e.Cancel = true;
            return;
        }

        DataGridDisplayConfig.ApplyDisplayName(property, e);

        var modelType = property.ComponentType;
        var typeName = modelType.Name;
        var propName = e.PropertyName;

        // Первичный ключ
        if (DataGridDisplayConfig.IsPrimaryKey(typeName, propName))
        {
            e.Column.IsReadOnly = true;
            return;
        }

        // Внешний ключ → создаём ComboBox
        if (propName.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = DataGridDisplayConfig.CreateForeignKeyColumn(modelType, propName, DataContext);
        }
    }
}

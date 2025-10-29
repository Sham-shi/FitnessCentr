using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.Views.Converters;
using FitnessCentrApp.Views.Helpers;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                var cellWidth = DisplayConfig.GetActualColumnContentWidth(DataGridAuto, column);

                // Получаем ширину заголовка
                var headerWidth = DisplayConfig.GetHeaderActualWidth(DataGridAuto, column);

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

        if (ShouldSkipColumn(type))
        {
            e.Cancel = true;
            return;
        }

        ApplyDisplayName(property, e);

        var modelType = property.ComponentType;
        var typeName = modelType.Name;
        var propName = e.PropertyName;

        // Первичный ключ
        if (IsPrimaryKey(typeName, propName))
        {
            e.Column.IsReadOnly = true;
            return;
        }

        // Внешний ключ → создаём ComboBox
        if (propName.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = CreateForeignKeyColumn(modelType, propName, DataContext);
        }
        else
        {
            // Для обычных колонок добавляем валидацию
            if (e.Column is DataGridBoundColumn boundColumn)
            {
                // Создаем binding с валидацией
                var binding = (Binding)boundColumn.Binding;
                binding.ValidatesOnDataErrors = true;
                binding.NotifyOnValidationError = true;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
        }
    }

    private static bool ShouldSkipColumn(Type type)
    {
        return (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            || !DisplayConfig.IsSimpleType(type);
    }

    private static void ApplyDisplayName(PropertyDescriptor property, DataGridAutoGeneratingColumnEventArgs e)
    {
        var displayAttr = property.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttr != null)
        {
            e.Column.Header = displayAttr.Name;
        }
    }

    private static bool IsPrimaryKey(string typeName, string propName)
    {
        return string.Equals(propName, $"{typeName}ID", StringComparison.OrdinalIgnoreCase);
    }

    private static DataGridColumn CreateForeignKeyColumn(Type modelType, string propName, object? dataContext)
    {
        var relatedName = propName.Replace("ID", "");
        var collectionName = DisplayConfig.ToPlural(relatedName);

        // fallback, если коллекция не найдена
        if (dataContext?.GetType().GetProperty(collectionName) == null)
            collectionName = relatedName;

        var navProp = modelType.GetProperty(relatedName);
        if (navProp == null)
            return new DataGridTextColumn { Binding = new Binding(propName), Header = relatedName };

        var relatedType = navProp.PropertyType;
        var displayMember = DisplayConfig.GetDisplayMemberName(relatedType);

        var comboColumn = new DataGridTemplateColumn
        {
            Header = relatedName
        };

        // обычный режим — ID
        var cellTemplate = new DataTemplate();
        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, new Binding(propName));
        cellTemplate.VisualTree = textFactory;
        comboColumn.CellTemplate = cellTemplate;

        // режим редактирования — ComboBox
        var editTemplate = new DataTemplate();
        var comboFactory = new FrameworkElementFactory(typeof(ComboBox));

        var itemsBinding = new Binding($"DataContext.{collectionName}")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1)
        };
        comboFactory.SetBinding(ComboBox.ItemsSourceProperty, itemsBinding);
        comboFactory.SetValue(ComboBox.DisplayMemberPathProperty, displayMember);
        comboFactory.SetValue(ComboBox.SelectedValuePathProperty, $"{relatedName}ID");
        comboFactory.SetBinding(ComboBox.SelectedValueProperty,
            new Binding(propName) { Mode = BindingMode.TwoWay });

        comboFactory.SetBinding(ComboBox.IsEnabledProperty, new Binding("IsReadOnly")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1),
            Converter = new InverseBooleanConverter()
        });

        editTemplate.VisualTree = comboFactory;
        comboColumn.CellEditingTemplate = editTemplate;

        return comboColumn;
    }

    private void DataGridAuto_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit && DataContext is ValidatableViewModel vm)
        {
            // Обновляем валидацию после редактирования ячейки
            var editedItem = e.Row.Item;
            // Здесь можно вызвать валидацию для измененного свойства
        }
    }
}

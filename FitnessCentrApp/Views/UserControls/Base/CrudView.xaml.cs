using DbFirst.Models;
using FitnessCentrApp.ViewModels;
using FitnessCentrApp.ViewModels.Base;
using FitnessCentrApp.ViewModels.Base.Interfaces;
using FitnessCentrApp.Views.Helpers;
using FitnessCentrApp.Views.ValidationRules;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

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
                        if (e2.PropertyName == nameof(vm.IsReadOnly))
                        {
                            if (vm.IsReadOnly)
                            {
                                // Вышли из редактирования — фиксируем и пересчитываем
                                Dispatcher.InvokeAsync(() =>
                                {
                                    foreach (var col in DataGridAuto.Columns)
                                    {
                                        // Снимаем ограничения на MinWidth
                                        col.MinWidth = 0;

                                        // Выполняем корректировку один раз
                                        AdjustAllColumnWidths();
                                    }
                                }, DispatcherPriority.Background);
                            }
                            else
                            {
                                // Вошли в режим редактирования — делаем колонки "плавающими"
                                foreach (var col in DataGridAuto.Columns)
                                {
                                    // запоминаем текущую ширину как минимальную
                                    double currentWidth = col.ActualWidth;

                                    // сбрасываем фиксированную ширину, разрешаем авторасчёт
                                    col.Width = DataGridLength.Auto;

                                    // задаём минимальную ширину, чтобы не схлопывались
                                    col.MinWidth = currentWidth;
                                }
                            }
                        }
                    };
                }
            }

            // Подключаем обработчик кликов по строкам
            DataGridAuto.LoadingRow += DataGridAuto_LoadingRow;

            //DataGridAuto.CellEditEnding -= DataGridAuto_CellEditEnding;
            //DataGridAuto.CellEditEnding += DataGridAuto_CellEditEnding;
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
            //DataGridAuto.Dispatcher.InvokeAsync(() => OnBeginEditRequested(item));
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
        if (DataContext is not IEditableViewModel vm)
            return;

        var item = e.Row.Item;

        var isReadOnlyBinding = BindingOperations.GetBinding(DataGridAuto, DataGrid.IsReadOnlyProperty);
        var isReadOnly = (bool)(DataGridAuto.IsReadOnly);

        // Если редактирование запрещено вообще
        if (isReadOnly)
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

        // Общая стоимость только для чтения
        if (propName.Equals("TotalPrice", StringComparison.OrdinalIgnoreCase))
        {
            e.Column.IsReadOnly = true;
            return;
        }

        // Внешний ключ → создаём ComboBox
        if (propName.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = DataGridDisplayConfig.CreateForeignKeyColumn(modelType, propName);
        }

        // Статус → создаём ComboBox с фиксированным списком значений
        if (propName.Equals("Status", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = DataGridDisplayConfig.CreateStatusColumn(propName);
            DataGridDisplayConfig.ApplyDisplayName(property, e);

            return;
        }

        // Колонка типа услуги → создаём ComboBox с фиксированными значениями
        if (propName.Equals("ServiceType", StringComparison.OrdinalIgnoreCase))
        {
            e.Column = DataGridDisplayConfig.CreateServiceTypeColumn(propName);
            DataGridDisplayConfig.ApplyDisplayName(property, e);

            return;
        }

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateOnly))
        {
            string format = (underlyingType == typeof(DateTime))
                ? "dd.MM.yyyy HH:mm"
                : "dd.MM.yyyy";

            e.Column = DataGridDisplayConfig.CreateDateInputColumn(propName, format, underlyingType);

            DataGridDisplayConfig.ApplyDisplayName(property, e);
            return;
        }

        if (propName.Equals("Phone", StringComparison.OrdinalIgnoreCase))
        {
            var binding = new Binding(propName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidationRules = { new PhoneValidationRule() }
            };
            (e.Column as DataGridTextColumn).Binding = binding;
        }

        if (propName.Equals("Email", StringComparison.OrdinalIgnoreCase))
        {
            var binding = new Binding(propName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidationRules = { new EmailValidationRule() }
            };
            (e.Column as DataGridTextColumn).Binding = binding;
        }

        if (propName.Equals("PhotoPath", StringComparison.OrdinalIgnoreCase))
        {
            // Создаем DataGridTemplateColumn для кастомного отображения/редактирования
            e.Column = DataGridDisplayConfig.CreatePhotoPathColumn(DataContext, propName);

            // Применяем заголовок из атрибута [Display]
            DataGridDisplayConfig.ApplyDisplayName(property, e);

            return; // Завершаем обработку для этого столбца
        }
    }

    //private void DataGridAuto_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    //{
    //    // Нам нужно дождаться применения изменений в источник данных
    //    if (e.EditAction != DataGridEditAction.Commit) return;

    //    // Определяем, какая колонка редактируется
    //    var columnName = e.Column?.SortMemberPath ?? e.Column?.Header?.ToString();
    //    if (string.IsNullOrWhiteSpace(columnName))
    //        return;

    //    // Нас интересуют только ServiceID и SessionsCount
    //    if (!columnName.Equals("ServiceID", StringComparison.OrdinalIgnoreCase) &&
    //        !columnName.Equals("SessionsCount", StringComparison.OrdinalIgnoreCase))
    //        return;

    //    // Попытка получить модель из строки
    //    var item = e.Row.Item;
    //    if (item is not Booking booking)
    //        return;

    //    // Обновляем источник привязки для редактируемого элемента (чтобы значение попало в модель)
    //    if (e.EditingElement is FrameworkElement fe)
    //    {
    //        var binding = BindingOperations.GetBindingExpression(fe, TextBox.TextProperty)
    //                      ?? BindingOperations.GetBindingExpression(fe, System.Windows.Controls.Primitives.Selector.SelectedValueProperty)
    //                      ?? BindingOperations.GetBindingExpression(fe, System.Windows.Controls.Primitives.Selector.SelectedItemProperty);

    //        binding?.UpdateSource();
    //    }

    //    // После выхода из режима редактирования обновляем TotalPrice
    //    if (DataContext is BookingsViewModel vm)
    //    {
    //        Dispatcher.BeginInvoke(new Action(() =>
    //        {
    //            try
    //            {
    //                vm.RecalculateTotalPrice(booking);

    //                // Обновляем UI после коммита
    //                CollectionViewSource.GetDefaultView(DataGridAuto.ItemsSource)?.Refresh();
    //            }
    //            catch { /* игнорируем безопасно */ }
    //        }), DispatcherPriority.Background);
    //    }
    //}
}

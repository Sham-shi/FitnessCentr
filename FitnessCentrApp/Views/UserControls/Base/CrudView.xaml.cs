using FitnessCentrApp.ViewModels.Base;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
            }

            // Подключаем обработчик кликов по строкам
            DataGridAuto.LoadingRow += DataGridAuto_LoadingRow;
        };
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

        // Пытаемся сфокусироваться на активной ячейке
        if (DataGridAuto.CurrentCell.Column?.GetCellContent(row)?.Parent is DataGridCell cell)
        {
            cell.Focus();
        }

        // Перемещаем фокус в редактируемую ячейку
        //row.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next)); // не работает
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

            //// Можно просто молча игнорировать, либо показать сообщение
            //MessageBox.Show(
            //    "Завершите редактирование текущей строки перед переходом к другой.",
            //    "Редактирование заблокировано",
            //    MessageBoxButton.OK,
            //    MessageBoxImage.Information);
        }
    }

    private void DataGridAuto_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var type = e.PropertyType;
        var property = e.PropertyDescriptor as PropertyDescriptor;
        if (property == null) return;

        // Пропускаем коллекции (IEnumerable, кроме string)
        if (typeof(IEnumerable).IsAssignableFrom(e.PropertyType) && type != typeof(string))
        {
            e.Cancel = true;
            return;
        }

        // Пропускаем навигационные свойства (ссылки на другие сущности)
        // то есть типы, которые не являются простыми
        if (!IsSimpleType(type))
        {
            e.Cancel = true;
            return;
        }

        // Если есть атрибут [Display(Name = "...")] — ставим красивое имя
        var displayAttr = property.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttr != null)
        {
            e.Column.Header = displayAttr.Name;
        }

        // Делаем ID только для чтения
        if (e.PropertyName.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
        {
            e.Column.IsReadOnly = true; //e.Cancel = true; // или закомментируй, если хочешь показывать
        }
    }

    // Проверяет, является ли тип простым (int, string, decimal, DateTime и т.п.)
    private bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive
            || underlyingType.IsEnum
            || underlyingType == typeof(string)
            || underlyingType == typeof(decimal)
            || underlyingType == typeof(DateTime)
            || underlyingType == typeof(Guid)
            || underlyingType == typeof(TimeSpan);
    }
}

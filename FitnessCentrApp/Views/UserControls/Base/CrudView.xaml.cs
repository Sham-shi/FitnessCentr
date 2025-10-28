using FitnessCentrApp.ViewModels.Base;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;

namespace FitnessCentrApp.Views.UserControls.Base;

/// <summary>
/// Логика взаимодействия для CrudView.xaml
/// </summary>
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

        // 🔹 Находим первую редактируемую колонку (не IsReadOnly)
        //    Если первая колонка не редактируемая (например, ID), пропускаем её
        var editableColumn = DataGridAuto.Columns
            .SkipWhile(c => c.IsReadOnly || c.Header?.ToString()?.Contains("ID", StringComparison.OrdinalIgnoreCase) == true)
            .FirstOrDefault();

        if (editableColumn == null)
            return;

        // Устанавливаем текущую ячейку
        DataGridAuto.CurrentCell = new DataGridCellInfo(item, editableColumn);

        // Начинаем редактирование
        DataGridAuto.BeginEdit();

        // Перемещаем фокус в редактируемую ячейку
        //row.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next)); // не работает
    }

    private void DataGridAuto_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        var property = e.PropertyDescriptor as System.ComponentModel.PropertyDescriptor;
        if (property == null) return;

        if (typeof(IEnumerable).IsAssignableFrom(e.PropertyType) && e.PropertyType != typeof(string))
        {
            e.Cancel = true;
            return;
        }

        var displayAttr = property.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttr != null)
        {
            e.Column.Header = displayAttr.Name;
        }

        // скрыть ID колонку, если нужно
        if (e.PropertyName.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
        {
            e.Column.IsReadOnly = true; //e.Cancel = true; // или закомментируй, если хочешь показывать
        }
    }

    private void DataGridAuto_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (DataContext is not FitnessCentrApp.ViewModels.Base.BaseCrudViewModel<object> vm)
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
}

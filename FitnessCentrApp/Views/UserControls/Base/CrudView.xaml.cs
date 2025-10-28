using System.Collections;
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

        // Разрешаем редактировать только выбранный элемент
        var item = e.Row.Item;
        if (!Equals(item, vm.SelectedItem) || vm.IsReadOnly)
        {
            e.Cancel = true; // отменяем редактирование
        }
    }
}

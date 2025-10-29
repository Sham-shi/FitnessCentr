using FitnessCentrApp.Views.Converters;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FitnessCentrApp.Views.Helpers;

public static class DataGridDisplayConfig
{
    public static readonly Dictionary<string, string> DisplayOverrides = new()
    {
        { "Branch",  "BranchName" },
        { "Trainer", "FullName" },
        { "Client", "FullName" },
        { "Service", "ServiceName" }
    };

    public static bool IsSimpleType(Type type)
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

    public static string ToPlural(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return word;

        if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            return word[..^1] + "ies";

        if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            return word + "es";

        if (word.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            word.EndsWith("sh", StringComparison.OrdinalIgnoreCase) ||
            word.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            word.EndsWith("z", StringComparison.OrdinalIgnoreCase))
            return word + "es";

        return word + "s";
    }

    public static string GetDisplayMemberName(Type relatedType)
    {
        // Если явно указано — используем
        if (DisplayOverrides.TryGetValue(relatedType.Name, out var prop))
            return prop;

        // Иначе — берём первое строковое свойство
        var firstString = relatedType
            .GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(string));
        if (firstString != null)
            return firstString.Name;

        //В крайнем случае — первое свойство в типе
        return relatedType.GetProperties().FirstOrDefault()?.Name ?? string.Empty;
    }

    public static double GetActualColumnContentWidth(DataGrid grid, DataGridColumn column)
    {
        // Пробуем получить ширину ячейки
        try
        {
            // Генерация визуального контейнера
            if (grid.Items.Count == 0)
                return 0;

            var cell = new DataGridCell();
            column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);

            // Получаем фактическую ширину после рендеринга
            grid.UpdateLayout();
            return column.ActualWidth;
        }
        catch
        {
            return 0;
        }
    }

    public static double GetHeaderActualWidth(DataGrid grid, DataGridColumn column)
    {
        try
        {
            if (grid.ColumnHeaderHeight <= 0)
                return 0;

            var header = column.Header?.ToString();
            if (string.IsNullOrEmpty(header))
                return 0;

            // Примерная оценка ширины текста заголовка
            var formattedText = new FormattedText(
                header,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(grid.FontFamily, grid.FontStyle, grid.FontWeight, grid.FontStretch),
                grid.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(grid).PixelsPerDip);

            // + небольшой отступ
            return formattedText.Width + 20;
        }
        catch
        {
            return 0;
        }
    }

    public static DataGridColumn CreateForeignKeyColumn(Type modelType, string propName, object? dataContext)
    {
        var relatedName = propName.Replace("ID", "");
        var collectionName = DataGridDisplayConfig.ToPlural(relatedName);

        // fallback, если коллекция не найдена
        if (dataContext?.GetType().GetProperty(collectionName) == null)
            collectionName = relatedName;

        var navProp = modelType.GetProperty(relatedName);
        if (navProp == null)
            return new DataGridTextColumn { Binding = new Binding(propName), Header = relatedName };

        var relatedType = navProp.PropertyType;
        var displayMember = DataGridDisplayConfig.GetDisplayMemberName(relatedType);

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

    public static void ApplyDisplayName(PropertyDescriptor property, DataGridAutoGeneratingColumnEventArgs e)
    {
        var displayAttr = property.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttr != null)
        {
            e.Column.Header = displayAttr.Name;
        }
    }

    public static bool ShouldSkipColumn(Type type)
    {
        return (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            || !DataGridDisplayConfig.IsSimpleType(type);
    }

    public static bool IsPrimaryKey(string typeName, string propName)
    {
        return string.Equals(propName, $"{typeName}ID", StringComparison.OrdinalIgnoreCase);
    }
}

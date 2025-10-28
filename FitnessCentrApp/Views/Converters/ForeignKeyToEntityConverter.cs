using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace FitnessCentrApp.Views.Converters;

public class ForeignKeyToEntityConverter : IValueConverter
{
    /// <summary>
    /// Преобразует навигационное свойство (Branch) в ID для ComboBox.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        // Например: Branch.BranchID → 3
        var idProp = value.GetType().GetProperty($"{value.GetType().Name}ID");
        return idProp?.GetValue(value);
    }

    /// <summary>
    /// При выборе нового ID в ComboBox ищет соответствующий объект Branch и возвращает его.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not IEnumerable collection || value == null)
            return null;

        var idValue = value;
        var first = collection.Cast<object>()
            .FirstOrDefault(x =>
            {
                var idProp = x.GetType().GetProperty($"{x.GetType().Name}ID");
                return idProp != null && Equals(idProp.GetValue(x), idValue);
            });

        return first; // например, Branch объект
    }
}

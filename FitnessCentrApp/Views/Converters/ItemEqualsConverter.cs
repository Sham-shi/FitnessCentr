using System.Globalization;
using System.Windows.Data;

namespace FitnessCentrApp.Views.Converters;

public class ItemEqualsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return false;

        var item = values[0];
        var editable = values[1];

        if (item == null && editable == null) return true;
        if (item == null || editable == null) return false;

        return Equals(item, editable);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

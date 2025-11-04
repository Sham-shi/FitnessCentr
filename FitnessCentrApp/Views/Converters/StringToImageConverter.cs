using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FitnessCentrApp.Views.Converters;

public class StringToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
            return GetDefaultImage();

        try
        {
            // 1. Убираем начальные слеши и определяем полный путь
            path = path.TrimStart('/', '\\');
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(appDir, path.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(fullPath))
                return GetDefaultImage();

            // 2. Используем FileStream с BitmapCacheOption.OnLoad, чтобы не блокировать файл
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // Ключевой момент: освободить файл
                image.StreamSource = stream;                  // Загрузка из потока
                image.DecodePixelWidth = 60; // миниатюра, как у вас было
                image.EndInit();
                image.Freeze(); // Рекомендуется
                return image;
            }
        }
        catch
        {
            return GetDefaultImage();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

    private static BitmapImage? GetDefaultImage()
    {
        // Можно вернуть иконку-заглушку или null
        return null;
    }
}
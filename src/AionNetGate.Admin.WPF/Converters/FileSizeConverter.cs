using System.Globalization;
using System.Windows.Data;

namespace AionNetGate.Admin.WPF.Converters;

/// <summary>
/// 文件大小转换器（自动选择单位）
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            if (size < 1024)
                return $"{size} B";

            if (size < 1024 * 1024)
                return $"{size / 1024.0:F2} KB";

            if (size < 1024 * 1024 * 1024)
                return $"{size / 1024.0 / 1024.0:F2} MB";

            return $"{size / 1024.0 / 1024.0 / 1024.0:F2} GB";
        }

        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

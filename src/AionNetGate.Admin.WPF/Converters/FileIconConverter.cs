using System.Globalization;
using System.Windows.Data;

namespace AionNetGate.Admin.WPF.Converters;

/// <summary>
/// 文件图标转换器
/// </summary>
public class FileIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDirectory)
        {
            return isDirectory ? "📁" : "📄";
        }
        return "📄";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

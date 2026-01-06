using System.Globalization;
using System.Windows.Data;

namespace AionNetGate.Admin.WPF.Converters;

/// <summary>
/// 连接时间转显示文本转换器
/// </summary>
public class ConnectionTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime connectedAt)
        {
            var duration = DateTime.Now - connectedAt;

            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays} 天 {duration.Hours} 小时";

            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours} 小时 {duration.Minutes} 分钟";

            if (duration.TotalMinutes >= 1)
                return $"{(int)duration.TotalMinutes} 分钟";

            return $"{(int)duration.TotalSeconds} 秒";
        }

        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HZtest.Resources_资源
{

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 安全获取集合数量
            int count = 0;
            if (value is System.Collections.ICollection coll)
                count = coll.Count;
            else if (value is int i)
                count = i;

            // 参数为 "0" → 无数据时显示；其他情况（如 ">0"）→ 有数据时显示
            return (parameter as string)?.Trim() == "0"
                ? (count == 0 ? Visibility.Visible : Visibility.Collapsed)
                : (count > 0 ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}


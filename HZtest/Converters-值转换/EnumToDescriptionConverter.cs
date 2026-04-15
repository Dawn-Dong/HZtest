using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Data;

namespace HZtest.Converters_值转换
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var type = value.GetType();

            // 获取枚举字段的 Description 特性
            var fieldInfo = type.GetField(value.ToString());
            if (fieldInfo == null) return value.ToString();

            var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();

            return descriptionAttribute?.Description ?? value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

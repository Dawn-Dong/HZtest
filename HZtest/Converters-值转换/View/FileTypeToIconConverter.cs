using HZtest.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace HZtest.Converters.View
{
    #region 数据的翻译官，把程序里的布尔值翻译成用户看得懂的图标。

    // 类型转图标
    public class FileTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileTypeEnum type)
            {
                return type == FileTypeEnum.Directory ? "📁" : "📄";
            }
            return "❓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // 类型转布尔（用于 Visibility）
    public class FileTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileTypeEnum type)
            {
                return type == FileTypeEnum.Directory;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    #endregion
}

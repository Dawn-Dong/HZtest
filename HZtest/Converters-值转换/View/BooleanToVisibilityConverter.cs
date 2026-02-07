using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace HZtest.Converters.View
{
    /// <summary>
    /// 布尔值 ↔ Visibility 转换器（工业HMI标准实现）
    /// 用途：控制加载指示器/提示框等元素的显示隐藏
    /// 特性：
    ///   - true → Visible | false → Collapsed（非Hidden！确保不占布局空间）
    ///   - 支持双向绑定（如需要）
    ///   - 空值/异常安全处理
    ///   - 支持反向转换（通过ConverterParameter="!"）
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 布尔值 → Visibility
        /// 示例: IsLoading=true → Visibility.Visible
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 安全处理：空值/非布尔值返回Collapsed
            if (value is not bool boolValue)
                return Visibility.Collapsed;

            // 支持反向转换：ConverterParameter="!" 时取反
            bool isReversed = parameter is string param && param.Equals("!", StringComparison.OrdinalIgnoreCase);
            bool finalValue = isReversed ? !boolValue : boolValue;

            return finalValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Visibility → 布尔值（用于双向绑定场景）
        /// 示例: Visibility.Visible → true
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility visibility)
                return false;

            // 支持反向转换参数
            bool isReversed = parameter is string param && param.Equals("!", StringComparison.OrdinalIgnoreCase);
            bool result = visibility == Visibility.Visible;

            return isReversed ? !result : result;
        }
    }
}

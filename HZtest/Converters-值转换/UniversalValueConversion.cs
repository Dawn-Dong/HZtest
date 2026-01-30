using HZtest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Data;

namespace HZtest.Converters
{
    public static class UniversalValueConversion
    {

        /// <summary>
        /// 高性能枚举转int（无装箱）
        /// </summary>
        public static int GetEnumNumber<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
        {
            // 底层直接转换，性能最好
            return Unsafe.As<TEnum, int>(ref enumValue);
        }

        /// <summary>
        /// 安全获取Index数组（处理null、int、int[]）
        /// </summary>
        public static int[] GetIndexArray(this RequestItem item)
        {
            if (item.Index == null) return Array.Empty<int>();

            if (item.Index is int single)
                return new[] { single };

            if (item.Index is IEnumerable<int> array)
                return array.ToArray();

            return Array.Empty<int>();
        }

        /// <summary>
        /// 安全获取单个Index值
        /// </summary>
        public static int GetSingleIndex(this RequestItem item, int defaultValue = 0)
        {
            return GetIndexArray(item).FirstOrDefault(defaultValue);
        }

        /// <summary>
        /// 将请求索引和响应值一一对应
        /// </summary>
        public static AxisData ParseAxisData(AxisEnum axis, BaseRequest request, int[][] responseValue)
        {

            var axisName = axis.ToString().ToUpper();

            var result = new Dictionary<int, double>();

            // 遍历每个请求项
            for (int i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var values = responseValue[i]; // 对应的值数组

                // 获取当前item的所有index
                int[] indices = item.GetIndexArray(); // 使用之前提供的扩展方法

                // 确保数量匹配
                if (indices.Length != values.Length)
                {
                    throw new InvalidDataException($"索引数量 ({indices.Length}) 与值数量 ({values.Length}) 不匹配");
                }

                // 建立映射：每个index对应一个值
                for (int j = 0; j < indices.Length; j++)
                {
                    result[indices[j]] = values[j];
                }
            }
            return AxisData.FromDictionary(result, axis);

        }

        /// <summary>
        /// 批量解析轴数据（轴列表、请求项、响应值三者的一一对应）
        /// </summary>
        public static List<AxisData> ParseAxisData(List<AxisEnum> axisList, BaseRequest request, int[][] responseValue)
        {
            // 验证数量一致性
            if (axisList.Count != request.Items.Count || axisList.Count != responseValue.Length)
            {
                throw new ArgumentException("轴列表、请求项和响应值数量不匹配，无法一一对应");
            }

            var result = new List<AxisData>();

            // 按索引遍历（i对应第i个轴）
            for (int i = 0; i < axisList.Count; i++)
            {
                var axis = axisList[i];
                var item = request.Items[i];
                var values = responseValue[i]; // 当前轴的值数组

                // 解析当前轴的映射字典
                var axisMap = ParseSingleAxisData(item, values);

                // 创建AxisData（只传入当前轴的映射）
                result.Add(AxisData.FromDictionary(axisMap, axis));
            }

            return result;
        }

        /// <summary>
        /// 解析单个轴的数据（私有辅助方法）
        /// </summary>
        private static Dictionary<int, double> ParseSingleAxisData(RequestItem item, int[] values)
        {
            var result = new Dictionary<int, double>();
            int[] indices = item.GetIndexArray(); // 获取当前item的索引数组

            if (indices.Length != values.Length)
            {
                throw new InvalidOperationException(
                    $"轴 {item.Path} 索引数量({indices.Length})与值数量({values.Length})不匹配");
            }

            // 建立映射：index → value
            for (int j = 0; j < indices.Length; j++)
            {
                result[indices[j]] = values[j];
            }

            return result;
        }

        /// <summary>
        /// 安全获取枚举的 Description 特性值（支持无效值返回）
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// 从 int 安全转换为枚举的 Description（核心方法！）
        /// </summary>
        public static string GetDescriptionFromInt<TEnum>(int value, string defaultDescription = "未知")
            where TEnum : struct, Enum
        {
            // 检查 int 是否是有效的枚举值
            if (Enum.IsDefined(typeof(TEnum), value))
            {
                var enumValue = (TEnum)(object)value; // 安全转换
                return enumValue.GetDescription();
            }
            return defaultDescription;
        }



    }


    public class IntToBoolConverter : IValueConverter
    {
        // value: 当前绑定的 int (CurrentMode)
        // parameter: RadioButton 对应的整数（通常为字符串形式）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return false;
            if (!int.TryParse(parameter.ToString(), out var paramInt)) return false;
            if (value is int vi)
            {
                return vi == paramInt;
            }
            return false;
        }

        // 当 RadioButton 变为 IsChecked = true 时，将 parameter 作为 int 返回给绑定属性
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool b) || !b) return Binding.DoNothing;
            if (parameter == null) return Binding.DoNothing;
            if (int.TryParse(parameter.ToString(), out var paramInt))
            {
                return paramInt;
            }
            return Binding.DoNothing;
        }
    }
}

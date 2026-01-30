using HZtest.Infrastructure;
using HZtest.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HZtest.Universal
{
    public static class ApiDataParser
    {



        /// <summary>
        /// 核心解析方法：必须传入requestIndices才能正确映射
        /// </summary>
        public static T Parse<T>(int[] requestIndices, int[] dataRow) where T : new()
        {
            var result = new T();
            var properties = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<IndexMappingAttribute>() != null)
                .ToList();

            // ✅ 按顺序映射：requestIndices[i] 对应 dataRow[i]
            for (int i = 0; i < requestIndices.Length; i++)
            {
                int index = requestIndices[i];          // 这是API定义的索引号（如18,19）
                int rawValue = dataRow[i];              // 这是返回的值（按顺序）

                // 找到标记了这个index的Property
                var prop = properties.FirstOrDefault(p =>
                    p.GetCustomAttribute<IndexMappingAttribute>()?.Index == index);

                if (prop != null)
                {
                    prop.SetValue(result, ConvertValue(rawValue, prop.PropertyType));
                }
            }

            return result;
        }

        /// <summary>
        /// 辅助方法：从完整的request解析
        /// </summary>
        public static T ParseFromRequest<T>(BaseRequest request, int[][] responseValue) where T : new()
        {
            // 假设只有一个Item
            var requestIndices = request.Items[0].GetIndexArray();
            var dataRow = responseValue[0];

            return Parse<T>(requestIndices, dataRow);
        }




        /// <summary>
        /// 批量解析（支持多个Items，每个Item有独立索引数组） 未测试
        /// </summary>
        public static List<T> ParseList<T>(BaseRequest request, int[][] responseValue) where T : new()
        {
            // ✅ 验证：Items数量必须等于responseValue行数
            if (request.Items.Count != responseValue.Length)
            {
                throw new ArgumentException(
                    $"请求Items数量({request.Items.Count})与响应值行数({responseValue.Length})不匹配");
            }

            var results = new List<T>();

            // 遍历每个Item（i对应第i个实体）
            for (int i = 0; i < request.Items.Count; i++)
            {
                // ✅ 获取当前Item的索引数组（顺序关键）
                var requestIndices = request.Items[i].GetIndexArray();

                // ✅ 获取当前Item对应的响应行
                var dataRow = responseValue[i];

                // ✅ 调用单个解析方法
                var entity = Parse<T>(requestIndices, dataRow);
                results.Add(entity);
            }

            return results;
        }

        /// <summary>
        /// 类型转换辅助方法
        /// </summary>
        private static object ConvertValue(int rawValue, Type targetType)
        {
            if (targetType == typeof(bool))
                return rawValue != 0;  // 0=false, 其他=true

            if (targetType == typeof(double))
                return rawValue * 0.0001;  // 你的转换逻辑

            if (targetType == typeof(int))
                return rawValue;

            if (targetType == typeof(string))
                return rawValue.ToString();

            return rawValue;
        }
    }
}

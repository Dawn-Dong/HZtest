using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HZtest.Models
{
    /// <summary>
    /// 轴数据类
    /// </summary>
    public class AxisData
    {
        /// <summary>
        /// 轴名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 轴编号
        /// </summary>
        public int Code { get; set; } = -1;

        /// <summary>
        /// 轴的实际进给数据
        /// </summary>
        public double ActualFeedRate { get; set; } = -1;

        /// <summary>
        /// 轴的剩余进给数据
        /// </summary>
        public double RemainingFeed { get; set; } = -1;


        /// <summary>
        /// 从解析结果创建AxisData（核心方法）
        /// </summary>
        public static AxisData FromDictionary(Dictionary<int, double> dataMap, AxisEnum axis)
        {
            return new AxisData
            {
                Name = axis.GetDescription(), // 使用扩展方法获取枚举描述
                Code = (int)axis,
                ActualFeedRate = dataMap.GetValueOrDefault(AxisDataIndices.ActualFeed, -1),
                RemainingFeed = dataMap.GetValueOrDefault(AxisDataIndices.RemainingFeed, -1)
            };
        }
    }

    /// <summary>
    /// 枚举扩展
    /// </summary>
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attr = field?.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            return attr?.Description ?? enumValue.ToString();
        }

    }
    /// <summary>
    /// 轴数据常量（实际进给=6，剩余进给=19）
    /// </summary>
    public static class AxisDataIndices
    {
        /// <summary>
        /// 实际进给
        /// </summary>
        public const int ActualFeed = 6;    // 实际进给
        /// <summary>
        /// 剩余进给
        /// </summary>
        public const int RemainingFeed = 19; // 剩余进给
    }


}




using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Infrastructure_基础设施
{
    /// <summary>
    /// 寄存器信息特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RegisterInfoAttribute : Attribute
    {
        /// <summary>
        /// 寄存器最大可写地址
        /// </summary>
        public int MaximumWritableAddress { get; set; }

        /// <summary>
        /// 寄存器位宽（8或16）
        /// </summary>
        public int BitWidth { get; }

        /// <summary>
        /// 最大可写值（如果为-1则自动计算为 (1 << BitWidth) - 1）
        /// </summary>
        public int MaxValue { get; }


        public RegisterInfoAttribute(int bitWidth, int maximumWritableAddress, int maxValue = -1)
        {
            BitWidth = bitWidth;
            MaximumWritableAddress = maximumWritableAddress;
            MaxValue = maxValue;

        }

    }
}

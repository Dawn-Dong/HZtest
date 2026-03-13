using HZtest.Infrastructure_基础设施;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HZtest.Models.Response
{
    public class RegisterOperationResponse
    {
        /// <summary>
        /// 十进制值
        /// </summary>
        public int? DecimalValue { get; set; }

        /// <summary>
        /// 二进制数
        /// </summary>
        public string BinaryValue { get; set; } = string.Empty;

        /// <summary>
        /// 转换为十进制值根据特性自动处理8位或16位寄存器
        /// </summary>
        /// <param name="registerType"></param>
        /// <exception cref="InvalidOperationException"></exception>

        public void ConvertToDecimalValue(RegisterTypeEnum registerType)
        {
            try
            {

                if (DecimalValue == null)
                {
                    throw new ArgumentNullException(nameof(DecimalValue), "转换的十进制值不应为空");
                }
                // 获取寄存器信息
                var registerInfo = GetRegisterInfo(registerType);

                var transformationResult = registerInfo.BitWidth switch
                {
                    8 => ConvertTo8_BitBinaryValue(),
                    16 => ConvertTo16_BitBinaryValue(),
                    _ => throw new ArgumentException($"寄存器 {registerType} 的位宽不支持: {registerInfo.BitWidth} 位"),
                };


            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"无法转换二进制值: {BinaryValue} 为十进制值", ex);
            }

        }



        /// <summary>
        /// 转成8位二进制字符串，格式为 "0000 0000"
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        private bool ConvertTo8_BitBinaryValue()
        {
            try
            {
                if (DecimalValue == null)
                {
                    throw new ArgumentNullException(nameof(DecimalValue), "转换的十进制不应为空");
                }
                int value = DecimalValue.Value;

                BinaryValue = string.Join(" ", Convert.ToString(value, 2).PadLeft(8, '0').ToCharArray());
                return true;

            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <summary>
        /// 转成16位二进制字符串，格式为 "0000 0000 0000 0000"
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        private bool ConvertTo16_BitBinaryValue()
        {
            try
            {
                if (DecimalValue == null)
                {
                    throw new ArgumentNullException(nameof(DecimalValue), "转换的十进制不应为空");
                }
                int value = DecimalValue.Value;

                BinaryValue = string.Join(" ", Convert.ToString(value, 2).PadLeft(16, '0').ToCharArray());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private RegisterInfoAttribute GetRegisterInfo(RegisterTypeEnum register)
        {
            var fieldInfo = typeof(RegisterTypeEnum).GetField(register.ToString());
            return fieldInfo?.GetCustomAttribute<RegisterInfoAttribute>()
                ?? throw new ArgumentException($"Register {register} has no RegisterInfoAttribute.");
        }


    }


    /// <summary>
    /// 单个寄存器位写入结果
    /// </summary>
    public class RegisterWriteResultResponse
    {
        /// <summary>寄存器类型（如 G、M、X、Y）</summary>
        public RegisterTypeEnum RegisterType { get; set; }

        /// <summary>寄存器地址</summary>
        public int RegisterAddress { get; set; }

        /// <summary>位位置（偏移量）</summary>
        public int? BitPosition { get; set; }

        /// <summary>写入的值（0或1）</summary>
        public int WriteValue { get; set; }

        /// <summary>是否成功</summary>
        public bool Success { get; set; }

        /// <summary>结果消息</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>友好描述</summary>
        public string Description => BitPosition.HasValue
            ? $"{RegisterType} | {RegisterAddress} 第 {BitPosition} 位 = {WriteValue}"
            : $"{RegisterType}{RegisterAddress} = {WriteValue}";
    }


}

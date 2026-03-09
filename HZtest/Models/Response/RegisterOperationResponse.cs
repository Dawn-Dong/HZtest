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
}

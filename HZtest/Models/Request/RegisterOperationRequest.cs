using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Request
{
    /// <summary>
    /// 寄存器操作请求
    /// </summary>
    public class RegisterOperationRequest
    {
        /// <summary>
        /// 操作的寄存器
        /// </summary>
        public RegisterTypeEnum RegisterType { get; set; } = RegisterTypeEnum.Error;

        /// <summary>
        /// 寄存器地址
        /// </summary>
        public int RegisterAddress { get; set; }

        /// <summary>
        /// 寄存器写入值
        /// </summary>
        public int RegisterWriteValue { get; set; }

        /// <summary>
        /// 寄存器偏移量
        /// </summary>
        public int? RegisterOffset { get; set; }


    }
}

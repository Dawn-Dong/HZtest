using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Ribbon;

namespace HZtest.Models.Request
{
    /// <summary>
    /// 用户变量读写请求
    /// </summary>
    public class UserVariablesReadWriteRequest
    {
        /// <summary>
        /// 操作地址号
        /// </summary>
        public int OperationAddressNumber { get; set; }

        /// <summary>
        /// 写入值
        /// </summary>
        public double? WriteValue { get; set; } = null;

        /// <summary>
        /// 操作类型 1：读 2：写
        /// </summary>
        public UserVariableOperationTypeEnum OperationType { get; set; } = UserVariableOperationTypeEnum.Read;

    }
}

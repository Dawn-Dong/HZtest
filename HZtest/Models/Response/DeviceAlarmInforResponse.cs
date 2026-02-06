using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Response
{
    /// <summary>
    /// 设备报警信息响应模型
    /// </summary>
    public class DeviceAlarmInforResponse
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// 报警文本
        /// </summary>
        public string Text { get; set; } = string.Empty;


    }
}

using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Response
{
    /// <summary>
    /// 订单详情响应模型
    /// </summary>
    public class OrderDetailsResponse
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; } = string.Empty;

        /// <summary>
        /// 工件序列号
        /// </summary>
        public int SerialNumber { get; set; } = 0;

        /// <summary>
        /// 下单数量
        /// </summary>
        public int OrderQuantity { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 订单详情状态
        /// </summary>
        public OrderDetailsEnum OrderDetailsType { get; set; } = OrderDetailsEnum.Error;

        /// <summary>
        /// 备注信息 这里一般是错误信息
        /// </summary>
        public string Remark { get; set; } = string.Empty;

    }
}

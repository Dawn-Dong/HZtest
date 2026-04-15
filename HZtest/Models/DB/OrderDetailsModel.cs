using HZtest.Models.BaseClass;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.DB
{
    /// <summary>
    /// 订单详情模型
    /// </summary>
    [Tenant("SQLite")]
    [SugarTable("OrderDetails")]
    public class OrderDetailsModel : ModelBaseClass
    {
        /// <summary>
        /// 外键，关联订单管理表
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long FK_OrderManagementId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 订单详情状态
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public OrderDetailsEnum OrderDetailsType { get; set; } = OrderDetailsEnum.Error;

        /// <summary>
        /// 序列号
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int SerialNumber { get; set; } = 0;
        /// <summary>
        /// 备注信息 这里一般是错误信息
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500)]
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 导航查询订单管理表
        /// </summary>
        [Navigate(NavigateType.ManyToOne, nameof(FK_OrderManagementId), nameof(OrderManagementModel.Id))]
        public OrderManagementModel OrderManagement { get; set; }
        /// <summary>
        /// 订单持续时间
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public TimeSpan? Duration => EndTime - StartTime;

    }
}

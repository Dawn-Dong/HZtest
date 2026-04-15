using HZtest.Models.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Event
{
    // 基础事件参数
    public class OrderEventArgs : EventArgs
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 事件发生时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 订单启动事件请求参数
    /// </summary>
    public class OrderStartRequestEventArgs : OrderEventArgs
    {
        /// <summary>
        /// 订单信息
        /// </summary>
        public OrderManagementModel OrderManagementModel { get; set; }
    }
    /// <summary>
    /// 开启前校验事件请求参数
    /// </summary>
    public class OrderEnablePreValidationRequestEventArgs : OrderEventArgs
    {
        /// <summary>
        /// 订单信息
        /// </summary>
        public OrderManagementModel OrderManagementModel { get; set; }
    }
    /// <summary>
    /// 开启前校验事件响应参数
    /// </summary>
    public class OrderEnablePreValidationResponseEventArgs : OrderEventArgs
    {
        /// <summary>
        /// 校验结果
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string ValidationMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 关闭订单事件参数
    /// </summary>
    public class OrderCloseEventArgs : OrderEventArgs
    {
        /// <summary>
        /// 是否是手动关闭订单（true：手动关闭，false：自动关闭）
        /// </summary>
        public bool isManualClose { get; set; } = false;

        public OrderManagementModel OrderManagementModel { get; set; }

    }

}

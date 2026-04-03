using HZtest.Models.BaseClass;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HZtest.Models.DB
{

    /// <summary>
    /// 订单管理模型
    /// </summary>
    [Tenant("SQLite")]
    [SugarTable("OrderManagement")]
    public class OrderManagementModel : ModelBaseClass
    {

        /// <summary>
        /// 订单编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string OrderCode
        {
            get;
            set;
        }


        /// <summary>
        /// 订单数量
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int OrderQuatity
        {
            get;
            set;
        }

        /// <summary>
        /// 加工件名称
        /// </summary>
        [SugarColumn(Length = 150, IsNullable = false)]
        public string ProcessingPartName
        {
            get;
            set;
        }

        /// <summary>
        /// 加工件文件路径
        /// </summary>
        [SugarColumn(Length = 300, IsNullable = false)]
        public string ProcessingPartFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public OrderStateEnum OrderState
        {
            get;
            set;
        }

        /// <summary>
        /// 数据验证方法，确保订单数据的有效性
        /// </summary>
        /// <returns></returns>
        public VerifyValueResponse VerifyData()
        {
            if (string.IsNullOrWhiteSpace(OrderCode))
                return VerifyValueResponse.Fail("订单编号不能为空");
            if (OrderQuatity <= 0)
                return VerifyValueResponse.Fail("订单数量必须大于0");
            if (string.IsNullOrWhiteSpace(ProcessingPartName))
                return VerifyValueResponse.Fail("加工件名称不能为空");
            if (string.IsNullOrWhiteSpace(ProcessingPartFilePath))
                return VerifyValueResponse.Fail("加工件文件必须选择");
            return VerifyValueResponse.Success();
        }

    }
}

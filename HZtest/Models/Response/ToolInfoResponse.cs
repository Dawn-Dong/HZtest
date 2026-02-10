using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Response
{
    /// <summary>
    /// 获取刀具信息操作响应模型
    /// </summary>
    public class ToolInfoResponse
    {
        /// <summary>
        /// 长度补偿值
        /// </summary>
        public double LengthCompensation { get; set; }

        /// <summary>
        /// 半径补偿值
        /// </summary>
        public double RadiusCompensation { get; set; }

        /// <summary>
        /// 长度磨损值
        /// </summary>
        public double LengthWear { get; set; }

        /// <summary>
        /// 半径磨损值
        /// </summary>
        public double RadiusWear { get; set; }

        /// <summary>
        /// 综合寿命
        /// </summary>
        public double ComprehensiveLifespan { get; set; }

    }
}

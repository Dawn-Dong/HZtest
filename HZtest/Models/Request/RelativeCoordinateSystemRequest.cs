using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Request
{
    /// <summary>
    /// 相对坐标系请求
    /// </summary>
    public class RelativeCoordinateSystemRequest
    {
        /// <summary>
        /// 操作坐标系编号
        /// </summary>
        public int OperatingCoordinateSystemId { get; set; }
        /// <summary>
        /// X轴值
        /// </summary>
        public double XAxisValue { get; set; }
        /// <summary>
        /// Y轴值
        /// </summary>
        public double YAxisValue { get; set; }
        /// <summary>
        /// Z轴值
        /// </summary>
        public double ZAxisValue { get; set; }
        /// <summary>
        /// B轴值
        /// </summary>
        public double BAxisValue { get; set; }
        /// <summary>
        /// C轴值
        /// </summary>
        public double CAxisValue { get; set; }

    }
}

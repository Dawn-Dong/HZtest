using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Response
{
    /// <summary>
    /// 坐标系响应
    /// </summary>
    public class RelativeCoordinateSystemResponse
    {
        /// <summary>
        /// X轴坐标值
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Y轴坐标值
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Z轴坐标值
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// B轴坐标值
        /// </summary>
        public double B { get; set; }
        /// <summary>
        /// C轴坐标值
        /// </summary>
        public double C { get; set; }


    }
}

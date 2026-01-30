using HZtest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.DTO
{
    /// <summary>
    /// 设置启动停止状态用
    /// </summary>
    public class StartStopStateDto
    {
        /// <summary>
        /// 给谁设置状态
        /// </summary>

        public StartStopStatusEnum SetStatus { get; set; }

        /// <summary>
        /// 设置成什么状态
        /// </summary>
        public bool State { get; set; } = false;


    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Constants
{
    /// <summary>
    /// 寄存器常量
    /// </summary>
   public class RegisterConstants
    {
        /// <summary>
        /// 工作模式控制、启动保持状态
        /// </summary>
        public const int RuOrStartPause = 2560;

    }

    /// <summary>
    /// 寄存器偏移常量
    /// </summary>
    public class RegisterOffsetConstants
    {

        /// <summary>
        /// 进给保持标志偏移
        /// </summary>
        public const int FeedHold = 4;

        /// <summary>
        /// 循环启动标志偏移
        /// </summary>
        public const int CycleStart = 5;
    }

}

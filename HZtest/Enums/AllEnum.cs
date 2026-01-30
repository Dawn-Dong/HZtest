using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HZtest.Models
{
    /// <summary>
    /// 轴枚举
    /// </summary>
    public enum AxisEnum
    {
        /// <summary>
        /// 进给轴X
        /// </summary>
        [Description("进给轴X")]
        X = 0,
        /// <summary>
        /// 进给轴Y
        /// </summary>
        [Description("进给轴Y")]
        Y = 1,
        /// <summary>
        /// 进给轴Z
        /// </summary>
        [Description("进给轴Z")]
        Z = 2,
        /// <summary>
        /// 旋转轴A
        /// </summary>
        [Description("旋转轴A")]
        A = 3,
        /// <summary>
        /// 旋转轴B
        /// </summary>
        [Description("旋转轴B")]
        B = 4,
        /// <summary>
        /// 旋转轴C
        /// </summary>
        [Description("旋转轴C")]
        C = 5,

    }


    /// <summary>
    /// 设置启停状态枚举
    /// </summary>
    public enum StartStopStatusEnum
    {

        /// <summary>
        /// 启动
        /// </summary>
        [Description("启动")]
        Start = 0,

        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        Stop = 1,

    }

    /// <summary>
    /// 设备运行模式枚举
    /// </summary>
    public enum DevOperationModeEnum 
    {
        /// <summary>
        /// 复位
        /// </summary>
        [Description("复位")]
        Reset = 0,

        /// <summary>
        /// 自动
        /// </summary>
        [Description("自动")]
        Auto = 1,

        /// <summary>
        /// 点动（对应手动）
        /// </summary>
        [Description("手动")]
        Jog = 2,

        /// <summary>
        /// 单步（对应手轮）
        /// </summary>
        [Description("手轮")]
        Step = 3,

        /// <summary>
        /// 机床面板
        /// </summary>
        [Description("机床面板")]
        Mpg = 4,

        /// <summary>
        /// 回零（对应回参考点）
        /// </summary>
        [Description("回参考点")]
        Home = 5,

        /// <summary>
        /// 程序PLC控制
        /// </summary>
        [Description("PMC控制")]
        PMC =6,

        /// <summary>
        /// 手动数据输入/单段运行
        /// </summary>
        [Description("手动数据输入")]
        MDI_SBL = 7,
    }

}

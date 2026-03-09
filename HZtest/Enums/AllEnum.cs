using HZtest.Infrastructure_基础设施;
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
        PMC = 6,

        /// <summary>
        /// 手动数据输入/单段运行
        /// </summary>
        [Description("手动数据输入")]
        MDI_SBL = 7,
    }
    /// <summary>
    /// 文件类型枚举
    /// </summary>
    public enum FileTypeEnum
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        [Description("未知类型")]
        Error = -1,

        /// <summary>
        /// 文件
        /// </summary>
        [Description("文件")]
        File = 0,

        /// <summary>
        /// 目录
        /// </summary>
        [Description("目录")]
        Directory = 1,
    }
    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum DeviceStateEnum
    {
        /// <summary>
        /// 错误模式
        /// </summary>
        [Description("未知模式错误")]
        Error = -1,

        /// <summary>
        /// 空闲
        /// </summary>
        [Description("空闲")]
        Free = 1,
        /// <summary>
        /// 保持状态
        /// </summary>
        [Description("保持状态")]
        Holding = 2,

        /// <summary>
        /// 运行状态
        /// </summary>
        [Description("运行状态")]
        Running = 3,
    }
    /// <summary>
    /// 用户变量操作类型
    /// </summary>
    public enum UserVariableOperationTypeEnum
    {
        /// <summary>
        /// 错误类型
        /// </summary>
        [Description("未知类型错误")]
        Error = -1,
        /// <summary>
        /// 读取操作
        /// </summary>
        [Description("读取操作")]
        Read = 1,
        /// <summary>
        /// 写入操作
        /// </summary>
        [Description("写入操作")]
        Write = 2,

    }

    /// <summary>
    /// 坐标系枚举
    /// </summary>
    public enum CoordinateSystemEnum
    {
        /// <summary>
        /// 未知错误坐标系
        /// </summary>
        [Description("未知错误坐标系")]
        Error = -1,
        /// <summary>
        /// 外部偏置坐标系
        /// </summary>
        [Description("外部偏置坐标系")]
        ExternalBias = 0,
        /// <summary>
        /// G54坐标系
        /// </summary>
        [Description("G54坐标系")]
        G54 = 1,
        /// <summary>
        /// G54偏置坐标系
        /// </summary>
        [Description("G54偏置坐标系")]
        G54Bias = 2,
        /// <summary>
        /// G55坐标系
        /// </summary>
        [Description("G55坐标系")]
        G55 = 3,
        /// <summary>
        /// G55偏置坐标系
        /// </summary>
        [Description("G55偏置坐标系")]
        G55Bias = 4,
        /// <summary>
        /// G56坐标系
        /// </summary>
        [Description("G56坐标系")]
        G56 = 5,
        /// <summary>
        /// G56偏置坐标系
        /// </summary>
        [Description("G56偏置坐标系")]
        G56Bias = 6,
        /// <summary>
        /// G57坐标系
        /// </summary>
        [Description("G57坐标系")]
        G57 = 7,
        /// <summary>
        /// G57偏置坐标系
        /// </summary>
        [Description("G57偏置坐标系")]
        G57Bias = 8,
        /// <summary>
        /// G58坐标系
        /// </summary>
        [Description("G58坐标系")]
        G58 = 9,
        /// <summary>
        /// G58偏置坐标系
        /// </summary>
        [Description("G58偏置坐标系")]
        G58Bias = 10,

        /// <summary>
        /// G59坐标系
        /// </summary>
        [Description("G59坐标系")]
        G59 = 11,
        /// <summary>
        /// G59偏置坐标系
        /// </summary>
        [Description("G59偏置坐标系")]
        G59Bias = 12,
        /// <summary>
        /// 相对坐标系
        /// </summary>
        [Description("相对坐标系")]
        Relative = 13,

    }

    /// <summary>
    /// 寄存器类型枚举
    /// </summary>
    public enum RegisterTypeEnum
    {

        /// <summary>
        /// 未知寄存器
        /// </summary>
        [Description("未知寄存器")]
        Error = -1,

        /// <summary>
        /// X寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 8, maximumWritableAddress: 511)]
        [Description("X寄存器")]
        X = 1,

        /// <summary>
        /// Y寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 8, maximumWritableAddress: 511)]
        [Description("Y寄存器")]
        Y = 2,

        /// <summary>
        /// R寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 8, maximumWritableAddress: 2047)]
        [Description("R寄存器")]
        R = 3,

        /// <summary>
        /// G寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 16, maximumWritableAddress: 3727)]
        [Description("G寄存器")]
        G = 4,

        /// <summary>
        /// B寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 16, maximumWritableAddress: 1721)]
        [Description("B寄存器")]
        B = 5,

        /// <summary>
        /// I寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 8, maximumWritableAddress: 127)]
        [Description("I寄存器")]
        I = 6,

        /// <summary>
        /// Q寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 8, maximumWritableAddress: 127)]
        [Description("Q寄存器")]
        Q = 7,

        /// <summary>
        /// W寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 16, maximumWritableAddress: 255)]
        [Description("W寄存器")]
        W = 8,

        /// <summary>
        /// D寄存器
        /// </summary>
        [RegisterInfo(bitWidth: 16, maximumWritableAddress: 255)]
        [Description("D寄存器")]
        D = 9,
    }

}

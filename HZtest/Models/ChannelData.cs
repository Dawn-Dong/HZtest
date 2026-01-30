using HZtest.Infrastructure;
using HZtest.Universal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HZtest.Models
{
    /// <summary>
    /// 获取启动停止状态
    /// </summary>
    public class StartStopState
    {
        /// <summary>
        /// 循环启动状态
        /// </summary>
        [IndexMapping(ChannelDataIndices.CycleStart, "循环启动状态")]
        public bool CycleStart { get; set; } = false;
        /// <summary>
        /// 进给保持状态
        /// </summary>
        [IndexMapping(ChannelDataIndices.FeedHold, "进给保持状态")]
        public bool FeedHold { get; set; } = false;


    }

    /// <summary>
    /// 获取主轴实际速度
    /// </summary>
    public class ActualSpindleSpeed
    {
        /// <summary>
        /// 主轴实际速度
        /// </summary>
        [IndexMapping(ChannelDataIndices.ActualSpindleSpeed, "主轴实际速度")]
        public int Speed { get; set; } = 0;


        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("value")]
        public int[][][] Value { get; set; } = new int[0][][]; // 三层嵌套数组！关键！




    }


    public class OperationMode
    {
        /// <summary>
        /// 当前模式
        /// </summary>
        [IndexMapping(ChannelDataIndices.Mode, "当前设备模式")]
        public int CurrentMode { get; set; } = 0;
    }





    /// <summary>
    /// 通道数据常量
    /// </summary>
    public static class ChannelDataIndices
    {
        /// <summary>
        /// 运行模式 0：复位、1：自动、2：点动 （对应手动） 、3：单步（对应手轮）、4：机床面板、5：回零（对应回参考点）、6：程序控制、7：手动数据
        /// </summary>
        public const int Mode = 16;
        /// <summary>
        /// 循环启动 0 不启动 1 启动
        /// </summary>
        public const int CycleStart = 18;

        /// <summary>
        /// 进给保持 0 不保持 1 保持
        /// </summary>
        public const int FeedHold = 19;

        /// <summary>
        ///主轴实际速度  有四个值未搞懂具体含义
        /// </summary>
        public const int ActualSpindleSpeed = 48;




    }




}

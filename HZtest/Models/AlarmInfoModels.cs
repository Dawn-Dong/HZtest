using HZtest.Models.Response;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models
{


    /// <summary>
    /// 报警信息模型
    /// </summary>
    public class AlarmInfoModels
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        public string AlarmCode { get; set; }

        /// <summary>
        /// 报警类型
        /// </summary>
        public AlarmTypeEnum AlarmType { get; set; } = AlarmTypeEnum.Error;

        /// <summary>
        /// 报警级别
        /// </summary>
        public AlarmLevelEnum AlarmLevel { get; set; } = AlarmLevelEnum.Error;

        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime AlarmTime { get; set; }

        /// <summary>
        /// 报警内容
        /// </summary>
        public string AlarmContent { get; set; } = string.Empty;
    }
    public class AlarmCompareResult
    {
        /// <summary>
        /// 是否有变化
        /// </summary>
        public bool HasChanged { get; set; }

        /// <summary>
        /// 新增的报警（在新列表中存在，旧列表中不存在）
        /// </summary>
        public List<DeviceAlarmInforResponse> Added { get; set; } = new();

        /// <summary>
        /// 移除的报警（在旧列表中存在，新列表中不存在）
        /// </summary>
        public List<DeviceAlarmInforResponse> Removed { get; set; } = new();

        /// <summary>
        /// 保持不变的报警
        /// </summary>
        public List<DeviceAlarmInforResponse> Unchanged { get; set; } = new();
    }





}

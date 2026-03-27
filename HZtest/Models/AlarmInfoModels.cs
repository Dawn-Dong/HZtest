using HZtest.Models.BaseClass;
using HZtest.Models.Response;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace HZtest.Models
{


    /// <summary>
    /// 报警信息模型
    /// </summary>
    [Tenant("SQLite")]
    [SugarTable("AlarmInfoModels")]
    public class AlarmInfoModels : ModelBaseClass
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string AlarmCode { get; set; } = string.Empty;

        /// <summary>
        /// 报警类型
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public AlarmTypeEnum AlarmType { get; set; } = AlarmTypeEnum.Error;

        /// <summary>
        /// 报警级别  IsAutoPasing为false时，AlarmLevel由华中提供的规则解析，IsAutoPasing为true时，AlarmLevel由配置的报警信息解析
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public AlarmLevelEnum AlarmLevel { get; set; } = AlarmLevelEnum.Error;

        /// <summary>
        /// true自动根据华中提供的规则解析 false不自动解析，根据配置的报警信息解析
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsAutoPasing { get; set; } = false;
        /// <summary>
        /// 报警时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime AlarmTime { get; set; }

        /// <summary>
        /// 报警内容
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public string AlarmContent { get; set; } = string.Empty;
    }


    /// <summary>
    /// 报警变化枚举
    /// </summary>
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
    /// <summary>
    /// 
    /// </summary>
    public class AlarmDisplayItem
    {
        public string Text { get; set; }
        public AlarmLevelEnum Level { get; set; }
        public bool IsAutoParsing { get; set; }

        // 颜色转换器（也可以写在XAML里）
        public Color LevelColor => Level switch
        {
            AlarmLevelEnum.ErrorLevel => Colors.Red,     // 红色
            AlarmLevelEnum.MessageLevel => Colors.Orange,    // 橙色                                                      
            // AlarmLevelEnum.MessageLevel => "#008000",       // 绿色
            _ => Colors.Black
        };

        public string SourceTag => IsAutoParsing ? "[手动]" : "[自动]";
        public string SourceColor => IsAutoParsing ? "#0066CC" : "#800080"; // 蓝/紫
    }







}

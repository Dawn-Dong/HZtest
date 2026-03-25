using HZtest.Models.BaseClass;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models
{
    /// <summary>
    /// 报警管理配置模型 - 用于存储报警管理相关的配置信息
    /// </summary>
    [Tenant("SQLite")]
    [SugarTable("AlarmManagementConfig")]
    public class AlarmManagementConfigModel : ModelBaseClass
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string AlarmCode { get; set; } = string.Empty;

        /// <summary>
        /// 报警级别
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public AlarmLevelEnum AlarmLevel { get; set; } = AlarmLevelEnum.Error;
    }
}

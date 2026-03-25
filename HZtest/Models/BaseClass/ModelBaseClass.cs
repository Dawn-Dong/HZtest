using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;

namespace HZtest.Models.BaseClass
{
    /// <summary>
    /// 基类 - 所有存在数据库的基类
    /// </summary>
    public class ModelBaseClass
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ModifyTime { get; set; } = null;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime CreateTime { get; set; }

    }
}

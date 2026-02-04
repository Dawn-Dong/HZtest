using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Response
{
    /// <summary>
    /// 获取文件详情信息操作响应模型
    /// </summary>
    public class FileOperationsResponse
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// 文件大小KB
        /// </summary>
        public int Size { get; set; } = 0;
        /// <summary>
        /// 文件修改时间
        /// </summary>
        public string ChangeTime { get; set; } = string.Empty;

    }
}

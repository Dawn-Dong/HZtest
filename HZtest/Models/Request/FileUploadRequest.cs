using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.Request
{
    /// <summary>
    /// 文件上传请求模型
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// 本地文件路径
        /// </summary>
        public string LocalFilePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 位于数控系统那个文件夹路径下
        /// </summary>
        public string LocatedPath { get; set; } = string.Empty;

        ///// <summary>
        ///// 记录用户在弹窗中的操作，确认上传为 true，取消上传为 false
        ///// </summary>
        //public bool UserOperation { get; set; } = false;
    }
}

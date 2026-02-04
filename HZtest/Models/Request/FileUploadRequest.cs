using System;
using System.Collections.Generic;
using System.IO;
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


        /// <summary>
        /// 上传操作类型（固定为 "set_value"，保留扩展性）
        /// </summary>
        public string Operation { get; set; } = "set_value";


        /// <summary>
        /// 文件上传固定参数（保留扩展性）
        /// </summary>
        public string Path { get; set; } = "/MACHINE/CONTROLLER/FILE";


        ///// <summary>
        ///// 记录用户在弹窗中的操作，确认上传为 true，取消上传为 false
        ///// </summary>
        //public bool UserOperation { get; set; } = false;

        /// <summary>
        /// 获取完整的数控系统文件路径（LocatedPath + FileName）
        /// </summary>
        public string GetTargetFilePath()
        {
            if (string.IsNullOrEmpty(LocatedPath))
                return FileName;

            return LocatedPath.EndsWith("/")
                ? $"{LocatedPath}{FileName}"
                : $"{LocatedPath}/{FileName}";
        }

        /// <summary>
        /// 验证请求参数是否有效
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(LocalFilePath) || !File.Exists(LocalFilePath))
            {
                errorMessage = "本地文件不存在或路径无效";
                return false;
            }

            if (string.IsNullOrWhiteSpace(GetTargetFilePath()))
            {
                errorMessage = "目标目录路径不能为空";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Operation))
            {
                errorMessage = "系统上传固定参数";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Path)) {
                errorMessage = "系统上传固定参数";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace HZtest.Models
{
    // API请求基类
    public class BaseRequest
    {
        /// <summary>
        /// 请求类型
        /// </summary>
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// 结构体
        /// </summary>
        public List<RequestItem> Items { get; set; } = new List<RequestItem>();


    }
    /// <summary>
    /// items中的结构
    /// </summary>
    public  class RequestItem
    {
        /// <summary>
        /// 访问路径
        /// </summary>
        public string Path { get; set; } = string.Empty;


        /// <summary>
        /// 访问位置
        /// </summary>
        public object? Index { get; set; }
        /// <summary>
        /// 访问偏移
        /// </summary>
        public int? Offset { get; set; }
        /// <summary>
        /// 一般为写入值
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// key值
        /// </summary>
        public object? Key { get; set; }

    }

}


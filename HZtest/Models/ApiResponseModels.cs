using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HZtest.Models
{
    /// <summary>
    /// API响应基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseResponse<T>
    {
        /// <summary>
        /// 状态
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; } = -99;

        /// <summary>
        /// 响应消息
        /// </summary>
        [JsonPropertyName("value")]
        public T Value { get; set; }
    }

}


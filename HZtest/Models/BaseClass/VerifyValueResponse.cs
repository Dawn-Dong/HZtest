using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Models.BaseClass
{
    /// <summary>
    /// 实体类：验证规则响应结果
    /// </summary>
    public class VerifyValueResponse
    {
        /// <summary>
        /// 验证规则响应结果
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 验证规则响应消息
        /// </summary>
        public string? Message { get; set; }

        #region 静态工厂方法 - 快速创建响应

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static VerifyValueResponse Success(string? message = null)
        {
            return new VerifyValueResponse
            {
                IsSuccess = true,
                Message = null
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static VerifyValueResponse Fail(string message)
        {
            return new VerifyValueResponse
            {
                IsSuccess = false,
                Message = message ?? "验证失败"
            };
        }

        #endregion
    }
}

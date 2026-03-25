using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Services.ResultModel
{
    public class DialogResult<TResult>
    {
        /// <summary>
        /// true 表示成功，false 表示取消
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 返回参数，Success为true时返回有效数据，Success为false时返回默认值
        /// </summary>
        public TResult Data { get; set; }

        /// <summary>
        /// 成功操作，返回数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DialogResult<TResult> Ok(TResult data) =>
            new() { Success = true, Data = data };

        /// <summary>
        /// 取消操作，返回默认值
        /// </summary>
        /// <returns></returns>
        public static DialogResult<TResult> Cancel() =>
            new() { Success = false, Data = default };
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest
{
    public class AppSettings
    {
        /// <summary>
        /// 接口地址
        /// </summary>
        public string BaseUrl { get; set; }  = string.Empty;  // 属性名必须与JSON完全一致
    }
}

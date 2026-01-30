using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Interfaces_接口定义
{
    /// <summary>
    /// 对话框 ViewModel 必须实现此接口
    /// </summary>
    public interface IDialogAware
    {
        event EventHandler<object> RequestClose;  // 关闭事件
        void OnDialogOpened(object parameter);     // 打开时传参
    }
}

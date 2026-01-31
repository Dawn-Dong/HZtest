using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace HZtest.Infrastructure_基础设施
{
    public class PageViewModelBaseClass : INotifyPropertyChanged
    {
        // 必须添加这个事件（接口要求）
        public event PropertyChangedEventHandler PropertyChanged;

        // 触发属性变更通知的方法
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
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
        //SetProperty 设置属性值并触发通知的方法
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}
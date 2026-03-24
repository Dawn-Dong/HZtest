using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.Models
{
    /// <summary>
    /// 下拉框选择项模型
    /// </summary>
    public class EnumDescriptionRegisterAddress
    {
        public int Value { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 储存二进制位值的模型
    /// </summary>
    public class BinaryValueModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 位的位置（0-15）
        /// </summary>
        public int BitPosition { get; set; }

        private bool _value = false;

        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)  // 确保值真的变了
                {
                    _value = value;
                    OnPropertyChanged();  // 触发通知
                }
            }
        }



        public ICommand ToggleCommand { get; set; }  // 指向 VM 的 

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

using HZtest.Interfaces_接口定义;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace HZtest.ViewModels.Dialogs
{
    public class ModeSelectionViewModel : IDialogAware, INotifyPropertyChanged
    {
        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;

        /// <summary>
        /// 当前模式值（绑定到 RadioButton）
        /// </summary>
        private int _currentMode;
        public int CurrentMode
        {
            get => _currentMode;
            set { _currentMode = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 用户选择的结果（只读）
        /// </summary>
        private int? _selectedMode;
        public int? SelectedMode
        {
            get => _selectedMode;
            private set { _selectedMode = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is int currentMode)
            {
                CurrentMode = currentMode;  // 初始化当前选中项
            }
        }

        /// <summary>
        /// 确定按钮命令
        /// </summary>
        public void Confirm()
        {
            SelectedMode = CurrentMode;  // 保存选择
            RequestClose?.Invoke(this, SelectedMode); // 通知关闭并返回结果
        }

        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }


    }
}

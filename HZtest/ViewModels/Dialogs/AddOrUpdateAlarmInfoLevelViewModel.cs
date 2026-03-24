using HZtest.Converters;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    public class AddOrUpdateAlarmInfoLevelViewModel : IDialogAware, INotifyPropertyChanged
    {        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;

        // 传递过来的ID
        private int Id = -1; // 默认-1表示新增，非-1表示更新

        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is int currentMode)
            {
                Id = currentMode;  // 初始化当前选中项
            }
        }
        #region UI 命令
        /// <summary>
        /// 新增/更新确认按钮命令
        /// </summary>
        public ICommand ConfirmCommand { get; }

        public ICommand CloseCommand { get; }

        #endregion
        #region UI绑定属性
        public List<EnumDescriptionRegisterAddress> AlarmLevelList { get; } = new List<EnumDescriptionRegisterAddress>();

        private string _alarmCode = string.Empty;

        public string AlarmCode
        {
            get => _alarmCode;
            set { _alarmCode = value; OnPropertyChanged(); }
        }


        private int _alarmLevel = -1;
        public int AlarmLevel
        {
            get => _alarmLevel;
            set { _alarmLevel = value; OnPropertyChanged(); }
        }

        public string ConfirmButtonContent => Id == -1 ? "新增" : "更新"; // 根据ID判断是新增还是更新


        #endregion

        public AddOrUpdateAlarmInfoLevelViewModel()
        {
            LoadCoordinateSystemOptions();
            // 初始化命令
            ConfirmCommand = new RelayCommand(Confirm);
            CloseCommand = new RelayCommand(Cancel);
        }

        private async void Confirm()
        {

        }
        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }


        /// <summary>
        /// 从枚举加载寄存器选项
        /// </summary>
        public void LoadCoordinateSystemOptions()
        {

            // 初始化枚举选项（自动获取Description）
            foreach (var value in Enum.GetValues(typeof(AlarmLevelEnum)))
            {
                if ((int)value == AlarmLevelEnum.Error.GetHashCode())
                    break;
                var description = UniversalValueConversion.GetDescription((AlarmLevelEnum)value);
                AlarmLevelList.Add(new EnumDescriptionRegisterAddress
                {
                    Value = (int)value,
                    Description = description
                });
            }
            _alarmLevel = AlarmLevelEnum.ErrorLevel.GetHashCode(); // 默认选择ErrorLevel
        }
    }
}

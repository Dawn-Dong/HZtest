using HZtest.Interfaces_接口定义;
using HZtest.Models.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    public class ConfigAlarmInfoLevelViewModel : IDialogAware, INotifyPropertyChanged
    {
        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;

        #region UI按钮命令      
        /// <summary>
        /// 关闭窗口命令
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 搜索查询命令
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// 新增报警配置命令
        /// </summary>
        public ICommand AddCommand { get; }

        #endregion


        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is int currentMode)
            {
                var CurrentMode = currentMode;  // 初始化当前选中项
            }
        }
        #region UI绑定属性
        private string _searchAlarmCode = string.Empty;
        public string SearchAlarmCode
        {
            get => _searchAlarmCode;
            set { _searchAlarmCode = value; OnPropertyChanged(); }
        }


        #endregion


        public ConfigAlarmInfoLevelViewModel()
        {
            CloseCommand = new RelayCommand(Cancel);
            SearchCommand = new RelayCommand(Search);
            AddCommand = new RelayCommand(AddAlarmConfig);
        }


        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }

        /// <summary>
        /// 筛选数据
        /// </summary>
        private async void Search()
        {

        }
        /// <summary>
        /// 新增报警配置
        /// </summary>
        private async void AddAlarmConfig()
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }


        }


    }
}

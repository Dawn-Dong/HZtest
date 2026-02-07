using HZtest.Infrastructure_基础设施;
using HZtest.Models.Response;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    /// <summary>
    /// 报警信息页面的视图模型
    /// </summary>
    public class AlarmInfoPageViewModel : PageViewModelBaseClass
    {

        private readonly DeviceService _deviceService;

        // 取消令牌（用于停止监控）
        //private CancellationTokenSource _cts;

        //按钮命令
        public ICommand RefreshAlarmInfoCommand { get; }

        //Ui属性
        private List<DeviceAlarmInforResponse> _alarmInfoList = new List<DeviceAlarmInforResponse>();
        public List<DeviceAlarmInforResponse> AlarmInfoList
        {
            get => _alarmInfoList;
            set
            {
                _alarmInfoList = value;
                OnPropertyChanged(nameof(AlarmInfoList));
            }
        }
        private bool _isLoading = false;
        /// <summary>
        /// 加载提示是否展示
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }


        public AlarmInfoPageViewModel(DeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            RefreshAlarmInfoCommand = new RelayCommand(GetAlarmInfoAsync);
            GetAlarmInfoAsync();
        }
        /// <summary>
        /// 获取设备报警信息
        /// </summary>
        private async void GetAlarmInfoAsync()
        {
            {
                try
                {
                    IsLoading = true;
                    var alarmInfo = await _deviceService.GetDeviceAlarmInforAsync();
                    await Task.Delay(1000);
                    AlarmInfoList = alarmInfo.Value;
                    IsLoading = false;
                    //AlarmInfoList = new List<DeviceAlarmInforResponse>();

                }
                catch (Exception ex)
                {
                    // 处理异常，例如记录日志或显示错误消息
                    System.Diagnostics.Debug.WriteLine($"获取报警信息时出错: {ex.Message}");
                }

            }
        }
    }
}

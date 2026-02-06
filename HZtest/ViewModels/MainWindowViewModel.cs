using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.ViewModels
{
    /// <summary>
    /// 主窗口的视图模型
    /// </summary>
    public class MainWindowViewModel : PageViewModelBaseClass
    {
        // ===== 依赖服务（构造函数注入）=====
        private readonly DeviceService _deviceService;

        // 取消令牌（用于停止监控）
        private CancellationTokenSource _cts;

        // ===== UI属性 =====
        private string _alertMessage;
        private bool _isMarqueeEnabled = true;
        public string AlertMessageText
        {
            get => _alertMessage;
            set
            {
                _alertMessage = value;
                OnPropertyChanged();
            }
        }
        public bool IsMarqueeEnabled
        {
            get => _isMarqueeEnabled;
            set { _isMarqueeEnabled = value; OnPropertyChanged(); }
        }

        public MainWindowViewModel(DeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        }

        /// <summary>
        /// 启动数据监控
        /// </summary>
        public void Initialize()
        {
            // 1. 启动数据监控
            StartDataMonitoring();
        }

        /// <summary>
        /// 信息监控循环
        /// </summary>
        private async void StartDataMonitoring()
        {
            _cts = new CancellationTokenSource();

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {

                    await GetDeviceAlarmInfor();

                    // 3. 等待（避免CPU占用过高）
                    await Task.Delay(100, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，忽略
            }
            catch (Exception ex)
            {
                // 记录日志（在ViewModel中不应弹出MessageBox）
                System.Diagnostics.Debug.WriteLine($"监控错误: {ex.Message}");
            }
        }
        /// <summary>
        /// 获取设备报警信息
        /// </summary>
        /// <returns></returns>
        private async Task GetDeviceAlarmInfor()
        {
            var deviceAlarmInfor = await _deviceService.GetDeviceAlarmInforAsync();
            StringBuilder loopDisplay = new StringBuilder();
            var alarmTotalCount = deviceAlarmInfor.Value.Count;
            for (int i = 0; i < deviceAlarmInfor.Value.Count; i++)
            {
                loopDisplay.Append($"({i+1}/{alarmTotalCount}) {deviceAlarmInfor.Value[i].Text}      ");
            }
            AlertMessageText = loopDisplay.ToString();
        }

        // 清理方法（页面关闭时调用）
        public void Cleanup()
        {
            _cts?.Cancel(); // 停止循环
        }

    }
}

using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HZtest.ViewModels
{
    public class FileOperationsPageViewModel : PageViewModelBaseClass
    {
        // ===== 依赖服务（构造函数注入）=====
        private readonly DeviceService _deviceService;


        // 取消令牌（用于停止监控）
        private CancellationTokenSource _cts;

        private string _currentRunningFile = string.Empty;
        /// <summary>
        /// 当前正在运行的文件
        /// </summary>
        public string CurrentRunningFile
        {
            get => _currentRunningFile;
            set { _currentRunningFile = value; OnPropertyChanged(); }
        }

        public FileOperationsPageViewModel(DeviceService deviceService )
        {
            _deviceService= deviceService ?? throw new ArgumentNullException(nameof(deviceService)); 
        }
        /// <summary>
        /// 启动数据监控
        /// </summary>
        public void Initialize()
        {
            // 启动监控（确保 SNCode 已设置）
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

                    await GetTheCurrentRunningFile();

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
        /// 获取当前运行文件
        /// </summary>
        /// <returns></returns>
        private async Task GetTheCurrentRunningFile()
        {
            var fileOperationsModel = await _deviceService.GetTheCurrentRunningFileAsync();


            CurrentRunningFile = fileOperationsModel.Value.RunningFile ?? "加载错误";


        }



        // 清理方法（页面关闭时调用）
        public void Cleanup()
        {
            _cts?.Cancel(); // 停止循环
        }


    }
}

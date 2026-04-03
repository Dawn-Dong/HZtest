using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models.DB;
using HZtest.Models.Request;
using HZtest.Models.Response;
using HZtest.Services;
using SqlSugar;
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
        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;
        private readonly SqlSugarClient _Db;
        private readonly IStructuredLogger _logger;


        // 取消令牌（用于停止监控）
        //private CancellationTokenSource _cts;

        //按钮命令
        public ICommand RefreshAlarmInfoCommand { get; }

        public ICommand ConfigAlarmInfoCommand { get; }

        //Ui属性
        private List<AlarmInfoModels> _alarmInfoList = new List<AlarmInfoModels>();
        public List<AlarmInfoModels> AlarmInfoList
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


        public AlarmInfoPageViewModel(DeviceService deviceService, IMessageService messageService, IDialogService dialogService, SqlSugarClient db, IStructuredLogger logger)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RefreshAlarmInfoCommand = new RelayCommand(GetAlarmInfoAsync);
            ConfigAlarmInfoCommand = new AsyncRelayCommand(ConfigAlarmInfoDialogsAsync);
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
                    //之类不能插入数据只能查询数据
                    IsLoading = true;
                    //不通过接口查询设备而是直接从数据库获取数据
                    // var alarmInfo = await _deviceService.GetDeviceAlarmInforAsync();
                    var alarmInfoList = await _Db.QueryableWithAttr<AlarmInfoModels>().ToListAsync();
                    await Task.Delay(1000);
                    AlarmInfoList = alarmInfoList;
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
        /// <summary>
        /// 配置报警信息弹窗页面
        /// </summary>
        private async Task ConfigAlarmInfoDialogsAsync()
        {
            try
            {
                //后弹出子对话框输入名称和选择本地文件
                var fileUploadRequest = await _dialogService.ShowDialogAsync<bool?>("ConfigAlarmInfoLevel", allowMultiLayer: true);
            }

            catch (Exception ex)
            {
                _message_service.ShowError($"对话框异常: {ex.Message}");
            }
        }
    }
}

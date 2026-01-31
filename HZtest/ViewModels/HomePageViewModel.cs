using HZtest.Converters;
using HZtest.DTO;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Services; // 必需
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
namespace HZtest.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        // 取消令牌（用于停止监控）
        private CancellationTokenSource _cts;

        private string _snCode;

        public string SNCode
        {
            get => _snCode;
            set
            {
                if (_snCode != value)
                {
                    _snCode = value;
                    OnPropertyChanged();
                    // ✅ 设置 SNCode 时自动加载数据
                }
            }
        }

        // 必须添加这个事件（接口要求）
        public event PropertyChangedEventHandler PropertyChanged;

        // 触发属性变更通知的方法
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private int _currentModeValue = 0;
        public int CurrentModeValue
        {
            get => _currentModeValue;
            set { _currentModeValue = value; OnPropertyChanged(); }
        }



        // ✅ 命令（业务逻辑）
        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }

        
        public ICommand OpenModeSelectionCommand { get; }


        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly IMessageService _message_service;
        private readonly DeviceService _deviceService;
        // ===== 状态属性（支持命令启用/禁用）=====
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set { _isBusy = value; OnPropertyChanged(); }
        }
        // 构造函数启动监控
        public HomePageViewModel(IDialogService dialogService, IMessageService messageService, DeviceService deviceService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message_service = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            StartCommand = new RelayCommand(async () => await ExecuteStartAsync());
            PauseCommand = new RelayCommand(async () => await ExecutePauseAsync());
            OpenModeSelectionCommand = new AsyncRelayCommand(OpenModeSelection, () => !IsBusy);
            //StartDataMonitoring();
        }
        /// <summary>
        /// 启动数据监控
        /// </summary>
        // 添加初始化方法，由 DevConnection 调用
        public void Initialize()
        {
            // 启动监控（确保 SNCode 已设置）
            StartDataMonitoring();

        }
        public string ModeIconColor { get; set; } = "#3e0505";
        #region UI属性
        // 通知UI更新的属性
        private string _xAxisActualFeedRateValue = "0.000";
        private string _xAxisRemainingFeedValue = "0.000";

        private string _yAxisActualFeedRateValue = "0.000";
        private string _yAxisRemainingFeedValue = "0.000";


        private string _zAxisActualFeedRateValue = "0.000";
        private string _zAxisRemainingFeedValue = "0.000";

        private string _bAxisActualFeedRateValue = "0.000";
        private string _bAxisRemainingFeedValue = "0.000";

        private string _cAxisActualFeedRateValue = "0.000";
        private string _cAxisRemainingFeedValue = "0.000";


        public string _mainAxisActualSpeed { get; set; } = "0"; // 原始数据属性

        public string _operationMode { get; set; } = "未知"; // 原始数据属性






        // 定义状态属性
        private string _startStatus = "Default"; // 默认值
        private string _pauseStartStatus = "Default";
        public string XAxisActualFeedRateValue
        {
            get => _xAxisActualFeedRateValue;
            set
            {
                _xAxisActualFeedRateValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string XAxisRemainingFeedValue
        {
            get => _xAxisRemainingFeedValue;
            set
            {
                _xAxisRemainingFeedValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }


        public string YAxisActualFeedRateValue
        {
            get => _yAxisActualFeedRateValue;
            set
            {
                _yAxisActualFeedRateValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string YAxisRemainingFeedValue
        {
            get => _yAxisRemainingFeedValue;
            set
            {
                _yAxisRemainingFeedValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }


        public string ZAxisActualFeedRateValue
        {
            get => _zAxisActualFeedRateValue;
            set
            {
                _zAxisActualFeedRateValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string ZAxisRemainingFeedValue
        {
            get => _zAxisRemainingFeedValue;
            set
            {
                _zAxisRemainingFeedValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }


        public string BAxisActualFeedRateValue
        {
            get => _bAxisActualFeedRateValue;
            set
            {
                _bAxisActualFeedRateValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string BAxisRemainingFeedValue
        {
            get => _bAxisRemainingFeedValue;
            set
            {
                _bAxisRemainingFeedValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string CAxisActualFeedRateValue
        {
            get => _cAxisActualFeedRateValue;
            set
            {
                _cAxisActualFeedRateValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string CAxisRemainingFeedValue
        {
            get => _cAxisRemainingFeedValue;
            set
            {
                _cAxisRemainingFeedValue = value;
                OnPropertyChanged(); // 触发通知
            }
        }

        public string StartStartStatus
        {
            get => _startStatus;
            set
            {
                _startStatus = value;
                OnPropertyChanged(); // 通知UI更新
            }
        }

        public string PauseStartStatus
        {
            get => _pauseStartStatus;
            set
            {
                _pauseStartStatus = value;
                OnPropertyChanged(); // 通知UI更新
            }
        }
        /// <summary>
        /// 主轴运行模式
        /// </summary>
        public string OperationMode
        {
            get => _operationMode;
            set
            {
                _operationMode = value;
                OnPropertyChanged(); // 触发通知
                OnPropertyChanged(nameof(OperationModeDisplay)); // 关键！通知拼接属性变化
            }
        }

        public string OperationModeDisplay => $"运行模式: {OperationMode}";

        /// <summary>
        /// 主轴实际转速
        /// </summary>
        public string MainAxisActualSpeed
        {
            get => _mainAxisActualSpeed;
            set
            {
                _mainAxisActualSpeed = value;
                OnPropertyChanged(); // 触发通知
                OnPropertyChanged(nameof(MainAxisSpeedDisplay)); // 关键！通知拼接属性变化
            }
        }
        public string MainAxisSpeedDisplay => $"主轴实际速度: {MainAxisActualSpeed:F1}";
        #endregion


        // 监控方法（在后台循环）
        private async void StartDataMonitoring()
        {
            _cts = new CancellationTokenSource();

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {

                    await AllAxisInformation();
                    await GetStartPauseButtonStateAsync();
                    await GetActualSpindleSpeedAsync();
                    await GetOperationModeAsync();


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
        /// 设置循环启动信号为 true
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteStartAsync()
        {

            var startStopStateDto = new StartStopStateDto()
            {
                SetStatus = StartStopStatusEnum.Start,
                State = true,

            };
            var startState = await _deviceService.SetStartPauseStateAsync(startStopStateDto);
            //暂不进行消息提示  方案参考 消息服务
        }
        /// <summary>
        /// 设置进给保持信号为 true
        /// </summary>
        /// <returns></returns>
        private async Task ExecutePauseAsync()
        {
            var startStopStateDto = new StartStopStateDto()
            {
                SetStatus = StartStopStatusEnum.Stop,
                State = true,

            };
            var stopState = await _deviceService.SetStartPauseStateAsync(startStopStateDto);
            //暂不进行消息提示  方案参考 消息服务

        }


        private async Task OpenModeSelection()
        {

            try
            {
                // ✅ 调用服务显示对话框
                // 参数1: 对话框名称（与 CreateDialog 方法中的 switch case 匹配）
                // 参数2: 传入当前模式值（可选）
                var result = await _dialogService.ShowDialogAsync<int?>("ModeSelection", CurrentModeValue);

                // 处理结果
                if (result.HasValue)
                {
                    // 用户点击了确定
                    CurrentModeValue = result.Value; // 更新本地状态
                    var setResult = await _deviceService.SetOperationModeAsync((DevOperationModeEnum)CurrentModeValue);
                    if (setResult.Code ==0)
                    {
                        _message_service.ShowMessage($"设置成功");
                    }
                    else
                    {
                        _message_service.ShowError($"{setResult.Status}");
                    }

                }
                else
                {
                    // 用户点击了取消
                    _message_service.ShowMessage("操作已取消");
                }
            }
            catch (Exception ex)
            {
                _message_service.ShowError($"对话框异常: {ex.Message}");
            }


        }

        #region 实时扫描更新消息
        /// <summary>
        /// 获取全部轴信息并且赋值
        /// </summary>
        /// <returns></returns>
        private async Task AllAxisInformation()
        {
            // 1. 调用你的接口获取数据
            var allAxisData = await _deviceService.BatchGetAllActualAndRemainingFeedAsync();

            // 2. 更新属性（自动通知UI）
            XAxisActualFeedRateValue = ValueConversion(allAxisData.Value[0].ActualFeedRate).ToString("F3");
            XAxisRemainingFeedValue = ValueConversion(allAxisData.Value[0].RemainingFeed).ToString("F3");

            YAxisActualFeedRateValue = ValueConversion(allAxisData.Value[1].ActualFeedRate).ToString("F3");
            YAxisRemainingFeedValue = ValueConversion(allAxisData.Value[1].RemainingFeed).ToString("F3");

            ZAxisActualFeedRateValue = ValueConversion(allAxisData.Value[2].ActualFeedRate).ToString("F3");
            ZAxisRemainingFeedValue = ValueConversion(allAxisData.Value[2].RemainingFeed).ToString("F3");

            BAxisActualFeedRateValue = ValueConversion(allAxisData.Value[3].ActualFeedRate).ToString("F3");
            BAxisRemainingFeedValue = ValueConversion(allAxisData.Value[3].RemainingFeed).ToString("F3");

            CAxisActualFeedRateValue = ValueConversion(allAxisData.Value[4].ActualFeedRate).ToString("F3");
            CAxisRemainingFeedValue = ValueConversion(allAxisData.Value[4].RemainingFeed).ToString("F3");

        }

        /// <summary>
        /// 获取启动停止按钮状态
        /// </summary>
        /// <returns></returns>
        private async Task GetStartPauseButtonStateAsync()
        {
            var startStopState = await _deviceService.GetStartPauseStateAsync();

            StartStartStatus = startStopState.Value.CycleStart ? "Enable" : "Default";
            PauseStartStatus = startStopState.Value.FeedHold ? "Enable" : "Default";
        }
        /// <summary>
        /// 获取主轴实际转速
        /// </summary>
        /// <returns></returns>

        private async Task GetActualSpindleSpeedAsync()
        {
            var actualSpindleSpeed = await _deviceService.GetActualSpindleSpeedAsync();
            MainAxisActualSpeed = actualSpindleSpeed.Value.ToString() ?? "0";
        }

        private async Task GetOperationModeAsync()
        {
            var cs = await _deviceService.GetOperationModeAsync();
            OperationMode = cs.Value.ToString();
        }

        #endregion


        // 清理方法（页面关闭时调用）
        public void Cleanup()
        {
            _cts?.Cancel(); // 停止循环
        }
        private double ValueConversion(double value)
        {
            return value / 100000;
        }




    }
}

using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models.Response;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    /// <summary>
    /// 刀具信息页面的视图模型
    /// </summary>
    public class ToolInfoViewModel : PageViewModelBaseClass
    {
        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;


        //Ui属性
        private string _toolNumber = string.Empty;
        public string ToolNumber
        {
            get => _toolNumber;
            set
            {
                _toolNumber = value;
                OnPropertyChanged(nameof(ToolNumber));
            }
        }



        private List<ToolInfoResponse> _toolInfoList = new List<ToolInfoResponse>();
        public List<ToolInfoResponse> ToolInfoList
        {
            get => _toolInfoList;
            set
            {
                _toolInfoList = value;
                OnPropertyChanged(nameof(ToolInfoList));
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

        //按钮命令
        public ICommand QueryToolInfoCommand { get; }


        public ToolInfoViewModel(DeviceService deviceService, IDialogService dialogService, IMessageService messageService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            QueryToolInfoCommand = new RelayCommand(GetToolInfoAsync);

        }

        /// <summary>
        /// 获取设备刀具信息
        /// </summary>
        private async void GetToolInfoAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ToolNumber))
                {
                    // 处理工具号为空的情况，例如显示错误消息
                    _message_service.ShowError("请输入刀具号");
                    return;
                }

                if (!int.TryParse(ToolNumber, out int toolNumberResult))
                {
                    // 处理工具号不是有效整数的情况，例如显示错误消息
                    _message_service.ShowError("刀具号必须是一个有效的整数");
                    return;
                }

                IsLoading = true;
                var toolInfo = await _deviceService.GetDeviceToolInforAsync(toolNumberResult);
                await Task.Delay(1000);
                var toolInfoResponse = toolInfo.Value;
                var toolInfoList = new List<ToolInfoResponse>();
                toolInfoList.Add(toolInfoResponse);
                if (toolInfoResponse != null)
                    ToolInfoList = toolInfoList;
                IsLoading = false;

            }
            catch (Exception ex)
            {
                // 处理异常，例如显示错误消息
                Console.WriteLine($"获取刀具信息失败: {ex.Message}");
                _message_service.ShowError($"报错{ex.Message}");
            }
        }
    }
}

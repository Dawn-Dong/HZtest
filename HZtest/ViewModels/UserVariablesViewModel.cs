using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    /// <summary>
    ///用户信息页面的视图模型.
    /// </summary>
    public class UserVariablesViewModel : PageViewModelBaseClass
    {

        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;

        //Ui属性
        private bool _isReadMode = true;
        public bool IsReadMode
        {
            get => _isReadMode;
            set
            {
                _isReadMode = value;
                OnPropertyChanged(nameof(IsReadMode));
            }
        }
        private bool _isWriteMode = false;

        public bool IsWriteMode
        {
            get => _isWriteMode;
            set
            {
                _isWriteMode = value;
                OnPropertyChanged(nameof(IsWriteMode));
                if (value)
                    UserVariableWriteValue = string.Empty;
            }
        }

        private string _userVariableAddress = string.Empty;
        public string UserVariableAddress
        {
            get => _userVariableAddress;
            set
            {
                _userVariableAddress = value;
                OnPropertyChanged(nameof(UserVariableAddress));
            }
        }
        private string _userVariableWriteValue = string.Empty;
        public string UserVariableWriteValue
        {
            get => _userVariableWriteValue;
            set
            {
                _userVariableWriteValue = value;
                OnPropertyChanged(nameof(UserVariableWriteValue));
            }
        }

        public ICommand UserVariableOperationCommand { get; }





        public UserVariablesViewModel(IDialogService dialogService, DeviceService deviceService, IMessageService message_service)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));

            UserVariableOperationCommand = new RelayCommand(ReadOrWriteUserVariableAsync);
        }

        /// <summary>
        /// 读写用户变量
        /// </summary>
        private async void ReadOrWriteUserVariableAsync()
        {
            try
            {
                if (!IsReadMode && !IsWriteMode)
                {
                    _message_service.ShowMessage("请选择操作模式（读或写）");
                    return;
                }
                if (IsWriteMode && string.IsNullOrWhiteSpace(UserVariableAddress))
                {
                    _message_service.ShowMessage("请输入用户变量地址");
                    return;
                }
                if (!int.TryParse(UserVariableAddress, out int address))
                {
                    _message_service.ShowMessage("用户变量地址必须是整数");
                    return;
                }
                //if (IsWriteMode && !double.TryParse(UserVariableWriteValue, out double writeValue))
                //{
                //    _message_service.ShowMessage("写入值必须是数字");
                //    return;
                //}
                var userVariableRequest = new UserVariablesReadWriteRequest
                {
                    OperationAddressNumber = address,
                    OperationType = IsWriteMode ? UserVariableOperationTypeEnum.Write : UserVariableOperationTypeEnum.Read,
                };
                if (IsWriteMode)
                {
                    if (!double.TryParse(UserVariableWriteValue, out double writeValue))
                    {
                        _message_service.ShowMessage("写入值必须是数字");
                        return;
                    }
                    userVariableRequest.WriteValue = writeValue;

                    var writeResult = await _deviceService.SetUserVariablesAsync(userVariableRequest);
                    if (writeResult.Value)
                    {
                        _message_service.ShowMessage("写入成功");
                    }
                    else
                    {
                        _message_service.ShowMessage($"写入失败{writeResult.Status}");
                    }
                }
                else
                {
                    var readResult = await _deviceService.GetUserVariablesAsync(userVariableRequest);

                    if (readResult.Code == 0)
                    {
                        _message_service.ShowMessage($"读取[{address}]得到的值: {readResult.Value}");
                    }
                    else
                    {
                        _message_service.ShowMessage($"读取失败{readResult.Status}");
                    }
                }

            }
            catch (Exception ex)
            {
                _message_service.ShowMessage($"错误:{ex.Message}");
            }
        }


    }
}
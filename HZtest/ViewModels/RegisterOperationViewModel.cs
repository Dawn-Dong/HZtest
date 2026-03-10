using HZtest.Converters;
using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    /// <summary>
    ///寄存器查询页面的视图模型.
    /// </summary>
    public class RegisterOperationViewModel : PageViewModelBaseClass
    {

        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;

        //Ui属性


        public List<EnumDescriptionRegisterAddress> RegisterTypeList { get; } = new List<EnumDescriptionRegisterAddress>();

        private int _selectedRegisterType = -1;
        public int SelectedRegisterType
        {
            get => _selectedRegisterType;
            set
            {
                _selectedRegisterType = value;
                OnPropertyChanged();
                OnRegisterTypeSelected((RegisterTypeEnum)_selectedRegisterType);
            }
        }


        private bool _isReadMode = true;
        /// <summary>
        /// 是否是读模式
        /// </summary>
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

        /// <summary>
        /// 是否是写模式
        /// </summary>
        public bool IsWriteMode
        {
            get => _isWriteMode;
            set
            {
                _isWriteMode = value;
                OnPropertyChanged(nameof(IsWriteMode));
                if (value)
                    RegisterWriteValue = string.Empty;
            }
        }

        private string _registerAddress = string.Empty;
        public string RegisterAddress
        {
            get => _registerAddress;
            set
            {
                _registerAddress = value;
                OnPropertyChanged(nameof(RegisterAddress));
            }
        }


        private string _registerAddressOffset = string.Empty;
        public string RegisterAddressOffset
        {
            get => _registerAddressOffset;
            set
            {
                _registerAddressOffset = value;
                OnPropertyChanged(nameof(RegisterAddressOffset));
            }
        }


        private string _registerWriteValue = string.Empty;
        public string RegisterWriteValue
        {
            get => _registerWriteValue;
            set
            {
                _registerWriteValue = value;
                OnPropertyChanged(nameof(RegisterWriteValue));
            }
        }
        /// <summary>
        /// 显示范围
        /// </summary>
        private string _registerRange = string.Empty;

        public string RegisterRange
        {
            get => _registerRange;
            set
            {
                _registerRange = value;
                OnPropertyChanged(nameof(RegisterRange));
            }
        }


        // ===== 命令 =====

        public ICommand RegisterOperationCommand { get; }


        /// <summary>
        /// 从枚举加载寄存器选项
        /// </summary>
        public void LoadCoordinateSystemOptions()
        {

            // 初始化枚举选项（自动获取Description）
            foreach (var value in Enum.GetValues(typeof(RegisterTypeEnum)))
            {
                if ((int)value == RegisterTypeEnum.Error.GetHashCode())
                    break;
                var description = UniversalValueConversion.GetDescription((RegisterTypeEnum)value);
                RegisterTypeList.Add(new EnumDescriptionRegisterAddress
                {
                    Value = (int)value,
                    Description = description
                });
            }
            _selectedRegisterType = RegisterTypeEnum.G.GetHashCode();
            var registerInfo = GetRegisterInfo((RegisterTypeEnum)_selectedRegisterType);
            _registerRange = $"范围：0-{registerInfo.MaximumWritableAddress}";
        }

        public RegisterOperationViewModel(IDialogService dialogService, DeviceService deviceService, IMessageService message_service)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));
            LoadCoordinateSystemOptions();
            RegisterOperationCommand = new RelayCommand(ReadOrWriteRegisterOperationAsync);
        }

        /// <summary>
        /// 读写寄存器操作的异步方法
        /// </summary>
        private async void ReadOrWriteRegisterOperationAsync()
        {
            try
            {
                if (!IsReadMode && !IsWriteMode)
                {
                    _message_service.ShowMessage("请选择操作模式（读或写）");
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegisterAddress))
                {
                    _message_service.ShowMessage("请输入寄存器地址");
                    return;
                }
                if (IsWriteMode && string.IsNullOrWhiteSpace(RegisterWriteValue))
                {
                    _message_service.ShowMessage("请输入写入值");
                    return;
                }

                if (!int.TryParse(RegisterAddress, out int address))
                {
                    _message_service.ShowMessage("寄存器地址必须是整数");
                    return;
                }
                var registerWriteValue = 0;
                if (IsWriteMode && !int.TryParse(RegisterWriteValue, out registerWriteValue))
                {
                    _message_service.ShowMessage("写入值必须是整数");
                    return;
                }

                if (!string.IsNullOrEmpty(RegisterAddressOffset) && !int.TryParse(RegisterAddressOffset, out int offsetAddress))
                {
                    _message_service.ShowMessage("偏移量必须是整数");
                    return;
                }
                var (DetermineOverResult, MaximumWritableAddress) = DetermineOverLimit(address, (RegisterTypeEnum)SelectedRegisterType);
                if (!DetermineOverResult)
                {
                    _message_service.ShowMessage($"寄存器地址超出最大范围{MaximumWritableAddress}");
                    return;
                }

                var registerOperationRequest = new RegisterOperationRequest()
                {
                    RegisterAddress = address,
                    RegisterType = (RegisterTypeEnum)SelectedRegisterType,
                    RegisterOffset = string.IsNullOrEmpty(RegisterAddressOffset) ? null : Convert.ToInt32(RegisterAddressOffset),
                    RegisterWriteValue = registerWriteValue
                };

                if (IsReadMode)
                {
                    var registerResponse = await _deviceService.GetRegisterAsync(registerOperationRequest);
                    _message_service.ShowMessage($"读取到的值：{registerResponse.Value.DecimalValue}");
                    return;
                }
                else
                {
                    var registerWriteRequest = await _deviceService.SetRegisterAsync(registerOperationRequest);
                    _message_service.ShowMessage(registerWriteRequest.Value ? $"值写入成功" : $"值写入失败");
                }


            }
            catch (Exception ex)
            {
                _message_service.ShowError($"错误:{ex.Message}");
            }
        }
        /// <summary>
        /// 判断输入的寄存器地址是否超过该设备的寄存器范围
        /// </summary>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="selectedRegisterType">寄存器类型</param>
        /// <returns></returns>
        private (bool Result, int MaximumWritableAddress) DetermineOverLimit(int registerAddress, RegisterTypeEnum selectedRegisterType)
        {
            try
            {
                var registerInfo = GetRegisterInfo(selectedRegisterType);
                return (Result: registerAddress > 0 && registerAddress <= registerInfo.MaximumWritableAddress, MaximumWritableAddress: registerInfo.MaximumWritableAddress);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// 获取寄存器特征
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private RegisterInfoAttribute GetRegisterInfo(RegisterTypeEnum register)
        {
            var fieldInfo = typeof(RegisterTypeEnum).GetField(register.ToString());
            return fieldInfo?.GetCustomAttribute<RegisterInfoAttribute>()
                ?? throw new ArgumentException($"寄存器 {register} 没有找到特性.");
        }

        /// <summary>
        /// 选择了新的寄存器类型
        /// </summary>
        private void OnRegisterTypeSelected(RegisterTypeEnum registerType)
        {
            var registerInfo = GetRegisterInfo(registerType);
            RegisterRange = $"范围：0-{registerInfo.MaximumWritableAddress}";

        }
    }
}
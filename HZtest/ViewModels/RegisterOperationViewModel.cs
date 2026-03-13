using HZtest.Converters;
using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        #region 实现二进制写入所需

        // 1. 是否勾选"二进制写入"
        private bool _isBinaryEnabled = false;
        /// <summary>
        /// 是否二进制模式
        /// </summary>
        public bool IsBinaryEnabled
        {
            get => _isBinaryEnabled;
            set
            {
                _isBinaryEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBinaryEnabled));
                OnPropertyChanged(nameof(ShowBinaryPanel));
                // 勾上时，把当前输入框的数字转成二进制显示
                if (value && !string.IsNullOrEmpty(RegisterWriteValue))
                {
                    if (int.TryParse(RegisterWriteValue, out int num))
                        UpdateBitsFromNumber(num);
                }


            }
        }

        // 是否显示二进制面板（勾选二进制且是写入模式）
        public bool ShowBinaryPanel => IsBinaryEnabled && IsWriteMode;

        // 2. 当前是8位还是16位（从特性读取）
        private int _bitCount = 8;
        public int BitCount
        {
            get => _bitCount;
            set
            {
                _bitCount = value;
                OnPropertyChanged(nameof(BitCount));
                // 位宽变化时重新初始化
                InitializeBits();
            }
        }
        // 3. 二进制位集合（从高位到低位排列，方便界面绑定）
        public ObservableCollection<BinaryValueModel> Bits { get; } = new ObservableCollection<BinaryValueModel>();

        // 4. 当前二进制值（用于发送命令）
        public int BinaryValue
        {
            get
            {
                int val = 0;
                foreach (var bit in Bits)
                {
                    if (bit.Value)
                        val |= (1 << bit.BitPosition);
                }
                return val;
            }
        }


        /// <summary>
        /// 从数值更新位显示（输入框改变时调用）
        /// </summary>
        public void UpdateBitsFromNumber(int num)
        {

            foreach (var bit in Bits)
            {
                bit.Value = (num & (1 << bit.BitPosition)) != 0;
            }
            OnPropertyChanged(nameof(BinaryValue));
        }

        /// <summary>
        /// 从位集合更新数值（位被点击时调用）
        /// </summary>
        public void UpdateValueFromBits()
        {
            // 更新输入框显示
            RegisterWriteValue = BinaryValue.ToString();
            OnPropertyChanged(nameof(RegisterWriteValue));
            OnPropertyChanged(nameof(BinaryValue));
        }

        /// <summary>
        /// 切换某一位的值（界面点击时调用）
        /// </summary>
        public void ToggleBit(int bitPosition)
        {
            var bit = Bits.FirstOrDefault(b => b.BitPosition == bitPosition);
            if (bit != null)
            {
                bit.Value = !bit.Value;
                UpdateValueFromBits();
            }
        }



        #endregion


        // ===== 命令 =====

        public ICommand RegisterOperationCommand { get; }
        public ICommand SwitchingHighAndLowPositionsCommand { get; }

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
            BitCount = registerInfo.BitWidth;
            _registerRange = $"范围：0-{registerInfo.MaximumWritableAddress}";
        }

        public RegisterOperationViewModel(IDialogService dialogService, DeviceService deviceService, IMessageService message_service)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));
            LoadCoordinateSystemOptions();
            RegisterOperationCommand = new RelayCommand(ReadOrWriteRegisterOperationAsync);
            SwitchingHighAndLowPositionsCommand = new RelayCommand<int>(OnBitToggle);
            // 初始化二进制位集合
            InitializeBits();
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
                if (!int.TryParse(RegisterAddress, out int address))
                {
                    _message_service.ShowMessage("寄存器地址必须是整数");
                    return;
                }
                if (IsWriteMode && IsBinaryEnabled && string.IsNullOrWhiteSpace(RegisterWriteValue))
                {
                    _message_service.ShowMessage("请输入写入值");
                    return;
                }


                var registerWriteValue = 0;
                if (IsWriteMode && !IsBinaryEnabled && !int.TryParse(RegisterWriteValue, out registerWriteValue))
                {
                    _message_service.ShowMessage("十进制写入值必须是整数");
                    return;
                }

                //if (!string.IsNullOrEmpty(RegisterAddressOffset) && !int.TryParse(RegisterAddressOffset, out int offsetAddress))
                //{
                //    _message_service.ShowMessage("偏移量必须是整数");
                //    return;
                //}


                var (DetermineOverResult, MaximumWritableAddress) = DetermineOverLimit(address, (RegisterTypeEnum)SelectedRegisterType);
                if (!DetermineOverResult)
                {
                    _message_service.ShowMessage($"寄存器地址超出最大范围{MaximumWritableAddress}");
                    return;
                }

                //读取的偏移量在此暂不启用（功能未废除）

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
                    if (!IsBinaryEnabled)
                    {
                        RegisterWriteValue = registerResponse.Value.DecimalValue.ToString() ?? string.Empty;
                    }
                    else
                    {
                        UpdateBitsFromNumber(registerResponse.Value.DecimalValue ?? 0);
                    }
                    _message_service.ShowMessage($"读取到的值：{registerResponse.Value.DecimalValue}");
                    return;
                }
                else
                {
                    //写入的情况包括二进制偏移量写入和十进制全量
                    if (!IsBinaryEnabled)
                    {
                        var registerWriteRequest = await _deviceService.SetRegisterAsync(registerOperationRequest);
                        _message_service.ShowMessage(registerWriteRequest.Value ? $"值写入成功" : $"值写入失败");
                    }
                    else
                    {
                        //先读取在按位批量写入
                        var registerReadRequest = await _deviceService.GetRegisterAsync(registerOperationRequest);

                        //输出对比结果（不一致的位）传到接口进行写入
                        var registerOperationRequestList = CompareAndUpdateBits(registerReadRequest.Value.DecimalValue ?? 0);

                        var response = await _deviceService.SetRegisterListAsync(registerOperationRequestList);
                        // 处理结果
                        if (response.Value != null)
                        {
                            foreach (var result in response.Value)
                            {
                                if (!result.Success)
                                {
                                    Console.WriteLine($"❌ 失败: {result.Description} - {result.Message}");
                                    // 输出: ❌ 失败: G100 第 5 位 = 1 - 设备响应超时
                                    _message_service.ShowError($"失败：{result.Description} - {result.Message}");
                                }
                                else
                                {
                                    Console.WriteLine($"✅ 成功: {result.Description}");


                                }
                            }
                        }

                        // 只获取失败的
                        var failed = response.Value?.Where(r => !r.Success).ToList();
                        if (failed?.Any() == true)
                        {
                            _message_service.ShowMessage($"以下 {failed.Count} 个位写入失败：\n" +
                                string.Join("\n", failed.Select(f => f.Description)));
                        }
                        else
                        {
                            _message_service.ShowMessage($"写入成功!（{response?.Value?.Count}/{response?.Value?.Count}） \n" +
                                 string.Join("\n", response?.Value?.Select(f => f.Description))
                                );
                        }
                    }
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
            BitCount = registerInfo.BitWidth;
            RegisterRange = $"范围：0-{registerInfo.MaximumWritableAddress}";

        }
        /// <summary>
        /// 初始化位集合（根据BitCount创建）
        /// </summary>
        public void InitializeBits()
        {
            Bits.Clear();
            // 从高位到低位显示（如15,14,13...0 或 7,6,5...0）
            for (int i = BitCount - 1; i >= 0; i--)
            {
                Bits.Add(new BinaryValueModel
                {
                    BitPosition = i,
                    Value = false,
                    ToggleCommand = SwitchingHighAndLowPositionsCommand  // 注入命令
                });
            }
        }

        /// <summary>
        /// 位切换处理
        /// </summary>
        private void OnBitToggle(int bitPosition)
        {
            var bit = Bits.FirstOrDefault(b => b.BitPosition == bitPosition);
            if (bit != null)
            {
                bit.Value = !bit.Value;
                UpdateValueFromBits();
            }
        }

        /// <summary>
        /// 对比输入值与当前Bits的差异，输出不一致的位
        /// </summary>
        /// <param name="originalValue">输入的原始值</param>
        public List<RegisterOperationRequest> CompareAndUpdateBits(int originalValue)
        {
            // 1. 将现在系统中的值输入转为二进制数组（与BitCount位数一致）
            bool[] existingBits = ConvertToBitArray(originalValue, BitCount);

            // 2. 与当前Bits对比，找出不一致
            var differences = new List<BinaryValueModel>();

            for (int i = 0; i < Bits.Count; i++)
            {
                // Bits是按高位到低位存储的（如15,14,13...0）
                // newBits[0]对应最高位
                bool currentValue = Bits[i].Value;
                bool existingValue = existingBits[i];

                if (currentValue != existingValue)
                {
                    differences.Add(new BinaryValueModel
                    {
                        BitPosition = Bits[i].BitPosition,
                        Value = Bits[i].Value,

                    });

                }
            }
            var registerOperationRequestList = new List<RegisterOperationRequest>();
            // 3. 输出差异结果
            if (differences.Count == 0)
            {
                _message_service.ShowMessage("所有位和现有值一致，无变化!");
                return registerOperationRequestList;
            }
            else
            {
                Console.WriteLine($"发现 {differences.Count} 处不一致：");
                foreach (var diff in differences)
                {
                    registerOperationRequestList.Add(new RegisterOperationRequest
                    {
                        RegisterType = (RegisterTypeEnum)SelectedRegisterType,
                        RegisterAddress = Convert.ToInt32(RegisterAddress),
                        RegisterOffset = diff.BitPosition, // 偏移量为位位置
                        RegisterWriteValue = diff.Value ? 1 : 0 // 写入1或0
                    });
                }
            }
            return registerOperationRequestList;
        }
        /// <summary>
        /// 将整数转为指定位数的二进制数组（高位在前）
        /// </summary>
        private bool[] ConvertToBitArray(int value, int bitCount)
        {
            bool[] bits = new bool[bitCount];

            for (int i = 0; i < bitCount; i++)
            {
                // 从高位到低位提取
                int bitPosition = bitCount - 1 - i;
                bits[i] = (value & (1 << bitPosition)) != 0;
            }

            return bits;
        }


        //private 

    }
}
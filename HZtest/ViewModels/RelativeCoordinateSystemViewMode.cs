using HZtest.Converters;
using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Resources_资源.Control.ViewModel;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    public class RelativeCoordinateSystemViewMode : PageViewModelBaseClass
    {

        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;

        // ===== 依赖属性（绑定）=====
        public List<EnumDescriptionRelativeCoordinateSystem> CoordinateSystemOptions { get; } = new List<EnumDescriptionRelativeCoordinateSystem>();

        private int _selectedCoordinateSystem = -1;
        public int SelectedCoordinateSystem
        {
            get => _selectedCoordinateSystem;
            set
            {
                _selectedCoordinateSystem = value;
                OnPropertyChanged();
            }
        }

        public RelativeCoordinateSystemViewModel CoordinateSystemData { get; } =
            new RelativeCoordinateSystemViewModel
            {
                XAxisCoordinateSystemValue = 0,
                YAxisCoordinateSystemValue = 0,
                ZAxisCoordinateSystemValue = 0,
                BAxisCoordinateSystemValue = 0,
                CAxisCoordinateSystemValue = 0
            };

        //按钮命令
        public ICommand CoordinateSystemQueryCommand { get; }
        public ICommand CoordinateSystemUploadCommand { get; }


        public RelativeCoordinateSystemViewMode(IDialogService dialogService, DeviceService deviceService, IMessageService message_service)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));
            LoadCoordinateSystemOptions();

            CoordinateSystemQueryCommand = new RelayCommand(GetCoordinateSystemAsync);
            CoordinateSystemUploadCommand = new RelayCommand(UploadCoordinateSystemAsync);

        }
        /// <summary>
        /// 从枚举加载坐标系选项
        /// </summary>
        public void LoadCoordinateSystemOptions()
        {

            // 初始化枚举选项（自动获取Description）
            foreach (var value in Enum.GetValues(typeof(CoordinateSystemEnum)))
            {
                var description = UniversalValueConversion.GetDescription((CoordinateSystemEnum)value);
                CoordinateSystemOptions.Add(new EnumDescriptionRelativeCoordinateSystem
                {
                    Value = (int)value,
                    Description = description
                });
            }
            SelectedCoordinateSystem = CoordinateSystemEnum.Relative.GetHashCode();
        }
        /// <summary>
        /// 获取对应坐标系的值
        /// </summary>
        private async void GetCoordinateSystemAsync()
        {
            try
            {
                if (SelectedCoordinateSystem == -1)
                {
                    _message_service.ShowError($"类型有误");
                    return;
                }
                var coordinateSystem = await _deviceService.GetCoordinateSystemAsync(SelectedCoordinateSystem);
                var coordinateSystemAxis = coordinateSystem.Value;
                CoordinateSystemData.XAxisCoordinateSystemValue = coordinateSystemAxis.X;
                CoordinateSystemData.YAxisCoordinateSystemValue = coordinateSystemAxis.Y;
                CoordinateSystemData.ZAxisCoordinateSystemValue = coordinateSystemAxis.Z;
                CoordinateSystemData.BAxisCoordinateSystemValue = coordinateSystemAxis.B;
                CoordinateSystemData.CAxisCoordinateSystemValue = coordinateSystemAxis.C;

            }
            catch (Exception ex)
            {

                _message_service.ShowError($"错误{ex.Message}");
            }
        }

        /// <summary>
        /// 上传坐标系值到设备
        /// </summary>
        private async void UploadCoordinateSystemAsync()
        {
            try
            {
                if (SelectedCoordinateSystem == -1)
                {
                    _message_service.ShowError($"类型有误");
                    return;
                }
                var request = new RelativeCoordinateSystemRequest
                {
                    OperatingCoordinateSystemId = SelectedCoordinateSystem,
                    XAxisValue = CoordinateSystemData.XAxisCoordinateSystemValue,
                    YAxisValue = CoordinateSystemData.YAxisCoordinateSystemValue,
                    ZAxisValue = CoordinateSystemData.ZAxisCoordinateSystemValue,
                    BAxisValue = CoordinateSystemData.BAxisCoordinateSystemValue,
                    CAxisValue = CoordinateSystemData.CAxisCoordinateSystemValue
                };
                var result = await _deviceService.SetCoordinateSystemAsync(request);
                if (result.Value)
                {
                    _message_service.ShowMessage($"上传成功");
                }
                else
                {
                    _message_service.ShowError($"上传失败:{result.Status}");
                }
            }
            catch (Exception ex)
            {
                _message_service.ShowError($"错误{ex.Message}");
            }

        }

    }
}

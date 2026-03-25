using HZtest.Converters;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    /// <summary>
    /// 新增/修改告警等级对话框VM
    /// </summary>
    public class AddOrUpdateAlarmInfoLevelViewModel : IDialogAware, INotifyPropertyChanged
    {        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;


        // ===== 依赖服务（构造函数注入）=====
        private readonly IMessageService _message_service;
        private readonly IStructuredLogger _logger;
        private readonly SqlSugarClient _db;

        // 传递过来的实体内容- 用于区分新增还是修改
        private AlarmManagementConfigModel? AlarmManagementConfig = null;  // 当前编辑的对象

        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is AlarmManagementConfigModel currentMode)
            {
                AlarmManagementConfig = currentMode;  // 初始化当前选中项
                AlarmCode = currentMode.AlarmCode;
                AlarmLevel = currentMode.AlarmLevel.GetHashCode();
            }
        }
        #region UI 命令
        /// <summary>
        /// 新增/更新确认按钮命令
        /// </summary>
        public ICommand ConfirmCommand { get; }

        public ICommand CloseCommand { get; }

        #endregion
        #region UI绑定属性
        public List<EnumDescriptionRegisterAddress> AlarmLevelList { get; } = new List<EnumDescriptionRegisterAddress>();

        private string _alarmCode = string.Empty;

        public string AlarmCode
        {
            get => _alarmCode;
            set { _alarmCode = value; OnPropertyChanged(); }
        }


        private int _alarmLevel = -1;
        public int AlarmLevel
        {
            get => _alarmLevel;
            set { _alarmLevel = value; OnPropertyChanged(); }
        }

        public string ConfirmButtonContent => AlarmManagementConfig == null ? "新增" : "更新"; // 根据ID判断是新增还是更新


        #endregion

        public AddOrUpdateAlarmInfoLevelViewModel(IMessageService message_service, IStructuredLogger logger, SqlSugarClient db)
        {
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            LoadCoordinateSystemOptions();
            // 初始化命令
            ConfirmCommand = new RelayCommand(Confirm);
            CloseCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// 确认按钮命令 新增/修改
        /// </summary>
        private async void Confirm()
        {
            try
            {
                var alarmManagementConfigModel = new AlarmManagementConfigModel
                {
                    AlarmCode = AlarmCode,
                    AlarmLevel = (AlarmLevelEnum)AlarmLevel,
                    CreateTime = DateTime.Now,
                };
                var result = 0;

                var existed = await _db.QueryableWithAttr<AlarmManagementConfigModel>()
                    .Where(it => it.AlarmCode == AlarmCode)
                    .WhereIF(AlarmManagementConfig != null, it => it.Id != AlarmManagementConfig.Id)
                    .AnyAsync();

                if (existed)
                {
                    _message_service.ShowError($"{(AlarmManagementConfig == null ? "新增" : "更新")}失败，报警代码不能重复");
                    return;
                }


                if (AlarmManagementConfig == null)
                {
                    alarmManagementConfigModel.Id = SnowFlakeSingle.Instance.NextId();

                    result = await _db.InsertableWithAttr(alarmManagementConfigModel).ExecuteCommandAsync();
                }
                else
                {
                    AlarmManagementConfig.ModifyTime = DateTime.Now;
                    AlarmManagementConfig.AlarmCode = AlarmCode;
                    AlarmManagementConfig.AlarmLevel = (AlarmLevelEnum)AlarmLevel;
                    result = await _db.UpdateableWithAttr(AlarmManagementConfig)
                           .IgnoreColumns(it => new { it.CreateTime })
                           //.Where(t => t.Id == AlarmManagementConfig.Id)
                           .ExecuteCommandAsync();

                }
                if (result > 0)
                {
                    _message_service.ShowMessage($"{(AlarmManagementConfig == null ? "新增" : "更新")}成功");
                    if (AlarmManagementConfig != null)
                        RequestClose?.Invoke(this, AlarmManagementConfig); // 通知关闭，返回 true 表示成功
                }
                else
                {
                    _message_service.ShowError($"{(AlarmManagementConfig == null ? "新增" : "更新")}失败");
                }
            }
            catch (Exception ex)
            {
                _message_service.ShowError($"操作失败: {ex.Message}");
                _logger.Error($"AddOrUpdateAlarmInfoLevelViewModel", ex);
            }
        }
        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }
        /// <summary>
        /// 从枚举加载寄存器选项
        /// </summary>
        public void LoadCoordinateSystemOptions()
        {

            // 初始化枚举选项（自动获取Description）
            foreach (var value in Enum.GetValues(typeof(AlarmLevelEnum)))
            {
                if ((int)value == AlarmLevelEnum.Error.GetHashCode())
                    break;
                var description = UniversalValueConversion.GetDescription((AlarmLevelEnum)value);
                AlarmLevelList.Add(new EnumDescriptionRegisterAddress
                {
                    Value = (int)value,
                    Description = description
                });
            }
            _alarmLevel = AlarmLevelEnum.ErrorLevel.GetHashCode(); // 默认选择ErrorLevel
        }
    }
}

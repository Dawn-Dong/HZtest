using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Services;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    public class ConfigAlarmInfoLevelViewModel : IDialogAware, INotifyPropertyChanged
    {
        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;
        private readonly IStructuredLogger _logger;
        private readonly SqlSugarClient _db;

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;

        #region UI按钮命令      
        /// <summary>
        /// 关闭窗口命令
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 搜索查询命令
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// 新增报警配置命令
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// 行编辑命令
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// 删除行命令
        /// </summary>

        public ICommand DeleteCommand { get; }

        #endregion


        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public async void OnDialogOpened(object parameter)
        {
            if (parameter is int currentMode)
            {
                var CurrentMode = currentMode;  // 初始化当前选中项
            }
            await QueryAlarmConfigLevel();

        }
        #region UI绑定属性
        private string _searchAlarmCode = string.Empty;
        public string SearchAlarmCode
        {
            get => _searchAlarmCode;
            set { _searchAlarmCode = value; OnPropertyChanged(); }
        }

        // 或每次赋值后通知
        private List<AlarmManagementConfigModel> _alarmConfigList;
        public List<AlarmManagementConfigModel> AlarmConfigList
        {
            get => _alarmConfigList;
            set { _alarmConfigList = value; OnPropertyChanged(); }
        }

        #endregion


        public ConfigAlarmInfoLevelViewModel(IDialogService dialogService, DeviceService deviceService, IMessageService messageService, IStructuredLogger logger, SqlSugarClient db)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            CloseCommand = new RelayCommand(Cancel);
            SearchCommand = new AsyncRelayCommand(Search);
            AddCommand = new AsyncRelayCommand(AddAlarmConfig);
            EditCommand = new AsyncRelayCommand<AlarmManagementConfigModel>(EditAlarmConfig);
        }


        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }

        /// <summary>
        /// 筛选数据
        /// </summary>
        private async Task Search()
        {
            await QueryAlarmConfigLevel(SearchAlarmCode);
        }
        /// <summary>
        /// 新增报警配置
        /// </summary>
        private async Task AddAlarmConfig()
        {
            try
            {
                // 打开新增报警配置窗口
                var result = await _dialogService.ShowDialogAsync<AlarmManagementConfigModel?>("AddOrUpdateAlarmInfoLevel", allowMultiLayer: true);
                if (result.Success)
                {
                    await QueryAlarmConfigLevel(SearchAlarmCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("ConfigAlarmInfoLevelViewModel", "新增报警失败", ex);
                _message_service.ShowMessage("查询报警配置失败");
            }
        }

        /// <summary>
        /// 查询报警配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task QueryAlarmConfigLevel(string alarmCode = null)
        {
            try
            {
                AlarmConfigList = await _db.QueryableWithAttr<AlarmManagementConfigModel>()
                                            .WhereIF(alarmCode != null, x => x.AlarmCode.Contains(alarmCode))
                                            .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("查询报警配置失败", ex);
                _message_service.ShowMessage("查询报警配置失败");
            }

        }
        /// <summary>
        /// 编辑报警配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private async Task EditAlarmConfig(AlarmManagementConfigModel config)
        {
            try
            {
                // 打开编辑窗口，传递当前选中项的ID
                var result = await _dialogService.ShowDialogAsync<AlarmManagementConfigModel?>("AddOrUpdateAlarmInfoLevel", config, allowMultiLayer: true);
                if (result != null)
                {
                    await QueryAlarmConfigLevel(SearchAlarmCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("编辑报警配置失败", ex);
                _message_service.ShowMessage($"编辑报警配置失败:{ex.Message}");
            }
        }

    }
}

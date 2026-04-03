using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models.DB;
using HZtest.Services;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    public class OrderManagementPageViewModel : PageViewModelBaseClass
    {
        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly DeviceService _deviceService;
        private readonly IMessageService _message_service;
        private readonly IStructuredLogger _logger;
        private readonly SqlSugarClient _Db;
        #region UI 属性
        // UI属性
        private List<OrderManagementModel> _orderManagementList { get; set; } = new List<OrderManagementModel>();

        public List<OrderManagementModel> OrderManagementList
        {
            get => _orderManagementList;
            set
            {
                if (_orderManagementList != value)
                {
                    _orderManagementList = value;
                    OnPropertyChanged(nameof(OrderManagementList));
                }
            }
        }


        private string _searchOrderCode;
        public string SearchOrderCode
        {
            get => _searchOrderCode;
            set
            {
                if (_searchOrderCode != value)
                {
                    _searchOrderCode = value;
                    OnPropertyChanged(nameof(SearchOrderCode));
                }
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
        #endregion
        #region 按钮命令
        //按钮命令
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


        public OrderManagementPageViewModel(IDialogService dialogService, DeviceService deviceService, IMessageService message_service, IStructuredLogger logger, SqlSugarClient db)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SearchCommand = new RelayCommand(SearchOrdersAsync);
            AddCommand = new AsyncRelayCommand(AddOrderAsync);
            EditCommand = new AsyncRelayCommand<OrderManagementModel>(EditOrder);
            DeleteCommand = new AsyncRelayCommand<OrderManagementModel>(DeleteOrder);
            SearchOrdersAsync();
        }

        /// <summary>
        /// 模糊查询订单 - 根据订单编号进行模糊查询，并按创建时间降序排序
        /// </summary>
        private async void SearchOrdersAsync()
        {
            try
            {
                // 显示加载提示
                IsLoading = true;
                OrderManagementList = await _Db.QueryableWithAttr<OrderManagementModel>()
                            .WhereIF(!string.IsNullOrWhiteSpace(SearchOrderCode), o => o.OrderCode.Contains(SearchOrderCode))
                            .OrderBy(o => o.CreateTime, OrderByType.Desc)
                            .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Error("查询订单时发生错误", ex);
                _message_service.ShowMessage("查询订单失败，请稍后再试。");
            }
            finally
            {
                // 隐藏加载提示
                IsLoading = false;
            }
        }
        /// <summary>
        /// 新增订单
        /// </summary>
        private async Task AddOrderAsync()
        {
            try
            {
                // 打开新增订单窗口
                var result = await _dialogService.ShowDialogAsync<OrderManagementModel?>("AddOrUpdateOrder");
                if (result.Success)
                {
                    // 刷新订单列表
                    SearchOrdersAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("新增订单失败", ex);
                _message_service.ShowMessage("新增订单失败，请稍后再试。");
            }

        }
        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private async Task EditOrder(OrderManagementModel config)
        {
            try
            {
                // 打开编辑窗口，传递当前选中项的ID
                var result = await _dialogService.ShowDialogAsync<OrderManagementModel?>("AddOrUpdateOrder", config, allowMultiLayer: true);
                if (result != null)
                {
                    SearchOrdersAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("编辑订单失败", ex);
                _message_service.ShowError($"编辑订单失败:{ex.Message}");
            }

        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private async Task DeleteOrder(OrderManagementModel config)
        {
            try
            {
                var result = await _Db.DeleteableWithAttr<OrderManagementModel>()
                           .Where(x => x.Id == config.Id)
                           .ExecuteCommandAsync();
                if (result > 0)
                {
                    _logger.Info($"删除订单成功:{config}");
                    _message_service.ShowMessage($"删除订单成功");
                }
                else
                {
                    _logger.Info($"删除订单失败:{config}");
                    _message_service.ShowError($"删除订单失败");
                }
                SearchOrdersAsync();

            }
            catch (Exception ex)
            {
                _logger.Error("删除订单失败", ex);
                _message_service.ShowError($"编辑订单失败:{ex.Message}");
            }

        }



    }
}

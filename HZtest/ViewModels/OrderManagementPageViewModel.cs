using Azure;
using Azure.Core;
using HZtest.Event;
using HZtest.Event.EventBus;
using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
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

        /// <summary>
        /// 任务命令 （可开启可关闭）
        /// </summary>
        public ICommand TeskCommand { get; }

        /// <summary>
        /// 订单详情命令
        /// </summary>
        public ICommand ViewDetailsCommand { get; }

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
            TeskCommand = new AsyncRelayCommand<OrderManagementModel>(TeskCommandExecute);
            ViewDetailsCommand = new AsyncRelayCommand(OpenOrderDetailsView);
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
                OrderManagementList.Clear();
                OrderManagementList = await _Db.QueryableWithAttr<OrderManagementModel>()
                            .WhereIF(!string.IsNullOrWhiteSpace(SearchOrderCode), o => o.OrderCode.Contains(SearchOrderCode))
                            .OrderBy(o => o.CreateTime, OrderByType.Desc)
                            .ToListAsync();
                IsLoading = false;
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

        /// <summary>
        /// 开启订单 关闭订单（开启/关闭任务）
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private async Task TeskCommandExecute(OrderManagementModel order)
        {

            try
            {
                if (order.OrderState == Models.OrderStateEnum.Pending)
                {
                    #region 状态校验
                    // 开启订单前，校验是否有其他订单正在执行中
                    var OpenTaskExistencen = await _Db.QueryableWithAttr<OrderManagementModel>()
                                               .AnyAsync(x => x.OrderState == OrderStateEnum.Processing);
                    if (OpenTaskExistencen)
                    {
                        _message_service.ShowError($"订单开启失败，已有任务在执行中。");
                        return;
                    }
                    var orderStartRequest = new OrderStartRequestEventArgs()
                    {
                        OrderManagementModel = order,
                        OrderCode = order.OrderCode,
                        Timestamp = DateTime.Now

                    };

                    //通过总线发送消息，通知订单服务
                    var bus = OrderEventBus.Instance;
                    //做前置校验并且等待返回结果
                    var response = await bus.PublishAndWaitForResponseAsync(order);
                    if (response != null && !response.IsValid)
                    {
                        _logger.Info($"订单{order.OrderCode} 预校验失败，信息：{response.ValidationMessage}");
                        _message_service.ShowError($"订单开启失败，{response.ValidationMessage}");
                        return;
                    }

                    #endregion

                    order.OrderState = OrderStateEnum.Processing;

                    var result = await _Db.UpdateableWithAttr(order)
                                    .IgnoreColumns(it => it.CreateTime)
                                    .ExecuteCommandAsync();
                    //通知订单开始
                    bus.PublishOrderStartRequest(orderStartRequest);
                    _logger.Info($"手动开启订单{order.OrderCode}  状态由{order.OrderState}修改为{OrderStateEnum.Processing}{(result != 0 ? "成功" : "失败")}");
                    _message_service.ShowMessage($"开启订单{(result != 0 ? "成功" : "失败")}");


                }
                else
                {
                    order.OrderState = OrderStateEnum.Failed;
                    order.ModifyTime = DateTime.Now;
                    //通过总线发送消息，通知订单服务开启订单
                    var bus = OrderEventBus.Instance;
                    var orderCloseRequest = new OrderCloseEventArgs()
                    {
                        OrderManagementModel = order,
                        OrderCode = order.OrderCode,
                        Timestamp = DateTime.Now,
                        isManualClose = true
                    };
                    bus.PublishOrderClose(orderCloseRequest);

                    var result = await _Db.UpdateableWithAttr(order)
                        .IgnoreColumns(it => it.CreateTime)
                        .ExecuteCommandAsync();
                    _logger.Info($"订单{order.OrderCode}  状态由{order.OrderState}修改为{OrderStateEnum.Failed}{(result != 0 ? "成功" : "失败")}");
                    _message_service.ShowMessage($"关闭订单{(result != 0 ? "成功" : "失败")}");

                }
                SearchOrdersAsync();

            }
            catch (Exception ex)
            {
                _logger.Error("订单状态修改失败", ex);
                _message_service.ShowError($"订单状态修改失败:{ex.Message}");
            }

        }

        /// <summary>
        /// 查看订单详情
        /// </summary>
        /// <returns></returns>
        private async Task OpenOrderDetailsView()
        {
            try
            {
                var result = await _dialogService.ShowDialogAsync<OrderDetailsModel?>("OrderDetails", allowMultiLayer: true);
            }
            catch (Exception ex)
            {
                _logger.Error("查看订单详情失败", ex);
                _message_service.ShowError($"查看订单详情失败:{ex.Message}");
            }
        }
    }
}

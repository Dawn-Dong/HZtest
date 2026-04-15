using HZtest.Event;
using HZtest.Event.EventBus;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.DB;
using HZtest.Models.Request;
using SqlSugar;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Claims;

namespace HZtest.Services.Processor
{
    /// <summary>
    /// 订单处理器 - 负责所有订单生命周期管理
    /// 单例模式，后台常驻
    /// </summary>
    public class OrderProcessor : IDisposable
    {

        private readonly ConcurrentDictionary<string, RunningOrderContext> _runningOrders =
        new ConcurrentDictionary<string, RunningOrderContext>();


        private readonly DeviceService _deviceService;
        private readonly IStructuredLogger _logger;
        private readonly IMessageService _messageService;
        private readonly SqlSugarClient _db;


        private readonly CancellationTokenSource _processorCts = new CancellationTokenSource();

        public OrderProcessor(DeviceService deviceService, IStructuredLogger logger, SqlSugarClient db, IMessageService messageService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(_deviceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
            _db = db ?? throw new ArgumentNullException(nameof(_db));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(_messageService));

            // ═══════════════════════════════════════════════════════
            // 关键：订阅事件总线，等待UI层的发号施令
            // ═══════════════════════════════════════════════════════
            OrderEventBus.Instance.OrderStartRequested += OnOrderStartRequested;
            OrderEventBus.Instance.OrderEnablePreValidationRequest += OnOrderEnablePreValidationRequest;
            OrderEventBus.Instance.OrderClose += OnOrderCloseRequested;

        }
        /// <summary>
        ///  处理订单开启请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnOrderStartRequested(object sender, OrderStartRequestEventArgs e)
        {
            try
            {
                //上传加工文件到设备必须要有名称才行   并且切换运行的加工文件为刚上传的
                //先生成机床的文件名称
                var deviceFileName = $"{e.OrderCode}_{DateTime.Now:yyyyMMddHHmmss}";
                var fileUploadRequest = new FileUploadRequest
                {
                    LocalFilePath = e.OrderManagementModel.ProcessingPartFilePath,
                    FileName = deviceFileName
                };
                var uploadFileResponse = await _deviceService.UploadFileAsync(fileUploadRequest);
                if (uploadFileResponse.Code != 0)
                {

                    _logger.Error("OrderProcessor", $"订单{e.OrderCode}开启失败，上传文件到设备失败: {uploadFileResponse.Status}");
                    var result = await _db.UpdateableWithAttr<OrderManagementModel>().SetColumns(it => new OrderManagementModel { OrderState = OrderStateEnum.Pending })
                        .Where(it => it.OrderCode == e.OrderCode).ExecuteCommandAsync();
                    _logger.Info("OrderProcessor", $"订单{e.OrderCode}状态更新为待处理，受影响行数: {result}");
                    _messageService.ShowError($"订单{e.OrderCode}开启失败，上传文件到设备失败: {uploadFileResponse.Status}");
                    return;
                }
                //切换运行的加工文件为刚上传的
                var switchFileResponse = await _deviceService.SetSwitchRunningFileAsync($"../prog/{deviceFileName}");
                if (!switchFileResponse.Value)
                {

                    _logger.Error("OrderProcessor", $"订单{e.OrderCode}开启失败，切换运行文件失败");
                    var result = await _db.UpdateableWithAttr<OrderManagementModel>().SetColumns(it => new OrderManagementModel { OrderState = OrderStateEnum.Pending })
                        .Where(it => it.OrderCode == e.OrderCode).ExecuteCommandAsync();
                    _logger.Info("OrderProcessor", $"订单{e.OrderCode}状态更新为待处理，受影响行数: {result}");
                    _messageService.ShowError($"订单{e.OrderCode}开启失败，切换运行文件失败");
                    return;
                }
                _logger.Info("OrderProcessor", $"订单{e.OrderCode}已成功上传文件并切换运行文件，订单开启成功");

                var context = new RunningOrderContext
                {
                    OrderId = e.OrderManagementModel.Id,
                    OrderCode = e.OrderCode,
                    TargetQuantity = e.OrderManagementModel.OrderQuatity,
                    CurrentQuantity = 0,
                    Status = OrderStateEnum.Processing,
                    DeviceFileName = deviceFileName,
                    MonitorCts = new CancellationTokenSource()
                };

                if (!_runningOrders.TryAdd(e.OrderCode, context))
                {
                    _messageService.ShowError($"订单{e.OrderCode}已在运行中");
                    return;
                }
                //启动循环
                context.MonitorTask = Task.Run(() => MonitorProductionLoopAsync(context));

                _logger.Info("OrderProcessor", $"订单{e.OrderCode}已启动监控，目标数量：{context.TargetQuantity}");

            }
            catch (Exception ex)
            {

                _logger.Error("OrderProcessor", $"订单{e.OrderCode}开启过程中发生异常: {ex.Message}");
                _messageService.ShowError($"订单{e.OrderCode}开启过程中发生异常: {ex.Message}");
            }
        }
        /// <summary>
        ///  订单开启前验证
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnOrderEnablePreValidationRequest(object sender, OrderEnablePreValidationRequestEventArgs e)
        {

            try
            {
                var validateResult = await ValidateOrder(e);

                var response = new OrderEnablePreValidationResponseEventArgs
                {
                    OrderCode = e.OrderManagementModel.OrderCode,
                    Timestamp = DateTime.Now,
                    IsValid = validateResult.IsValid, // 这里应该调用实际的验证逻辑
                    ValidationMessage = validateResult.message
                };
                OrderEventBus.Instance.PublishOrderStartResponse(response);
            }
            catch (Exception ex)
            {
                _logger.Error("OrderProcessor", $"订单{e.OrderCode}开启前验证过程中发生异常: {ex.Message}");
            }
        }
        /// <summary>
        /// 校验订单是否合法（核心方法）
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        private async Task<(bool IsValid, string message)> ValidateOrder(OrderEnablePreValidationRequestEventArgs orderEnablePre)
        {
            try
            {


                // 这里应该调用实际的验证逻辑
                //校验文件是否存在
                if (!File.Exists(orderEnablePre.OrderManagementModel.ProcessingPartFilePath))
                {
                    return (IsValid: false, "订单对应加工文件不存在");
                }
                //校验设备是否 空闲 可用 无报警 
                var deviceState = await _deviceService.GetDeviceStateAsync();
                if (deviceState.Value != DeviceStateEnum.Free)
                {
                    return (IsValid: false, "设备不为空闲状态无法开启");
                }
                var deviceAlarmInfo = await _deviceService.GetDeviceAlarmInforAsync();
                if (deviceAlarmInfo.Value.Count != 0)
                {
                    return (IsValid: false, "设备存在报警无法开启");
                }
                var deviceOperationMode = await _deviceService.GetOperationModeAsync();
                if (deviceOperationMode.Value != DevOperationModeEnum.Auto)
                {
                    return (IsValid: false, "设备不是自动模式不可开启");
                }

                return (true, "验证成功");
            }
            catch (Exception ex)
            {
                _logger.Error("OrderProcessor", $"订单验证过程中发生异常: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// 监控生产循环  记录生产数量和状态  直到完成或异常（核心方法）
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task MonitorProductionLoopAsync(RunningOrderContext context)
        {
            var orderCode = context.OrderCode;
            var token = context.MonitorCts.Token;
            context.CurrentPartNumber = 1;
            _logger.Info("OrderProcessor", $"订单{orderCode}监控循环已启动，目标{context.TargetQuantity}件");
            try
            {
                while (!token.IsCancellationRequested)
                {

                    //获取设备启动和暂停按钮状态
                    var deviceOperatingStatus = await _deviceService.GetStartPauseStateAsync();
                    //获取设备报警信息
                    var deviceAlarmInfo = await _deviceService.GetDeviceAlarmInforAsync();
                    if (deviceAlarmInfo.Value?.Count > 0)
                    {
                        ///记录异常，结束当前件（失败）
                        await CompletePartAsync(context, isNormal: false, errorMessage: $"设备报警: {deviceAlarmInfo.Value}");
                        context.IsDeviceNormal = false;
                        continue;
                    }

                    //获取设备运行状态
                    var deviceState = await _deviceService.GetDeviceStateAsync();
                    if (deviceState.Value != DeviceStateEnum.Running)
                    {
                        // 记录异常，结束当前件（失败）
                        await CompletePartAsync(context, isNormal: false, errorMessage: $"设备状态异常: {deviceState.Value}");
                        context.IsDeviceNormal = false;
                        continue;
                    }
                    //获取设备运行模式
                    var deviceOperationMode = await _deviceService.GetOperationModeAsync();
                    if (deviceOperationMode.Value != DevOperationModeEnum.Auto)
                    {
                        ///记录异常，结束当前件（失败）
                        await CompletePartAsync(context, isNormal: false, errorMessage: $"设备运行模式不是自动模式:变为》》 {deviceOperationMode.Value}");
                        context.IsDeviceNormal = false;
                        continue;
                    }
                    var cycleStart = deviceOperatingStatus.Value.CycleStart;
                    if (context.IsDeviceNormal && cycleStart && !context.CurrentPartStartTime.HasValue)
                    {
                        context.CurrentPartStartTime = DateTime.Now;
                        context.LastCycleStart = true;
                        _logger.Info("OrderProcessor", $"第{context.CurrentPartNumber}件开始 {context.CurrentPartStartTime:HH:mm:ss}");
                    }

                    // 检测CycleStart下降沿（件结束）或产量增加
                    if (context.IsDeviceNormal && !cycleStart && context.LastCycleStart && context.CurrentPartStartTime.HasValue)
                    {
                        await CompletePartAsync(context, isNormal: true);
                        continue;
                    }
                    await Task.Delay(300); // 每隔300ms更新一次
                }
            }
            catch (Exception ex)
            {
                _logger.Error("OrderProcessor", $"订单{context.OrderCode}监控循环发生异常已结束: {ex.Message}");
                context.Status = OrderStateEnum.Error;
            }
            finally
            {

                _logger.Info("OrderProcessor", $"订单{context.OrderCode}监控循环已结束，订单上下文已移除");
            }
        }
        /// <summary>
        /// 完成当前件（正常或异常）
        /// </summary>
        private async Task CompletePartAsync(RunningOrderContext context, bool isNormal,
            string errorCode = null, string errorMessage = null)
        {
            var record = new OrderDetailsModel
            {
                Id = SnowFlakeSingle.Instance.NextId(),
                FK_OrderManagementId = context.OrderId,
                StartTime = context.CurrentPartStartTime,
                EndTime = DateTime.Now,
                CreateTime = DateTime.Now,
                OrderDetailsType = isNormal ? OrderDetailsEnum.NormalCompletion : OrderDetailsEnum.ProcessingAnomaly,
                Remark = errorMessage
            };
            context.OrderDetailsList.Add(record);

            context.CurrentQuantity++;

            _logger.Info("OrderProcessor",
                $"第{context.CurrentPartNumber}件完成 {(isNormal ? "✓" : "✗")} " +
                $"耗时{record.Duration?.TotalSeconds:F1}秒" +
                (isNormal ? "" : $" 异常:{errorMessage}"));

            if (context.CurrentQuantity == context.TargetQuantity)
            {
                //结束订单循环监控
                context.MonitorCts.Cancel();
                OnOrderCloseRequested("1", new OrderCloseEventArgs() { OrderManagementModel = new OrderManagementModel() { Id = context.OrderId } });
                _logger.Info("OrderProcessor", $"订单{context.OrderCode}监控循环已结束，订单状态已更新为完成");
                var result = await _db.InsertableWithAttr(context.OrderDetailsList).ExecuteCommandAsync();
                _logger.Info("OrderProcessor", $"订单{context.OrderCode}完成，订单详情已记录");
            }
            // 准备下一件
            context.CurrentPartNumber++;
            context.CurrentPartStartTime = null;
        }

        /// <summary>
        /// 关闭订单监控（核心方法）  可能是UI层的关闭按钮触发，  需要优雅地结束所有监控循环，释放资源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnOrderCloseRequested(object sender, OrderCloseEventArgs e)
        {
            _logger.Info("OrderProcessor", $"订单关闭请求已收到，正在关闭所有订单监控...");

            if (_runningOrders.Count == 0)
            {
                _logger.Info("OrderProcessor", $"当前没有正在运行的监控，无需关闭循环");
                var result = await _db.UpdateableWithAttr<OrderManagementModel>().SetColumns(it => new OrderManagementModel { OrderState = OrderStateEnum.Failed })
                         .Where(it => it.Id == e.OrderManagementModel.Id).ExecuteCommandAsync();
                return;
            }
            foreach (var kvp in _runningOrders)
            {
                var context = kvp.Value;
                context.MonitorCts.Cancel();
                if (e.isManualClose)
                {
                    _logger.Info("OrderProcessor", $"订单{context.OrderCode}监控已请求取消（手动关闭）");
                }
                else
                {
                    _logger.Info("OrderProcessor", $"订单{context.OrderCode}监控已请求取消（自动关闭）");
                    var result = await _db.UpdateableWithAttr<OrderManagementModel>().SetColumns(it => new OrderManagementModel { OrderState = OrderStateEnum.Completed })
                       .Where(it => it.Id == e.OrderManagementModel.Id).ExecuteCommandAsync();
                }
                // 清理资源，移除订单上下文
                _runningOrders.TryRemove(kvp.Key, out _);
            }
        }
        public void Dispose()
        {
            _processorCts.Cancel();
        }
    }


    // 辅助类
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Fail(string msg) => new ValidationResult { IsValid = false, ErrorMessage = msg };
    }

    /// <summary>
    /// 订单运行上下文 - 记录订单的实时状态和生产信息（核心类，贯穿订单生命周期）
    /// </summary>
    public class RunningOrderContext
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }           // 订单编号
        /// <summary>
        /// 目标数量
        /// </summary>
        public int TargetQuantity { get; set; }         // 目标数量（10件）
        /// <summary>
        /// 当前完成数
        /// </summary>
        public int CurrentQuantity { get; set; }        // 当前完成数

        /// <summary>
        /// 当前零件序号
        /// </summary>
        public int CurrentPartNumber { get; set; }
        /// <summary>
        /// 当前零件开始时间
        /// </summary>
        public DateTime? CurrentPartStartTime { get; set; }
        /// <summary>
        /// 状态枚举：待开始 运行中、完成、异常等（核心字段，控制订单生命周期）
        /// </summary>
        public OrderStateEnum Status { get; set; }      // 运行中/暂停/完成等
        /// <summary>
        /// 设备上对应的文件名
        /// </summary>
        public string DeviceFileName { get; set; }      // 设备上的文件名

        /// <summary>
        /// 订单详情列表（记录每次状态变化的时间戳和备注，核心字段，用于后续分析和追溯）
        /// </summary>
        public List<OrderDetailsModel> OrderDetailsList { get; set; } = new List<OrderDetailsModel>();
        // 关键：控制监控循环的生命周期
        public CancellationTokenSource MonitorCts { get; set; }

        // 监控任务引用（用于等待完成）
        public Task MonitorTask { get; set; }
        /// <summary>
        /// 当前件状态（内部状态机）
        /// </summary>
        public PartState CurrentPartState { get; set; } = PartState.Waiting;
        /// <summary>
        /// 上次启动按钮状态
        /// </summary>
        public bool LastCycleStart { get; set; }

        /// <summary>
        /// 设备是否正常  true 正常 false  异常
        /// </summary>
        public bool IsDeviceNormal { get; set; } = true;
    }
}

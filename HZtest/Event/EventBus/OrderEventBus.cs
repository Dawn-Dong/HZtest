using HZtest.Models.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Event.EventBus
{
    public class OrderEventBus
    {
        private static readonly Lazy<OrderEventBus> _instance =
       new Lazy<OrderEventBus>(() => new OrderEventBus());
        public static OrderEventBus Instance => _instance.Value;

        /// <summary>
        ///  订单开始事件
        /// </summary>
        public event EventHandler<OrderStartRequestEventArgs> OrderStartRequested;
        /// <summary>
        ///  订单启用预校验请求事件
        /// </summary>
        public event EventHandler<OrderEnablePreValidationRequestEventArgs> OrderEnablePreValidationRequest;

        /// <summary>
        /// 订单关闭事件
        /// </summary>
        public event EventHandler<OrderCloseEventArgs> OrderClose;

        /// <summary>
        /// 发布订单开始事件
        /// </summary>
        /// <param name="args"></param>
        public void PublishOrderStartRequest(OrderStartRequestEventArgs args) =>
            OrderStartRequested?.Invoke(this, args);

        /// <summary>
        /// 发布订单关闭事件
        /// </summary>
        public void PublishOrderClose(OrderCloseEventArgs args) =>
            OrderClose?.Invoke(this, args);

        /// <summary>
        /// 发布请求并等待响应（核心方法）
        /// </summary>
        public async Task<OrderEnablePreValidationResponseEventArgs> PublishAndWaitForResponseAsync(
           OrderManagementModel request,
            int timeoutMs = 5000)
        {
            var tcs = new TaskCompletionSource<OrderEnablePreValidationResponseEventArgs>();

            // 使用弱引用避免内存泄漏
            var handler = new EventHandler<OrderEnablePreValidationResponseEventArgs>((s, e) =>
            {
                if (e.OrderCode == request.OrderCode)
                {
                    tcs.TrySetResult(e);
                }
            });

            OrderEnablePreValidationResponse += handler;

            try
            {


                // 发布请求
                OrderEnablePreValidationRequest?.Invoke(this, new OrderEnablePreValidationRequestEventArgs() { OrderManagementModel = request });

                // 等待响应
                using var cts = new CancellationTokenSource(timeoutMs);
                await using (cts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                OrderEnablePreValidationResponse -= handler;
            }
        }



        // 响应事件（内部使用）
        public event EventHandler<OrderEnablePreValidationResponseEventArgs> OrderEnablePreValidationResponse;

        /// <summary>
        /// 发布订单启用预校验响应事件
        /// </summary>
        /// <param name="e"></param>
        public void PublishOrderStartResponse(OrderEnablePreValidationResponseEventArgs e) =>
            OrderEnablePreValidationResponse?.Invoke(this, e);


    }
}

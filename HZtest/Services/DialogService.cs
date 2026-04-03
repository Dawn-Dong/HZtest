using HZtest.Interfaces_接口定义;
using HZtest.Services;
using HZtest.Services.ResultModel;
using HZtest.View.Dialogs;
using HZtest.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace HZtest.Services
{
    public class DialogService : IDialogService
    {
        private Grid _rootGrid;
        private readonly IServiceProvider _serviceProvider;

        // 弹窗栈：支持多层
        private readonly Stack<DialogLayer> _dialogStack = new();

        // 单层模式：复用同一个容器
        private Grid _singleOverlay;
        private ContentControl _singleContainer;

        private class DialogLayer
        {
            public Grid Overlay { get; set; }
            public ContentControl Container { get; set; }
            public IDialogAware ViewModel { get; set; }
            public TaskCompletionSource<object> Tcs { get; set; }
            public string DialogName { get; set; }
            public DateTime OpenTime { get; set; }
        }
        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public void Initialize(Grid rootGrid)
        {
            _rootGrid = rootGrid;
            InitializeSingleMode(); // 预创建单层容器
        }

        /// <summary>
        /// 初始化单层模式容器
        /// </summary>
        private void InitializeSingleMode()
        {
            _singleOverlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = true
            };

            _singleContainer = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            _singleOverlay.Children.Add(_singleContainer);

            if (_rootGrid.RowDefinitions?.Count > 0)
                Grid.SetRowSpan(_singleOverlay, _rootGrid.RowDefinitions.Count);
            if (_rootGrid.ColumnDefinitions?.Count > 0)
                Grid.SetColumnSpan(_singleOverlay, _rootGrid.ColumnDefinitions.Count);

            Panel.SetZIndex(_singleOverlay, 9999);
            _rootGrid.Children.Add(_singleOverlay);
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        /// <param name="dialogName">对话框名称</param>
        /// <param name="parameter">传入参数</param>
        /// <param name="allowMultiLayer">true=允许多层弹窗, false=单层模式(默认)</param>
        public async Task<DialogResult<TResult>> ShowDialogAsync<TResult>(
            string dialogName,
            object parameter = null,
            bool allowMultiLayer = false)
        {
            if (_rootGrid == null)
                throw new InvalidOperationException("DialogService 未初始化");

            // 单层模式：关闭已有弹窗
            if (!allowMultiLayer && _dialogStack.Count > 0)
            {
                await CloseTopDialogAsync();
            }

            object rawResult;
            if (allowMultiLayer)
            {
                rawResult = await ShowMultiLayerAsync<object>(dialogName, parameter);
            }
            else
            {
                rawResult = await ShowSingleLayerAsync<object>(dialogName, parameter);
            }

            // 解析结果
            return ParseResult<TResult>(rawResult);
        }

        private DialogResult<TResult> ParseResult<TResult>(object rawResult)
        {
            // 情况1：null → 取消/失败
            if (rawResult == null)
                return DialogResult<TResult>.Cancel();

            // 情况2：bool → true=成功但无数据(返回default)，false=取消
            if (rawResult is bool b)
                return b
                    ? new DialogResult<TResult> { Success = true, Data = default }
                    : DialogResult<TResult>.Cancel();

            // 情况3：TResult 类型 → 成功
            if (rawResult is TResult data)
                return DialogResult<TResult>.Ok(data);

            // 情况4：其他类型 → 尝试转换，失败则取消
            try
            {
                return DialogResult<TResult>.Ok((TResult)rawResult);
            }
            catch
            {
                return DialogResult<TResult>.Cancel();
            }
        }

        public Task<bool> ShowConfirmAsync(string message, string title = "确认")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        /// <summary>
        /// 单层模式显示
        /// </summary>
        private async Task<TResult> ShowSingleLayerAsync<TResult>(string dialogName, object parameter)
        {
            var dialog = CreateDialog(dialogName);

            // 清理旧事件
            if (_singleContainer.Content is FrameworkElement oldElement
                && oldElement.DataContext is IDialogAware oldVm)
            {
                oldVm.RequestClose -= OnSingleLayerClose;
            }

            // 设置新弹窗
            if (dialog is FrameworkElement fe && fe.DataContext is IDialogAware vm)
            {
                vm.OnDialogOpened(parameter);
                vm.RequestClose += OnSingleLayerClose;
            }

            _singleContainer.Content = dialog;
            _singleOverlay.Visibility = Visibility.Visible;

            // 创建新的 TCS（单层模式只有一个）
            var tcs = new TaskCompletionSource<object>();
            _dialogStack.Push(new DialogLayer
            {
                Overlay = _singleOverlay,
                Container = _singleContainer,
                ViewModel = dialog.DataContext as IDialogAware,
                Tcs = tcs,
                DialogName = dialogName,
                OpenTime = DateTime.Now
            });

            return (TResult)await tcs.Task;
        }

        /// <summary>
        /// 单层模式关闭处理
        /// </summary>
        private void OnSingleLayerClose(object sender, object result)
        {
            if (_dialogStack.Count == 0) return;

            var layer = _dialogStack.Pop();
            layer.ViewModel.RequestClose -= OnSingleLayerClose;

            _singleOverlay.Visibility = Visibility.Collapsed;
            _singleContainer.Content = null;

            layer.Tcs.SetResult(result);
        }

        /// <summary>
        /// 多层模式显示
        /// </summary>
        private async Task<TResult> ShowMultiLayerAsync<TResult>(string dialogName, object parameter)
        {
            var dialog = CreateDialog(dialogName);

            if (dialog is FrameworkElement fe && fe.DataContext is IDialogAware vm)
            {
                vm.OnDialogOpened(parameter);
            }

            // 创建新遮罩层
            var layer = CreateDialogLayer(dialog, dialogName);
            _dialogStack.Push(layer);

            // 处理下层
            if (_dialogStack.Count > 1)
            {
                var belowLayer = _dialogStack.ElementAt(1); // 获取下层
                belowLayer.Overlay.IsHitTestVisible = false;
                belowLayer.Overlay.Opacity = 0.5; // 变暗
            }

            _rootGrid.Children.Add(layer.Overlay);
            Panel.SetZIndex(layer.Overlay, 1000 + _dialogStack.Count);

            return (TResult)await layer.Tcs.Task;
        }

        /// <summary>
        /// 创建对话框层
        /// </summary>
        private DialogLayer CreateDialogLayer(UserControl dialog, string dialogName)
        {
            var overlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            if (_rootGrid.RowDefinitions?.Count > 0)
                Grid.SetRowSpan(overlay, _rootGrid.RowDefinitions.Count);
            if (_rootGrid.ColumnDefinitions?.Count > 0)
                Grid.SetColumnSpan(overlay, _rootGrid.ColumnDefinitions.Count);

            var container = new ContentControl
            {
                Content = dialog,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            overlay.Children.Add(container);

            var tcs = new TaskCompletionSource<object>();

            if (dialog is FrameworkElement element && element.DataContext is IDialogAware aware)
            {
                EventHandler<object> handler = null;
                handler = (s, result) =>
                {
                    aware.RequestClose -= handler;
                    CloseLayer(result);
                };
                aware.RequestClose += handler;
            }

            return new DialogLayer
            {
                Overlay = overlay,
                Container = container,
                ViewModel = dialog.DataContext as IDialogAware,
                Tcs = tcs,
                DialogName = dialogName,
                OpenTime = DateTime.Now
            };
        }

        /// <summary>
        /// 关闭当前层（多层模式）
        /// </summary>
        private void CloseLayer(object result)
        {
            if (_dialogStack.Count == 0) return;

            var layer = _dialogStack.Pop();

            _rootGrid.Children.Remove(layer.Overlay);

            // 恢复下层
            if (_dialogStack.Count > 0)
            {
                var below = _dialogStack.Peek();
                below.Overlay.IsHitTestVisible = true;
                below.Overlay.Opacity = 1.0;
            }

            layer.Tcs.SetResult(result);
        }

        /// <summary>
        /// 强制关闭顶层弹窗
        /// </summary>
        private async Task CloseTopDialogAsync()
        {
            if (_dialogStack.Count == 0) return;

            var topLayer = _dialogStack.Peek();

            // 触发关闭（模拟点击关闭）
            if (topLayer.ViewModel != null)
            {
                // 发送取消结果
                topLayer.ViewModel.GetType()
                    .GetMethod("OnDialogClosed")
                    ?.Invoke(topLayer.ViewModel, new object[] { null });
            }

            // 等待关闭完成
            await Task.Delay(50);
        }

        /// <summary>
        /// 获取当前弹窗数量
        /// </summary>
        public int DialogCount => _dialogStack.Count;

        /// <summary>
        /// 获取弹窗信息（调试用）
        /// </summary>
        public IEnumerable<string> GetDialogStackInfo()
        {
            return _dialogStack.Select(l => $"{l.DialogName} ({l.OpenTime:HH:mm:ss})").Reverse();
        }

        /// <summary>
        /// 打开对话框实例（工厂方法）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private UserControl CreateDialog(string name)
        {
            return name switch
            {
                "ModeSelection" => _serviceProvider.GetRequiredService<ModeSelectionDialog>(),
                "UploadFile" => _serviceProvider.GetRequiredService<UploadFileDialogs>(),
                "ConfigAlarmInfoLevel" => _serviceProvider.GetRequiredService<ConfigAlarmInfoLevelDialogs>(),
                "AddOrUpdateAlarmInfoLevel" => _serviceProvider.GetRequiredService<AddOrUpdateAlarmInfoLevelDialogs>(),
                "AddOrUpdateOrder" => _serviceProvider.GetRequiredService<AddOrUpdateOrderDialogs>(),
                _ => throw new ArgumentException($"未知对话框: {name}")
            };
        }
    }
}
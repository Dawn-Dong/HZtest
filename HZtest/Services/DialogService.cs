using HZtest.Interfaces_接口定义;
using HZtest.Services;
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
        private Grid _overlay;
        private ContentControl _container;
        private readonly IServiceProvider _serviceProvider;
        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Initialize(Grid rootGrid)
        {
            if (rootGrid == null)
                throw new ArgumentNullException(nameof(rootGrid));

            _rootGrid = rootGrid;
            InitializeOverlay();
        }

        private void InitializeOverlay()
        {
            // 遮罩层（半透明），拉伸覆盖整个 RootGrid
            _overlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = true
            };

            // 对话框容器（居中）
            _container = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            _overlay.Children.Add(_container);

            // 如果 RootGrid 有行/列，设置覆盖所有行列
            if (_rootGrid.RowDefinitions?.Count > 0)
                Grid.SetRowSpan(_overlay, _rootGrid.RowDefinitions.Count);
            if (_rootGrid.ColumnDefinitions?.Count > 0)
                Grid.SetColumnSpan(_overlay, _rootGrid.ColumnDefinitions.Count);

            // 确保在最上层
            Panel.SetZIndex(_overlay, 9999);

            _rootGrid.Children.Add(_overlay);
        }

        public async Task<TResult> ShowDialogAsync<TResult>(string dialogName, object parameter = null)
        {
            if (_overlay == null || _container == null)
                throw new InvalidOperationException("DialogService 未初始化。请在 MainWindow.Loaded 中调用 dialogService.Initialize(rootGrid)。");

            var dialog = CreateDialog(dialogName);

            // 如果实现了 IDialogAware，传递参数
            if (dialog is FrameworkElement element && element.DataContext is IDialogAware vm)
            {
                vm.OnDialogOpened(parameter);
            }

            _container.Content = dialog;
            _overlay.Visibility = Visibility.Visible;

            var tcs = new TaskCompletionSource<TResult>();

            // 监听关闭事件
            if (dialog is FrameworkElement fe && fe.DataContext is IDialogAware aware)
            {
                EventHandler<object> handler = null;
                handler = (s, result) =>
                {
                    // 移除并完成任务
                    _overlay.Visibility = Visibility.Collapsed;
                    _container.Content = null;
                    aware.RequestClose -= handler;
                    tcs.SetResult((TResult)result);
                };
                aware.RequestClose += handler;
            }
            else
            {
                // 如果 dialog 没有 IDialogAware，仍需防止任务永远不完成 — 可根据需要抛异常或返回默认
                throw new InvalidOperationException("对话框未实现 IDialogAware，无法获取关闭事件。");
            }

            return await tcs.Task;
        }

        private UserControl CreateDialog(string name)
        {
            return name switch
            {
                "ModeSelection" => _serviceProvider.GetRequiredService<ModeSelectionDialog>(),
                "UploadFile" => _serviceProvider.GetRequiredService<UploadFileDialogs>(),
                _ => throw new ArgumentException($"未知对话框: {name}")
            };
        }

        public Task<bool> ShowConfirmAsync(string message, string title = "确认")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }


    }
}
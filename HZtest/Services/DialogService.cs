using HZtest.Interfaces_接口定义;
using HZtest.Services;
using HZtest.Views.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class DialogService : IDialogService
{
    private Grid _rootGrid;
    private Grid _overlay;
    private ContentControl _container;

    public DialogService()
    {
        
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
        // 遮罩层（半透明）
        _overlay = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
            Visibility = Visibility.Collapsed
        };

        // 对话框容器（居中）
        _container = new ContentControl
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        _overlay.Children.Add(_container);
        _rootGrid.Children.Add(_overlay);
    }
    public async Task<TResult> ShowDialogAsync<TResult>(string dialogName, object parameter = null)
    {
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
            aware.RequestClose += (s, result) =>
            {
                _overlay.Visibility = Visibility.Collapsed;
                _container.Content = null;
                tcs.SetResult((TResult)result);
            };
        }

        return await tcs.Task;
    }

    private UserControl CreateDialog(string name)
    {
        return name switch
        {
            "ModeSelection" => new ModeSelectionDialog(),
            _ => throw new ArgumentException($"未知对话框: {name}")
        };
    }




    public Task<bool> ShowConfirmAsync(string message, string title = "确认")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

}
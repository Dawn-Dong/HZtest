using HZtest.Interfaces_接口定义;
using HZtest.Services;
using System.Windows;

public class MessageService : IMessageService
{
    public void Show(string message, string title = "提示")
        => Application.Current.Dispatcher.Invoke(() =>
            MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information));

    public void ShowError(string message)
        => Application.Current.Dispatcher.Invoke(() =>
            MessageBox.Show(Application.Current.MainWindow, message, "错误", MessageBoxButton.OK, MessageBoxImage.Error));

    public void ShowMessage(string message, string title = "提示")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
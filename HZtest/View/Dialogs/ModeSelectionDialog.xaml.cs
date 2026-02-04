// 手动创建 .xaml.cs 文件后，将内容改为：
using HZtest.ViewModels.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace HZtest.Views.Dialogs
{
    public partial class ModeSelectionDialog : UserControl
    {
        /// <summary>
        /// 关闭请求事件（传递给 Service）
        /// </summary>
        public event EventHandler<object> RequestClose;
        public ModeSelectionDialog(ModeSelectionViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            // 监听 ViewModel 的关闭事件
            viewModel.RequestClose += (s, result) =>
            {
                // 将事件冒泡给 DialogService
                RequestClose?.Invoke(this, result);
            };
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // ✅ ViewModel 处理逻辑
            (DataContext as ModeSelectionViewModel)?.Confirm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // ✅ ViewModel 处理逻辑
            (DataContext as ModeSelectionViewModel)?.Cancel();
        }




    }
}
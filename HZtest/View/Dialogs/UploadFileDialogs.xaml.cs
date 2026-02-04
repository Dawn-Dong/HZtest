using HZtest.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HZtest.View.Dialogs
{
    /// <summary>
    /// UploadFileDialogs.xaml 的交互逻辑
    /// </summary>
    public partial class UploadFileDialogs : UserControl
    {
        public UploadFileDialogs(UploadFileViewModel viewModel )
        {
            InitializeComponent();

            // 使用 DI 注入的 ViewModel 作为 DataContext，避免在页面中 new 未定义的服务
            DataContext = viewModel;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // ✅ ViewModel 处理逻辑
            (DataContext as UploadFileViewModel)?.Confirm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // ✅ ViewModel 处理逻辑
            (DataContext as UploadFileViewModel)?.Cancel();
        }


    }
}

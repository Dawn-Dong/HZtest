using HZtest.Models;
using HZtest.ViewModels;
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

namespace HZtest.View
{
    /// <summary>
    /// FileOperationsPage.xaml 的交互逻辑
    /// </summary>
    public partial class FileOperationsPage : Page
    {
        public FileOperationsPage(FileOperationsPageViewModel viewModel)
        {
            InitializeComponent();
            // 使用 DI 注入的 ViewModel 作为 DataContext，避免在页面中 new 未定义的服务
            DataContext = viewModel;

            // 立即绑定回调，确保无论页面如何被创建/显示都能接收到文本更新。
            if (viewModel != null)
            {
                viewModel.FileDetailsChanged += OnFileDetailsChanged;
            }

            // 页面卸载时停止监控
            this.Unloaded += (s, e) =>
            {
                (this.DataContext as FileOperationsPageViewModel)?.Cleanup();
            };
        }
        private void OnFileDetailsChanged(string text)
        {
            Dispatcher.BeginInvoke(() => GcodeEditor.Text = text ?? string.Empty);
        }
        // 选中事件直接在这里处理
        private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // e.NewValue 就是选中的 FileNode 对象
            if (e.NewValue is FileNode node)
            {
                // 这里一定能断点进来！
                System.Diagnostics.Debug.WriteLine($"选中了：{node.Name}");

                // 传给 ViewModel
                if (DataContext is FileOperationsPageViewModel vm)
                {
                    vm.SelectedNode = node;  // 这样 ViewModel 也能收到

                    // 如果是文件，直接读取显示
                    if (!node.IsDirectory)
                    {
                        try
                        {
                            string content = System.IO.File.ReadAllText(node.FullPath);
                            GcodeEditor.Text = content;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"读取失败：{ex.Message}");
                        }
                    }
                }
            }
        }


    }
}

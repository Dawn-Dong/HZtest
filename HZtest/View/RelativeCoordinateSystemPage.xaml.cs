using HZtest.Interfaces_接口定义;
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
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class RelativeCoordinateSystemPage : Page
    {
        public RelativeCoordinateSystemPage(RelativeCoordinateSystemViewMode viewModel)
        {
            InitializeComponent();
            // 页面加载时启动监控

            // 页面卸载时停止监控
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

    }
}

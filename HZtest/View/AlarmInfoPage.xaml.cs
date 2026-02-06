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
    /// AlarmInfoPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmInfoPage : Page
    {
        // 使用 DI 注入的 ViewModel 作为 DataContext，避免在页面中 new 未定义的服务
        public AlarmInfoPage(AlarmInfoPageViewModel viewModel )
        {
            InitializeComponent();
            DataContext = viewModel;

        }



    }
}

using HZtest.Services;
using HZtest.View;
using HZtest.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HZtest
{
    /// <summary>
    /// DevConnection.xaml 的交互逻辑
    /// </summary>
    public partial class DevConnection : Page
    {
       // private readonly DeviceService _deviceService = new();

        public DevConnection()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 按钮点击事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取TextBox中的值
            string snCode = SnTextBox.Text.Trim();
            // 检查输入是否为空
            if (string.IsNullOrEmpty(snCode))
            {
                MessageBox.Show("请输入设备SN码！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TestConnectionButton.IsEnabled = false;
            var result = await DeviceService.GetDeviceInfoAsync(snCode);
            TestConnectionButton.IsEnabled = true;



            if (result.Code == 0)
            {
                DeviceService.CurrentSNCode = snCode;
                // ✅ 从 App 获取服务实例
                // 从DI获取HomePage和ViewModel
                var homePage = App.Services.GetRequiredService<HomePage>();
                var viewModel = App.Services.GetRequiredService<HomePageViewModel>();
                // 传递 SNCode（触发属性 setter 中的加载逻辑）
                homePage.DataContext = viewModel;  // 绑定
                viewModel.SNCode = snCode;
                viewModel.Initialize(snCode);
               
                // ✅ 创建 HomePageViewModel 并注入服务
                // 获取 MainWindow 实例并切换页面
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToHomePage(snCode);
            }
            else
            {
                PromptLabel.Content = "连接失败检查设备连接状态和地址是否正确";
                PromptLabel.Foreground = Brushes.Red;
            }




        }


    }
}

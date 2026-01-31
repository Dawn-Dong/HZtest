using HZtest.Services;
using HZtest.View;
using HZtest.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HZtest
{
    /// <summary>
    /// DevConnection.xaml 的交互逻辑
    /// 现在通过构造器注入 DeviceService（由 DI 提供）
    /// </summary>
    public partial class DevConnection : Page
    {
        private readonly DeviceService _deviceService;

        public DevConnection(DeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            InitializeComponent();
        }

        /// <summary>
        /// 按钮点击事件处理程序
        /// </summary>
        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string snCode = SnTextBox.Text.Trim();
            if (string.IsNullOrEmpty(snCode))
            {
                MessageBox.Show("请输入设备SN码！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TestConnectionButton.IsEnabled = false;
            var result = await _deviceService.GetDeviceInfoAsync(snCode);
            TestConnectionButton.IsEnabled = true;

            if (result.Code == 0)
            {
                _deviceService.SetCurrentSNCode(snCode);

                // 从 DI 创建 HomePage 实例
                var homePage = App.Services.GetRequiredService<HomePage>();

                // 如果 HomePage 的 DataContext 是 HomePageViewModel 类型，则进行 SN 码的设置与初始化
                if (homePage.DataContext is HomePageViewModel viewModel)
                {
                    viewModel.SNCode = snCode;
                    viewModel.Initialize();
                }

                // 使用 MainWindow 的重载方法直接传入 homePage 实例进行导航（保证显示的是已初始化的实例）
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToHomePage(homePage, snCode);
            }
            else
            {
                PromptLabel.Content = "连接失败，检查设备连接状态和地址是否正确";
                PromptLabel.Foreground = Brushes.Red;
            }
        }
    }
}

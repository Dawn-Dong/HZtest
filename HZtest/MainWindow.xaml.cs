using HZtest.View;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // App.OnStartup 中已经将起始页设置为 DevConnection，这里不强制 new
            // MainFrame.Content = new DevConnection();
        }

        /// <summary>
        /// 在 DevConnection 验证成功后由此方法导航到 HomePage（通过 DI 创建并初始化）
        /// </summary>
        public void NavigateToHomePage(string SNCode)
        {
            // 从全局 ServiceProvider 获取由容器创建的 HomePage 与 ViewModel
            var viewModel = App.Services.GetRequiredService<ViewModels.HomePageViewModel>();
            var homePage = App.Services.GetRequiredService<HomePage>();

            // 初始化 ViewModel（如果需要传 SN 并触发加载）
            viewModel.SNCode = SNCode;
            viewModel.Initialize(SNCode);

            // 使用容器创建的页面实例进行导航
            MainFrame.Content = homePage;

            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = $"当前操作设备SN码:{SNCode}";
                StatusTextBlock.Foreground = Brushes.Orange;
            }
        }

        /// <summary>
        /// 重载：如果调用方已经创建并初始化了 HomePage 实例，直接显示它（避免再次创建新实例）
        /// </summary>
        public void NavigateToHomePage(HomePage homePage, string SNCode)
        {
            if (homePage == null)
            {
                NavigateToHomePage(SNCode);
                return;
            }

            MainFrame.Content = homePage;

            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = $"当前操作设备SN码:{SNCode}";
                StatusTextBlock.Foreground = Brushes.Orange;
            }
        }
    }
}
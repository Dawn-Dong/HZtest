using HZtest.Interfaces_接口定义;
using HZtest.View;
using HZtest.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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

namespace HZtest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            // 使用 DI 注入的 ViewModel 作为 DataContext，避免在页面中 new 未定义的服务
            DataContext = viewModel;
            _mainWindowViewModel = viewModel;

            // 页面卸载时停止监控
            this.Unloaded += (s, e) =>
            {
                (this.DataContext as FileOperationsPageViewModel)?.Cleanup();
            };
        }

        /// <summary>
        /// 在 DevConnection 验证成功后由此方法导航到 HomePage（通过 DI 创建并初始化）
        /// </summary>
        public void NavigateToHomePage(string SNCode)
        {
            // 只从容器获取 Page 实例（Page 的构造函数由 DI 注入 ViewModel）
            var homePage = App.Services.GetRequiredService<HomePage>();

            // 把 Page 显示出来
            MainFrame.Content = homePage;

            // 获取该 Page 的 DataContext（由 HomePage 构造函数设置）
            if (homePage.DataContext is ViewModels.HomePageViewModel viewModel)
            {
                viewModel.Initialize();
            }
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = $"当前操作设备SN码:{SNCode}";
                StatusTextBlock.Foreground = Brushes.Orange;
                _mainWindowViewModel.Initialize();
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
                _mainWindowViewModel.Initialize();
            }
        }


        // 点击 “文件” 按钮时在右侧 Frame 显示 FileOperationsPage
        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            // 直接 new 页面并显示；如果你通过 DI 管理页面，可以改为从 App.Services 获取实例
            var fileOperationsPage = App.Services.GetService<FileOperationsPage>();
            if (fileOperationsPage.DataContext is FileOperationsPageViewModel viewModel)
            {
                viewModel.Initialize();
            }
            MainFrame.Content = fileOperationsPage;




            // 或者： MainFrame.Navigate(new FileOperationsPage());
        }

        // 主页按钮导航
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // 如果你有 HomePage 的 DI 实例化逻辑，使用 NavigateToHomePage 或直接 new
            var homePage = App.Services.GetService<HomePage>();
            // 如果 HomePage 的 DataContext 是 HomePageViewModel 类型，则进行 SN 码的设置与初始化
            if (homePage.DataContext is HomePageViewModel viewModel)
            {
                viewModel.Initialize();
            }
            MainFrame.Content = homePage;

        }

        private void AlarmInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var alarmInfoPage = App.Services.GetService<AlarmInfoPage>();
            if (alarmInfoPage.DataContext is AlarmInfoPageViewModel viewModel)
            {

                // viewModel.Initialize();
            }
            MainFrame.Content = alarmInfoPage;
        }

        private void ToolInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var toolInfoPage = App.Services.GetService<ToolInfoPage>();
            if (toolInfoPage.DataContext is ToolInfoViewModel viewModel)
            {
                //viewModel.Initialize();
            }
            MainFrame.Content = toolInfoPage;


        }
    }
}
using HZtest.Interfaces_接口定义;
using HZtest.Resources_资源.Control.ViewModel;
using HZtest.Services.Processor;
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

        // 默认样式
        private static readonly Brush DefaultBackground = Brushes.LightGray;
        private static readonly Brush DefaultForeground = Brushes.Black;
        private static readonly FontWeight DefaultFontWeight = FontWeights.Normal;

        // 选中样式
        private static readonly Brush SelectedBackground = Brushes.DodgerBlue;
        private static readonly Brush SelectedForeground = Brushes.White;
        private static readonly FontWeight SelectedFontWeight = FontWeights.Bold;

        private Button _currentSelectedButton;
        /// <summary>
        /// 操作的设备SN码
        /// </summary>
        public string OperatingEQPSNCode = string.Empty;

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
            SelectButton(HomeButton);
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = $"当前操作设备SN码:{SNCode}";
                StatusTextBlock.Foreground = Brushes.Orange;
                OperatingEQPSNCode = SNCode ?? string.Empty;
                _mainWindowViewModel.Initialize();
            }
            // 自动创建OrderProcessor并注入依赖
            var processor = App.Services.GetRequiredService<OrderProcessor>();
        }


        // 点击 “文件” 按钮时在右侧 Frame 显示 FileOperationsPage
        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 直接 new 页面并显示；如果你通过 DI 管理页面，可以改为从 App.Services 获取实例
            var fileOperationsPage = App.Services.GetService<FileOperationsPage>();
            if (fileOperationsPage.DataContext is FileOperationsPageViewModel viewModel)
            {
                viewModel.Initialize();
            }
            MainFrame.Content = fileOperationsPage;

            SelectButton(FileButton);


            // 或者： MainFrame.Navigate(new FileOperationsPage());
        }

        // 主页按钮导航
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 如果你有 HomePage 的 DI 实例化逻辑，使用 NavigateToHomePage 或直接 new
            var homePage = App.Services.GetService<HomePage>();
            // 如果 HomePage 的 DataContext 是 HomePageViewModel 类型，则进行 SN 码的设置与初始化
            if (homePage.DataContext is HomePageViewModel viewModel)
            {
                viewModel.Initialize();
            }
            MainFrame.Content = homePage;
            SelectButton(HomeButton);

        }
        /// <summary>
        /// 点击 “报警信息” 按钮时在右侧 Frame 显示 AlarmInfoPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var alarmInfoPage = App.Services.GetService<AlarmInfoPage>();
            if (alarmInfoPage.DataContext is AlarmInfoPageViewModel viewModel)
            {

                // viewModel.Initialize();
            }
            MainFrame.Content = alarmInfoPage;
            SelectButton(AlarmInfoButton);
        }
        /// <summary>
        /// 点击 “工具信息” 按钮时在右侧 Frame 显示 ToolInfoPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void ToolInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var toolInfoPage = App.Services.GetService<ToolInfoPage>();
            if (toolInfoPage.DataContext is ToolInfoViewModel viewModel)
            {
                //viewModel.Initialize();
            }
            MainFrame.Content = toolInfoPage;
            SelectButton(ToolInfoButton);


        }
        /// <summary>
        ///  点击 “用户变量” 按钮时在右侧 Frame 显示 UserVariablesPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserVariablesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var userVariablesPage = App.Services.GetService<UserVariablesPage>();
            if (userVariablesPage.DataContext is UserVariablesViewModel viewModel)
            {
                //viewModel.Initialize();
            }
            MainFrame.Content = userVariablesPage;
            SelectButton(UserVariablesButton);
        }
        /// <summary>
        ///  点击 “相对坐标系” 按钮时在右侧 Frame 显示 RelativeCoordinateSystemPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RelativeCoordinateSystemButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var relativeCoordinateSystemPage = App.Services.GetService<RelativeCoordinateSystemPage>();
            if (relativeCoordinateSystemPage.DataContext is RelativeCoordinateSystemViewModel viewModel)
            {
                //viewModel.Initialize();
            }
            MainFrame.Content = relativeCoordinateSystemPage;
            SelectButton(RelativeCoordinateSystemButton);

        }
        /// <summary>
        ///  点击 “绝对坐标系” 按钮时在右侧 Frame 显示 RegisterOperationPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var RegisterOperaionPage = App.Services.GetService<RegisterOperationPage>();
            if (RegisterOperaionPage.DataContext is RegisterOperationViewModel viewModel)
            {
                //viewModel.Initialize();
            }
            MainFrame.Content = RegisterOperaionPage;
            SelectButton(RegisterOperationButton);
        }

        /// <summary>
        ///  点击 “订单管理” 按钮时在右侧 Frame 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderManagementButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OperatingEQPSNCode))
            {
                MessageBox.Show("请先确保设备链接成功在操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var OrderManagementPage = App.Services.GetService<OrderManagementPage>();
            if (OrderManagementPage.DataContext is OrderManagementPageViewModel viewModel)
            {
                //viewModel.Initialize();
            }
            MainFrame.Content = OrderManagementPage;
            SelectButton(OrderManagementButton);
        }


        /// <summary>
        /// 将对应的按钮设置选中样式；同时重置之前选中按钮的样式
        /// </summary>
        /// <param name="button">按钮</param>
        private void SelectButton(Button button)
        {
            // 重置之前按钮
            if (_currentSelectedButton != null)
                SetButtonStyle(_currentSelectedButton, false);

            // 设置新按钮
            SetButtonStyle(button, true);
            _currentSelectedButton = button;
        }

        private void SetButtonStyle(Button button, bool isSelected)
        {
            button.Background = isSelected ? SelectedBackground : DefaultBackground;
            button.Foreground = isSelected ? SelectedForeground : DefaultForeground;
            button.FontWeight = isSelected ? SelectedFontWeight : DefaultFontWeight;

        }


    }
}
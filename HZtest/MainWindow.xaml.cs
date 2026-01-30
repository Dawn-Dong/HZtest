using HZtest.View;
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
        public MainWindow()
        {
            InitializeComponent();
            // 启动时加载 DevConnection 页面
            MainFrame.Content = new DevConnection();
        }

        
        /// <summary>
        ///在 DevConnection 验证成功后调用此方法
        /// </summary>
        /// <param name="SNCode"></param>
        public void NavigateToHomePage(string SNCode)
        {
            MainFrame.Content = new HomePage();
            StatusTextBlock.Text = $"当前操作设备SN码:{SNCode}";
            StatusTextBlock.Foreground = Brushes.Orange;

        }


    }
}
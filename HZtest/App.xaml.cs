using HZtest.Interfaces_接口定义;
using HZtest.Services;
using HZtest.View;
using HZtest.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;


namespace HZtest
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 1. 配置
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // 2. 注册服务
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            // 3. 启动主窗口
            var mainWindow = Services.GetRequiredService<MainWindow>();
            Current.MainWindow = mainWindow;

            // 4. 延迟初始化需要UI元素的服务
            mainWindow.Loaded += (s, args) =>
            {
                InitializeDialogService(mainWindow);

                // 导航到起始页
                if (mainWindow.MainFrame != null)
                {
                    mainWindow.MainFrame.Content = Services.GetRequiredService<DevConnection>();
                }
            };

            mainWindow.Show();
            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 注册配置  单例模式 注入    
            // 构造函数注入，自动获得那个唯一的 Configuration 实例
            services.AddSingleton(Configuration);

            // 注册服务（根据你的实际代码调整）
            services.AddSingleton<IMessageService, MessageService>();

            // DialogService 延迟初始化（不在这里传RootGrid）
            services.AddSingleton<IDialogService, DialogService>();



            // ViewModels - 不传递SN码参数，通过属性设置  
            //每次请求 Services.GetRequiredService<HomePageViewModel>(); 都创建一个全新的实例。
            services.AddTransient<HomePageViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<DevConnection>();
            services.AddTransient<HomePage>();
            services.AddTransient<FileOperationsPage>();
        }

        /// <summary>
        /// 初始化 DialogService对话服务并确保其拥有对 RootGrid 的name
        /// </summary>
        /// <param name="mainWindow"></param>
        private void InitializeDialogService(MainWindow mainWindow)
        {
            var rootGrid = mainWindow.FindName("RootGrid") as Grid;
            if (rootGrid == null)
            {
                MessageBox.Show("MainWindow.xaml 必须包含 Name=\"RootGrid\" 的 Grid");
                return;
            }

            try
            {
                var dialogService = Services.GetRequiredService<IDialogService>();
                dialogService.Initialize(rootGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法初始化对话服务：{ex.Message}");
            }
        }
    }
}
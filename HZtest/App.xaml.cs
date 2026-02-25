using HZtest.Interfaces_接口定义;
using HZtest.Services;
using HZtest.Universal;
using HZtest.View;
using HZtest.View.Dialogs;
using HZtest.ViewModels;
using HZtest.ViewModels.Dialogs;
using HZtest.Views.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
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
        /// <summary>
        /// 注册服务 - 请在此处注册你的服务 
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureServices(IServiceCollection services)
        {
            // 注册配置  单例模式 注入    
            services.AddSingleton(Configuration);
            
            // 日志
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // 注册服务（根据你的实际代码调整）
            services.AddSingleton<IMessageService, MessageService>();

            // DialogService 延迟初始化（不在这里传RootGrid）
            services.AddSingleton<IDialogService, DialogService>();
            // MainWindow 


            // ViewModels - 不传递SN码参数，通过属性设置  
            services.AddTransient<HomePageViewModel>();
            services.AddTransient<FileOperationsPageViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<AlarmInfoPageViewModel>();
            services.AddTransient<ToolInfoViewModel>();
            services.AddTransient<UserVariablesViewModel>();


            // ViewModels Dialogs的 
            services.AddTransient<UploadFileViewModel>();
            services.AddTransient<ModeSelectionViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<DevConnection>();
            services.AddTransient<HomePage>();
            services.AddTransient<FileOperationsPage>();
            services.AddTransient<AlarmInfoPage>();
            services.AddTransient<ToolInfoPage>();
            services.AddTransient<UserVariablesPage>();

            // VIews Dialogs的
            services.AddTransient<ModeSelectionDialog>();
            services.AddTransient<UploadFileDialogs>();


            // ApiClient: 使用 Typed Client，通过配置设置 BaseAddress；SetHandlerLifetime 控制 handler 重用周期
            var apiSettings = Configuration.GetSection("ApiSettings").Get<AppSettings>() ?? new AppSettings();
            services.AddHttpClient<ApiClient>(client =>
            {
                if (!string.IsNullOrEmpty(apiSettings.BaseUrl))
                {
                    client.BaseAddress = new Uri(apiSettings.BaseUrl);
                }
                client.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // 关键：自动解压，否则读到的是压缩字节（可能显示为 % 开头）
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // DeviceService：保持单例以保存 SN 状态（可根据需要改为 transient）
            services.AddSingleton<DeviceService>();
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
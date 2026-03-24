using HZtest.Interfaces_接口定义;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace HZtest.Infrastructure_基础设施
{
    /// <summary>
    /// 三层结构化日志封装类
    /// 
    /// 设计目标：
    /// 1. 单参数：_logger.Debug("测试") 
    ///    → logs/debug/debug-20260323.log
    ///    
    /// 2. 双参数：_logger.Debug("api", "测试")
    ///    → logs/debug/api-20260323.log
    ///    
    /// 3. 三参数：_logger.Debug("接口调用日志", "api", "测试")
    ///    → logs/接口调用日志/api-20260323.log
    /// </summary>
    /// <summary>
    /// 结构化日志实现 - 无歧义重载版本
    /// </summary>
    public class StructuredLogger : IStructuredLogger
    {
        private readonly Logger _logger;

        public StructuredLogger(string name = null)
        {
            EnsureNLogConfig();
            _logger = name == null
                ? LogManager.GetCurrentClassLogger()
                : LogManager.GetLogger(name);

            // 调试用：检查配置是否加载
            System.Diagnostics.Debug.WriteLine($"Logger: {_logger.Name}");
            System.Diagnostics.Debug.WriteLine($"Targets: {LogManager.Configuration?.AllTargets?.Count ?? 0}");
        }
        /// <summary>
        /// 手动添加 NLog 配置加载逻辑，确保在任何环境下都能正确加载 nlog.json 配置文件
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void EnsureNLogConfig()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var configPath = Path.Combine(baseDir, "nlog.json");

                System.Diagnostics.Debug.WriteLine($"=== 极简测试 ===");
                System.Diagnostics.Debug.WriteLine($"路径: {configPath}");
                System.Diagnostics.Debug.WriteLine($"存在: {File.Exists(configPath)}");


                if (LogManager.Configuration == null)
                {
                    if (File.Exists(configPath))
                    {
                        try
                        {
                            // 使用 NLog.Extensions.Logging 加载 JSON
                            var microsoftConfig = new ConfigurationBuilder()
                                .SetBasePath(baseDir)
                                .AddJsonFile("nlog.json", optional: false, reloadOnChange: true)
                                .Build();

                            var nlogConfig = new NLogLoggingConfiguration(microsoftConfig.GetSection("NLog"));
                            LogManager.Configuration = nlogConfig;

                            System.Diagnostics.Debug.WriteLine("JSON 配置加载成功");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"加载失败: {ex.Message}");

                        }
                    }
                }

                LogManager.Flush();
            }
            catch (Exception ex)
            {

                throw new Exception($"{ex.Message}");
            }

        }

        // ========== Debug ==========

        /// <summary>
        /// 单参数：_logger.Debug("测试");
        /// </summary>
        public void Debug(string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Debug, _logger.Name, message);
            _logger.Log(logEvent);
        }

        /// <summary>
        /// 双参数：_logger.Debug("点击", "点击了测试连接001");
        /// </summary>
        public void Debug(string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Debug, _logger.Name, message);
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        /// <summary>
        /// 三参数：_logger.Debug("测试文件", "点击", "点击了测试连接001");
        /// </summary>
        public void Debug(string folder, string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Debug, _logger.Name, message);
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        // ========== Info ==========

        /// <summary>
        /// 单参数：_logger.Info("测试");
        /// </summary>
        public void Info(string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);
            _logger.Log(logEvent);
        }

        /// <summary>
        /// 双参数：_logger.Info("点击", "点击了测试连接001");
        /// </summary>
        public void Info(string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        /// <summary>
        /// 三参数：_logger.Info("测试文件", "点击", "点击了测试连接001");
        /// </summary>
        public void Info(string folder, string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        // ========== Error ==========

        public void Error(string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
            _logger.Log(logEvent);
        }

        public void Error(string message, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
            logEvent.Exception = ex;
            _logger.Log(logEvent);
        }

        public void Error(string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Error(string subCategory, string message, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
            logEvent.Exception = ex;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Error(string folder, string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Error(string folder, string subCategory, string message, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _logger.Name, message);
            logEvent.Exception = ex;
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        // ========== Warn ==========

        public void Warn(string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Warn, _logger.Name, message);
            _logger.Log(logEvent);
        }

        public void Warn(string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Warn, _logger.Name, message);
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Warn(string folder, string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Warn, _logger.Name, message);
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        // ========== Fatal ==========

        public void Fatal(string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, message);
            _logger.Log(logEvent);
        }

        public void Fatal(string message, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, message);
            logEvent.Exception = ex;
            _logger.Log(logEvent);
        }

        public void Fatal(string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, message);
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Fatal(string subCategory, string message, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, message);
            logEvent.Exception = ex;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Fatal(string folder, string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, message);
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Fatal(string folder, string subCategory, string message, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Fatal, _logger.Name, message);
            logEvent.Exception = ex;
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        // ========== Trace ==========

        public void Trace(string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Trace, _logger.Name, message);
            _logger.Log(logEvent);
        }

        public void Trace(string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Trace, _logger.Name, message);
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }

        public void Trace(string folder, string subCategory, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Trace, _logger.Name, message);
            logEvent.Properties["Folder"] = folder;
            logEvent.Properties["SubCategory"] = subCategory;
            _logger.Log(logEvent);
        }
    }
}

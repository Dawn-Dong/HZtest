using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.DB;
using HZtest.Models.Response;
using HZtest.Services;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace HZtest.ViewModels
{
    /// <summary>
    /// 主窗口的视图模型
    /// </summary>
    public class MainWindowViewModel : PageViewModelBaseClass
    {
        // ===== 依赖服务（构造函数注入）=====
        private readonly DeviceService _deviceService;
        private readonly SqlSugarClient _Db;
        private readonly IStructuredLogger _logger;

        // 定义颜色转换帮助方法
        private static Brush HexToBrush(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        private List<DeviceAlarmInforResponse> _deviceAlarmInforOldList = new List<DeviceAlarmInforResponse>();

        // 取消令牌（用于停止监控）
        private CancellationTokenSource _cts;

        // ===== UI属性 =====
        // 用于绑定到 TextBlock.Inlines 的集合
        public ObservableCollection<Inline> AlertInlines { get; } = new();
        // 可选：保留纯文本版本用于其他用途
        private string _alarmMessageText;
        public string AlarmMessageText
        {
            get => _alarmMessageText;
            set { _alarmMessageText = value; OnPropertyChanged(); }
        }

        private bool _isMarqueeEnabled = true;
        public bool IsMarqueeEnabled
        {
            get => _isMarqueeEnabled;
            set { _isMarqueeEnabled = value; OnPropertyChanged(); }
        }

        public MainWindowViewModel(DeviceService deviceService, SqlSugarClient db, IStructuredLogger logger)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 启动数据监控
        /// </summary>
        public void Initialize()
        {
            // 1. 启动数据监控
            StartDataMonitoring();
        }

        /// <summary>
        /// 信息监控循环
        /// </summary>
        private async void StartDataMonitoring()
        {
            _cts = new CancellationTokenSource();

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {

                    await GetDeviceAlarmInfor();

                    // 3. 等待（避免CPU占用过高）
                    await Task.Delay(500, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，忽略
            }
            catch (Exception ex)
            {
                // 记录日志（在ViewModel中不应弹出MessageBox）
                System.Diagnostics.Debug.WriteLine($"监控错误: {ex.Message}");
                _logger.Error("MainWindowViewModel", $"监控错误: {ex.Message}", ex);

            }
        }
        /// <summary>
        /// 获取设备报警信息
        /// </summary>
        /// <returns></returns>
        private async Task GetDeviceAlarmInfor()
        {
            try
            {
                var deviceAlarmInfor = await _deviceService.GetDeviceAlarmInforAsync();
                if (deviceAlarmInfor.Code != 0 || deviceAlarmInfor.Value.Count == 0)
                {
                    AlertInlines.Clear();
                    return;
                }


                //var vd = ReadFromCsv("D:\\Data/AlarmInfo.csv");

                var alarmCompareResult = CompareAlarms(_deviceAlarmInforOldList, deviceAlarmInfor.Value);
                if (!alarmCompareResult.HasChanged)
                    return;

                var AlarmManagementConfigList = await _Db.QueryableWithAttr<AlarmManagementConfigModel>().ToListAsync();
                var displayItems = new List<AlarmDisplayItem>();
                int totalCount = deviceAlarmInfor.Value.Count;
               // StringBuilder loopDisplay = new StringBuilder();
                //组织报警信息显示文本
                for (int i = 0; i < deviceAlarmInfor.Value.Count; i++)
                {
                    var alarm = deviceAlarmInfor.Value[i];
                    var (alarmType, alarmLevel, isAutoParsing) = await AnalyzeAlarmCodeAsync(alarm.Number, AlarmManagementConfigList);
                    // 添加到显示
                    displayItems.Add(new AlarmDisplayItem
                    {
                        Text = $"({i + 1}/{totalCount}) {alarm.Text}",
                        Level = alarmLevel,
                        IsAutoParsing = isAutoParsing
                    });
                   // loopDisplay.Append($"({i + 1}/{totalCount}) {deviceAlarmInfor.Value[i].Text}      ");

                }
                //AlarmMessageText = loopDisplay.ToString();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 挂起 dispatcher 处理，批量更新完成后再恢复
                    using (Dispatcher.CurrentDispatcher.DisableProcessing())
                    {
                        AlertInlines.Clear();
                        foreach (var item in displayItems)
                        {
                            // 添加报警文本
                            AlertInlines.Add(new Run(item.Text)
                            {
                                Foreground = new SolidColorBrush(item.LevelColor),
                                FontSize = 14,
                                FontWeight = FontWeights.Bold,
                                BaselineAlignment = BaselineAlignment.Center
                            });

                            AlertInlines.Add(new Run(item.SourceTag)
                            {
                                Foreground = HexToBrush(item.SourceColor),
                                FontSize = 10,
                                BaselineAlignment = BaselineAlignment.Center
                            });

                            // 添加分隔符
                            AlertInlines.Add(new Run("    ")
                            {
                                Foreground = Brushes.Gray,
                                FontSize = 14
                            });
                        }
                    }
                });


                var alarmInfos = new List<AlarmInfoModels>();
                var alarmTime = DateTime.Now;
                foreach (var alarm in alarmCompareResult.Added)
                {

                    var (alarmType, alarmLevel, isAutoPasing) = await AnalyzeAlarmCodeAsync(alarm.Number, AlarmManagementConfigList);
                    alarmInfos.Add(new AlarmInfoModels
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        AlarmCode = alarm.Number,
                        AlarmType = alarmType,
                        AlarmLevel = alarmLevel,
                        AlarmTime = alarmTime,
                        AlarmContent = alarm.Text,
                        IsAutoPasing = isAutoPasing,
                        CreateTime = alarmTime
                    });
                }


                var result = await _Db.InsertableWithAttr(alarmInfos).ExecuteCommandAsync();

                // 保存到数据库
                if (result > 0)
                {
                    _deviceAlarmInforOldList = deviceAlarmInfor.Value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取报警信息错误: {ex.Message}");
                _logger.Error("MainWindowViewModel", $"获取报警信息错误: {ex.Message}", ex);
            }

            //if (AppendToCsv(alarmInfos, "D:\\Data/AlarmInfo.csv"))
            //{
            //    _deviceAlarmInforOldList = deviceAlarmInfor.Value;
            //}

        }
        /// <summary>
        /// 报警级别解析分为自动根据华中给的规则解析和根据配置的规则解析
        /// </summary>
        /// <param name="alarmCode"></param>
        /// <returns></returns>
        private async Task<(AlarmTypeEnum alarmType, AlarmLevelEnum alarmLevel, bool isAutoPasing)> AnalyzeAlarmCodeAsync(string alarmCode, List<AlarmManagementConfigModel> alarmManagementConfigList)
        {

            var alarmConfig = alarmManagementConfigList.Where(a => a.AlarmCode == alarmCode).FirstOrDefault();

            var alarmLevel = AlarmLevelEnum.Error;

            char codeType = alarmCode[0]; // 获取报警代码的第一个字符 AlarmTypeEnum
            var alarmType = (AlarmTypeEnum)int.Parse(codeType.ToString());
            await Task.Delay(100); // 模拟异步操作
            if (alarmConfig != null)
            {
                alarmLevel = alarmConfig.AlarmLevel;
                return (alarmType, alarmLevel, true);
            }
            else
            {
                // 如果没有配置，则根据华中给的规则解析
                if (alarmCode.Length >= 2)
                {

                    char codeLevel = alarmCode[1]; // 获取报警代码的第二个字符 AlarmLevelEnum

                    alarmLevel = (AlarmLevelEnum)int.Parse(codeLevel.ToString());
                    return (alarmType, alarmLevel, false);
                }
                else
                {
                    // 无法解析，返回默认值
                    return (AlarmTypeEnum.Error, AlarmLevelEnum.Error, false);
                }
            }

        }
        #region 废弃改为用SQLite数据库存储报警信息
        /// <summary>
        /// 数据保存为CSV文件
        /// </summary>
        /// <param name="records">保存的值</param>
        /// <param name="filePath">保存的文件路径</param>
        public bool AppendToCsv(List<AlarmInfoModels> records, string filePath)
        {
            try
            {

                if (records == null || records.Count == 0)
                    return false;
                // 创建目录
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                bool fileExists = File.Exists(filePath);

                using var writer = new StreamWriter(filePath, append: true, Encoding.UTF8);


                if (!fileExists)
                {

                    writer.WriteLine("AlarmCode,AlarmType,AlarmLevel,AlarmTime,AlarmContent"); // 写入表头
                }

                foreach (var r in records)
                {
                    writer.WriteLine($"{r.AlarmCode},{r.AlarmType.GetHashCode()},{r.AlarmLevel.GetHashCode()},{r.AlarmTime:yyyy-MM-dd HH:mm:ss},{EscapeCsv(r.AlarmContent)}");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // 包含逗号、引号、换行，需要引号包裹
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                // 内部引号转义： " → ""
                value = value.Replace("\"", "\"\"");
                value = $"\"{value}\"";
            }

            return value;
        }

        /// <summary>
        /// 读取CSV文件并转换为AlarmInfoModels列表
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns></returns>
        public List<AlarmInfoModels> ReadFromCsv(string filePath)
        {
            var records = new List<AlarmInfoModels>();

            if (!File.Exists(filePath))
                return records;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            for (int i = 1; i < lines.Length; i++)  // 跳过表头
            {
                var parts = ParseCsvLine(lines[i]);
                if (parts.Length >= 5)
                {
                    records.Add(new AlarmInfoModels
                    {
                        AlarmCode = parts[0],
                        AlarmType = (AlarmTypeEnum)int.Parse(parts[1]),
                        AlarmLevel = (AlarmLevelEnum)int.Parse(parts[2]),
                        AlarmTime = DateTime.Parse(parts[3]),
                        AlarmContent = parts[4]
                    });
                }
            }

            return records;
        }

        /// <summary>
        /// 解析CSV行（处理引号包裹的情况）
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 转义的引号 ""
                        current.Append('"');
                        i++; // 跳过下一个
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        #endregion

        /// <summary>
        /// 对比两组报警数据
        /// </summary>
        /// <param name="oldAlarms">老报警信息</param>
        /// <param name="newAlarms">新报警信息</param>
        /// <returns></returns>
        public AlarmCompareResult CompareAlarms(
            List<DeviceAlarmInforResponse> oldAlarms,
            List<DeviceAlarmInforResponse> newAlarms)
        {
            var result = new AlarmCompareResult();

            // 用字典加速查找（以Number为Key）
            var oldDict = oldAlarms?.ToDictionary(a => a.Number, a => a.Text)
                          ?? new Dictionary<string, string>();

            var newDict = newAlarms?.ToDictionary(a => a.Number, a => a.Text)
                          ?? new Dictionary<string, string>();

            // 1. 找出新增的（新中有，旧中没有）
            foreach (var newItem in newAlarms ?? new List<DeviceAlarmInforResponse>())
            {
                if (!oldDict.ContainsKey(newItem.Number))
                {
                    result.Added.Add(newItem);
                }
                else if (oldDict[newItem.Number] != newItem.Text)
                {
                    // Number相同但Text不同，视为变化（先移除旧，再添加新）
                    result.Removed.Add(new DeviceAlarmInforResponse
                    {
                        Number = newItem.Number,
                        Text = oldDict[newItem.Number]
                    });
                    result.Added.Add(newItem);
                }
                else
                {
                    // 完全相同
                    result.Unchanged.Add(newItem);
                }
            }

            // 2. 找出移除的（旧中有，新中没有）
            foreach (var oldItem in oldAlarms ?? new List<DeviceAlarmInforResponse>())
            {
                if (!newDict.ContainsKey(oldItem.Number))
                {
                    result.Removed.Add(oldItem);
                }
            }

            // 3. 判断是否有变化
            result.HasChanged = result.Added.Count > 0 || result.Removed.Count > 0;

            return result;
        }





        // 清理方法（页面关闭时调用）
        public void Cleanup()
        {
            _cts?.Cancel(); // 停止循环
        }

    }
}

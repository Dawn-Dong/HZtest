using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    public class FileOperationsPageViewModel : PageViewModelBaseClass
    {
        // 添加一个 Action 委托供 View 注册
        public Action<string> FileDetailsChanged { get; set; }
        // ===== 依赖服务（构造函数注入）=====
        private readonly DeviceService _deviceService;


        // 取消令牌（用于停止监控）
        private CancellationTokenSource _cts;


        // 命令（业务逻辑）
        public ICommand FileDetailsCommand { get; }


        private string _currentRunningFile = string.Empty;
        /// <summary>
        /// 当前正在运行的文件
        /// </summary>
        public string CurrentRunningFile
        {
            get => _currentRunningFile;
            set { _currentRunningFile = value; OnPropertyChanged(); }
        }


        private string _currentRunningFileDetails = string.Empty;
        /// <summary>
        /// 当前正在运行的文件详情
        /// </summary>
        public string CurrentRunningFileDetails
        {
            get => _currentRunningFileDetails;
            set {
                _currentRunningFileDetails = value; OnPropertyChanged();
                // 触发回调
                FileDetailsChanged?.Invoke(value);
            }
        }



        public FileOperationsPageViewModel(DeviceService deviceService)
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            // 绑定命令
            FileDetailsCommand = new RelayCommand(async () => await ExecuteReadFileDetailsAsync());

        }

        /// <summary>
        /// 读取文件详情命令执行方法
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteReadFileDetailsAsync()
        {
            // 读取文件详情
            var response = await _deviceService.GetTheCurrentRunningDetailsFileAsync(CurrentRunningFile);
            //CurrentRunningFileDetails = response.Value.RunningDetailsFile ?? "加载为空或者错误了";
            CurrentRunningFileDetails = "这是 G-code（数控编程语言），显然不是 JSON 格式。报错是因为 System.Text.Json 尝试把 %1234 解析为 JSON，但 JSON 不允许以 % 开头。\r\n你需要根据返回类型做分支处理：如果是 string 类型就直接返回文本，否则尝试 JSON 反序列化。\r\ncsharp\r\n复制\r\npublic async Task<T?> SendAsync<T>(\r\n    HttpMethod method, \r\n    string path, \r\n    object? body = null, \r\n    CancellationToken cancellationToken = default)\r\n{\r\n    using var request = new HttpRequestMessage(method, path);\r\n\r\n    if (body != null)\r\n    {\r\n        var json = JsonSerializer.Serialize(body, _jsonOptions);\r\n        request.Content = new StringContent(json, Encoding.UTF8, \"application/json\");\r\n    }\r\n\r\n    try\r\n    {\r\n        var response = await _http.SendAsync(\r\n            request, \r\n            HttpCompletionOption.ResponseHeadersRead, \r\n            cancellationToken).ConfigureAwait(false);\r\n        \r\n        _logger.LogDebug(\r\n            \"Content-Type: {Type}, Encoding: {Encoding}\",\r\n            response.Content.Headers.ContentType?.MediaType,\r\n            string.Join(\",\", response.Content.Headers.ContentEncoding));\r\n        \r\n        response.EnsureSuccessStatusCode();\r\n\r\n        // 获取原始流\r\n        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);\r\n        Stream? decompressedStream = null;\r\n        \r\n        try\r\n        {\r\n            var finalStream = stream;\r\n            \r\n            if (response.Content.Headers.ContentEncoding.Contains(\"gzip\"))\r\n            {\r\n                finalStream = decompressedStream = new GZipStream(\r\n                    stream, CompressionMode.Decompress, leaveOpen: false);\r\n            }\r\n            else if (response.Content.Headers.ContentEncoding.Contains(\"deflate\"))\r\n            {\r\n                finalStream = decompressedStream = new DeflateStream(\r\n                    stream, CompressionMode.Decompress, leaveOpen: false);\r\n            }\r\n\r\n            // 🔑 关键分支：如果是 string 类型，直接读取文本（支持 G-code、URL 编码等）\r\n            if (typeof(T) == typeof(string))\r\n            {\r\n                using var reader = new StreamReader(finalStream, Encoding.UTF8);\r\n                var content = await reader.ReadToEndAsync(cancellationToken);\r\n                \r\n                // 处理 URL 编码（如果 API 返回 %7B%22... 这种）\r\n                if (!string.IsNullOrEmpty(content) && content[0] == '%')\r\n                {\r\n                    // 检查是否整个内容都是 URL 编码（而不是像 G-code 这种只有开头有 %）\r\n                    // G-code 的 % 后面一般是数字或空行，URL 编码的 % 后面是十六进制\r\n                    if (LooksLikeUrlEncoded(content))\r\n                    {\r\n                        _logger.LogWarning(\"检测到 URL 编码响应，正在解码\");\r\n                        content = Uri.UnescapeDataString(content);\r\n                    }\r\n                    // 否则保持原样（G-code 的 % 是合法的）\r\n                }\r\n                \r\n                return (T?)(object?)content;\r\n            }\r\n            \r\n            // 其他类型：走 JSON 反序列化（流式）\r\n            return await JsonSerializer.DeserializeAsync<T>(\r\n                finalStream, _jsonOptions, cancellationToken);\r\n        }\r\n        finally\r\n        {\r\n            if (decompressedStream != null)\r\n                await decompressedStream.DisposeAsync();\r\n            else\r\n                await stream.DisposeAsync();\r\n        }\r\n    }\r\n    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)\r\n    {\r\n        _logger.LogInformation(\"请求被取消: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n    catch (HttpRequestException ex)\r\n    {\r\n        _logger.LogError(ex, \"HTTP 请求失败: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n    catch (JsonException ex)\r\n    {\r\n        _logger.LogError(ex, \"JSON 反序列化失败: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n    catch (Exception ex)\r\n    {\r\n        _logger.LogError(ex, \"请求处理异常: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n}\r\n\r\n//  helper：判断是否是 URL 编码（简单启发式）\r\nprivate static bool LooksLikeUrlEncoded(string content)\r\n{\r\n    // 如果内容里包含大量 %XX 模式（X 是十六进制字符），则认为是 URL 编码\r\n    // G-code 的 % 后面通常是数字（如 %1234），不是十六进制对\r\n    if (content.Length < 3) return false;\r\n    \r\n    // 检查前几个 % 后面是否跟着十六进制字符\r\n    int hexCount = 0;\r\n    for (int i = 0; i < content.Length - 2; i++)\r\n    {\r\n        if (content[i] == '%')\r\n        {\r\n            if (char.IsAsciiHexDigit(content[i + 1]) && char.IsAsciiHexDigit(content[i + 2]))\r\n            {\r\n                hexCount++;\r\n                if (hexCount >= 3) return true; // 有 3 个连续的 %XX，大概率是 URL 编码\r\n            }\r\n            else\r\n            {\r\n                // 遇到 % 后面不是十六进制，可能是 G-code 的 %\r\n                return false;\r\n            }\r\n        }\r\n    }\r\n    return false;\r\n}\r\n调用示例：\r\ncsharp\r\n复制\r\n// 获取 G-code 文本（直接返回字符串，不经过 JSON 解析）\r\nvar gcode = await client.SendAsync<string>(HttpMethod.Get, \"/api/program/123\");\r\n\r\n// 获取 JSON 对象（正常反序列化）\r\nvar result = await client.SendAsync<MyDto>(HttpMethod.Get, \"/api/data\");\r\n关键点：\r\ntypeof(T) == typeof(string) 时直接读文本，保留 G-code 的 % 前缀\r\nLooksLikeUrlEncoded 辅助方法区分 G-code（%1234）和 URL 编码（%7B%22），避免误伤\r\n非字符串类型才走 JSON 反序列化逻辑";
        }


        /// <summary>
        /// 启动数据监控
        /// </summary>
        public void Initialize()
        {
            // 启动监控（确保 SNCode 已设置）
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

                    await GetTheCurrentRunningFile();

                    // 3. 等待（避免CPU占用过高）
                    await Task.Delay(100, _cts.Token);
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
            }
        }

        /// <summary>
        /// 获取当前运行文件
        /// </summary>
        /// <returns></returns>
        private async Task GetTheCurrentRunningFile()
        {
            var fileOperationsModel = await _deviceService.GetTheCurrentRunningFileAsync();
            CurrentRunningFile = fileOperationsModel.Value.RunningFile ?? "加载错误";
        }



        // 清理方法（页面关闭时调用）
        public void Cleanup()
        {
            _cts?.Cancel(); // 停止循环
        }


    }
}

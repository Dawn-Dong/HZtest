using HZtest.Infrastructure_基础设施;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HZtest.ViewModels
{
    public class FileOperationsPageViewModel : PageViewModelBaseClass
    {
        // 添加一个 Action 委托供 View 注册
        public Action<string> FileDetailsChanged { get; set; }
        // ===== 依赖服务（构造函数注入）=====
        private readonly IDialogService _dialogService;
        private readonly IMessageService _message_service;
        private readonly DeviceService _deviceService;


        // 取消令牌（用于停止监控）
        private CancellationTokenSource _cts;


        // 命令（业务逻辑） 和XAML页面的按钮绑定
        /// <summary>
        /// 显示运行文件详情命令
        /// </summary>
        public ICommand FileDetailsCommand { get; }
        /// <summary>
        /// 获取目录文件列表并构建树节点
        /// </summary>
        public ICommand DirectoryFileListCommand { get; }
        /// <summary>
        /// 切换运行文件命令
        /// </summary>
        public ICommand SwitchRunningFileCommand { get; }
        /// <summary>
        /// 上传文件命令
        /// </summary>
        public ICommand UploadFileCommand { get; }
        /// <summary>
        /// 删除文件命令
        /// </summary>

        public ICommand DeleteFileCommand { get; }


        #region 树节点用
        public ObservableCollection<FileNode> TreeItems { get; } = new();

        private FileNode _selectedNode;
        public FileNode SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                OnPropertyChanged();

                if (value != null)
                {
                    // 在输出窗口查看（调试 -> 窗口 -> 输出）
                    Debug.WriteLine($"选中了：{value.Name}");
                    Debug.WriteLine($"是文件夹吗：{value.IsDirectory}");
                    Debug.WriteLine($"完整路径：{value.FullPath}");
                    Debug.WriteLine($"是否展开：{value.IsExpanded}");
                    Debug.WriteLine($"是否选中：{value.IsSelected}");
                }
            }
        }

        public event Action<string> FileSelected;

        #endregion


        #region 页面绑定属性
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
            set
            {
                _currentRunningFileDetails = value; OnPropertyChanged();
                // 触发回调
                FileDetailsChanged?.Invoke(value);
            }
        }

        private string _localFilePath = string.Empty;

        /// <summary>
        /// 要上传的文件路径
        /// </summary>
        public string LocalFilePath
        {
            get => _localFilePath;
            set { _localFilePath = value; OnPropertyChanged(); }
        }
        #endregion

        public FileOperationsPageViewModel(IDialogService dialogService, IMessageService messageService, DeviceService deviceService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message_service = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            // XAML页面的按钮绑定
            FileDetailsCommand = new RelayCommand(async () => await ExecuteReadFileDetailsAsync());
            SwitchRunningFileCommand = new RelayCommand(async () => await ExecuteSwitchRunningFileAsync());
            DirectoryFileListCommand = new RelayCommand(GetDirectoryFileList);
            DeleteFileCommand = new AsyncRelayCommand(async () => await ExecuteDeleteFileAsync());

            //弹窗绑定
            UploadFileCommand = new AsyncRelayCommand(OpenUploadFileDialogs);

        }

        #region 命令实现
        /// <summary>
        /// 读取文件详情命令执行方法
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteReadFileDetailsAsync()
        {
            // 读取文件详情
            var response = await _deviceService.GetTheCurrentRunningDetailsFileAsync(CurrentRunningFile);
            CurrentRunningFileDetails = response.Value.RunningDetailsFile ?? "加载为空或者错误了";
            // CurrentRunningFileDetails = "这是 G-code（数控编程语言），显然不是 JSON 格式。报错是因为 System.Text.Json 尝试把 %1234 解析为 JSON，但 JSON 不允许以 % 开头。\r\n你需要根据返回类型做分支处理：如果是 string 类型就直接返回文本，否则尝试 JSON 反序列化。\r\ncsharp\r\n复制\r\npublic async Task<T?> SendAsync<T>(\r\n    HttpMethod method, \r\n    string path, \r\n    object? body = null, \r\n    CancellationToken cancellationToken = default)\r\n{\r\n    using var request = new HttpRequestMessage(method, path);\r\n\r\n    if (body != null)\r\n    {\r\n        var json = JsonSerializer.Serialize(body, _jsonOptions);\r\n        request.Content = new StringContent(json, Encoding.UTF8, \"application/json\");\r\n    }\r\n\r\n    try\r\n    {\r\n        var response = await _http.SendAsync(\r\n            request, \r\n            HttpCompletionOption.ResponseHeadersRead, \r\n            cancellationToken).ConfigureAwait(false);\r\n        \r\n        _logger.LogDebug(\r\n            \"Content-Type: {Type}, Encoding: {Encoding}\",\r\n            response.Content.Headers.ContentType?.MediaType,\r\n            string.Join(\",\", response.Content.Headers.ContentEncoding));\r\n        \r\n        response.EnsureSuccessStatusCode();\r\n\r\n        // 获取原始流\r\n        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);\r\n        Stream? decompressedStream = null;\r\n        \r\n        try\r\n        {\r\n            var finalStream = stream;\r\n            \r\n            if (response.Content.Headers.ContentEncoding.Contains(\"gzip\"))\r\n            {\r\n                finalStream = decompressedStream = new GZipStream(\r\n                    stream, CompressionMode.Decompress, leaveOpen: false);\r\n            }\r\n            else if (response.Content.Headers.ContentEncoding.Contains(\"deflate\"))\r\n            {\r\n                finalStream = decompressedStream = new DeflateStream(\r\n                    stream, CompressionMode.Decompress, leaveOpen: false);\r\n            }\r\n\r\n            // 🔑 关键分支：如果是 string 类型，直接读取文本（支持 G-code、URL 编码等）\r\n            if (typeof(T) == typeof(string))\r\n            {\r\n                using var reader = new StreamReader(finalStream, Encoding.UTF8);\r\n                var content = await reader.ReadToEndAsync(cancellationToken);\r\n                \r\n                // 处理 URL 编码（如果 API 返回 %7B%22... 这种）\r\n                if (!string.IsNullOrEmpty(content) && content[0] == '%')\r\n                {\r\n                    // 检查是否整个内容都是 URL 编码（而不是像 G-code 这种只有开头有 %）\r\n                    // G-code 的 % 后面一般是数字或空行，URL 编码的 % 后面是十六进制\r\n                    if (LooksLikeUrlEncoded(content))\r\n                    {\r\n                        _logger.LogWarning(\"检测到 URL 编码响应，正在解码\");\r\n                        content = Uri.UnescapeDataString(content);\r\n                    }\r\n                    // 否则保持原样（G-code 的 % 是合法的）\r\n                }\r\n                \r\n                return (T?)(object?)content;\r\n            }\r\n            \r\n            // 其他类型：走 JSON 反序列化（流式）\r\n            return await JsonSerializer.DeserializeAsync<T>(\r\n                finalStream, _jsonOptions, cancellationToken);\r\n        }\r\n        finally\r\n        {\r\n            if (decompressedStream != null)\r\n                await decompressedStream.DisposeAsync();\r\n            else\r\n                await stream.DisposeAsync();\r\n        }\r\n    }\r\n    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)\r\n    {\r\n        _logger.LogInformation(\"请求被取消: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n    catch (HttpRequestException ex)\r\n    {\r\n        _logger.LogError(ex, \"HTTP 请求失败: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n    catch (JsonException ex)\r\n    {\r\n        _logger.LogError(ex, \"JSON 反序列化失败: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n    catch (Exception ex)\r\n    {\r\n        _logger.LogError(ex, \"请求处理异常: {Method} {Path}\", method, path);\r\n        throw;\r\n    }\r\n}\r\n\r\n//  helper：判断是否是 URL 编码（简单启发式）\r\nprivate static bool LooksLikeUrlEncoded(string content)\r\n{\r\n    // 如果内容里包含大量 %XX 模式（X 是十六进制字符），则认为是 URL 编码\r\n    // G-code 的 % 后面通常是数字（如 %1234），不是十六进制对\r\n    if (content.Length < 3) return false;\r\n    \r\n    // 检查前几个 % 后面是否跟着十六进制字符\r\n    int hexCount = 0;\r\n    for (int i = 0; i < content.Length - 2; i++)\r\n    {\r\n        if (content[i] == '%')\r\n        {\r\n            if (char.IsAsciiHexDigit(content[i + 1]) && char.IsAsciiHexDigit(content[i + 2]))\r\n            {\r\n                hexCount++;\r\n                if (hexCount >= 3) return true; // 有 3 个连续的 %XX，大概率是 URL 编码\r\n            }\r\n            else\r\n            {\r\n                // 遇到 % 后面不是十六进制，可能是 G-code 的 %\r\n                return false;\r\n            }\r\n        }\r\n    }\r\n    return false;\r\n}\r\n调用示例：\r\ncsharp\r\n复制\r\n// 获取 G-code 文本（直接返回字符串，不经过 JSON 解析）\r\nvar gcode = await client.SendAsync<string>(HttpMethod.Get, \"/api/program/123\");\r\n\r\n// 获取 JSON 对象（正常反序列化）\r\nvar result = await client.SendAsync<MyDto>(HttpMethod.Get, \"/api/data\");\r\n关键点：\r\ntypeof(T) == typeof(string) 时直接读文本，保留 G-code 的 % 前缀\r\nLooksLikeUrlEncoded 辅助方法区分 G-code（%1234）和 URL 编码（%7B%22），避免误伤\r\n非字符串类型才走 JSON 反序列化逻辑";
        }

        /// <summary>
        /// 切换运行文件命令执行方法
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteSwitchRunningFileAsync()
        {
            if (SelectedNode == null)
            {
                _message_service.ShowError("请先选择一个文件。");
                return;
            }
            if (SelectedNode.IsDirectory)
            {
                _message_service.ShowError("请选择一个文件，而不是文件夹。");
                return;
            }
            if (string.IsNullOrEmpty(SelectedNode.FullPath))
            {
                _message_service.ShowError("选中的文件路径无效。");
                return;
            }
            if (string.IsNullOrEmpty(SelectedNode.CompleteFullPath))
            {
                _message_service.ShowError("选中的文件完整路径无效。");
                return;
            }
            //加卡控状态卡控和轴归零卡控

            //先确保手动和空闲状态
            var operationMode = await _deviceService.GetOperationModeAsync();
            if (operationMode.Value != DevOperationModeEnum.Jog)
            {
                _message_service.ShowError("先切换手动模式。");
                return;
            }
            var deviceState = await _deviceService.GetDeviceStateAsync();
            if (deviceState.Value != DeviceStateEnum.Free)
            {
                _message_service.ShowError("先确保设备现在是空闲状态。");
                return;
            }
            //// 1. 调用你的接口获取数据
            var allAxisData = await _deviceService.BatchGetAllActualAndRemainingFeedAsync();
            //判断数据
            const double ZeroThreshold = 0.0;

            bool IsAllAxesStopped = allAxisData.Value != null &&
                                    allAxisData.Value.Count >= 5 && // 确保有5轴数据
                                    allAxisData.Value
                                        .Take(5) // 仅检查前5轴（X/Y/Z/B/C）
                                        .All(axis => axis.ActualFeedRate == ZeroThreshold
                                        && !double.IsNaN(axis.ActualFeedRate) && // 实际可省略（0.0 != NaN）
                                           !double.IsInfinity(axis.ActualFeedRate));
            if (!IsAllAxesStopped)
            {
                _message_service.ShowError("先将设备各个轴归零。");
                return;
            }



            var response = await _deviceService.SetSwitchRunningFileAsync(SelectedNode.CompleteFullPath);
            if (response.Value)
                _message_service.ShowMessage("切换成功！");
            else
                _message_service.ShowError("切换失败！");


        }

        /// <summary>
        /// 删除文件操作命令执行方法
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteDeleteFileAsync()
        {
            if (SelectedNode == null)
            {
                _message_service.ShowError("请先选择一个要删除文件。");
                return;
            }
            if (SelectedNode.IsDirectory)
            {
                _message_service.ShowError("不可删除文件夹。");
                return;
            }
            var confirm = await _dialogService.ShowConfirmAsync(
                $"确定要删除 '{SelectedNode.Name}' 吗？此操作不可撤销。",
                "确认删除");
            if (!confirm) return;



            var response = await _deviceService.DeleteFileAsync(SelectedNode.FullPath);
            if (response.Value)
            {
                _message_service.ShowMessage("删除成功！");
                // 从树中移除节点
                if (SelectedNode.Parent != null)
                {
                    SelectedNode.Parent.Children.Remove(SelectedNode);
                }
                else
                {
                    TreeItems.Remove(SelectedNode);
                }
            }
            else
            {
                _message_service.ShowError("删除失败！");
            }
        }
        #endregion
        /// <summary>
        /// 启动数据监控
        /// </summary>
        public void Initialize()
        {
            GetDirectoryFileList();
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

        #region 树节点实现方法
        /// <summary>
        /// 获取目录文件列表并构建树节点
        /// </summary>
        /// <returns></returns>
        public async void GetDirectoryFileList()
        {
            var response = await _deviceService.GetDirectoryFileWithDetailsListAsync();
            if (response?.Value?.FileDetailsList is not List<FileDetails> fileList) return;

            TreeItems.Clear();
            var cache = new Dictionary<string, FileNode>(StringComparer.OrdinalIgnoreCase);

            // 第一阶段：创建所有节点（按路径深度排序，确保父级在前）
            foreach (var fileDetail in fileList.OrderBy(f => f.Name.Count(c => c == '/')))
            {
                CreateNode(fileDetail, cache);
            }

            // 第二阶段：建立父子关系
            foreach (var node in cache.Values)
            {
                var parentPath = GetParentPath(node.FullPath);

                if (string.IsNullOrEmpty(parentPath))
                {
                    // 根级节点
                    TreeItems.Add(node);
                }
                else if (cache.TryGetValue(parentPath, out var parent))
                {
                    // 挂到父节点下
                    node.Parent = parent;
                    parent.Children.Add(node);

                    // 确保父节点是目录类型（有子项必定是目录）
                    if (parent.Type != FileTypeEnum.Directory)
                    {
                        parent.Type = FileTypeEnum.Directory;
                        OnPropertyChanged(nameof(parent.IsDirectory));
                        OnPropertyChanged(nameof(parent.DisplaySize));
                    }
                }
            }
        }

        // 创建单个节点
        private FileNode CreateNode(FileDetails details, Dictionary<string, FileNode> cache)
        {
            // 已存在则返回
            if (cache.TryGetValue(details.Name, out var existing)) return existing;

            var node = new FileNode
            {
                Name = GetFileName(details.Name),
                FullPath = details.Name,           // Name 字段实际存的是路径
                Type = details.Type,
                Size = details.Size,
                ChangeTime = details.ChangeTime,
                IsExpanded = details.Type == FileTypeEnum.Directory
            };

            cache[details.Name] = node;
            return node;
        }

        // 获取父路径
        private static string GetParentPath(string path)
        {
            var lastSlash = path.LastIndexOf('/');
            return lastSlash > 0 ? path.Substring(0, lastSlash) : null;
        }

        // 获取文件名（路径最后一部分）
        private static string GetFileName(string path) =>
            path.Split('/').Last();


        #endregion


        /// <summary>
        /// 打开上传文件对话框和相关逻辑处理
        /// </summary>
        /// <returns></returns>

        private async Task OpenUploadFileDialogs()
        {
            try
            {
                var path = string.Empty;
                //做简便操作处理

                // 获取上传路径
                var targetPath = await GetUploadPathAsync();
                if (targetPath == null) return;
                path = targetPath;
                //局部方法 用来确定文件上传到哪里
                async Task<string?> GetUploadPathAsync()
                {
                    // 情况1：什么都没选 → 根目录
                    if (SelectedNode == null)
                    {
                        var ok = await _dialogService.ShowConfirmAsync("不选文件夹默认上传到根目录，是否继续？", "继续");
                        return ok ? "" : null;
                    }

                    // 情况2：选中了文件 → 其父目录
                    if (!SelectedNode.IsDirectory)
                    {
                        var ok = await _dialogService.ShowConfirmAsync("选择文件默认放到该文件父目录，是否继续？", "继续");
                        return ok ? GetBeforeLast(SelectedNode.FullPath, '/') : null;
                    }

                    // 情况3：选中了文件夹 → 直接使用
                    return SelectedNode.FullPath;
                }

                // 静态局部函数 辅助方法（直接写在这里或放到扩展类）
                static string GetBeforeLast(string str, char separator)
                {
                    if (string.IsNullOrEmpty(str)) return str;
                    int lastIndex = str.LastIndexOf(separator);
                    return lastIndex >= 0 ? str.Substring(0, lastIndex) : str;
                }

                //后弹出子对话框输入名称和选择本地文件
                var FileUploadRequest = await _dialogService.ShowDialogAsync<FileUploadRequest?>("UploadFile");

                if (FileUploadRequest == null)
                {
                    // 用户点击了取消
                    _message_service.ShowMessage("操作已取消");
                    return;
                }
                if (string.IsNullOrEmpty(FileUploadRequest.LocalFilePath))
                {
                    _message_service.ShowError("上传文件路径无效。");
                    return;
                }
                if (string.IsNullOrEmpty(FileUploadRequest.FileName))
                    _message_service.ShowError("文件名称不能命名为空");
                FileUploadRequest.LocatedPath = path;

                //请求接口实际执行上传到数控系统操作
                var response = await _deviceService.UploadFileAsync(FileUploadRequest);
                if (response.Code != 0)
                {
                    _message_service.ShowError($"上传异常: {response.Status}");

                }

            }
            catch (Exception ex)
            {
                _message_service.ShowError($"对话框异常: {ex.Message}");
            }
        }

        // 清理方法（页面关闭时调用）
        public void Cleanup()
        {
            _cts?.Cancel(); // 停止循环
        }


    }
}

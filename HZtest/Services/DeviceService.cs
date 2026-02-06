using HZtest.Constants;
using HZtest.Converters;
using HZtest.DTO;
using HZtest.Models;
using HZtest.Models.Request;
using HZtest.Models.Response;
using HZtest.Universal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HZtest.Services
{
    /// <summary>
    /// 注入式 DeviceService —— 不再使用静态方法/字段，便于 DI/测试/并发控制
    /// 注册建议：Singleton（保持 CurrentSNCode 状态）或根据需求调整为 Scoped/Transient
    /// </summary>
    public class DeviceService
    {
        private readonly ApiClient _apiClient;

        // 保存当前 SN（实例字段，不再静态）
        private string? _currentSNCode;
        public string? CurrentSNCode => _currentSNCode;

        public DeviceService(ApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }
        /// <summary>
        /// 设置当前操作的设备 SNCode
        /// </summary>
        /// <param name="snCode"></param>
        public void SetCurrentSNCode(string snCode)
        {
            _currentSNCode = snCode;
        }

        public string GetCurrentSNCode() => CurrentSNCode ?? "未设置";

        public void ClearSNCode() => _currentSNCode = null;

        #region 主页面操作接口
        /// <summary>
        /// 获取连接状态
        /// </summary>
        public async Task<BaseResponse<string>> GetDeviceInfoAsync(string snCode)
        {
            try
            {
                var result = await _apiClient.SendAsync<BaseResponse<string>>(HttpMethod.Get, $"/tools/test/{snCode}").ConfigureAwait(false);
                return result ?? new BaseResponse<string>();
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<string>
                {
                    Status = "接口错误",
                    Value = $"网络错误: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 单个获取轴实际进给数据
        /// </summary>
        public async Task<BaseResponse<AxisData>> GetActualFeedAsync(AxisEnum axis)
        {
            try
            {
                var baseResp = new BaseResponse<AxisData>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.ActualFeed } });

                var result = await _apiClient.SendAsync<BaseResponse<int[][]>>(HttpMethod.Get, $"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                baseResp.Code = result?.Code ?? -1;
                baseResp.Status = result?.Status ?? "无响应";

                if (result?.Code == 0 && result.Value != null)
                {
                    var axisData = UniversalValueConversion.ParseAxisData(axis, request, result.Value);
                    baseResp.Value = axisData;
                }
                return baseResp;
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<AxisData>
                {
                    Status = $"网络错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 单个获取轴剩余进给数据
        /// </summary>
        public async Task<BaseResponse<AxisData>> GetRemainingFeedAsync(AxisEnum axis)
        {
            try
            {
                var baseResp = new BaseResponse<AxisData>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed } });

                var result = await _apiClient.SendAsync<BaseResponse<int[][]>>(HttpMethod.Get, $"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                baseResp.Code = result?.Code ?? -1;
                baseResp.Status = result?.Status ?? "无响应";

                if (result?.Code == 0 && result.Value != null)
                {
                    var axisData = UniversalValueConversion.ParseAxisData(axis, request, result.Value);
                    baseResp.Value = axisData;
                }
                return baseResp;
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<AxisData>
                {
                    Status = $"网络错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 单个获取轴实际进给和剩余进给数据
        /// </summary>
        public async Task<BaseResponse<AxisData>> GetActualAndRemainingFeedAsync(AxisEnum axis)
        {
            try
            {
                var baseResp = new BaseResponse<AxisData>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed, AxisDataIndices.ActualFeed } });

                var result = await _apiClient.SendAsync<BaseResponse<int[][]>>(HttpMethod.Get, $"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                baseResp.Code = result?.Code ?? -1;
                baseResp.Status = result?.Status ?? "无响应";

                if (result?.Code == 0 && result.Value != null)
                {
                    var axisData = UniversalValueConversion.ParseAxisData(axis, request, result.Value);
                    baseResp.Value = axisData;
                }
                return baseResp;
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<AxisData>
                {
                    Status = $"网络错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 批量获取轴实际进给和剩余进给数据
        /// </summary>
        public async Task<BaseResponse<List<AxisData>>> BatchGetActualAndRemainingFeedAsync(List<AxisEnum> axisList)
        {
            try
            {
                var baseResp = new BaseResponse<List<AxisData>>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                foreach (var axis in axisList)
                {
                    request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed, AxisDataIndices.ActualFeed } });
                }

                var result = await _apiClient.SendAsync<BaseResponse<int[][]>>(HttpMethod.Get, $"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                baseResp.Code = result?.Code ?? -1;
                baseResp.Status = result?.Status ?? "无响应";

                if (result?.Code == 0 && result.Value != null)
                {
                    var axisData = UniversalValueConversion.ParseAxisData(axisList, request, result.Value);
                    baseResp.Value = axisData;
                }
                return baseResp;
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<List<AxisData>>
                {
                    Status = $"网络错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 批量获取全部轴实际进给和剩余进给数据 --默认全部
        /// </summary>
        public async Task<BaseResponse<List<AxisData>>> BatchGetAllActualAndRemainingFeedAsync()
        {
            try
            {
                var baseResp = new BaseResponse<List<AxisData>>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                var axisList = Enum.GetValues(typeof(AxisEnum)).Cast<AxisEnum>().ToList();
                foreach (var axis in axisList)
                {
                    request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed, AxisDataIndices.ActualFeed } });
                }

                var result = await _apiClient.SendAsync<BaseResponse<int[][]>>(HttpMethod.Get, $"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                baseResp.Code = result?.Code ?? -1;
                baseResp.Status = result?.Status ?? "无响应";

                if (result?.Code == 0 && result.Value != null)
                {
                    var axisData = UniversalValueConversion.ParseAxisData(axisList, request, result.Value);
                    baseResp.Value = axisData;
                }
                return baseResp;
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<List<AxisData>>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 获取启动和暂停状态
        /// </summary>
        public async Task<BaseResponse<StartStopState>> GetStartPauseStateAsync()
        {
            try
            {
                var baseResp = new BaseResponse<StartStopState>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem { Path = "/MACHINE/CONTROLLER/VARIABLE@CHAN_0", Index = new[] { ChannelDataIndices.FeedHold, ChannelDataIndices.CycleStart } });

                var result = await _apiClient.SendAsync<BaseResponse<int[][]>>(HttpMethod.Get, $"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                baseResp.Code = result?.Code ?? -1;
                baseResp.Status = result?.Status ?? "无响应";

                if (result?.Code == 0 && result.Value != null)
                {
                    baseResp.Value = ApiDataParser.ParseFromRequest<StartStopState>(request, result.Value);
                }
                return baseResp;
            }
            catch (HttpRequestException ex)
            {
                return new BaseResponse<StartStopState>
                {
                    Status = $"网络错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 设置启动/暂停信号（实例方法，保留并优化原逻辑）
        /// </summary>
        public async Task<BaseResponse<bool>> SetStartPauseStateAsync(StartStopStateDto startStopState)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "set_value",
                    Items = new List<RequestItem>(),
                };

                // 确定寄存器偏移量
                int offset = startStopState.SetStatus switch
                {
                    StartStopStatusEnum.Start => RegisterOffsetConstants.CycleStart,
                    StartStopStatusEnum.Stop => RegisterOffsetConstants.FeedHold,
                    _ => throw new ArgumentException($"不支持的 SetStatus: {startStopState.SetStatus}")
                };

                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@REG_G",
                    Index = RegisterConstants.RuOrStartPause,
                    Offset = offset,
                    Value = Convert.ToInt32(startStopState.State)
                });

                var result = await _apiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                bool? firstValue = result?.Value?.FirstOrDefault();

                return new BaseResponse<bool>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = firstValue ?? default(bool),
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 获取主轴实际转速（实例方法）
        /// </summary>
        public async Task<BaseResponse<int>> GetActualSpindleSpeedAsync()
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<int> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };

                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@CHAN_0",
                    Index = ChannelDataIndices.ActualSpindleSpeed,
                });

                var result = await _apiClient.PostAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

                if (result?.Code == 0 && result.Value != null)
                {
                    var parsed = ApiDataParser.ParseFromRequest<ActualSpindleSpeed>(request, result.Value);
                    return new BaseResponse<int>
                    {
                        Code = result.Code,
                        Status = result.Status,
                        Value = parsed.Speed
                    };
                }

                return new BaseResponse<int>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<int>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 获取运行模式（实例方法）
        /// </summary>
        public async Task<BaseResponse<DevOperationModeEnum>> GetOperationModeAsync()
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<DevOperationModeEnum> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };

                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@CHAN_0",
                    Index = ChannelDataIndices.Mode,
                });

                var result = await _apiClient.PostAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

                var operationMode = new OperationMode();
                if (result?.Code == 0 && result.Value != null)
                {
                    operationMode = ApiDataParser.ParseFromRequest<OperationMode>(request, result.Value);
                }

                return new BaseResponse<DevOperationModeEnum>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = (DevOperationModeEnum)operationMode.CurrentMode,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<DevOperationModeEnum>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 设置运行模式 - 实例方法，保留并优化原逻辑
        /// </summary>
        public async Task<BaseResponse<bool>> SetOperationModeAsync(DevOperationModeEnum mode)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "set_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@REG_G",
                    Index = RegisterConstants.RuOrStartPause,
                    Offset = 0,
                    Value = 0
                });
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@REG_G",
                    Index = RegisterConstants.RuOrStartPause,
                    Offset = 1,
                    Value = 0
                });

                //目前只支持Auto和Jog模式切换
                if (mode == DevOperationModeEnum.Auto)
                {
                    request.Items.Add(new RequestItem
                    {
                        Path = "/MACHINE/CONTROLLER/VARIABLE@REG_G",
                        Index = RegisterConstants.RuOrStartPause,
                        Offset = 0,
                        Value = 1
                    });
                }

                if (mode == DevOperationModeEnum.Jog)
                {
                    request.Items.Add(new RequestItem
                    {
                        Path = "/MACHINE/CONTROLLER/VARIABLE@REG_G",
                        Index = RegisterConstants.RuOrStartPause,
                        Offset = 1,
                        Value = 1
                    });
                }

                var result = await _apiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

                if (result?.Value == null)
                {
                    return new BaseResponse<bool>
                    {
                        Code = result?.Code ?? -1,
                        Status = result?.Status ?? "未知错误",
                        Value = false
                    };
                }

                if (result.Value.Any(x => x == false))
                {
                    return new BaseResponse<bool>
                    {
                        Code = -1,
                        Status = "设置失败",
                        Value = false,
                    };
                }

                return new BaseResponse<bool>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = true,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }
        #endregion


        #region 文件操作页面接口
        /// <summary>
        /// 获取当前运行文件名称
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<BaseResponse<FileOperationsModel>> GetTheCurrentRunningFileAsync()
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<FileOperationsModel> { Status = "未设置设备 SNCode" };
            }
            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/PROGRAM",
                });

                var result = await _apiClient.PostAsync<BaseResponse<string[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var fileOperationsModel = new FileOperationsModel();
                fileOperationsModel.RunningFile = result?.Value.ExtractFirstValue() ?? string.Empty;
                return new BaseResponse<FileOperationsModel>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = fileOperationsModel,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FileOperationsModel>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 获取当前运行文件名称详情
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<BaseResponse<FileOperationsModel>> GetTheCurrentRunningDetailsFileAsync(string filsName)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<FileOperationsModel> { Status = "未设置设备 SNCode" };
            }
            if (string.IsNullOrEmpty(filsName))
            {
                return new BaseResponse<FileOperationsModel> { Status = "文件名称为空" };
            }
            try
            {
                var analysisFilsName = Path.GetFileName(filsName);
                if (string.IsNullOrEmpty(analysisFilsName))
                {
                    return new BaseResponse<FileOperationsModel> { Status = "文件名称解析失败" };
                }

                //和其他的不太一样参数传递方式不一样
                var path = $"/v1/{CurrentSNCode}/file?key={analysisFilsName}&path=/MACHINE/CONTROLLER/FILE";
                // 直接读取原始字符串响应（多行文本）
                var result = await _apiClient.GetAsync<string>(path).ConfigureAwait(false);

                var fileOperationsModel = new FileOperationsModel();
                fileOperationsModel.RunningDetailsFile = result ?? string.Empty;
                var response = new BaseResponse<FileOperationsModel>()
                {
                    Code = 0,
                    Status = "成功"
                };

                if (string.IsNullOrEmpty(result))
                {
                    response.Code = -1;
                    response.Status = "文件内容为空";
                }
                response.Value = fileOperationsModel;
                return response;

            }
            catch (Exception ex)
            {
                return new BaseResponse<FileOperationsModel>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }


        /// <summary>
        /// 获取G代码目录下文件列表
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<BaseResponse<FileOperationsModel>> GetDirectoryFileListAsync()
        {

            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<FileOperationsModel> { Status = "未设置设备 SNCode" };
            }
            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_keys",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/FILE",
                });

                var result = await _apiClient.PostAsync<BaseResponse<string[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var fileOperationsModel = new FileOperationsModel();
                fileOperationsModel.DirectoryFileList = result?.Value[0].ToList() ?? new List<string>();
                var cs = await GetFilesDetailsAsync(fileOperationsModel.DirectoryFileList.ToArray());

                return new BaseResponse<FileOperationsModel>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = fileOperationsModel,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FileOperationsModel>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }
        /// <summary>
        /// 切换运行文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> SetSwitchRunningFileAsync(string completeFullPath)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }
            if (string.IsNullOrEmpty(completeFullPath))
            {
                return new BaseResponse<bool> { Status = "详细文件路径为空" };
            }
            try
            {
                var request = new BaseRequest
                {
                    Operation = "set_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/CONSOLE",
                    Value = $"prog 0 select {completeFullPath}"
                });

                var result = await _apiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                bool? firstValue = result?.Value?.FirstOrDefault();

                return new BaseResponse<bool>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = firstValue ?? default(bool),
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 获取文件数组中的文件是文件夹还是文件
        /// </summary>
        /// <param name="fullNameArray">文件名称数组</param>
        /// <returns></returns>
        public async Task<BaseResponse<List<FileDetails>>> GetFilesDetailsAsync(string[] fullNameArray)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<List<FileDetails>> { Status = "未设置设备 SNCode" };
            }
            if (fullNameArray.Length == 0)
            {
                return new BaseResponse<List<FileDetails>> { Status = "查询的文件数组为空" };
            }
            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_attributes",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/FILE",
                    Key = fullNameArray
                });

                var result = await _apiClient.PostAsync<BaseResponse<FileOperationsResponse[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var fileOperations = result?.Value[0];
                var fileOperationsList = new List<FileDetails>();
                for (int i = 0; i < fileOperations?.Count(); i++)
                {
                    fileOperationsList.Add(new FileDetails()
                    {
                        Name = fullNameArray[i],
                        Type = fileOperations[i].Type == "directory" ? FileTypeEnum.Directory : FileTypeEnum.File,
                        Size = fileOperations[i].Size,
                        ChangeTime = fileOperations[i].ChangeTime,
                    });

                }

                return new BaseResponse<List<FileDetails>>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = fileOperationsList
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<FileDetails>>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }


        /// <summary>
        /// 获取G代码目录下文件列表(带文件详情的)
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResponse<FileOperationsModel>> GetDirectoryFileWithDetailsListAsync()
        {

            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<FileOperationsModel> { Status = "未设置设备 SNCode" };
            }
            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_keys",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/FILE",
                });

                var result = await _apiClient.PostAsync<BaseResponse<string[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var fileOperationsModel = new FileOperationsModel();
                fileOperationsModel.DirectoryFileList = result?.Value[0].ToList() ?? new List<string>();
                var FilesDetails = await GetFilesDetailsAsync(fileOperationsModel.DirectoryFileList.ToArray());
                fileOperationsModel.FileDetailsList = FilesDetails.Value ?? new List<FileDetails>();

                return new BaseResponse<FileOperationsModel>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = fileOperationsModel,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<FileOperationsModel>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }
        /// <summary>
        /// 服务器上传文件到机床
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>

        public async Task<BaseResponse<bool>> UploadFileAsync(FileUploadRequest fileUploadRequest)
        {


            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }
            try
            {

                var result = await _apiClient.UploadFileAsync<BaseResponse<string>>($"/v1/{CurrentSNCode}/file", fileUploadRequest).ConfigureAwait(false);

                if (result == null)
                {
                    return new BaseResponse<bool>
                    {
                        Code = -1,
                        Status = "上传失败，未收到响应",
                        Value = false,
                    };
                }

                return new BaseResponse<bool>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = result?.Status == "SUCCESS",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 删除机床上的文件
        /// </summary>
        /// <param name="fullNameArray">文件名称数组</param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> DeleteFileAsync(string path)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }
            if (string.IsNullOrEmpty(path))
            {
                return new BaseResponse<bool> { Status = "删除文件路径不能为空" };
            }
            try
            {
                var request = new BaseRequest
                {
                    Operation = "delete",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/FILE",
                    Key = path
                });

                var result = await _apiClient.GetAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                //var fileOperations = result?.Value[0];

                return new BaseResponse<bool>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = result?.Value[0] ?? false
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }


        #endregion

        #region 状态信息的获取

        /// <summary>
        /// 获取设备运行状态
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResponse<DeviceStateEnum>> GetDeviceStateAsync()
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<DeviceStateEnum> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };

                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/STATUS",
                });

                var result = await _apiClient.PostAsync<BaseResponse<string[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var state = result?.Value.ExtractFirstValue() ?? string.Empty;
                return new BaseResponse<DeviceStateEnum>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = state.StringToEnum<DeviceStateEnum>() ?? DeviceStateEnum.Error,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<DeviceStateEnum>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }
        /// <summary>
        /// 获取设备报警信息
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResponse<List<DeviceAlarmInforResponse>>> GetDeviceAlarmInforAsync()
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<List<DeviceAlarmInforResponse>> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };

                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/WARNING",
                });

                var result = await _apiClient.PostAsync<BaseResponse<List<DeviceAlarmInforResponse>[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var deviceAlarmInfor = result?.Value?[0]?[0] ?? new List<DeviceAlarmInforResponse>();
                return new BaseResponse<List<DeviceAlarmInforResponse>>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = deviceAlarmInfor
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<DeviceAlarmInforResponse>>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }






        #endregion


    }
}

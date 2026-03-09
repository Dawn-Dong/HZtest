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
        /// 复位信号设置触发
        /// </summary>
        public async Task<BaseResponse<bool>> SetResetTriggerAsync()
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
                    Offset = RegisterOffsetConstants.Reset,
                    Value = 1
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

        /// <summary>
        /// 获取设备刀具信息
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResponse<ToolInfoResponse>> GetDeviceToolInfoAsync(int toolNumber)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<ToolInfoResponse> { Status = "未设置设备 SNCode" };
            }

            try
            {
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                //长度补偿值的索引计算方式，每个刀具占用200个索引，长度补偿在每个刀具的第6个索引位置（偏移5）
                var lengthCompensationIndex = 70000 + ((toolNumber - 1) * 200) + 6;
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
                    Index = lengthCompensationIndex,
                });
                //半径补偿
                var radiusCompensationIndex = 70000 + ((toolNumber - 1) * 200) + 11;
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
                    Index = radiusCompensationIndex,
                });
                //长度磨损
                var lengtWearIndex = 70000 + ((toolNumber - 1) * 200) + 29;
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
                    Index = lengtWearIndex,
                });
                //半径磨损
                var radiusWearIndex = 70000 + ((toolNumber - 1) * 200) + 34;
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
                    Index = radiusWearIndex,
                });
                //综合寿命
                var comprehensiveLifespanIndex = 70000 + ((toolNumber - 1) * 200) + 79;
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
                    Index = comprehensiveLifespanIndex,
                });


                var result = await _apiClient.PostAsync<BaseResponse<double[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

                var deviceToolInforArry = result?.Value;

                var toolInforResponse = new ToolInfoResponse()
                {
                    ToolNumber = toolNumber,
                    LengthCompensation = deviceToolInforArry != null && deviceToolInforArry.Length > 0 ? deviceToolInforArry[0]?[0] ?? 0 : 0,
                    RadiusCompensation = deviceToolInforArry != null && deviceToolInforArry.Length > 1 ? deviceToolInforArry[1]?[0] ?? 0 : 0,
                    LengthWear = deviceToolInforArry != null && deviceToolInforArry.Length > 2 ? deviceToolInforArry[2]?[0] ?? 0 : 0,
                    RadiusWear = deviceToolInforArry != null && deviceToolInforArry.Length > 3 ? deviceToolInforArry[3]?[0] ?? 0 : 0,
                    ComprehensiveLifespan = deviceToolInforArry != null && deviceToolInforArry.Length > 4 ? deviceToolInforArry[4]?[0] ?? 0 : 0,
                };



                return new BaseResponse<ToolInfoResponse>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = toolInforResponse
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ToolInfoResponse>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        ///// <summary>
        ///// 获取设备刀具列表信息
        ///// </summary>
        ///// <returns></returns>
        //public async Task<BaseResponse<List<ToolInfoResponse>>> GetDeviceToolInfoListAsync(string scope)
        //{
        //    if (string.IsNullOrEmpty(CurrentSNCode))
        //    {
        //        return new BaseResponse<List<ToolInfoResponse>> { Status = "未设置设备 SNCode" };
        //    }

        //    try
        //    {
        //        // 1. 验证并解析范围
        //        if (!ValidateScopeFormat(scope, out int minToolNumber, out int maxToolNumber))
        //        {
        //            return new BaseResponse<List<ToolInfoResponse>>
        //            {
        //                Status = $"无效的刀具范围格式: {scope}. 格式应为 'min-max' (例如: 1-10)"
        //            };
        //        }

        //        // 2. 验证范围有效性
        //        if (minToolNumber <= 0 || maxToolNumber < minToolNumber)
        //        {
        //            return new BaseResponse<List<ToolInfoResponse>>
        //            {
        //                Status = $"刀具范围无效: {minToolNumber}-{maxToolNumber}. 范围应为正整数且 min <= max"
        //            };
        //        }

        //        var request = new BaseRequest
        //        {
        //            Operation = "get_value",
        //            Items = new List<RequestItem>(),
        //        };

        //        for (int i = minToolNumber; i <= maxToolNumber; i++)
        //        {

        //        }

        //        //长度补偿值的索引计算方式，每个刀具占用200个索引，长度补偿在每个刀具的第6个索引位置（偏移5）
        //        var lengthCompensationIndex = 70000 + ((toolNumber - 1) * 200) + 6;
        //        request.Items.Add(new RequestItem
        //        {
        //            Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
        //            Index = lengthCompensationIndex,
        //        });
        //        //半径补偿
        //        var radiusCompensationIndex = 70000 + ((toolNumber - 1) * 200) + 11;
        //        request.Items.Add(new RequestItem
        //        {
        //            Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
        //            Index = radiusCompensationIndex,
        //        });
        //        //长度磨损
        //        var lengtWearIndex = 70000 + ((toolNumber - 1) * 200) + 29;
        //        request.Items.Add(new RequestItem
        //        {
        //            Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
        //            Index = lengtWearIndex,
        //        });
        //        //半径磨损
        //        var radiusWearIndex = 70000 + ((toolNumber - 1) * 200) + 34;
        //        request.Items.Add(new RequestItem
        //        {
        //            Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
        //            Index = radiusWearIndex,
        //        });
        //        //综合寿命
        //        var comprehensiveLifespanIndex = 70000 + ((toolNumber - 1) * 200) + 79;
        //        request.Items.Add(new RequestItem
        //        {
        //            Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
        //            Index = comprehensiveLifespanIndex,
        //        });


        //        var result = await _apiClient.PostAsync<BaseResponse<double[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

        //        var deviceToolInforArry = result?.Value;

        //        var toolInforResponse = new ToolInfoResponse()
        //        {
        //            LengthCompensation = deviceToolInforArry != null && deviceToolInforArry.Length > 0 ? deviceToolInforArry[0]?[0] ?? 0 : 0,
        //            RadiusCompensation = deviceToolInforArry != null && deviceToolInforArry.Length > 1 ? deviceToolInforArry[1]?[0] ?? 0 : 0,
        //            LengthWear = deviceToolInforArry != null && deviceToolInforArry.Length > 2 ? deviceToolInforArry[2]?[0] ?? 0 : 0,
        //            RadiusWear = deviceToolInforArry != null && deviceToolInforArry.Length > 3 ? deviceToolInforArry[3]?[0] ?? 0 : 0,
        //            ComprehensiveLifespan = deviceToolInforArry != null && deviceToolInforArry.Length > 4 ? deviceToolInforArry[4]?[0] ?? 0 : 0,
        //        };



        //        return new BaseResponse<ToolInfoResponse>
        //        {
        //            Code = result?.Code ?? -1,
        //            Status = result?.Status ?? "未知错误",
        //            Value = toolInforResponse
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BaseResponse<ToolInfoResponse>
        //        {
        //            Status = $"错误: {ex.Message}",
        //        };
        //    }
        //}
        #region 批量获取刀具信息
        ///// <summary>
        ///// 批量获取设备刀具列表信息（支持范围查询）
        ///// </summary>
        ///// <param name="scope">刀具范围，格式：min-max（例如：1-10）</param>
        ///// <returns>包含刀具列表的响应</returns>
        //public async Task<BaseResponse<List<ToolInfoResponse>>> GetDeviceToolInfoListAsync(string scope)
        //{
        //    if (string.IsNullOrEmpty(CurrentSNCode))
        //    {
        //        return new BaseResponse<List<ToolInfoResponse>> { Status = "未设置设备 SNCode" };
        //    }

        //    try
        //    {
        //        scope = "1-10";
        //        // 1. 验证并解析范围
        //        if (!ValidateScopeFormat(scope, out int minToolNumber, out int maxToolNumber))
        //        {
        //            return new BaseResponse<List<ToolInfoResponse>>
        //            {
        //                Status = $"无效的刀具范围格式: {scope}. 格式应为 'min-max' (例如: 1-10)"
        //            };
        //        }

        //        // 2. 验证范围有效性
        //        if (minToolNumber <= 0 || maxToolNumber < minToolNumber)
        //        {
        //            return new BaseResponse<List<ToolInfoResponse>>
        //            {
        //                Status = $"刀具范围无效: {minToolNumber}-{maxToolNumber}. 范围应为正整数且 min <= max"
        //            };
        //        }

        //        // 3. 构建批量请求（一次性获取所有刀具数据）
        //        var request = BuildBulkToolRequest(minToolNumber, maxToolNumber);

        //        // 4. 发送批量请求
        //        var result = await _apiClient.PostAsync<BaseResponse<double[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

        //        // 5. 解析结果并构建刀具列表
        //        var toolList = ParseToolResponse(result, minToolNumber, maxToolNumber);

        //        // 6. 返回结果
        //        return new BaseResponse<List<ToolInfoResponse>>
        //        {
        //            Code = result?.Code ?? -1,
        //            Status = result?.Status ?? "成功",
        //            Value = toolList
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BaseResponse<List<ToolInfoResponse>>
        //        {
        //            Status = $"获取刀具数据失败: {ex.Message}"
        //        };
        //    }
        //}

        ///// <summary>
        ///// 验证并解析刀具范围
        ///// </summary>
        //private bool ValidateScopeFormat(string scope, out int min, out int max)
        //{
        //    min = 0;
        //    max = 0;

        //    if (string.IsNullOrWhiteSpace(scope))
        //        return false;

        //    string[] parts = scope.Split(new[] { '-' }, 2);
        //    if (parts.Length != 2)
        //        return false;

        //    if (!int.TryParse(parts[0], out min) || !int.TryParse(parts[1], out max))
        //        return false;

        //    return true;
        //}

        ///// <summary>
        ///// 构建批量请求（为所有刀具预计算索引）
        ///// </summary>
        //private BaseRequest BuildBulkToolRequest(int minToolNumber, int maxToolNumber)
        //{
        //    var request = new BaseRequest
        //    {
        //        Operation = "get_value",
        //        Items = new List<RequestItem>()
        //    };

        //    // 为每个刀具计算5个索引（长度补偿、半径补偿、长度磨损、半径磨损、综合寿命）
        //    for (int toolNumber = minToolNumber; toolNumber <= maxToolNumber; toolNumber++)
        //    {
        //        // 长度补偿
        //        AddIndex(request, toolNumber, 6);
        //        // 半径补偿
        //        AddIndex(request, toolNumber, 11);
        //        // 长度磨损
        //        AddIndex(request, toolNumber, 29);
        //        // 半径磨损
        //        AddIndex(request, toolNumber, 34);
        //        // 综合寿命
        //        AddIndex(request, toolNumber, 79);
        //    }

        //    return request;
        //}

        ///// <summary>
        ///// 添加索引到请求
        ///// </summary>
        //private void AddIndex(BaseRequest request, int toolNumber, int offset)
        //{
        //    int index = 70000 + ((toolNumber - 1) * 200) + offset;
        //    request.Items.Add(new RequestItem
        //    {
        //        Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
        //        Index = index
        //    });
        //}

        ///// <summary>
        ///// 解析批量响应（将返回的二维数组转换为刀具列表）
        ///// </summary>
        //private List<ToolInfoResponse> ParseToolResponse(
        //    BaseResponse<double[][]> result,
        //    int minToolNumber,
        //    int maxToolNumber)
        //{
        //    var toolList = new List<ToolInfoResponse>();
        //    int totalTools = maxToolNumber - minToolNumber + 1;

        //    if (result?.Value == null || result.Value.Length != totalTools * 5)
        //    {
        //        // 处理异常情况：返回空列表或部分数据
        //        return Enumerable.Range(minToolNumber, totalTools)
        //                         .Select(_ => new ToolInfoResponse())
        //                         .ToList();
        //    }

        //    // 按刀具分组处理（每5个值对应一个刀具）
        //    for (int i = 0; i < totalTools; i++)
        //    {
        //        var toolResponse = new ToolInfoResponse
        //        {
        //            ToolNumber = i + 1,
        //            LengthCompensation = result.Value[i * 5]?[0] == null ? 0 : result.Value[i * 5][0],
        //            RadiusCompensation = result.Value[i * 5 + 1]?[0] ?? 0.0,
        //            LengthWear = result.Value[i * 5 + 2]?[0] ?? 0,
        //            RadiusWear = result.Value[i * 5 + 3]?[0] ?? 0,
        //            ComprehensiveLifespan = result.Value[i * 5 + 4]?[0] ?? 0
        //        };

        //        toolList.Add(toolResponse);
        //    }

        //    return toolList;
        //}





        /// <summary>
        /// 批量获取设备刀具列表信息（支持范围查询，自动分批次）
        /// </summary>
        /// <param name="scope">刀具范围，格式：min-max（例如：1-100）</param>
        /// <returns>包含刀具列表的响应</returns>
        public async Task<BaseResponse<List<ToolInfoResponse>>> GetDeviceToolInfoListAsync(string scope)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<List<ToolInfoResponse>> { Status = "未设置设备 SNCode" };
            }

            try
            {
                // 1. 验证并解析范围
                if (!ValidateScopeFormat(scope, out int minToolNumber, out int maxToolNumber))
                {
                    return new BaseResponse<List<ToolInfoResponse>>
                    {
                        Status = $"无效的刀具范围格式: {scope}. 格式应为 'min-max' (例如: 1-10)"
                    };
                }

                // 2. 验证范围有效性
                if (minToolNumber <= 0 || maxToolNumber < minToolNumber)
                {
                    return new BaseResponse<List<ToolInfoResponse>>
                    {
                        Status = $"刀具范围无效: {minToolNumber}-{maxToolNumber}. 范围应为正整数且 min <= max"
                    };
                }

                // 3. 分批次处理（每批10条）
                int batchSize = 10; // 每批10条刀具
                int totalTools = maxToolNumber - minToolNumber + 1;
                int batchCount = (int)Math.Ceiling((double)totalTools / batchSize);

                var allToolList = new List<ToolInfoResponse>();
                var failedBatches = new List<int>();

                for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
                {
                    // 计算当前批次的刀具范围
                    int currentMin = minToolNumber + batchIndex * batchSize;
                    int currentMax = Math.Min(currentMin + batchSize - 1, maxToolNumber);

                    // 构建当前批次请求
                    var request = BuildBulkToolRequest(currentMin, currentMax);

                    // 发送当前批次请求
                    var batchResult = await _apiClient.PostAsync<BaseResponse<double[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                    await Task.Delay(100); // 可选：添加短暂延迟以避免过快请求（根据实际情况调整）
                    // 处理批次结果
                    if (batchResult?.Code == 0 && batchResult.Value != null)
                    {
                        var batchToolList = ParseToolResponse(batchResult, currentMin, currentMax, allToolList.Count);
                        allToolList.AddRange(batchToolList);
                    }
                    else
                    {
                        failedBatches.Add(batchIndex);
                    }
                }

                // 4. 返回结果
                if (allToolList.Count == 0 && failedBatches.Count == batchCount)
                {
                    return new BaseResponse<List<ToolInfoResponse>>
                    {
                        Status = $"所有批次请求失败。失败批次: {string.Join(", ", failedBatches)}"
                    };
                }

                return new BaseResponse<List<ToolInfoResponse>>
                {
                    Code = 0,
                    Status = failedBatches.Count > 0
                        ? $"部分批次失败（失败批次: {string.Join(", ", failedBatches)}）"
                        : "成功",
                    Value = allToolList
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<ToolInfoResponse>>
                {
                    Status = $"获取刀具数据失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 验证并解析刀具范围（与之前相同）
        /// </summary>
        private bool ValidateScopeFormat(string scope, out int min, out int max)
        {
            min = 0;
            max = 0;

            if (string.IsNullOrWhiteSpace(scope))
                return false;

            string[] parts = scope.Split(new[] { '-' }, 2);
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out min) || !int.TryParse(parts[1], out max))
                return false;

            return true;
        }

        /// <summary>
        /// 构建批量请求（为所有刀具预计算索引，与之前相同）
        /// </summary>
        private BaseRequest BuildBulkToolRequest(int minToolNumber, int maxToolNumber)
        {
            var request = new BaseRequest
            {
                Operation = "get_value",
                Items = new List<RequestItem>()
            };

            for (int toolNumber = minToolNumber; toolNumber <= maxToolNumber; toolNumber++)
            {
                AddIndex(request, toolNumber, 6);
                AddIndex(request, toolNumber, 11);
                AddIndex(request, toolNumber, 29);
                AddIndex(request, toolNumber, 34);
                AddIndex(request, toolNumber, 79);
            }

            return request;
        }

        /// <summary>
        /// 添加索引到请求（与之前相同）
        /// </summary>
        private void AddIndex(BaseRequest request, int toolNumber, int offset)
        {
            int index = 70000 + ((toolNumber - 1) * 200) + offset;
            request.Items.Add(new RequestItem
            {
                Path = "/MACHINE/CONTROLLER/VARIABLE@MACRO",
                Index = index
            });
        }

        /// <summary>
        /// 解析批量响应（已修复类型问题）
        /// </summary>
        private List<ToolInfoResponse> ParseToolResponse(
            BaseResponse<double[][]> result,
            int minToolNumber,
            int maxToolNumber,
            int existingLength
            )
        {
            var toolList = new List<ToolInfoResponse>();
            int totalTools = maxToolNumber - minToolNumber + 1;

            if (result?.Value == null || result.Value.Length != totalTools * 5)
            {
                // 返回空列表（工业级处理：不返回错误，而是返回空数据）
                return new List<ToolInfoResponse>();
            }

            for (int i = 0; i < totalTools; i++)
            {
                var toolResponse = new ToolInfoResponse
                {
                    // ✅ 工业级修复：使用 double 类型的默认值 0.0
                    ToolNumber = i + existingLength + 1,
                    LengthCompensation = result.Value[i * 5]?[0] ?? 0.0,
                    RadiusCompensation = result.Value[i * 5 + 1]?[0] ?? 0.0,
                    LengthWear = result.Value[i * 5 + 2]?[0] ?? 0.0,
                    RadiusWear = result.Value[i * 5 + 3]?[0] ?? 0.0,
                    ComprehensiveLifespan = result.Value[i * 5 + 4]?[0] ?? 0.0
                };

                toolList.Add(toolResponse);
            }

            return toolList;
        }

        #endregion
        /// <summary>
        /// 获取用户变量值
        /// </summary>
        /// <param name="readAddress">要获取的变量地址</param>
        /// <returns></returns>
        public async Task<BaseResponse<double>> GetUserVariablesAsync(UserVariablesReadWriteRequest userVariables)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<double> { Status = "未设置设备 SNCode" };
            }
            try
            {
                if (userVariables.OperationAddressNumber < 700000 || userVariables.OperationAddressNumber > 719999)
                {
                    return new BaseResponse<double> { Status = "用户变量地址必须在700000-719999之间" };
                }
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/PARAMETER",
                    Index = userVariables.OperationAddressNumber,
                });
                var result = await _apiClient.GetAsync<BaseResponse<double[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                return new BaseResponse<double>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = result?.Value[0][0] ?? 0
                };

            }
            catch (Exception ex)
            {
                return new BaseResponse<double>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }
        /// <summary>
        /// 写入用户变量值
        /// </summary>
        /// <param name="readAddress">要获取的变量地址</param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> SetUserVariablesAsync(UserVariablesReadWriteRequest userVariables)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }
            try
            {
                if (userVariables.OperationAddressNumber < 700000 || userVariables.OperationAddressNumber > 719999)
                {
                    return new BaseResponse<bool> { Status = "用户变量地址必须在700000-719999之间" };
                }
                if (userVariables.WriteValue == null)
                    return new BaseResponse<bool> { Status = "写入值为空" };

                var request = new BaseRequest
                {
                    Operation = "set_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/PARAMETER",
                    Index = userVariables.OperationAddressNumber,
                    Value = userVariables.WriteValue,
                });
                var result = await _apiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
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
        #region 相对坐标系接口


        /// <summary>
        /// 获取坐标系值
        /// </summary>
        /// <param name="">要获取的变量地址</param>
        /// <returns></returns>
        public async Task<BaseResponse<RelativeCoordinateSystemResponse>> GetCoordinateSystemAsync(int OperatingCoordinateSystemId)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<RelativeCoordinateSystemResponse> { Status = "未设置设备 SNCode" };
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
                    Path = "/MACHINE/CONTROLLER/COORDINATE",
                    Index = OperatingCoordinateSystemId,
                });
                var result = await _apiClient.GetAsync<BaseResponse<RelativeCoordinateSystemResponse[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                return new BaseResponse<RelativeCoordinateSystemResponse>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = result?.Value[0][0] ?? new RelativeCoordinateSystemResponse()
                };

            }
            catch (Exception ex)
            {
                return new BaseResponse<RelativeCoordinateSystemResponse>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }


        /// <summary>
        /// 上传坐标系值
        /// </summary>
        /// <param name="">要获取的变量地址</param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> SetCoordinateSystemAsync(RelativeCoordinateSystemRequest coordinateSystemRequest)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<bool> { Status = "未设置设备 SNCode" };
            }
            try
            {
                var value = new
                {
                    x = coordinateSystemRequest.XAxisValue,
                    y = coordinateSystemRequest.YAxisValue,
                    z = coordinateSystemRequest.ZAxisValue,
                    b = coordinateSystemRequest.BAxisValue,
                    c = coordinateSystemRequest.CAxisValue
                };

                var request = new BaseRequest
                {
                    Operation = "set_value",
                    Items = new List<RequestItem>(),
                };
                request.Items.Add(new RequestItem
                {
                    Path = "/MACHINE/CONTROLLER/COORDINATE",
                    Index = coordinateSystemRequest.OperatingCoordinateSystemId,
                    Value = value
                });

                var result = await _apiClient.GetAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);

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


        #region 寄存器操作接口

        /// <summary>
        /// 读取寄存器
        /// </summary>
        /// <param name="userVariables"></param>
        /// <returns></returns>
        public async Task<BaseResponse<RegisterOperationResponse>> GetRegisterAsync(RegisterOperationRequest register)
        {
            if (string.IsNullOrEmpty(CurrentSNCode))
            {
                return new BaseResponse<RegisterOperationResponse> { Status = "未设置设备 SNCode" };
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
                    Path = $"/MACHINE/CONTROLLER/VARIABLE@REG_{register.RegisterType.ToString().ToUpper()}",
                    Index = register.RegisterAddress,
                });


                var result = await _apiClient.GetAsync<BaseResponse<int?[][]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
                var registerOperationResponse = new RegisterOperationResponse()
                {
                    DecimalValue = result?.Value[0][0],
                };
                //转换对应寄存器类型的十进制数值为最终的十进制数值（有符号数、无符号数、浮点数等）
                registerOperationResponse.ConvertToDecimalValue(register.RegisterType);

                return new BaseResponse<RegisterOperationResponse>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = registerOperationResponse
                };

            }
            catch (Exception ex)
            {
                return new BaseResponse<RegisterOperationResponse>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 写入寄存器
        /// </summary>
        /// <param name="userVariables"></param>
        /// <returns></returns>
        public async Task<BaseResponse<bool>> SetRegisterAsync(RegisterOperationRequest register)
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
                    Path = $"/MACHINE/CONTROLLER/VARIABLE@REG_{register.RegisterType.ToString().ToUpper()}",
                    Index = register.RegisterAddress,
                    Offset = register.RegisterOffset,
                    Value = register.RegisterWriteValue
                });
                var result = await _apiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request).ConfigureAwait(false);
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

    }
}

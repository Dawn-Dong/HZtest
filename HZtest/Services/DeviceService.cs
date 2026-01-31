using HZtest.Constants;
using HZtest.Converters;
using HZtest.DTO;
using HZtest.Models;
using HZtest.Universal;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HZtest.Services
{
    // 只做一件事：调用API
    public static class DeviceService
    {
        //private static readonly HttpClient _http = new HttpClient();



        private static string _currentSNCode; // 私有字段存储实际值
        // 只允许在为空时写入
        public static string CurrentSNCode
        {
            get => _currentSNCode;
            set
            {
                // 只有在为空时才能赋值
                if (string.IsNullOrEmpty(_currentSNCode))
                {
                    _currentSNCode = value;
                }
                // 否则忽略（已设置过，禁止修改）
            }
        }


        static DeviceService()
        {

        }
        /// <summary>
        /// 新增：获取当前保存的SNCode
        /// </summary>
        public static string GetCurrentSNCode()
        {
            return CurrentSNCode ?? "未设置";
        }

        // 可选：如果需要强制重置，添加额外方法
        public static void ResetSNCode(string newSNCode)
        {
            _currentSNCode = newSNCode;
        }

        // 可选：清空以便重新设置
        public static void ClearSNCode()
        {
            _currentSNCode = null;
        }


        /// <summary>
        /// 获取连接状态
        /// </summary>
        /// <param name="snCode">设备SN码</param>
        /// <returns>连接状态  </returns>
        public static async Task<BaseResponse<string>> GetDeviceInfoAsync(string snCode)
        {
            try
            {



                // 直接调用GET，无请求体
                var json = await ApiClient.GetAsync<BaseResponse<string>>($"/tools/test/{snCode}");

                // // 发送GET请求
                // var response = await _http.GetAsync($"/tools/test/{snCode}");


                // // 确保成功
                //response.EnsureSuccessStatusCode();

                // // 解析响应
                // var json = await response.Content.ReadAsStringAsync();

                return await ApiClient.GetAsync<BaseResponse<string>>($"/tools/test/{snCode}") ?? new BaseResponse<string>();
            }
            catch (HttpRequestException ex)
            {
                // 网络错误处理
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
        /// <param name="axis">要获取的轴</param>
        public static async Task<BaseResponse<AxisData>> GetActualFeedAsync(AxisEnum axis)
        {
            try
            {
                var BaseResponse = new BaseResponse<AxisData>();
                var shuz = axis.ToString().ToUpper();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),

                };
                request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.ActualFeed } });



                //获取当前运行的程序
                //request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/PROGRAM" });


                var result = await ApiClient.GetAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);
                BaseResponse.Code = result.Code;
                BaseResponse.Status = result.Status;


                if (result?.Code == 0 && result.Value != null)
                {
                    // 解析映射
                    var axisData = UniversalValueConversion.ParseAxisData(axis, request, result.Value);
                    BaseResponse.Value = axisData;

                }
                return BaseResponse;



            }
            catch (HttpRequestException ex)
            {
                // 网络错误处理
                return new BaseResponse<AxisData>
                {
                    Status = $"网络错误: {ex.Message}",
                };

            }

        }
        /// <summary>
        /// 单个获取轴剩余进给数据
        /// </summary>
        /// <param name="axis">要获取的轴</param>
        public static async Task<BaseResponse<AxisData>> GetRemainingFeedAsync(AxisEnum axis)
        {
            try
            {
                var BaseResponse = new BaseResponse<AxisData>();
                var shuz = axis.ToString().ToUpper();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),

                };
                request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed } });



                //获取当前运行的程序
                //request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/PROGRAM" });


                var result = await ApiClient.GetAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);
                BaseResponse.Code = result.Code;
                BaseResponse.Status = result.Status;


                if (result?.Code == 0 && result.Value != null)
                {
                    // 解析映射
                    var axisData = UniversalValueConversion.ParseAxisData(axis, request, result.Value);
                    BaseResponse.Value = axisData;

                }
                return BaseResponse;



            }
            catch (HttpRequestException ex)
            {
                // 网络错误处理
                return new BaseResponse<AxisData>
                {
                    Status = $"网络错误: {ex.Message}",
                };

            }

        }

        /// <summary>
        /// 单个获取轴实际进给和剩余进给数据
        /// </summary>
        /// <param name="axis">要获取的轴</param>
        public static async Task<BaseResponse<AxisData>> GetActualAndRemainingFeedAsync(AxisEnum axis)
        {
            try
            {
                var BaseResponse = new BaseResponse<AxisData>();
                var shuz = axis.ToString().ToUpper();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),

                };
                request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed, AxisDataIndices.ActualFeed } });



                //获取当前运行的程序
                //request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/PROGRAM" });


                var result = await ApiClient.GetAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);
                BaseResponse.Code = result.Code;
                BaseResponse.Status = result.Status;


                if (result?.Code == 0 && result.Value != null)
                {
                    // 解析映射
                    var axisData = UniversalValueConversion.ParseAxisData(axis, request, result.Value);
                    BaseResponse.Value = axisData;

                }
                return BaseResponse;



            }
            catch (HttpRequestException ex)
            {
                // 网络错误处理
                return new BaseResponse<AxisData>
                {
                    Status = $"网络错误: {ex.Message}",
                };

            }

        }

        /// <summary>
        /// 批量获取轴实际进给和剩余进给数据
        /// </summary>
        /// <param name="axisList">要获取的轴</param>
        public static async Task<BaseResponse<List<AxisData>>> BatchGetActualAndRemainingFeedAsync(List<AxisEnum> axisList)
        {
            try
            {
                var BaseResponse = new BaseResponse<List<AxisData>>();
                //var shuz = axis.ToString().ToUpper();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),

                };
                foreach (var axis in axisList)
                {
                    request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed, AxisDataIndices.ActualFeed } });
                }



                //获取当前运行的程序
                //request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/PROGRAM" });


                var result = await ApiClient.GetAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);
                BaseResponse.Code = result.Code;
                BaseResponse.Status = result.Status;


                if (result?.Code == 0 && result.Value != null)
                {
                    // 解析映射
                    var axisData = UniversalValueConversion.ParseAxisData(axisList, request, result.Value);
                    BaseResponse.Value = axisData;

                }
                return BaseResponse;



            }
            catch (HttpRequestException ex)
            {
                // 网络错误处理
                return new BaseResponse<List<AxisData>>
                {
                    Status = $"网络错误: {ex.Message}",
                };

            }

        }


        /// <summary>
        /// 批量获取全部轴实际进给和剩余进给数据 --默认全部
        /// </summary>
        /// <param name="axisList">要获取的轴</param>
        public static async Task<BaseResponse<List<AxisData>>> BatchGetAllActualAndRemainingFeedAsync()
        {
            try
            {
                var BaseResponse = new BaseResponse<List<AxisData>>();
                //var shuz = axis.ToString().ToUpper();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),

                };
                // 获取所有枚举值
                var axisList = Enum.GetValues(typeof(AxisEnum)).Cast<AxisEnum>().ToList();
                foreach (var axis in axisList)
                {
                    request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/VARIABLE@AXIS_{axis.GetEnumNumber()}", Index = new[] { AxisDataIndices.RemainingFeed, AxisDataIndices.ActualFeed } });
                }



                //获取当前运行的程序
                //request.Items.Add(new RequestItem { Path = $"/MACHINE/CONTROLLER/PROGRAM" });


                var result = await ApiClient.GetAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);
                BaseResponse.Code = result.Code;
                BaseResponse.Status = result.Status;


                if (result?.Code == 0 && result.Value != null)
                {
                    // 解析映射
                    var axisData = UniversalValueConversion.ParseAxisData(axisList, request, result.Value);
                    BaseResponse.Value = axisData;

                }
                return BaseResponse;



            }
            catch (HttpRequestException ex)
            {
                // 网络错误处理
                return new BaseResponse<List<AxisData>>
                {
                    Status = $"错误: {ex.Message}",
                };

            }

        }

        /// <summary>
        /// 获取启动和暂停状态
        /// </summary>
        /// <returns></returns>
        public static async Task<BaseResponse<StartStopState>> GetStartPauseStateAsync()
        {
            try
            {
                var BaseResponse = new BaseResponse<StartStopState>();
                var request = new BaseRequest
                {
                    Operation = "get_value",
                    Items = new List<RequestItem>(),

                };
                // request.Items.Add(new RequestItem { Path = "/MACHINE/CONTROLLER/VARIABLE@CHAN_0", Index = new[] { ChannelDataIndices.CycleStart, ChannelDataIndices.FeedHold } });
                request.Items.Add(new RequestItem { Path = "/MACHINE/CONTROLLER/VARIABLE@CHAN_0", Index = new[] { ChannelDataIndices.FeedHold, ChannelDataIndices.CycleStart } });

                var result = await ApiClient.GetAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);
                BaseResponse.Code = result.Code;
                BaseResponse.Status = result.Status;

                if (result?.Code == 0 && result.Value != null)
                {
                    BaseResponse.Value = ApiDataParser.ParseFromRequest<StartStopState>(request, result.Value);
                }

                return BaseResponse;

            }
            catch (Exception ex)
            {
                // 网络错误处理
                return new BaseResponse<StartStopState>
                {
                    Status = $"错误: {ex.Message}",
                };
            }

        }
        /// <summary>
        /// 设置进给保持和循环启动
        /// </summary>
        /// <param name="startStopState">操作那个按钮操作的值</param>
        /// <returns></returns>

        public static async Task<BaseResponse<bool>> SetStartPauseStateAsync(StartStopStateDto startStopState)
        {
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

                var result = await ApiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request);
                // 安全提取第一个 bool 值
                bool? firstValue = result?.Value?.FirstOrDefault();

                return new BaseResponse<bool>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = firstValue ?? default(bool),
                    // 可选：如果 API 成功但无数据，是否算失败？按需处理
                };

            }
            catch (Exception ex)
            {
                // 网络错误处理
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 获取主轴实际转速
        /// </summary>
        /// <param name="startStopState"></param>
        /// <returns></returns>
        public static async Task<BaseResponse<int>> GetActualSpindleSpeedAsync()
        {

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

                //var jason = await ApiClient.GetAsync($"/v1/{CurrentSNCode}/data", true, request);
                var result = await ApiClient.PostAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);


                return new BaseResponse<int>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    //Value = result.Speed,
                    // 可选：如果 API 成功但无数据，是否算失败？按需处理
                };

            }
            catch (Exception ex)
            {
                // 网络错误处理
                return new BaseResponse<int>
                {
                    Status = $"错误: {ex.Message}",
                };

            }

        }

        /// <summary>
        /// 获取运行模式
        /// </summary>
        /// <returns></returns>
        public static async Task<BaseResponse<string>> GetOperationModeAsync()
        {
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

                var result = await ApiClient.PostAsync<BaseResponse<int[][]>>($"/v1/{CurrentSNCode}/data", request);

                var operationMode = new OperationMode();
                if (result?.Code == 0 && result.Value != null)
                {
                    operationMode = ApiDataParser.ParseFromRequest<OperationMode>(request, result.Value);

                }
                return new BaseResponse<string>
                {
                    Code = result?.Code ?? -1,
                    Status = result?.Status ?? "未知错误",
                    Value = UniversalValueConversion.GetDescriptionFromInt<DevOperationModeEnum>(operationMode.CurrentMode),
                    // 可选：如果 API 成功但无数据，是否算失败？按需处理
                };
            }
            catch (Exception ex)
            {
                // 网络错误处理
                return new BaseResponse<string>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// 设置运行模式 - 慎用该功能
        /// </summary>
        /// <returns></returns>
        public static async Task<BaseResponse<bool>> SetOperationModeAsync(DevOperationModeEnum mode)
        {
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

                var result = await ApiClient.PostAsync<BaseResponse<bool[]>>($"/v1/{CurrentSNCode}/data", request);
                // 安全提取第一个 bool 值
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
                    // 可选：如果 API 成功但无数据，是否算失败？按需处理
                };
            }
            catch (Exception ex)
            {
                // 网络错误处理
                return new BaseResponse<bool>
                {
                    Status = $"错误: {ex.Message}",
                };
            }
        }







    }


}

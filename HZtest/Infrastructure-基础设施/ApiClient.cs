// Services/ApiClient.cs
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HZtest.Universal
{
    /// <summary>
    /// 通用HTTP客户端，负责序列化、发送请求、接收响应
    /// </summary>
    public static class ApiClient
    {
        private static readonly HttpClient _http = new HttpClient();
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        static ApiClient()
        {
            var settings = App.Configuration.GetSection("ApiSettings").Get<AppSettings>() ?? new AppSettings();
            _http.BaseAddress = new Uri(settings.BaseUrl);
        }

        /// <summary>
        /// 发送GET请求并返回反序列化结果
        /// </summary>
        public static async Task<TResponse> GetAsync<TResponse>(string path, object requestBody = null)
        {
            return await SendRequestAsync<TResponse>(HttpMethod.Get, path, requestBody);
        }

        /// <summary>
        /// 发送POST请求并返回反序列化结果
        /// </summary>
        public static async Task<TResponse> PostAsync<TResponse>(string path, object requestBody = null)
        {
            return await SendRequestAsync<TResponse>(HttpMethod.Post, path, requestBody);
        }

        /// <summary>
        /// 发送GET请求并返回反序列化结果
        /// </summary>
        public static async Task<string> GetAsync(string path, bool isSerialization, object requestBody = null)
        {
            return await SendRequestAsync(HttpMethod.Get, path, requestBody, isSerialization );
        }

        /// <summary>
        /// 发送POST请求并返回反序列化结果
        /// </summary>
        public static async Task<string> PostAsync(string path, bool isSerialization , object requestBody = null )
        {
            return await SendRequestAsync(HttpMethod.Post, path, requestBody, isSerialization);
        }

        /// <summary>
        /// 通用请求方法（私有）
        /// </summary>
        private static async Task<string> SendRequestAsync(
            HttpMethod method,
            string path,
            object requestBody,
            bool isSerialization)
        {
            try
            {
                // 构建请求
                var request = new HttpRequestMessage(method, path);

                // 如果有请求体，序列化并添加
                if (requestBody != null)
                {
                    string jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }

                // 发送请求
                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // 反序列化响应
                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception ex)
            {
                // 统一错误处理（可记录日志）
                System.Diagnostics.Debug.WriteLine($"请求失败: {ex.Message}");
                throw;     // 抛出异常，让调用方处理
            }
        }

        /// <summary>
        /// 不jason序列化通用请求方法（私有）
        /// </summary>
        private static async Task<TResponse> SendRequestAsync<TResponse>(
            HttpMethod method,
            string path,
            object requestBody
            )
        {
            try
            {
                // 构建请求
                var request = new HttpRequestMessage(method, path);

                // 如果有请求体，序列化并添加
                if (requestBody != null)
                {
                    string jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }

                // 发送请求
                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // 反序列化响应
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions) ?? default;
            }
            catch (Exception ex)
            {
                // 统一错误处理（可记录日志）
                System.Diagnostics.Debug.WriteLine($"请求失败: {ex.Message}");
                throw;     // 抛出异常，让调用方处理
            }
        }



    }
}
// Services/ApiClient.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HZtest.Universal
{
    /// <summary>
    /// 基于 IHttpClientFactory 的通用 HTTP 客户端（Typed client）。
    /// 提供通用 SendAsync<T>，并在内部使用流式反序列化以减少内存占用。
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiClient> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 通用发送并反序列化为 T（适用于 GET/POST/PUT 等）
        /// </summary>
        public async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(method, path);

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            try
            {
                // ResponseHeadersRead 可让我们先处理头，流式读取内容，降低大响应内存开销
                var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var result = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("请求被取消: {Method} {Path}", method, path);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP 请求失败: {Method} {Path}", method, path);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "反序列化或其他错误: {Method} {Path}", method, path);
                throw;
            }
        }

        /// <summary>
        /// 便捷方法：GET 返回字符串（非反序列化）
        /// </summary>
        public async Task<string> GetStringAsync(string path, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 便捷方法：POST 并反序列化为 T
        /// </summary>
        public Task<T?> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
        {
            return SendAsync<T>(HttpMethod.Post, path, body, cancellationToken);
        }
    }
}
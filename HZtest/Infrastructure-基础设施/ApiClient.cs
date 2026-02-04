using HZtest.Models.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        //public async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken cancellationToken = default)
        //{
        //    using var request = new HttpRequestMessage(method, path);

        //    if (body != null)
        //    {
        //        var json = JsonSerializer.Serialize(body, _jsonOptions);
        //        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        //    }

        //    try
        //    {
        //        // ResponseHeadersRead 可让我们先处理头，流式读取内容，降低大响应内存开销
        //        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        //        response.EnsureSuccessStatusCode();

        //        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        //        var result = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
        //        return result;
        //    }
        //    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        //    {
        //        _logger.LogInformation("请求被取消: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(ex, "HTTP 请求失败: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "反序列化或其他错误: {Method} {Path}", method, path);
        //        throw;
        //    }
        //}

        //public async Task<string> SendCSAsync(HttpMethod method, string path, object? body = null, CancellationToken cancellationToken = default)
        //{
        //    using var request = new HttpRequestMessage(method, path);

        //    if (body != null)
        //    {
        //        var json = JsonSerializer.Serialize(body, _jsonOptions);
        //        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        //    }

        //    try
        //    {
        //        // ResponseHeadersRead 可让我们先处理头，流式读取内容，降低大响应内存开销
        //        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        //        // 调试：记录 Content-Type 和 Content-Encoding
        //        _logger.LogDebug("Content-Type: {Type}, Encoding: {Encoding}",
        //            response.Content.Headers.ContentType?.MediaType,
        //            string.Join(",", response.Content.Headers.ContentEncoding));
        //        response.EnsureSuccessStatusCode();            
        //        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        //        // 处理 Gzip（如果 HttpClientHandler 没配置自动解压）
        //        Stream decompressedStream = stream;
        //        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        //        {
        //            decompressedStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        //        }
        //        else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
        //        {
        //            decompressedStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
        //        }

        //        // 读取文本（即使是流式，要返回 string 也得全读入内存）
        //        using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
        //        var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        //        //// 处理 URL 编码（API 有时会返回 %7B%22... 形式的编码 JSON）
        //        //if (!string.IsNullOrEmpty(content) && content.StartsWith("%"))
        //        //{
        //        //    _logger.LogWarning("检测到 URL 编码响应，正在解码");
        //        //    content = WebUtility.UrlDecode(content); // 或 HttpUtility.UrlDecode
        //        //}

        //        return content;

        //    }
        //    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        //    {
        //        _logger.LogInformation("请求被取消: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(ex, "HTTP 请求失败: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "反序列化或其他错误: {Method} {Path}", method, path);
        //        throw;
        //    }
        //}
        //public async Task<T?> SendAsync<T>(
        //    HttpMethod method,
        //    string path,
        //    object? body = null,
        //    CancellationToken cancellationToken = default)
        //{
        //    using var request = new HttpRequestMessage(method, path);

        //    if (body != null)
        //    {
        //        var json = JsonSerializer.Serialize(body, _jsonOptions);
        //        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        //    }

        //    try
        //    {
        //        // ResponseHeadersRead 保持流式特性，避免大响应占用过多内存
        //        var response = await _http.SendAsync(
        //            request,
        //            HttpCompletionOption.ResponseHeadersRead,
        //            cancellationToken).ConfigureAwait(false);

        //        _logger.LogDebug(
        //            "Content-Type: {Type}, Encoding: {Encoding}",
        //            response.Content.Headers.ContentType?.MediaType,
        //            string.Join(",", response.Content.Headers.ContentEncoding));

        //        response.EnsureSuccessStatusCode();

        //        // 获取响应流（无需等待完整内容下载）
        //        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        //        // 处理压缩（如果 HttpClientHandler 未配置自动解压）
        //        Stream? decompressedStream = null;
        //        try
        //        {
        //            var finalStream = stream;

        //            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        //            {
        //                finalStream = decompressedStream = new GZipStream(
        //                    stream,
        //                    CompressionMode.Decompress,
        //                    leaveOpen: false); // 注意：设为 false，让 GZipStream 关闭时同时关闭底层流
        //            }
        //            else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
        //            {
        //                finalStream = decompressedStream = new DeflateStream(
        //                    stream,
        //                    CompressionMode.Decompress,
        //                    leaveOpen: false);
        //            }
        //            // 关键分支：如果是 string 类型，直接读取文本（支持 G-code、URL 编码等）
        //            if (typeof(T) == typeof(string))
        //            {
        //                using var reader = new StreamReader(finalStream, Encoding.UTF8);
        //                var content = await reader.ReadToEndAsync(cancellationToken);

        //                // 处理 URL 编码（如果 API 返回 %7B%22... 这种）
        //                if (!string.IsNullOrEmpty(content) && content[0] == '%')
        //                {
        //                    // 检查是否整个内容都是 URL 编码（而不是像 G-code 这种只有开头有 %）
        //                    // G-code 的 % 后面一般是数字或空行，URL 编码的 % 后面是十六进制
        //                    if (LooksLikeUrlEncoded(content))
        //                    {
        //                        _logger.LogWarning("检测到 URL 编码响应，正在解码");
        //                        content = Uri.UnescapeDataString(content);
        //                    }
        //                    // 否则保持原样（G-code 的 % 是合法的）
        //                }

        //                return (T?)(object?)content;
        //            }

        //            // 其他类型：走 JSON 反序列化（流式）
        //            return await JsonSerializer.DeserializeAsync<T>(
        //                finalStream, _jsonOptions, cancellationToken);
        //        }
        //        finally
        //        {
        //            // 确保解压流（如果有）被正确释放
        //            if (decompressedStream != null)
        //            {
        //                await decompressedStream.DisposeAsync();
        //            }
        //            else
        //            {
        //                await stream.DisposeAsync();
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        //    {
        //        _logger.LogInformation("请求被取消: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(ex, "HTTP 请求失败: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (JsonException ex)
        //    {
        //        _logger.LogError(ex, "JSON 反序列化失败: {Method} {Path}", method, path);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "请求处理异常: {Method} {Path}", method, path);
        //        throw;
        //    }
        //}



        public async Task<T?> SendAsync<T>(
            HttpMethod method,
            string path,
            object? body = null,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(method, path);

            if (body != null)
            {
                // 【关键】优先判断是否为 HttpContent（含 MultipartFormDataContent）
                if (body is HttpContent httpContent)
                {
                    request.Content = httpContent; // 直接使用，保留 boundary/编码等元数据
                }
                else
                {
                    var json = JsonSerializer.Serialize(body, _jsonOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }
            if (request.Content != null)
            {
                _logger.LogInformation(
                    "【DEBUG】Request Content-Type: {ContentType}\nBoundary: {Boundary}",
                    request.Content.Headers.ContentType?.ToString() ?? "MISSING!",
                    request.Content.Headers.ContentType?.Parameters
                        .FirstOrDefault(p => p.Name == "boundary")?.Value ?? "N/A");
            }
            try
            {
                // ResponseHeadersRead 保持流式特性，避免大响应占用过多内存
                var response = await _http.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);

                _logger.LogDebug(
                    "Content-Type: {Type}, Encoding: {Encoding}",
                    response.Content.Headers.ContentType?.MediaType,
                    string.Join(",", response.Content.Headers.ContentEncoding));

                response.EnsureSuccessStatusCode();

                // 获取响应流（无需等待完整内容下载）
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                // 处理压缩（如果 HttpClientHandler 未配置自动解压）
                Stream? decompressedStream = null;
                try
                {
                    var finalStream = stream;

                    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        finalStream = decompressedStream = new GZipStream(
                            stream,
                            CompressionMode.Decompress,
                            leaveOpen: false); // 注意：设为 false，让 GZipStream 关闭时同时关闭底层流
                    }
                    else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
                    {
                        finalStream = decompressedStream = new DeflateStream(
                            stream,
                            CompressionMode.Decompress,
                            leaveOpen: false);
                    }
                    // 关键分支：如果是 string 类型，直接读取文本（支持 G-code、URL 编码等）
                    if (typeof(T) == typeof(string))
                    {
                        using var reader = new StreamReader(finalStream, Encoding.UTF8);
                        var content = await reader.ReadToEndAsync(cancellationToken);

                        // 处理 URL 编码（如果 API 返回 %7B%22... 这种）
                        if (!string.IsNullOrEmpty(content) && content[0] == '%')
                        {
                            // 检查是否整个内容都是 URL 编码（而不是像 G-code 这种只有开头有 %）
                            // G-code 的 % 后面一般是数字或空行，URL 编码的 % 后面是十六进制
                            if (LooksLikeUrlEncoded(content))
                            {
                                _logger.LogWarning("检测到 URL 编码响应，正在解码");
                                content = Uri.UnescapeDataString(content);
                            }
                            // 否则保持原样（G-code 的 % 是合法的）
                        }

                        return (T?)(object?)content;
                    }

                    // 其他类型：走 JSON 反序列化（流式）
                    return await JsonSerializer.DeserializeAsync<T>(
                        finalStream, _jsonOptions, cancellationToken);
                }
                finally
                {
                    // 确保解压流（如果有）被正确释放
                    if (decompressedStream != null)
                    {
                        await decompressedStream.DisposeAsync();
                    }
                    else
                    {
                        await stream.DisposeAsync();
                    }
                }
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
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON 反序列化失败: {Method} {Path}", method, path);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "请求处理异常: {Method} {Path}", method, path);
                throw;
            }
        }



        /// <summary>
        /// 上传文件（multipart/form-data）
        /// </summary>
        /// <typeparam name="TResponse">API 响应类型（如 BaseResponse<string[][]>）</typeparam>
        /// <param name="snCode">设备 SNCode</param>
        /// <param name="request">上传请求参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API 响应</returns>
        public async Task<TResponse?> UploadFileAsync<TResponse>(
            string path,
            FileUploadRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. 参数验证（前置校验，避免无效请求）
            if (!request.IsValid(out var errorMsg))
                throw new InvalidOperationException($"请求参数无效: {errorMsg}");

            // 2. 构建 multipart/form-data 内容
            using var multipartContent = new MultipartFormDataContent();

            // 添加字符串参数（自动设置 ContentDisposition.Name）
            multipartContent.Add(new StringContent(request.Path), "path");
            multipartContent.Add(new StringContent(request.Operation), "operation");
            multipartContent.Add(new StringContent(request.GetTargetFilePath()), "key");

            // 3. 添加文件内容（流式读取，避免大文件内存爆炸）
            var fileName = request.FileName
                ?? Path.GetFileName(request.LocalFilePath)
                ?? throw new InvalidOperationException("无法获取文件名");

            // 使用 FileStream + StreamContent（内存友好）
            var fileStream = new FileStream(
                request.LocalFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                useAsync: true);

            var fileContent = new StreamContent(fileStream, 8192);
            //fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            //fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            //{
            //    Name = "value",
            //    FileName = fileName
            //};

            multipartContent.Add(fileContent, "value", "[object Object]");

            try
            {



                // 4. 调用通用 SendAsync（已支持 MultipartFormDataContent）
                return await SendAsync<TResponse>(
                    HttpMethod.Post,
                    path,
                    multipartContent, // ✅ 直接传入 MultipartFormDataContent
                    cancellationToken);
            }
            catch
            {
                // 确保文件流在异常时释放（MultipartFormDataContent 会 Dispose 内容）
                fileStream?.Dispose();
                fileContent?.Dispose();
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
        /// <summary>
        /// 便捷方法：Get 并反序列化为 T
        /// </summary>
        public Task<T?> GetAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
        {
            return SendAsync<T>(HttpMethod.Get, path, body, cancellationToken);
        }

        //  helper：判断是否是 URL 编码（简单启发式）
        private static bool LooksLikeUrlEncoded(string content)
        {
            // 如果内容里包含大量 %XX 模式（X 是十六进制字符），则认为是 URL 编码
            // G-code 的 % 后面通常是数字（如 %1234），不是十六进制对
            if (content.Length < 3) return false;

            // 检查前几个 % 后面是否跟着十六进制字符
            int hexCount = 0;
            for (int i = 0; i < content.Length - 2; i++)
            {
                if (content[i] == '%')
                {
                    if (char.IsAsciiHexDigit(content[i + 1]) && char.IsAsciiHexDigit(content[i + 2]))
                    {
                        hexCount++;
                        if (hexCount >= 3) return true; // 有 3 个连续的 %XX，大概率是 URL 编码
                    }
                    else
                    {
                        // 遇到 % 后面不是十六进制，可能是 G-code 的 %
                        return false;
                    }
                }
            }
            return false;
        }

    }
}
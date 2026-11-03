using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace XHD.Core.View.Mcp
{
    /// <summary>
    /// MCP 工具不直接写业务逻辑，统一转发到现有 /API 控制器。
    /// 当前请求的 Bearer Token 会原样传给 API，由 API 再做一次权限校验。
    /// </summary>
    public sealed class McpApiForwarder
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public McpApiForwarder(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public Task<string> GetAsync(string path, params (string Key, object Value)[] query)
        {
            var url = BuildUrl(path, query);
            return SendAsync(HttpMethod.Get, url, null);
        }

        public Task<string> PostJsonAsync(string path, object body)
        {
            var url = BuildUrl(path, Array.Empty<(string, object)>());
            return SendAsync(HttpMethod.Post, url, body);
        }

        public Task<string> PostFormAsync(string path, IEnumerable<KeyValuePair<string, string>> form)
        {
            var url = BuildUrl(path, Array.Empty<(string, object)>());
            var content = new FormUrlEncodedContent(form);
            return SendAsync(HttpMethod.Post, url, content);
        }

        private string BuildUrl(string path, (string Key, object Value)[] query)
        {
            var context = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("MCP 请求上下文不存在。");

            var configured = _configuration["Mcp:ApiBaseUrl"];
            var baseUrl = !string.IsNullOrWhiteSpace(configured)
                ? configured.TrimEnd('/')
                : $"{context.Request.Scheme}://{context.Request.Host}";

            var builder = new StringBuilder(baseUrl);
            if (!path.StartsWith("/")) builder.Append('/');
            builder.Append(path);

            var values = query
                .Where(a => a.Value != null && !string.IsNullOrWhiteSpace(a.Value.ToString()))
                .Select(a => $"{Uri.EscapeDataString(a.Key)}={Uri.EscapeDataString(a.Value.ToString())}")
                .ToList();

            if (values.Count > 0)
            {
                builder.Append(builder.ToString().Contains('?') ? '&' : '?');
                builder.Append(string.Join("&", values));
            }

            return builder.ToString();
        }

        private async Task<string> SendAsync(HttpMethod method, string url, object body)
        {
            var context = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("MCP 请求上下文不存在。");

            var token = context.Request.Headers.Authorization.ToString();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(7).Trim();
            }

            using var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (body is HttpContent content)
            {
                request.Content = content;
            }
            else if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }

            var client = _httpClientFactory.CreateClient("McpApi");
            client.Timeout = TimeSpan.FromSeconds(60);
            using var response = await client.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"{{\"code\":-1,\"msg\":\"API 请求失败：{response.StatusCode}\",\"detail\":{System.Text.Json.JsonSerializer.Serialize(text)}}}";
            }

            return text;
        }
    }
}

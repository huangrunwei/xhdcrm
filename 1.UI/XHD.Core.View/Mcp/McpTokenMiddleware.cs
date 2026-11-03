using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Http;
using XHD.Core.Common;
using XHD.Core.Models;

namespace XHD.Core.View.Mcp
{
    /// <summary>
    /// 仅拦截 /mcp：使用 Web 后台生成的长期 Bearer Token。
    /// 其他 Controller 继续使用原有 Cookie 鉴权，不受影响。
    /// </summary>
    public sealed class McpTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFreeSql _fsql;

        public McpTokenMiddleware(RequestDelegate next, IFreeSql fsql)
        {
            _next = next;
            _fsql = fsql;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/mcp"))
            {
                await _next(context);
                return;
            }

            if (HttpMethods.IsOptions(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            var header = context.Request.Headers.Authorization.ToString();
            var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? header.Substring(7).Trim()
                : header.Trim();

            if (!ApiTokenUtil.IsApiToken(token))
            {
                await WriteUnauthorized(context, "缺少有效的 Bearer Token，请先在 CRM 后台创建 AI 接入令牌。");
                return;
            }

            try
            {
                var hash = ApiTokenUtil.Hash(token);
                var apiToken = _fsql.Select<Sys_ApiToken>()
                    .Where(a => a.token_hash == hash && a.is_revoked != 1)
                    .First();

                if (apiToken == null || (apiToken.expires_at.HasValue && apiToken.expires_at.Value < DateTime.Now))
                {
                    await WriteUnauthorized(context, "Token 无效或已过期，请重新生成。");
                    return;
                }

                var employee = _fsql.Select<hr_employee>().Where(a => a.id == apiToken.emp_id).First();
                if (employee == null
                    || (!string.Equals(employee.uid, "admin", StringComparison.OrdinalIgnoreCase)
                        && (employee.status != 1 || employee.canlogin == 0)))
                {
                    await WriteUnauthorized(context, "账号已被禁用或限制登录。");
                    return;
                }

                if (string.IsNullOrWhiteSpace(apiToken.token_suffix))
                {
                    _fsql.Update<Sys_ApiToken>()
                        .Where(a => a.id == apiToken.id)
                        .Set(a => a.last_used_at, DateTime.Now)
                        .Set(a => a.token_suffix, token.Length > 8 ? token[^8..] : token)
                        .ExecuteAffrows();
                }
                else
                {
                    _fsql.Update<Sys_ApiToken>()
                        .Where(a => a.id == apiToken.id)
                        .Set(a => a.last_used_at, DateTime.Now)
                        .ExecuteAffrows();
                }

                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Sid, employee.id),
                    new Claim(ClaimTypes.Name, employee.name ?? ""),
                    new Claim("uid", employee.uid ?? "")
                }, "McpToken");

                context.User = new ClaimsPrincipal(identity);
                await _next(context);
            }
            catch (Exception)
            {
                await WriteUnauthorized(context, "Token 校验失败，请重新生成。");
            }
        }

        private static async Task WriteUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new { code = -9, msg = message });
        }
    }
}

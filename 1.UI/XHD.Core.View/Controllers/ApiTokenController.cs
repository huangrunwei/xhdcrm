using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XHD.Core.Common;
using XHD.Core.Models;

namespace XHD.Core.View.Controllers
{
    /// <summary>
    /// 用户自助创建长期 API / MCP Token，可配置多个，用于区分不同 AI 客户端。
    /// </summary>
    [Authorize]
    public class ApiTokenController : Controller
    {
        private readonly IFreeSql _fsql;

        public ApiTokenController(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Grid()
        {
            var empId = CurrentEmpId;
            var list = _fsql.Select<Sys_ApiToken>()
                .Where(a => a.emp_id == empId)
                .OrderByDescending(a => a.create_time)
                .ToList();

            var data = list.Select(a =>
            {
                var prefix = (a.token_prefix ?? "").TrimEnd('.');
                var suffix = a.token_suffix ?? "";
                var mask = prefix + "********" + suffix;
                return new
                {
                    id = a.id,
                    token_name = a.token_name,
                    token_prefix = prefix,
                    token_suffix = suffix,
                    token_mask = mask,
                    remarks = a.remarks,
                    create_time = a.create_time?.ToString("yyyy-MM-dd HH:mm"),
                    last_used_at = a.last_used_at?.ToString("yyyy-MM-dd HH:mm") ?? "未使用",
                    expires_at = a.expires_at?.ToString("yyyy-MM-dd") ?? "长期",
                    is_revoked = a.is_revoked == 1,
                    status = a.is_revoked == 1 ? "已撤销" : (a.expires_at.HasValue && a.expires_at.Value < DateTime.Now ? "已过期" : "正常")
                };
            }).ToList();

            return JsonContent(new { code = 0, msg = "", count = data.Count, data });
        }

        [HttpPost]
        public IActionResult Create(string tokenName, string remarks, int expiresDays = 3650)
        {
            var empId = CurrentEmpId;
            if (string.IsNullOrWhiteSpace(empId))
            {
                return JsonContent(new { code = -1, msg = "登录状态失效，请重新登录。" });
            }

            if (string.IsNullOrWhiteSpace(tokenName))
            {
                return JsonContent(new { code = -1, msg = "请填写 Token 名称，例如 WorkBuddy、Cursor。" });
            }

            expiresDays = Math.Clamp(expiresDays, 1, 36500);
            var raw = ApiTokenUtil.Generate();
            var entity = new Sys_ApiToken
            {
                id = Guid.NewGuid().ToString(),
                emp_id = empId,
                token_name = tokenName.Trim(),
                token_hash = ApiTokenUtil.Hash(raw),
                token_prefix = ApiTokenUtil.DisplayPrefix(raw),
                token_suffix = raw.Length > 8 ? raw[^8..] : raw,
                remarks = remarks ?? "",
                expires_at = DateTime.Now.AddDays(expiresDays),
                is_revoked = 0,
                create_id = empId,
                create_time = DateTime.Now
            };

            _fsql.Insert(entity).ExecuteAffrows();
            return JsonContent(new
            {
                code = 0,
                msg = "创建成功，请立即复制 Token，离开页面后不再显示完整 Token。",
                data = new
                {
                    entity.id,
                    entity.token_name,
                    token = raw,
                    expires_at = entity.expires_at?.ToString("yyyy-MM-dd HH:mm")
                }
            });
        }

        [HttpPost]
        public IActionResult Revoke(string id)
        {
            var empId = CurrentEmpId;
            var token = _fsql.Select<Sys_ApiToken>().Where(a => a.id == id && a.emp_id == empId).First();
            if (token == null)
            {
                return JsonContent(new { code = -1, msg = "Token不存在。" });
            }

            _fsql.Update<Sys_ApiToken>()
                .Where(a => a.id == id)
                .Set(a => a.is_revoked, 1)
                .ExecuteAffrows();

            return JsonContent(new { code = 0, msg = "已撤销。" });
        }

        private IActionResult JsonContent(object data)
        {
            return Content(JsonConvert.SerializeObject(data), "application/json; charset=utf-8");
        }

        private string CurrentEmpId => User.FindFirst(ClaimTypes.Sid)?.Value ?? "";
    }
}

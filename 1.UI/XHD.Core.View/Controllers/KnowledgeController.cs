using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using FreeSql;

using XHD.Core.Common;
using XHD.Core.IServices;
using XHD.Core.Models;

namespace XHD.Core.View.Controllers
{
    /// <summary>
    /// 知识库
    /// </summary>
    [Authorize]
    public class KnowledgeController : Controller
    {
        private readonly ICrm_KnowledgeService _service;
        private readonly ISys_logService _logService;
        private readonly IDBAuthService _dBAuthService;
        private readonly IFreeSql _fsql;

        public KnowledgeController(
            ICrm_KnowledgeService service,
            ISys_logService logService,
            IDBAuthService dBAuthService,
            IFreeSql fsql)
        {
            _service = service;
            _logService = logService;
            _dBAuthService = dBAuthService;
            _fsql = fsql;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Info(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Content("缺少知识参数");
            }

            //浏览次数 +1
            await _fsql.Update<Crm_Knowledge>()
                .Set(a => a.view_count == a.view_count + 1)
                .Where(a => a.id == id)
                .ExecuteAffrowsAsync();

            var knowledge = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            if (knowledge == null)
            {
                return Content("知识不存在或已删除");
            }

            ViewData["knowledge"] = knowledge;
            return View();
        }

        public async Task<IActionResult> Add(string id)
        {
            Crm_Knowledge knowledge = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                knowledge = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            }

            ViewData["knowledge"] = knowledge;
            return View();
        }

        public async Task<string> Grid(PageView<Crm_Knowledge> model)
        {
            var exp = BuildQuery();
            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.is_top desc, a.update_time desc, a.create_time desc");
            return result.ToString();
        }

        public async Task<string> Stats()
        {
            var data = await _fsql.Select<Crm_Knowledge>()
                .GroupBy(a => a.category)
                .ToListAsync(a => new { name = a.Key, count = a.Count() });

            var total = data.Sum(a => a.count);
            var list = data.Select(a => new
            {
                name = string.IsNullOrWhiteSpace(a.name) ? "未分类" : a.name,
                count = a.count
            }).ToList();

            return JsonConvert.SerializeObject(new { code = 0, total, data = list });
        }

        private Expression<Func<Crm_Knowledge, bool>> BuildQuery()
        {
            Expression<Func<Crm_Knowledge, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["category"]))
            {
                exp = exp.And(a => a.category == Request.Query["category"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                var keyword = Request.Query["keyword"].ToString();
                exp = exp.And(a => a.title.Contains(keyword) || a.keywords.Contains(keyword) || a.summary.Contains(keyword));
            }

            return exp;
        }

        public async Task<string> Save(Crm_Knowledge model)
        {
            var result = 0;
            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(model.title))
            {
                return XHDResult.Error("标题不能为空！").ToString();
            }

            if (string.IsNullOrWhiteSpace(model.id))
            {
                if (!await _dBAuthService.GetAuth(userId, "Knowledge|add"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = userId;
                model.create_time = DateTime.Now;
                model.update_time = DateTime.Now;
                model.view_count = 0;
                model.is_top = model.is_top ?? 0;

                result = await _service.AddAsync(model);
            }
            else
            {
                if (!await _dBAuthService.GetAuth(userId, "Knowledge|edit"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                var old = (await _service.GridAsync(a => a.id == model.id, 1, 1)).data.FirstOrDefault();
                if (old == null)
                {
                    return XHDResult.Error("找不到数据！").ToString();
                }

                model.create_id = old.create_id;
                model.create_time = old.create_time;
                model.update_time = DateTime.Now;
                model.is_top = model.is_top ?? 0;

                result = await _service.UpdateAsync(model);
            }

            if (result == 0)
            {
                return XHDResult.Error("操作失败，系统错误！").ToString();
            }

            await _logService.AddAsync(new Sys_log
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                EventType = "[知识库]保存",
                EventID = model.id,
                EventTitle = model.title,
                UserID = userId,
                UserName = userName,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now,
                Log_Content = $"分类：{model.category}"
            });

            return XHDResult.Success(model.id).ToString();
        }

        public async Task<string> Delete(string id)
        {
            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;

            if (!await _dBAuthService.GetAuth(userId, "Knowledge|del"))
            {
                return XHDResult.Error("无权限！").ToString();
            }

            var knowledge = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            if (knowledge == null)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            var result = await _service.DeleteAsync(id);
            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            await _logService.DeleteLog(new Sys_log
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                EventType = "[知识库]删除",
                EventID = id,
                EventTitle = knowledge.title,
                UserID = userId,
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now
            });

            return XHDResult.Success().ToString();
        }
    }
}

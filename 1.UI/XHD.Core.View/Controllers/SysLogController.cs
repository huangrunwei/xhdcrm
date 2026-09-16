using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using System.Security.Claims;
using System.Linq.Expressions;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Common;
using XHD.Core.Models;




namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SysLogController : Controller
    {
        private readonly ILogger<SysLogController> _logger;
        private readonly ISys_logService _service;

        public SysLogController(ILogger<SysLogController> logger, ISys_logService service)
        {
            _service = service;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        public async Task<string> Grid(PageView<Sys_log> model)
        {
            Expression<Func<Sys_log, bool>> exp = a => 1==1;

            if (!string.IsNullOrWhiteSpace(Request.Query["date1"]))
            {
                exp = exp.And(a => a.EventDate >= DateTime.Parse(Request.Query["date1"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["date2"]))
            {
                exp = exp.And(a => a.EventDate <= DateTime.Parse(Request.Query["date2"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["logtype"]))
            {
                exp = exp.And(a => a.EventType == Request.Query["logtype"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.EventID == Request.Query["customer_id"]);
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "EventDate Desc");

            return result.ToString();
        }

        public async Task<string> LogType()
        {
            var result = await _service.LogType();

            return result.ToString();
        }

        public async Task<string> Delete(string id)
        {
            if (User.FindFirst(ClaimTypes.Sid).Value!="admin")
            {
                return XHDResult.Error("只有超级管理员才能删除！").ToString();
            }
           
            var result = await _service.DeleteAsync(id);

            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            return XHDResult.Success().ToString();
        }
    }
}

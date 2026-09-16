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
    public class SysInfoController : Controller
    {
        private readonly ILogger<SysLogController> _logger;
        private readonly ISys_infoService _service;

        public SysInfoController(ILogger<SysLogController> logger, ISys_infoService service)
        {
            _service = service;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Grid()
        {
            Expression<Func<Sys_info, bool>> exp = a => 1 == 1;

            var result = await _service.GridAsync(exp);

            return result.ToString();
        }

        public async Task<string> Save(string key,string value)
        { 
            Sys_info model=new Sys_info();

            model.sys_key = key;
            model.sys_value = value;

            var result = await _service.UpdateAsync(model);

            return XHDResult.Success().ToString();

        }
    }
}

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

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Common;
using XHD.Core.Models;

using System.Linq.Expressions;
using Newtonsoft.Json.Converters;
using System.Data;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SysParamTypeController : Controller
    {
        private readonly ILogger<SysParamTypeController> _logger;
        private readonly ISys_Param_TypeService _ParamTypeService;

        public SysParamTypeController(ILogger<SysParamTypeController> logger,
            ISys_Param_TypeService ParamTypeService)
        {
            _logger = logger;
            _ParamTypeService = ParamTypeService;
        }


        public async Task<string> Tree()
        {
            Expression<Func<Sys_Param_Type, bool>> exp = a => 1 == 1;

            var result = await _ParamTypeService.GridAsync(exp, "params_order");

            JArray arr = new JArray();
            foreach (var param in result.data)
            {
                JObject obj = new JObject();
                obj.Add("id", param.id);
                obj.Add("title", param.params_name);
                arr.Add(obj);
            }

            return arr.ToString();
        }
    }
}

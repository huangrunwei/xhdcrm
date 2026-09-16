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
using XHD.Core.Models;
using System.Security.Claims;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Common;
using XHD.Core.View;

using System.Linq.Expressions;

namespace XHD.Core.View.Controllers
{
    public class SysButtonController : Controller
    {
        private readonly ILogger<SysButtonController> _logger;
        private readonly ISys_ButtonService _service;
        private readonly ISys_ButtonRepository _repository;

        public SysButtonController(ILogger<SysButtonController> logger, ISys_ButtonService service, ISys_ButtonRepository repository)
        {
            _service = service;
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> Grid()
        {
            Expression<Func<Sys_Button, bool>> exp = a => true;
            var result = await _service.GridAsync(exp);

            return result.ToString();
        }
    }
}

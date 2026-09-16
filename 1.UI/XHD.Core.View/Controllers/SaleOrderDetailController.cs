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
using System.Collections;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SaleOrderDetailController : Controller
    {
        private readonly ILogger<SaleOrderDetailController> _logger;
        private readonly ISale_order_detailsService _service;

        public SaleOrderDetailController(ILogger<SaleOrderDetailController> logger, ISale_order_detailsService service)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<string> Grid(PageView<Sale_order_details> model)
        {
            Expression<Func<Sale_order_details, bool>> exp = a => a.order_id == Request.Form["id"];

            //if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            //{
            //    exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["T_name"]));
            //}

            var result = await _service.GridAsync(exp);

            return result.ToString();
        }
    }
}

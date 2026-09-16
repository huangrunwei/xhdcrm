using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq.Expressions;
using System.IO;
using Newtonsoft.Json.Linq;
using XHD.Core.IServices;
using XHD.Core.Common;
using XHD.Core.Models;
using XHD.Core.View.Configs;


namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class CustomerBatchController : Controller
    {
        private readonly ICRM_Customer_BathService _service;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<CRM_Customer> _logExt = new();
        private readonly ISys_ParamService _paramService;
        private readonly ISys_Param_ProvincesService _provincesService;
        private readonly ICRM_CustomerService _customerService;
        private readonly IFreeSql _fsql;
        private readonly ISys_logService _logService;

        public CustomerBatchController(ICRM_Customer_BathService service, IDBAuthService dBAuthService, ISys_ParamService paramService, ISys_Param_ProvincesService provincesService, ICRM_CustomerService customerService, IFreeSql fsql, ISys_logService logService)
        {
            _service = service;
            _dBAuthService = dBAuthService;
            _paramService = paramService;
            _provincesService = provincesService;
            _customerService = customerService;
            this._fsql = fsql;
            _logService = logService;
        }

        public IActionResult Add()
        {
            var allParams = _paramService.Grid(p => true).data;
            var cus_industry = allParams.Where(p => p.params_type == "cus_industry").ToDictionary(p => p.id, p => p.params_name);
            var cus_type = allParams.Where(p => p.params_type == "cus_type").ToDictionary(p => p.id, p => p.params_name);
            var cus_level = allParams.Where(p => p.params_type == "cus_level").ToDictionary(p => p.id, p => p.params_name);
            var cus_source = allParams.Where(p => p.params_type == "cus_source").ToDictionary(p => p.id, p => p.params_name);

            var provinces = _provincesService.Grid(p => true).data.ToDictionary(p => p.id, p => p.Provinces);

            ViewData["cus_industry"] = cus_industry;
            ViewData["cus_type"] = cus_type;
            ViewData["cus_level"] = cus_level;
            ViewData["cus_source"] = cus_source;
            ViewData["Province"] = provinces;

            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Grid(PageView<CRM_Customer_Bath> model)
        {
            Expression<Func<CRM_Customer_Bath, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["old_emp_id"]))
            {
                exp = exp.And(a => a.old_emp_id == Request.Query["old_emp_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["new_emp_id"]))
            {
                exp = exp.And(a => a.new_emp_id == Request.Query["new_emp_id"]);
            }


            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(a => a.create_time >= DateTime.Parse(Request.Query["date1"]));
            }

            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(a => a.create_time <= DateTime.Parse(Request.Query["date2"]));
            }

            //权限


            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");

            return result.ToString();
        }

        public async Task<string> Save(CRM_Customer_Bath model)
        {
            var result = 0;

            model.id = UUIDNext.Uuid.NewSequential().ToString();
            model.create_time = DateTime.Now;
            model.create_id= User.FindFirst(ClaimTypes.Sid).Value;            

            // --- 转移事件已记录，开始转移客户 ---
            // 1. 从 Request.Form 中单独获取 customer_ids 字段
            var customerIdsStr = Request.Form["customer_ids"].ToString();

            int count = 0;

            if (!string.IsNullOrEmpty(customerIdsStr))
            {
                // 2. 按逗号分割，得到 ID 数组
                var ids = customerIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                count = ids.Length;

                // 2. 循环处理每一个客户
                foreach (var id in ids)
                {
                    await _fsql.Update<CRM_Customer>().Set(a => a.emp_id == model.new_emp_id).Where(a => a.id == id).ExecuteAffrowsAsync();

                    // 4. 记录日志
                    Sys_log logmodels = new Sys_log();

                    logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                    logmodels.EventType = "[客户]批量转移";
                    logmodels.EventID = model.id; // 关联本次批量操作的批次ID
                    logmodels.cus_id = id;        // 记录具体被操作的客户ID
                    logmodels.UserID = User.FindFirst(ClaimTypes.Sid).Value;
                    logmodels.UserName = User.FindFirst(ClaimTypes.Name).Value;
                    logmodels.IPStreet = HttpContext.Connection.RemoteIpAddress.ToString();
                    logmodels.EventDate = DateTime.Now;
                    logmodels.Log_Content = $"将客户{id}从员工 {model.old_emp_id} 转移给员工 {model.new_emp_id}";

                    await _logService.UpdateLog(logmodels);
                }
            }

            model.cus_count = count;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "CRM_Customer_Bath|add");

            result = await _service.AddAsync(model);

            return XHDResult.Success().ToString();
        }
    }
}

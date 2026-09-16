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
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SaleContractController : Controller
    {
        private readonly ILogger<SaleContractController> _logger;
        private readonly ISale_contractService _service;
        private readonly ISale_contract_attaService _detailservice;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sale_contract> logext = new SysLogExt<Sale_contract>();  //日志

        public SaleContractController(ILogger<SaleContractController> logger, ISale_contractService service, ISale_contract_attaService detailservice, ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _detailservice = detailservice;

            _LogService = LogService;
            _dBAuthService = dBAuthService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        public async Task<string> Grid(PageView<Sale_contract> model)
        {
            Expression<Func<Sale_contract, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["cus_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["Contract_name"]))
            {
                exp = exp.And(a => a.Contract_name.Contains(Request.Query["Contract_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                exp = exp.And(a => a.Our_Contractor_id == Request.Query["emp_id"]);
            }

            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(a => a.Sign_date >= DateTime.Parse(Request.Query["date1"]));
            }

            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(a => a.Sign_date <= DateTime.Parse(Request.Query["date2"]));
            }

            //权限
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");

            return result.ToString();
        }

        public async Task<string> Save(Sale_contract model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = User.FindFirst(ClaimTypes.Sid).Value;
                model.create_time = DateTime.Now;

                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Sale_Contract|add");

                if (authbtn)
                {
                    result = await _service.AddAsync(model);
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Sale_Contract|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Sale_contract, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _service.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    result = await _service.UpdateAsync(model);

                    //对比实体差别

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[合同]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.Contract_name;
                        logmodels.UserID = User.FindFirst(ClaimTypes.Sid).Value;
                        logmodels.UserName = User.FindFirst(ClaimTypes.Name).Value;
                        logmodels.IPStreet = HttpContext.Connection.RemoteIpAddress.ToString();
                        logmodels.EventDate = DateTime.Now;
                        logmodels.Log_Content = content;
                        
                        await _LogService.UpdateLog(logmodels);
                    }
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }

            if (result == 0)
            {
                return XHDResult.Error("操作失败，系统错误！").ToString();
            }
                        
            return XHDResult.Success(model.id).ToString();
        }

        public async Task<string> Delete(string id)
        {
            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Sale_Contract|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<Sale_contract, bool>> exp = a => a.id == id;
                var checkdata = await _service.GridAsync(exp, 1, 1);

                if (checkdata.count == 0)
                {
                    return XHDResult.Error("找不到此数据！").ToString();
                }

                result = await _service.DeleteAsync(id);

                //先存储删除的实体记录，用日志形式
                logext.getEntityText(checkdata.data[0]);

                //记录日志
                Sys_log logmodels = new Sys_log();

                logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                logmodels.EventType = "[合同]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].Contract_name;
                logmodels.UserID = User.FindFirst(ClaimTypes.Sid).Value;
                logmodels.UserName = User.FindFirst(ClaimTypes.Name).Value;
                logmodels.IPStreet = HttpContext.Connection.RemoteIpAddress.ToString();
                logmodels.EventDate = DateTime.Now;
                
                //logmodels.Log_Content = checkdata.data[0].follow_content;

                
                await _LogService.DeleteLog(logmodels);
            }
            else
            {
                return XHDResult.Error("无权限！").ToString();
            }


            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            //删除详情项
            Expression<Func<Sale_contract_atta, bool>> expatta = a => a.contract_id == id;

            await _detailservice.DeleteAsync(expatta);

            return XHDResult.Success().ToString();
        }
    }
}

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
    public class ParamsProvinceController : Controller
    {
        private readonly ILogger<ParamsProvinceController> _logger;
        private readonly ISys_Param_ProvincesService _service;
        private readonly ICRM_CustomerService _customerservice;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly ISys_Param_CityService _CityService;
        private readonly SysLogExt<Sys_Param_Provinces> logext = new SysLogExt<Sys_Param_Provinces>();  //日志

        public ParamsProvinceController(ILogger<ParamsProvinceController> logger, ISys_Param_ProvincesService service, ICRM_CustomerService customerservice, ISys_Param_CityService CityService, ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _customerservice=customerservice;
            _CityService=CityService;

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

        public async Task<string> Grid(PageView<Sys_Param_Provinces> model)
        {
            Expression<Func<Sys_Param_Provinces, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            {
                exp = exp.And(a => a.Provinces.Contains(Request.Query["T_name"]));
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "Provinces_order");

            return result.ToString();
        }

        public async Task<string> Combo(PageView<Sys_Param_Provinces> model)
        {
            Expression<Func<Sys_Param_Provinces, bool>> exp = a => 1 == 1;

            var result = await _service.GridAsync(exp, "Provinces_order");

            JArray arr = new JArray();

            foreach (Sys_Param_Provinces p in result.data)
            {
                JObject obj = new JObject();

                obj.Add("id", p.id);
                obj.Add("title", p.Provinces);
                obj.Add("text", p.Provinces);

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> Save(Sys_Param_Provinces model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_time = DateTime.Now;
                model.create_id = User.FindFirst(ClaimTypes.Sid).Value;

                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "params_provinces|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "params_provinces|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Sys_Param_Provinces, bool>> exp = a => a.id == model.id;
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
                        logmodels.EventType = "[省份]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.Provinces;
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

            return XHDResult.Success().ToString();
        }

        public async Task<string> Delete(string id)
        {
            Expression<Func<CRM_Customer, bool>> expcustomer = a => a.Provinces_id == id;

            var resultcustomer = await _customerservice.GridAsync(expcustomer, 1, 1);

            if (resultcustomer.count > 0)
            {
                return XHDResult.Error("此省份下有客户，不能删除！").ToString();
            }


            Expression<Func<Sys_Param_City, bool>> expcity = a => a.Provinces_id == id;
            var resultcity = await _CityService.GridAsync(expcity, 1, 1);

            if (resultcity.count > 0)
            {
                return XHDResult.Error("此省份下有城市，不能删除！").ToString();
            }


            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "params_provinces|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<Sys_Param_Provinces, bool>> exp = a => a.id == id;
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
                logmodels.EventType = "[省份]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].Provinces;
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

            //var result = await _service.Delete(id);

            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            return XHDResult.Success().ToString();
        }
    }
}

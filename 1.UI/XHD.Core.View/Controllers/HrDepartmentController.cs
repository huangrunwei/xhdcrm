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
    public class HrDepartmentController : Controller
    {
        private readonly ILogger<HrDepartmentController> _logger;
        private readonly Ihr_departmentService _service;
        private readonly Ihr_employeeService _employeeservice;

        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;

        private readonly SysLogExt<hr_department> logext = new SysLogExt<hr_department>();  //日志

        public HrDepartmentController(ILogger<HrDepartmentController> logger, Ihr_departmentService service, Ihr_employeeService employeeservice, ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _employeeservice = employeeservice;

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

        public async Task<string> Grid()
        {
            Expression<Func<hr_department, bool>> exp = a => true;
            var result = await _service.GridAsync(exp, "dep_order");

            //var json = JsonConvert.SerializeObject(result, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });

            return result.ToString();
        }

        /// <summary>
        /// 用于下拉
        /// </summary>
        /// <returns></returns>
        public async Task<string> Combo()
        {
            var result = await _service.Combo();

            return result.ToString();
        }

        /// <summary>
        /// 用于求上级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> ComboTree(string id)
        {
            var result = await _service.Combo(id);

            return result.ToString();
        }

        public async Task<string> Save(hr_department model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_department|add");

                if (authbtn)
                {
                    model.id = UUIDNext.Uuid.NewSequential().ToString();
                    result = await _service.AddAsync(model);
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                return result.ToString();
            }
            else
            {
                if (model.parentid == model.id)
                {
                    return XHDResult.Error("上级不能是自己！").ToString();
                }

                //还有一种情况，上级不是是自己的下级，这样会造成树的断裂
                //后端判断比较复杂，需要用到递归，暂时交前段处理

                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_department|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<hr_department, bool>> exp = a => a.id == model.id;
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
                        logmodels.EventType = "[部门]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.dep_name;
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

        public async Task<string> Del(string id)
        {
            //有下级，不能删除
            Expression<Func<hr_department, bool>> exp = a => a.parentid == id;

            var resultcategory = await _service.GridAsync(exp);

            if (resultcategory.count > 0)
            {
                return XHDResult.Error("此部门下含有下级，不能删除！").ToString();
            }

            //有员工
            Expression<Func<hr_employee, bool>> expemp = a => a.dep_id == id;

            var resultroleemp = await _employeeservice.GridAsync(expemp, 1, 1);

            if (resultroleemp.count > 0)
            {
                return XHDResult.Error("此部门下有员工，不能删除！").ToString();
            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "sys_role|del");

            if (authbtn)
            {
                //判断是否有数据
                exp = a => a.id == id;
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
                logmodels.EventType = "[部门]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].dep_name;
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

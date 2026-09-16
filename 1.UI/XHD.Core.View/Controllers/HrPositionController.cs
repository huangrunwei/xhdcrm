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
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class HrPositionController : Controller
    {
        private readonly ILogger<HrPositionController> _logger;
        private readonly Ihr_positionService _service;
        private readonly Ihr_employeeService _employeeservice;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<hr_position> logext = new SysLogExt<hr_position>();  //日志

        public HrPositionController(ILogger<HrPositionController> logger, Ihr_positionService service, Ihr_employeeService employeeservice, ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _employeeservice= employeeservice;
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

        public async Task<string> Grid(PageView<hr_position> model)
        {
            Expression<Func<hr_position, bool>> exp = a => true;
            var result = await _service.GridAsync(exp, model.Page, model.Limit, "position_level");

            return result.ToString();
        }

        public async Task<string> Combo()
        {
            Expression<Func<hr_position, bool>> exp = a => true;
            var result = await _service.GridAsync(exp, "position_level");

            JArray arr = new JArray();

            foreach (hr_position p in result.data)
            {
                JObject obj = new JObject();

                obj.Add("id", p.id);
                obj.Add("text", p.position_name);

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> Save(hr_position model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_post|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_post|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<hr_position, bool>> exp = a => a.id == model.id;
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
                        logmodels.EventType = "[职务]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.position_name;
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
            //有员工
            Expression<Func<hr_employee, bool>> expemp = a => a.position_id == id;

            var resultroleemp = await _employeeservice.GridAsync(expemp, 1, 1);

            if (resultroleemp.count > 0)
            {
                return XHDResult.Error("此职务下有员工，不能删除！").ToString();
            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_post|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<hr_position, bool>> exp = a => a.id == id;
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
                logmodels.EventType = "[职务]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].position_name;
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

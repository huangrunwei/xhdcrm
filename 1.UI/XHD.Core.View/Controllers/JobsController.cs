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
    public class JobsController : Controller
    {
        private readonly IJobsService _service;

        public JobsController(IJobsService service)
        {
            _service = service;
        }

        //管理页
        public IActionResult Index()
        {
            return View();
        }

        //编辑
        public IActionResult Add()
        {
            return View();
        }

        public async Task<string> Grid(PageView<Jobs> model)
        {
            Expression<Func<Jobs, bool>> exp = a => a.create_id == User.FindFirst(ClaimTypes.Sid).Value;

            if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            {
                exp = exp.And(a => a.job_title.Contains(Request.Query["T_name"]));
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "create_time desc");

            return result.ToString();
        }

        public async Task<string> Save(Jobs model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_time = DateTime.Now;
                model.create_id = User.FindFirst(ClaimTypes.Sid).Value;

                result = await _service.AddAsync(model);

                return XHDResult.Error("无权限！").ToString();

            }
            else
            {

                //日志
                Expression<Func<Jobs, bool>> exp = a => a.id == model.id;
                var checknulldata = await _service.GridAsync(exp, 1, 1);

                if (checknulldata.count == 0)
                {
                    return XHDResult.Error("找不到数据！").ToString();
                }

                result = await _service.UpdateAsync(model);




                if (result == 0)
                {
                    return XHDResult.Error("操作失败，系统错误！").ToString();
                }
            }

            return XHDResult.Success().ToString();
        }

        public async Task<string> Delete(string id)
        {
            var result = 0;

            //判断是否有数据
            Expression<Func<Jobs, bool>> exp = a => a.id == id;
            var checkdata = await _service.GridAsync(exp, 1, 1);

            if (checkdata.count == 0)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            result = await _service.DeleteAsync(id);

            //var result = await _service.Delete(id);

            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            return XHDResult.Success().ToString();
        }
    }
}

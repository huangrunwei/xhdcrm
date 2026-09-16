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
using System.Linq.Expressions;
using XHD.Core.Models;
using XHD.Core.View.Configs;
using Microsoft.Extensions.Configuration;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISys_MenuService _service;
        private readonly IFreeSql _fsql;
        private readonly ICRM_CustomerService _Customerservice;
        private readonly ISys_infoService _infoservice;

        public HomeController(ILogger<HomeController> logger, ISys_MenuService service, IFreeSql fsql, ICRM_CustomerService Customerservice, ISys_infoService infoservice)
        {
            _service = service;
            _logger = logger;
            _fsql = fsql;
            _Customerservice = Customerservice;
            _infoservice = infoservice;

            ////检查是否已配置
            //IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            //IConfigurationRoot root = builder.Build();

            //var isConfig = root["isConfig"];

            //if (!isConfig.ToString().Equals("1"))
            //{
            //    Response.Redirect("/SysConfig/index");
            //}
        }

        public IActionResult Index()
        {
            // 检查是否已配置
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfigurationRoot root = builder.Build();

            

            ViewData["user_name"] = User.FindFirst(ClaimTypes.Name).Value;

            var infodata = _infoservice.Grid(a => a.sys_key== "sys_name");

            if (infodata.count == 0)
            {
                return Redirect("/Account/index");
            }

            ViewData["company"] = infodata.data[0].sys_value;

            return View();


            //return View();
        }

        public IActionResult home()
        {
            //按理说从架构方面，这里不应该出现直接从数据库查询的语句和代码
            //因为非常简单，就1句代码的问题，所以我也不去从仓储和业务层写了。
            //如果是复杂代码，为了保证架构的统一性，还是建议写到仓储层和业务层。
            _fsql.Select<CRM_Customer>().Where(a => 1 == 1).Count(out var cuscount).Page(1, 1);
            _fsql.Select<CRM_follow>().Where(a => 1 == 1).Count(out var followcount).Page(1, 1);
            _fsql.Select<Sale_order>().Where(a => 1 == 1).Count(out var ordercount).Page(1, 1);
            _fsql.Select<Sale_contract>().Where(a => 1 == 1).Count(out var contractcount).Page(1, 1);
            _fsql.Select<Finance_Receive>().Where(a => 1 == 1).Count(out var receivecount).Page(1, 1);

            ViewData["cuscount"] = cuscount;
            ViewData["followcount"] = followcount;
            ViewData["ordercount"] = ordercount;
            ViewData["contractcount"] = contractcount;
            ViewData["receivecount"] = receivecount;

            return View();
        }

        [HttpGet]
        public async Task<string> iniUrl()
        {
            try
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var emp_id = claimIdentity.FindFirst(ClaimTypes.Sid).Value;
                var emp_name = claimIdentity.FindFirst(ClaimTypes.Name).Value;
                var uid = claimIdentity.FindFirst("uid").Value;

                JObject objinfo = new JObject();

                objinfo.Add("homeInfo", new JObject{
                    { "title","首页"},
                    { "href","/Home/home"},
                });

                Expression<Func<Sys_info, bool>> exp = a => 1 == 1;
                var result = await _infoservice.GridAsync(exp);

                Dictionary<string, string> dic = new Dictionary<string, string>();

                foreach (var item in result.data)
                {
                    dic[item.sys_key] = item.sys_value;
                }

                var sys_name = dic["sys_name"];
                if (sys_name.Length > 6)
                {
                    sys_name = sys_name.Substring(0, 6);
                }

                var sys_logo = dic["sys_logo"];

                objinfo.Add("logoInfo", new JObject{
                    { "title",sys_name},
                    { "image", sys_logo },
                    { "href",""}
                    });


                //objinfo.Add("logoInfo", new JObject{
                //    { "title",sys_name},
                //    { "image", "/layuimini/images/logo.png" },
                //    { "href",""}
                //    });




                //开始构建目录树
                Expression<Func<Sys_Menu, bool>> expWhere = a => 1 == 1;

                //如果不是admin管理员，则需要查找权限
                if (uid != "admin")
                {
                    List<string> menulist = await _service.GetMenuByEmpID(emp_id);

                    expWhere = expWhere.And(a => menulist.Contains(a.id));
                }

                var menujson = await _service.Tree(expWhere, false);

                objinfo.Add("menuInfo", menujson);

                return objinfo.ToString();
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("System_", ex.ToString());

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return XHDResult.Error("系统错误！").ToString();
            }
        }
    }
}
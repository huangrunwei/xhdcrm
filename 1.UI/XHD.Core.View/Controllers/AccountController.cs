using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.Common.DEncrypt;
using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    public class AccountController : Controller
    {
        private readonly string CaptchaCodeSessionName = "CaptchaCode";
        private readonly Ihr_employeeService _service;
        private readonly ISys_MenuService _menuService;
        private readonly ISys_ButtonService _buttonService;
        private readonly ISys_Param_ProvincesService _provincesService;
        private readonly ISys_Param_CityService _cityService;
        private readonly ISys_Param_TypeService _typeService;
        private readonly ISys_logService _LogService;
        private readonly ISys_infoService _infoservice;
        public AccountController(
            Ihr_employeeService service,
            ISys_MenuService menuService,
            ISys_ButtonService buttonService,
            ISys_Param_ProvincesService _provincesService,
            ISys_Param_CityService _cityService,
            ISys_Param_TypeService typeService,
            ISys_logService LogService,
            ISys_infoService infoservice)
        {
            _service = service;
            _LogService = LogService;
            _infoservice = infoservice;
            _menuService = menuService;
            _buttonService = buttonService;
            this._provincesService = _provincesService;
            this._cityService = _cityService;
            this._typeService = typeService;
        }
        public async Task<IActionResult> Index()
        {
            // 1. 系统信息
            var sysInfoData = _infoservice.Grid(a => true);
            if (sysInfoData.data.Count == 0)
            {
                var sysInfoList = await LoadInitDataFromJsonFileAsync<Sys_info>("SysInfo.json");
                if (sysInfoList != null && sysInfoList.Any())
                {
                    await _infoservice.AddAsync(sysInfoList);
                    ViewData["company"] = sysInfoList.FirstOrDefault(i => i.sys_key == "sys_name")?.sys_value;
                }
            }
            else
            {
                var company = sysInfoData.data.FirstOrDefault(a => a.sys_key == "sys_name");
                ViewData["company"] = company?.sys_value;
            }
            // 2. 员工
            var employeeData = await _service.GridAsync(a => true);
            if (employeeData.data.Count == 0)
            {
                var employees = await LoadInitDataFromJsonFileAsync<hr_employee>("HrEmployees.json");
                if (employees != null && employees.Any())
                    await _service.AddAsync(employees);
            }
            // 3. 菜单
            var menuData = await _menuService.GridAsync(a => true);
            if (menuData.data.Count == 0)
            {
                var menus = await LoadInitDataFromJsonFileAsync<Sys_Menu>("SysMenus.json");
                if (menus != null && menus.Any())
                    await _menuService.AddAsync(menus);
            }
            // 4. 按钮
            var buttonData = await _buttonService.GridAsync(a => true);
            if (buttonData.count == 0)
            {
                var buttons = await LoadInitDataFromJsonFileAsync<Sys_Button>("SysButtons.json");
                if (buttons != null && buttons.Any())
                    await _buttonService.AddAsync(buttons);
            }
            // 5. 省份
            var provinceData = await _provincesService.GridAsync(a => true);
            if (provinceData.count == 0)
            {
                var provinces = await LoadInitDataFromJsonFileAsync<Sys_Param_Provinces>("SysParamProvinces.json");
                if (provinces != null && provinces.Any())
                    await _provincesService.AddAsync(provinces);
            }
            // 6. 城市
            var cityData = await _cityService.GridAsync(a => true);
            if (cityData.count == 0)
            {
                var cities = await LoadInitDataFromJsonFileAsync<Sys_Param_City>("SysParamCities.json");
                if (cities != null && cities.Any())
                    await _cityService.AddAsync(cities);
            }
            // 7. 参数类型
            var typeData = await _typeService.GridAsync(a => true);
            if (typeData.count == 0)
            {
                var types = await LoadInitDataFromJsonFileAsync<Sys_Param_Type>("SysParamTypes.json");
                if (types != null && types.Any())
                    await _typeService.AddAsync(types);
            }
            // 8. 生成 AES Key（保持不变）
            string code = RandomNum(16);
            HttpContext.Session.SetString("AES_Key", code);
            ViewBag.AES_Key = code;
            return View();
        }
        /// <summary>
        /// 【改造后】读取初始化json数组文件，兼容AOT裁剪，使用Newtonsoft反序列化
        /// 上层调用完全不变 await LoadInitDataFromJsonFileAsync<T>("xxx.json")
        /// </summary>
        private async Task<List<T>> LoadInitDataFromJsonFileAsync<T>(string fileName)
        {
            var jsonPath = System.IO.Path.Combine(AppContext.BaseDirectory, fileName);
            if (!System.IO.File.Exists(jsonPath))
            {
                jsonPath = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "/ConfigData", fileName);
                if (!System.IO.File.Exists(jsonPath))
                    throw new FileNotFoundException($"未找到初始化文件：{fileName}");
            }
            var jsonContent = await System.IO.File.ReadAllTextAsync(jsonPath);
            // 不再使用 System.Text.Json.Deserialize<List<T>> （AOT裁剪会抛反射异常）
            // 使用 Newtonsoft.Json 反序列化数组
            var list = JsonConvert.DeserializeObject<List<T>>(jsonContent);
            return list ?? new List<T>();
        }

        /// <summary>
        /// 验证码【SVG版本，替换原来的CreateImageAsync】
        /// </summary>
        /// <returns>SVG矢量图</returns>
        public IActionResult ValiCode()
        {
            int width = 120;
            int height = 42;
            Random random = new Random(Guid.NewGuid().GetHashCode());
            string code = RandomNum(4);
            //存入Session，原有逻辑不变
            HttpContext.Session.SetString(CaptchaCodeSessionName, code);

            StringBuilder sb = new StringBuilder();
            sb.Append($@"<svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg'>
  <rect width='{width}' height='{height}' fill='#ffffff'/>");

            //绘制4条干扰线，对应原来的曲线干扰
            for (int i = 0; i < 4; i++)
            {
                int x1 = random.Next(width);
                int y1 = random.Next(height);
                int x2 = random.Next(width);
                int y2 = random.Next(height);
                int r = random.Next(60, 160);
                int g = random.Next(60, 160);
                int b = random.Next(60, 160);
                sb.Append($"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='rgb({r},{g},{b})' stroke-width='0.7'/>");
            }

            //绘制字符，带旋转偏移
            for (int i = 0; i < code.Length; i++)
            {
                int r = random.Next(0, 255);
                int g = random.Next(0, 255);
                int b = random.Next(0, 255);
                float x = 10 + i * 28;
                float y = random.Next(2, 6);
                int rotate = random.Next(-10, 10);
                sb.Append($"<text x='{x}' y='{y + 30}' fill='rgb({r},{g},{b})' font-size='28'  transform='rotate({rotate},{x},{y + 30})'>{code[i]}</text>");
            }
            sb.Append("</svg>");

            Response.ContentType = "image/svg+xml";
            return Content(sb.ToString());
        }

        public async Task<string> Login(hr_employee model)
        {
            try
            {
                var valicode_session = HttpContext.Session.GetString(CaptchaCodeSessionName);
                if (valicode_session.ToLower() != Request.Form["valicode"].ToString().ToLower())
                {
                    return XHDResult.Error("验证码错误！").ToString();
                }
            }
            catch
            {
                return XHDResult.Error(-9, "验证码已过期！").ToString();
            }
            var AES_Key = HttpContext.Session.GetString("AES_Key");
            try
            {
                var depwd = AESEncrypt.AesDecrypt(model.pwd, AES_Key);
                model.pwd = depwd;
            }
            catch
            {
                return XHDResult.Error(-9, "系统错误！").ToString();
            }
            var result = await _service.Login(model);
            if (result.Value<int>("code") == 0)
            {
                //验证通过，存储coockies
                var list = result.Value<JArray>("data");
                var employee = list[0];
                var claims = new List<Claim>
                {
                    new Claim (ClaimTypes.Sid,employee.Value<string>("id")),        //id
                    new Claim (ClaimTypes.Name,employee.Value<string>("name")),     //用户名
                    new Claim ("uid",employee.Value<string>("uid"))                 //登陆账号
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                //日志
                await _LogService.LoginLog(employee.Value<string>("id"), employee.Value<string>("name"), HttpContext.Connection.RemoteIpAddress?.ToString());
                return XHDResult.Success().ToString();
            }
            else
            {
                return result.ToString();
            }
        }
        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public string SignOut()
        {
            _ = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return XHDResult.Success().ToString();
        }
        #region 验证码工具方法
        //生成随机验证码字符串——4位（字母数字，和你原来完全一致）
        private string RandomNum(int int_NumberLength)
        {
            char[] chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz".ToCharArray();
            Random random = new Random(Guid.NewGuid().GetHashCode());
            string validateCode = string.Empty;
            for (int i = 0; i < int_NumberLength; i++)
                validateCode += chars[random.Next(0, chars.Length)].ToString();
            return validateCode;
        }
        #endregion
    }
}

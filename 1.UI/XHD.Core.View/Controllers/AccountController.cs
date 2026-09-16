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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
        /// 验证码
        /// </summary>
        /// <returns>文件流</returns>
        public async Task<IActionResult> ValiCode()
        {
            byte[] buf = await this.CreateImageAsync();
            return File(buf, "image/png");
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
        #region 生成验证码
        //生成随机验证码数字字符串——4位数
        private string RandomNum(int int_NumberLength)
        {
            char[] chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz".ToCharArray();
            Random random = new Random();
            string validateCode = string.Empty;
            for (int i = 0; i < int_NumberLength; i++)
                validateCode += chars[random.Next(0, chars.Length)].ToString();
            return validateCode;
        }
        //生成随机点
        private int[] RandomPoint()
        {
            int[] intArray = new int[6];
            for (int i = 0; i < 6; i += 2)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                switch (i)
                {
                    case 0:
                        intArray[i] = random.Next(0, 10);
                        break;
                    case 2:
                        intArray[i] = random.Next(45, 55);
                        break;
                    case 4:
                        intArray[i] = random.Next(90, 100);
                        break;
                }
            }
            for (int i = 1; i < 6; i += 2)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                intArray[i] = random.Next(0, 42);
            }
            return intArray;
        }
        private async Task<byte[]> CreateImageAsync()
        {
            int width = 120;
            int height = 42;
            Random random = new Random();
            string code = RandomNum(4);
            // 将验证码存入session
            HttpContext.Session.SetString(CaptchaCodeSessionName, code);
            using (var image = new Image<Rgba32>(width, height))
            {
                // 背景色
                image.Mutate(ctx => ctx.BackgroundColor(Color.White));
                // 画曲线 - 修正版本
                var pen = Pens.Solid(Color.Black, 0.2f);
                for (int i = 0; i < 4; i++)
                {
                    int[] points = RandomPoint();
                    var path = new PathBuilder()
                        .AddLine(points[0], points[1], points[2], points[3])
                        .AddLine(points[2], points[3], points[4], points[5])
                        .Build();
                    image.Mutate(ctx => ctx.Draw(pen, path));
                }
                // 加载字体（需要准备字体文件）
                var fontCollection = new FontCollection();
                // 获取系统默认字体
                var fontFamily = SystemFonts.Families.FirstOrDefault();
                // 绘制验证码字符
                for (int i = 0; i < code.Length; i++)
                {
                    var font = fontFamily.CreateFont(28, FontStyle.Bold);
                    var brush = new SolidBrush(GetRandomColor());
                    int y = (i + 1) % 2 == 0 ? 2 : 4;
                    image.Mutate(ctx => ctx.DrawText(
                        code[i].ToString(),
                        font,
                        brush,
                        new PointF(i * 30, y + 10)));
                }
                // 转换为byte数组
                using (var ms = new MemoryStream())
                {
                    await image.SaveAsync(ms, new PngEncoder());
                    return ms.ToArray();
                }
            }
        }
        private Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);
            int int_Red = RandomNum_First.Next(210);
            int int_Green = RandomNum_Sencond.Next(180);
            int int_Blue = (int_Red + int_Green > 300) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;
            return Color.FromRgb((byte)int_Red, (byte)int_Green, (byte)int_Blue);
        }
        #endregion
    }
}

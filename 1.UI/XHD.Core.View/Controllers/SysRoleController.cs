using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SysRoleController : Controller
    {
        private readonly ILogger<SysRoleController> _logger;
        private readonly ISys_roleService _service;
        private readonly Ihr_employeeService _employeeService;
        private readonly ISys_logService _logService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sys_role> _logExt = new();

        public SysRoleController(
            ILogger<SysRoleController> logger,
            Ihr_employeeService roleEmpService,
            ISys_roleService service,
            ISys_logService logService,
            IDBAuthService dBAuthService)
        {
            _logger = logger;
            _employeeService = roleEmpService;
            _service = service;
            _logService = logService;
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

        public IActionResult Auth()
        {
            return View();
        }

        public async Task<string> Grid(PageView<Sys_role> model)
        {
            var result = await _service.GridAsync(r => true, model.Page, model.Limit, "RoleSort");
            return result.ToString();
        }

        public async Task<string> Save(Sys_role model)
        {
            if (string.IsNullOrWhiteSpace(model.id))
            {
                // 新增
                if (!await _dBAuthService.GetAuth(GetUserId(), "sys_role|add"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                model.id = UUIDNext.Uuid.NewSequential().ToString();
                var result = await _service.AddAsync(model);
                if (result == 0)
                {
                    return XHDResult.Error("操作失败，系统错误！").ToString();
                }
            }
            else
            {
                // 编辑
                if (!await _dBAuthService.GetAuth(GetUserId(), "sys_role|edit"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                var old = (await _service.GridAsync(r => r.id == model.id, 1, 1)).data.FirstOrDefault();
                if (old == null)
                {
                    return XHDResult.Error("找不到数据！").ToString();
                }

                var result = await _service.UpdateAsync(model);
                if (result == 0)
                {
                    return XHDResult.Error("操作失败，系统错误！").ToString();
                }

                var logContent = _logExt.LogContent(old, model);
                if (!string.IsNullOrEmpty(logContent))
                {
                    await _logService.UpdateLog(new Sys_log
                    {
                        id = Guid.NewGuid().ToString(),
                        EventType = "[角色]修改",
                        EventID = model.id,
                        EventTitle = model.RoleName,
                        UserID = GetUserId(),
                        UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                        IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        EventDate = DateTime.Now,
                        Log_Content = logContent
                    });
                }
            }

            return XHDResult.Success().ToString();
        }

        public async Task<string> Del(string id)
        {
            // 检查角色下是否有员工
            if ((await _employeeService.GridAsync(re => re.role_id == id, 1, 1)).count > 0)
            {
                return XHDResult.Error("此角色下有员工，不能删除！").ToString();
            }

            if (!await _dBAuthService.GetAuth(GetUserId(), "sys_role|del"))
            {
                return XHDResult.Error("无权限！").ToString();
            }

            var role = (await _service.GridAsync(r => r.id == id, 1, 1)).data.FirstOrDefault();
            if (role == null)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            var result = await _service.DeleteAsync(id);
            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            _logExt.getEntityText(role);
            await _logService.DeleteLog(new Sys_log
            {
                id = Guid.NewGuid().ToString(),
                EventType = "[角色]删除",
                EventID = id,
                EventTitle = role.RoleName,
                UserID = GetUserId(),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now
            });

            return XHDResult.Success().ToString();
        }

        public async Task<string> Combo()
        {
            var roles = (await _service.GridAsync(r => true, "RoleSort")).data;
            var arr = new JArray(roles.Select(r => new JObject { ["id"] = r.id, ["text"] = r.RoleName }));
            return arr.ToString();
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.Sid)?.Value;
        }
    }
}
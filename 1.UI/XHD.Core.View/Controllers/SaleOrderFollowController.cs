using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

using Newtonsoft.Json;

using XHD.Core.Common;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    /// <summary>
    /// 订单售后跟进
    /// </summary>
    [Authorize]
    public class SaleOrderFollowController : Controller
    {
        private readonly ILogger<SaleOrderFollowController> _logger;
        private readonly ISale_order_followService _service;
        private readonly ISale_orderService _orderService;
        private readonly ISys_logService _logService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sale_order_follow> _logExt = new SysLogExt<Sale_order_follow>();

        public SaleOrderFollowController(
            ILogger<SaleOrderFollowController> logger,
            ISale_order_followService service,
            ISale_orderService orderService,
            ISys_logService logService,
            IDBAuthService dBAuthService)
        {
            _logger = logger;
            _service = service;
            _orderService = orderService;
            _logService = logService;
            _dBAuthService = dBAuthService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Add(string id, string order_id)
        {
            Sale_order_follow follow = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                follow = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            }

            var oid = follow?.order_id ?? order_id;
            Sale_order order = null;
            if (!string.IsNullOrWhiteSpace(oid))
            {
                if (!await CanAccessOrderAsync(oid))
                {
                    return Forbid();
                }
                order = (await _orderService.GridAsync(a => a.id == oid, 1, 1)).data.FirstOrDefault();
            }

            ViewData["follow"] = follow;
            ViewData["order"] = order;
            return View();
        }

        public async Task<string> Grid(PageView<Sale_order_follow> model)
        {
            Expression<Func<Sale_order_follow, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["order_id"]))
            {
                exp = exp.And(a => a.order_id == Request.Query["order_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["cus_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                exp = exp.And(a => a.employee_id == Request.Query["emp_id"]);
            }

            if (PageValidate.IsNumber(Request.Query["service_status"]))
            {
                var status = int.Parse(Request.Query["service_status"]);
                exp = exp.And(a => a.service_status == status);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                var keyword = Request.Query["keyword"].ToString();
                exp = exp.And(a => a.follow_content.Contains(keyword) || a.Order.sn.Contains(keyword));
            }

            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(a => a.follow_time >= DateTime.Parse(Request.Query["date1"]));
            }

            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(a => a.follow_time <= DateTime.Parse(Request.Query["date2"]));
            }

            //数据权限（跟随后订单所属客户）
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);
            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.follow_time desc");
            return result.ToString();
        }

        public async Task<string> Save(Sale_order_follow model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.order_id))
            {
                return XHDResult.Error("请选择订单！").ToString();
            }

            if (!await CanAccessOrderAsync(model.order_id))
            {
                return XHDResult.Error("无权操作该订单！").ToString();
            }

            if (string.IsNullOrWhiteSpace(model.id))
            {
                if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_Follow|add"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                var order = (await _orderService.GridAsync(a => a.id == model.order_id, 1, 1)).data.FirstOrDefault();
                if (order == null)
                {
                    return XHDResult.Error("订单不存在！").ToString();
                }

                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.customer_id = order.customer_id;
                model.employee_id = GetUserId();
                model.follow_time = model.follow_time ?? DateTime.Now;
                model.create_time = DateTime.Now;
                model.service_status = model.service_status ?? 0;

                result = await _service.AddAsync(model);
            }
            else
            {
                if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_Follow|edit"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                var old = (await _service.GridAsync(a => a.id == model.id, 1, 1)).data.FirstOrDefault();
                if (old == null)
                {
                    return XHDResult.Error("找不到数据！").ToString();
                }

                if (!await CanAccessOrderAsync(old.order_id))
                {
                    return XHDResult.Error("无权操作该跟进记录！").ToString();
                }

                if (string.IsNullOrWhiteSpace(model.order_id)) model.order_id = old.order_id;
                if (string.IsNullOrWhiteSpace(model.customer_id)) model.customer_id = old.customer_id;
                if (string.IsNullOrWhiteSpace(model.employee_id)) model.employee_id = old.employee_id;
                if (!model.follow_time.HasValue) model.follow_time = old.follow_time;

                result = await _service.UpdateAsync(model);

                var content = _logExt.LogContent(old, model);
                if (!string.IsNullOrEmpty(content))
                {
                    await _logService.UpdateLog(new Sys_log
                    {
                        id = UUIDNext.Uuid.NewSequential().ToString(),
                        EventType = "[订单售后]修改",
                        EventID = model.id,
                        EventTitle = model.service_type,
                        UserID = GetUserId(),
                        UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                        IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        EventDate = DateTime.Now,
                        Log_Content = content
                    });
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
            if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_Follow|del"))
            {
                return XHDResult.Error("无权限！").ToString();
            }

            var follow = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            if (follow == null)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            if (!await CanAccessOrderAsync(follow.order_id))
            {
                return XHDResult.Error("无权操作该跟进记录！").ToString();
            }

            var result = await _service.DeleteAsync(id);
            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            await _logService.DeleteLog(new Sys_log
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                EventType = "[订单售后]删除",
                EventID = id,
                EventTitle = follow.service_type,
                UserID = GetUserId(),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now
            });

            return XHDResult.Success().ToString();
        }

        private async Task<bool> CanAccessOrderAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return false;
            }

            var order = (await _orderService.GridAsync(a => a.id == orderId, 1, 1)).data.FirstOrDefault();
            if (order == null)
            {
                return false;
            }

            var roledata = await _dBAuthService.GetDataAuth(GetUserId());
            if (roledata.authtype == 4)
            {
                return true;
            }

            if (roledata.authtype == 0)
            {
                return false;
            }

            return (roledata.empList ?? new List<string>()).Contains(order.emp_id);
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.Sid)?.Value;
        }
    }
}

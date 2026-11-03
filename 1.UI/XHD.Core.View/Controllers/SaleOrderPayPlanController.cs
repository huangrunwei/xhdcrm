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
using Newtonsoft.Json.Linq;

using FreeSql;

using XHD.Core.Common;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.Services;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    /// <summary>
    /// 订单收款计划（含收款单）
    /// </summary>
    [Authorize]
    public class SaleOrderPayPlanController : Controller
    {
        private readonly ILogger<SaleOrderPayPlanController> _logger;
        private readonly ISale_order_pay_planService _service;
        private readonly ISale_orderService _orderService;
        private readonly IFinance_ReceiveService _receiveService;
        private readonly ICalendarSyncService _calendarSyncService;
        private readonly ISys_logService _logService;
        private readonly IDBAuthService _dBAuthService;
        private readonly ISys_infoService _infoService;
        private readonly IFreeSql _fsql;

        public SaleOrderPayPlanController(
            ILogger<SaleOrderPayPlanController> logger,
            ISale_order_pay_planService service,
            ISale_orderService orderService,
            IFinance_ReceiveService receiveService,
            ICalendarSyncService calendarSyncService,
            ISys_logService logService,
            IDBAuthService dBAuthService,
            ISys_infoService infoService,
            IFreeSql fsql)
        {
            _logger = logger;
            _service = service;
            _orderService = orderService;
            _receiveService = receiveService;
            _calendarSyncService = calendarSyncService;
            _logService = logService;
            _dBAuthService = dBAuthService;
            _infoService = infoService;
            _fsql = fsql;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Add(string id, string order_id)
        {
            Sale_order_pay_plan plan = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                plan = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            }

            var oid = plan?.order_id ?? order_id;
            Sale_order order = null;
            if (!string.IsNullOrWhiteSpace(oid))
            {
                if (!await CanAccessOrderAsync(oid))
                {
                    return Forbid();
                }
                order = (await _orderService.GridAsync(a => a.id == oid, 1, 1)).data.FirstOrDefault();
            }

            ViewData["plan"] = plan;
            ViewData["order"] = order;
            return View();
        }

        /// <summary>
        /// 收款单（收款计划详情）
        /// </summary>
        public async Task<IActionResult> Receipt(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Content("缺少收款计划参数");
            }

            var plan = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            if (plan == null)
            {
                return Content("收款计划不存在或已删除");
            }

            if (!await CanAccessOrderAsync(plan.order_id))
            {
                return Forbid();
            }

            var receipts = (await _receiveService.GridAsync(a => a.pay_plan_id == id, "a.Receive_date desc")).data;
            var paidAmount = receipts.Sum(a => a.Receive_amount ?? 0m);

            var companyName = _infoService.Grid(a => a.sys_key == "sys_name").data.FirstOrDefault()?.sys_value;
            var companyLogo = _infoService.Grid(a => a.sys_key == "sys_logo").data.FirstOrDefault()?.sys_value;

            ViewData["plan"] = plan;
            ViewData["receipts"] = receipts;
            ViewData["paidAmount"] = paidAmount;
            ViewData["remainAmount"] = Math.Max((plan.plan_amount ?? 0m) - paidAmount, 0m);
            ViewData["amountUpper"] = MoneyToChinese.ToChinese(plan.plan_amount ?? 0m);
            ViewData["paidUpper"] = MoneyToChinese.ToChinese(paidAmount);
            ViewData["companyName"] = companyName;
            ViewData["companyLogo"] = companyLogo;
            ViewData["planStatusText"] = CalendarSyncService.PlanStatusText(plan.plan_status);

            return View();
        }

        public async Task<string> Grid(PageView<Sale_order_pay_plan> model)
        {
            var exp = await BuildQueryAsync();
            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.plan_date asc, a.create_time asc");
            return result.ToString();
        }

        /// <summary>
        /// 收款计划汇总
        /// </summary>
        public async Task<string> Summary()
        {
            var exp = await BuildQueryAsync();
            var list = (await _service.GridAsync(exp)).data;

            var total = list.Sum(a => a.plan_amount ?? 0m);
            var received = list.Sum(a => a.received_amount ?? 0m);
            var overdue = list.Count(a => a.plan_status == 3);

            var data = new
            {
                count = list.Count,
                total,
                received,
                remain = Math.Max(total - received, 0m),
                overdue,
                finished = list.Count(a => a.plan_status == 2)
            };

            return JsonConvert.SerializeObject(new { code = 0, data = new[] { data } });
        }

        private async Task<Expression<Func<Sale_order_pay_plan, bool>>> BuildQueryAsync()
        {
            Expression<Func<Sale_order_pay_plan, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["order_id"]))
            {
                exp = exp.And(a => a.order_id == Request.Query["order_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.Order.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                exp = exp.And(a => a.Order.emp_id == Request.Query["emp_id"]);
            }

            if (PageValidate.IsNumber(Request.Query["plan_status"]))
            {
                var status = int.Parse(Request.Query["plan_status"]);
                exp = exp.And(a => a.plan_status == status);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                var keyword = Request.Query["keyword"].ToString();
                exp = exp.And(a => a.plan_name.Contains(keyword) || a.Order.sn.Contains(keyword) || a.Order.customer.cus_name.Contains(keyword));
            }

            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(a => a.plan_date >= DateTime.Parse(Request.Query["date1"]));
            }

            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(a => a.plan_date <= DateTime.Parse(Request.Query["date2"]));
            }

            //数据权限
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);
            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.Order.customer.emp_id));
            }

            return exp;
        }

        public async Task<string> Save(Sale_order_pay_plan model)
        {
            int result = 0;

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
                if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_PayPlan|add"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.plan_no = $"SKJH-{DateTime.Now:yyyyMMddHHmmss}-{model.id.Split('-')[2]}";
                model.create_id = GetUserId();
                model.create_time = DateTime.Now;
                model.received_amount = 0;
                model.plan_status = 0;

                result = await _service.AddAsync(model);
            }
            else
            {
                if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_PayPlan|edit"))
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
                    return XHDResult.Error("无权操作该收款计划！").ToString();
                }

                if (string.IsNullOrWhiteSpace(model.order_id)) model.order_id = old.order_id;
                if (string.IsNullOrWhiteSpace(model.plan_no)) model.plan_no = old.plan_no;

                result = await _service.UpdateAsync(model);
            }

            if (result == 0)
            {
                return XHDResult.Error("操作失败，系统错误！").ToString();
            }

            //重算状态并同步日程
            await _service.RecalcAsync(model.id);
            await _calendarSyncService.SyncReceivePlanAsync(model.id);

            await _logService.AddAsync(new Sys_log
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                EventType = "[收款计划]保存",
                EventID = model.id,
                EventTitle = model.plan_name,
                UserID = GetUserId(),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now,
                Log_Content = $"订单：{model.order_id}，计划金额：{model.plan_amount}，计划日期：{model.plan_date:yyyy-MM-dd}"
            });

            return XHDResult.Success(model.id).ToString();
        }

        public async Task<string> Delete(string id)
        {
            if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_PayPlan|del"))
            {
                return XHDResult.Error("无权限！").ToString();
            }

            var plan = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            if (plan == null)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            if (!await CanAccessOrderAsync(plan.order_id))
            {
                return XHDResult.Error("无权操作该收款计划！").ToString();
            }

            var hasReceive = (await _receiveService.GridAsync(a => a.pay_plan_id == id, 1, 1)).count > 0;
            if (hasReceive)
            {
                return XHDResult.Error("该计划下已有收款记录，不能删除！").ToString();
            }

            var result = await _service.DeleteAsync(id);
            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            await _calendarSyncService.RemoveBySourceAsync(CalendarSyncService.SourceReceivePlan, id);

            await _logService.DeleteLog(new Sys_log
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                EventType = "[收款计划]删除",
                EventID = id,
                EventTitle = plan.plan_name,
                UserID = GetUserId(),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now
            });

            return XHDResult.Success().ToString();
        }

        /// <summary>
        /// 收款单登记收款
        /// </summary>
        public async Task<string> RegisterReceive()
        {
            if (!await _dBAuthService.GetAuth(GetUserId(), "Sale_Order_PayPlan|receive"))
            {
                return XHDResult.Error("无权限！").ToString();
            }

            var planId = Request.Form["plan_id"].ToString();
            var plan = (await _service.GridAsync(a => a.id == planId, 1, 1)).data.FirstOrDefault();
            if (plan == null)
            {
                return XHDResult.Error("收款计划不存在！").ToString();
            }

            if (!await CanAccessOrderAsync(plan.order_id))
            {
                return XHDResult.Error("无权操作该收款计划！").ToString();
            }

            if (!decimal.TryParse(Request.Form["amount"].ToString(), out var amount) || amount <= 0)
            {
                return XHDResult.Error("请输入正确的收款金额！").ToString();
            }

            var orderId = plan.order_id;
            var receiveDate = DateTime.TryParse(Request.Form["receive_date"].ToString(), out var dt) ? dt : DateTime.Today;
            var payTypeId = Request.Form["pay_type_id"].ToString();
            if (string.IsNullOrWhiteSpace(payTypeId)) payTypeId = plan.pay_type_id;

            var receive = new Finance_Receive
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                order_id = orderId,
                pay_plan_id = planId,
                Pay_type_id = payTypeId,
                Payee_id = GetUserId(),
                Receive_amount = amount,
                Receive_date = receiveDate,
                Receive_num = $"SK-{DateTime.Now:yyyyMMddHHmmssfff}",
                Remarks = Request.Form["remarks"].ToString(),
                create_id = GetUserId(),
                create_time = DateTime.Now
            };

            var result = await _receiveService.AddAsync(receive);
            if (result == 0)
            {
                return XHDResult.Error("登记收款失败！").ToString();
            }

            await _service.RecalcAsync(planId);
            await _service.RecalcOrderTotalsAsync(orderId);
            await _calendarSyncService.SyncReceivePlanAsync(planId);

            await _logService.AddAsync(new Sys_log
            {
                id = UUIDNext.Uuid.NewSequential().ToString(),
                EventType = "[收款计划]登记收款",
                EventID = planId,
                EventTitle = $"{plan.plan_name} 收款 {amount:N2} 元",
                UserID = GetUserId(),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now,
                Log_Content = $"收款单号：{receive.Receive_num}，金额：{amount:N2}，日期：{receiveDate:yyyy-MM-dd}"
            });

            return XHDResult.Success(receive.id).ToString();
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

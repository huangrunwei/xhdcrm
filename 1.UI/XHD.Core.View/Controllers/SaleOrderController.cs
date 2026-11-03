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
using FreeSql;
using XHD.Core.View.Configs;
using XHD.Core.View.Services;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SaleOrderController : Controller
    {
        private readonly ILogger<SaleOrderController> _logger;
        private readonly ISale_orderService _service;
        private readonly ISale_order_detailsService _detailservice;
        private readonly IFinance_InvoiceService _InvoiceService;
        private readonly IFinance_ReceiveService _ReceiveService;
        private readonly ISale_order_pay_planService _PayPlanService;
        private readonly ISale_order_followService _FollowService;
        private readonly ICalendarSyncService _CalendarSyncService;
        private readonly IFreeSql _fsql;

        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sale_order> logext = new SysLogExt<Sale_order>();  //日志

        public SaleOrderController(
            ILogger<SaleOrderController> logger, 
            ISale_orderService service, 
            ISale_order_detailsService detailservice,
            IFinance_InvoiceService InvoiceService,
            IFinance_ReceiveService ReceiveService,
            ISale_order_pay_planService PayPlanService,
            ISale_order_followService FollowService,
            ICalendarSyncService CalendarSyncService,
            IFreeSql fsql,
            ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _detailservice = detailservice;
            _InvoiceService = InvoiceService;
            _ReceiveService = ReceiveService;
            _PayPlanService = PayPlanService;
            _FollowService = FollowService;
            _CalendarSyncService = CalendarSyncService;
            _fsql = fsql;


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

        /// <summary>
        /// 订单详情 / 订单经营报表
        /// </summary>
        public async Task<IActionResult> Info(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Content("缺少订单参数");
            }

            var orderResult = await _service.GridAsync(a => a.id == id, 1, 1);
            var order = orderResult.data.FirstOrDefault();

            if (order == null)
            {
                return Content("订单不存在或已删除");
            }

            //数据权限
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);
            if (roledata.authtype != 4 && !roledata.empList.Contains(order.emp_id))
            {
                return Content("您没有查看该订单的权限");
            }

            var details = (await _detailservice.GridAsync(a => a.order_id == id)).data;
            var plans = (await _PayPlanService.GridAsync(a => a.order_id == id, "a.plan_date asc")).data;
            var receives = (await _ReceiveService.GridAsync(a => a.order_id == id, "a.Receive_date desc")).data;
            var invoices = (await _InvoiceService.GridAsync(a => a.order_id == id, "a.invoice_date desc")).data;
            var follows = (await _FollowService.GridAsync(a => a.order_id == id, "a.follow_time desc")).data;
            var logs = await _fsql.Select<Sys_log>()
                .Where(a => a.EventID == id)
                .OrderByDescending(a => a.EventDate)
                .ToListAsync();

            var orderAmount = order.total_amount ?? order.Order_amount ?? 0m;
            var received = receives.Sum(a => a.Receive_amount ?? 0m);
            var arrears = Math.Max(orderAmount - received, 0m);
            var invoiceAmount = invoices.Sum(a => a.invoice_amount ?? 0m);
            var planTotal = plans.Sum(a => a.plan_amount ?? 0m);
            var planReceived = plans.Sum(a => a.received_amount ?? 0m);
            var planRemain = Math.Max(planTotal - planReceived, 0m);
            var overdueCount = plans.Count(a => a.plan_status == 3);
            var rate = orderAmount > 0 ? Math.Round(received / orderAmount * 100m, 1) : 0m;
            var days = order.Order_date.HasValue ? (DateTime.Today - order.Order_date.Value.Date).Days : 0;
            var lastFollow = follows.FirstOrDefault()?.follow_time;

            var chart = new
            {
                orderAmount,
                received,
                arrears,
                invoiceAmount,
                uninvoice = Math.Max(orderAmount - invoiceAmount, 0m),
                planTotal,
                planReceived,
                planRemain,
                rate,
                overdueCount,
                followCount = follows.Count,
                planStatus = new[]
                {
                    plans.Count(a => (a.plan_status ?? 0) == 0),
                    plans.Count(a => a.plan_status == 1),
                    plans.Count(a => a.plan_status == 2),
                    plans.Count(a => a.plan_status == 3)
                },
                products = details.Select(a => new
                {
                    name = a.Product?.product_name ?? "未命名产品",
                    amount = a.amount ?? 0m,
                    quantity = a.quantity ?? 0
                })
            };

            ViewData["order"] = order;
            ViewData["details"] = details;
            ViewData["plans"] = plans;
            ViewData["receives"] = receives;
            ViewData["invoices"] = invoices;
            ViewData["follows"] = follows;
            ViewData["logs"] = logs;
            ViewData["orderAmount"] = orderAmount;
            ViewData["received"] = received;
            ViewData["arrears"] = arrears;
            ViewData["invoiceAmount"] = invoiceAmount;
            ViewData["planTotal"] = planTotal;
            ViewData["planReceived"] = planReceived;
            ViewData["planRemain"] = planRemain;
            ViewData["overdueCount"] = overdueCount;
            ViewData["rate"] = rate;
            ViewData["days"] = days;
            ViewData["lastFollow"] = lastFollow;
            ViewData["chartJson"] = JsonConvert.SerializeObject(chart, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });

            return View();
        }

        public async Task<string> Grid(PageView<Sale_order> model)
        {
            Expression<Func<Sale_order, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["cus_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                exp = exp.And(a => a.emp_id == Request.Query["emp_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["Order_status_id"]))
            {
                exp = exp.And(a => a.Order_status_id == Request.Query["Order_status_id"]);
            }

            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(a => a.Order_date >= DateTime.Parse(Request.Query["date1"]));
            }

            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(a => a.Order_date <= DateTime.Parse(Request.Query["date2"]));
            }

            //权限
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");

            return result.ToString();
        }

        public async Task<string> Save(Sale_order model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                var ids = model.id.Split("-");
                var sn = $"OR-{DateTime.Now.ToString("yyyyMMddHHmmss")}-{ids[2]}";
                model.sn = sn;
                //model.sn = "DD-" + DateTime.Now.ToString("yyyy-MM-dd-") + DateTime.Now.GetHashCode().ToString().Replace("-", "");
                model.create_id = User.FindFirst(ClaimTypes.Sid).Value;
                model.create_time = DateTime.Now;

                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Sale_Order|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid)?.Value, "Sale_Order|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Sale_order, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _service.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    result = await _service.UpdateAsync(model);
                    await _service.UpdateArrearsMoney(model.id);
                     _service.UpdateOrderInvoice(model.id);

                    //成交人变更时，同步该订单下收款计划的日历归属
                    if (checknulldata.data[0].emp_id != model.emp_id)
                    {
                        await _CalendarSyncService.ResyncReceivePlansByOrderAsync(model.id);
                    }
                    //对比实体差别

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[订单]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.sn;
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

            //更新详情
            //先删除详情
            Expression<Func<Sale_order_details, bool>> expdetails = a => a.order_id == model.id;
            await _detailservice.DeleteAsync(expdetails);

            JArray arr = JArray.Parse(Request.Form["T_data"]);

            Sale_order_details modelsdetail = new Sale_order_details();
            modelsdetail.order_id = model.id;

            foreach (JObject item in arr)
            {
                modelsdetail.product_id=item.Value<string>("product_id");
                modelsdetail.price = item.Value<decimal>("price");
                modelsdetail.quantity = item.Value<int>("quantity");
                modelsdetail.amount = item.Value<decimal>("amount");

                await _detailservice.AddAsync(modelsdetail);
            }

            await CustomerScoreService.RecalculateAsync(_fsql, model.customer_id);
            return XHDResult.Success().ToString();
        }

        public async Task<string> Delete(string id)
        {
            //判断是否有发票
            Expression<Func<Finance_Invoice, bool>> expinvoice = a => a.Order.id == id;
            var invoicelist = await _InvoiceService.GridAsync(expinvoice);

            if (invoicelist.count > 0)
            {
                return XHDResult.Error("此订单下含有发票，不能删除！").ToString();
            }


            //判断是否有收款
            Expression<Func<Finance_Receive, bool>> expreceive = a => a.Order.id == id;
            var receivelist = await _ReceiveService.GridAsync(expreceive);

            if (receivelist.count > 0)
            {
                return XHDResult.Error("此订单下含有收款，不能删除！").ToString();
            }


            var result = 0;
            var deletedCustomerId = string.Empty;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Sale_Order|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<Sale_order, bool>> exp = a => a.id == id;
                var checkdata = await _service.GridAsync(exp, 1, 1);

                if (checkdata.count == 0)
                {
                    return XHDResult.Error("找不到此数据！").ToString();
                }

                deletedCustomerId = checkdata.data[0].customer_id;
                result = await _service.DeleteAsync(id);

                //先存储删除的实体记录，用日志形式
                logext.getEntityText(checkdata.data[0]);

                //记录日志
                Sys_log logmodels = new Sys_log();

                logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                logmodels.EventType = "[订单]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].sn;
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

            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            //删除详情项
            Expression<Func<Sale_order_details, bool>> expdetails = a => a.order_id == id;
            var detailslist = await _detailservice.GridAsync(expdetails);

            await _detailservice.DeleteAsync(expdetails);

            //删除收款计划及对应日历
            await _CalendarSyncService.RemoveReceivePlansByOrderAsync(id);
            await _fsql.Delete<Sale_order_pay_plan>().Where(a => a.order_id == id).ExecuteAffrowsAsync();

            //删除订单售后跟进
            await _fsql.Delete<Sale_order_follow>().Where(a => a.order_id == id).ExecuteAffrowsAsync();

            if (!string.IsNullOrWhiteSpace(deletedCustomerId))
            {
                await CustomerScoreService.RecalculateAsync(_fsql, deletedCustomerId);
            }

            SysLogExt<Sale_order_details> logdetails = new SysLogExt<Sale_order_details>();  //日志
            foreach (Sale_order_details model in detailslist.data)
            {
                //先存储删除的实体记录，用日志形式
                logdetails.getEntityText(model);
            }

            return XHDResult.Success().ToString();
        }
    }
}

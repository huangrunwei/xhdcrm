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
using XHD.Core.View.Services;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class FinanceReceiveController : Controller
    {
        private readonly ILogger<FinanceReceiveController> _logger;
        private readonly IFinance_ReceiveService _service;
        private readonly ISale_orderService _OrderService;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly ISale_order_pay_planService _PayPlanService;
        private readonly ICalendarSyncService _CalendarSyncService;
        private readonly IFreeSql _fsql;
        private readonly SysLogExt<Finance_Receive> logext = new SysLogExt<Finance_Receive>();  //日志
        public FinanceReceiveController(ILogger<FinanceReceiveController> logger, IFinance_ReceiveService service, IFreeSql freeSql, ISale_orderService OrderService, ISys_logService LogService, IDBAuthService dBAuthService, ISale_order_pay_planService PayPlanService, ICalendarSyncService CalendarSyncService)
        {
            _service = service;
            _logger = logger;
            _fsql = freeSql;
            _LogService = LogService;
            _OrderService = OrderService;
            _PayPlanService = PayPlanService;
            _CalendarSyncService = CalendarSyncService;

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

        public async Task<string> Grid(PageView<Finance_Receive> model)
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            //if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            //{
            //    exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["T_name"]));
            //}
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(a => a.Order.customer.cus_name.Contains(Request.Query["cus_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                exp = exp.And(a => a.Payee_id == Request.Query["emp_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["order_id"]))
            {
                exp = exp.And(a => a.order_id == Request.Query["order_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.Order.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["pay_plan_id"]))
            {
                exp = exp.And(a => a.pay_plan_id == Request.Query["pay_plan_id"]);
            }

            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(a => a.Receive_date >= DateTime.Parse(Request.Query["date1"]));
            }

            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(a => a.Receive_date <= DateTime.Parse(Request.Query["date2"]));
            }

            //权限
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.Order.customer.emp_id));
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");

            return result.ToString();
        }

        public async Task<string> Save(Finance_Receive model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = User.FindFirst(ClaimTypes.Sid).Value;
                model.create_time = DateTime.Now;

                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Finance_Receive|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Finance_Receive|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Finance_Receive, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _service.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    //编辑时若表单未携带收款计划，保留原关联
                    if (string.IsNullOrWhiteSpace(model.pay_plan_id))
                    {
                        model.pay_plan_id = checknulldata.data[0].pay_plan_id;
                    }

                    result = await _service.UpdateAsync(model);

                    //对比实体差别

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[收款]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.Receive_num;
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

            //更新订单
            _OrderService.UpdateOrderReceive(model.order_id);
            await _PayPlanService.RecalcOrderTotalsAsync(model.order_id);

            //同步收款计划与日程
            if (!string.IsNullOrWhiteSpace(model.pay_plan_id))
            {
                await _PayPlanService.RecalcAsync(model.pay_plan_id);
                await _CalendarSyncService.SyncReceivePlanAsync(model.pay_plan_id);
            }

            await RecalcCustomerScoreAsync(model.order_id);
            return XHDResult.Success().ToString();
        }

        public async Task<string> Delete(string id)
        {
            //先查询信息
            Expression<Func<Finance_Receive, bool>> exp = a => a.id == id;
            var receiveInfo = await _service.GridAsync(exp);

            if (receiveInfo.count == 0)
            {
                return XHDResult.Error("找不到数据！").ToString();
            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Finance_Receive|del");

            if (authbtn)
            {
                result = await _service.DeleteAsync(id);

                //先存储删除的实体记录，用日志形式
                logext.getEntityText(receiveInfo.data[0]);

                //记录日志
                Sys_log logmodels = new Sys_log();

                logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                logmodels.EventType = "[收款]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = receiveInfo.data[0].Receive_num;
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

            //更新订单            
            _OrderService.UpdateOrderReceive(receiveInfo.data[0].order_id);
            await _PayPlanService.RecalcOrderTotalsAsync(receiveInfo.data[0].order_id);

            //同步收款计划与日程
            var planId = receiveInfo.data[0].pay_plan_id;
            if (!string.IsNullOrWhiteSpace(planId))
            {
                await _PayPlanService.RecalcAsync(planId);
                await _CalendarSyncService.SyncReceivePlanAsync(planId);
            }

            await RecalcCustomerScoreAsync(receiveInfo.data[0].order_id);
            return XHDResult.Success().ToString();
        }

        private async Task RecalcCustomerScoreAsync(string orderId)
        {
            var order = await _fsql.Select<Sale_order>().Where(a => a.id == orderId).FirstAsync();
            if (order != null)
            {
                await CustomerScoreService.RecalculateAsync(_fsql, order.customer_id);
            }
        }
    }
}

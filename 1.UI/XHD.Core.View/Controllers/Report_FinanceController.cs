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
using XHD.Core.View.Models;


namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class Report_FinanceController : ReportControllerBase
    {
        private readonly ILogger<Report_FinanceController> _logger;
        private readonly IFinance_InvoiceService _InvoiceService;
        private readonly IFinance_ReceiveService _ReceiveService;

        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sale_order> logext = new SysLogExt<Sale_order>();  //日志

        public Report_FinanceController(
            ILogger<Report_FinanceController> logger,            
            IFinance_InvoiceService InvoiceService,
            IFinance_ReceiveService ReceiveService, 
            ISys_logService LogService,
            IDBAuthService dBAuthService,
            IFreeSql fsql) : base(fsql, dBAuthService)
        {
           
            _logger = logger;
            _InvoiceService = InvoiceService;
            _ReceiveService = ReceiveService;


            _LogService = LogService;
            _dBAuthService = dBAuthService;
        }

        public async Task<IActionResult> Index()
        {
            var scope = await GetScopeAsync();
            var year = GetSelectedYear();
            var empId = GetSelectedEmp(scope);

            var allCustomers = await ReportFsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var allOrders = await ReportFsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync();
            var allReceives = await ReportFsql.Select<Finance_Receive>().Where(a => a.isDelete != 1).ToListAsync();
            var allInvoices = await ReportFsql.Select<Finance_Invoice>().Where(a => a.isDelete != 1).ToListAsync();
            var allPlans = await ReportFsql.Select<Sale_order_pay_plan>().Where(a => a.isDelete != 1).ToListAsync();
            var paramNames = await GetParamNamesAsync();

            var managedCustomerIds = allCustomers
                .Where(a => scope.Full || scope.AllowedEmpIds.Contains(a.emp_id))
                .Select(a => a.id)
                .ToHashSet();

            var orders = allOrders.Where(a => managedCustomerIds.Contains(a.customer_id)).ToList();
            if (!string.IsNullOrWhiteSpace(empId))
            {
                orders = orders.Where(a => a.emp_id == empId).ToList();
            }

            var orderIds = orders.Select(a => a.id).ToHashSet();
            var visibleReceives = allReceives.Where(a => orderIds.Contains(a.order_id)).ToList();
            var visibleInvoices = allInvoices.Where(a => orderIds.Contains(a.order_id)).ToList();
            var visiblePlans = allPlans.Where(a => orderIds.Contains(a.order_id)).ToList();

            var yearReceives = visibleReceives
                .Where(a => a.Receive_date.HasValue && a.Receive_date.Value.Year == year)
                .ToList();
            var yearInvoices = visibleInvoices
                .Where(a => a.invoice_date.HasValue && a.invoice_date.Value.Year == year)
                .ToList();

            var customerNames = allCustomers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var orderMap = orders.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var employeeNames = await GetEmployeeNamesAsync(scope);

            var receivedByOrder = visibleReceives
                .GroupBy(a => a.order_id)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Receive_amount ?? 0m));
            var totalReceivedAll = visibleReceives.Sum(a => a.Receive_amount ?? 0m);
            var totalOrderAmount = orders.Sum(OrderAmount);
            var outstanding = orders.Sum(a => Math.Max(OrderAmount(a) - receivedByOrder.GetValueOrDefault(a.id), 0m));

            var today = DateTime.Today;
            var pendingPlans = visiblePlans.Where(a => (a.plan_status ?? 0) != 2).ToList();
            var overduePlans = pendingPlans.Where(a => a.plan_date.HasValue && a.plan_date.Value.Date < today).ToList();

            var aging = new List<ReportNamedValue>
            {
                new ReportNamedValue { Name = "已逾期", Value = pendingPlans.Where(a => a.plan_date.HasValue && a.plan_date.Value.Date < today).Sum(a => a.plan_amount ?? 0m), Extra = overduePlans.Count + " 笔" },
                new ReportNamedValue { Name = "7天内", Value = pendingPlans.Where(a => a.plan_date.HasValue && a.plan_date.Value.Date >= today && a.plan_date.Value.Date <= today.AddDays(7)).Sum(a => a.plan_amount ?? 0m), Extra = pendingPlans.Count(a => a.plan_date.HasValue && a.plan_date.Value.Date >= today && a.plan_date.Value.Date <= today.AddDays(7)) + " 笔" },
                new ReportNamedValue { Name = "30天内", Value = pendingPlans.Where(a => a.plan_date.HasValue && a.plan_date.Value.Date > today.AddDays(7) && a.plan_date.Value.Date <= today.AddDays(30)).Sum(a => a.plan_amount ?? 0m), Extra = pendingPlans.Count(a => a.plan_date.HasValue && a.plan_date.Value.Date > today.AddDays(7) && a.plan_date.Value.Date <= today.AddDays(30)) + " 笔" },
                new ReportNamedValue { Name = "30天以上", Value = pendingPlans.Where(a => !a.plan_date.HasValue || a.plan_date.Value.Date > today.AddDays(30)).Sum(a => a.plan_amount ?? 0m), Extra = pendingPlans.Count(a => !a.plan_date.HasValue || a.plan_date.Value.Date > today.AddDays(30)) + " 笔" }
            };

            var model = new FinanceReportModel
            {
                Year = year,
                EmpId = empId,
                EmpName = string.IsNullOrWhiteSpace(empId)
                    ? "全部员工"
                    : employeeNames.GetValueOrDefault(empId, "未知员工"),
                Employees = BuildEmployeeOptions(scope, empId),
                Years = BuildYearOptions(year),
                Kpis = new List<ReportKpi>
                {
                    new ReportKpi { Label = "年度回款", Value = "¥" + yearReceives.Sum(a => a.Receive_amount ?? 0m).ToString("N2"), Hint = year + " 年收款金额", Icon = "fa-rmb", Tone = "green" },
                    new ReportKpi { Label = "年度开票", Value = "¥" + yearInvoices.Sum(a => a.invoice_amount ?? 0m).ToString("N2"), Hint = year + " 年开票金额", Icon = "fa-file-text-o", Tone = "blue" },
                    new ReportKpi { Label = "应收未收", Value = "¥" + outstanding.ToString("N2"), Hint = "全部订单未收余额", Icon = "fa-exclamation-circle", Tone = "red" },
                    new ReportKpi { Label = "整体回款率", Value = Percent(totalReceivedAll, totalOrderAmount), Hint = "已回款 / 订单总额", Icon = "fa-pie-chart", Tone = "cyan" },
                    new ReportKpi { Label = "待收计划", Value = pendingPlans.Count.ToString("N0"), Hint = "未完成收款计划", Icon = "fa-calendar-check-o", Tone = "orange" },
                    new ReportKpi { Label = "逾期计划", Value = overduePlans.Count.ToString("N0"), Hint = "已超过计划日期", Icon = "fa-clock-o", Tone = "purple" }
                },
                Trend = Enumerable.Range(1, 12).Select(month => new ReportTrendPoint
                {
                    Month = month + "月",
                    Count = yearReceives.Where(a => a.Receive_date.HasValue && a.Receive_date.Value.Month == month).Sum(a => a.Receive_amount ?? 0m),
                    Amount = yearInvoices.Where(a => a.invoice_date.HasValue && a.invoice_date.Value.Month == month).Sum(a => a.invoice_amount ?? 0m)
                }).ToList(),
                PayTypes = yearReceives
                    .GroupBy(a => a.Pay_type_id)
                    .Select(g => new ReportNamedValue
                    {
                        Name = ParamName(paramNames, g.Key, "未设置"),
                        Value = g.Sum(a => a.Receive_amount ?? 0m),
                        Extra = g.Count() + " 笔"
                    })
                    .OrderByDescending(a => a.Value)
                    .ToList(),
                InvoiceTypes = yearInvoices
                    .GroupBy(a => a.invoice_type_id)
                    .Select(g => new ReportNamedValue
                    {
                        Name = ParamName(paramNames, g.Key, "未设置"),
                        Value = g.Sum(a => a.invoice_amount ?? 0m),
                        Extra = g.Count() + " 张"
                    })
                    .OrderByDescending(a => a.Value)
                    .ToList(),
                Aging = aging,
                RecentReceives = yearReceives
                    .OrderByDescending(a => a.Receive_date)
                    .Take(8)
                    .Select(a =>
                    {
                        var order = orderMap.GetValueOrDefault(a.order_id);
                        var customer = order == null ? null : customerNames.GetValueOrDefault(order.customer_id);
                        return new ReportReceiveRow
                        {
                            Id = a.id,
                            Num = a.Receive_num,
                            Customer = customer?.cus_name ?? "未知客户",
                            OrderSn = order?.sn ?? "-",
                            Amount = a.Receive_amount ?? 0m,
                            Date = a.Receive_date,
                            PayType = ParamName(paramNames, a.Pay_type_id, "未设置"),
                            Payee = employeeNames.GetValueOrDefault(a.Payee_id, "-")
                        };
                    })
                    .ToList(),
                PendingPlans = pendingPlans
                    .OrderBy(a => a.plan_date)
                    .Take(8)
                    .Select(a =>
                    {
                        var order = orderMap.GetValueOrDefault(a.order_id);
                        var customer = order == null ? null : customerNames.GetValueOrDefault(order.customer_id);
                        return new ReportPlanRow
                        {
                            Id = a.id,
                            PlanName = a.plan_name,
                            OrderSn = order?.sn ?? "-",
                            Customer = customer?.cus_name ?? "未知客户",
                            Amount = a.plan_amount ?? 0m,
                            PlanDate = a.plan_date,
                            Tag = PlanTag(a.plan_status, a.plan_date),
                            TagClass = PlanTagClass(a.plan_status, a.plan_date)
                        };
                    })
                    .ToList(),
                TopCustomers = yearReceives
                    .GroupBy(a =>
                    {
                        var order = orderMap.GetValueOrDefault(a.order_id);
                        return order?.customer_id ?? "";
                    })
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                    .Select(g =>
                    {
                        var customer = customerNames.GetValueOrDefault(g.Key);
                        return new ReportCustomerRow
                        {
                            Id = g.Key,
                            Name = customer?.cus_name ?? "未知客户",
                            Owner = customer == null ? "-" : employeeNames.GetValueOrDefault(customer.emp_id, "-"),
                            Level = ParamName(paramNames, customer?.cus_level_id, "未分级"),
                            Industry = ParamName(paramNames, customer?.cus_industry_id, "未分类"),
                            OrderCount = g.Count(),
                            OrderAmount = g.Sum(a => a.Receive_amount ?? 0m),
                            Phone = customer?.cus_tel ?? ""
                        };
                    })
                    .OrderByDescending(a => a.OrderAmount)
                    .Take(6)
                    .ToList()
            };

            return View(model);
        }

        public async Task<string> ReportYear()
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Receive_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Receive_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.Payee_id));
                }
            }


            var result = await _ReceiveService.ReportYear(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<int>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportYearSum()
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Receive_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Receive_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.Payee_id));
                }
            }

            var result = await _ReceiveService.ReportYearSum(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<decimal>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportPayType()
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Receive_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Receive_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.Payee_id));
                }
            }

            var result = await _ReceiveService.ReportPayType(exp);

            JArray arr = new JArray();
            for (int i = 0; i < result.Count; i++)
            {
                JObject obj = new JObject();
                obj.Add("value", result[i].Value<int>("count"));
                obj.Add("name", result[i].Value<string>("xmonth"));

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportInvoice()
        {
            Expression<Func<Finance_Invoice, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.invoice_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.invoice_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.emp_id));
                }
            }

            var result = await _InvoiceService.ReportYear(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<int>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportInvoiceSum()
        {
            Expression<Func<Finance_Invoice, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.invoice_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.invoice_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.emp_id));
                }
            }

            var result = await _InvoiceService.ReportYearSum(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<decimal>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportInvoiceType()
        {
            Expression<Func<Finance_Invoice, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.invoice_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.invoice_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.emp_id));
                }
            }

            var result = await _InvoiceService.ReportInvoiceType(exp);

            JArray arr = new JArray();
            for (int i = 0; i < result.Count; i++)
            {
                JObject obj = new JObject();
                obj.Add("value", result[i].Value<int>("count"));
                obj.Add("name", result[i].Value<string>("xmonth"));

                arr.Add(obj);
            }

            return arr.ToString();
        }
    }
}

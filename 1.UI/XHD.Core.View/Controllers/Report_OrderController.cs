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
    public class Report_OrderController : ReportControllerBase
    {
        private readonly ILogger<Report_OrderController> _logger;
        private readonly ISale_orderService _service;
        private readonly ISale_order_detailsService _detailservice;
        private readonly IFinance_InvoiceService _InvoiceService;
        private readonly IFinance_ReceiveService _ReceiveService;

        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sale_order> logext = new SysLogExt<Sale_order>();  //日志

        public Report_OrderController(
            ILogger<Report_OrderController> logger,
            ISale_orderService service,
            ISale_order_detailsService detailservice,
            IFinance_InvoiceService InvoiceService,
            IFinance_ReceiveService ReceiveService, ISys_logService LogService, IDBAuthService dBAuthService, IFreeSql fsql)
            : base(fsql, dBAuthService)
        {
            _service = service;
            _logger = logger;
            _detailservice = detailservice;
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
            var allDetails = await ReportFsql.Select<Sale_order_details>().ToListAsync();
            var allProducts = await ReportFsql.Select<Product>().ToListAsync();
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

            var yearOrders = orders.Where(a => OrderDate(a).HasValue && OrderDate(a).Value.Year == year).ToList();
            var orderIds = yearOrders.Select(a => a.id).ToHashSet();
            var receives = allReceives.Where(a => orderIds.Contains(a.order_id)).ToList();
            var details = allDetails.Where(a => orderIds.Contains(a.order_id)).ToList();

            var customerNames = allCustomers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var employeeNames = await GetEmployeeNamesAsync(scope);
            var productNames = allProducts.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First().product_name);

            var receivedByOrder = receives
                .GroupBy(a => a.order_id)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Receive_amount ?? 0m));
            var totalReceived = receives.Sum(a => a.Receive_amount ?? 0m);
            var totalAmount = yearOrders.Sum(OrderAmount);
            var arrears = yearOrders.Sum(a => Math.Max(OrderAmount(a) - receivedByOrder.GetValueOrDefault(a.id), 0m));
            var completed = yearOrders.Count(a =>
            {
                var statusName = ParamName(paramNames, a.Order_status_id, "");
                return StatusClass(a.Order_status_id, statusName) == "success";
            });

            var statusData = yearOrders
                .GroupBy(a => a.Order_status_id)
                .Select(g => new ReportNamedValue
                {
                    Name = ParamName(paramNames, g.Key, "未设置"),
                    Value = g.Count(),
                    Extra = "¥" + g.Sum(OrderAmount).ToString("N2")
                })
                .OrderByDescending(a => a.Value)
                .ToList();

            var payTypeData = yearOrders
                .GroupBy(a => a.pay_type_id)
                .Select(g => new ReportNamedValue
                {
                    Name = ParamName(paramNames, g.Key, "未设置"),
                    Value = g.Sum(OrderAmount),
                    Extra = g.Count() + " 单"
                })
                .OrderByDescending(a => a.Value)
                .ToList();

            var totalDetailAmount = details.Sum(a => a.amount ?? 0m);
            var productData = details
                .GroupBy(a => a.product_id)
                .Select(g => new ReportNamedValue
                {
                    Name = productNames.GetValueOrDefault(g.Key, "未命名产品"),
                    Value = g.Sum(a => a.amount ?? 0m),
                    Extra = g.Sum(a => a.quantity ?? 0) + " 件 · " + Percent(g.Sum(a => a.amount ?? 0m), totalDetailAmount)
                })
                .OrderByDescending(a => a.Value)
                .Take(8)
                .ToList();

            var model = new OrderReportModel
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
                    new ReportKpi { Label = "订单数量", Value = yearOrders.Count.ToString("N0"), Hint = year + " 年订单", Icon = "fa-shopping-cart", Tone = "blue" },
                    new ReportKpi { Label = "订单金额", Value = "¥" + totalAmount.ToString("N2"), Hint = "年度合同/订单总额", Icon = "fa-money", Tone = "green" },
                    new ReportKpi { Label = "已回款", Value = "¥" + totalReceived.ToString("N2"), Hint = "年度订单累计回款", Icon = "fa-rmb", Tone = "cyan" },
                    new ReportKpi { Label = "应收余额", Value = "¥" + arrears.ToString("N2"), Hint = "年度订单未收金额", Icon = "fa-exclamation-circle", Tone = "red" },
                    new ReportKpi { Label = "平均客单价", Value = "¥" + (yearOrders.Count == 0 ? 0m : totalAmount / yearOrders.Count).ToString("N2"), Hint = "订单金额 / 订单数量", Icon = "fa-calculator", Tone = "orange" },
                    new ReportKpi { Label = "完成率", Value = Percent(completed, yearOrders.Count), Hint = completed + " / " + yearOrders.Count + " 单已完成", Icon = "fa-check-circle-o", Tone = "purple" }
                },
                Trend = Enumerable.Range(1, 12).Select(month => new ReportTrendPoint
                {
                    Month = month + "月",
                    Count = yearOrders.Count(a => OrderDate(a).HasValue && OrderDate(a).Value.Month == month),
                    Amount = yearOrders.Where(a => OrderDate(a).HasValue && OrderDate(a).Value.Month == month).Sum(OrderAmount)
                }).ToList(),
                Status = statusData,
                PayTypes = payTypeData,
                Products = productData,
                EmployeeRanking = yearOrders
                    .GroupBy(a => a.emp_id)
                    .Select(g => new ReportEmployeeRow
                    {
                        Name = employeeNames.GetValueOrDefault(g.Key, "未分配"),
                        Count = g.Count(),
                        Amount = g.Sum(OrderAmount),
                        Extra = "回款 ¥" + g.Sum(a => receivedByOrder.GetValueOrDefault(a.id)).ToString("N2")
                    })
                    .OrderByDescending(a => a.Amount)
                    .Take(6)
                    .ToList(),
                RecentOrders = yearOrders
                    .OrderByDescending(a => OrderDate(a))
                    .Take(8)
                    .Select(a =>
                    {
                        var statusName = ParamName(paramNames, a.Order_status_id, "未设置");
                        var customer = customerNames.GetValueOrDefault(a.customer_id);
                        return new ReportOrderRow
                        {
                            Id = a.id,
                            Sn = a.sn,
                            Customer = customer?.cus_name ?? "未知客户",
                            Owner = employeeNames.GetValueOrDefault(a.emp_id, "-"),
                            Status = statusName,
                            StatusClass = StatusClass(a.Order_status_id, statusName),
                            PayType = ParamName(paramNames, a.pay_type_id, "未设置"),
                            Amount = OrderAmount(a),
                            Received = receivedByOrder.GetValueOrDefault(a.id),
                            Date = OrderDate(a)
                        };
                    })
                    .ToList(),
                TopCustomers = yearOrders
                    .GroupBy(a => a.customer_id)
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
                            OrderAmount = g.Sum(OrderAmount),
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
            Expression<Func<Sale_order, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Order_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Order_date.Value.Year == DateTime.Now.Year);
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

            var result = await _service.ReportYear(exp);

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
            Expression<Func<Sale_order, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Order_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Order_date.Value.Year == DateTime.Now.Year);
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

            var result = await _service.ReportYearSum(exp);

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

        public async Task<string> ReportStatus()
        {
            Expression<Func<Sale_order, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Order_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Order_date.Value.Year == DateTime.Now.Year);
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

            var result = await _service.ReportStatus(exp);

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

        public async Task<string> ReportPayType()
        {
            Expression<Func<Sale_order, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Order_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Order_date.Value.Year == DateTime.Now.Year);
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

            var result = await _service.ReportPayType(exp);

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

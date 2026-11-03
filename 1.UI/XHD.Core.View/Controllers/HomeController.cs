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
using XHD.Core.View.Models;
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
        private readonly IDBAuthService _dBAuthService;

        public HomeController(ILogger<HomeController> logger, ISys_MenuService service, IFreeSql fsql, ICRM_CustomerService Customerservice, ISys_infoService infoservice, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _fsql = fsql;
            _Customerservice = Customerservice;
            _infoservice = infoservice;
            _dBAuthService = dBAuthService;

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

        public async Task<IActionResult> home()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allCustomers = await _fsql.Select<CRM_Customer>().ToListAsync();
            var allFollows = await _fsql.Select<CRM_follow>().ToListAsync();
            var allOrders = await _fsql.Select<Sale_order>().ToListAsync();
            var allContracts = await _fsql.Select<Sale_contract>().ToListAsync();
            var allReceives = await _fsql.Select<Finance_Receive>().ToListAsync();
            var allInvoices = await _fsql.Select<Finance_Invoice>().ToListAsync();
            var allPlans = await _fsql.Select<Sale_order_pay_plan>().ToListAsync();
            var employees = await _fsql.Select<hr_employee>().ToListAsync();
            var parameters = await _fsql.Select<Sys_Param>().ToListAsync();

            var empId = User.FindFirst(ClaimTypes.Sid)?.Value ?? string.Empty;
            var uid = User.FindFirst("uid")?.Value ?? string.Empty;
            var isAdmin = string.Equals(uid, "admin", StringComparison.OrdinalIgnoreCase);
            var allowedMenus = isAdmin ? new List<string>() : await _service.GetMenuByEmpID(empId);
            bool HasMenu(string menuId) => isAdmin || allowedMenus.Contains(menuId);

            //数据权限：0无权限、1本人、2本部门、3本部门及下级、4全部
            var roleData = await _dBAuthService.GetDataAuth(empId);
            var fullData = roleData.authtype == 4;
            var allowedEmpIds = roleData.empList ?? new List<string>();

            List<CRM_Customer> customers;
            HashSet<string> managedCustomerIds;

            if (fullData)
            {
                customers = allCustomers;
                managedCustomerIds = allCustomers.Select(a => a.id).ToHashSet();
            }
            else if (roleData.authtype == 0)
            {
                customers = new List<CRM_Customer>();
                managedCustomerIds = new HashSet<string>();
            }
            else
            {
                managedCustomerIds = allCustomers
                    .Where(a => allowedEmpIds.Contains(a.emp_id))
                    .Select(a => a.id)
                    .ToHashSet();

                //客户列表沿用客户模块规则：本人客户 + 公客
                customers = allCustomers
                    .Where(a => allowedEmpIds.Contains(a.emp_id) || a.isPrivate == 1)
                    .ToList();
            }

            var follows = fullData
                ? allFollows
                : allFollows.Where(a => managedCustomerIds.Contains(a.customer_id)).ToList();

            var orders = fullData
                ? allOrders
                : allOrders.Where(a => managedCustomerIds.Contains(a.customer_id)).ToList();

            var contracts = fullData
                ? allContracts
                : allContracts.Where(a => managedCustomerIds.Contains(a.customer_id)).ToList();

            var visibleOrderIds = orders.Select(a => a.id).ToHashSet();

            var receives = fullData
                ? allReceives
                : allReceives.Where(a => visibleOrderIds.Contains(a.order_id)).ToList();

            var invoices = fullData
                ? allInvoices
                : allInvoices.Where(a => visibleOrderIds.Contains(a.order_id)).ToList();

            var plans = fullData
                ? allPlans
                : allPlans.Where(a => visibleOrderIds.Contains(a.order_id)).ToList();

            var paramNames = parameters.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First().params_name);
            var customerNames = allCustomers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First().cus_name);
            var employeeNames = employees.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First().name);
            var orderMap = orders.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());

            decimal OrderTotal(Sale_order o) => o.total_amount ?? o.Order_amount ?? 0m;

            var orderAmount = orders.Sum(OrderTotal);
            var receiveAmount = receives.Sum(r => r.Receive_amount ?? 0m);
            var contractAmount = contracts.Sum(c => c.Contract_amount ?? 0m);
            var invoiceAmount = invoices.Sum(i => i.invoice_amount ?? 0m);
            var arrears = Math.Max(orderAmount - receiveAmount, 0m);
            var payRate = orderAmount > 0 ? Math.Round(receiveAmount / orderAmount * 100m, 1) : 0m;

            var hour = DateTime.Now.Hour;
            var greeting = hour < 6 ? "凌晨好" : hour < 9 ? "早上好" : hour < 12 ? "上午好" : hour < 14 ? "中午好" : hour < 18 ? "下午好" : "晚上好";
            var weekday = "日一二三四五六"[(int)DateTime.Now.DayOfWeek];

            var months = Enumerable.Range(0, 6).Select(i => monthStart.AddMonths(-5 + i)).ToList();

            var statusStats = orders
                .GroupBy(o => o.Order_status_id)
                .Select(g => new NameValueItem
                {
                    Name = paramNames.GetValueOrDefault(g.Key, "未设置"),
                    Count = g.Count(),
                    Amount = g.Sum(OrderTotal)
                })
                .OrderByDescending(a => a.Count)
                .Take(5)
                .ToList();

            var orderCount = orders.Count;
            foreach (var item in statusStats)
            {
                item.Percent = orderCount > 0 ? Math.Round((decimal)item.Count / orderCount * 100m, 0) : 0m;
            }

            var industryStats = customers
                .GroupBy(c => c.cus_industry_id)
                .Select(g => new NameValueItem
                {
                    Name = paramNames.GetValueOrDefault(g.Key, "未分类"),
                    Count = g.Count()
                })
                .OrderByDescending(a => a.Count)
                .Take(6)
                .ToList();

            foreach (var item in industryStats)
            {
                item.Percent = customers.Count > 0 ? Math.Round((decimal)item.Count / customers.Count * 100m, 0) : 0m;
            }

            var reminders = new List<ReminderItem>();

            if (HasMenu("Sale_Order_PayPlan"))
            {
                foreach (var p in plans
                    .Where(a => a.plan_date.HasValue && (a.plan_status ?? 0) != 2 && a.plan_date.Value.Date <= today.AddDays(30))
                    .OrderBy(a => a.plan_date)
                    .Take(6))
                {
                    var order = orderMap.GetValueOrDefault(p.order_id);
                    var days = (p.plan_date.Value.Date - today).Days;
                    var overdue = p.plan_status == 3 || days < 0;
                    reminders.Add(new ReminderItem
                    {
                        Type = "收款计划",
                        Title = p.plan_name,
                        Customer = order == null ? "" : customerNames.GetValueOrDefault(order.customer_id, ""),
                        Meta = order == null ? "" : $"订单 {order.sn}",
                        Tag = overdue ? "已逾期" : days == 0 ? "今天" : $"{days}天后",
                        TagClass = overdue ? "danger" : "warn",
                        DateText = p.plan_date.Value.ToString("MM-dd"),
                        Amount = p.plan_amount ?? 0m,
                        Link = $"/SaleOrderPayPlan/Receipt?id={p.id}",
                        SortDate = p.plan_date.Value
                    });
                }
            }

            if (HasMenu("Sale_Contract"))
            {
                foreach (var c in contracts
                    .Where(a => a.End_date.HasValue && a.End_date.Value.Date <= today.AddDays(30))
                    .OrderBy(a => a.End_date)
                    .Take(6))
                {
                    var days = (c.End_date.Value.Date - today).Days;
                    reminders.Add(new ReminderItem
                    {
                        Type = "合同到期",
                        Title = c.Contract_name,
                        Customer = customerNames.GetValueOrDefault(c.customer_id, ""),
                        Meta = $"合同 {c.sn}",
                        Tag = days < 0 ? "已到期" : days == 0 ? "今天" : $"{days}天后",
                        TagClass = days <= 7 ? "danger" : "info",
                        DateText = c.End_date.Value.ToString("MM-dd"),
                        Amount = c.Contract_amount ?? 0m,
                        Link = $"/SaleContract/Add?id={c.id}",
                        SortDate = c.End_date.Value
                    });
                }
            }

            var dashboard = new HomeDashboard
            {
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Greeting = greeting,
                TodayText = $"{DateTime.Now:yyyy年M月d日} 星期{weekday}",
                IsAdmin = isAdmin,
                AllowedMenus = allowedMenus,
                CustomerCount = customers.Count,
                FollowCount = follows.Count,
                OrderCount = orders.Count,
                ContractCount = contracts.Count,
                ReceiveCount = receives.Count,
                OrderAmount = orderAmount,
                ReceiveAmount = receiveAmount,
                Arrears = arrears,
                ContractAmount = contractAmount,
                InvoiceAmount = invoiceAmount,
                PayRate = payRate,
                MonthNewCustomers = customers.Count(a => a.create_time.HasValue && a.create_time.Value >= monthStart),
                MonthFollows = follows.Count(a => a.follow_time.HasValue && a.follow_time.Value >= monthStart),
                MonthOrderAmount = orders.Where(a => a.Order_date.HasValue && a.Order_date.Value >= monthStart).Sum(OrderTotal),
                MonthReceiveAmount = receives.Where(a => a.Receive_date.HasValue && a.Receive_date.Value >= monthStart).Sum(a => a.Receive_amount ?? 0m),
                PendingPlanCount = plans.Count(a => (a.plan_status ?? 0) != 2 && a.plan_date.HasValue && a.plan_date.Value.Date <= today.AddDays(30)),
                TrendMonths = months.Select(m => m.ToString("M月")).ToList(),
                TrendOrderAmount = months.Select(m => orders
                    .Where(o => o.Order_date.HasValue && o.Order_date.Value.Year == m.Year && o.Order_date.Value.Month == m.Month)
                    .Sum(OrderTotal)).ToList(),
                TrendReceiveAmount = months.Select(m => receives
                    .Where(r => r.Receive_date.HasValue && r.Receive_date.Value.Year == m.Year && r.Receive_date.Value.Month == m.Month)
                    .Sum(r => r.Receive_amount ?? 0m)).ToList(),
                OrderStatuses = statusStats,
                Industries = industryStats,
                RecentOrders = orders
                    .OrderByDescending(o => o.Order_date ?? o.create_time ?? DateTime.MinValue)
                    .Take(5)
                    .Select(o => new RecentOrderItem
                    {
                        Id = o.id,
                        Sn = o.sn,
                        Customer = customerNames.GetValueOrDefault(o.customer_id, ""),
                        Status = paramNames.GetValueOrDefault(o.Order_status_id, "未设置"),
                        Date = o.Order_date,
                        Amount = OrderTotal(o)
                    }).ToList(),
                RecentFollows = follows
                    .OrderByDescending(f => f.follow_time ?? DateTime.MinValue)
                    .Take(5)
                    .Select(f => new RecentFollowItem
                    {
                        CustomerId = f.customer_id,
                        Customer = customerNames.GetValueOrDefault(f.customer_id, ""),
                        Content = string.IsNullOrEmpty(f.follow_content)
                            ? ""
                            : (f.follow_content.Length > 60 ? f.follow_content.Substring(0, 60) + "…" : f.follow_content),
                        Employee = employeeNames.GetValueOrDefault(f.employee_id, ""),
                        Time = f.follow_time
                    }).ToList(),
                Reminders = reminders.OrderBy(a => a.SortDate).Take(6).ToList()
            };

            return View(dashboard);
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
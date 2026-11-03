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
    public class Report_CustomerController : ReportControllerBase
    {
        private readonly ILogger<Report_CustomerController> _logger;
        private readonly ICRM_CustomerService _service;
        private readonly ICRM_followService _followservice;
        private readonly IDBAuthService _dBAuthService;

        private readonly SysLogExt<CRM_Customer> logext = new SysLogExt<CRM_Customer>();

        public Report_CustomerController(
            ILogger<Report_CustomerController> logger,
            ICRM_CustomerService service,
            ICRM_followService followservice,
            IDBAuthService dBAuthService,
            IFreeSql fsql
            ) : base(fsql, dBAuthService)
        {
            _service = service;
            _followservice=followservice;
            _logger = logger;  
            _dBAuthService = dBAuthService;
        }

        public async Task<IActionResult> Index()
        {
            var scope = await GetScopeAsync();
            var year = GetSelectedYear();
            var empId = GetSelectedEmp(scope);

            var allCustomers = await ReportFsql.Select<CRM_Customer>().Where(a => a.isDelete != 1).ToListAsync();
            var allFollows = await ReportFsql.Select<CRM_follow>().Where(a => a.isDelete != 1).ToListAsync();
            var allOrders = await ReportFsql.Select<Sale_order>().Where(a => a.isDelete != 1).ToListAsync();
            var paramNames = await GetParamNamesAsync();

            var managedCustomerIds = allCustomers
                .Where(a => scope.Full || scope.AllowedEmpIds.Contains(a.emp_id))
                .Select(a => a.id)
                .ToHashSet();

            var customers = allCustomers
                .Where(a => scope.Full || scope.AllowedEmpIds.Contains(a.emp_id) || a.isPrivate == 1)
                .ToList();
            var follows = allFollows.Where(a => managedCustomerIds.Contains(a.customer_id)).ToList();
            var orders = allOrders.Where(a => managedCustomerIds.Contains(a.customer_id)).ToList();

            if (!string.IsNullOrWhiteSpace(empId))
            {
                customers = customers.Where(a => a.emp_id == empId).ToList();
                follows = follows.Where(a => a.employee_id == empId).ToList();
                orders = orders.Where(a => a.emp_id == empId).ToList();
            }

            var yearCustomers = customers.Where(a => a.create_time.HasValue && a.create_time.Value.Year == year).ToList();
            var yearFollows = follows.Where(a => a.follow_time.HasValue && a.follow_time.Value.Year == year).ToList();
            var yearOrders = orders.Where(a => OrderDate(a).HasValue && OrderDate(a).Value.Year == year).ToList();

            var customerNames = allCustomers.GroupBy(a => a.id).ToDictionary(g => g.Key, g => g.First());
            var employeeNames = await GetEmployeeNamesAsync(scope);
            var orderCustomerIds = yearOrders.Select(a => a.customer_id).Distinct().ToHashSet();
            var silentThreshold = DateTime.Today.AddDays(-30);

            var model = new CustomerReportModel
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
                    new ReportKpi { Label = "客户总数", Value = customers.Count.ToString("N0"), Hint = "当前权限范围", Icon = "fa-users", Tone = "blue" },
                    new ReportKpi { Label = "年度新增", Value = yearCustomers.Count.ToString("N0"), Hint = year + " 年新建客户", Icon = "fa-user-plus", Tone = "green" },
                    new ReportKpi { Label = "年度跟进", Value = yearFollows.Count.ToString("N0"), Hint = year + " 年沟通记录", Icon = "fa-phone", Tone = "cyan" },
                    new ReportKpi { Label = "成交客户", Value = orderCustomerIds.Count.ToString("N0"), Hint = "本年度有订单", Icon = "fa-handshake-o", Tone = "orange" },
                    new ReportKpi { Label = "客户转化率", Value = Percent(orderCustomerIds.Count, customers.Count), Hint = orderCustomerIds.Count + " / " + customers.Count + " 位客户", Icon = "fa-line-chart", Tone = "purple" },
                    new ReportKpi { Label = "长期未跟进", Value = customers.Count(a => !a.lastfollow.HasValue || a.lastfollow.Value < silentThreshold).ToString("N0"), Hint = "超过 30 天未联系", Icon = "fa-clock-o", Tone = "red" }
                },
                Trend = Enumerable.Range(1, 12).Select(month => new ReportTrendPoint
                {
                    Month = month + "月",
                    Count = yearCustomers.Count(a => a.create_time.HasValue && a.create_time.Value.Month == month),
                    Amount = yearFollows.Count(a => a.follow_time.HasValue && a.follow_time.Value.Month == month)
                }).ToList(),
                Industry = BuildDistribution(yearCustomers, a => a.cus_industry_id, paramNames, "未分类"),
                Level = BuildDistribution(yearCustomers, a => a.cus_level_id, paramNames, "未分级"),
                Type = BuildDistribution(yearCustomers, a => a.cus_type_id, paramNames, "未分类"),
                Source = BuildDistribution(yearCustomers, a => a.cus_source_id, paramNames, "未设置"),
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
                            Phone = customer?.cus_tel ?? "",
                            OrderCount = g.Count(),
                            OrderAmount = g.Sum(OrderAmount),
                            LastFollow = customer?.lastfollow
                        };
                    })
                    .OrderByDescending(a => a.OrderAmount)
                    .Take(8)
                    .ToList(),
                RecentCustomers = yearCustomers
                    .OrderByDescending(a => a.create_time)
                    .Take(6)
                    .Select(a => new ReportCustomerRow
                    {
                        Id = a.id,
                        Name = a.cus_name,
                        Owner = employeeNames.GetValueOrDefault(a.emp_id, "-"),
                        Level = ParamName(paramNames, a.cus_level_id, "未分级"),
                        Industry = ParamName(paramNames, a.cus_industry_id, "未分类"),
                        Phone = a.cus_tel,
                        CreateTime = a.create_time,
                        LastFollow = a.lastfollow,
                        OrderCount = yearOrders.Count(o => o.customer_id == a.id),
                        OrderAmount = yearOrders.Where(o => o.customer_id == a.id).Sum(OrderAmount)
                    })
                    .ToList(),
                SilentCustomers = customers
                    .Where(a => !a.lastfollow.HasValue || a.lastfollow.Value < silentThreshold)
                    .Select(a => new ReportCustomerRow
                    {
                        Id = a.id,
                        Name = a.cus_name,
                        Owner = employeeNames.GetValueOrDefault(a.emp_id, "-"),
                        Level = ParamName(paramNames, a.cus_level_id, "未分级"),
                        Phone = a.cus_tel,
                        LastFollow = a.lastfollow,
                        SilentDays = a.lastfollow.HasValue
                            ? Math.Max((DateTime.Today - a.lastfollow.Value.Date).Days, 0)
                            : 999,
                        OrderCount = yearOrders.Count(o => o.customer_id == a.id),
                        OrderAmount = yearOrders.Where(o => o.customer_id == a.id).Sum(OrderAmount)
                    })
                    .OrderByDescending(a => a.SilentDays)
                    .Take(6)
                    .ToList(),
                EmployeeRanking = yearOrders
                    .GroupBy(a => a.emp_id)
                    .Select(g => new ReportEmployeeRow
                    {
                        Name = employeeNames.GetValueOrDefault(g.Key, "未分配"),
                        Count = g.Count(),
                        Amount = g.Sum(OrderAmount),
                        Extra = "成交客户 " + g.Select(a => a.customer_id).Distinct().Count() + " 位"
                    })
                    .OrderByDescending(a => a.Amount)
                    .Take(6)
                    .ToList()
            };

            return View(model);
        }

        private static List<ReportNamedValue> BuildDistribution(
            IEnumerable<CRM_Customer> customers,
            Func<CRM_Customer, string> selector,
            Dictionary<string, string> paramNames,
            string fallback)
        {
            var list = customers.ToList();
            return list
                .GroupBy(selector)
                .Select(g => new ReportNamedValue
                {
                    Name = ParamName(paramNames, g.Key, fallback),
                    Value = g.Count(),
                    Extra = Percent(g.Count(), list.Count)
                })
                .OrderByDescending(a => a.Value)
                .Take(7)
                .ToList();
        }

        public async Task<string> ReportYear()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => true;

            // 年份处理：提取当前年份变量，避免表达式缓存
            int currentYear = DateTime.Now.Year;
            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                int queryYear = int.Parse(Request.Query["year"]);
                exp = exp.And(a => a.create_time.Value.Year == queryYear);
            }
            else
            {
                exp = exp.And(a => a.create_time.Value.Year == currentYear);
            }

            // emp_id IN 查询
            string empIdStr = Request.Query["emp_id"];
            if (!string.IsNullOrWhiteSpace(empIdStr))
            {
                string[] empArray = empIdStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                // 必须判断数组非空，防止 IN() 语法错误
                if (empArray.Length > 0)
                {
                    // 关键点：直接使用数组变量，不要用本地List
                    exp = exp.And(a => empArray.Contains(a.emp_id));
                }
            }

            var result = await _service.ReportYear(exp);

            JArray arr=new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata= result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();
                
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

        public async Task<string> ReportFollowYear()
        {
            Expression<Func<CRM_follow, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.follow_time.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.follow_time.Value.Year == DateTime.Now.Year);
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
                    exp = exp.And(a => list.Contains(a.employee_id));
                }
            }

            var result = await _followservice.ReportYear(exp);

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

        public async Task<string> ReportIndustry()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.create_time.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.create_time.Value.Year == DateTime.Now.Year);
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

            var result = await _service.ReportIndustry(exp);

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

        public async Task<string> ReportType()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.create_time.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.create_time.Value.Year == DateTime.Now.Year);
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


            var result = await _service.ReportType(exp);

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

        public async Task<string> ReportLevel()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.create_time.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.create_time.Value.Year == DateTime.Now.Year);
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


            var result = await _service.ReportLevel(exp);

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

        public async Task<string> ReportSource()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.create_time.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.create_time.Value.Year == DateTime.Now.Year);
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


            var result = await _service.ReportSource(exp);

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

        public async Task<string> ReportProvinces()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.create_time.Value.Year == Request.Query["year"]);
            }
            //else
            //{
            //    exp = exp.And(a => a.create_time.Value.Year == DateTime.Now.Year);
            //}

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

            var result = await _service.ReportProvinces(exp);

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

        public async Task<string> ReportCity()
        {
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.create_time.Value.Year == Request.Query["year"]);
            }
            //else
            //{
            //    exp = exp.And(a => a.create_time.Value.Year == DateTime.Now.Year);
            //}

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

            var result = await _service.ReportCity(exp);

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

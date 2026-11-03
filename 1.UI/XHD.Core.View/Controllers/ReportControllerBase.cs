using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Models;

namespace XHD.Core.View.Controllers
{
    /// <summary>
    /// 报表页公共能力：年份、员工筛选、数据权限和参数字典。
    /// </summary>
    public abstract class ReportControllerBase : Controller
    {
        protected readonly IFreeSql ReportFsql;
        protected readonly IDBAuthService ReportDBAuth;

        protected ReportControllerBase(IFreeSql fsql, IDBAuthService dBAuthService)
        {
            ReportFsql = fsql;
            ReportDBAuth = dBAuthService;
        }

        protected string CurrentEmpId => User.FindFirst(ClaimTypes.Sid)?.Value ?? string.Empty;
        protected bool IsAdmin => string.Equals(User.FindFirst("uid")?.Value, "admin", StringComparison.OrdinalIgnoreCase);

        protected async Task<ReportScope> GetScopeAsync()
        {
            var scope = new ReportScope();

            if (IsAdmin)
            {
                scope.Full = true;
            }
            else
            {
                var role = await ReportDBAuth.GetDataAuth(CurrentEmpId);
                scope.Full = role.authtype == 4;
                if (role.authtype != 0)
                {
                    foreach (var id in role.empList ?? new List<string>())
                    {
                        scope.AllowedEmpIds.Add(id);
                    }
                }
            }

            var employees = await ReportFsql.Select<hr_employee>()
                .Where(a => a.uid != "admin")
                .ToListAsync();

            scope.Employees = scope.Full
                ? employees
                : employees.Where(a => scope.AllowedEmpIds.Contains(a.id)).ToList();

            return scope;
        }

        protected int GetSelectedYear()
        {
            var current = DateTime.Now.Year;
            if (int.TryParse(Request.Query["year"], out var year) && year >= 2000 && year <= current + 1)
            {
                return year;
            }
            return current;
        }

        protected string GetSelectedEmp(ReportScope scope)
        {
            var empId = Request.Query["emp_id"].ToString();
            if (string.IsNullOrWhiteSpace(empId))
            {
                return string.Empty;
            }

            if (!scope.Full && !scope.AllowedEmpIds.Contains(empId))
            {
                return string.Empty;
            }

            return scope.Employees.Any(a => a.id == empId) ? empId : string.Empty;
        }

        protected List<ReportOption> BuildYearOptions(int selectedYear)
        {
            var results = new List<ReportOption>();
            var current = DateTime.Now.Year;
            var min = Math.Min(current - 4, selectedYear);
            for (var year = current; year >= min; year--)
            {
                results.Add(new ReportOption { Value = year.ToString(), Text = year + " 年", Selected = year == selectedYear });
            }
            return results;
        }

        protected List<ReportOption> BuildEmployeeOptions(ReportScope scope, string selectedEmp)
        {
            var results = new List<ReportOption>
            {
                new ReportOption { Value = "", Text = "全部员工", Selected = string.IsNullOrEmpty(selectedEmp) }
            };

            foreach (var employee in scope.Employees.OrderBy(a => a.name))
            {
                results.Add(new ReportOption
                {
                    Value = employee.id,
                    Text = employee.name,
                    Selected = employee.id == selectedEmp
                });
            }
            return results;
        }

        protected async Task<Dictionary<string, string>> GetEmployeeNamesAsync(ReportScope scope)
        {
            var names = scope.Employees
                .GroupBy(a => a.id)
                .ToDictionary(g => g.Key, g => g.First().name);

            var admin = await ReportFsql.Select<hr_employee>().Where(a => a.uid == "admin").FirstAsync();
            if (admin != null && !names.ContainsKey(admin.id))
            {
                names[admin.id] = "超级管理员";
            }
            return names;
        }

        protected async Task<Dictionary<string, string>> GetParamNamesAsync()
        {
            var parameters = await ReportFsql.Select<Sys_Param>().ToListAsync();
            return parameters
                .GroupBy(a => a.id)
                .ToDictionary(g => g.Key, g => g.First().params_name);
        }

        protected async Task<Dictionary<string, string>> GetProvinceNamesAsync()
        {
            var provinces = await ReportFsql.Select<Sys_Param_Provinces>().ToListAsync();
            return provinces
                .GroupBy(a => a.id)
                .ToDictionary(g => g.Key, g => g.First().Provinces);
        }

        protected static DateTime? OrderDate(Sale_order order)
        {
            return order.Order_date ?? order.create_time;
        }

        protected static decimal OrderAmount(Sale_order order)
        {
            return order.total_amount ?? order.Order_amount ?? 0m;
        }

        protected static string ParamName(Dictionary<string, string> names, string id, string fallback = "未设置")
        {
            if (!string.IsNullOrWhiteSpace(id) && names.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return fallback;
        }

        protected static string StatusClass(string statusId, string statusName)
        {
            if (statusId == "P_ORDER_DONE" || (statusName ?? string.Empty).Contains("完成"))
            {
                return "success";
            }
            if (statusId == "P_ORDER_UNPAID" || (statusName ?? string.Empty).Contains("待付"))
            {
                return "warn";
            }
            if (statusId == "P_ORDER_DOING" || (statusName ?? string.Empty).Contains("执行"))
            {
                return "info";
            }
            return "gray";
        }

        protected static string PlanTag(int? status, DateTime? planDate)
        {
            var today = DateTime.Today;
            if (status == 2)
            {
                return "已完成";
            }
            if (status == 3 || (planDate.HasValue && planDate.Value.Date < today))
            {
                return "已逾期";
            }
            if (planDate.HasValue && planDate.Value.Date == today)
            {
                return "今天";
            }
            if (planDate.HasValue && planDate.Value.Date <= today.AddDays(7))
            {
                return "7天内";
            }
            if (planDate.HasValue && planDate.Value.Date <= today.AddDays(30))
            {
                return "30天内";
            }
            return "待收款";
        }

        protected static string PlanTagClass(int? status, DateTime? planDate)
        {
            var today = DateTime.Today;
            if (status == 3 || (planDate.HasValue && planDate.Value.Date < today))
            {
                return "danger";
            }
            if (status == 2)
            {
                return "success";
            }
            if (planDate.HasValue && planDate.Value.Date <= today.AddDays(7))
            {
                return "warn";
            }
            return "info";
        }

        protected static string Percent(decimal value, decimal total)
        {
            return total <= 0 ? "0.0%" : (value / total * 100m).ToString("0.0") + "%";
        }
    }
}

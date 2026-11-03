using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using FreeSql;
using XHD.Core.Common;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICRM_CustomerService _service;
        private readonly ICRM_ContactService _contactService;
        private readonly ICRM_followService _followService;
        private readonly ISale_orderService _orderService;
        private readonly ISale_contractService _contractService;
        private readonly IDBAuthService _dBAuthService;
        private readonly ISys_ParamService _paramService;
        private readonly ISys_Param_ProvincesService _provincesService;
        private readonly ISys_logService _logService;
        private readonly ISys_infoService _infoService;
        private readonly IFreeSql _fsql;
        private readonly SysLogExt<CRM_Customer> _logExt = new();

        public CustomerController(
            ICRM_CustomerService service,
            ICRM_ContactService contactService,
            ICRM_followService followService,
            ISale_orderService orderService,
            ISale_contractService contractService,
            IDBAuthService dBAuthService,
            ISys_ParamService paramService,
            ISys_Param_ProvincesService provincesService,
            ISys_logService logService,
            ISys_infoService infoService,
            IFreeSql fsql)
        {
            _service = service;
            _contactService = contactService;
            _followService = followService;
            _orderService = orderService;
            _contractService = contractService;
            _dBAuthService = dBAuthService;
            _paramService = paramService;
            _provincesService = provincesService;
            _logService = logService;
            _infoService = infoService;
            _fsql = fsql;
        }

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult Index()
        {
            var allParams = _paramService.Grid(p => true).data;
            var cus_industry = allParams.Where(p => p.params_type == "cus_industry").ToDictionary(p => p.id, p => p.params_name);
            var cus_type = allParams.Where(p => p.params_type == "cus_type").ToDictionary(p => p.id, p => p.params_name);
            var cus_level = allParams.Where(p => p.params_type == "cus_level").ToDictionary(p => p.id, p => p.params_name);
            var cus_source = allParams.Where(p => p.params_type == "cus_source").ToDictionary(p => p.id, p => p.params_name);

            var provinces = _provincesService.Grid(p => true).data.ToDictionary(p => p.id, p => p.Provinces);

            ViewData["cus_industry"] = cus_industry;
            ViewData["cus_type"] = cus_type;
            ViewData["cus_level"] = cus_level;
            ViewData["cus_source"] = cus_source;
            ViewData["Province"] = provinces;

            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult CustomerMap()
        {
            var mapKey = _infoService.Grid(i => i.sys_key == "map_key").data.FirstOrDefault()?.sys_value;
            ViewData["map_key"] = mapKey;
            return View();
        }

        public IActionResult mapmark()
        {
            var mapKey = _infoService.Grid(i => i.sys_key == "map_key").data.FirstOrDefault()?.sys_value;
            ViewData["map_key"] = mapKey;
            return View();
        }

        /// <summary>
        /// 客户分析：订单/回款/合同/跟进/收款计划综合看板
        /// </summary>
        public async Task<string> Analysis(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return XHDResult.Error("缺少客户参数").ToString();
            }

            var customer = (await _service.GridAsync(a => a.id == id, 1, 1)).data.FirstOrDefault();
            if (customer == null)
            {
                return XHDResult.Error("客户不存在或已删除").ToString();
            }

            var orders = await _fsql.Select<Sale_order>().Where(a => a.customer_id == id).ToListAsync();
            var orderIds = orders.Select(a => a.id).ToList();
            var contracts = await _fsql.Select<Sale_contract>().Where(a => a.customer_id == id).ToListAsync();
            var follows = await _fsql.Select<CRM_follow>().Where(a => a.customer_id == id).ToListAsync();

            var receives = new List<Finance_Receive>();
            var plans = new List<Sale_order_pay_plan>();
            var invoices = new List<Finance_Invoice>();
            var details = new List<Sale_order_details>();

            if (orderIds.Count > 0)
            {
                receives = await _fsql.Select<Finance_Receive>().Where(a => orderIds.Contains(a.order_id)).ToListAsync();
                plans = await _fsql.Select<Sale_order_pay_plan>().Where(a => orderIds.Contains(a.order_id)).ToListAsync();
                invoices = await _fsql.Select<Finance_Invoice>().Where(a => orderIds.Contains(a.order_id)).ToListAsync();
                details = await _fsql.Select<Sale_order_details>().Where(a => orderIds.Contains(a.order_id)).ToListAsync();
            }

            //产品名称字典
            var productIds = details.Select(a => a.product_id).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var productDict = new Dictionary<string, string>();
            if (productIds.Count > 0)
            {
                var products = await _fsql.Select<Product>().Where(a => productIds.Contains(a.id)).ToListAsync();
                productDict = products.GroupBy(a => a.id).ToDictionary(a => a.Key, a => a.First().product_name);
            }

            //收款方式名称字典
            var payTypeIds = orders.Select(a => a.pay_type_id).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var payTypeDict = new Dictionary<string, string>();
            if (payTypeIds.Count > 0)
            {
                var payParams = await _fsql.Select<Sys_Param>().Where(a => payTypeIds.Contains(a.id)).ToListAsync();
                payTypeDict = payParams.GroupBy(a => a.id).ToDictionary(a => a.Key, a => a.First().params_name);
            }

            var orderAmount = orders.Sum(a => a.total_amount ?? a.Order_amount ?? 0m);
            var received = receives.Sum(a => a.Receive_amount ?? 0m);
            var arrears = Math.Max(orderAmount - received, 0m);
            var invoiceAmount = invoices.Sum(a => a.invoice_amount ?? 0m);
            var contractAmount = contracts.Sum(a => a.Contract_amount ?? 0m);
            var planTotal = plans.Sum(a => a.plan_amount ?? 0m);
            var planRemain = Math.Max(planTotal - plans.Sum(a => a.received_amount ?? 0m), 0m);
            var overdueCount = plans.Count(a => a.plan_status == 3);
            var lastFollowTime = follows.Where(a => a.follow_time.HasValue).Select(a => a.follow_time).OrderByDescending(a => a).FirstOrDefault();
            var firstOrderTime = orders.Where(a => a.Order_date.HasValue).Select(a => a.Order_date).OrderBy(a => a).FirstOrDefault();
            var lastFollowDays = lastFollowTime.HasValue ? (DateTime.Today - lastFollowTime.Value.Date).Days : -1;
            var cooperationDays = firstOrderTime.HasValue ? (DateTime.Today - firstOrderTime.Value.Date).Days : (DateTime.Today - (customer.create_time?.Date ?? DateTime.Today)).Days;
            var avgOrder = orders.Count > 0 ? Math.Round(orderAmount / orders.Count, 2) : 0m;
            var payRate = orderAmount > 0 ? Math.Min(received / orderAmount, 1m) : 0m;

            //客户健康分：回款 40 + 复购 20 + 最近跟进 20 + 合同 10 + 无欠款 10
            //统一使用 CustomerScoreService，详情页与客户列表共用同一套规则
            var scoreResult = XHD.Core.View.Services.CustomerScoreService.Calculate(
                orders.Count, orderAmount, received, contracts.Count, lastFollowTime);
            var healthScore = scoreResult.Score;
            var healthLabel = scoreResult.Label;
            var healthColor = scoreResult.Color;
            await _fsql.Update<CRM_Customer>()
                .Where(a => a.id == id)
                .Set(a => a.score, healthScore)
                .ExecuteAffrowsAsync();

            //近12个月订单/回款趋势
            var months = Enumerable.Range(0, 12).Select(i => DateTime.Today.AddMonths(i - 11)).ToList();
            var monthLabels = months.Select(a => a.ToString("yyyy-MM")).ToList();
            var orderTrend = months.Select(m => orders
                .Where(o => o.Order_date.HasValue && o.Order_date.Value.Year == m.Year && o.Order_date.Value.Month == m.Month)
                .Sum(o => o.total_amount ?? o.Order_amount ?? 0m)).ToList();
            var receiveTrend = months.Select(m => receives
                .Where(r => r.Receive_date.HasValue && r.Receive_date.Value.Year == m.Year && r.Receive_date.Value.Month == m.Month)
                .Sum(r => r.Receive_amount ?? 0m)).ToList();

            //产品偏好
            var productsStat = details
                .GroupBy(a => string.IsNullOrWhiteSpace(a.product_id) ? "未关联产品" : (productDict.TryGetValue(a.product_id, out var pname) ? pname : "未命名产品"))
                .Select(g => new { name = g.Key, value = g.Sum(a => a.amount ?? 0m), quantity = g.Sum(a => a.quantity ?? 0) })
                .OrderByDescending(a => a.value)
                .Take(8)
                .ToList();

            //收款方式分布（按订单）
            var payTypeStat = orders
                .GroupBy(a => string.IsNullOrWhiteSpace(a.pay_type_id) ? "未设置" : (payTypeDict.TryGetValue(a.pay_type_id, out var tname) ? tname : "其他"))
                .Select(g => new { name = g.Key, value = g.Sum(a => a.total_amount ?? a.Order_amount ?? 0m), count = g.Count() })
                .OrderByDescending(a => a.value)
                .ToList();

            //最近动态
            var timeline = new List<(DateTime time, Dictionary<string, object> item)>();

            void AddTimeline(DateTime? time, string type, string color, string title, string desc)
            {
                if (!time.HasValue) return;
                timeline.Add((time.Value, new Dictionary<string, object>
                {
                    { "type", type },
                    { "color", color },
                    { "title", title },
                    { "desc", desc },
                    { "time", time.Value.ToString("yyyy-MM-dd HH:mm") }
                }));
            }

            foreach (var f in follows)
            {
                AddTimeline(f.follow_time, "跟进", "#1677ff", "客户沟通", Truncate(f.follow_content, 80));
            }
            foreach (var o in orders)
            {
                AddTimeline(o.Order_date ?? o.create_time, "订单", "#16a34a", o.sn, $"订单金额 ¥{(o.total_amount ?? o.Order_amount ?? 0m):N2}");
            }
            foreach (var c in contracts)
            {
                AddTimeline(c.Sign_date ?? c.create_time, "合同", "#7c3aed", c.Contract_name, $"合同金额 ¥{(c.Contract_amount ?? 0m):N2}");
            }
            foreach (var r in receives)
            {
                AddTimeline(r.Receive_date ?? r.create_time, "收款", "#f59e0b", r.Receive_num, $"收款金额 ¥{(r.Receive_amount ?? 0m):N2}");
            }
            foreach (var p in plans)
            {
                AddTimeline(p.plan_date, "收款计划", "#0ea5e9", p.plan_name, $"计划 ¥{(p.plan_amount ?? 0m):N2} · {PlanStatusText(p.plan_status)}");
            }

            var timelineData = timeline.OrderByDescending(a => a.time).Take(20).Select(a => a.item).ToList();

            var kpi = new
            {
                orderCount = orders.Count,
                orderAmount,
                received,
                arrears,
                invoiceAmount,
                contractCount = contracts.Count,
                contractAmount,
                followCount = follows.Count,
                planCount = plans.Count,
                planTotal,
                planRemain,
                overdueCount,
                avgOrder,
                payRate = Math.Round(payRate * 100m, 1),
                lastFollowDays,
                lastFollowTime = lastFollowTime?.ToString("yyyy-MM-dd"),
                cooperationDays,
                repeatOrders = Math.Max(orders.Count - 1, 0)
            };

            var data = new
            {
                kpi,
                health = new { score = healthScore, label = healthLabel, color = healthColor },
                trend = new { months = monthLabels, orderAmount = orderTrend, receiveAmount = receiveTrend },
                payTypes = payTypeStat,
                products = productsStat,
                timeline = timelineData
            };

            return JsonConvert.SerializeObject(new { code = 0, data = new[] { data } });
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "…";
        }

        private static string PlanStatusText(int? status)
        {
            switch (status)
            {
                case 2: return "已收清";
                case 3: return "已逾期";
                case 1: return "部分收款";
                default: return "待收款";
            }
        }

        public async Task<string> Grid(PageView<CRM_Customer> model)
        {
            var exp = await BuildCustomerQueryExpression();
            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");
            return result.ToString();
        }

        public async Task<string> GetPoint()
        {
            //获取基础查询条件
            var exp = await BuildCustomerQueryExpression(includePrivate: true);
            //追加坐标过滤条件，数据库直接过滤，不要内存Where
            exp = exp.And(c => c.x > 0 && c.y > 0);

            //直接投影，只拿需要的5个字段，不要加载完整实体
            var CustomerList = await _service.GridAsync(exp);

            var List = CustomerList.data.Select(c => new
            {
                cus_name = c.cus_name,
                cus_add = c.cus_add,
                cus_tel = c.cus_tel,
                x = c.x,
                y = c.y
            }).ToList();

            

            JArray jArray = JArray.FromObject(List);
            
            var result = XHDResult.Success(jArray);
            //直接序列化对象，避免手动构造JArray再ToString的双重开销
            return result.ToString();
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.Sid)?.Value;
        }

        private async Task<bool> CheckAuthAsync(string operation)
        {
            // 直接将常量 "CRM_Customer" 写在这里
            return await _dBAuthService.GetAuth(GetUserId(), $"CRM_Customer|{operation}");
        }

        public async Task<string> Save(CRM_Customer model)
        {
            if (string.IsNullOrWhiteSpace(model.id))
            {
                // 新增
                if (!await CheckAuthAsync("add"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.sn = $"CU-{DateTime.Now:yyyyMMdd}-{model.id.Split('-')[2]}";
                model.create_id = GetUserId();
                model.create_time = DateTime.Now;
                model.isDelete = 0;

                var result = await _service.AddAsync(model);
                if (result == 0)
                {
                    return XHDResult.Error("操作失败，系统错误！").ToString();
                }
            }
            else
            {
                // 编辑
                if (!await CheckAuthAsync("edit"))
                {
                    return XHDResult.Error("无权限！").ToString();
                }

                var old = (await _service.GridAsync(c => c.id == model.id, 1, 1)).data.FirstOrDefault();
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
                        id = UUIDNext.Uuid.NewSequential().ToString(),
                        EventType = "[客户]修改",   // 直接写字符串，不再使用常量
                        EventID = model.id,
                        cus_id = model.id,
                        EventTitle = model.cus_name,
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

        public async Task<string> Excute()
        {
            var cusName = Request.Form["cus_name"].ToString();
            var currentId = Request.Form["id"].ToString();

            var exists = await _service.GridAsync(c => c.cus_name == cusName && c.id != currentId);
            if (exists.count > 0)
            {
                return XHDResult.Success().ToString();
            }
            return XHDResult.Error("").ToString();
        }

        public async Task<string> Delete(string id)
        {
            // 检查关联数据
            if ((await _contactService.GridAsync(c => c.customer_id == id, 1, 1)).count > 0)
            {
                return XHDResult.Error("此客户下含有联系人，不能删除！").ToString();
            }
            if ((await _followService.GridAsync(f => f.customer_id == id, 1, 1)).count > 0)
            {
                return XHDResult.Error("此客户下含有跟进，不能删除！").ToString();
            }
            if ((await _orderService.GridAsync(o => o.customer_id == id, 1, 1)).count > 0)
            {
                return XHDResult.Error("此客户下含有订单，不能删除！").ToString();
            }
            if ((await _contractService.GridAsync(c => c.customer_id == id, 1, 1)).count > 0)
            {
                return XHDResult.Error("此客户下含有合同，不能删除！").ToString();
            }

            if (!await _dBAuthService.GetAuth(GetUserId(), "CRM_Customer|del"))
            {
                return XHDResult.Error("无权限！").ToString();
            }

            var customer = (await _service.GridAsync(c => c.id == id, 1, 1)).data.FirstOrDefault();
            if (customer == null)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            var result = await _service.DeleteAsync(id);
            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            _logExt.getEntityText(customer);
            await _logService.DeleteLog(new Sys_log
            {
                id = Guid.NewGuid().ToString(),
                EventType = "[客户]删除",
                EventID = id,
                cus_id = id,
                EventTitle = customer.cus_name,
                UserID = GetUserId(),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value,
                IPStreet = HttpContext.Connection.RemoteIpAddress?.ToString(),
                EventDate = DateTime.Now
            });

            return XHDResult.Success().ToString();
        }

        public async Task<ActionResult> Export()
        {
            // 原有业务逻辑完全保留
            var exp = await BuildCustomerQueryExpression(includePrivate: true);
            var customers = (await _service.GridAsync(exp, "a.create_time desc")).data;

            // 表头定义（和原代码完全一致）
            var sheetTitle = new[] {
        "编号","客户名字","地址","电话","传真",
        "网址","行业","省份","城市","类别",
        "级别","来源","描述","备注","归属",
        "公私","最后跟进","坐标","创建时间"
    };

            // 1. 构建DataTable：先固定表头列，空数据也强制保留表头
            var dt = new DataTable();
            foreach (var title in sheetTitle)
            {
                dt.Columns.Add(title);
            }

            // 2. 填充数据行，完全保留原映射逻辑
            foreach (var c in customers)
            {
                var row = dt.NewRow();
                row["编号"] = c.sn ?? "";
                row["客户名字"] = c.cus_name ?? "";
                row["地址"] = c.cus_add ?? "";
                row["电话"] = c.cus_tel ?? "";
                row["传真"] = c.cus_fax ?? "";
                row["网址"] = c.cus_website ?? "";
                row["行业"] = c.cus_industry?.params_name ?? "";
                row["省份"] = c.Provinces?.Provinces ?? "";
                row["城市"] = c.City?.City ?? "";
                row["类别"] = c.cus_type?.params_name ?? "";
                row["级别"] = c.cus_level?.params_name ?? "";
                row["来源"] = c.cus_source?.params_name ?? "";
                row["描述"] = c.DesCripe ?? "";
                row["备注"] = c.Remarks ?? "";
                row["归属"] = c.Employee?.name ?? "";
                row["公私"] = c.isPrivate == 1 ? "公客" : "私客";
                row["最后跟进"] = c.lastfollow ?? null;
                row["坐标"] = c.xy ?? "";
                row["创建时间"] = c.create_time ?? null;
                dt.Rows.Add(row);
            }

            // 3. 配置样式
            var excelConfig = new OpenXmlConfiguration
            {
                AutoFilter = true,
                TableStyles = TableStyles.Default,
                DynamicColumns = sheetTitle.Select((title, index) => new DynamicExcelColumn(title)
                {
                    Index = index,
                    Width = 15
                }).ToArray()
            };

            // 4. 写入流
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await ms.SaveAsAsync(
                    value: dt,
                    sheetName: "客户列表",
                    excelType: ExcelType.XLSX,
                    configuration: excelConfig
                );
                fileBytes = ms.ToArray();
            }

            // 5. 返回文件
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"客户信息{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }


        // ========== 私有辅助方法 ==========

        /// <summary>
        /// 构建客户查询的通用表达式（含过滤条件和数据权限）
        /// </summary>
        /// <param name="includePrivate">是否包含私客（true表示全部，false表示只取公客且受权限控制）</param>
        private async Task<Expression<Func<CRM_Customer, bool>>> BuildCustomerQueryExpression(bool includePrivate = false)
        {
            Expression<Func<CRM_Customer, bool>> exp = c => true;

            if (Request.Query["type"] == "bath")
            {
                if (!string.IsNullOrWhiteSpace(Request.Query["old_emp_id"]))
                {
                    exp = exp.And(c => c.emp_id == Request.Query["old_emp_id"]);
                }
                else
                {
                    exp = c => false;
                }
            }

            // 查询参数
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(c => c.cus_name.Contains(Request.Query["cus_name"]));
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                exp = exp.And(c => c.cus_name.Contains(Request.Query["keyword"]));
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["id"]))
            {
                exp = exp.And(c => c.id == Request.Query["id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_tel"]))
            {
                exp = exp.And(c => c.cus_tel.Contains(Request.Query["cus_tel"]));
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_industry_id"]))
            {
                exp = exp.And(c => c.cus_industry_id == Request.Query["cus_industry_id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_type_id"]))
            {
                exp = exp.And(c => c.cus_type_id == Request.Query["cus_type_id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_level_id"]))
            {
                exp = exp.And(c => c.cus_level_id == Request.Query["cus_level_id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["cus_source_id"]))
            {
                exp = exp.And(c => c.cus_source_id == Request.Query["cus_source_id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["Provinces_id"]))
            {
                exp = exp.And(c => c.Provinces_id == Request.Query["Provinces_id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["City_id"]))
            {
                exp = exp.And(c => c.City_id == Request.Query["City_id"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["score_level"]))
            {
                var scoreLevel = Request.Query["score_level"].ToString();
                if (scoreLevel == "high")
                {
                    exp = exp.And(c => c.score >= 80);
                }
                else if (scoreLevel == "good")
                {
                    exp = exp.And(c => c.score >= 60 && c.score < 80);
                }
                else if (scoreLevel == "potential")
                {
                    exp = exp.And(c => c.score >= 40 && c.score < 60);
                }
                else if (scoreLevel == "low")
                {
                    exp = exp.And(c => c.score < 40);
                }
                else if (scoreLevel == "none")
                {
                    exp = exp.And(c => c.score == null);
                }
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                exp = exp.And(c => c.emp_id == Request.Query["emp_id"]);
            }
            if (PageValidate.IsDateTime(Request.Query["date1"]))
            {
                exp = exp.And(c => c.create_time >= DateTime.Parse(Request.Query["date1"]));
            }
            if (PageValidate.IsDateTime(Request.Query["date2"]))
            {
                exp = exp.And(c => c.create_time <= DateTime.Parse(Request.Query["date2"]));
            }

            // 数据权限处理
            var roledata = await _dBAuthService.GetDataAuth(GetUserId());
            var isFullAccess = roledata.authtype == 4;

            if (!includePrivate)
            {
                // 表格查询：根据 isPrivate 参数决定
                if (PageValidate.IsNumber(Request.Query["isPrivate"]))
                {
                    var isPrivate = int.Parse(Request.Query["isPrivate"]);
                    if (isPrivate == 1)
                    {
                        exp = exp.And(c => c.isPrivate == 1);
                    }
                    else
                    {
                        exp = exp.And(c => c.isPrivate == 0);
                        if (!isFullAccess)
                        {
                            exp = exp.And(c => roledata.empList.Contains(c.emp_id));
                        }
                    }
                }
                else
                {
                    if (!isFullAccess)
                    {
                        exp = exp.And(c => roledata.empList.Contains(c.emp_id) || c.isPrivate == 1);
                    }
                }
            }
            else
            {
                // 地图/导出：只应用普通权限（不加 isPrivate 过滤）
                if (!isFullAccess)
                {
                    exp = exp.And(c => roledata.empList.Contains(c.emp_id));
                }
            }

            return exp;
        }


    }
}
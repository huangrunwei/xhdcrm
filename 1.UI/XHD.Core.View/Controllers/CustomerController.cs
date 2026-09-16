using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
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
            ISys_infoService infoService)
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
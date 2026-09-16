using FreeSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using UUIDNext;
using XHD.Core.Common;
using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;


namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class CRM_ContactController : Controller
    {
        private readonly ILogger<CRM_ContactController> _logger;
        private readonly ICRM_ContactService _service;
        private readonly ICRM_followService _followservice;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<CRM_Contact> logext = new SysLogExt<CRM_Contact>();
       

        public CRM_ContactController(
            ILogger<CRM_ContactController> logger,
            ICRM_ContactService service, 
            ICRM_followService followservice, 
            ISys_logService LogService, 
            IDBAuthService dBAuthService
            )
        {
            _service = service;
            _logger = logger;
            _followservice = followservice;

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

        public async Task<string> Grid(PageView<CRM_Contact> model)
        {
            Expression<Func<CRM_Contact, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["C_name"]))
            {
                exp = exp.And(a => a.C_name.Contains(Request.Query["C_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                exp = exp.And(a => a.C_name.Contains(Request.Query["keyword"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["cus_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["C_tel"]))
            {
                exp = exp.And(a => a.C_tel.Contains(Request.Query["C_tel"]));
            }

            //exp = exp.And(a => _fsql.Select<hr_employee>().As("b").ToList(b => b.id).Contains(a.create_id));

            //权限
            //var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }


            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");

            return result.ToString();
        }

        public async Task<string> Save(CRM_Contact model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = User.FindFirst(ClaimTypes.Sid).Value;
                model.create_time = DateTime.Now;

                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "CRM_Contact|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "CRM_Contact|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<CRM_Contact, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _service.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    result = await _service.UpdateAsync(model);

                    //对比实体差别
                    
                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[联系人]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.C_name;
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

            return XHDResult.Success().ToString();
        }

        public async Task<string> Delete(string id)
        {
            //判断是否有跟进
            Expression<Func<CRM_follow, bool>> expfollow = a => a.contact_id == id;
            var followlist = await _followservice.GridAsync(expfollow, 1, 1);

            if (followlist.count > 0)
            {
                return XHDResult.Error("此联系人下含有跟进，不能删除！").ToString();
            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "CRM_Contact|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<CRM_Contact, bool>> exp = a => a.id == id;
                var checkdata = await _service.GridAsync(exp, 1, 1);

                if (checkdata.count == 0)
                {
                    return XHDResult.Error("找不到此数据！").ToString();
                }

                result = await _service.DeleteAsync(id);

                //日志

                //先存储删除的实体记录，用日志形式
                logext.getEntityText(checkdata.data[0]);

                //记录日志
                Sys_log logmodels = new Sys_log();

                logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                logmodels.EventType = "[联系人]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].C_name;
                logmodels.UserID = User.FindFirst(ClaimTypes.Sid).Value;
                logmodels.UserName = User.FindFirst(ClaimTypes.Name).Value;
                logmodels.IPStreet = HttpContext.Connection.RemoteIpAddress.ToString();
                logmodels.EventDate = DateTime.Now;
                //logmodels.Log_Content = content;

                
                await _LogService.DeleteLog(logmodels);
                //await _LogService.DeleteLog(logmodels);
            }
            else
            {
                return XHDResult.Error("无权限！").ToString();
            }

            //var result = await _service.Delete(id);

            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            return XHDResult.Success().ToString();
        }


        public async Task<ActionResult> Export()
        {
            // ============ 原有业务逻辑完全保留，一行未改 ============
            // 1. 构建查询表达式
            Expression<Func<CRM_Contact, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["C_name"]))
            {
                exp = exp.And(a => a.C_name.Contains(Request.Query["C_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                exp = exp.And(a => a.C_name.Contains(Request.Query["keyword"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["customer_id"]))
            {
                exp = exp.And(a => a.customer_id == Request.Query["customer_id"]);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["cus_name"]))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["cus_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["C_tel"]))
            {
                exp = exp.And(a => a.C_tel.Contains(Request.Query["C_tel"]));
            }

            // 2. 权限校验逻辑完全保留
            var roledata = await _dBAuthService.GetDataAuth(User.FindFirst(ClaimTypes.Sid).Value);
            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }

            // 3. 获取数据
            var result = await _service.GridAsync(exp, "a.create_time desc");

            // ============ Excel生成逻辑修复：空数据也强制保留表头 ============
            // 表头定义（和原代码完全一致，严格保证列顺序不变）
            var sheetTitle = new string[] {
        "姓名","客户名字","性别","部门","职务",
        "生日","电话","微信","邮箱","手机",
        "QQ","地址","爱好","备注"
    };

            // 1. 构建DataTable：先固定表头列，哪怕无数据行，表头也会完整生成
            var dt = new DataTable();
            // 先注册所有表头列，锁定列顺序和名称
            foreach (var title in sheetTitle)
            {
                dt.Columns.Add(title);
            }

            // 2. 填充数据行（完全保留原字段映射、空值处理逻辑）
            foreach (var contact in result.data)
            {
                var cusName = contact.customer?.cus_name ?? "";
                var row = dt.NewRow();

                row["姓名"] = contact.C_name ?? "";
                row["客户名字"] = cusName;
                row["性别"] = contact.C_sex ?? null;
                row["部门"] = contact.C_department ?? "";
                row["职务"] = contact.C_position ?? "";
                row["生日"] = contact.C_birthday ?? "";
                row["电话"] = contact.C_tel ?? "";
                row["微信"] = contact.C_weichat ?? "";
                row["邮箱"] = contact.C_email ?? "";
                row["手机"] = contact.C_mob ?? "";
                row["QQ"] = contact.C_QQ ?? "";
                row["地址"] = contact.C_add ?? "";
                row["爱好"] = contact.C_hobby ?? "";
                row["备注"] = contact.C_remarks ?? "";

                dt.Rows.Add(row);
            }

            // 3. 配置MiniExcel样式与列规则
            var excelConfig = new OpenXmlConfiguration
            {
                AutoFilter = true, // 开启自动筛选（和原NPOI功能一致）
                TableStyles = TableStyles.Default, // 表格默认美观样式
                DynamicColumns = sheetTitle.Select((title, index) => new DynamicExcelColumn(title)
                {
                    Index = index, // 强制指定列顺序，和原表头100%对齐
                    Width = 15 // 统一列宽，可根据需求调整
                }).ToArray()
            };

            // 4. 异步流式写入内存流，转byte[]返回，彻底解决流释放问题
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                // 传入DataTable，哪怕无数据行，也会生成完整表头
                await ms.SaveAsAsync(
                    value: dt,
                    sheetName: "联系人列表",
                    excelType: ExcelType.XLSX,
                    configuration: excelConfig
                );
                fileBytes = ms.ToArray();
            }

            // 5. 返回xlsx文件
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"联系人列表{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }


    }
}

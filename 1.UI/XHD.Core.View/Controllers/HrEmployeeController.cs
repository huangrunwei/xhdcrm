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
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class HrEmployeeController : Controller
    {
        private readonly ILogger<HrEmployeeController> _logger;
        private readonly Ihr_employeeService _service;
        private readonly ICRM_CustomerService _customerservice;
        private readonly ICRM_followService _followservice;
        private readonly ISale_orderService _orderservice;
        private readonly ISale_contractService _contractservice;
        private readonly IFinance_ReceiveService _receiveservice;
        private readonly IFinance_InvoiceService _invoiceservice;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<hr_employee> logext = new SysLogExt<hr_employee>();  //日志

        public HrEmployeeController(
            ILogger<HrEmployeeController> logger,
            Ihr_employeeService service,
            ICRM_CustomerService customerservice,
            ICRM_followService followservice,
            ISale_orderService orderservice,
            ISale_contractService contractservice,
            IFinance_ReceiveService receiveservice,
            IFinance_InvoiceService invoiceservice,
            ISys_logService LogService,
            IDBAuthService dBAuthService
            )
        {
            _service = service;
            _logger = logger;
            _customerservice = customerservice;
            _followservice = followservice;
            _orderservice = orderservice;
            _contractservice = contractservice;
            _receiveservice = receiveservice;
            _invoiceservice = invoiceservice;

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

        public IActionResult Me()
        {
            return View();
        }

        public IActionResult password()
        {
            return View();
        }

        public IActionResult syspwd()
        {
            return View();
        }

        public async Task<string> Grid(PageView<hr_employee> model)
        {
            Expression<Func<hr_employee, bool>> exp = a => a.id != "admin";

            if (Request.Query["id"].Equals("me"))
            {
                exp = a => a.id == User.FindFirst(ClaimTypes.Sid).Value;
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            {
                exp = exp.And(a => a.name.Contains(Request.Query["T_name"]));
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["keyword"]))
            {
                exp = exp.And(a => a.name.Contains(Request.Query["keyword"]));
            }

            

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "sort");

            //return JsonConvert.SerializeObject(result, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }).ToString();
            return result.ToString();
        }

        public async Task<string> Save(hr_employee model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_employee|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_employee|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<hr_employee, bool>> exp = a => a.id == model.id;
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
                        logmodels.EventType = "[员工]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.name;
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



            //Expression<Func<Sys_role_emp, bool>> exproleemp = a => a.empID == model.id;

            //await _role_empService.DeleteAsync(exproleemp);

            //Sys_role_emp modelroleemp = new Sys_role_emp();

            //modelroleemp.empID = model.id;

            //JArray arr = JArray.Parse(Request.Form["T_data"]);

            //foreach (JObject obj in arr)
            //{
            //    modelroleemp.RoleID = obj["Role"].Value<string>("id");

            //    await _role_empService.AddAsync(modelroleemp);
            //}


            return XHDResult.Success().ToString();
        }

        public async Task<string> Delete(string id)
        {
            if (id.Equals("admin"))
            {
                //删管理员，还是算了吧
                return XHDResult.Error("管理员不能删除！").ToString();
            }

            //客户
            Expression<Func<CRM_Customer, bool>> expcustomer = a => a.emp_id == id;

            var resultcustomer = await _customerservice.GridAsync(expcustomer, 1, 1);

            if (resultcustomer.count > 0)
            {
                return XHDResult.Error("此员工下有客户，不能删除！").ToString();
            }

            //跟进
            Expression<Func<CRM_follow, bool>> expfollow = a => a.employee_id == id;

            var resultfollow = await _followservice.GridAsync(expfollow, 1, 1);

            if (resultfollow.count > 0)
            {
                return XHDResult.Error("此员工下有跟进，不能删除！").ToString();
            }

            //订单
            Expression<Func<Sale_order, bool>> exporder = a => a.emp_id == id;

            var resultorder = await _orderservice.GridAsync(exporder, 1, 1);

            if (resultorder.count > 0)
            {
                return XHDResult.Error("此员工下有订单，不能删除！").ToString();
            }

            //合同
            Expression<Func<Sale_contract, bool>> expcontract = a => a.Our_Contractor_id == id;

            var resultcontract = await _contractservice.GridAsync(expcontract, 1, 1);

            if (resultcontract.count > 0)
            {
                return XHDResult.Error("此员工下有合同，不能删除！").ToString();
            }

            //收款
            Expression<Func<Finance_Receive, bool>> expreceive = a => a.Payee_id == id;

            var resultreceive = await _receiveservice.GridAsync(expreceive, 1, 1);

            if (resultreceive.count > 0)
            {
                return XHDResult.Error("此员工下有收款，不能删除！").ToString();
            }

            //发票
            Expression<Func<Finance_Invoice, bool>> expinvoice = a => a.emp_id == id;

            var resultinvoice = await _invoiceservice.GridAsync(expinvoice, 1, 1);

            if (resultinvoice.count > 0)
            {
                return XHDResult.Error("此员工下有发票，不能删除！").ToString();
            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "hr_employee|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<hr_employee, bool>> exp = a => a.id == id;
                var checkdata = await _service.GridAsync(exp, 1, 1);

                if (checkdata.count == 0)
                {
                    return XHDResult.Error("找不到此数据！").ToString();
                }

                result = await _service.DeleteAsync(id);

                //先存储删除的实体记录，用日志形式
                logext.getEntityText(checkdata.data[0]);

                //记录日志
                Sys_log logmodels = new Sys_log();

                logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                logmodels.EventType = "[员工]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].name;
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

            //var result = await _service.Delete(id);

            if (result == 0)
            {
                return XHDResult.Error("删除失败！").ToString();
            }

            return XHDResult.Success().ToString();
        }

        public async Task<string> modifyPWD()
        {
            var oldpassword = Request.Form["oldpassword"];

            Expression<Func<hr_employee, bool>> expwhere = a => a.id == User.FindFirst(ClaimTypes.Sid).Value;

            var data = await _service.GridAsync(expwhere);

            if (data.count == 0)
            {
                return XHDResult.Success("系统错误，找不到此用户！").ToString();
            }

            var checkpwd = Common.DEncrypt.MD5Comm.MD5Hash(oldpassword);

            if (!data.data[0].pwd.Equals(checkpwd))
            {
                return XHDResult.Success("原密码不正确！").ToString();
            }

            var password = Request.Form["password"];

            Expression<Func<hr_employee, hr_employee>> exppwd = a => new hr_employee { pwd = Common.DEncrypt.MD5Comm.MD5Hash(password) };


            await _service.UpdateAsync(exppwd, expwhere);

            return XHDResult.Success("修改成功！").ToString();
        }

        public async Task<string> PWD()
        {
            var password = Request.Form["password"];
            var id = Request.Form["id"];

            Expression<Func<hr_employee, hr_employee>> exppwd = a => new hr_employee { pwd = Common.DEncrypt.MD5Comm.MD5Hash(password) };
            Expression<Func<hr_employee, bool>> expwhere = a => a.id == id;

            await _service.UpdateAsync(exppwd, expwhere);

            return XHDResult.Success("修改成功！").ToString();
        }
    }
}

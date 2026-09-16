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
    public class SysParamController : Controller
    {
        private readonly ILogger<SysParamController> _logger;
        private readonly ISys_ParamService _service;
        private readonly ICRM_CustomerService _CustomerService;
        private readonly ICRM_followService _FollowService;
        private readonly ISale_orderService _OrderService;
        private readonly IFinance_ReceiveService _ReceiveService;
        private readonly IFinance_InvoiceService _InvoiceService;
        private readonly IMessage_newsService _NewsService;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sys_Param> logext = new SysLogExt<Sys_Param>();  //日志

        public SysParamController(ILogger<SysParamController> logger,
            ISys_ParamService service,
            ICRM_CustomerService CustomerService,
            ICRM_followService FollowService,
            ISale_orderService OrderService,
            IFinance_ReceiveService ReceiveService,
            IFinance_InvoiceService InvoiceService,
            IMessage_newsService NewsService,
            ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _service = service;
            _logger = logger;
            _CustomerService = CustomerService;
            _FollowService = FollowService;
            _OrderService = OrderService;
            _ReceiveService = ReceiveService;
            _InvoiceService= InvoiceService;
            _NewsService = NewsService;

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

        public async Task<string> Grid(PageView<Sys_Param> model)
        {
            Expression<Func<Sys_Param, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["T_type"]))
            {
                exp = exp.And(a => a.params_type == Request.Query["T_type"]);
            }

            var result = await _service.GridAsync(exp, model.Page, model.Limit, "a.params_order");

            //return JsonConvert.SerializeObject(result, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }).ToString();
            return result.ToString();
        }

        public async Task<string> Combo(string type)
        {
            Expression<Func<Sys_Param, bool>> exp = a => a.params_type == type;

            var result = await _service.GridAsync(exp, "a.params_order");

            //return JsonConvert.SerializeObject(result, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }).ToString();
            return result.ToString();
        }

        public async Task<string> Save(Sys_Param model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "sys_params|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "sys_params|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Sys_Param, bool>> exp = a => a.id == model.id;
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
                        logmodels.EventType = "[参数]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.params_name;
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
            Expression<Func<Sys_Param, bool>> expparams = a => a.id == id;

            var resultparams = await _service.GridAsync(expparams, 1, 1);

            if (resultparams.count == 0)
            {
                return XHDResult.Error("找不到此数据！").ToString();
            }

            var params_type = resultparams.data[0].params_type;

            //根据参数类别来判断，可以减少查询

            //客户
            Expression<Func<CRM_Customer, bool>> expcustomer = null;
            var resultcustomer = new XHDData<CRM_Customer>();

            //跟进
            Expression<Func<CRM_follow, bool>> expfollow = null;
            var resultfollow = new XHDData<CRM_follow>();

            //订单
            Expression<Func<Sale_order, bool>> exporder = null;
            var resultorder = new XHDData<Sale_order>();

            switch (params_type)
            {
                //客户
                case "cus_source":
                    expcustomer = a => a.cus_source_id == id;
                    resultcustomer = await _CustomerService.GridAsync(expcustomer, 1, 1);
                    if (resultcustomer.count > 0)
                    {
                        return XHDResult.Error("此参数下有客户，不能删除！").ToString();
                    }
                    break;
                case "cus_level":
                    expcustomer = a => a.cus_level_id == id;
                    resultcustomer = await _CustomerService.GridAsync(expcustomer, 1, 1);
                    if (resultcustomer.count > 0)
                    {
                        return XHDResult.Error("此参数下有客户，不能删除！").ToString();
                    }
                    break;
                case "cus_type":
                    expcustomer = a => a.cus_type_id == id;
                    resultcustomer = await _CustomerService.GridAsync(expcustomer, 1, 1);
                    if (resultcustomer.count > 0)
                    {
                        return XHDResult.Error("此参数下有客户，不能删除！").ToString();
                    }
                    break;
                case "cus_industry":
                    expcustomer = a => a.cus_industry_id == id;
                    resultcustomer = await _CustomerService.GridAsync(expcustomer, 1, 1);
                    if (resultcustomer.count > 0)
                    {
                        return XHDResult.Error("此参数下有客户，不能删除！").ToString();
                    }
                    break;

                //跟进
                case "follow_type":
                    expfollow = a => a.follow_type_id == id;
                    resultfollow = await _FollowService.GridAsync(expfollow, 1, 1);
                    if (resultfollow.count > 0)
                    {
                        return XHDResult.Error("此参数下有跟进，不能删除！").ToString();
                    }
                    break;
                case "follow_aim":
                    expfollow = a => a.follow_aim_id == id;
                    resultfollow = await _FollowService.GridAsync(expfollow, 1, 1);
                    if (resultfollow.count > 0)
                    {
                        return XHDResult.Error("此参数下有跟进，不能删除！").ToString();
                    }
                    break;

                //订单
                case "order_status":
                    exporder = a => a.Order_status_id == id;
                    resultorder = await _OrderService.GridAsync(exporder, 1, 1);
                    if (resultorder.count > 0)
                    {
                        return XHDResult.Error("此参数下有订单，不能删除！").ToString();
                    }
                    break;
                case "pay_type":
                    exporder = a => a.pay_type_id == id;
                    resultorder = await _OrderService.GridAsync(exporder, 1, 1);
                    if (resultorder.count > 0)
                    {
                        return XHDResult.Error("此参数下有订单，不能删除！").ToString();
                    }
                    
                    //收款
                    Expression<Func<Finance_Receive, bool>> expreceive = a => a.Pay_type_id == id; ;
                    var resultreceive = await _ReceiveService.GridAsync(expreceive, 1, 1);

                    if (resultreceive.count > 0)
                    {
                        return XHDResult.Error("此参数下有收款，不能删除！").ToString();
                    }

                    break;

                //发票
                case "invoice_type":
                    Expression<Func<Finance_Invoice, bool>> expinvoice = a => a.invoice_type_id == id; ;
                    var resultinvoice = await _InvoiceService.GridAsync(expinvoice, 1, 1);

                    if (resultinvoice.count > 0)
                    {
                        return XHDResult.Error("此参数下有发票，不能删除！").ToString();
                    }

                    break;

                //消息
                case "message_type":
                    Expression<Func<Message_news, bool>> expnews = a => a.news_type_id == id; ;
                    var resultnews = await _NewsService.GridAsync(expnews, 1, 1);

                    if (resultnews.count > 0)
                    {
                        return XHDResult.Error("此参数下有消息，不能删除！").ToString();
                    }

                    break;

            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "sys_params|del");

            if (authbtn)
            {
                //判断是否有数据
                Expression<Func<Sys_Param, bool>> exp = a => a.id == id;
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
                logmodels.EventType = "[参数]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].params_name;
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

    }
}

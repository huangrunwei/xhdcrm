
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.Common.DEncrypt;
using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;
using XHD.Core.View.Services;

namespace XHD.Core.View.Controllers
{
    public partial class APIController : Controller
    {
        private hr_employee employee = null;
        private readonly IDataProtector _tokenProtector;
        private readonly Ihr_employeeService _empservice;
        private readonly ICRM_CustomerService _customerservice;
        private readonly ICRM_followService _followservice;
        private readonly ISale_orderService _OrderService;
        private readonly ISale_order_detailsService _OrderDetailsService;
        private readonly ICRM_ContactService _contactservice;
        private readonly ISale_contractService _contractservice;
        private readonly ISale_contract_attaService _contractattaservice;
        private readonly IFinance_ReceiveService _ReceiveService;
        private readonly ISys_ParamService _SysParamService;
        private readonly IProductService _productservice;
        private readonly IDBAuthService _dBAuthService;
        private readonly ISys_logService _LogService;
        private readonly IFreeSql _fsql;

        public APIController(Ihr_employeeService empservice, ICRM_CustomerService customerservice, IDBAuthService dBAuthService, ICRM_followService followservice, ISale_orderService orderService, ISale_contractService contractservice, IFinance_ReceiveService receiveService, ICRM_ContactService contactservice, ISys_ParamService sysParamService, ISys_logService logService, IProductService productservice, ISale_order_detailsService orderDetailsService, ISale_contract_attaService contractattaservice, IFreeSql fsql, IDataProtectionProvider dataProtectionProvider)
        {
            _empservice = empservice;
            _customerservice = customerservice;
            _dBAuthService = dBAuthService;
            _followservice = followservice;
            _OrderService = orderService;
            _contractservice = contractservice;
            _ReceiveService = receiveService;
            _contactservice = contactservice;
            _SysParamService = sysParamService;
            _LogService = logService;
            _productservice = productservice;
            _OrderDetailsService = orderDetailsService;
            _contractattaservice = contractattaservice;
            _fsql = fsql;
            _tokenProtector = dataProtectionProvider.CreateProtector("XHD.Api.Token.v1");
        }

        #region 登录认证
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="uid">手机号</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Login(string uid, string? pwd)
        {
            pwd = (pwd ?? string.Empty).ToUpper();

            Expression<Func<hr_employee, bool>> expwhere = a => a.uid == uid && a.pwd == pwd;

            if (uid != "admin")
            {
                expwhere = expwhere.And(a => a.status == 1);
            }

            var list = await _empservice.GridAsync(expwhere);

            if (list.count == 0)
            {
                return XHDResult.Error("账号密码不匹配！").ToString();
            }

            var l = list.data[0];

            string id = l.id;
            var outtime = DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss");
            var expires = DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds();

            // 使用 ASP.NET Core Data Protection 生成防篡改令牌，不再使用固定密钥的 DES
            string tokentext = $"{id}|{expires}";
            string encrypttoken = _tokenProtector.Protect(tokentext);

            //返回Token
            JObject obj = new JObject();
            obj.Add("token", encrypttoken);
            obj.Add("id", id);
            obj.Add("outtime", outtime);
            obj.Add("RealName", l.name);
            //obj.Add("password", l.pwd);
            obj.Add("phone", l.tel);

            return XHDResult.Success(obj).ToString();
        }

        /// <summary>
        /// check token
        /// </summary>
        /// <returns></returns>
        public JObject checkToken()
        {
            // 获取所有请求头
            var allHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            // 获取特定请求头（例如Authorization）
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authValue = (string)authorizationHeader;

                if (string.IsNullOrWhiteSpace(authValue))
                {
                    return XHDResult.Error(-9, "Token错误！");
                }

                var _token = authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authValue.Substring(7).Trim()
                    : authValue.Trim();

                if (_token.Length == 0 || _token.Length > 2000)
                {
                    return XHDResult.Error(-9, "Token错误！");
                }

                // 用户在 Web 后台生成的长期 API / MCP Token
                if (ApiTokenUtil.IsApiToken(_token))
                {
                    try
                    {
                        var tokenHash = ApiTokenUtil.Hash(_token);
                        var apiToken = _fsql.Select<Sys_ApiToken>()
                            .Where(a => a.token_hash == tokenHash && a.is_revoked != 1)
                            .First();

                        if (apiToken == null || (apiToken.expires_at.HasValue && apiToken.expires_at.Value < DateTime.Now))
                        {
                            return XHDResult.Error(-9, "API Token无效或已过期，请重新生成！");
                        }

                        var apiUser = _fsql.Select<hr_employee>().Where(a => a.id == apiToken.emp_id).First();
                        if (apiUser == null)
                        {
                            return XHDResult.Error(-9, "找不到此用户！");
                        }

                        if (!string.Equals(apiUser.uid, "admin", StringComparison.OrdinalIgnoreCase)
                            && (apiUser.status != 1 || apiUser.canlogin == 0))
                        {
                            return XHDResult.Error(-9, "账号已被禁用！");
                        }

                        if (string.IsNullOrWhiteSpace(apiToken.token_suffix))
                        {
                            _fsql.Update<Sys_ApiToken>()
                                .Where(a => a.id == apiToken.id)
                                .Set(a => a.last_used_at, DateTime.Now)
                                .Set(a => a.token_suffix, _token.Length > 8 ? _token[^8..] : _token)
                                .ExecuteAffrows();
                        }
                        else
                        {
                            _fsql.Update<Sys_ApiToken>()
                                .Where(a => a.id == apiToken.id)
                                .Set(a => a.last_used_at, DateTime.Now)
                                .ExecuteAffrows();
                        }

                        employee = apiUser;
                        return XHDResult.Success();
                    }
                    catch
                    {
                        return XHDResult.Error(-9, "API Token校验失败，请重新生成！");
                    }
                }

                string decrypttoken;
                try
                {
                    decrypttoken = _tokenProtector.Unprotect(_token);
                }
                catch
                {
                    return XHDResult.Error(-9, "Token错误！");
                }

                string[] tokenitems = decrypttoken.Split('|');

                string id = "";
                string tokentime = "";

                if (tokenitems.Length == 2)
                {
                    id = tokenitems[0];
                    tokentime = tokenitems[1];
                }

                if (string.IsNullOrWhiteSpace(id) || !long.TryParse(tokentime, out var expireUnix))
                {
                    return XHDResult.Error(-9, "Token错误！");
                }

                if (expireUnix <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return XHDResult.Error(-9, "身份验证过期，请重新登录！");
                }


                Expression<Func<hr_employee, bool>> expwhere = a => a.id == id;

                var list = _empservice.Grid(expwhere);

                if (list.count == 0)
                {
                    return XHDResult.Error(-9, "找不到此用户！");
                }

                var currentUser = list.data[0];
                if (currentUser.uid != "admin" && (currentUser.status != 1 || currentUser.canlogin == 0))
                {
                    return XHDResult.Error(-9, "账号已被禁用！");
                }

                employee = currentUser;

                return XHDResult.Success();

            }

            // 获取用户代理
            var userAgent = Request.Headers.UserAgent;

            return XHDResult.Error(-9, "认证失败！");


        }

        public async Task<string> ModifyPWD(string pwd)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            Console.WriteLine($"pwd=>{pwd}");

            Expression<Func<hr_employee, hr_employee>> exppwd = a => new hr_employee { pwd = MD5Comm.MD5Hash(pwd) };
            Expression<Func<hr_employee, bool>> expwhere = a => a.id == employee.id;

            await _empservice.UpdateAsync(exppwd, expwhere);

            return XHDResult.Success("修改成功！").ToString();
        }

        #endregion

        #region 客户
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serchtxt"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<string> CustomerList(string? serchtxt, int page, int limit)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            if (!await _dBAuthService.GetAuth(employee.id, "CRM_Customer"))
            {
                return XHDResult.Error("无客户菜单权限！").ToString();
            }

            //查询数据
            Expression<Func<CRM_Customer, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.cus_name.Contains(serchtxt) || a.cus_tel.Contains(serchtxt));
            }


            //权限

            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.emp_id) || a.isPrivate == 1);
            }



            var result = await _customerservice.GridAsync(exp, page, limit, "a.create_time desc");

            return result.ToString();

        }

        public async Task<string> CustomerSave([FromBody] CRM_Customer model)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                var ids = model.id.Split("-");
                var sn = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}{ids[2]}";

                model.sn = sn;
                model.isDelete = 0;
                model.create_id = employee.id;
                model.create_time = DateTime.Now;
                if (string.IsNullOrWhiteSpace(model.emp_id)) model.emp_id = employee.id;

                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "CRM_Customer|add");

                if (authbtn)
                {
                    result = await _customerservice.AddAsync(model);
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "CRM_Customer|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<CRM_Customer, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _customerservice.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    if (!await CanAccessCustomerAsync(model.id))
                    {
                        return XHDResult.Error("无权操作该客户！").ToString();
                    }

                    result = await _customerservice.UpdateAsync(model);

                    //对比实体差别

                    SysLogExt<CRM_Customer> logext = new SysLogExt<CRM_Customer>();

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[客户]修改";
                        logmodels.EventID = model.id;
                        //logmodels.EventTitle = model.id;
                        logmodels.UserID = employee.id;
                        logmodels.UserName = employee.name;
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

        #endregion

        #region 联系人

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serchtxt"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<string> ContactList(string? serchtxt, string? customer_id, int page = 1, int limit = 10)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            //查询数据
            Expression<Func<CRM_Contact, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.C_name.Contains(serchtxt));
            }

            if (!string.IsNullOrWhiteSpace(customer_id))
            {
                exp = exp.And(a => a.customer_id == customer_id);
            }

            //权限

            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }

            var result = await _contactservice.GridAsync(exp, page, limit, "a.create_time desc");

            return result.ToString();

        }

        public async Task<string> ContactSave([FromBody] CRM_Contact model)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = employee.id;
                model.create_time = DateTime.Now;

                if (!await CanAccessCustomerAsync(model.customer_id))
                {
                    return XHDResult.Error("无权操作该客户！").ToString();
                }

                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "CRM_Contact|add");

                if (authbtn)
                {
                    result = await _contactservice.AddAsync(model);
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "CRM_Contact|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<CRM_Contact, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _contactservice.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    if (!await CanAccessCustomerAsync(checknulldata.data[0].customer_id))
                    {
                        return XHDResult.Error("无权操作该联系人！").ToString();
                    }

                    result = await _contactservice.UpdateAsync(model);

                    //对比实体差别

                    SysLogExt<CRM_Contact> logext = new SysLogExt<CRM_Contact>();

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[联系人]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.C_name;
                        logmodels.UserID = employee.id;
                        logmodels.UserName = employee.name;
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
        #endregion

        #region 跟进
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serchtxt"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<string> FollowList(string? serchtxt, string? customer_id, int page, int limit)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            if (!await _dBAuthService.GetAuth(employee.id, "CRM_Follow"))
            {
                return XHDResult.Error("无跟进菜单权限！").ToString();
            }

            //查询数据
            Expression<Func<CRM_follow, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(serchtxt));
            }

            if (!string.IsNullOrWhiteSpace(customer_id))
            {
                exp = exp.And(a => a.customer_id == customer_id);
            }


            //权限

            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }



            var result = await _followservice.GridAsync(exp, page, limit, "a.Follow_time desc");

            return result.ToString();

        }

        public async Task<string> FollowSave([FromBody] CRM_follow model)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.employee_id = employee.id;
                model.follow_time = DateTime.Now;

                if (!await CanAccessCustomerAsync(model.customer_id))
                {
                    return XHDResult.Error("无权操作该客户！").ToString();
                }

                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "CRM_Follow|add");

                if (authbtn)
                {
                    result = await _followservice.AddAsync(model);

                    await _customerservice.LastFollow(model.customer_id);

                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "CRM_Follow|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<CRM_follow, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _followservice.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    if (!await CanAccessCustomerAsync(checknulldata.data[0].customer_id))
                    {
                        return XHDResult.Error("无权操作该跟进记录！").ToString();
                    }

                    result = await _followservice.UpdateAsync(model);

                    //对比实体差别

                    SysLogExt<CRM_follow> logext = new SysLogExt<CRM_follow>();

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[跟进]修改";
                        logmodels.EventID = model.id;
                        //logmodels.EventTitle = model.id;
                        logmodels.UserID = employee.id;
                        logmodels.UserName = employee.name;
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

            await CustomerScoreService.RecalculateAsync(_fsql, model.customer_id);
            return XHDResult.Success().ToString();
        }

        #endregion

        #region 订单
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serchtxt"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<string> OrderList(string? serchtxt, string? customer_id, int page, int limit)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            if (!await _dBAuthService.GetAuth(employee.id, "Sale_Menu_order"))
            {
                return XHDResult.Error("无订单菜单权限！").ToString();
            }

            //查询数据
            Expression<Func<Sale_order, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(serchtxt));
            }

            if (!string.IsNullOrWhiteSpace(customer_id))
            {
                exp = exp.And(a => a.customer_id == customer_id);
            }


            //权限

            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }



            var result = await _OrderService.GridAsync(exp, page, limit, "a.create_time desc");

            return result.ToString();

        }

        public async Task<string> OrderDetails(string order_id)
        {
            var userresult = checkToken();
            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            if (!await _dBAuthService.GetAuth(employee.id, "Sale_Menu_order"))
            {
                return XHDResult.Error("无订单菜单权限！").ToString();
            }

            if (!await CanAccessOrderAsync(order_id))
            {
                return XHDResult.Error("无权访问该订单！").ToString();
            }

            Expression<Func<Sale_order_details, bool>> exp = a => a.order_id == order_id;

            //if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            //{
            //    exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["T_name"]));
            //}

            var result = await _OrderDetailsService.GridAsync(exp);

            return result.ToString();
        }

        public async Task<string> OrderSave([FromBody] Sale_order model)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var result = 0;

            var details = model.Order_details;

            model.Order_details = "";


            if (model.Order_amount < 0)
            {
                model.Order_amount = 0;
            }

            if (model.discount_amount < 0)
            {
                model.discount_amount = 0;
            }

            model.total_amount = model.Order_amount - model.discount_amount;

            if (model.total_amount < 0)
            {
                model.total_amount = 0;
            }


            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = employee.id;
                model.create_time = DateTime.Now;
                if (string.IsNullOrWhiteSpace(model.emp_id)) model.emp_id = employee.id;

                if (!await CanAccessCustomerAsync(model.customer_id))
                {
                    return XHDResult.Error("无权操作该客户！").ToString();
                }

                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "Sale_Order|add");

                if (authbtn)
                {
                    result = await _OrderService.AddAsync(model);
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "Sale_Order|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Sale_order, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _OrderService.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    if (!await CanAccessCustomerAsync(checknulldata.data[0].customer_id))
                    {
                        return XHDResult.Error("无权操作该订单！").ToString();
                    }

                    result = await _OrderService.UpdateAsync(model);

                    //对比实体差别

                    SysLogExt<Sale_order> logext = new SysLogExt<Sale_order>();

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[订单]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.sn;
                        logmodels.UserID = employee.id;
                        logmodels.UserName = employee.name;
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

            //先删除详情
            Expression<Func<Sale_order_details, bool>> expdetails = a => a.order_id == model.id;
            await _OrderDetailsService.DeleteAsync(expdetails);

            JArray arr = JArray.Parse(details);

            Sale_order_details modelsdetail = new Sale_order_details();
            modelsdetail.order_id = model.id;

            foreach (JObject item in arr)
            {
                modelsdetail.product_id = item.Value<string>("product_id");
                modelsdetail.price = item.Value<decimal>("price");
                modelsdetail.quantity = item.Value<int>("quantity");
                modelsdetail.amount = item.Value<decimal>("amount");

                await _OrderDetailsService.AddAsync(modelsdetail);
            }

            await CustomerScoreService.RecalculateAsync(_fsql, model.customer_id);
            return XHDResult.Success().ToString();
        }

        #endregion

        #region 合同

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serchtxt"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<string> ContractList(string? serchtxt, string? customer_id, int page, int limit)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            if (!await _dBAuthService.GetAuth(employee.id, "Sale_Contract"))
            {
                return XHDResult.Error("无合同菜单权限！").ToString();
            }

            //查询数据
            Expression<Func<Sale_contract, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.customer.cus_name.Contains(serchtxt));
            }

            if (!string.IsNullOrWhiteSpace(customer_id))
            {
                exp = exp.And(a => a.customer_id == customer_id);
            }

            //权限
            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.customer.emp_id));
            }

            var result = await _contractservice.GridAsync(exp, page, limit, "a.create_time desc");

            return result.ToString();

        }

        public async Task<string> ContractAtta(string contract_id)
        {
            var userresult = checkToken();
            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var contract = await _fsql.Select<Sale_contract>().Where(a => a.id == contract_id).FirstAsync();
            if (contract == null || !await CanAccessCustomerAsync(contract.customer_id))
            {
                return XHDResult.Error("无权访问该合同！").ToString();
            }

            Expression<Func<Sale_contract_atta, bool>> exp = a => a.contract_id == contract_id;

            //if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            //{
            //    exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["T_name"]));
            //}

            var result = await _contractattaservice.GridAsync(exp);

            return result.ToString();
        }

        public async Task<string> ContractSave([FromBody] Sale_contract model)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.Our_Contractor_id = employee.id;
                model.create_time = DateTime.Now;

                if (!await CanAccessCustomerAsync(model.customer_id))
                {
                    return XHDResult.Error("无权操作该客户！").ToString();
                }

                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "Sale_contract|add");

                if (authbtn)
                {
                    result = await _contractservice.AddAsync(model);

                    await _customerservice.LastFollow(model.customer_id);

                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "Sale_contract|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Sale_contract, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _contractservice.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    if (!await CanAccessCustomerAsync(checknulldata.data[0].customer_id))
                    {
                        return XHDResult.Error("无权操作该合同！").ToString();
                    }

                    result = await _contractservice.UpdateAsync(model);

                    //对比实体差别

                    SysLogExt<Sale_contract> logext = new SysLogExt<Sale_contract>();

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[合同]修改";
                        logmodels.EventID = model.id;
                        //logmodels.EventTitle = model.id;
                        logmodels.UserID = employee.id;
                        logmodels.UserName = employee.name;
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

            await CustomerScoreService.RecalculateAsync(_fsql, model.customer_id);
            return XHDResult.Success().ToString();
        }

        #endregion

        #region 收款

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serchtxt"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<string> ReceiveList(string? serchtxt, string? customer_id, int page, int limit)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            if (!await _dBAuthService.GetAuth(employee.id, "Finance_Receive"))
            {
                return XHDResult.Error("无财务菜单权限！").ToString();
            }

            //查询数据
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.Order.customer.cus_name.Contains(serchtxt));
            }

            if (!string.IsNullOrWhiteSpace(customer_id))
            {
                exp = exp.And(a => a.Order.customer.id == customer_id);
            }

            //权限
            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.Order.customer.emp_id));
            }



            var result = await _ReceiveService.GridAsync(exp, page, limit, "a.create_time desc");

            return result.ToString();

        }

        public async Task<string> ReceiveSave([FromBody] Finance_Receive model)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.create_id = employee.id;
                model.create_time = DateTime.Now;
                if (string.IsNullOrWhiteSpace(model.Payee_id)) model.Payee_id = employee.id;

                if (!await CanAccessOrderAsync(model.order_id))
                {
                    return XHDResult.Error("无权操作该订单！").ToString();
                }

                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "Finance_Receive|add");

                if (authbtn)
                {
                    result = await _ReceiveService.AddAsync(model);

                    var receiveScoreOrder = await _fsql.Select<Sale_order>().Where(a => a.id == model.order_id).FirstAsync();
                    if (receiveScoreOrder != null)
                    {
                        await CustomerScoreService.RecalculateAsync(_fsql, receiveScoreOrder.customer_id);
                    }
                }
                else
                {
                    return XHDResult.Error("无权限！").ToString();
                }
            }
            else
            {
                //权限
                var authbtn = await _dBAuthService.GetAuth(employee.id, "Finance_Receive|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Finance_Receive, bool>> exp = a => a.id == model.id;
                    var checknulldata = await _ReceiveService.GridAsync(exp, 1, 1);

                    if (checknulldata.count == 0)
                    {
                        return XHDResult.Error("找不到数据！").ToString();
                    }

                    if (!await CanAccessOrderAsync(checknulldata.data[0].order_id))
                    {
                        return XHDResult.Error("无权操作该收款记录！").ToString();
                    }

                    result = await _ReceiveService.UpdateAsync(model);

                    var receiveScoreOrder = await _fsql.Select<Sale_order>().Where(a => a.id == model.order_id).FirstAsync();
                    if (receiveScoreOrder != null)
                    {
                        await CustomerScoreService.RecalculateAsync(_fsql, receiveScoreOrder.customer_id);
                    }

                    //对比实体差别

                    SysLogExt<Finance_Receive> logext = new SysLogExt<Finance_Receive>();

                    var content = logext.LogContent(checknulldata.data[0], model);

                    if (content.Length > 0)
                    {
                        //添加修改日志
                        Sys_log logmodels = new Sys_log();

                        logmodels.id = UUIDNext.Uuid.NewSequential().ToString();
                        logmodels.EventType = "[收款]修改";
                        logmodels.EventID = model.id;
                        //logmodels.EventTitle = model.id;
                        logmodels.UserID = employee.id;
                        logmodels.UserName = employee.name;
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


        #endregion

        #region 参数、产品

        public async Task<string> CountData()
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            //权限
            //var roledata = await _dBAuthService.GetDataAuth(employee.id);

            //if (roledata.authtype != 4)
            //{
            //    exp = exp.And(a => roledata.empList.Contains(a.Order.customer.emp_id));
            //}
            //else
            //{ 

            //}

            _fsql.Select<CRM_Customer>().Where(a => a.emp_id == employee.id).Count(out var cuscount).Page(1, 1);
            _fsql.Select<CRM_follow>().Where(a => a.employee_id == employee.id).Count(out var followcount).Page(1, 1);
            _fsql.Select<Sale_order>().Where(a => a.emp_id == employee.id).Count(out var ordercount).Page(1, 1);
            _fsql.Select<Sale_contract>().Where(a => a.customer_id == employee.id).Count(out var contractcount).Page(1, 1);
            //_fsql.Select<Finance_Receive>().Where(a => 1 == 1).Count(out var receivecount).Page(1, 1);

            JObject obj=new JObject();

            obj.Add("cuscount", cuscount);
            obj.Add("followcount", followcount);
            obj.Add("ordercount", ordercount);
            obj.Add("contractcount", contractcount);

            return XHDResult.Success(obj).ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<bool> CanAccessCustomerAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return false;
            }

            var customer = await _fsql.Select<CRM_Customer>().Where(a => a.id == customerId).FirstAsync();
            if (customer == null)
            {
                return false;
            }

            var roledata = await _dBAuthService.GetDataAuth(employee.id);
            if (roledata.authtype == 4)
            {
                return true;
            }

            if (roledata.authtype == 0)
            {
                return false;
            }

            return (roledata.empList ?? new List<string>()).Contains(customer.emp_id);
        }

        private async Task<bool> CanAccessOrderAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return false;
            }

            var order = await _fsql.Select<Sale_order>().Where(a => a.id == orderId).FirstAsync();
            return order != null && await CanAccessCustomerAsync(order.customer_id);
        }

        public async Task<string> paramsCombo(string type)
        {
            var userresult = checkToken();
            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            Expression<Func<Sys_Param, bool>> exp = a => a.params_type == type;

            var result = await _SysParamService.GridAsync(exp, "params_order");

            //return JsonConvert.SerializeObject(result, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }).ToString();
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<string> ProductList(string? serchtxt, int page = 1, int limit = 10)
        {
            var userresult = checkToken();
            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            //查询数据
            Expression<Func<Product, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.product_name.Contains(serchtxt));
            }

            var result = await _productservice.GridAsync(exp, page, limit, "a.create_time desc");

            return result.ToString();
        }

        public async Task<string> EmployeeList(string? serchtxt, int page = 1, int limit = 10)
        {
            //身份验证
            var userresult = checkToken();

            if (userresult.Value<int>("code") != 0)
            {
                return userresult.ToString();
            }

            // 查询数据
            Expression<Func<hr_employee, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(serchtxt))
            {
                exp = exp.And(a => a.name.Contains(serchtxt));
            }

            //权限
            var roledata = await _dBAuthService.GetDataAuth(employee.id);

            if (roledata.authtype != 4)
            {
                exp = exp.And(a => roledata.empList.Contains(a.id));
            }

            var result = await _empservice.GridAsync(exp, "a.create_time desc");

            return result.ToString();
        }

        #endregion



    }
}

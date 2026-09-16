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
    public class ProductCategoryController : Controller
    {
        private readonly ILogger<ProductCategoryController> _logger;
        private readonly IProduct_categoryService _service;
        private readonly IProductService _ProductService;
        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Product_category> logext = new SysLogExt<Product_category>();

        public ProductCategoryController(ILogger<ProductCategoryController> logger,
            IProductService ProductService,
            IProduct_categoryService Service, ISys_logService LogService, IDBAuthService dBAuthService)
        {
            _logger = logger;
            _service = Service;
            _ProductService = ProductService;

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
        public async Task<string> Grid()
        {
            Expression<Func<Product_category, bool>> exp = a => 1 == 1;

            var result = await _service.GridAsync(exp);

            return result.ToString();
        }

        public async Task<string> Tree(string id)
        {
            var result = await _service.Tree(id);

            JObject firstObj = new JObject();
            firstObj.Add("id", "root");
            firstObj.Add("title", "无");
            firstObj.Add("parentid", "");
            result.AddFirst(firstObj);

            return result.ToString();
        }

        public async Task<string> TreeAll()
        {
            var result = await _service.Tree();

            return result.ToString();
        }

        public async Task<string> Save(Product_category model)
        {
            var result = 0;

            if (string.IsNullOrWhiteSpace(model.id))
            {
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                //权限
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Product_Category|add");

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
                var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Product_Category|edit");

                if (authbtn)
                {
                    //日志
                    Expression<Func<Product_category, bool>> exp = a => a.id == model.id;
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
                        logmodels.EventType = "[产品类别]修改";
                        logmodels.EventID = model.id;
                        logmodels.EventTitle = model.category_name;
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


            //有下级，不能删除
            Expression<Func<Product_category, bool>> exp = a => a.parentid == id;

            var resultcategory = await _service.GridAsync(exp);

            if (resultcategory.count > 0)
            {
                return XHDResult.Error("此类别下含有下级，不能删除！").ToString();
            }

            //有产品，不能删除
            Expression<Func<Product, bool>> exppro = a => a.category_id == id;

            var resultproduct = await _ProductService.GridAsync(exppro, 1, 1);

            if (resultproduct.count > 0)
            {
                return XHDResult.Error("类别下含有产品，不能删除！").ToString();
            }

            var result = 0;

            //权限
            var authbtn = await _dBAuthService.GetAuth(User.FindFirst(ClaimTypes.Sid).Value, "Product_Category|del");

            if (authbtn)
            {
                //判断是否有数据
                exp = a => a.id == id;
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
                logmodels.EventType = "[产品类别]删除";
                logmodels.EventID = id;
                logmodels.EventTitle = checkdata.data[0].category_name;
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

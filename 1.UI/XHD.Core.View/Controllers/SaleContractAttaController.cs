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
using System.Linq.Expressions;
using System.Threading.Tasks;

using System.Security.Claims;
using System.IO;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Common;
using XHD.Core.Models;

using Newtonsoft.Json.Converters;
using System.Collections;
using Microsoft.AspNetCore.Hosting.Server;


namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SaleContractAttaController : Controller
    {
        private readonly ILogger<SaleContractAttaController> _logger;
        private readonly ISale_contractService _service;
        private readonly ISale_contract_attaService _detailservice;

        public SaleContractAttaController(ILogger<SaleContractAttaController> logger, ISale_contractService service, ISale_contract_attaService detailservice)
        {
            _service = service;
            _logger = logger;
            _detailservice = detailservice;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        public async Task<string> Grid(PageView<Sale_contract_atta> model)
        {
            Expression<Func<Sale_contract_atta, bool>> exp = a => a.contract_id == Request.Query["id"];

            //if (!string.IsNullOrWhiteSpace(Request.Query["T_name"]))
            //{
            //    exp = exp.And(a => a.customer.cus_name.Contains(Request.Query["T_name"]));
            //}

            var result = await _detailservice.GridAsync(exp, model.Page, model.Limit, "a.create_time desc");

            return result.ToString();
        }

        /// <summary>
        /// 文件上传
        /// 此代码会区分分片上传代码。
        /// </summary>
        /// <returns></returns>
        public async Task<string> Upload()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var emp_id = claimIdentity.FindFirst(ClaimTypes.Sid).Value;
            var emp_name = claimIdentity.FindFirst(ClaimTypes.Name).Value;
            var uid = claimIdentity.FindFirst("uid").Value;

            var files = Request.Form.Files;



            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {

                    if (Request.Form.Any(a => a.Key == "chunk"))
                    {
                        //分片上传
                        //var basePath = Path.GetDirectoryName($"{Directory.GetCurrentDirectory()}/upload/contract/{ Request.Form["guid"] }-{Request.Form["id"]}/");
                        var basePath = Path.GetDirectoryName($"{Directory.GetCurrentDirectory()}/wwwroot/upload/contract/{Request.Form["guid"]}-{Request.Form["id"]}/");

                        if (!Directory.Exists(basePath))
                        {
                            Directory.CreateDirectory(basePath);
                        }
                        var filePath = basePath + "/" + Request.Form["chunk"];
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        //普通上传
                        var basePath = Path.GetDirectoryName($"{Directory.GetCurrentDirectory()}/wwwroot/upload/contract/{ Request.Form["contract_id"] }/");
                        
                        if (!Directory.Exists(basePath))
                        {
                            Directory.CreateDirectory(basePath);
                        }
                        string fileName = Request.Form["name"];//文件名
                        string fileExt = Path.GetExtension(fileName);//获取文件后缀

                        byte[] buffer = Guid.NewGuid().ToByteArray();
                        var out_trad_id = BitConverter.ToInt64(buffer, 0).ToString();

                        var finalFilePath = Path.Combine(basePath, $"{ out_trad_id }{fileExt}");//最终的文件名

                        using (var stream = new FileStream(finalFilePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);

                            Sale_contract_atta model = new Sale_contract_atta();
                            model.id = UUIDNext.Uuid.NewSequential().ToString();
                            model.contract_id = Request.Form["contract_id"];
                            model.file_name = fileName;
                            model.real_name = $"{ out_trad_id }{fileExt}";
                            model.file_size = (int)(stream.Length / 1024);
                            model.create_time = DateTime.Now;
                            model.create_id = emp_id;

                            await _detailservice.AddAsync(model);
                        }

                        
                    }
                }
            }

            return XHDResult.Success().ToString();
        }

        /// <summary>
        /// 文件合并
        /// </summary>
        /// <returns></returns>
        public async Task<string> Meger()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var emp_id = claimIdentity.FindFirst(ClaimTypes.Sid).Value;
            var emp_name = claimIdentity.FindFirst(ClaimTypes.Name).Value;
            var uid = claimIdentity.FindFirst("uid").Value;

            //var basePath = Path.GetDirectoryName($"{Directory.GetCurrentDirectory()}/upload/contract/{ Request.Form["guid"] }/{Request.Form["id"]}/");
            var basePath = Path.GetDirectoryName($"{Directory.GetCurrentDirectory()}/wwwroot/upload/contract/");
            var savePath = Path.GetDirectoryName($"{basePath}/{Request.Form["contract_id"]}/");



            try
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                var temporary = Path.GetDirectoryName($"{basePath}/{ Request.Form["guid"] }-{Request.Form["id"]}/");//临时文件夹

                if (!Directory.Exists(savePath))
                {
                    //临时文件夹不存在，说明文件应该不是分片上传，无需合并
                    return XHDResult.Success().ToString();
                }

                string fileName = Request.Form["name"];//文件名
                string fileExt = Path.GetExtension(fileName);//获取文件后缀
                var files = Directory.GetFiles(temporary);//获得下面的所有文件

                byte[] buffer = Guid.NewGuid().ToByteArray();
                var out_trad_id = BitConverter.ToInt64(buffer, 0).ToString();

                var finalFilePath = Path.Combine(savePath, $"{ out_trad_id }{fileExt}");//最终的文件名
                //var fs = new FileStream(finalFilePath, FileMode.Create);
                using (var fs = new FileStream(finalFilePath, FileMode.Create))
                {
                    foreach (var part in files.OrderBy(x => x.Length).ThenBy(x => x))
                    {
                        var bytes = System.IO.File.ReadAllBytes(part);
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                        bytes = null;
                        System.IO.File.Delete(part);//删除分块
                    }
                    //Directory.Delete(temporaryID);//删除文件夹
                    Directory.Delete(temporary, true);//删除文件夹

                    Sale_contract_atta model = new Sale_contract_atta();
                    model.id = UUIDNext.Uuid.NewSequential().ToString();
                    model.contract_id = Request.Form["contract_id"];
                    model.file_name = fileName;
                    model.real_name = $"{ out_trad_id }{fileExt}";
                    model.file_size = (int)(fs.Length / 1024);
                    model.create_time = DateTime.Now;
                    model.create_id = emp_id;

                    await _detailservice.AddAsync(model);

                }


            }
            catch (Exception ex)
            {
                NLogger.WriteLog("file_err_", ex.ToString());
            }

            return XHDResult.Success().ToString();
        }

        public async Task<string> Del(string id)
        {
            Expression<Func<Sale_contract_atta, bool>> exp = a => a.id == id;

            var data = await _detailservice.GridAsync(exp);

            await _detailservice.DeleteAsync(exp);

            var basePath = Path.GetDirectoryName($"{Directory.GetCurrentDirectory()}/wwwroot/upload/contract/");
            var savePath = Path.GetDirectoryName($"{basePath}/{Request.Form["contract_id"]}/");
            var filePath = Path.Combine(savePath, data.data[0].real_name);

            try
            {
                System.IO.File.Delete(filePath);
            }
            catch
            {

            }


            return XHDResult.Success().ToString();
        }
    }
}

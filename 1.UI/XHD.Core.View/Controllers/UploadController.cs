
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XHD.Core.IServices;
using XHD.Core.Models;

namespace XHD.Core.View.Controllers
{
    public class UploadController : Controller
    {
        private readonly ICRM_Customer_attaService _customerattaservice;

        public UploadController(ICRM_Customer_attaService customerattaservice)
        {
            _customerattaservice = customerattaservice;
        }

        public async Task<string> Image()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var out_trad_id = BitConverter.ToInt64(buffer, 0).ToString();


            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return "No File";
            }
            var file = files[0];
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif" || fileExtension == ".jpeg")
            {

            }
            else
            {
                return "File type error";
            }

            var UploadDir = $"Upload/Image/{DateTime.Now.ToString("yyyy-MM-dd")}";
            var fileDir = $"{Directory.GetCurrentDirectory()}/wwwroot/Upload/Image/{DateTime.Now.ToString("yyyy-MM-dd")}";
            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            var filename = $"{out_trad_id}{fileExtension}";
            var fullpath = Path.Combine(fileDir, filename);
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {
                //file.CopyTo(fs);
                //fs.Close();
                await file.CopyToAsync(fs);
            }
            var url = $"../{UploadDir}/{filename}";

            JObject obj = new JObject();
            obj.Add("location", url);

            return obj.ToString();
        }

        [DisableRequestSizeLimit]
        public async Task<string> FileUp()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var out_trad_id = BitConverter.ToInt64(buffer, 0).ToString();


            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return "No File";
            }
            var file = files[0];
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension == ".apk")
            {

            }
            else
            {
                return "File type error";
            }

            var UploadDir = $"Upload/File/{DateTime.Now.ToString("yyyy-MM-dd")}";
            var fileDir = $"{Directory.GetCurrentDirectory()}/wwwroot/Upload/File/{DateTime.Now.ToString("yyyy-MM-dd")}";
            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            var filename = $"{out_trad_id}{fileExtension}";
            var fullpath = Path.Combine(fileDir, filename);
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {
                //file.CopyTo(fs);
                //fs.Close();
                await file.CopyToAsync(fs);
            }
            var url = $"../{UploadDir}/{filename}";

            JObject obj = new JObject();
            obj.Add("location", url);

            return obj.ToString();
        }


        /// <summary>
        /// 下载本地物理文件（指定文件名）
        /// </summary>
        /// <param name="fileName">客户端接收的文件名（如 "我的报告.pdf"）</param>
        /// <returns>文件流</returns>

        public IActionResult DownloadCustomerAtta(string id)
        {
            Expression<Func<CRM_Customer_atta, bool>> exp = a => a.id == id;

            var attadata = _customerattaservice.Grid(exp);

            if (attadata.count == 0)
            {
                return NotFound("文件不存在");
            }

            var data = attadata.data[0];

            //var url = '/upload/customer/' + data.cus_id + "/" + data.real_name;

            // 1. 服务器本地文件路径（确保路径正确，建议使用绝对路径）
            //string serverFilePath = Path.Combine(
            //    Directory.GetCurrentDirectory(),  // 项目根目录
            //    "/upload/customer/", data.cus_id, data.real_name  // 服务器存储的原始文件
            //);

            string serverFilePath = $"{Directory.GetCurrentDirectory()}/wwwroot/upload/customer/{data.cus_id}/{data.real_name}";

            Console.WriteLine(serverFilePath);

            // 2. 验证文件是否存在
            if (!System.IO.File.Exists(serverFilePath))
            {
                return NotFound("文件不存在");
            }

            // 2. 动态获取 Content-Type
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(serverFilePath, out string? contentType))
            {
                contentType = "application/octet-stream"; // 未知类型默认二进制流
            }

            // 3. 读取文件流（使用 FileStream 确保资源释放）
            var fileStream = new FileStream(serverFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            // 4. 设置响应头：指定下载文件名（支持中文，需编码）
            var contentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                // 编码文件名，避免中文乱码
                FileNameStar = Uri.EscapeDataString(data.file_name),
                FileName = Uri.EscapeDataString(data.file_name)
            };
            Response.Headers.TryAdd(HeaderNames.ContentDisposition, contentDisposition.ToString());

            return File(fileStream, contentType, enableRangeProcessing: true); // enableRangeProcessing 支持断点续传
        }
    }
}
